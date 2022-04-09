namespace AzureFunctions.Domain.Messages;

public class EmailToSend
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string PlainBody { get; set; }
    public string HtmlBody { get; set; }
}