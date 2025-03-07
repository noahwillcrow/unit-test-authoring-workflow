namespace ShoppingSystem.Orders;

public class Order
{
    public string OrderId { get; }
    public string UserId { get; }
    public string ProductId { get; }
    public int Quantity { get; }
    public decimal Price { get; }

    public Order(string orderId, string userId, string productId, int quantity, decimal price)
    {
        OrderId = orderId;
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
}

public interface IInventoryService
{
    Task<bool> IsInStockAsync(string productId, int quantity, CancellationToken cancellationToken);
    Task ReserveStockAsync(string productId, int quantity, CancellationToken cancellationToken);
}

public interface INotificationService
{
    Task SendOrderConfirmationAsync(string userId, string orderId, CancellationToken cancellationToken);
}

public interface IPaymentGateway
{
    Task<bool> ProcessPaymentAsync(string accountId, decimal amount, CancellationToken cancellationToken);
}
