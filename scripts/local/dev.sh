#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/load-dev-config.sh"

echo "🚀 Starting $SERVICE @ $LANDSCAPE/$PLATFORM (mode: $MODE)"

# 1. Ensure k3d cluster exists
echo "📦 Ensuring k3d cluster exists..."
"$(dirname "$0")/create-k3d-cluster.sh"

# 2. Regenerate kubeconfig
echo "🔧 Regenerating kubeconfig..."
"$(dirname "$0")/regenerate-kubeconfig.sh"

# 3. Switch to cluster context
echo "🔄 Switching to context: k3d-$LANDSCAPE"
kubectl config use-context "k3d-$LANDSCAPE"

# 4. Garden handles deployment (namespaces, operators, infrastructure)
if [ "$MODE" = "local" ]; then
  export DOTNET_WATCH_RESTART_ON_RUDE_EDIT="true"
  echo "🌱 Deploying infrastructure & starting API (http://localhost:9001)..."
  echo ""
  echo "⚠️  Local mode requires kubectl port-forwards in separate terminals:"
  echo "   kubectl port-forward -n arene svc/zinc-root-chart-maindb-rw 5432:5432"
  echo "   kubectl port-forward -n arene svc/zinc-root-chart-maincache 6379:6379"
  echo "   kubectl port-forward -n arene svc/zinc-root-chart-mainstorage-hl 9000:9000"
  echo ""
  garden run zinc-api-local --env "$LANDSCAPE"
else
  echo "🌱 Deploying infrastructure via Garden..."
  garden deploy --env "$LANDSCAPE"
  echo "✅ Deployment complete!"
fi
