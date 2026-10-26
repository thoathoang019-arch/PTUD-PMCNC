using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Controllers;

// ============================================================
// Bài 11: Endpoint để FE hiển thị badge "đang nối môi trường nào".
//   GET /api/env/info -> { environment, buildTag, jwtKeyLength, serverTimeUtc }
//
// jwtKeyLength chỉ trả về ĐỘ DÀI key (số ký tự) để chứng minh mỗi môi trường
// một key khác nhau — TUYỆT ĐỐI không trả về giá trị key thật.
// ============================================================
[ApiController]
[Route("api/[controller]")]
public class EnvController : ControllerBase
{
    private readonly IConfiguration _config;

    public EnvController(IConfiguration config) => _config = config;

    [HttpGet("info")]
    public IActionResult Info()
    {
        var env = _config["App:Environment"]
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "Unknown";

        var buildTag = _config["App:BuildTag"]
                       ?? Environment.GetEnvironmentVariable("APP_VERSION")
                       ?? "local-dev";

        var keyLength = (_config["Jwt:SecretKey"] ?? "").Length;

        return Ok(new
        {
            environment = env,
            buildTag,
            jwtKeyLength = keyLength,
            serverTimeUtc = DateTimeOffset.UtcNow.ToString("o")
        });
    }
}
