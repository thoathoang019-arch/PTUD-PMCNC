#!/usr/bin/env bash
# Build image tu Dockerfile roi chay container, anh xa cong, truyen ENV, test API.
set -euo pipefail
IMAGE="onlineshop-api:v1"
NAME="onlineshop"
cd "$(dirname "$0")/OnlineShop"

echo "==> [1] Build image (multi-stage, ARG BUILD_CONFIGURATION=Release)"
docker build -t "$IMAGE" .

echo "==> [2] Xoa container cu (neu co) roi chay container moi"
docker rm -f "$NAME" 2>/dev/null || true

# Truyen bien moi truong bang --env-file (gon hon khi co nhieu bien)
# Hoac dung -e KEY=VALUE de truyen tung bien
docker run -d --name "$NAME" -p 8080:8080 \
  --env-file .env.example \
  "$IMAGE"

echo "==> [3] Trang thai container"; sleep 3; docker ps

echo "==> [4] Kiem tra user (phai la appuser, khong phai root)"
docker exec "$NAME" whoami

echo "==> [5] Test API bang curl"
curl -s http://localhost:8080/api/products | head -c 300; echo

echo "==> Xem log:   docker logs $NAME"
echo "==> Vao trong: docker exec -it $NAME bash"
echo "==> Don dep:   docker stop $NAME && docker rm $NAME && docker rmi $IMAGE"
