using System.ComponentModel;

namespace Order.WebAPI.Dtos;

/// <summary>
/// Response DTO for error information
/// </summary>
[Description("Standard error response for API errors")]
public class ErrorResponse
{
    /// <summary>
    /// The main error message
    /// </summary>
    [Description("The main error message")]
    public string Message { get; init; }
}
