using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrossCutting.Entities.NotMapped;

namespace CrossCutting.Entities;

public class Tag : AuditableEntity
{
    [Key]
    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Id { get; set; }
    
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string ColorHexCode { get; set; }
    
    [Required]
    public required bool IsNonAccommodation { get; set; }
    
    [Required]
    public List<string> AccommodationIds { get; set; }
    
    [Required]
    [DefaultValue(0)]
    public required int SortIndex { get; set; }
}