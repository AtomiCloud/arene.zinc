#!/usr/bin/env bash
set -eou pipefail

# install dependencies
echo "⬇️ Installing Dependencies..."
dotnet restore
echo "✅ Done!"

# run precommit
echo "🏃‍➡️ Running Pre-Commit..."
pre-commit run --all -v
echo "✅ Done!"
