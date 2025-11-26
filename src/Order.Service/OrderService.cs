using Order.Data;
using Order.Model;
using Order.Model.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required", nameof(status));

            return await _orderRepository.GetByStatusAsync(status);
        }

        public async Task<Result> UpdateStatusAsync(Guid orderId, string status)
        {
            if (orderId == Guid.Empty)
                return Result.Failure("Order id is required");

            if (string.IsNullOrWhiteSpace(status))
                return Result.Failure("Status is required");

            try
            {
                await _orderRepository.UpdateStatusAsync(orderId, status);
                return Result.Success();
            }
            catch (OrderStatusNotFoundException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (OrderNotFoundException ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public async Task<Result<OrderDetail>> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            if (createOrderDto.ResellerId == Guid.Empty)
                return Result<OrderDetail>.Failure("ResellerId id is required");

            if (createOrderDto.CustomerId == Guid.Empty)
                return Result<OrderDetail>.Failure("CustomerId id is required");

            var itemsList = createOrderDto.Items?.ToList();
            if (itemsList is null || itemsList.Count is 0)
                return Result<OrderDetail>.Failure("At least one order item is required");

            try
            {
                var order = await _orderRepository.CreateOrderAsync(createOrderDto);
                return Result<OrderDetail>.Success(order);
            }
            catch (ProductNotFoundException ex)
            {
                return Result<OrderDetail>.Failure(ex.Message);
            }
            catch (ServiceNotFoundException ex)
            {
                return Result<OrderDetail>.Failure(ex.Message);
            }
        }

        public async Task<Result<IEnumerable<MonthlyProfit>>> GetMonthlyProfitAsync() =>
            Result<IEnumerable<MonthlyProfit>>.Success(await _orderRepository.GetMonthlyProfitAsync());
    }
}
