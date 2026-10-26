# ===================================================================
# FRONTEND DOCKERFILE — BÀI 10: CI/CD VỚI JENKINS
# ===================================================================
# Mục đích: Đóng gói frontend SPA vào NGINX container
# Giống Bài 08, 09 — tái sử dụng pattern đã học
# ===================================================================

# --- Stage duy nhất: NGINX phục vụ file tĩnh ---
# NGINX Alpine nhẹ (~23MB), phù hợp cho static file serving
FROM nginx:1.27-alpine

# === Metadata theo OCI Image Spec ===
LABEL maintainer="PTUD-PMCNC-Student"
LABEL description="Frontend CI/CD Demo - Bài 10"

# --- Xóa default config của NGINX ---
RUN rm /etc/nginx/conf.d/default.conf

# --- Copy custom NGINX config ---
# nginx-frontend.conf: cấu hình phục vụ SPA (Single Page Application)
COPY nginx-frontend.conf /etc/nginx/conf.d/default.conf

# --- Copy file HTML vào thư mục phục vụ ---
# /usr/share/nginx/html là document root mặc định của NGINX
COPY index.html /usr/share/nginx/html/index.html

# --- Port mà container lắng nghe ---
EXPOSE 80

# --- Healthcheck: kiểm tra NGINX có phản hồi không ---
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -qO- http://localhost/ || exit 1

# --- NGINX chạy foreground (không daemon) ---
CMD ["nginx", "-g", "daemon off;"]
