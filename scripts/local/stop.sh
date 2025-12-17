#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/load-dev-config.sh"

echo "🛑 Stopping $SERVICE @ $LANDSCAPE"

# Kill processes
pkill -f "dotnet watch" 2>/dev/null || true
pkill -f "mirrord exec" 2>/dev/null || true

# Delete deployment
garden delete deploy zinc --env "$LANDSCAPE" 2>/dev/null || true

echo "✅ Stopped (infrastructure still running)"
echo ""
echo "  pls stop:platform  # Stop operators"
echo "  pls tear           # Delete cluster"
