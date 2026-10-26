namespace Shop.Business.DTOs;

// ============================================================
// Bài 11: DTO cho tính năng đăng nhập JWT.
// ============================================================

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public string Environment { get; set; } = "";
    public int ExpiresInMinutes { get; set; }
}

public class MeResponse
{
    public string? Username { get; set; }
    public string? TokenIssuer { get; set; }
    public string Environment { get; set; } = "";
}
