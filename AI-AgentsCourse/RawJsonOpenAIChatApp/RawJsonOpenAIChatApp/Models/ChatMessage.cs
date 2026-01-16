namespace OpenAIConsoleChatApp.Models;

public class ChatMessage
{
    public required ChatRole Role { get; set;  }
    public string? Content { get; set; }
}