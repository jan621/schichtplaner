using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CrossCutting.Entities;

public class User : IdentityUser
{
    [Required] public required string FirstName { get; set; }

    [Required] public required string LastName { get; set; }

    [NotMapped] public string FullName => FirstName + " " + LastName;

    [Required] public bool Activated { get; set; }

    [Required] public bool Deleted { get; set; }

    [Required] public required Organization Organization { get; set; }

    public List<Team>? Teams { get; set; }

    public Guid? PasswordResetGuid { get; set; }
    
    public string? SmoobuApiKey { get; set; }
}