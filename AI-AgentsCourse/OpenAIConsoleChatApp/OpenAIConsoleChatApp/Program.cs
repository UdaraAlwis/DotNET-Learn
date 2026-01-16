using dotenv.net;
using OpenAI.Chat;
using OpenAI.Responses;

DotEnv.Load();
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(openAiApiKey))
{
    throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
}

ChatClient client = new ChatClient(model: "gpt-4.1-nano", openAiApiKey);

List<ChatMessage> messages = [
    new AssistantChatMessage("Hello! How can I assist you today?")
];

Console.WriteLine(messages[0].Content[0].Text);

while (true)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input?.ToLower() == "exit")
    {
        break;
    }

    Console.ResetColor();

    messages.Add(new UserChatMessage(input!));

    ChatCompletion completion = client.CompleteChat(messages);

    var response = completion.Content[0].Text;

    messages.Add(new AssistantChatMessage(response));

    Console.WriteLine(response);
}