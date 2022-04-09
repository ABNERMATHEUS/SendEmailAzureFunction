using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;

namespace AzureFunctions.Domain.Service;

public static class SendEmailService
{
    public static List<string> GetToEmailAddressList(string toEmails)
    {
        // If there are no "ToEmails", then send it to the default "FromEmail"
        if (string.IsNullOrWhiteSpace(toEmails))
            return new List<string>() {Environment.GetEnvironmentVariable("EmailFromEmail")};

        return new List<string>(toEmails.Split(";"));
    }

    public static async Task SendEmail(
        string host,
        int port,
        string user,
        string password,
        string fromName,
        string fromEmail,
        List<string> toEmails,
        string subject,
        string bodyPlain,
        string bodyHtml,
        string linkedResourcePath,
        string attachmentPath,
        bool hostUsesLocalCertificate = false)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));

        message.Subject = subject;

        foreach (var toEmail in toEmails)
        {
            message.To.Add(new MailboxAddress(toEmail, toEmail));
        }

        var builder = new BodyBuilder();

        if (!string.IsNullOrWhiteSpace(bodyPlain))
            builder.TextBody = bodyPlain;

        if (!string.IsNullOrWhiteSpace(bodyHtml))
        {
            if (!string.IsNullOrWhiteSpace(linkedResourcePath))
            {
                var image = await builder.LinkedResources.AddAsync(linkedResourcePath);
                image.ContentId = MimeUtils.GenerateMessageId();
                builder.HtmlBody = string.Format(bodyHtml, image.ContentId);
            }
            else
            {
                builder.HtmlBody = bodyHtml;
            }
        }

        // Attachment
        if (!string.IsNullOrWhiteSpace(attachmentPath))
            await builder.Attachments.AddAsync(attachmentPath);

        // Assigns the email body to the message
        message.Body = builder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        
        if (hostUsesLocalCertificate)
            smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        
        await smtpClient.ConnectAsync(host, port, true);
        smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
        await smtpClient.AuthenticateAsync(user, password);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}