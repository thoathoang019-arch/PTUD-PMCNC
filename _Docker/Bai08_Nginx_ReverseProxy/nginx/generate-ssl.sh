#!/bin/sh
# ═══════════════════════════════════════════════════════════════
# Script tạo chứng chỉ SSL tự ký (self-signed) cho demo local
# ═══════════════════════════════════════════════════════════════
#
# Giải thích lệnh openssl:
#   req -x509       : Tạo chứng chỉ X.509 (tiêu chuẩn SSL/TLS)
#   -nodes          : Không đặt mật khẩu cho private key
#   -newkey rsa:2048: Tạo RSA key 2048-bit (đủ mạnh cho demo)
#   -keyout         : Đường dẫn lưu private key
#   -out            : Đường dẫn lưu certificate
#   -days 365       : Thời hạn 1 năm
#   -subj "/CN=..." : Common Name = domain name
#
# LƯU Ý QUAN TRỌNG:
# - Chứng chỉ tự ký → trình duyệt LUÔN cảnh báo "Not Secure"
# - Trên production: dùng Certbot/Let's Encrypt để lấy chứng chỉ miễn phí
#   được trình duyệt tin cậy thật sự

SSL_DIR="/etc/nginx/ssl"
CERT_FILE="$SSL_DIR/myapp.crt"
KEY_FILE="$SSL_DIR/myapp.key"

# Kiểm tra nếu chứng chỉ đã tồn tại → bỏ qua (tránh tạo lại mỗi lần restart)
if [ -f "$CERT_FILE" ] && [ -f "$KEY_FILE" ]; then
    echo "✅ Chứng chỉ SSL đã tồn tại, bỏ qua việc tạo mới."
    echo "   - Certificate: $CERT_FILE"
    echo "   - Private Key: $KEY_FILE"
    exit 0
fi

mkdir -p "$SSL_DIR"

# Cài openssl nếu chưa có (một số bản nginx:alpine có thể thiếu)
if ! command -v openssl > /dev/null 2>&1; then
    echo "⏳ Cài đặt openssl..."
    apk add --no-cache openssl
fi

openssl req -x509 -nodes -newkey rsa:2048 \
    -keyout "$KEY_FILE" \
    -out "$CERT_FILE" \
    -days 365 \
    -subj "/C=VN/ST=HCMC/L=HCMC/O=OnlineShop/CN=myapp.local"

echo "✅ Đã tạo chứng chỉ SSL tự ký tại $SSL_DIR/"
echo "   - Certificate: $CERT_FILE"
echo "   - Private Key: $KEY_FILE"
