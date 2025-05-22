namespace Movies.Contracts.Responses
{
    public class MovieResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int YearOfRelease { get; init; }
        public IEnumerable<string> Genres { get; init; } = Enumerable.Empty<string>();
    }
}
