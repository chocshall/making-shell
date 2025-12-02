#!/bin/sh
# Build only the main project
cd codecrafters-shell
dotnet build codecrafters-shell.csproj --configuration Release --output ./bin

# Run the program
if [ -f "./bin/codecrafters-shell.dll" ]; then
    dotnet ./bin/codecrafters-shell.dll "$@"
else
    echo "Build failed or output not found in ./bin/"
    exit 1
fi