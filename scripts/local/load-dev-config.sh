#!/usr/bin/env bash
# Load development configuration from dev.yaml
# Usage: source ./scripts/local/load-dev-config.sh

DEV_YAML="${DEV_YAML:-./dev.yaml}"

if [ ! -f "$DEV_YAML" ]; then
  echo "❌ $DEV_YAML not found"
  exit 1
fi

# Parse LPSM variables
export LANDSCAPE=$(grep '^landscape:' "$DEV_YAML" | awk '{print $2}')
export PLATFORM=$(grep '^platform:' "$DEV_YAML" | awk '{print $2}')
export SERVICE=$(grep '^service:' "$DEV_YAML" | awk '{print $2}')
export LAYER=$(grep '^layer:' "$DEV_YAML" | awk '{print $2}' | tr -d '"')
export MODE=$(grep '^mode:' "$DEV_YAML" | awk '{print $2}')

# ASP.NET Configuration
export ASPNETCORE_ENVIRONMENT=$(grep 'environment:' "$DEV_YAML" | grep -A1 'aspnetcore:' | tail -1 | awk '{print $2}')
export ASPNETCORE_URLS=$(grep 'urls:' "$DEV_YAML" | awk '{print $2}' | tr -d '"')

# Mirrord Configuration
export MIRRORD_TARGET=$(grep 'target:' "$DEV_YAML" | grep -A2 'mirrord:' | grep 'target:' | awk '{print $2}')
export MIRRORD_NAMESPACE=$(grep 'targetNamespace:' "$DEV_YAML" | awk '{print $2}')

# Defaults
LANDSCAPE="${LANDSCAPE:-lapras}"
PLATFORM="${PLATFORM:-arene}"
SERVICE="${SERVICE:-zinc}"
LAYER="${LAYER:-2}"
MODE="${MODE:-local}"
ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://+:9001}"
MIRRORD_TARGET="${MIRRORD_TARGET:-pod/zinc-mirrord-target-0}"
MIRRORD_NAMESPACE="${MIRRORD_NAMESPACE:-arene}"
