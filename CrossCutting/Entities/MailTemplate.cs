using System.ComponentModel.DataAnnotations;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;

namespace CrossCutting.Entities;

public class MailTemplate : AuditableEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public MailTemplateType TemplateType { get; set; }

    [Required]
    public required string Subject { get; set; }

    [Required]
    public required string Body { get; set; }
}