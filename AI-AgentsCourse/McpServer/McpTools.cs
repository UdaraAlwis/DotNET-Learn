using McpServer.Models;
using McpServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer
{
    [McpServerToolType]
    public static class McpTools
    {
        [McpServerTool, Description("Retrieves a list of all invoices in the InvoiceApp")]
        public static Task<List<Invoice>> ListInvoices(InvoiceApiClient client)
        {
            return client.ListInvoices();
        }

        [McpServerTool, Description("Finds the invoice with this name")]
        public static Task<Invoice> FindInvoiceByName(string invoiceName, InvoiceApiClient client)
        {
            return client.FindInvoiceByName(invoiceName);
        }

        [McpServerTool, Description("Creates a new invoice and returns the new invoice object")]
        public static Task<Invoice> CreateInvoice(CreateInvoiceRequest createInvoiceRequest, InvoiceApiClient client)
        {
            return client.CreateInvoice(createInvoiceRequest);
        }

        [McpServerTool, Description("Marks an invoice as paid")]
        public static Task MarkAsPaid(string invoiceId, InvoiceApiClient client)
        {
            return client.MarkAsPaid(int.Parse(invoiceId));
        }
    }
}
