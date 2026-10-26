#!/bin/bash
# ============================================================
# deploy.sh — Script Deploy trên Máy CD (Máy B)
# ============================================================
# Dùng khi muốn deploy thủ công (không qua webhook)
# Hoặc dùng bên trong webhook-listener.js
#
# Cách dùng: ./deploy.sh <image> <tag>
# Ví dụ:     ./deploy.sh ghcr.io/username/onlineshop-api 5
# ============================================================

set -e  # Dừng nếu có lỗi

IMAGE=${1:?"Thiếu tham số IMAGE. Dùng: ./deploy.sh <image> <tag>"}
TAG=${2:?"Thiếu tham số TAG. Dùng: ./deploy.sh <image> <tag>"}

echo "=========================================="
echo "🚀 DEPLOYING: ${IMAGE}:${TAG}"
echo "=========================================="

# Bước 1: Pull image mới
echo "📥 Pulling image..."
docker pull "${IMAGE}:${TAG}"

# Bước 2: Cập nhật .env
echo "📝 Updating .env..."
DEPLOY_DIR="$(cd "$(dirname "$0")" && pwd)"
sed -i "s|^IMAGE_TAG=.*|IMAGE_TAG=${TAG}|" "${DEPLOY_DIR}/.env" 2>/dev/null || true
sed -i "s|^IMAGE_NAME=.*|IMAGE_NAME=${IMAGE}|" "${DEPLOY_DIR}/.env" 2>/dev/null || true

# Bước 3: Restart services
echo "🔄 Restarting services..."
cd "${DEPLOY_DIR}"
docker compose up -d --force-recreate backend

# Bước 4: Chờ và kiểm tra health
echo "⏳ Waiting for app to start..."
sleep 10

echo "🏥 Checking health..."
if curl -sf http://localhost/api/products > /dev/null 2>&1; then
    echo "✅ DEPLOY SUCCESS — API is responding"
else
    echo "⚠️  API not responding yet (may still be starting)"
fi

echo "=========================================="
echo "✅ Deploy complete: ${IMAGE}:${TAG}"
echo "=========================================="
