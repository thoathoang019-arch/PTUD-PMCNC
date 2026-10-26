// ============================================================
// Unit Test — Bài 10: CI/CD với Jenkins
// ============================================================
// Mục đích:
//   - Kiểm tra logic nghiệp vụ (business logic) ĐÚNG hay SAI
//   - Chạy tự động trong Jenkins pipeline (stage 'Test')
//   - Nếu test FAIL → pipeline DỪNG, KHÔNG build/push image
//
// Unit Test là gì?
//   - Test một "đơn vị" nhỏ nhất (1 method/function)
//   - Chạy ĐỘC LẬP — không cần database, network, file system
//   - Chạy NHANH — milliseconds
//   - Có thể chạy lại BẤT CỨ LÚC NÀO
//
// Framework: xUnit (.NET) — tương tự Jest (Node.js), PyTest (Python)
// Pattern: Arrange → Act → Assert (AAA)
//   Arrange: Chuẩn bị dữ liệu, khởi tạo object
//   Act:     Gọi method cần test
//   Assert:  Kiểm tra kết quả đúng như kỳ vọng
// ============================================================

using Shop.Business.Enums;
using Shop.Business.Services;
using Shop.Business.Strategies;

namespace Shop.Tests;

/// <summary>
/// Test ShippingFeeService — logic tính phí vận chuyển (OCP - Strategy pattern)
/// Đây là business logic core, PHẢI có Unit Test để đảm bảo tính đúng đắn.
/// </summary>
public class ShippingFeeServiceTests
{
    // Tạo service với tất cả strategies — mô phỏng DI container
    private readonly ShippingFeeService _service;

    public ShippingFeeServiceTests()
    {
        // Arrange (chung cho tất cả test): khởi tạo service với các strategies
        var strategies = new IShippingFeeStrategy[]
        {
            new StandardShippingStrategy(),
            new ExpressShippingStrategy(),
            new SameDayShippingStrategy(),
            new InternationalShippingStrategy()
        };
        _service = new ShippingFeeService(strategies);
    }

    // ── Test 1: Giao thường — miễn phí khi đơn >= 500.000đ ──────────
    [Fact]
    public void Standard_FreeShipping_WhenOrderAbove500k()
    {
        // Arrange
        var method = ShippingMethod.Standard;
        var amount = 600_000m; // > 500.000đ

        // Act
        var fee = _service.CalculateFee(method, amount);

        // Assert: phải miễn phí
        Assert.Equal(0m, fee);
    }

    // ── Test 2: Giao thường — tính phí 15.000đ khi đơn < 500.000đ ───
    [Fact]
    public void Standard_ChargesFee_WhenOrderBelow500k()
    {
        // Arrange
        var method = ShippingMethod.Standard;
        var amount = 200_000m; // < 500.000đ

        // Act
        var fee = _service.CalculateFee(method, amount);

        // Assert: phải tính phí 15.000đ
        Assert.Equal(15_000m, fee);
    }

    // ── Test 3: Giao thường — edge case đúng 500.000đ ───────────────
    [Fact]
    public void Standard_FreeShipping_WhenOrderExactly500k()
    {
        // Arrange — test ranh giới (boundary test)
        var fee = _service.CalculateFee(ShippingMethod.Standard, 500_000m);

        // Assert: đúng 500k → miễn phí
        Assert.Equal(0m, fee);
    }

    // ── Test 4: Giao nhanh — luôn 30.000đ ───────────────────────────
    [Fact]
    public void Express_AlwaysCharges30k()
    {
        // Act — test với nhiều mức giá khác nhau
        var fee1 = _service.CalculateFee(ShippingMethod.Express, 100_000m);
        var fee2 = _service.CalculateFee(ShippingMethod.Express, 1_000_000m);

        // Assert: luôn 30.000đ bất kể giá trị đơn hàng
        Assert.Equal(30_000m, fee1);
        Assert.Equal(30_000m, fee2);
    }

    // ── Test 5: Giao trong ngày — luôn 50.000đ ──────────────────────
    [Fact]
    public void SameDay_AlwaysCharges50k()
    {
        var fee = _service.CalculateFee(ShippingMethod.SameDay, 300_000m);
        Assert.Equal(50_000m, fee);
    }

    // ── Test 6: Giao quốc tế — luôn 200.000đ ───────────────────────
    [Fact]
    public void International_AlwaysCharges200k()
    {
        var fee = _service.CalculateFee(ShippingMethod.International, 1_000_000m);
        Assert.Equal(200_000m, fee);
    }

    // ── Test 7: Phương thức không hỗ trợ → throw exception ──────────
    [Fact]
    public void UnsupportedMethod_ThrowsException()
    {
        // Arrange: tạo service CHỈ có StandardStrategy (thiếu các loại khác)
        var limitedService = new ShippingFeeService(new IShippingFeeStrategy[]
        {
            new StandardShippingStrategy()
        });

        // Act & Assert: gọi Express nhưng không có strategy → exception
        Assert.Throws<InvalidOperationException>(
            () => limitedService.CalculateFee(ShippingMethod.Express, 100_000m)
        );
    }

    // ── Test 8: Theory — test nhiều input cùng 1 logic ──────────────
    // [Theory] + [InlineData] cho phép chạy cùng test với nhiều bộ dữ liệu
    // Rất hữu ích để test nhiều edge case mà không viết nhiều method
    [Theory]
    [InlineData(0, 15_000)]        // 0đ → tính phí
    [InlineData(499_999, 15_000)]  // Dưới 500k → tính phí
    [InlineData(500_000, 0)]       // Đúng 500k → miễn phí
    [InlineData(500_001, 0)]       // Trên 500k → miễn phí
    [InlineData(10_000_000, 0)]    // 10 triệu → miễn phí
    public void Standard_FeeThreshold_WithMultipleAmounts(
        decimal amount, decimal expectedFee)
    {
        var fee = _service.CalculateFee(ShippingMethod.Standard, amount);
        Assert.Equal(expectedFee, fee);
    }
}
