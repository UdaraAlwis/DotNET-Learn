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
                new ChatMessage(ChatRole.System, "You are a helpful CLI assistant. Use the provided functions when appropriate.")
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
