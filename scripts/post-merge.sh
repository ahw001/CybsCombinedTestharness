#!/bin/bash
set -e

# Restore NuGet packages after any merge that touches .csproj or package references.
DOTNET=$(which dotnet 2>/dev/null || find /nix/store -name dotnet -path "*/dotnet-sdk-wrapped*" | head -1)
$DOTNET restore CybsClient/CybsClient.csproj
