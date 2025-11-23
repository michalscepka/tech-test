using System;

namespace Order.Model.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order '{orderId}' not found")
    {
    }
}
