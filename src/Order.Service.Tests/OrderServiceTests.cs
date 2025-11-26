using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private OrderContext _orderContext;
        private DbConnection _connection;

        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderServiceEmailId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderProductEmailId = Guid.NewGuid().ToByteArray();


        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection;

            _orderContext = new OrderContext(options);
            _orderContext.Database.EnsureDeleted();
            _orderContext.Database.EnsureCreated();

            _orderRepository = new OrderRepository(_orderContext);
            _orderService = new OrderService(_orderRepository);

            await AddReferenceDataAsync(_orderContext);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            _orderContext.Dispose();
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }

        [Test]
        public async Task GetByStatusAsync_WithValidStatus_ReturnsSuccessWithOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            // Act
            var result = await _orderService.GetByStatusAsync("Created");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Count());
            Assert.IsTrue(result.Value.Any(o => o.Id == orderId1));
            Assert.IsTrue(result.Value.Any(o => o.Id == orderId2));
        }

        [Test]
        public async Task GetByStatusAsync_WithNonExistingStatus_ReturnsSuccessWithEmptyCollection()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var result = await _orderService.GetByStatusAsync("NonExistingStatus");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(0, result.Value.Count());
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public async Task GetByStatusAsync_WithInvalidStatus_ReturnsFailure(string invalidStatus)
        {
            // Act
            var result = await _orderService.GetByStatusAsync(invalidStatus);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Status is required", result.Error);
            Assert.IsNull(result.Value);
        }

        [Test]
        public async Task GetByStatusAsync_WhenNoOrders_ReturnsSuccessWithEmptyCollection()
        {
            // Act
            var result = await _orderService.GetByStatusAsync("Created");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(0, result.Value.Count());
        }

        [Test]
        public async Task UpdateStatusAsync_WithValidParameters_ReturnsSuccess()
        {
            // Arrange
            var completedStatusId = Guid.NewGuid().ToByteArray();
            _orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = completedStatusId,
                Name = "Completed",
            });
            await _orderContext.SaveChangesAsync();

            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);

            // Act
            var result = await _orderService.UpdateStatusAsync(orderId, "Completed");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);

            var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);
            Assert.AreEqual("Completed", updatedOrder.StatusName);
        }

        [Test]
        public async Task UpdateStatusAsync_WithEmptyGuidOrderId_ReturnsFailure()
        {
            // Act
            var result = await _orderService.UpdateStatusAsync(Guid.Empty, "Completed");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Order id is required", result.Error);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public async Task UpdateStatusAsync_WithInvalidStatus_ReturnsFailure(string invalidStatus)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);

            // Act
            var result = await _orderService.UpdateStatusAsync(orderId, invalidStatus);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Status is required", result.Error);
        }

        [Test]
        public async Task UpdateStatusAsync_WithNonExistentOrder_ReturnsFailure()
        {
            // Arrange
            var nonExistentOrderId = Guid.NewGuid();

            // Act
            var result = await _orderService.UpdateStatusAsync(nonExistentOrderId, "Completed");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error.Contains("not found"));
        }

        [Test]
        public async Task UpdateStatusAsync_WithNonExistentStatus_ReturnsFailure()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);

            // Act
            var result = await _orderService.UpdateStatusAsync(orderId, "NonExistentStatus");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error.Contains("not found"));
        }

        [Test]
        public async Task CreateOrderAsync_WithValidData_ReturnsSuccessWithOrderDetail()
        {
            // Arrange
            var resellerId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = resellerId,
                CustomerId = customerId,
                Items = new List<CreateOrderItemDto>
                {
                    new()
                    {
                        ProductId = new Guid(_orderProductEmailId),
                        ServiceId = new Guid(_orderServiceEmailId),
                        Quantity = 2
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(resellerId, result.Value.ResellerId);
            Assert.AreEqual(customerId, result.Value.CustomerId);
            Assert.AreEqual(1, result.Value.Items.Count());
        }

        [Test]
        public async Task CreateOrderAsync_WithEmptyResellerId_ReturnsFailure()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.Empty,
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemDto>
                {
                    new()
                    {
                        ProductId = new Guid(_orderProductEmailId),
                        ServiceId = new Guid(_orderServiceEmailId),
                        Quantity = 1
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("ResellerId id is required", result.Error);
            Assert.IsNull(result.Value);
        }

        [Test]
        public async Task CreateOrderAsync_WithEmptyCustomerId_ReturnsFailure()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.Empty,
                Items = new List<CreateOrderItemDto>
                {
                    new()
                    {
                        ProductId = new Guid(_orderProductEmailId),
                        ServiceId = new Guid(_orderServiceEmailId),
                        Quantity = 1
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("CustomerId id is required", result.Error);
            Assert.IsNull(result.Value);
        }

        [Test]
        public async Task CreateOrderAsync_WithNullItems_ReturnsFailure()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = null
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("At least one order item is required", result.Error);
            Assert.IsNull(result.Value);
        }

        [Test]
        public async Task CreateOrderAsync_WithEmptyItems_ReturnsFailure()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemDto>()
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("At least one order item is required", result.Error);
            Assert.IsNull(result.Value);
        }

        [Test]
        public async Task CreateOrderAsync_WithMultipleItems_ReturnsSuccessWithAllItems()
        {
            // Arrange
            var product2Id = Guid.NewGuid().ToByteArray();
            _orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = product2Id,
                Name = "200GB Mailbox",
                UnitCost = 1.5m,
                UnitPrice = 2.0m,
                ServiceId = _orderServiceEmailId
            });
            await _orderContext.SaveChangesAsync();

            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemDto>
                {
                    new()
                    {
                        ProductId = new Guid(_orderProductEmailId),
                        ServiceId = new Guid(_orderServiceEmailId),
                        Quantity = 2
                    },
                    new()
                    {
                        ProductId = new Guid(product2Id),
                        ServiceId = new Guid(_orderServiceEmailId),
                        Quantity = 3
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(2, result.Value.Items.Count());
        }

        [Test]
        public async Task CreateOrderAsync_WithNonExistentProduct_ReturnsFailure()
        {
            // Arrange
            var nonExistentProductId = Guid.NewGuid();
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemDto>
                {
                    new()
                    {
                        ProductId = nonExistentProductId,
                        ServiceId = new Guid(_orderServiceEmailId),
                        Quantity = 1
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error.Contains("not found"));
            Assert.IsTrue(result.Error.Contains(nonExistentProductId.ToString()));
            Assert.IsNull(result.Value);
        }

        [Test]
        public async Task CreateOrderAsync_WithNonExistentService_ReturnsFailure()
        {
            // Arrange
            var nonExistentServiceId = Guid.NewGuid();
            var createOrderDto = new CreateOrderDto
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemDto>
                {
                    new()
                    {
                        ProductId = new Guid(_orderProductEmailId),
                        ServiceId = nonExistentServiceId,
                        Quantity = 1
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(createOrderDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error.Contains("not found"));
            Assert.IsTrue(result.Error.Contains(nonExistentServiceId.ToString()));
            Assert.IsNull(result.Value);
        }

        private async Task AddOrder(Guid orderId, int quantity)
        {
            var orderIdBytes = orderId.ToByteArray();
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = DateTime.Now,
                StatusId = _orderStatusCreatedId,
            });

            _orderContext.OrderItem.Add(new Data.Entities.OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = "Created",
            });

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
