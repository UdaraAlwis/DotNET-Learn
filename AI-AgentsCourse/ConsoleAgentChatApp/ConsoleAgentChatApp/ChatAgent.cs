using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAgentChatApp
{
    public static class ChatAgent
    {
        public static async Task RunAsync(IServiceProvider sp)
        {
            var client = sp.GetRequiredService<IChatClient>();

            var chatOptions = sp.GetRequiredService<ChatOptions>();

            var history = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a helpful CLI assistant. Use the provided functions when appropriate." +
                "If a tool call fails due to some invalid arguments, " +
                "then make an attempt to fix the arguments yourself by using your best judgement, " +
                "then try calling the tool again.")
            };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to the Console Agent Chat App! (empty = exit).");

            while (true)
            {
                Console.ResetColor();
                Console.WriteLine();
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.ResetColor();
                    break;
                }

                if (input == "/history")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Chat History:");
                    foreach (var item in history)
                    {
                        switch (item.Role.Value)
                        {
                            case "user":
                                {
                                    Console.WriteLine($"USER: {item.Text}");
                                    continue;
                                }
                            case "assistant" when !string.IsNullOrWhiteSpace(item.Text):
                                {
                                    Console.WriteLine($"AI: {item.Text}");
                                    continue;
                                }
                            case "assistant" when item.Contents?.Any() ?? false:
                                {
                                    Console.WriteLine($"REQUEST: {item.Contents[0].ToString()}");
                                    continue;
                                }
                            case "tool":
                                {
                                    Console.WriteLine($"TOOL CALL: {item.Text}");
                                    continue;
                                }
                        }
                    }
                    Console.ResetColor();
                }

                history.Add(new ChatMessage(ChatRole.User, input));

                var response = await client.GetResponseAsync(history, chatOptions);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Assistant: {response.Text}");
                history.AddRange(response.Messages);
            }
        }
    }
}
