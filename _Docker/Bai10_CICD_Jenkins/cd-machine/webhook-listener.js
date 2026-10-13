// ============================================================
// Webhook Listener — Máy CD (Máy B) — Bài 10: CI/CD
// ============================================================
// Nhiệm vụ:
//   1. Lắng nghe HTTP POST /deploy từ Jenkins (Máy A)
//   2. Nhận thông tin image + tag mới
//   3. Chạy docker pull → docker compose up (deploy)
//
// Chạy: node webhook-listener.js
// Port mặc định: 5000
//
// Trong thực tế: có thể dùng webhook CLI, Watchtower, hoặc
// bất kỳ tool nào lắng nghe HTTP và chạy script.
// Ở đây dùng Node.js thuần để hiểu rõ cơ chế hoạt động.
// ============================================================

const http = require('http');
const { execSync, exec } = require('child_process');
const fs = require('fs');

const PORT = process.env.WEBHOOK_PORT || 5000;
const DEPLOY_DIR = process.env.DEPLOY_DIR || '/opt/deploy';
const LOG_FILE = process.env.LOG_FILE || './deploy.log';

// ── Ghi log ─────────────────────────────────────────────────
function log(message) {
    const timestamp = new Date().toISOString();
    const line = `[${timestamp}] ${message}`;
    console.log(line);
    fs.appendFileSync(LOG_FILE, line + '\n');
}

// ── Xử lý request deploy ───────────────────────────────────
function handleDeploy(body, res) {
    try {
        const data = JSON.parse(body);
        const { image, tag, build_number } = data;

        if (!image || !tag) {
            res.writeHead(400, { 'Content-Type': 'application/json' });
            res.end(JSON.stringify({ error: 'Missing image or tag' }));
            return;
        }

        const fullImage = `${image}:${tag}`;
        log(`========================================`);
        log(`🚀 DEPLOY REQUEST RECEIVED`);
        log(`   Image: ${fullImage}`);
        log(`   Build: #${build_number}`);
        log(`========================================`);

        // Trả response ngay cho Jenkins (không chờ deploy xong)
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
            status: 'deploying',
            image: fullImage,
            message: 'Deploy started'
        }));

        // Chạy deploy trong background
        deployAsync(image, tag, build_number);

    } catch (err) {
        log(`❌ Parse error: ${err.message}`);
        res.writeHead(400, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ error: 'Invalid JSON' }));
    }
}

// ── Deploy: pull image → update docker-compose → restart ────
async function deployAsync(image, tag, buildNumber) {
    try {
        const fullImage = `${image}:${tag}`;

        // Bước 1: Pull image mới từ Registry
        log(`📥 Step 1: Pulling image ${fullImage}...`);
        execSync(`docker pull ${fullImage}`, { stdio: 'inherit' });
        log(`✅ Image pulled successfully`);

        // Bước 2: Cập nhật biến IMAGE_TAG trong .env
        log(`📝 Step 2: Updating .env with TAG=${tag}...`);
        const envPath = `${DEPLOY_DIR}/.env`;
        if (fs.existsSync(envPath)) {
            let envContent = fs.readFileSync(envPath, 'utf8');
            // Thay thế IMAGE_TAG (hoặc thêm mới)
            if (envContent.includes('IMAGE_TAG=')) {
                envContent = envContent.replace(/IMAGE_TAG=.*/, `IMAGE_TAG=${tag}`);
            } else {
                envContent += `\nIMAGE_TAG=${tag}\n`;
            }
            if (envContent.includes('IMAGE_NAME=')) {
                envContent = envContent.replace(/IMAGE_NAME=.*/, `IMAGE_NAME=${image}`);
            } else {
                envContent += `IMAGE_NAME=${image}\n`;
            }
            fs.writeFileSync(envPath, envContent);
        }
        log(`✅ .env updated`);

        // Bước 3: Docker Compose up (restart với image mới)
        // Migration sẽ tự chạy khi app khởi động (xem Program.cs)
        log(`🔄 Step 3: Restarting services with docker compose...`);
        execSync(
            `cd ${DEPLOY_DIR} && docker compose up -d --force-recreate backend`,
            { stdio: 'inherit' }
        );
        log(`✅ Services restarted with ${fullImage}`);

        // Bước 4: Kiểm tra health
        log(`🏥 Step 4: Checking health...`);
        await new Promise(resolve => setTimeout(resolve, 5000)); // Chờ 5s
        try {
            execSync('curl -sf http://localhost/api/products', { timeout: 10000 });
            log(`✅ Health check PASSED — API is responding`);
        } catch {
            log(`⚠️ Health check failed — API may still be starting up`);
        }

        log(`========================================`);
        log(`✅ DEPLOY COMPLETE — Build #${buildNumber}`);
        log(`   Running: ${fullImage}`);
        log(`========================================\n`);

    } catch (err) {
        log(`❌ DEPLOY FAILED: ${err.message}`);
    }
}

// ── HTTP Server ─────────────────────────────────────────────
const server = http.createServer((req, res) => {
    // CORS headers (cho frontend demo gọi)
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

    if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
    }

    // POST /deploy — endpoint chính
    if (req.method === 'POST' && req.url === '/deploy') {
        let body = '';
        req.on('data', chunk => body += chunk);
        req.on('end', () => handleDeploy(body, res));
        return;
    }

    // GET /status — kiểm tra listener đang chạy
    if (req.method === 'GET' && req.url === '/status') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
            status: 'running',
            service: 'CD Webhook Listener — Bai 10',
            deploy_dir: DEPLOY_DIR,
            uptime: process.uptime()
        }));
        return;
    }

    // GET /logs — xem deploy log gần nhất
    if (req.method === 'GET' && req.url === '/logs') {
        res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
        try {
            const logs = fs.readFileSync(LOG_FILE, 'utf8');
            // Lấy 50 dòng cuối
            const lines = logs.split('\n').slice(-50).join('\n');
            res.end(lines);
        } catch {
            res.end('No logs yet');
        }
        return;
    }

    // 404 cho các route khác
    res.writeHead(404, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({
        error: 'Not found',
        endpoints: {
            'POST /deploy': 'Trigger deployment (from Jenkins)',
            'GET /status': 'Check listener status',
            'GET /logs': 'View recent deploy logs'
        }
    }));
});

server.listen(PORT, () => {
    log(`========================================`);
    log(`🎯 CD Webhook Listener started`);
    log(`   Port: ${PORT}`);
    log(`   Deploy dir: ${DEPLOY_DIR}`);
    log(`   Endpoints:`);
    log(`     POST /deploy  — Trigger deploy`);
    log(`     GET  /status  — Check status`);
    log(`     GET  /logs    — View logs`);
    log(`========================================`);
});
