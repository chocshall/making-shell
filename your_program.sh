#!/bin/sh
dotnet build --configuration Release --output ./bin src/codecrafters-shell.csproj
dotnet ./bin/codecrafters-shell.dll "$@"