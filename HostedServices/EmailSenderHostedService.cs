using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BackgroundEmailSenderSample.Models.Options;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BackgroundEmailSenderSample.HostedServices
{
    public class EmailSenderHostedService : IEmailSender, IHostedService, IDisposable
    {
        //The BufferBlock<T> is a thread-safe producer/consumer queue
        private readonly BufferBlock<MimeMessage> mailMessages;
        private readonly ILogger logger;
        private Task sendTask;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IOptionsMonitor<SmtpOptions> optionsMonitor;
        public EmailSenderHostedService(IConfiguration configuration, IOptionsMonitor<SmtpOptions> optionsMonitor, ILogger<EmailSenderHostedService> logger)
        {
            this.optionsMonitor = optionsMonitor;
            this.logger = logger;
            this.mailMessages = new BufferBlock<MimeMessage>();
        }

        // This public method will be used by other services (such as a Controller or an application service)
        // to send messages, which will be actually delivered asynchronously in background.
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(optionsMonitor.CurrentValue.Sender));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };
            // We just enqueue the message in memory. Delivery will be attempted in background (see the DeliverAsync method)
            #warning Implement durable persistence or messages will be lost when the application is restarted
            await this.mailMessages.SendAsync(message);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting background e-mail delivery");
            cancellationTokenSource = new CancellationTokenSource();
            // The StartAsync method just needs to start a background task (or a timer)
            sendTask = DeliverAsync(cancellationTokenSource.Token);
            return Task.CompletedTask;
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //Let's cancel the e-mail delivery
            CancelSendTask();
            //Next, we wait for sendTask to end, but no longer than what the web host allows
            await Task.WhenAny(sendTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private void CancelSendTask()
        {
            try
            {
                if (cancellationTokenSource != null)
                {
                    logger.LogInformation("Stopping e-mail background delivery");
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = null;
                }
            }
            catch
            {

            }
        }

        public async Task DeliverAsync(CancellationToken token)
        {
            logger.LogInformation("E-mail background delivery started");

            while (!token.IsCancellationRequested)
            {
                MimeMessage message = null;
                try
                {
                    // Let's wait for a message to appear in the queue
                    // If the token gets canceled, then we'll stop waiting
                    // since an OperationCanceledException will be thrown
                    message = await mailMessages.ReceiveAsync(token);

                    // As soon as a message is available, we'll send it
                    var options = this.optionsMonitor.CurrentValue;
                    using var client = new SmtpClient();
                    await client.ConnectAsync(options.Host, options.Port, options.Security);
                    if (!string.IsNullOrEmpty(options.Username))
                    {
                        await client.AuthenticateAsync(options.Username, options.Password);
                    }
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    logger.LogInformation($"E-mail sent to {message.To}");
                }
                catch (OperationCanceledException)
                {
                    // We need to terminate the delivery, so we'll just break the while loop
                    break;
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, "Couldn't send an e-mail to {recipient}", message.To[0]);
                    // Just wait a second, perhaps the mail server was busy
                    await Task.Delay(1000);
                    // Then re-queue this email for later delivery
                    #warning Some email messages will be stuck in the loop if the SMTP server will always reject them because of their content
                    await mailMessages.SendAsync(message);
                }
            }

            logger.LogInformation("E-mail background delivery stopped");
        }

        public void Dispose()
        {
            CancelSendTask();
        }
    }

}