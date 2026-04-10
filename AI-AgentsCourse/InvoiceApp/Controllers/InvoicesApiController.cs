using Microsoft.AspNetCore.Mvc;
using InvoiceApp.Data;
using InvoiceApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceApp.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoicesApiController : ControllerBase
    {
        private readonly InvoiceContext _context;
        public InvoicesApiController(InvoiceContext context)
        {
            _context = context;
        }

        // GET: api/invoices
        [HttpGet]
        public ActionResult<IEnumerable<Invoice>> GetInvoices()
        {
            return _context.Invoices.OrderByDescending(i => i.Date).ToList();
        }

        // GET: api/invoices/by-description?description=SomeValue
        [HttpGet("by-description")]
        public ActionResult<Invoice> GetInvoiceByDescription([FromQuery] string description)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.Description != null && i.Description.ToLower() == description.ToLower());
            if (invoice == null)
            {
                return NotFound();
            }
            return invoice;
        }

        // POST: api/invoices
        [HttpPost]
        public ActionResult<Invoice> PostInvoice([FromBody] Invoice invoice)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetInvoices), new { id = invoice.Id }, invoice);
        }

        // DELETE: api/invoices
        [HttpDelete("all")]
        public IActionResult DeleteAllInvoices()
        {
            _context.Invoices.RemoveRange(_context.Invoices);
            _context.SaveChanges();
            return Ok();
        }

        // POST: api/invoices/{id}/status
        [HttpPost("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound(new { success = false, message = "Invoice not found." });
            }
            if (dto.Status != "Paid" && dto.Status != "Pending")
            {
                return BadRequest(new { success = false, message = "Invalid status." });
            }
            invoice.Status = dto.Status;
            _context.SaveChanges();
            return Ok(new { success = true });
        }

        public class StatusUpdateDto
        {
            public string? Status { get; set; }
        }
    }
}
