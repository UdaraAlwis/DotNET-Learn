# From Zero to Hero: REST APIs in .NET

https://dometrain.com/course/from-zero-to-hero-rest-apis-in-asp-net-core/

Movies.Api - Contains the API Controllers
Movies.Application - Contains the Business Logic
Movies.Contracts - Contains the DTOs


#### - Use CreatedAtAction instead of Ok() or Created() for easy Location headers

```csharp
return CreatedAtAction(nameof(Get), new { id = movieToCreate.Id }, movieToCreate);
```

#### - Register the Services in the Application Layer instead in the API Layer

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddSingleton<IMovieRepository, MovieRepository>();
    return services;
}

// Then Add that in the Program.cs in API

builder.Services.AddApplication();
```

#### - Maintain Contract (DTO) Mapping at the API Layer
```csharp
public static MovieResponse ToMovieResponse(this Movie movie)
{
    return new MovieResponse
    {
        Id = movie.Id,
        Title = movie.Title,
        YearOfRelease = movie.YearOfRelease,
        Genres = movie.Genres
    };
}
```

#### - Use API Routes for defining the endpoint paths

```csharp
public static class ApiEndpoints
{
    private const string ApiBase = "/api";

    public static class Movies
    {
        private const string Base = $"{ApiBase}/movies";
        ...
    }
    ...
} 

// Then to register the endpoints

[ApiController]
public class MovieController : ControllerBase
{
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
    {
        ...
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        ...
    }
}
```

### Install and Set up WSL and Docker Desktop

```bash
wsl --install
```
then install docker desktop
https://apps.microsoft.com/detail/xp8cbj40xlbwkx?hl=en-US&gl=US

### Set up Database Connectivity

```csharp
public class NgpsqlConnectionFactory : IDbConnectionFactory
{
    ...
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}

// Register the IDbConnectionFactory in the Application Layer instead in the API Layer

public static class ApplicationServiceCollectionExtension
{
    ...
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new NgpsqlConnectionFactory(connectionString));
        services.AddSingleton<DbInitializer>();
        return services;
    }
}

// Then Add that in the Program.cs in API
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

// Make sure to add DB init in Program.cs in API
var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();
```