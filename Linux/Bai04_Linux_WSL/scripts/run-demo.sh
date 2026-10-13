#!/usr/bin/env bash
# Chay lai toan bo demo buoi 5 tu dau den cuoi (tien cho luyen tap & quay video).
set +e
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$HERE"

pkill -9 -f OnlineShopMini 2>/dev/null
bash 04-teardown.sh >/dev/null 2>&1
sleep 1

echo "# BAN GHI DEMO THUC TE - gan voi app OnlineShopMini cua nhom"
echo
echo "\$ dotnet --version"; dotnet --version
echo
echo "===== BUOC 1: PUBLISH APP ====="
echo "\$ bash 00-publish.sh \"/mnt/d/PTUD PMCNC/OnlineShopMini\" /tmp/onlineshop-publish"
dotnet publish "/mnt/d/PTUD PMCNC/OnlineShopMini" -c Release -o /tmp/onlineshop-publish 2>&1 | tail -2
echo
echo "===== BUOC 2: TAO USER/GROUP + PHAN QUYEN ====="
bash 01-setup-server.sh /tmp/onlineshop-publish
echo
echo "\$ id appuser"; id appuser
echo "\$ getent group appgrp"; getent group appgrp
echo
echo "===== BUOC 3: CHAY APP BANG appuser + CURL API ====="
bash 02-run-app.sh 5113
echo
echo "===== BUOC 4: SO SANH QUYEN user thuong vs root ====="
bash 03-permissions-demo.sh
echo
echo "===== BUOC 5: LOG DO appuser GHI ====="
echo "\$ ls -l /opt/onlineshop/logs/"; ls -l /opt/onlineshop/logs/
echo "\$ head -6 /opt/onlineshop/logs/app.log"; head -6 /opt/onlineshop/logs/app.log
echo
echo "===== HET ====="
