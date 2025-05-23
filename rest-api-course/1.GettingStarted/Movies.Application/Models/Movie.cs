using System.Text.RegularExpressions;

namespace Movies.Application.Models
{
    public partial class Movie
    {
        public required Guid Id { get; init; }
        public required string Title { get; set; }
        public int YearOfRelease { get; set; }
        public string Slug => GenerateSlug();
        public required List<string> Genres { get; init; } = new List<string>();
        
        private string GenerateSlug()
        {
            var sluggetedTitle = SlugRegex().Replace(Title, string.Empty)
                .ToLower().Replace(' ', '-');
            return $"{sluggetedTitle}-{YearOfRelease}";
        }

        [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
        private static partial Regex SlugRegex();
    }
}
