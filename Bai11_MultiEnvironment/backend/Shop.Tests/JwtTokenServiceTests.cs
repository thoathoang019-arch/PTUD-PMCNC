// ============================================================
// Unit Test — JwtTokenService (Bài 11)
// Chứng minh bằng test: secret tách biệt theo môi trường.
//   - Cùng key: token verify OK.
//   - Khác key (Dev -> Staging): verify FAIL (tương đương 401 ở API).
// Test này nằm trong stage Test của pipeline: fail thì CHẶN build/push.
// ============================================================

using Shop.Business.Services;

namespace Shop.Tests;

public class JwtTokenServiceTests
{
    private const string DevKey = "dev-secret-key-0123456789-abcdefghij-DEV";
    private const string StagingKey = "staging-secret-key-9876543210-zyxwvut-STG";

    [Fact]
    public void Token_Generated_And_Validated_With_SameKey_IsValid()
    {
        var svc = new JwtTokenService(DevKey, "Development");

        var token = svc.GenerateToken("alice");
        var result = svc.ValidateToken(token);

        Assert.True(result.IsValid);
        Assert.Equal("alice", result.Username);
        Assert.Equal("Development", result.Issuer);
    }

    // ⭐ Test quan trọng nhất của Bài 11:
    // token ký bằng key Dev KHÔNG verify được bằng key Staging.
    [Fact]
    public void Token_From_DevKey_Fails_On_StagingKey()
    {
        var dev = new JwtTokenService(DevKey, "Development");
        var staging = new JwtTokenService(StagingKey, "Staging");

        var devToken = dev.GenerateToken("alice");
        var result = staging.ValidateToken(devToken);

        Assert.False(result.IsValid);              // -> API trả 401
        Assert.Contains("Chữ ký", result.Error);
    }

    [Fact]
    public void Tampered_Token_IsRejected()
    {
        var svc = new JwtTokenService(DevKey, "Development");
        var token = svc.GenerateToken("alice");

        // Sửa 2 ký tự cuối (phần signature) -> chữ ký sai.
        var tampered = token[..^2] + (token.EndsWith("a") ? "bb" : "aa");

        Assert.False(svc.ValidateToken(tampered).IsValid);
    }

    [Fact]
    public void Empty_Or_Null_Token_IsRejected()
    {
        var svc = new JwtTokenService(DevKey, "Development");

        Assert.False(svc.ValidateToken("").IsValid);
        Assert.False(svc.ValidateToken(null).IsValid);
        Assert.False(svc.ValidateToken("abc.def").IsValid);  // thiếu phần signature
    }

    [Fact]
    public void Constructor_Throws_When_Key_Missing()
    {
        Assert.Throws<ArgumentException>(() => new JwtTokenService("", "Development"));
    }
}
