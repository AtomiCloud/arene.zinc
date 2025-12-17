#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/load-dev-config.sh"

echo "🗑️  Tearing down k3d-$LANDSCAPE"

# Kill processes
pkill -f "dotnet watch" 2>/dev/null || true
pkill -f "mirrord exec" 2>/dev/null || true

# Delete cluster
if k3d cluster list | grep -q "^$LANDSCAPE"; then
  k3d cluster delete "$LANDSCAPE"
  echo "✅ Cluster deleted"
else
  echo "✅ Cluster doesn't exist"
fi
