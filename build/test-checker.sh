#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
test_project="$repo_root/LotSizingDataModel.Checker.Tests/LotSizingDataModel.Checker.Tests.csproj"
result_directory="$repo_root/TestResults/Checker"

mkdir -p "$result_directory"

dotnet test "$test_project" \
  -c Release \
  --logger "trx;LogFileName=checker-tests.trx" \
  --results-directory "$result_directory"

echo "Checker regression suite completed successfully."
echo "TRX: $result_directory/checker-tests.trx"
