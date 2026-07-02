using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossCutting.Entities;

public class Organization
{
    [Key]
    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public string Id { get; set; }

    [Required] public required string Name { get; set; }

    [NotMapped] public User? Leader { get; set; }

    public required List<User> Users { get; set; }

    [Required] [DefaultValue(8)] public required byte TimeTrackingGapHours { get; set; } = 8;

    [Required] public bool UserAllowedEditTimeTracking { get; set; } = false;

    [Required] public bool UserAllowedManualTimeTracking { get; set; } = false;
}