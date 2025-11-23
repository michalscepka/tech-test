namespace Order.WebAPI.Dtos.Requests;

public class FilterOrderRequest
{
    /// <summary>
    /// Status name filter used when retrieving orders.
    /// </summary>
    public string Status { get; set; }
}
