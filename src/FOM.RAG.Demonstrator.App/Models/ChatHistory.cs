using OpenAI.Chat;

namespace FOM.RAG.Demonstrator.App.Models;

public class ChatHistory
{
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    public void AddMessage(string role, string msg)
    {
        //var chatMessage = new OldChatMessage(role, msg);

        if (role == "user")
        {
            Messages.Add(new UserChatMessage(msg));

        }
        else //if (role == "assistant")
        {
            Messages.Add(new SystemChatMessage(msg));
        }

    }
}
