using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace McpServer
{
    [McpServerPromptType]
    public static class McpPrompts
    {
        [McpServerPrompt, Description("Creates a prompt to pay an invoice")]
        public static ChatMessage PayInvoicePrompt([Description("The name of the invoice to mark as paid")] string invoiceName)
            => new ChatMessage(ChatRole.User, $"Find the invoice \"{invoiceName}\" and mark it as paid.");
    }
}
