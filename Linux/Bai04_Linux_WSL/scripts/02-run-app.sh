#!/usr/bin/env bash
# Chay app bang USER RIENG (khong phai root) va ghi log ra logs/app.log
# Dung:  sudo bash 02-run-app.sh [port]
set -uo pipefail
APP_NAME="onlineshop"; APP_USER="appuser"; APP_ROOT="/opt/${APP_NAME}"
DLL_NAME="OnlineShopMini.dll"; PORT="${1:-5113}"

echo "==> Khoi chay app bang user '$APP_USER' tren cong $PORT (log -> $APP_ROOT/logs/app.log)"
# cd vao app/ de content-root dung; HOME tro vao logs de app ghi tam.
sudo -u "$APP_USER" env \
  HOME="$APP_ROOT/logs" \
  ASPNETCORE_URLS="http://localhost:${PORT}" \
  DOTNET_ENVIRONMENT=Production \
  DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  bash -c "cd '$APP_ROOT/app' && nohup dotnet '$DLL_NAME' >> '$APP_ROOT/logs/app.log' 2>&1 &"

echo "   Doi app khoi dong..."; sleep 8
echo "==> Goi thu API bang curl (chung minh app chay duoc voi quyen han che):"
curl -s "http://localhost:${PORT}/api/products" | head -c 400 || true
echo; echo
echo "==> Tien trinh dang chay (chu USER la 'appuser', KHONG phai root):"
ps -o user,pid,cmd -C dotnet | head || true
