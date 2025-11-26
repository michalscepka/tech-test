using System;

namespace Order.Model;

/// <summary>
/// Request model for an order item within a create order request.
/// </summary>
public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }

    public Guid ServiceId { get; set; }

    public int Quantity { get; set; }
}
