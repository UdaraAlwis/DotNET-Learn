namespace ConsoleAgentChatApp.Services
{
    public class EmailService
    {
        public Task EmailFrind(string friendName, string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[EmailService] Emailing {friendName}: {message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
