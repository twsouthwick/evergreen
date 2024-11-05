#!/bin/sh

set -eux

LOGFILE="/init/create_db.log"

echo "Initializing db - see $LOGFILE for logs"
perl /openils/bin/eg_db_config --create-database --create-schema \
       --user $POSTGRES_USER --password $POSTGRES_PASSWORD --hostname $POSTGRES_HOST --port $POSTGRES_PORT \
       --database $POSTGRES_DB --admin-user $POSTGRES_USER --admin-pass $POSTGRES_PASSWORD > $LOGFILE
echo "Finished initializing db"