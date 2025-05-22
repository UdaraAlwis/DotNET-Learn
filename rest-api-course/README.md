# From Zero to Hero: REST APIs in .NET

https://dometrain.com/course/from-zero-to-hero-rest-apis-in-asp-net-core/

Movies.Api - Contains the API Controllers
Movies.Application - Contains the Business Logic
Movies.Contracts - Contains the DTOs


#### - Use CreatedAtAction instead of Ok() or Created()

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
```

