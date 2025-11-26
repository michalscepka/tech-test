using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Order.WebAPI.Dtos.Requests;

/// <summary>
/// Request payload for creating a new order.
/// </summary>
[Description("Request payload for creating a new order")]
public class CreateOrderRequest
{
    /// <summary>
    /// Unique identifier of the reseller.
    /// </summary>
    [Description("Unique identifier of the reseller")]
    public Guid ResellerId { get; set; }

    /// <summary>
    /// Unique identifier of the customer.
    /// </summary>
    [Description("Unique identifier of the customer")]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// List of items to include in the order.
    /// </summary>
    [Description("List of items to include in the order")]
    public List<CreateOrderItemRequest> Products { get; set; }
}

/// <summary>
/// Request payload for an order item.
/// </summary>
[Description("Request payload for an order item")]
public class CreateOrderItemRequest
{
    /// <summary>
    /// Unique identifier of the product.
    /// </summary>
    [Description("Unique identifier of the product")]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Unique identifier of the service.
    /// </summary>
    [Description("Unique identifier of the service")]
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Quantity of the product.
    /// </summary>
    [Description("Quantity of the product")]
    public int Quantity { get; set; }
}

