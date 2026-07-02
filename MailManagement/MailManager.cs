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
                smtp.Credentials = new NetworkCredential(_config.From, _config.Password);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(email);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }
}