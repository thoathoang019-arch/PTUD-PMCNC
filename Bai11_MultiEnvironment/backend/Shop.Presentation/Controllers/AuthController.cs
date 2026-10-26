using Shop.Business.DTOs;
using Shop.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Controllers;

// ============================================================
// Bài 11: Đăng nhập JWT.
//   POST /api/auth/login  -> trả JWT ký bằng SecretKey của môi trường
//   GET  /api/auth/me     -> endpoint CẦN TOKEN (tự kiểm tra Bearer, không dùng [Authorize])
//
// Dùng để demo trước lớp: token lấy ở Dev đem sang Staging gọi /api/auth/me -> 401
// vì Staging ký/kiểm tra bằng SecretKey khác.
// ============================================================
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwt;
    private readonly IConfiguration _config;

    public AuthController(IJwtTokenService jwt, IConfiguration config)
    {
        _jwt = jwt;
        _config = config;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Thiếu username hoặc password." });

        // DEMO: không kiểm tra mật khẩu thật — mục tiêu là minh hoạ JWT ký bằng secret môi trường.
        // Thực tế: so khớp hash mật khẩu trong DB trước khi cấp token.
        var token = _jwt.GenerateToken(request.Username);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = request.Username,
            Environment = CurrentEnvironment,
            ExpiresInMinutes = 60
        });
    }

    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { message = "Thiếu Bearer token trong header Authorization." });

        var token = authHeader["Bearer ".Length..].Trim();
        var result = _jwt.ValidateToken(token);
        if (!result.IsValid)
            return Unauthorized(new { message = result.Error });

        return Ok(new MeResponse
        {
            Username = result.Username,
            TokenIssuer = result.Issuer,
            Environment = CurrentEnvironment
        });
    }

    private string CurrentEnvironment =>
        _config["App:Environment"]
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? "Unknown";
}
