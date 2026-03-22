namespace ConsoleAgentChatApp.Services
{
    public class WardrobeService
    {
        public Task<string[]> ListClothing()
        {
            return Task.FromResult<string[]>([
                "Blue Jeans",
                "White T-Shirt",
                "Black Jacket",
                "Red Dress",
                "Sneakers",
                "Sandals",
                "Sweater",
                "Shorts",
                "Skirt",
                "Blouse",
                "Hat",
                "Scarf",
                "Gloves"
            ]);
        }
    }
}
