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

            int turnsSinceLastSummary = 0;
            const int SUMMARY_INTERVAL = 5;

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
                turnsSinceLastSummary++;
                if (turnsSinceLastSummary >= SUMMARY_INTERVAL)
                {
                    var summary = await SummarizeHistory(history, client, chatOptions);
                    history = [
                        history[0],
                        new ChatMessage(ChatRole.System, summary)
                    ];
                    turnsSinceLastSummary = 0;
                }
            }
        }

        static async Task<string> SummarizeHistory(List<ChatMessage> history, IChatClient client, ChatOptions chatOptions)
        {
            var summaryPrompt = "Summarize the following conversation in a few sentences: \n\n";
            foreach (var message in history)
            {
                summaryPrompt += $"{message.Role}: {message.Text}\n";
            }

            var summaryHistory = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, summaryPrompt),
            };

            var summaryResponse = await client.GetResponseAsync(summaryPrompt, chatOptions);
            return summaryResponse.Text;
        }
    }
}
