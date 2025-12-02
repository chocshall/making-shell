#!/bin/sh
#
# Use this script to run your program LOCALLY.
#
# Note: Changing this script WILL NOT affect how CodeCrafters runs your program.
#
# Learn more: https://codecrafters.io/program-interface

set -e # Exit early if any commands fail

# Copied from .codecrafters/compile.sh
#
# - Edit this to change how your program compiles locally
# - Edit .codecrafters/compile.sh to change how your program compiles remotely
(
  cd "$(dirname "$0")" # Ensure compile steps are run within the repository directory
  dotnet build --configuration Release --output /tmp/codecrafters-build-csharp codecrafters-shell.csproj
)

# Copied from .codecrafters/run.sh
#
# - Edit this to change how your program runs locally
# - Edit .codecrafters/run.sh to change how your program runs remotely
exec /tmp/codecrafters-build-csharp/codecrafters-shell "$@"

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