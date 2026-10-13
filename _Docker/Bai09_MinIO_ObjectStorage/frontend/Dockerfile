# ============================================================
# Dockerfile cho Frontend — Bài 09: MinIO Object Storage
# ============================================================
# Tái sử dụng pattern từ Bài 08: đóng gói SPA vào NGINX container
# NGINX serve static files + SPA fallback (try_files)
# ============================================================

FROM nginx:1.27-alpine

# Xóa config mặc định của NGINX
RUN rm /etc/nginx/conf.d/default.conf

# Copy file cấu hình NGINX cho frontend
COPY nginx-frontend.conf /etc/nginx/conf.d/default.conf

# Copy file HTML vào thư mục serve của NGINX
COPY index.html /usr/share/nginx/html/index.html

# NGINX listen port 80 (trong container)
EXPOSE 80

# NGINX chạy ở foreground (mặc định của image nginx)
CMD ["nginx", "-g", "daemon off;"]
