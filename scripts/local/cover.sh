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

# Split target by comma and process each package
IFS=',' read -ra packages <<<"$target"
for package in "${packages[@]}"; do
  # Trim whitespace
  package=$(echo "$package" | xargs)

  # Get coverage for the package
  package_coverage=$(xmlstarlet sel -t -v "//package[@name='$package']/@line-rate" "coverage/$test_dir/coverage.cobertura.xml")

  # Check if package exists in coverage report
  if [ -z "$package_coverage" ]; then
    echo "⚠️ Warning: Package '$package' not found in coverage report"
    continue
  fi

  # Convert coverage to percentage
  package_percentage=$(echo "$package_coverage * 100" | bc -l | awk '{printf "%.2f", $0}')

  echo "🧪 Coverage for $package: $package_percentage%"
done

echo ""
