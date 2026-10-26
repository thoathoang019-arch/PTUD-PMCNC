namespace Shop.Business.Interfaces;

// ============================================================
// Bài 11: Dịch vụ JWT ký/kiểm tra token bằng Secret Key.
// Mỗi môi trường (Dev/Staging/Prod) nạp một SecretKey khác nhau,
// nên token cấp ở môi trường này sẽ KHÔNG verify được ở môi trường khác.
// ============================================================

/// <summary>Kết quả kiểm tra token: hợp lệ hay không, kèm thông tin đọc được.</summary>
public record TokenValidationResult(bool IsValid, string? Username, string? Issuer, string? Error)
{
    public static TokenValidationResult Success(string? username, string? issuer)
        => new(true, username, issuer, null);

    public static TokenValidationResult Fail(string error)
        => new(false, null, null, error);
}

public interface IJwtTokenService
{
    /// <summary>Sinh JWT (HS256) cho <paramref name="username"/>, ký bằng SecretKey của môi trường.</summary>
    string GenerateToken(string username, int expiresMinutes = 60);

    /// <summary>Kiểm tra token: đúng chữ ký (đúng SecretKey) và chưa hết hạn.</summary>
    TokenValidationResult ValidateToken(string? token);
}
