namespace E_commerce.Common;

// Supported payment method string constants. Enforced at the service layer
// to standardize allowed values across API request payloads.
public static class PaymentMethods
{
    public const string CreditCard = "CreditCard";
    public const string Cash = "Cash";
}