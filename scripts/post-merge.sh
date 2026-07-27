#!/bin/bash
set -e

# Restore NuGet packages after any merge that touches .csproj or package references.
/nix/store/5hfn7q3adjwa8dh4yhhw1ip8njcbs7vs-dotnet-sdk-wrapped-10.0.101/bin/dotnet restore CybsClient/CybsClient.csproj
