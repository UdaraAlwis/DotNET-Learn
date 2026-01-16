namespace RawJsonOpenAIChatApp.Models;

public class ChatResponse
{
    public required List<Choice> Choices { get; set; }
}

public class Choice
{
    public required ChatMessage Message { get; set; }
}