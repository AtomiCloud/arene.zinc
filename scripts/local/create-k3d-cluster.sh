#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/load-dev-config.sh"

CONFIG="./infra/k3d.$LANDSCAPE.yaml"

if [ ! -f "$CONFIG" ]; then
  echo "❌ Config not found: $CONFIG"
  exit 1
fi

# Check if cluster exists
if k3d cluster list | grep -q "^$LANDSCAPE"; then
  echo "✅ Cluster $LANDSCAPE already exists"
  exit 0
fi

# Create cluster
echo "🥟 Creating k3d cluster: $LANDSCAPE"
k3d cluster create "$LANDSCAPE" --config "$CONFIG" --wait
echo "🚀 Cluster created!"

# Wait for core components
echo "🕑 Waiting for cluster readiness..."
CTX="k3d-$LANDSCAPE"
kubectl --context "$CTX" -n kube-system wait --for=jsonpath=.status.readyReplicas=1 --timeout=300s deployment metrics-server
kubectl --context "$CTX" -n kube-system wait --for=jsonpath=.status.readyReplicas=1 --timeout=300s deployment coredns
kubectl --context "$CTX" -n kube-system wait --for=jsonpath=.status.readyReplicas=1 --timeout=300s deployment local-path-provisioner
kubectl --context "$CTX" -n kube-system wait --for=jsonpath=.status.succeeded=1 --timeout=300s job helm-install-traefik-crd
kubectl --context "$CTX" -n kube-system wait --for=jsonpath=.status.succeeded=1 --timeout=300s job helm-install-traefik
kubectl --context "$CTX" -n kube-system wait --for=jsonpath=.status.readyReplicas=1 --timeout=300s deployment traefik
echo "✅ Cluster ready!"
