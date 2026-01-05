namespace Movies.Application.Models
{
    public class MovieRating
    {
        public required Guid MovieId { get; init; }
        public required String Slug { get; init; }
        public required int Rating { get; init; }
    }
}
