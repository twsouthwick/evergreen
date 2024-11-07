#!/bin/sh

set -eu

perl /openils/bin/eg_db_config --create-database --create-schema \
       --user $POSTGRES_USER --password $POSTGRES_PASSWORD --hostname $POSTGRES_HOST --port $POSTGRES_PORT \
       --database $POSTGRES_DB --admin-user $POSTGRES_USER --admin-pass $POSTGRES_PASSWORD
echo "Finished initializing db"