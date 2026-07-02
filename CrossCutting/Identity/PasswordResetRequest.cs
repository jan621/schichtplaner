namespace CrossCutting.Identity;

public class PasswordResetRequest
{
    public required string UserName { get; set; }
    
    public required string Token { get; set; }

    public required string Password { get; set; }
}