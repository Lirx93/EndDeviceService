#! /bin/bash

dotnet publish -o ./bin/Release
docker build -t avionicscloud/enddeviceservice .