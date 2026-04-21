namespace McpServer.Models
{
    public class CreateInvoiceRequest
    {
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
