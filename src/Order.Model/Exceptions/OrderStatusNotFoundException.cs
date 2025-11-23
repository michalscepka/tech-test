using System;

namespace Order.Model.Exceptions;

public class OrderStatusNotFoundException : Exception
{
    public OrderStatusNotFoundException(string status)
        : base($"Status '{status}' not found")
    {
    }
}
