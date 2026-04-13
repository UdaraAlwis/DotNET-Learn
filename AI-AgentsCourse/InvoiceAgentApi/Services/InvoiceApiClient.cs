using InvoiceAgentApi.Models;
using System.Text.Json;

namespace InvoiceAgentApi.Services
{
    public class InvoiceApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseUrl = "http://localhost:5000/api/invoices";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public async Task<List<Invoice>> ListInvoices()
        {
            var response = await _httpClient.GetAsync(_baseUrl);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var invoices = JsonSerializer.Deserialize<List<Invoice>>(json, _jsonOptions);
            return invoices ?? new List<Invoice>();
        }

        public async Task<Invoice> FindInvoiceByName(string name)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/by-description?description={Uri.EscapeDataString(name)}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var invoice = JsonSerializer.Deserialize<Invoice>(json, _jsonOptions);
            return invoice!;
        }
    }
}
