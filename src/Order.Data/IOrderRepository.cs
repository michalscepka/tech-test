using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Returns summaries for orders whose status name equals the supplied value.
        /// </summary>
        /// <param name="status">Status name filter.</param>
        /// <returns>Ordered list of matching order summaries.</returns>
        Task<IEnumerable<OrderSummary>> GetByStatusAsync(string status);
    }
}
