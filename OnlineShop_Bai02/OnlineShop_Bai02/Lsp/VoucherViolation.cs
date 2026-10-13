namespace ShopMini.Lsp;

// ❌ VI PHAM LSP — Nghiep vu: ap ma giam gia (Voucher) len tien don hang.
// Class cha dinh nghia "hop dong" (Post-condition): Apply() LUON tra ve so tien phai tra sau giam, va KHONG BAO GIO AM.
public abstract class Voucher
{
    public string Code { get; }
    protected Voucher(string code) => Code = code;
    public abstract decimal Apply(decimal orderAmount);   // hop dong: tra ve >= 0
}

public class PercentVoucherBad : Voucher
{
    private readonly decimal _percent;
    public PercentVoucherBad(string code, decimal percent) : base(code) => _percent = percent;
    public override decimal Apply(decimal orderAmount) => orderAmount * (1 - _percent / 100m);
}

// Class con PHA hop dong (Post-condition): Khong them check tien am.
// -> Chuong trinh KHONG SAP, van chay tiep nhung tinh ra tien am -> "Chet tham lang", cong ty mat tien!
public class FlashSaleVoucherBad : Voucher
{
    private readonly decimal _value;
    public FlashSaleVoucherBad(string code, decimal value) : base(code) => _value = value;
    
    public override decimal Apply(decimal orderAmount)
    {
        // Loi logic: Khong dung Math.Max(0, ...), dan den viec tra ve so tien am 
        // neu don hang nho hon gia tri Voucher.
        return orderAmount - _value;   
    }
}
