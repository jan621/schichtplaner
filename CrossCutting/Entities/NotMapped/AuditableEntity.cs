using System.ComponentModel;

namespace CrossCutting.Entities.NotMapped;

public abstract class AuditableEntity
{
    [DefaultValue(null)]
    public DateTime CreatedDate { get; set; }
    
    [DefaultValue(null)]
    public string? CreatedBy { get; set; }
    
    [DefaultValue(null)]
    public DateTime UpdatedDate { get; set; }
    
    [DefaultValue(null)]
    public string? UpdatedBy { get; set; } 
    
    [DefaultValue(null)]
    public string? Organization { get; set; } 
}