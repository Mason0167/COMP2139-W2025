namespace COMP2139_ICE1.Services;

using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Sockets; // Required for SocketException
using Polly; // Required for Polly
using Polly.Retry; // Required for RetryPolicy
using System.Security.Authentication; // Required for AuthenticationException

// <summary>
// Service for sending emails using MailKit, implementing the IEmailSender interface.
// SMTP settings are retrieved from the application's configuration.
public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly AsyncRetryPolicy _retryPolicy;

    // <summary>
    // Initializes a new instance of the <see cref="EmailSender"/> class.
    // <param name="config">The application's configuration, used to retrieve SMTP settings.</param>
    public EmailSender(IConfiguration config)
    {
        _config = config;
        _retryPolicy = Policy
            .Handle<SocketException>()
            .Or<AuthenticationException>()
            .WaitAndRetryAsync(
                retryCount: 3, // Retry 3 times
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // Exponential backoff
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    // Log the retry attempt (optional)
                    Console.WriteLine($"Retry {retryCount} encountered exception {exception.GetType().Name}. Waiting {timeSpan.TotalSeconds} seconds.");
                });
    }

    // <summary>
    // Sends an email asynchronously using the configured SMTP settings.
    // <param name="email">The recipient's email address.</param>
    // <param name="subject">The subject of the email.</param>
    // <param name="htmlMessage">The HTML body of the email.</param>
    // <returns>A Task that represents the asynchronous email sending operation.</returns>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Retrieve SMTP settings from the application configuration.
        // These settings are expected to be under a section named "SmtpSettings".
        var smtp = _config.GetSection("SmtpSettings");

        // Create a new MimeMessage to construct the email.
        var message = new MimeMessage();
        // Set the sender's name and email address.
        message.From.Add(new MailboxAddress(smtp["SenderName"], smtp["SenderEmail"]));
        // Set the recipient's email address.
        message.To.Add(new MailboxAddress(email, email));
        // Set the email subject.
        message.Subject = subject;

        // Create a BodyBuilder to construct the email body, specifically for HTML content.
        var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
        message.Body = bodyBuilder.ToMessageBody();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Use an SmtpClient for connecting to the SMTP server.
            using var client = new SmtpClient();
            
            // Connect to the SMTP host and port.
            await client.ConnectAsync(
                smtp["Host"],
                int.Parse(smtp["Port"]),
                MailKit.Security.SecureSocketOptions.SslOnConnect
            );

            // Authenticate with the SMTP server using the provided username and password.
            await client.AuthenticateAsync(smtp["Username"], smtp["Password"]);

            // Send the constructed email message.
            await client.SendAsync(message);
            
            // Disconnect from the SMTP server, gracefully shutting down the connection.
            await client.DisconnectAsync(true);
        });
    }
}

