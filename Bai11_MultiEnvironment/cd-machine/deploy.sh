#!/usr/bin/env bash
# ============================================================
# deploy.sh — chạy trên MÁY CD. Pull đúng tag image rồi dựng lại container.
# ============================================================
# Dùng cho CẢ 2 trường hợp:
#   - Deploy bản mới  : webhook-listener gọi với IMAGE_TAG=staging-42
#   - ROLLBACK bản cũ : Jenkins job rollback gọi với IMAGE_TAG=staging-41
# Không build lại từ source — chỉ kéo image đã có sẵn trên Registry.
#
# Biến vào:
#   IMAGE_TAG  (bắt buộc) tag cần chạy, vd: staging-42
#   ENV_DIR    (tùy chọn) thư mục chứa .env + docker-compose.cd.yml (mặc định = thư mục script)
#   DRY_RUN=1  (tùy chọn) chỉ in lệnh, KHÔNG gọi docker — dùng để test wiring.
# ============================================================
set -euo pipefail

ENV_DIR="${ENV_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)}"
COMPOSE_FILE="${ENV_DIR}/docker-compose.cd.yml"
ENV_FILE="${ENV_DIR}/.env"

if [[ -z "${IMAGE_TAG:-}" ]]; then
  echo "❌ Thiếu IMAGE_TAG. Vd: IMAGE_TAG=staging-42 bash deploy.sh" >&2
  exit 1
fi

# Kiểm tra tag an toàn (chữ/số . _ -)
if [[ ! "${IMAGE_TAG}" =~ ^[A-Za-z0-9._-]+$ ]]; then
  echo "❌ IMAGE_TAG không hợp lệ: ${IMAGE_TAG}" >&2
  exit 1
fi

if [[ ! -f "${ENV_FILE}" ]]; then
  echo "❌ Không thấy ${ENV_FILE}. Copy .env.example thành .env và điền giá trị trước." >&2
  exit 1
fi

echo "──────────────────────────────────────────────"
echo "🚀 Deploy tag: ${IMAGE_TAG}"
echo "   compose : ${COMPOSE_FILE}"
echo "   env     : ${ENV_FILE}"
echo "──────────────────────────────────────────────"

# Cập nhật dòng IMAGE_TAG trong .env (thêm nếu chưa có) — đây là "trạng thái đang chạy" của máy.
if grep -q '^IMAGE_TAG=' "${ENV_FILE}"; then
  # thay tại chỗ, có backup .env.bak để lỡ tay còn khôi phục
  sed -i.bak "s|^IMAGE_TAG=.*|IMAGE_TAG=${IMAGE_TAG}|" "${ENV_FILE}"
else
  echo "IMAGE_TAG=${IMAGE_TAG}" >> "${ENV_FILE}"
fi

run() {
  echo "+ $*"
  if [[ "${DRY_RUN:-0}" == "1" ]]; then
    return 0
  fi
  "$@"
}

# Pull đúng tag + dựng lại (chỉ recreate khi image đổi). --remove-orphans dọn container thừa.
run docker compose --env-file "${ENV_FILE}" -f "${COMPOSE_FILE}" pull
run docker compose --env-file "${ENV_FILE}" -f "${COMPOSE_FILE}" up -d --remove-orphans

echo "✅ Đã deploy tag ${IMAGE_TAG}."
if [[ "${DRY_RUN:-0}" == "1" ]]; then
  echo "   (DRY_RUN=1 — chỉ in lệnh, chưa gọi docker thật.)"
fi
