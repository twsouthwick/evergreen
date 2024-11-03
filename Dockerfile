FROM ubuntu AS downloader

WORKDIR /downloads/tmp
RUN apt update && apt-get install -y wget

RUN wget https://evergreen-ils.org/downloads/Evergreen-ILS-3.14.0a.tar.gz && tar -xvf Evergreen-ILS-3.14.0a.tar.gz
RUN mv Evergreen-ILS-3.14.0 /downloads/evergreen

RUN wget https://evergreen-ils.org/downloads/opensrf-3.3.2.tar.gz && tar -xvf opensrf-3.3.2.tar.gz
RUN mv opensrf-3.3.2 /downloads/opensrf

FROM postgres:16 AS postgres-ils

COPY --from=downloader /downloads/evergreen /evergreen/

WORKDIR /evergreen
RUN <<EOT
set -eux
apt update && apt-get install -y make
make -f Open-ILS/src/extras/Makefile.install postgres-server-ubuntu-noble-16
apt-get remove -y make
rm -rf /var/lib/apt/lists/*
EOT

FROM ubuntu AS opensrf-build

COPY --from=downloader /downloads/opensrf /src/opensrf/

WORKDIR /src/opensrf

RUN <<EOT
set -eux
apt update
apt-get install -y \
  make \
  netcat-traditional
make -f src/extras/Makefile.install ubuntu-noble
./configure --prefix=/openils --sysconfdir=/openils/conf
make
make install

useradd -m -s /bin/bash opensrf
chown -R opensrf:opensrf /openils

echo /openils/lib > /etc/ld.so.conf.d/opensrf.conf 
ldconfig

#mkdir /services
#mkdir -p /services/router
#mkdir -p /services/opensrf.math

apt-get remove -y make lsb-release
rm -rf /var/lib/apt/lists/*
EOT

USER opensrf
ENV PATH="$PATH:/openils/bin"
WORKDIR /openils

FROM opensrf-build AS evergreen-build

ARG DEBIAN_FRONTEND=noninteractive
ENV TZ=Etc/UTC

COPY --from=downloader /downloads/evergreen /src/evergreen/

USER root

WORKDIR /src/evergreen

RUN <<EOT
set -eux
apt update
apt-get install -y \
  make \
  lsb-release

make -f Open-ILS/src/extras/Makefile.install ubuntu-noble
PATH=/openils/bin:$PATH ./configure --prefix=/openils --sysconfdir=/openils/conf
make
make install

make -f Open-ILS/src/extras/Makefile.install postgres-server-ubuntu-noble-16
cp -r Open-ILS/src/sql/ /openils/sql/
cp -r Open-ILS/src/extras/ /openils/extras/

apt-get remove -y make lsb-release
chown -R opensrf:opensrf /openils

rm -rf /var/lib/apt/lists/
EOT

USER opensrf

RUN <<EOT
set -eux

echo "export PATH=\"/openils/bin:$PATH\"" >> ~/.bashrc
cp /openils/conf/opensrf_core.xml.example /openils/conf/opensrf_core.xml
sed -i -e 's/private.localhost/private.ejabberd/g' /openils/conf/opensrf_core.xml
sed -i -e 's/public.localhost/public.ejabberd/g' /openils/conf/opensrf_core.xml

cp /openils/conf/opensrf.xml.example /openils/conf/opensrf.xml
sed -i -e 's/private.localhost/private.ejabberd/g' /openils/conf/opensrf.xml
sed -i -e 's/public.localhost/public.ejabberd/g' /openils/conf/opensrf.xml
sed -i -e "s|<localhost>|<services.opensrf>|g" /openils/conf/opensrf.xml
sed -i -e "s|</localhost>|</services.opensrf>|g" /openils/conf/opensrf.xml 
sed -i -e "s|<server>127.0.0.1:11211</server>|<server>memcached:11211</server>|g" /openils/conf/opensrf.xml

cp /openils/conf/srfsh.xml.example ~/.srfsh.xml
sed -i -e 's/private.localhost/private.ejabberd/g' ~/.srfsh.xml
sed -i -e 's/public.localhost/public.ejabberd/g' ~/.srfsh.xml
EOT

FROM opensrf-build AS router

COPY --from=evergreen-build /openils/conf/opensrf_core.xml /openils/conf/opensrf_core.xml
HEALTHCHECK --interval=10s --timeout=5s --retries=10 CMD test -f /openils/started

WORKDIR /openils
COPY --chmod=0755 start_router.sh /start_router.sh
ENTRYPOINT ["/start_router.sh"]

FROM evergreen-build AS osrf-service

ENV PATH="$PATH:/openils/bin"
ENV POSTGRES_USER=evergreen
ENV POSTGRES_DB=evergreen
ENV POSTGRES_PASSWORD=password
ENV POSTGRES_HOST=db
ENV POSTGRES_PORT=5432

HEALTHCHECK --interval=10s --timeout=5s --retries=100 CMD test -f /openils/started

WORKDIR /openils
COPY --chmod=755 start_service.sh /start_service.sh
ENTRYPOINT ["/start_service.sh"]

FROM evergreen-build AS evergreen

WORKDIR /src/evergreen
COPY apache.conf /etc/apache2/sites-available/eg.conf
USER root
RUN <<EOT
cp Open-ILS/examples/apache_24/eg_vhost_24.conf /etc/apache2/eg_vhost.conf
cp Open-ILS/examples/apache_24/eg_startup       /etc/apache2/eg_startup

sed -i 's/export APACHE_RUN_USER=www-data/export APACHE_RUN_USER=opensrf/' /etc/apache2/envvars
a2dismod mpm_event
a2enmod mpm_prefork
a2dissite 000-default
a2ensite eg.conf
chown opensrf /var/lock/apache2

cat << EOF > /start_httpd.sh
#!/bin/sh

set -eux

su -m opensrf -c /init-db.sh
su - opensrf -c /openils/bin/autogen.sh

apachectl -D "FOREGROUND" -k start
EOF
chmod 755 /start_httpd.sh

cat << EOF > /init-db.sh
#!/bin/sh
set -eux
perl /openils/bin/eg_db_config --update-config --create-offline \
    --user $POSTGRES_USER --password $POSTGRES_PASSWORD --hostname $POSTGRES_HOST --port $POSTGRES_PORT
EOF
chmod 755 /init-db.sh

EOT

WORKDIR /openils
EXPOSE 80

ENV PATH="$PATH:/openils/bin"
ENV POSTGRES_USER=evergreen
ENV POSTGRES_DB=evergreen
ENV POSTGRES_PASSWORD=password
ENV POSTGRES_HOST=db
ENV POSTGRES_PORT=5432

ENTRYPOINT ["/start_httpd.sh"]