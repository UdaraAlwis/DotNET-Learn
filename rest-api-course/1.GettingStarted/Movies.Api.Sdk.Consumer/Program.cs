
using Movies.Api.Sdk;
using Movies.Contracts.Requests;
using Refit;
using System.Text.Json;

var moviesApi = RestService.For<IMoviesApi>("https://localhost:7258");

//var movie = await moviesApi.GetMovieAsync("crimson-skies-2015");

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

Console.ReadLine();