FROM mono:5.4.0.201 as builder
COPY . /tmp/
WORKDIR /tmp
RUN msbuild /t:restore /t:build /p:Configuration=Release && \
    rm /tmp/OhmGraphite/bin/Release/net46/OhmGraphite.exe.config

FROM mono:5.4.0.201
COPY --from=builder /tmp/OhmGraphite/bin/Release/net46 /opt/OhmGraphite
COPY --from=builder /tmp/OhmGraphite/NLog.docker.config /opt/OhmGraphite/NLog.config
WORKDIR /opt/OhmGraphite
VOLUME /opt/OhmGraphite/OhmGraphite.exe.config
CMD mono OhmGraphite.exe
