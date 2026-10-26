using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Shop.Business.Interfaces;

namespace Shop.Business.Services;

// ============================================================
// JwtTokenService — tự cài JWT HS256 bằng System.Security.Cryptography
// (KHÔNG dùng gói NuGet ngoài, giữ project build gọn như các buổi trước).
//
// Cấu trúc JWT: base64url(header).base64url(payload).base64url(signature)
//   - header  = {"alg":"HS256","typ":"JWT"}
//   - payload = {"sub":username,"iss":env,"iat":...,"exp":...}
//   - signature = HMAC-SHA256(secretKey, header.payload)
//
// Điểm mấu chốt Bài 11: signature phụ thuộc secretKey. Đổi key = đổi signature,
// nên token của Dev không qua được kiểm tra ở Staging/Prod (khác key) -> 401.
// ============================================================
public class JwtTokenService : IJwtTokenService
{
    private readonly byte[] _key;
    private readonly string _issuer;

    public JwtTokenService(string secretKey, string issuer)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Jwt:SecretKey chưa được cấu hình.", nameof(secretKey));

        _key = Encoding.UTF8.GetBytes(secretKey);
        _issuer = issuer;
    }

    public string GenerateToken(string username, int expiresMinutes = 60)
    {
        var now = DateTimeOffset.UtcNow;

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["sub"] = username,
            ["iss"] = _issuer,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(expiresMinutes).ToUnixTimeSeconds()
        };

        var headerPart = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var payloadPart = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{headerPart}.{payloadPart}";
        var signaturePart = Base64UrlEncode(Sign(signingInput));

        return $"{signingInput}.{signaturePart}";
    }

    public TokenValidationResult ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return TokenValidationResult.Fail("Token rỗng.");

        var parts = token.Split('.');
        if (parts.Length != 3)
            return TokenValidationResult.Fail("Token sai định dạng (cần 3 phần header.payload.signature).");

        // 1) Kiểm tra chữ ký: ký lại header.payload bằng key của môi trường này rồi so sánh.
        var signingInput = $"{parts[0]}.{parts[1]}";
        var expectedSig = Base64UrlEncode(Sign(signingInput));

        // So sánh theo kiểu constant-time để chống timing attack.
        var equal = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSig),
            Encoding.UTF8.GetBytes(parts[2]));
        if (!equal)
            return TokenValidationResult.Fail("Chữ ký không hợp lệ — sai SecretKey của môi trường?");

        // 2) Đọc payload, kiểm tra hạn dùng.
        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var exp = root.GetProperty("exp").GetInt64();
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
                return TokenValidationResult.Fail("Token đã hết hạn.");

            var sub = root.TryGetProperty("sub", out var s) ? s.GetString() : null;
            var iss = root.TryGetProperty("iss", out var i) ? i.GetString() : null;
            return TokenValidationResult.Success(sub, iss);
        }
        catch
        {
            return TokenValidationResult.Fail("Payload không đọc được.");
        }
    }

    private byte[] Sign(string input)
    {
        using var hmac = new HMACSHA256(_key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    // base64url: base64 chuẩn nhưng thay +/ -> -_ và bỏ padding '='.
    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
