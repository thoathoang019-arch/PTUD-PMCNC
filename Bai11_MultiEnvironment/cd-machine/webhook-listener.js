// ============================================================
// webhook-listener.js — chạy trên MỖI MÁY CD (Dev/Staging/Prod)
// ============================================================
// Vai trò: nhận webhook HTTP từ Jenkins (CI) -> gọi deploy.sh để
//          docker pull đúng tag rồi docker compose up -d.
//
// KHÔNG dùng thư viện ngoài — chỉ core module 'http'/'child_process'
// (chạy được ngay bằng: node webhook-listener.js).
//
// Endpoints:
//   GET  /health   -> 200 "ok"          (Jenkins/monitor kiểm tra máy sống)
//   POST /deploy   -> body JSON { tag, image?, build_number? }
//                     chạy deploy.sh với TAG tương ứng, trả 202 Accepted.
//
// Bảo vệ tối thiểu: header "X-Deploy-Token" phải khớp DEPLOY_TOKEN (nếu đặt).
// ============================================================

const http = require('http');
const { spawn } = require('child_process');
const path = require('path');

const PORT = parseInt(process.env.PORT || '9000', 10);
const DEPLOY_TOKEN = process.env.DEPLOY_TOKEN || ''; // để trống = không bắt buộc token (chỉ nên dùng trong LAN lớp học)
const DEPLOY_SCRIPT = process.env.DEPLOY_SCRIPT || path.join(__dirname, 'deploy.sh');

// Tag hợp lệ: chữ/số/dấu . _ - (vd dev-18, staging-42, prod-7-ab12cd3). Chặn injection.
const TAG_RE = /^[A-Za-z0-9._-]{1,128}$/;

function send(res, code, obj) {
  const body = JSON.stringify(obj);
  res.writeHead(code, { 'Content-Type': 'application/json; charset=utf-8' });
  res.end(body);
}

const server = http.createServer((req, res) => {
  if (req.method === 'GET' && req.url === '/health') {
    return send(res, 200, { status: 'ok', ts: new Date().toISOString() });
  }

  if (req.method === 'POST' && req.url === '/deploy') {
    if (DEPLOY_TOKEN && req.headers['x-deploy-token'] !== DEPLOY_TOKEN) {
      return send(res, 401, { error: 'Sai hoặc thiếu X-Deploy-Token.' });
    }

    let raw = '';
    req.on('data', (c) => {
      raw += c;
      if (raw.length > 1e6) req.destroy(); // chặn body quá lớn
    });
    req.on('end', () => {
      let payload;
      try {
        payload = JSON.parse(raw || '{}');
      } catch {
        return send(res, 400, { error: 'Body không phải JSON hợp lệ.' });
      }

      const tag = String(payload.tag || '').trim();
      if (!TAG_RE.test(tag)) {
        return send(res, 400, { error: 'Thiếu/không hợp lệ "tag".', got: tag });
      }

      console.log(`[deploy] nhận yêu cầu deploy tag=${tag} build=${payload.build_number || '?'}`);

      // Gọi deploy.sh với TAG. Không chờ xong (deploy có thể lâu) -> trả 202 ngay.
      const child = spawn('bash', [DEPLOY_SCRIPT], {
        env: { ...process.env, IMAGE_TAG: tag },
        stdio: 'inherit',
      });
      child.on('error', (e) => console.error('[deploy] lỗi chạy deploy.sh:', e.message));
      child.on('exit', (code) => console.log(`[deploy] deploy.sh kết thúc, exit=${code}`));

      return send(res, 202, { accepted: true, tag });
    });
    return;
  }

  send(res, 404, { error: 'Not found' });
});

server.listen(PORT, () => {
  console.log(`webhook-listener nghe trên cổng ${PORT} (deploy script: ${DEPLOY_SCRIPT})`);
});
