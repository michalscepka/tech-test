using System;
using System.Collections.Generic;

namespace Order.Model.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(IEnumerable<Guid> productIds)
        : base($"Products '{string.Join(",", productIds)}' not found")
    {
    }
}
