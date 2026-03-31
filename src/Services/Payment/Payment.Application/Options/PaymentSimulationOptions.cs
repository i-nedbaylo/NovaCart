namespace NovaCart.Services.Payment.Application.Options;

/// <summary>
/// Options for the simulated payment processing.
/// NOTE: Simplified for demo purposes. In production, replace with a real payment gateway integration.
/// </summary>
public sealed class PaymentSimulationOptions
{
    /// <summary>
    /// Delay to simulate payment processing time.
    /// </summary>
    public TimeSpan ProcessingDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Success rate percentage (0–100). Default: 80%.
    /// </summary>
    public int SuccessRatePercent { get; set; } = 80;
}
