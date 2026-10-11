using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ecom_bookshopping_1003.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ecom_bookshopping_1003.Utility
{
    public static class OrderNotificationHelper
    {
        public static void SendOrderNotifications(
            OrderHeader orderHeader,
            List<OrderDetail> orderDetails,
            IEmailSender emailSender,
            IConfiguration config)
        {
            Task.Run(async () =>
            {
                try
                {
                    string emailItemsHtml = "";
                    string smsBooksSummary = "";

                    foreach (var item in orderDetails)
                    {
                        string title = item.Product.Title;
                        string author = item.Product.Author; 

                        emailItemsHtml += $@"
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd;'>
                                    <strong>{title}</strong><br/>
                                    <span style='color: #666; font-size: 12px;'>Author: {author}</span>
                                </td>
                                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Count}</td>
                                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>${item.Price}</td>
                                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>${item.Count * item.Price}</td>
                            </tr>";

                        smsBooksSummary += $"\n- {title} (by {author}) x{item.Count}";
                    }

                    string emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                            <h2 style='color: #2b7a78;'>Order Confirmed!</h2>
                            <p>Hi <strong>{orderHeader.Name}</strong>,</p>
                            <p>Your Order Succesfully Place...</p>
                            <p><strong>Order ID:</strong> #{orderHeader.Id}</p>
                            <p><strong>Status:</strong> {orderHeader.PaymentStatus}</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>
                                <thead>
                                    <tr style='background-color: #f2f2f2;'>
                                        <th style='padding: 8px; border: 1px solid #ddd; text-align: left;'>Book & Author</th>
                                        <th style='padding: 8px; border: 1px solid #ddd; text-align: center;'>Qty</th>
                                        <th style='padding: 8px; border: 1px solid #ddd; text-align: right;'>Price</th>
                                        <th style='padding: 8px; border: 1px solid #ddd; text-align: right;'>Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {emailItemsHtml}
                                </tbody>
                            </table>

                            <h3 style='text-align: right; color: #17252a; margin-top: 15px;'>Grand Total: ${orderHeader.OrderTotal}</h3>
                            <p style='font-size: 13px; color: #666; text-align: center;'>
                                Thank you for shopping with Book Shopping!
                            </p>
                        </div>";

                    string userEmail = orderHeader.ApplicationUser?.Email;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        await emailSender.SendEmailAsync(userEmail, $"Order Confirmed - #{orderHeader.Id}", emailBody);
                    }

                    string userPhone = orderHeader.PhoneNumber ?? orderHeader.ApplicationUser?.PhoneNumber;
                    if (!string.IsNullOrEmpty(userPhone))
                    {
                        string accountSid = config["TwilioSettings:AccountSid"];
                        string authToken = config["TwilioSettings:AuthToken"];
                        string fromPhone = config["TwilioSettings:PhoneNumber"];

                        string formattedPhone = userPhone.Trim();
                        if (formattedPhone.StartsWith("0"))
                        {
                            formattedPhone = "+91" + formattedPhone.Substring(1);
                        }
                        else if (!formattedPhone.StartsWith("+"))
                        {
                            formattedPhone = "+91" + formattedPhone;
                        }

                        TwilioClient.Init(accountSid, authToken);

                        string smsBody = $"Hello {orderHeader.Name}, your order #{orderHeader.Id} is confirmed!\nItems:{smsBooksSummary}\nTotal: ${orderHeader.OrderTotal}. Thank you!";

                        MessageResource.Create(
                            body: smsBody,
                            from: new PhoneNumber(fromPhone),
                            to: new PhoneNumber(formattedPhone)
                        );

                        var twiml = new Twilio.TwiML.VoiceResponse();
                        twiml.Say(
                            $"Namaste {orderHeader.Name}! Book Shopping se aapka order number {orderHeader.Id} confirm ho gaya hai. Hamare saath shopping karne ke liye dhanyawaad. Have a nice day!",
                            voice: "Polly.Aditi",
                            language: "hi-IN"
                        );

                        CallResource.Create(
                            twiml: twiml.ToString(),
                            from: new PhoneNumber(fromPhone),
                            to: new PhoneNumber(formattedPhone)
                        );
                    }
                }
                catch (Exception)
                {
                    
                }
            });
        }
    }
}