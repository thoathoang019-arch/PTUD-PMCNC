#!/usr/bin/env bash
# Minh hoa su khac biet giua user thuong va root (sudo), va bao ve file cau hinh.
set -uo pipefail
APP_ROOT="/opt/onlineshop"; APP_USER="appuser"

echo "### A) User 'nobody' (others) co doc duoc file cau hinh bi mat khong? ###"
echo "\$ sudo -u nobody cat $APP_ROOT/config/appsettings.Production.json"
sudo -u nobody cat "$APP_ROOT/config/appsettings.Production.json" \
  && echo "(doc duoc - KHONG an toan!)" \
  || echo ">> Permission denied — file cau hinh duoc bao ve (quyen 640, others khong doc)."

echo
echo "### B) User chay app co ghi vao vung he thong (/etc) khong? ###"
echo "\$ sudo -u $APP_USER touch /etc/test-appuser.txt"
sudo -u "$APP_USER" touch /etc/test-appuser.txt \
  && echo "(ghi duoc?!)" \
  || echo ">> Permission denied — user thuong KHONG duoc dung /etc (can root)."

echo
echo "### C) Cung thao tac do voi sudo (root) thi duoc: ###"
echo "\$ sudo touch /etc/test-root.txt && ls -l /etc/test-root.txt"
sudo touch /etc/test-root.txt && ls -l /etc/test-root.txt && sudo rm -f /etc/test-root.txt
echo ">> Voi sudo (root) thi ghi duoc — nhung app KHONG nen chay bang root."
