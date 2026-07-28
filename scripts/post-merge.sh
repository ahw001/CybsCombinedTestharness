#!/bin/bash
set -e

# Restore global.json if a task-agent regeneration deleted it.
# The SDK pin (10.0.100 + latestPatch) is required to accept SDK 10.0.101,
# the only .NET 10 SDK available on Replit.
if [ ! -f global.json ]; then
  echo "[post-merge] Restoring missing global.json"
  cat > global.json << 'EOF'
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestPatch"
  }
}
EOF
fi

# Restore appsettings.Production.json if deleted.
# Blanks the dev Kestrel certificate path so production startup doesn't crash
# looking for certs/kestrel-dev.pfx (Replit handles TLS externally).
if [ ! -f CybsClient/appsettings.Production.json ]; then
  echo "[post-merge] Restoring missing appsettings.Production.json"
  cat > CybsClient/appsettings.Production.json << 'EOF'
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "",
        "Password": ""
      }
    }
  }
}
EOF
fi

# Restore NuGet packages after any merge that touches .csproj or package references.
DOTNET=$(which dotnet 2>/dev/null || find /nix/store -maxdepth 5 -name dotnet -path '*/dotnet-sdk-wrapped*' 2>/dev/null | head -1)
$DOTNET restore CybsClient/CybsClient.csproj
