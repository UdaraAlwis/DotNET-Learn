using dotenv.net;
using RawJsonOpenAIChatApp.Models;

DotEnv.Load();
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(openAiApiKey))
{
    throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
}

List<ChatMessage> messages = new()
{
    new ChatMessage
    {
        Role = ChatRole.System,
        Content = "Hello, what do you want to do today?"
    },
};

Console.WriteLine(messages[0].Content);

var aiService = new RawJsonOpenAIChatApp.Services.OpenAiService(openAiApiKey);

while (true)
{
    Console.ForegroundColor = ConsoleColor.Blue;

    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput) || userInput?.ToLower() == "exit")
    {
        break;
    }

    Console.ResetColor();

    messages.Add(new ChatMessage
    {
        Role = ChatRole.User,
        Content = userInput
    });

    var responseMessage = await aiService.CompleteChat(messages);

    messages.Add(responseMessage);

    Console.WriteLine(responseMessage.Content);
}
