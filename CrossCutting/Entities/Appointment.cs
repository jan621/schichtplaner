using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrossCutting.Entities.NotMapped;

namespace CrossCutting.Entities;

public class Appointment : AuditableEntity
{
    [Key]
    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Id { get; set; }

    public required DateTime Date { get; set; }
    
    public required Tag Tag { get; set; }
}