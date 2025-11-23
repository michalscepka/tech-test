using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Service;
using Order.WebAPI.Dtos;
using Order.WebAPI.Dtos.Requests;

namespace Order.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Retrieves orders whose status matches the supplied query filter.
        /// </summary>
        /// <param name="request">Query containing the status to filter by.</param>
        /// <returns>HTTP 200 with the filtered orders.</returns>
        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FilterOrder([FromQuery] FilterOrderRequest request)
        {
            var orders = await _orderService.GetByStatusAsync(request.Status);
            return Ok(orders);
        }

        /// <summary>
        /// Updates the status of the specified order.
        /// </summary>
        /// <param name="request">Payload containing the order id and desired status.</param>
        /// <returns>HTTP 204 when updated successfully; HTTP 404 with error details otherwise.</returns>
        [HttpPatch("status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            var result = await _orderService.UpdateStatusAsync(request.OrderId, request.Status);

            return result.IsSuccess
                ? NoContent()
                : NotFound(new ErrorResponse {Message = result.Error});
        }
    }
}
