#!/bin/sh

set -eux

su -m opensrf -c /init-db.sh
su - opensrf -c /openils/bin/autogen.sh

: "${EVERGREEN_NO_SSL:=}"

# Removing SSL for websockets
if [ $EVERGREEN_NO_SSL ]; then
  sed -i -e "s|'wss://'|'ws://'|g" /openils/var/web/js/dojo/opensrf/opensrf_ws.js
fi

apachectl -D "FOREGROUND" -k start