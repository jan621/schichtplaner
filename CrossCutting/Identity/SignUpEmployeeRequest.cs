using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;

namespace CrossCutting.Identity;

public class SignUpEmployeeRequest
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName => FirstName + " " + LastName;

    public required string Email { get; set; }
    
    public required List<Team> Teams { get; set; }
    
    public required Organization Organization { get; set; }
    
    public Role Role { get; set; }
}