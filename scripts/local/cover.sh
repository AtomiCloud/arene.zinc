#!/usr/bin/env bash

test_dir="$1"
target="$2"

# Ensure test_dir is not null or empty
[ -z "$test_dir" ] && echo "❌ Error: test directory parameter is required" && exit 1
[ -z "$target" ] && echo "❌ Error: target parameter is required" && exit 1

set -eou pipefail

# Function for cleanup
cleanup() {
  echo "🧹 Cleaning up TestResults directory..."
  rm -rf "$test_dir/TestResults"
}

# Set trap to ensure cleanup happens on exit
trap cleanup EXIT

# Clean up before starting
echo "🧹 Cleaning up previous test results..."
rm -rf "$test_dir/TestResults"
rm -rf "coverage/$test_dir"

# Create destination directory
echo "📁 Creating coverage directory..."
mkdir -p "coverage/$test_dir"

# Run tests and generate coverage
echo "🧪 Running tests with coverage collection..."
dotnet test "$test_dir" --collect:"XPlat Code Coverage;Format=cobertura"

# Find and copy coverage files to the destination
echo "📋 Copying coverage files to destination..."
find "$test_dir/TestResults" -name "coverage.cobertura.xml" | while read -r file; do
  echo "📄 Copying $(basename "$file")..."
  cp "$file" "coverage/$test_dir/"
done

echo "✅ Coverage processing complete!"

echo ""

# Get overall coverage first using grep and sed
overall_line_rate=$(grep -m 1 '<coverage' "coverage/$test_dir/coverage.cobertura.xml" | sed -n 's/.*line-rate="\([^"]*\)".*/\1/p')
overall_branch_rate=$(grep -m 1 '<coverage' "coverage/$test_dir/coverage.cobertura.xml" | sed -n 's/.*branch-rate="\([^"]*\)".*/\1/p')
overall_line_pct=$(echo "$overall_line_rate * 100" | bc -l | awk '{printf "%.2f", $0}')
overall_branch_pct=$(echo "$overall_branch_rate * 100" | bc -l | awk '{printf "%.2f", $0}')

echo "📊 Overall Coverage: $overall_line_pct% lines, $overall_branch_pct% branches"
echo ""

# Split target by comma and process each package
IFS=',' read -ra packages <<<"$target"
for package in "${packages[@]}"; do
  # Trim whitespace
  package=$(echo "$package" | xargs)

  # Get coverage for the package using grep and sed
  package_line=$(grep "package name=\"$package\"" "coverage/$test_dir/coverage.cobertura.xml")

  # Check if package exists in coverage report
  if [ -z "$package_line" ]; then
    echo "⚠️  Warning: Package '$package' not found in coverage report"
    continue
  fi

  package_coverage=$(echo "$package_line" | sed -n 's/.*line-rate="\([^"]*\)".*/\1/p')
  package_branch=$(echo "$package_line" | sed -n 's/.*branch-rate="\([^"]*\)".*/\1/p')

  # Convert coverage to percentage
  package_line_pct=$(echo "$package_coverage * 100" | bc -l | awk '{printf "%.2f", $0}')
  package_branch_pct=$(echo "$package_branch * 100" | bc -l | awk '{printf "%.2f", $0}')

  echo "🧪 Coverage for $package: $package_line_pct% lines, $package_branch_pct% branches"
done

echo ""
