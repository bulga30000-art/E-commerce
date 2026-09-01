namespace E_commerce.Common;

// Payment status string constants (Default in database configuration is "Pending").
// Validated and controlled within PaymentService operations.
public static class PaymentStatuses
{
    public const string Pending = "Pending";     // Cash payment awaiting admin confirmation
    public const string Completed = "Completed"; // Instant for CreditCard, or post-confirmation for Cash
    public const string Failed = "Failed";       // Mark as uncollectible or failed transaction
}