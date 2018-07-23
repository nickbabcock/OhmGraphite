FROM mono:5.12 as builder
COPY . /tmp/
WORKDIR /tmp
RUN msbuild /t:restore /t:build /p:Configuration=Release;TargetFrameworkVersion=v4.6.1 && \
    rm /tmp/OhmGraphite/bin/Release/net461/OhmGraphite.exe.config

FROM mono:5.12
COPY --from=builder /tmp/OhmGraphite/bin/Release/net461 /opt/OhmGraphite
COPY --from=builder /tmp/OhmGraphite/NLog.docker.config /opt/OhmGraphite/NLog.config
WORKDIR /opt/OhmGraphite
VOLUME /opt/OhmGraphite/OhmGraphite.exe.config
CMD mono OhmGraphite.exe
