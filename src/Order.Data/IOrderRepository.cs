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

        /// <summary>
        /// Updates an existing order with the provided status name.
        /// </summary>
        /// <param name="orderId">Order identifier to update.</param>
        /// <param name="status">Status name to apply to the order.</param>
        /// <exception cref="Order.Model.Exceptions.OrderNotFoundException">Thrown when the target order cannot be found.</exception>
        /// <exception cref="Order.Model.Exceptions.OrderStatusNotFoundException">Thrown when the supplied status does not exist.</exception>
        Task UpdateStatusAsync(Guid orderId, string status);
    }
}
