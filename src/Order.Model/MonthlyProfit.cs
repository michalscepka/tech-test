namespace Order.Model;

/// <summary>
/// Represents profit calculation for a specific month.
/// </summary>
public class MonthlyProfit
{
    /// <summary>
    /// Year of the calculation.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Month of the calculation (1-12).
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Total cost for all completed orders in this month.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Total price for all completed orders in this month.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Calculated profit (TotalPrice - TotalCost).
    /// </summary>
    public decimal Profit { get; set; }

    /// <summary>
    /// Number of completed orders in this month.
    /// </summary>
    public int OrderCount { get; set; }
}
