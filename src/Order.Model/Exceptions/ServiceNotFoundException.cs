using System;
using System.Collections.Generic;

namespace Order.Model.Exceptions;

public class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException(IEnumerable<Guid> serviceIds)
        : base($"Services '{string.Join(",", serviceIds)}' not found")
    {
    }
}
