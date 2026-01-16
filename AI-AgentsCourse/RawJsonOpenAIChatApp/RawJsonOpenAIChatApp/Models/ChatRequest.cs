namespace OpenAIConsoleChatApp.Models;

public class ChatRequest
{
    public required string Model { get; set; }
    public required List<ChatMessage> Messages { get; set; }
}