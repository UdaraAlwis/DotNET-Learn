using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceApp.Models;
using InvoiceApp.Data;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

public class InvoicesModel : PageModel
{
    private readonly InvoiceContext _context;
    public List<Invoice> Invoices { get; set; } = new List<Invoice>();

    public InvoicesModel(InvoiceContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        Invoices = _context.Invoices.OrderByDescending(i => i.Date).ToList();
    }


}
