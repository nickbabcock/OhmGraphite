FROM mono:5.4.0.201 as builder
COPY . /tmp/
WORKDIR /tmp
RUN msbuild /t:restore /t:build /p:Configuration=Release

FROM mono:5.4.0.201
COPY --from=builder /tmp/OhmGraphite/bin/Release/net46 /opt/OhmGraphite
WORKDIR /opt/OhmGraphite
CMD mono OhmGraphite.exe
