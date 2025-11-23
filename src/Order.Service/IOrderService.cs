using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Retrieves the collection of orders whose status name matches the supplied filter.
        /// </summary>
        /// <param name="status">Status name to match, e.g. "Failed".</param>
        /// <returns>Order summaries ordered by creation date for the requested status.</returns>
        Task<IEnumerable<OrderSummary>> GetByStatusAsync(string status);

        /// <summary>
        /// Updates the status of an order to the supplied status name.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="status">Target status name.</param>
        /// <returns>Result payload describing success or the reason the update could not be applied.</returns>
        Task<Result> UpdateStatusAsync(Guid orderId, string status);
    }
}
