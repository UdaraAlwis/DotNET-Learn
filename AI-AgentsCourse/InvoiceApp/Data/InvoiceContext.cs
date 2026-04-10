using Microsoft.EntityFrameworkCore;
using InvoiceApp.Models;

namespace InvoiceApp.Data
{
    public class InvoiceContext : DbContext
    {
        public InvoiceContext(DbContextOptions<InvoiceContext> options) : base(options) { }

        public DbSet<Invoice> Invoices { get; set; }
    }
}
