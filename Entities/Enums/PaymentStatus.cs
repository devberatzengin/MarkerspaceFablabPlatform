namespace MakerspaceFablabPlatform.Entities.Enums;

public enum PaymentStatus
{
    Pending,    // Waiting for payment
    Paid,       // Successfully paid
    Failed,     // Payment failed
    Refunded,   // Refunded to user
    Cancelled   // Cancelled/Void

}