using System.ComponentModel.DataAnnotations;
using CrossCutting.Entities.NotMapped;

namespace CrossCutting.Entities;

public class Team : AuditableEntity
{
    [Key] public int Id { get; set; }
    
    [Required] public required string Name { get; set; }
    
    [Required] public required List<User> Members { get; set; }
    
    public override bool Equals(object o)
    {
        var other = o as Team;
        return other?.Id == Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => Name;
}