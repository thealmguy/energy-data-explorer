#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "$0")" && pwd)"
solution="$repo_root/energy-data-explorer.sln"
if [ ! -f "$solution" ]; then
  solution="$repo_root/energy-data-explorer.slnx"
fi

if [ ! -f "$solution" ]; then
  echo "Solution file not found: $repo_root/energy-data-explorer.sln or $repo_root/energy-data-explorer.slnx"
  exit 1
fi

dotnet restore "$solution"
dotnet build "$solution" -c Release

echo "Build completed successfully."
