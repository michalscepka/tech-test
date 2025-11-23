using System;
using System.ComponentModel;

namespace Order.WebAPI.Dtos.Requests;

/// <summary>
/// Request payload for updating the status of an order.
/// </summary>
[Description("Request payload for updating the status of an order")]
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Unique identifier of the order to update.
    /// </summary>
    [Description("Unique identifier of the order to update")]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Target status value that will be applied to the order.
    /// </summary>
    [Description("Target status value that will be applied to the order")]
    public string Status { get; set; }
}
