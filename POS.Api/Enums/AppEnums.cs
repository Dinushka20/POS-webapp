namespace POS.Api.Enums
{
    public enum UserRole
    {
        Admin,
        Manager,
        Cashier
    }

    public enum PaymentMode
    {
        Cash,
        Card,
        Split
    }

    public enum StockAdjustReason
    {
        Purchase,
        Return,
        Damage,
        Adjustment
    }

    public enum CustomerTier
    {
        Silver,
        Gold,
        Platinum
    }

    public enum OrderStatus
    {
        Completed,
        Refunded,
        Voided
    }
}
