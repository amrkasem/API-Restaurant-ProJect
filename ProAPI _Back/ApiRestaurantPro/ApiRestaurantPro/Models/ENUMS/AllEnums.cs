namespace ApiRestaurantPro.Models.ENUMS
{
    //11 All Enums

    public enum OrderStatus
    {
        Pending = 1,
        Preparing = 2,
        Ready = 3,
        Delivered = 4,
        Canceled = 5
    }
    public enum OrderType
    {
        DineIn = 1,
        Takeaway = 2,
        Delivery = 3
    }
    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        DebitCard = 3,
        OnlinePayment = 4
    }

    public enum UserRole
    {
        Guest = 1,
        Subscriber = 2,
        Customer = 3,
        Admin = 4
    }
}
