namespace E_commerce.Common;

// Loyalty program constants (Phase 6). Centralizes business rules for points calculations
// to allow easy adjustments to earn/redeem rates without modifying core service logic.
public static class LoyaltyConstants
{
    // Earning policy: Customers earn 1 point for every EarnRateAmount spent (after applying discounts).
    // Integer floor calculation is applied (e.g. $95 spent yields 9 points).
    public const decimal EarnRateAmount = 10m; // $10 spent = 1 point earned

    // Redemption policy: Each point redeemed grants RedeemPointValue as a discount during checkout.
    // Setting 1 point = 0.10 currency units creates a sustainable 10:1 point-to-value business model.
    public const decimal RedeemPointValue = 0.10m; // 1 point = $0.10 discount
}