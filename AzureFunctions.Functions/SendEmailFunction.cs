using System;
using System.Threading.Tasks;
using AzureFunctions.Domain.Messages;
using AzureFunctions.Domain.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Functions;

public static class SendEmailFunction
{
    [FunctionName("SendEmailFunction")]
    public static async Task RunAsync(
        [ServiceBusTrigger("send-email", Connection = "SeviceBusConnection")]
        EmailToSend message, ILogger log)
    {
        try
        {
            var toEmailsList = SendEmailService.GetToEmailAddressList(message.To);

            log.LogInformation("Calling the SendEmail method...");
            await SendEmailService.SendEmail(
                Environment.GetEnvironmentVariable("EmailHost"),
                int.Parse(Environment.GetEnvironmentVariable("EmailPort")),
                Environment.GetEnvironmentVariable("EmailUser"),
                Environment.GetEnvironmentVariable("EmailPassword"),
                Environment.GetEnvironmentVariable("EmailFromName"),
                Environment.GetEnvironmentVariable("EmailFromEmail"),
                toEmailsList,
                message.Subject,
                message.PlainBody,
                message.HtmlBody,
                null,
                null,
                bool.Parse(Environment.GetEnvironmentVariable("EmailHostUsesLocalCertificate"))
            );

            log.LogInformation($"Email to: {string.Join(";", toEmailsList.ToArray())} sent successfully!");
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);
        }
    }
}