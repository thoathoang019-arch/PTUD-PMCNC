#!/bin/bash
# ============================================================
# generate-deploy-key.sh — Tạo SSH Deploy Key cho Jenkins
# ============================================================
# Deploy Key: cặp SSH key RIÊNG cho Jenkins (không dùng key cá nhân)
#
# Bước 1: Chạy script này → tạo jenkins_deploy_key + .pub
# Bước 2: Copy nội dung .pub → GitHub repo → Settings → Deploy keys
# Bước 3: Copy nội dung private key → Jenkins Credentials
#
# ⚠️ Mỗi Deploy Key chỉ gắn được với MỘT repository
# ============================================================

echo "🔑 Generating SSH Deploy Key for Jenkins..."
echo ""

# Tạo key pair (ed25519 — thuật toán mạnh, key ngắn)
# -C: comment (ghi chú để nhận dạng)
# -f: file output
# -N "": không đặt passphrase (Jenkins cần tự động dùng)
ssh-keygen -t ed25519 -C "jenkins-deploy-key" -f jenkins_deploy_key -N ""

echo ""
echo "✅ Key pair generated!"
echo ""
echo "=========================================="
echo "📋 PUBLIC KEY (dán vào GitHub Deploy Keys):"
echo "=========================================="
cat jenkins_deploy_key.pub
echo ""
echo "=========================================="
echo "🔒 PRIVATE KEY (dán vào Jenkins Credentials):"
echo "=========================================="
echo "File: jenkins_deploy_key"
echo "(Mở file này, copy toàn bộ nội dung)"
echo ""
echo "=========================================="
echo "📌 Các bước tiếp theo:"
echo "=========================================="
echo "1. GitHub → Repository → Settings → Deploy keys → Add deploy key"
echo "   Dán nội dung file: jenkins_deploy_key.pub"
echo "   KHÔNG tick 'Allow write access' (Jenkins chỉ cần đọc)"
echo ""
echo "2. Jenkins → Manage Jenkins → Credentials → Add"
echo "   Kind: SSH Username with private key"
echo "   ID: github-deploy-key"
echo "   Username: git"
echo "   Private Key: Enter directly → paste nội dung jenkins_deploy_key"
echo "=========================================="
