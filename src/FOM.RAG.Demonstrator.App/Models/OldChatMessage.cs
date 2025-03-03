namespace FOM.RAG.Demonstrator.App.Models;

public class OldChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public OldChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
        Timestamp = DateTime.Now;
    }
}
