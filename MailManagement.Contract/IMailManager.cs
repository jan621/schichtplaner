using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;

namespace MailManagement.Contract;

public interface IMailManager
{
    MailTemplate? GetMailTemplateByType(MailTemplateType mailTemplateType);
    
    Task SendAsync(MailMessage message);

    Task EnsureDefaultTemplatesAsync();
}