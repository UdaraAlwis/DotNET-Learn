using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostChat([FromBody] List<ChatMessage> messages)
        {
            // This is a placeholder for the actual implementation that would call OpenAI API
            // In a real implementation, you would forward these messages to OpenAI or your chosen AI provider
            
            // Simulate an async operation
            await Task.Delay(300); // Simulate a delay to make this truly async
            
            // Process the last user message (in a real implementation, you'd send all messages to the AI service)
            string lastUserMessage = "No message found";
            if (messages != null && messages.Count > 0)
            {
                var userMessages = messages.FindAll(m => m.Role.ToLower() == "user");
                if (userMessages.Count > 0)
                {
                    lastUserMessage = userMessages[userMessages.Count - 1].Content;
                }
            }
            
            // For now, we'll just echo back a simple response that includes the user's message
            var assistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = $"You said: '{lastUserMessage}'. This is a placeholder response. In a real implementation, this would be the response from the AI service."
            };
            
            return Ok(assistantMessage);
        }
    }

    public class ChatMessage
    {
        public required string Role { get; set; }
        public required string Content { get; set; }
    }
}
