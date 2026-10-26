using Shop.Business.DTOs;

namespace Shop.Business.Interfaces;

public interface IOrderAppService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> PlaceOrderAsync(PlaceOrderRequestDto request);   // Chuan SRP
}
