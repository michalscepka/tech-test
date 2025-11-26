using Microsoft.EntityFrameworkCore;
using Order.Model;
using Order.Model.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetByStatusAsync(string status)
        {
            var trimmedStatus = status.Trim();

            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Where(x => x.Status.Name == trimmedStatus)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task UpdateStatusAsync(Guid orderId, string status)
        {
            var trimmedStatus = status.Trim();
            var statusEntity = await _orderContext.OrderStatus.SingleOrDefaultAsync(x => x.Name == trimmedStatus)
                ?? throw new OrderStatusNotFoundException(trimmedStatus);

            var orderIdBytes = orderId.ToByteArray();
            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory()
                        ? x.Id.SequenceEqual(orderIdBytes)
                        : x.Id == orderIdBytes)
                .SingleOrDefaultAsync()
                ?? throw new OrderNotFoundException(orderId);

            order.StatusId = statusEntity.Id;
            await _orderContext.SaveChangesAsync();
        }

        public async Task<OrderDetail> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var orderId = Guid.NewGuid();
            var orderIdBytes = orderId.ToByteArray();

            var createdStatus = await _orderContext.OrderStatus.FirstOrDefaultAsync(x => x.Name == "Created");

            var missingProducts = createOrderDto.Items
                .Select(x => x.ProductId)
                .Distinct()
                .Where(pid => !_orderContext.OrderProduct.Any(p =>
                    _orderContext.IsInMemoryDatabase()
                        ? p.Id.SequenceEqual(pid.ToByteArray())
                        : p.Id == pid.ToByteArray()))
                .ToList();

            if (missingProducts.Count != 0)
                throw new ProductNotFoundException(missingProducts);

            var missingServices = createOrderDto.Items
                .Select(x => x.ServiceId)
                .Distinct()
                .Where(sid => !_orderContext.OrderService.Any(s =>
                    _orderContext.IsInMemoryDatabase()
                        ? s.Id.SequenceEqual(sid.ToByteArray())
                        : s.Id == sid.ToByteArray()))
                .ToList();

            if (missingServices.Count != 0)
                throw new ServiceNotFoundException(missingServices);

            var order = new Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = createOrderDto.ResellerId.ToByteArray(),
                CustomerId = createOrderDto.CustomerId.ToByteArray(),
                StatusId = createdStatus.Id,
                CreatedDate = DateTime.UtcNow
            };

            _orderContext.Order.Add(order);

            foreach (var item in createOrderDto.Items)
            {
                var orderItem = new Entities.OrderItem
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    OrderId = orderIdBytes,
                    ProductId = item.ProductId.ToByteArray(),
                    ServiceId = item.ServiceId.ToByteArray(),
                    Quantity = item.Quantity
                };

                _orderContext.OrderItem.Add(orderItem);
            }

            await _orderContext.SaveChangesAsync();

            return await GetOrderByIdAsync(orderId);
        }

        public async Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitAsync()
        {
            var completedOrders = await _orderContext.Order
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Status)
                .Where(x => x.Status.Name == "Completed")
                .ToListAsync();

            var monthlyProfits = completedOrders
                .GroupBy(o => new { o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new MonthlyProfit
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalCost = g.Sum(o => o.Items.Sum(i => i.Quantity * i.Product.UnitCost) ?? 0),
                    TotalPrice = g.Sum(o => o.Items.Sum(i => i.Quantity * i.Product.UnitPrice) ?? 0),
                    Profit = g.Sum(o => o.Items.Sum(i => i.Quantity * (i.Product.UnitPrice - i.Product.UnitCost)) ?? 0),
                    OrderCount = g.Count()
                })
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToList();

            return monthlyProfits;
        }
    }
}
