namespace OpenAIConsoleChatApp.Models;

public class ChatResponse
{
    public required List<Choice> Choice { get; set; }
}

public class Choice
{
    public required ChatMessage Message { get; set; }
}