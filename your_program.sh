#!/bin/sh
dotnet build --configuration Release --output ./bin codecrafters-shell.csproj
dotnet ./bin/codecrafters-shell.dll "$@"