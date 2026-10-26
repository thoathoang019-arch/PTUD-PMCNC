#!/usr/bin/env bash
# Publish app backend cua nhom ra ban chay (self-contained cua .NET).
# Dung:  bash 00-publish.sh <duong-dan-project> [thu-muc-out]
set -euo pipefail
PROJECT="${1:-}"
OUT="${2:-/tmp/onlineshop-publish}"
if [ -z "$PROJECT" ]; then
  echo "Dung: bash 00-publish.sh <project.csproj hoac thu-muc-project> [out]"
  echo "Vi du: bash 00-publish.sh ~/OnlineShopMini /tmp/onlineshop-publish"
  exit 1
fi
dotnet publish "$PROJECT" -c Release -o "$OUT"
echo "Da publish vao: $OUT"
