using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;

namespace CrossCutting.Identity;

public class SignUpRequest
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName => FirstName + " " + LastName;

    public required string Email { get; set; }
    
    public required string Password { get; set; }

    public required string RePeatPassword { get; set; }

    public required string CompanyName { get; set; }

    public Role Role { get; set; }
}