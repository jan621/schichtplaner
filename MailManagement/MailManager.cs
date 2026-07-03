using System.Net;
using System.Net.Mail;
using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;
using Datastoring.EfCore;
using MailManagement.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailMessage = CrossCutting.DataObjects.MailMessage;

namespace MailManagement;

public class MailManager(
    PlanerContext context,
    IOptions<MailConfiguration> config,
    ILogger<MailManager> logger,
    IConfiguration configuration)
    : IMailManager
{
    private readonly MailConfiguration _config = config.Value;

    public MailTemplate? GetMailTemplateByType(MailTemplateType mailTemplateType)
    {
        var dbResult = context.MailTemplates.SingleOrDefault(m => m.TemplateType == mailTemplateType);
        return dbResult;
    }

    public async Task SendAsync(MailMessage message)
    {
        try
        {
            var email = new System.Net.Mail.MailMessage()
            {
                Subject = message.Subject,
                Body = message.Body,
                To = { message.To },
                From = new MailAddress(_config.From),
                IsBodyHtml = true
            };

            // ReSharper disable once ConvertToUsingDeclaration
            using (var smtp = new SmtpClient(_config.Host, _config.Port))
            {
                var smtpUser = string.IsNullOrWhiteSpace(_config.UserName) ? _config.From : _config.UserName;
                smtp.Credentials = new NetworkCredential(smtpUser, _config.Password);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(email);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send mail to {Recipient}", message.To);
        }
    }

    public async Task EnsureDefaultTemplatesAsync()
    {
        if (context.MailTemplates.Any())
            return;

        // Absolute links in mails need the public address of this installation,
        // e.g. https://planer.example.com (env var AppBaseUrl).
        var baseUrl = (configuration["AppBaseUrl"] ?? string.Empty).TrimEnd('/');
        var now = DateTime.UtcNow;

        context.MailTemplates.AddRange(
            new MailTemplate
            {
                TemplateType = MailTemplateType.ConfirmRegistration,
                Subject = "[Schichtplaner] Account bestätigen",
                Body = $"Hallo {{0}},<br/><br/>bitte bestätige deinen Account über diesen Link:<br/>" +
                       $"<a href=\"{baseUrl}/confirmMail/{{1}}\">Account bestätigen</a>",
                CreatedDate = now, UpdatedDate = now
            },
            new MailTemplate
            {
                TemplateType = MailTemplateType.ResetPassword,
                Subject = "[Schichtplaner] Passwort zurücksetzen",
                Body = $"Hallo {{0}},<br/><br/>setze dein Passwort über diesen Link zurück:<br/>" +
                       $"<a href=\"{baseUrl}/passwordReset-{{1}}\">Passwort zurücksetzen</a><br/><br/>" +
                       "Dein Bestätigungscode:<br/>{2}",
                CreatedDate = now, UpdatedDate = now
            },
            new MailTemplate
            {
                TemplateType = MailTemplateType.EmployeeInvitation,
                Subject = "[Schichtplaner] Einladung",
                Body = $"Hallo {{0}},<br/><br/>du wurdest zur Organisation {{1}} eingeladen.<br/>" +
                       $"Bestätige deinen Account über diesen Link:<br/>" +
                       $"<a href=\"{baseUrl}/confirmEmployeeAccount/{{2}}\">Account bestätigen</a><br/><br/>" +
                       "Dein Startpasswort: {3}",
                CreatedDate = now, UpdatedDate = now
            },
            new MailTemplate
            {
                TemplateType = MailTemplateType.AccountActivatedByHost,
                Subject = "[Schichtplaner] Account freigeschaltet",
                Body = $"Hallo {{0}},<br/><br/>dein Account wurde freigeschaltet. " +
                       $"Du kannst dich jetzt anmelden:<br/><a href=\"{baseUrl}/\">Zur Anmeldung</a>",
                CreatedDate = now, UpdatedDate = now
            });

        await context.SaveChangesAsync();
    }
}