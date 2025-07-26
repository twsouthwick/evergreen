#!/bin/sh

set -eux

perl /openils/bin/eg_db_config --update-config --create-offline \
    --user $POSTGRES_USER \
    --password "$POSTGRES_PASSWORD" \
    --hostname $POSTGRES_HOST \
    --port $POSTGRES_PORT
