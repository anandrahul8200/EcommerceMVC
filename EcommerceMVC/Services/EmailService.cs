using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using EcommerceMVC.Models;

namespace EcommerceMVC.Services
{
    public class EmailService
    {
        private readonly EcommerceContext _db;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly bool _emailEnabled;

        public EmailService()
        {
            _db = new EcommerceContext();
            
            // Load settings from database or config
            _smtpServer = GetSetting("SMTP_Server", "smtp.gmail.com");
            _smtpPort = int.Parse(GetSetting("SMTP_Port", "587"));
            _smtpUsername = GetSetting("SMTP_Username", "");
            _smtpPassword = GetSetting("SMTP_Password", "");
            _enableSsl = bool.Parse(GetSetting("SMTP_EnableSSL", "true"));
            _fromAddress = GetSetting("Email_FromAddress", "noreply@yourdomain.com");
            _fromName = GetSetting("Email_FromName", "E-commerce Store");
            _emailEnabled = bool.Parse(GetSetting("Email_Enabled", "true"));
        }

        private string GetSetting(string key, string defaultValue)
        {
            try
            {
                var setting = _db.SystemSettings.FirstOrDefault(s => s.SettingKey == key);
                return setting?.SettingValue ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task<bool> SendOrderConfirmationAsync(Order order)
        {
            if (!_emailEnabled) return true;

            var subject = $"Order Confirmation - {order.OrderNumber}";
            var body = GenerateOrderConfirmationEmail(order);

            return await SendEmailAsync(
                order.Customer.Email,
                order.Customer.FullName,
                subject,
                body,
                "OrderConfirmation",
                order.OrderID,
                order.CustomerID
            );
        }

        public async Task<bool> SendWelcomeEmailAsync(Customer customer, string password = null)
        {
            if (!_emailEnabled) return true;

            var subject = "Welcome to Our Store!";
            var body = GenerateWelcomeEmail(customer, password);

            return await SendEmailAsync(
                customer.Email,
                customer.FullName,
                subject,
                body,
                "Welcome",
                null,
                customer.CustomerID
            );
        }

        public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
        {
            if (!_emailEnabled) return true;

            var subject = "Password Reset Request";
            var body = GeneratePasswordResetEmail(resetToken);

            return await SendEmailAsync(
                email,
                "",
                subject,
                body,
                "PasswordReset"
            );
        }

        public async Task<bool> SendLowStockAlertAsync(Product product, int currentStock)
        {
            if (!_emailEnabled) return true;

            var adminEmail = GetSetting("Admin_Email", "admin@yourdomain.com");
            var subject = $"Low Stock Alert - {product.ProductName}";
            var body = GenerateLowStockEmail(product, currentStock);

            return await SendEmailAsync(
                adminEmail,
                "Store Administrator",
                subject,
                body,
                "LowStockAlert"
            );
        }

        private async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, 
            string emailType, int? orderId = null, int? customerId = null)
        {
            var notification = new EmailNotification
            {
                EmailType = emailType,
                RecipientEmail = toEmail,
                RecipientName = toName,
                Subject = subject,
                Body = body,
                OrderID = orderId,
                CustomerID = customerId,
                Status = "Pending",
                CreatedDate = DateTime.Now
            };

            _db.EmailNotifications.Add(notification);
            _db.SaveChanges();

            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = _enableSsl;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    var message = new MailMessage
                    {
                        From = new MailAddress(_fromAddress, _fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    message.To.Add(new MailAddress(toEmail, toName));

                    await client.SendMailAsync(message);
                }

                // Update notification status
                notification.Status = "Sent";
                notification.SentDate = DateTime.Now;
                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                // Update notification with error
                notification.Status = "Failed";
                notification.ErrorMessage = ex.Message;
                _db.SaveChanges();

                return false;
            }
        }

        private string GenerateOrderConfirmationEmail(Order order)
        {
            var itemsHtml = "";
            foreach (var item in order.OrderItems)
            {
                itemsHtml += $@"
                    <tr>
                        <td>{item.Product.ProductName}</td>
                        <td>{item.Quantity}</td>
                        <td>${item.UnitPrice:N2}</td>
                        <td>${(item.Quantity * item.UnitPrice):N2}</td>
                    </tr>";
            }

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Order Confirmation</h2>
                    <p>Dear {order.Customer.FullName},</p>
                    <p>Thank you for your order! Here are the details:</p>
                    
                    <h3>Order #{order.OrderNumber}</h3>
                    <p><strong>Order Date:</strong> {order.OrderDate:MMMM dd, yyyy}</p>
                    <p><strong>Status:</strong> {order.OrderStatus}</p>
                    
                    <table border='1' cellpadding='10' style='border-collapse: collapse; width: 100%;'>
                        <thead>
                            <tr style='background-color: #f5f5f5;'>
                                <th>Product</th>
                                <th>Quantity</th>
                                <th>Price</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                    </table>
                    
                    <div style='margin-top: 20px;'>
                        <p><strong>Subtotal:</strong> ${order.SubTotal:N2}</p>
                        <p><strong>Tax:</strong> ${order.TaxAmount:N2}</p>
                        <p><strong>Shipping:</strong> ${order.ShippingAmount:N2}</p>
                        <h3><strong>Total: ${order.TotalAmount:N2}</strong></h3>
                    </div>
                    
                    <p>We'll send you another email when your order ships.</p>
                    <p>Thank you for shopping with us!</p>
                </body>
                </html>";
        }

        private string GenerateWelcomeEmail(Customer customer, string password)
        {
            var passwordInfo = string.IsNullOrEmpty(password) ? 
                "You can log in using the password you created during registration." :
                $"Your temporary password is: <strong>{password}</strong><br/>Please change it after your first login.";

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Welcome to Our Store!</h2>
                    <p>Dear {customer.FullName},</p>
                    <p>Welcome to our e-commerce store! We're excited to have you as a customer.</p>
                    
                    <h3>Your Account Details:</h3>
                    <p><strong>Email:</strong> {customer.Email}</p>
                    <p>{passwordInfo}</p>
                    
                    <p>You can now:</p>
                    <ul>
                        <li>Browse our product catalog</li>
                        <li>Add items to your cart</li>
                        <li>Track your orders</li>
                        <li>Leave product reviews</li>
                    </ul>
                    
                    <p>Happy shopping!</p>
                </body>
                </html>";
        }

        private string GeneratePasswordResetEmail(string resetToken)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Password Reset Request</h2>
                    <p>You requested a password reset for your account.</p>
                    <p>Your reset token is: <strong>{resetToken}</strong></p>
                    <p>This token will expire in 24 hours.</p>
                    <p>If you didn't request this reset, please ignore this email.</p>
                </body>
                </html>";
        }

        private string GenerateLowStockEmail(Product product, int currentStock)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Low Stock Alert</h2>
                    <p><strong>Product:</strong> {product.ProductName}</p>
                    <p><strong>SKU:</strong> {product.SKU}</p>
                    <p><strong>Current Stock:</strong> {currentStock}</p>
                    <p><strong>Minimum Level:</strong> {product.MinStockLevel}</p>
                    <p><strong>Reorder Point:</strong> {product.ReorderPoint}</p>
                    
                    <p style='color: red;'><strong>Action Required:</strong> Please restock this product.</p>
                </body>
                </html>";
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}