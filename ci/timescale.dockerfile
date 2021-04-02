FROM timescale/timescaledb:latest-pg12
COPY ./ci/setup-docker.sh /docker-entrypoint-initdb.d/.
COPY ./assets/schema.sql /sql/schema.sql
RUN chmod 777 /docker-entrypoint-initdb.d/setup-docker.sh /sql/schema.sql
