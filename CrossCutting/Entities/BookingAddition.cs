using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrossCutting.Entities.NotMapped;

namespace CrossCutting.Entities;

public class BookingAddition: AuditableEntity
{
    [Key]
    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Id { get; set; }
    
    public required string BookingId { get; set; }
    
    public string? Note { get; set; }
    
    public required List<User> Employees { get; set; }
}