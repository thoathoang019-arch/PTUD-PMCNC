#!/usr/bin/env bash
# Don dep demo: dung app, xoa user/group va thu muc /opt/onlineshop.
set -uo pipefail
echo "==> Dung app dang chay (neu co)"
pkill -9 -f OnlineShopMini 2>/dev/null || true
echo "==> Xoa user & group"
userdel appuser 2>/dev/null && echo "  da xoa appuser" || echo "  appuser khong ton tai"
groupdel appgrp 2>/dev/null && echo "  da xoa appgrp" || echo "  appgrp khong ton tai"
echo "==> Xoa thu muc app"
rm -rf /opt/onlineshop && echo "  da xoa /opt/onlineshop"
echo "Xong."
