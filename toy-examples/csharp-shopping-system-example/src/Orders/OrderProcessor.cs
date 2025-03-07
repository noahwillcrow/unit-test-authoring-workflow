namespace ShoppingSystem.Orders;

public class OrderProcessor
{
    private readonly IInventoryService _InventoryService;
    private readonly INotificationService _NotificationService;
    private readonly IPaymentGateway _PaymentGateway;

    public OrderProcessor(IInventoryService inventoryService, INotificationService notificationService, IPaymentGateway paymentGateway)
    {
        _InventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _NotificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _PaymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
    }

    public async Task<bool> ProcessOrderAsync(Order order, string accountId, CancellationToken cancellationToken)
    {
        if (order == null || string.IsNullOrWhiteSpace(accountId))
        {
            return false; // Invalid order or account
        }

        // Check inventory
        var isInStock = await _InventoryService.IsInStockAsync(order.ProductId, order.Quantity, cancellationToken);
        if (!isInStock)
        {
            return false; // Item out of stock
        }

        // Attempt payment
        var isPaymentProcessingSuccessful = await _PaymentGateway.ProcessPaymentAsync(accountId, order.Price, cancellationToken);
        if (!isPaymentProcessingSuccessful)
        {
            return false; // Payment failed
        }

        // Reserve stock
        await _InventoryService.ReserveStockAsync(order.ProductId, order.Quantity, cancellationToken);

        // Send confirmation
        await _NotificationService.SendOrderConfirmationAsync(order.UserId, order.OrderId, cancellationToken);

        return true; // Order processed successfully
    }
}
