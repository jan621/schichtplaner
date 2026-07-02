namespace CrossCutting.DataObjects;

public class MailConfiguration
{
    public required string From { get; set; }
    
    public required string Host { get; set; }
    
    public int Port { get; set; }
    
    public required string UserName { get; set; }
    
    public required string Password { get; set; }
    
    public required string DisplayName { get; set; }
}