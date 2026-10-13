using Shop.Business.DTOs;
using Shop.Business.Entities;
using Shop.Business.Enums;
using Shop.Business.Interfaces;

namespace Shop.Business.Legacy;

/// <summary>
/// ❌ CLASS VI PHAM SRP - DANH CHO DEMO.
/// Mot class om dom toi 5 trach nhiem khac nhau:
/// 1. Validation quy tac dat hang
/// 2. Business logic: tinh phi ship bang switch-case
/// 3. Persistence: truc tiep luu don + tru ton kho
/// 4. Formatting: tu dinh dang van ban hoa don
/// 5. Logging: tu ghi log he thong
/// -> Co 5 LY DO de thay doi class nay (vi pham SRP nghiem trong)!
/// </summary>
public class BadOrderManager
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public BadOrderManager(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<object> PlaceOrderBadSRPAsync(PlaceOrderRequestDto request)
    {
        // TRACH NHIEM 1: Validation
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("[SRP Violation] Ten khach hang khong duoc de trong.");
        if (request.Quantity <= 0)
            throw new ArgumentException("[SRP Violation] So luong phai lon hon 0.");

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null)
            throw new KeyNotFoundException($"[SRP Violation] Khong tim thay san pham Id = {request.ProductId}");
        if (product.Stock < request.Quantity)
            throw new InvalidOperationException($"[SRP Violation] San pham '{product.Name}' khong du ton kho.");

        var itemsAmount = product.Price * request.Quantity;

        // TRACH NHIEM 2: Business logic - tu tinh phi ship bang switch-case
        decimal shippingFee;
        switch (request.ShippingMethod)
        {
            case ShippingMethod.Standard: shippingFee = itemsAmount >= 500_000m ? 0m : 15_000m; break;
            case ShippingMethod.Express: shippingFee = 30_000m; break;
            case ShippingMethod.SameDay: shippingFee = 50_000m; break;
            case ShippingMethod.International: shippingFee = 200_000m; break;
            default: throw new InvalidOperationException("[SRP Violation] Phuong thuc van chuyen khong ho tro.");
        }

        var total = itemsAmount + shippingFee;

        // TRACH NHIEM 3: Persistence - truc tiep luu don + tru ton kho
        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            ShippingMethod = request.ShippingMethod,
            ItemsAmount = itemsAmount,
            ShippingFee = shippingFee,
            Total = total,
            Status = "Confirmed",
            CreatedAt = DateTime.Now
        };
        await _orderRepository.AddAsync(order);
        product.Stock -= request.Quantity;
        await _productRepository.UpdateAsync(product);

        // TRACH NHIEM 4: Formatting - tu dinh dang hoa don
        string receiptText =
            "=== HOA DON DAT HANG (VI PHAM SRP) ===\n" +
            $"Don hang: #{order.Id}\n" +
            $"Khach hang: {order.CustomerName}\n" +
            $"San pham: {order.ProductName} x{order.Quantity}\n" +
            $"Tien hang: {order.ItemsAmount:N0} VND\n" +
            $"Phi van chuyen ({order.ShippingMethod}): {order.ShippingFee:N0} VND\n" +
            $"Tong cong: {order.Total:N0} VND\n" +
            "======================================";

        // TRACH NHIEM 5: Logging - tu ghi log he thong
        Console.WriteLine($"[LOG BAD SRP] Tao don #{order.Id} luc {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(receiptText);

        return new
        {
            OrderId = order.Id,
            order.CustomerName,
            order.ProductName,
            order.Quantity,
            order.ShippingFee,
            order.Total,
            ReceiptText = receiptText,
            Method = "BadOrderManager (VI PHAM SRP)",
            SrpViolationReason = "1 class om 5 trach nhiem: Validation + Switch-case + Persistence + Formatting + Logging"
        };
    }
}
