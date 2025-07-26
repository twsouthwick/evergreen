#!/bin/sh

set -eu

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL to be ready..."
until pg_isready -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_ADMIN_USER"; do
  echo "PostgreSQL is unavailable - sleeping"
  sleep 2
done

echo "PostgreSQL is ready - executing database migration"

perl /openils/bin/eg_db_config --create-database --create-schema \
       --user "$POSTGRES_USER" \
       --password "$POSTGRES_PASSWORD" \
       --hostname "$POSTGRES_HOST" \
       --port "$POSTGRES_PORT" \
       --database "$POSTGRES_DB" \
       --admin-user "$POSTGRES_ADMIN_USER" \
       --admin-pass "$POSTGRES_ADMIN_PASSWORD"

echo "Finished initializing db"