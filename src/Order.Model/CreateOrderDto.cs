using System;
using System.Collections.Generic;

namespace Order.Model;

/// <summary>
/// Request model for creating a new order.
/// </summary>
public class CreateOrderDto
{
    public Guid ResellerId { get; set; }

    public Guid CustomerId { get; set; }

    public IEnumerable<CreateOrderItemDto> Items { get; set; }
}
