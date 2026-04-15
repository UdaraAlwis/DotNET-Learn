using InvoiceAgentApi.Services;
using Microsoft.Extensions.AI;

namespace InvoiceAgentApi
{
    public static class FunctionRegistry
    {
        public static IEnumerable<AITool> GetTools(IServiceProvider sp)
        {
            var docService = sp.GetRequiredService<DocumentationClient>();

            yield return AIFunctionFactory.Create(typeof(DocumentationClient).GetMethod(nameof(DocumentationClient.GetDocumentationPage),
               [typeof(string)])!,
               docService,
               new AIFunctionFactoryOptions
               {
                   Name = "read_documentation_page",
                   Description = "Retrieves the contents of this page in the documentation"
               });

            var apiClient = sp.GetRequiredService<InvoiceApiClient>();

            yield return AIFunctionFactory.Create(typeof(InvoiceApiClient).GetMethod(nameof(InvoiceApiClient.ListInvoices), 
                Type.EmptyTypes)!, 
                apiClient,
                new AIFunctionFactoryOptions
                {
                    Name = "list_invoices",
                    Description = "Retrieve a list of all invoices in the system"
                });

            yield return AIFunctionFactory.Create(typeof(InvoiceApiClient).GetMethod(nameof(InvoiceApiClient.FindInvoiceByName),
                [typeof(string)])!,
                apiClient,
                new AIFunctionFactoryOptions
                {
                    Name = "find_invoice_by_name",
                    Description = "Finds the invoice with this name"
                });
        }
    }
}