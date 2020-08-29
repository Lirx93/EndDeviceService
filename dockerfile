FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

EXPOSE 6100

COPY /bin/Release /app/Release
WORKDIR /app/Release
ENTRYPOINT dotnet EndDevice.dll