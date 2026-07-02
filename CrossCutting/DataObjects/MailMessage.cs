namespace CrossCutting.DataObjects;

// ReSharper disable once ClassNeverInstantiated.Global
public class MailMessage
{
    public required string To { get; set; }
    
    public string? Subject { get; set; }
    
    public string? Body { get; set; }
    
    public string? From { get; set; }
}