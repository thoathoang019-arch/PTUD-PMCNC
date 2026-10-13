using Microsoft.AspNetCore.Mvc;
using ShopMini.Isp;

namespace ShopMini.Controllers;

[ApiController]
[Route("api/isp")]
public class IspController : ControllerBase
{
    // ❌ Customer service bi ep implement ham cua Admin -> nem NotSupportedException.
    [HttpGet("violation")]
    public IActionResult Violation()
    {
        var svc = new CustomerOrderServiceBad();
        var myOrders = svc.GetMyOrders();
        string deleteError;
        try { svc.DeleteOrder(1); deleteError = "(khong loi?)"; }
        catch (NotSupportedException ex) { deleteError = ex.Message; }
        return Ok(new
        {
            myOrders,
            deleteError,
            note = "Fat interface IOrderServiceFat ep Customer implement DeleteOrder/UpdateStatus (cua Admin) -> nem NotSupportedException. VI PHAM ISP."
        });
    }

    // ✅ Tach interface: Khach dung IOrderReadService, Admin dung IOrderManageService (inject qua DI).
    [HttpGet("solution")]
    public IActionResult Solution([FromServices] IOrderReadService read,
                                  [FromServices] IOrderManageService manage)
    {
        var customerView = read.GetMyOrders();               // khach chi doc
        var adminUpdated = manage.UpdateStatus(1, "Delivered"); // admin doi trang thai
        return Ok(new
        {
            customerView,
            adminUpdated,
            note = "IOrderReadService (khach) tach khoi IOrderManageService (admin) -> moi ben chi phu thuoc thu can. TUAN THU ISP."
        });
    }
}
