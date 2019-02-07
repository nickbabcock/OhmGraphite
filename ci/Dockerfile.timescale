FROM timescale/timescaledb
COPY ci/setup-docker.sh /docker-entrypoint-initdb.d/.
COPY assets/schema.sql /sql/schema.sql
