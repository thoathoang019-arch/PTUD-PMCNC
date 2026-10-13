# Dockerfile cho Frontend - Đóng gói file tĩnh vào NGINX image
# Đây là cách phổ biến nhất để deploy frontend (React/Vue/Angular) trong thực tế:
# Build xong (npm run build) → copy bản build vào image NGINX → NGINX phục vụ file tĩnh.

FROM nginx:1.27-alpine

# Xóa file cấu hình mặc định của NGINX
RUN rm /etc/nginx/conf.d/default.conf

# Copy file tĩnh vào thư mục gốc của NGINX
COPY index.html /usr/share/nginx/html/index.html

# Copy cấu hình NGINX cho SPA (try_files fallback)
COPY nginx-frontend.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
