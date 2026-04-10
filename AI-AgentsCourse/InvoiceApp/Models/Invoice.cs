using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public required string Status { get; set; }

        [Required]
        public DateTime Due { get; set; }
    }
}
