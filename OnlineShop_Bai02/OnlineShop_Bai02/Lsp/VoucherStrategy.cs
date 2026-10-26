namespace ShopMini.Lsp;

// ✅ AP DUNG LSP — Ket qua ap voucher: co trang thai thanh cong/that bai.
public record VoucherResult(bool Success, string Message, decimal Discount, decimal FinalAmount);

// Hop dong moi: Apply() LUON tra ve VoucherResult (khong bao gio nem Exception vi nghiep vu).
public interface IVoucherStrategy
{
    string Code { get; }
    VoucherResult Apply(decimal orderAmount);
}

public class PercentVoucher : IVoucherStrategy
{
    private readonly decimal _percent;
    public string Code { get; }
    public PercentVoucher(string code, decimal percent) { Code = code; _percent = percent; }
    public VoucherResult Apply(decimal amount)
    {
        var discount = amount * _percent / 100m;
        return new VoucherResult(true, $"Giam {_percent}%", discount, amount - discount);
    }
}

public class FixedVoucher : IVoucherStrategy
{
    private readonly decimal _value;
    public string Code { get; }
    public FixedVoucher(string code, decimal value) { Code = code; _value = value; }
    public VoucherResult Apply(decimal amount)
    {
        var discount = Math.Min(_value, amount);
        return new VoucherResult(true, $"Giam {_value:N0}d", discount, amount - discount);
    }
}

// Class con TON TRONG hop dong: het luot dung -> tra VoucherResult(Success=false),
// KHONG nem Exception -> API van chay muot -> thay the duoc nhau (tuan thu LSP).
public class OneTimeVoucher : IVoucherStrategy
{
    private bool _used;
    public string Code { get; }
    public OneTimeVoucher(string code) => Code = code;
    public VoucherResult Apply(decimal amount)
    {
        if (_used) return new VoucherResult(false, $"Voucher {Code} da het luot su dung", 0, amount);
        _used = true;
        var discount = amount * 0.1m;
        return new VoucherResult(true, "Giam 10% (dung 1 lan)", discount, amount - discount);
    }
}
