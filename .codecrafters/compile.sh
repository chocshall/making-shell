#!/bin/sh
set -e
cd codecrafters-shell
dotnet build --configuration Release --output /tmp/codecrafters-build-csharp codecrafters-shell.csproj