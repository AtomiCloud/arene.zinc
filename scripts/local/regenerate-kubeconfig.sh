#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "$0")/load-dev-config.sh"

echo "🛠 Regenerating kubeconfig"

# Create directories
mkdir -p "$HOME/.kube/configs"
mkdir -p "$HOME/.kube/k3dconfigs"
mkdir -p "$HOME/.kube/atomiconfigs"

# Export k3d cluster config
k3d kubeconfig get "$LANDSCAPE" >"$HOME/.kube/k3dconfigs/k3d-$LANDSCAPE"

# Build KUBECONFIG from existing files
cfgs=()
while IFS= read -r -d '' f; do cfgs+=("$f"); done < <(find "$HOME/.kube/configs" "$HOME/.kube/k3dconfigs" "$HOME/.kube/atomiconfigs" -maxdepth 1 -type f -print0 2>/dev/null)

KUBECONFIG="$(
  IFS=:
  echo "${cfgs[*]}"
)" kubectl config view --flatten >"$HOME/.kube/config"
chmod 600 ~/.kube/config

echo "✅ Kubeconfig regenerated"
