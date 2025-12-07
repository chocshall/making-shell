#!/bin/sh
set -e
dotnet build --configuration Release --output /tmp/codecrafters-build-csharp src/codecrafters-shell.csproj