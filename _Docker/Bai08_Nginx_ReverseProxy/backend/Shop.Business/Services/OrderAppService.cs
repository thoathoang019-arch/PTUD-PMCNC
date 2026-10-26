using Shop.Business.DTOs;
using Shop.Business.Entities;
using Shop.Business.Interfaces;

namespace Shop.Business.Services;

// ✅ AP DUNG SRP - Service nay chi DIEU PHOI nghiep vu dat hang.
// No UY THAC: luu tru cho Repository, tinh phi ship cho ShippingFeeService,
// gui mail cho EmailService. Moi lop mot ly do de thay doi.
public class OrderAppService : IOrderAppService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShippingFeeService _shippingFeeService;
    private readonly IEmailService _emailService;

    public OrderAppService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IShippingFeeService shippingFeeService,
        IEmailService emailService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _shippingFeeService = shippingFeeService;
        _emailService = emailService;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(ToDto);
    }

    public async Task<OrderDto> PlaceOrderAsync(PlaceOrderRequestDto request)
    {
        // Validate co ban
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("Ten khach hang khong duoc de trong.");
        if (request.Quantity <= 0)
            throw new ArgumentException("So luong phai lon hon 0.");

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null)
            throw new KeyNotFoundException($"Khong tim thay san pham co Id = {request.ProductId}");

        if (product.Stock < request.Quantity)
            throw new InvalidOperationException(
                $"San pham '{product.Name}' khong du ton kho (con {product.Stock}, can {request.Quantity}).");

        var itemsAmount = product.Price * request.Quantity;

        // Tinh phi ship qua Strategy (OCP)
        var shippingFee = _shippingFeeService.CalculateFee(request.ShippingMethod, itemsAmount);

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            ShippingMethod = request.ShippingMethod,
            ItemsAmount = itemsAmount,
            ShippingFee = shippingFee,
            Total = itemsAmount + shippingFee,
            Status = "Confirmed",
            CreatedAt = DateTime.Now
        };

        // Uy thac luu tru + cap nhat ton kho
        await _orderRepository.AddAsync(order);
        product.Stock -= request.Quantity;
        await _productRepository.UpdateAsync(product);

        // Uy thac gui email
        _emailService.SendOrderConfirmation(request.Email, order);

        return ToDto(order);
    }

    private static OrderDto ToDto(Order o) => new()
    {
        Id = o.Id,
        CustomerName = o.CustomerName,
        ProductId = o.ProductId,
        ProductName = o.ProductName,
        Quantity = o.Quantity,
        ShippingMethod = o.ShippingMethod.ToString(),
        ItemsAmount = o.ItemsAmount,
        ShippingFee = o.ShippingFee,
        Total = o.Total,
        Status = o.Status,
        CreatedAt = o.CreatedAt
    };
}
