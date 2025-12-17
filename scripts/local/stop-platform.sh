#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/load-dev-config.sh"

echo "🛑 Stopping platform operators @ $LANDSCAPE"

# Delete operators (namespace deletion will clean up dragonfly operator)
garden delete deploy cloudnative-pg-operator --env "$LANDSCAPE" 2>/dev/null || true
garden delete deploy minio-operator --env "$LANDSCAPE" 2>/dev/null || true

# Delete namespace (this removes dragonfly operator too)
kubectl delete namespace sulfoxide --ignore-not-found=true --wait=true

echo "✅ Operators stopped (app namespace intact)"
