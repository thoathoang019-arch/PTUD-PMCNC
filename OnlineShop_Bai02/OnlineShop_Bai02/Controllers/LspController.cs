using Microsoft.AspNetCore.Mvc;
using ShopMini.Lsp;

namespace ShopMini.Controllers;

[ApiController]
[Route("api/lsp")]
public class LspController : ControllerBase
{
    // ❌ Ap dung voucher Flash Sale 100k cho don 20k -> Tinh ra am 80k. 
    // API khong sap nhung cong ty mat tien -> "Silent failure" vi pham Post-condition (Dieu kien dau ra).
    [HttpGet("violation")]
    public IActionResult Violation([FromQuery] decimal orderAmount = 20000)
    {
        var voucher = new FlashSaleVoucherBad("SALE100K", 100000); // Giam thang 100k
        var finalAmount = voucher.Apply(orderAmount);
        
        return Ok(new 
        { 
            orderAmount, 
            finalAmount,
            note = "Lop con FlashSaleVoucherBad pha vo cam ket (tra ve tien am). Chuong trinh KHONG BAO LOI (Silent failure) nhung logic sai hoan toan. Day moi la su nguy hiem cua LSP!" 
        });
    }

    // ✅ Cung tinh huong (Don 20k, ma 100k): Dung FixedVoucher co implement Math.Min -> tra ve tien = 0.
    [HttpGet("solution")]
    public IActionResult Solution([FromQuery] decimal orderAmount = 20000)
    {
        IVoucherStrategy voucher = new FixedVoucher("SALE100K", 100000);
        var result = voucher.Apply(orderAmount);              
        
        return Ok(new
        {
            orderAmount,
            result,
            note = "FixedVoucher kiem soat tien am bang Math.Min(gia tri, don hang). Tra ve Result on dinh, khong bao gio am. TUAN THU LSP!"
        });
    }
}
