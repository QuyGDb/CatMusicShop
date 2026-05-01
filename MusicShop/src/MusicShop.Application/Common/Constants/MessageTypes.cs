namespace MusicShop.Application.Common.Constants;

public static class MessageTypes
{
    public static class Orders
    {
        public const string Created   = "orders.order.created";
        public const string Cancelled = "orders.order.cancelled";
    }

    public static class Payments
    {
        public const string Processed = "payments.payment.processed";
        public const string Refunded  = "payments.payment.refunded";
    }

    public static class Stripe
    {
        public const string PaymentSucceeded = "stripe.payment_intent.succeeded";
        public const string RefundCreated    = "stripe.charge.refunded";
    }
}
