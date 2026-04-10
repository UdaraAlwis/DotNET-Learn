using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceApp.Models;
using InvoiceApp.Data;

public class SubmitInvoiceModel : PageModel
{
    private readonly InvoiceContext _context;

    [BindProperty]
    public Invoice Invoice { get; set; } = new Invoice { Date = System.DateTime.Today, Description = string.Empty, Status = "Pending" };

    public SubmitInvoiceModel(InvoiceContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        Invoice = new Invoice {
            Date = System.DateTime.Today,
            Due = System.DateTime.Today.AddMonths(1),
            Description = string.Empty,
            Status = "Pending"
        };
    }

    public IActionResult OnPost()
    {
        // Set Status to "Pending" if not provided (form never posts it)
        if (string.IsNullOrEmpty(Invoice.Status))
        {
            Invoice.Status = "Pending";
            // Remove ModelState error for Status so validation passes
            ModelState.Remove("Invoice.Status");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        _context.Invoices.Add(Invoice);
        _context.SaveChanges();
        return RedirectToPage("/Invoices");
    }
}
