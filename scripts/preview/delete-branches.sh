#!/usr/bin/env zsh
set -euo pipefail

if [ ! -f ./.preview-branches ]; then
  echo "Error: .preview-branches file not found"
  echo "No branches to clean up"
  exit 0
fi

# Load branch info
source ./.preview-branches

echo "Deleting preview environment branches..."
echo "Branch name: $BRANCH_NAME"

# Check required environment variables
: "${NEON_API_KEY:?Error: NEON_API_KEY not set}"
: "${TIGRIS_API_KEY:?Error: TIGRIS_API_KEY not set}"

# ============================================================================
# Delete Neon Database Branch
# ============================================================================

echo ""
echo "Deleting Neon database branch..."

curl -s -X DELETE \
  "https://console.neon.tech/api/v2/projects/${NEON_PROJECT_ID}/branches/${NEON_BRANCH_ID}" \
  -H "Authorization: Bearer ${NEON_API_KEY}"

echo "✓ Neon branch deleted: $NEON_BRANCH_ID"

# ============================================================================
# Delete Tigris Bucket Branch
# ============================================================================

echo ""
echo "Deleting Tigris bucket branch..."

curl -s -X DELETE \
  "https://api.preview.tigrisdata.cloud/v1/projects/${TIGRIS_PROJECT_ID}/buckets/${TIGRIS_BRANCH_BUCKET}" \
  -H "Authorization: Bearer ${TIGRIS_API_KEY}"

echo "✓ Tigris branch deleted: $TIGRIS_BRANCH_BUCKET"

# ============================================================================
# Cleanup
# ============================================================================

rm -f ./.preview-branches
rm -f ./infra/root_chart/values.absol-generated.yaml

echo ""
echo "✓ Cleanup complete"
