using System.Text.Json;

namespace ConsoleAgentChatApp.Services
{
    public class WeatherService (string apiKey)
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<string[]> GetWeatherInCity(string city, CancellationToken cancellationToken)
        {
            var url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={Uri.EscapeDataString(city)}&aqi=no";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if(!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to get weather data: {responseContent}");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            var descriptionElement = root.GetProperty("current").GetProperty("condition").GetProperty("text");

            string[] descriptions = [descriptionElement.GetString()!];
            return descriptions;
        }
    }
}
