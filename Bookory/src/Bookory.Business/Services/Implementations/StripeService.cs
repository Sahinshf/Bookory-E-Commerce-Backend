﻿using Bookory.Business.Services.Interfaces;
using Stripe;

namespace Bookory.Business.Services.Implementations;

public class StripeService : IStripeService
{
    public async Task<string> ChargeAsync(string stripeEmail, string stripeToken, decimal totalAmount)
    {
        var service = new PaymentIntentService();

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(totalAmount*100),
            Currency = "usd",
            PaymentMethodTypes = new List<string>
        {
            "card",
        },
            PaymentMethod = stripeToken,
            Confirm = true,
            ReceiptEmail = stripeEmail,
        };

        var paymentIntent = await service.CreateAsync(options);

        if (paymentIntent.Status == "succeeded")
        {
            return paymentIntent.Id;
        }

        throw new Exception("Payment failed");
    }

    //public async Task<string> ChargeAsync(string stripeEmail, string stripeToken, decimal totalAmount)
    //{

    //    var options = new ChargeCreateOptions
    //    {
    //        Amount = (int)(totalAmount * 100), // Converting decimal amount to integer in cents
    //        Currency = "usd",
    //        Description = "Payment description",
    //        Source = stripeToken,
    //        ReceiptEmail = stripeEmail,
    //        Metadata = new Dictionary<string, string>
    //            {
    //                { "Order ID", Guid.NewGuid().ToString() } // Optional: Adding metadata for custom tracking
    //            }
    //    };

    //    var service = new ChargeService();
    //    var charge = await service.CreateAsync(options);

    //    return charge.Id; // Return the charge ID upon successful payment
    //}

}
