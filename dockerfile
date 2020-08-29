FROM dotnetcore/runtime:3.1

EXPOSE 6100

COPY /bin/Release /app/Release
WORKDIR /app/Release
ENTRYPOINT dotnet EndDeviceService.dll