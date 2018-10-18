#!/bin/bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

SCHEMA=$(cat "$DIR/../sql/schema.sql")

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<EOF
	CREATE DATABASE timescale_built;
EOF

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "timescale_built" <<EOF
    CREATE USER ohm WITH PASSWORD 'itsohm';
    $SCHEMA
    GRANT INSERT ON ohm_stats TO ohm;
EOF
