namespace Shop.Business.Enums;

// Phuong thuc van chuyen. "International" la loai MOI duoc mo rong sau nay
// -> dung de minh hoa OCP: them loai moi ma khong sua code cu.
public enum ShippingMethod
{
    Standard = 1,       // Giao thuong
    Express = 2,        // Giao nhanh
    SameDay = 3,        // Giao trong ngay
    International = 4    // Giao quoc te (loai mo rong)
}
