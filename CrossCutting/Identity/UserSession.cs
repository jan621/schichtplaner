namespace CrossCutting.Identity;

public class UserSession
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName => FirstName + " " + LastName;
    
    public required string Email { get; set; }
}