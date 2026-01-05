using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;
using System.Text.Json;

var services = new ServiceCollection();

services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => 
        new RefitSettings()
        {
            // Register the service to retrieve auth token
            AuthorizationHeaderValueGetter = async (requestMessage, cancellationToken) =>
            {
                var tokenProvider = s.GetRequiredService<AuthTokenProvider>();
                var token = await tokenProvider.GetTokenAsync();
                return token;
            }
        }).ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7258"));

var provider = services.BuildServiceProvider();

// Get the IMoviesApi service
var moviesApi = provider.GetRequiredService<IMoviesApi>();


Console.WriteLine("Create new Movie: Dalmations 101");

// Create a new movie
var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest()
{
    Title = "Dalmations 101",
    Genres = new List<string> { "Animation", "Family" },
    YearOfRelease = 2024,
});

Console.WriteLine("Update the Movie: Dalmations 101");

// Update the movie
var updatedMovie = await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest()
{
    Title = "Dalmations 101",
    Genres = new List<string> { "Animation", "Family", "Adventure" },
    YearOfRelease = 2024,
});

Console.WriteLine("Retrieve the Movie by slug: dalmations-101-2024");

// Get the movie by slug
var movie = await moviesApi.GetMovieAsync("dalmations-101-2024");

Console.WriteLine(JsonSerializer.Serialize(movie));

// Get all the movies
Console.WriteLine("Retrieve all the Movies");

var movies = await moviesApi.GetMoviesAsync(new GetAllMoviesRequest() 
{
    Title = null,
    YearOfRelease = null,
    SortBy = null,
    Page = 1,
    PageSize = 10
});

foreach (var item in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(item));
}

Console.WriteLine("Delete the Movie: Dalmations 101");

// Delete the movie
await moviesApi.DeleteMovieAsync(updatedMovie.Id);

// Get all the movies
Console.WriteLine("Retrieve all the Movies");

var moviesFinal = await moviesApi.GetMoviesAsync(new GetAllMoviesRequest()
{
    Title = null,
    YearOfRelease = null,
    SortBy = null,
    Page = 1,
    PageSize = 10
});

foreach (var item in moviesFinal.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(item));
}

Console.ReadLine();