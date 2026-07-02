using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrossCutting.Entities.NotMapped;

namespace CrossCutting.Entities;

public class TimeTrackingEntry : AuditableEntity
{
    [Key]
    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Id { get; set; }
    
    [Required] public required DateTime Start { get; set; }

    public DateTime? End { get; set; }

    [Required] public required User User { get; set; }
    
    [NotMapped]
    public string SumHours
    {
        get
        {
            if (!End.HasValue) return string.Empty;
            
            var duration = End.Value - Start;

            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;

            return $"{hours:00}:{minutes:00}";

        }
    }
}