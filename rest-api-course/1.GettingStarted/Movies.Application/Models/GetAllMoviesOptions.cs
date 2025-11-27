namespace Movies.Application.Models
{
    public class GetAllMoviesOptions
    {
        public required string? Title { get; init; }
        public required int? YearOfRelease { get; init; }
        public Guid? UserId { get; set; }
    }
}
