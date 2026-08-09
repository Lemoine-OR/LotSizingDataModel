#!/usr/bin/env sh
set -eu
ROOT="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
dotnet test "$ROOT/LotSizingDataModel.Checker.Tests/LotSizingDataModel.Checker.Tests.csproj" -c "${1:-Debug}"
echo "Expected test count after Package 17: 30"
