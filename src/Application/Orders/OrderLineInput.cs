namespace Application.Orders;

public sealed record OrderLineInput(long ProductId, int Quantity);
