using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer
{
    [McpServerResourceType]
    public static class McpResources
    {
        [McpServerResource(MimeType = "text/markdown"), Description("Document describing how to use the InvoiceApp platform")]
        public static string GetDocumentationMarkdown()
        {
            return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Docs", "getting-started.md"));
        }
    }
}
