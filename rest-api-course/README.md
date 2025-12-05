# From Zero to Hero: REST APIs in .NET

https://dometrain.com/course/from-zero-to-hero-rest-apis-in-asp-net-core/

- Movies.Api - Contains the API Controllers
- Movies.Application - Contains the Business Logic
- Movies.Contracts - Contains the DTOs
- Helpers - Identiy.Api - A simple Identity API for JWT Generation

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

### Adding a Service Layer

Movies.Api
    - Controllers (will be calling the Services)
Movies.Application
    - Services (will be calling the Repositories)
    - Repositories (will be calling the Database)

### Adding Valiadtion

Using FluentValidation.DependencyInjectionExtensions in the Movies.Application layer

MovieValidator.cs
```csharp
public class MovieValidator : AbstractValidator<Movie>
{
    ...
    RuleFor(x => x.Slug)
        .MustAsync(ValidateSlug)
        .WithMessage("This movie already exists in the system!");
    ...
    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);

        if (existingMovie is not null)
        {
            return existingMovie.Id == movie.Id;
        }

        return existingMovie is null;
    }
}
...
```

Use it in the Services by calling ValidateAndThrowAsync before proceeding with the business logic

```csharp
public class MovieService : IMovieService
{
    ...
    private readonly IValidator<Movie> _movieValidator;
    ...
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }
    ...
}
``` 

ValidationMappingMiddleware.cs to map ValidationException to HTTP 400 Bad Request

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var validationFailureResponse = new ValidationFailureResponse 
        {
            Errors = ex.Errors.Select(x => new ValidationResponse 
            {
                PropertyName = x.PropertyName,
                Message = x.ErrorMessage,
            })
        };

        await context.Response.WriteAsJsonAsync(validationFailureResponse);
    }
}
```


Register the Middleware in Program.cs

```csharp
app.UseMiddleware<ValidationMappingMiddleware>();
```

### Cancallation Tokens

Pass CancellationToken from the API Layer to the Application Layer to the Database Layer

```csharp
...
[HttpPost(ApiEndpoints.Movies.Create)]
public async Task<IActionResult> Create([FromBody]CreateMovieRequest request, 
    CancellationToken cancellationToken)
...
MovieService.CreateAsync(Movie movie, CancellationToken cancellationToken)
...
public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
{
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    using var transaction = connection.BeginTransaction();

    var result = await connection.ExecuteAsync(new CommandDefinition(
        """
        INSERT INTO movies (id, title, yearofrelease, slug) 
        VALUES (@Id, @Title, @YearOfRelease, @Slug)
        """, movie, cancellationToken: cancellationToken));
}
...
```

### Authentication and Authorization with JWT

With the use of Microsoft.AspNetCore.Authentication.JwtBearer in Movies.Api layer

```csharp
builder.Services.AddAuthentication(x => 
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => 
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true
    };
    
});
...
builder.Services.AddAuthorization();
...
app.UseAuthentication();
```

Then in Controllers

```csharp
[Authorize]
public class MovieController : ControllerBase
{
    ...
}
```

### Validating claims and limiting actions

```csharp
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("Admin", 
        p => p.RequireClaim("admin", "true"));
    x.AddPolicy("Trusted",
        p => p.RequireAssertion(c => 
            c.User.HasClaim(claim => claim is { Type: "admin", Value: "true"}) ||
            c.User.HasClaim(claim => claim is { Type: "trusted_member", Value: "true" })));
});
```
Then in Controller or endpoint

```csharp
[Authorize(Policy = "Admin")]
public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
{
    ...
}
```

### Extracting UserId from Claims in Token

```csharp
var userId = context.User.Claims.SingleOrDefault(c => c.Type == "userid");

if (Guid.TryParse(userId?.Value, out Guid parsedId))
{
    return parsedId;
}
```

### Passing Query Parameters to Application Layer

```csharp
[HttpGet(ApiEndpoints.Movies.GetAll)]
public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, 
    CancellationToken cancellationToken)
{
    var userId = HttpContext.GetUserId();
    var options = request.MapToOptions()
                        .WithUser(userId);
    var movies = await _movieService.GetAllAsync(options, cancellationToken);
...
}
```

Mapping Query Parameters DTO to Options to pass down to Application Layer 

```csharp
public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest getAllMoviesRequest)
{
    return new GetAllMoviesOptions
    {
        Title = getAllMoviesRequest.Title,
        YearOfRelease = getAllMoviesRequest.Year,
    };
}
```

### HATEOAS implementation

```csharp
    public abstract class HalResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Link> Links { get; set; } = new();
    }

    public class Link
    {
        public required string Href { get; init; }
        public required string Rel { get; init; }
        public string? Type { get; init; }
    }
```

Extends the MovieResponse to include HATEOAS Links

```csharp
public class MovieResponse : HalResponse
{
    ...
}
```

Adding Links in the Controller before returning the response

```csharp
[HttpGet(ApiEndpoints.Movies.Get)]
public async Task<IActionResult> Get([FromRoute] string idOrSlug,
    [FromServices] LinkGenerator linkGenerator,
    CancellationToken cancellationToken)
{
    ...

    var movieResponse = movie.MapToResponse();

    var movieObj = new { id = movie.Id };
    movieResponse.Links.Add(new Link
    {
        Href = linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { idOrSlug = movie.Id }) ?? string.Empty,
        Rel = "self",
        Type = "GET"
    });

    ...
}
```

*HATEOAS unnecessarily bloats the response size and complexity, so it will be undone in the project!*

### Versioning the API

Basic Versioning with URL Segmenting

```csharp
public static class ApiEndpoints
{
    private const string ApiBase = "/api";

    public static class V1
    {
        private const string VersionBase = $"{ApiBase}/v1";
        public static class Movies
        {
            private const string Base = $"{VersionBase}/movies";

            ...
        }
        ...
    }

    public static class V2
    {
        private const string VersionBase = $"{ApiBase}/v2";
        public static class Movies
        {
            private const string Base = $"{VersionBase}/movies";

            ...
        }
        ...
    }
}
```

*This is not an ideal way of versioning as its more manual and bloated*

Advanced Versioning with ```Asp.Versioning.Mvc``` package

In Program.cs with the following changes

```csharp
builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddMvc();
```

Then in Controllers define the version in Header

```csharp
[ApiVersion(1.0, Deprecated = true)]
[HttpGet(ApiEndpoints.Movies.Get)]
public async Task<IActionResult> GetV1([FromRoute] string idOrSlug,
    CancellationToken cancellationToken)
{
    ...
}

...

[ApiVersion(2.0)]
[HttpGet(ApiEndpoints.Movies.Get)]
public async Task<IActionResult> GetV2([FromRoute] string idOrSlug,
    CancellationToken cancellationToken)
{
    ...
}
```

In client side request add the request Header,
```
Accept: application/json; api-version=2.0
```

### Swagger and Versioning

Implementing Versioning with Swagger using ```Asp.Versioning.Mvc.ApiExplorer``` package

```csharp
[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
public class MoviesController : ControllerBase
{
    ...
    [MapToApiVersion(1.0)]
    public async Task<IActionResult> GetV1([FromRoute] string idOrSlug,
        CancellationToken cancellationToken)
    {
        ...
    }

    [MapToApiVersion(2.0)]
    public async Task<IActionResult> GetV2([FromRoute] string idOrSlug,
        CancellationToken cancellationToken)
    {
        ...
    }
}
```

*It is not best to maintain Multiple versions in the same Controller as Swagger can get confusing with the mapping*

Set up the boilerplate code for Custom Swagger Versioning

```csharp
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    ...
}
```

```csharp
public class SwaggerDefaultValues : IOperationFilter
{
    ...
}
```

In Program.cs

```csharp
builder.Services.AddApiVersioning(x =>
{
    ...
}).AddMvc().AddApiExplorer();

builder.Services.AddControllers();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(x => x.OperationFilter<SwaggerDefaultValues>());
```

*It is recommended to keep 1 version per Controller*


### Swagger Authentication

Update the ConfigureSwaggerOptions.cs to include Security Definition and Requirement

```csharp
options.AddSecurityDefinition("Brearer", new OpenApiSecurityScheme()
{
    In = ParameterLocation.Header,
    Description = "Please enter a valid token",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "Bearer"
});

options.AddSecurityRequirement(new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Brearer"
            }
        },
        Array.Empty<string>()
    }
});
```

### Endpoint response types in Swagger 

Exposing the possible return types of Endpoints in Swagger using ```ProducesResponseType``` attribute

```csharp
[Authorize(AuthConstants.TrustedMemberPolicyName)]
[HttpPut(ApiEndpoints.Movies.Update)]
[ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Update(...)
{
    ...
}
```

### Health Checks

In Program.cs

```csharp
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

...
app.MapHealthChecks("_health");
```

Create DatabaseHealthCheck.cs to check the database connectivity

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    ...
}
```

### Response Caching

Client side caching with Response Caching Middleware

In Program.cs

```csharp
builder.Services.AddResponseCaching();
...
app.UseResponseCaching();
```

Then in the Controller add the ResponseCache attribute

```csharp
[HttpGet(ApiEndpoints.Movies.Get)]
[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
...
public async Task<IActionResult> GetV1([FromRoute] string idOrSlug,
    CancellationToken cancellationToken)
{
    ...
}
```

### Output Caching

Server side caching with Output Caching Middleware

In Program.cs

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
    options.AddPolicy("MovieCache",builder =>
    {
        builder.Cache().Expire(TimeSpan.FromMinutes(1))
        .SetVaryByQuery(new[] { "title", "yearOfRelease", "sortBy", "page", "pageSize" })
        .Tag("movies");
    });
});
...

app.UseOutputCache();
```

Then in the Controller add the OutputCache attribute

```csharp
[HttpGet(ApiEndpoints.Movies.GetAll)]
[OutputCache(PolicyName = "MovieCache")]
public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, 
    CancellationToken cancellationToken)
{
    ...
}
```

You need to invalidate the cache when a movie is created, updated or deleted 
Use the IOutputCacheStore to evict the cache by tag

```csharp
public class MoviesController : ControllerBase
{
    ...
    private readonly IOutputCacheStore _outputCacheStore;
    ...


    await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
}
```

### Api Key based Authentication

Authenticate using an API Key passed in the request header

To be used for Service to Service communication

AuthConstants.cs
```csharp
public const string ApiKeyHeaderName = "x-api-key";
```

Implement ApiKeyAuthFilter.cs

```csharp
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    ...

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if(!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, 
            out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key Missing!");
            return;
        }

        var apiKey = _configuration["ApiKey"]!;
        if(!apiKey.Equals(extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key!");
            return;
        }
    }
}   
```

Register the ApiKeyAuthFilter in Program.cs

```csharp
builder.Services.AddScoped<ApiKeyAuthFilter>();
```

Then in the Controller add the ServiceFilter attribute

```csharp
[ServiceFilter(typeof(ApiKeyAuthFilter))]
...
public async Task<IActionResult> Create([FromBody]CreateMovieRequest request, 
    CancellationToken cancellationToken)
{
    ...
}
```

### Mixed Authentication: Either with JWT or API Key

Based on either JWT Bearer Token or API Key in Header

Implement AdminAuthRequirement.cs

```csharp
public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    ...
    
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if(context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }

        var httpContext = context.Resource as HttpContext;
        if(httpContext is null)
        {
            return Task.CompletedTask;
        }

        ...

        var identity = (ClaimsIdentity)httpContext.User.Identity!;
        // Add the admin claim if API Key is valid
        identity.AddClaim(new Claim("userid", "61325d03-5f97-4d46-8ce0-e613a187a94b"));
        context.Succeed(this);
        return Task.CompletedTask;
    }
}
```

Then in Program.cs register the Authorization

```csharp
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy(AuthConstants.AdminUserPolicyName, 
        p => p.AddRequirements(new AdminAuthRequirement(config["ApiKey"]!)));

    ...
});
```

### Creating an SDK for the REST API with Refit

Structure,
- Movies.Api.Sdk - Contains the Refit interfaces for connecting to the API
- Movies.Api.Sdk.Consumer - A console app that consumes the SDK

Install Refit package in Movies.Api.Sdk project

Create IMoviesApi.cs interface

```csharp
public interface IMoviesApi
{
    [Get(ApiEndpoints.Movies.Get)]
    Task<MovieResponse> GetMovieAsync(string idOrSlug);
    ...
}
```

Consume it in the Movies.Api.Sdk.Consumer project

```csharp

var moviesApi = RestService.For<IMoviesApi>("https://localhost:7258");

var movie = await moviesApi.GetMovieAsync("crimson-skies-2015");
```

### Using HttpClient Factory in the SDK Consumer

Install Microsoft.Extensions.DependencyInjection in the Consumer project

Install Refit.HttpClientFactory in the Consumer project

Register the Refit Client with HttpClient Factory

```csharp
var services = new ServiceCollection();
builder.Services.AddRefitClient<IMoviesApi>()
    .ConfigureHttpClient(c => 
    {
        c.BaseAddress = new Uri("https://localhost:7258");
    });
var provider = services.BuildServiceProvider();
```

Then access the IMoviesApi instance when you need it

```csharp
var moviesApi = provider.GetRequiredService<IMoviesApi>();
```

### Handling Token Generation and Refreshing in the SDK Consumer

Add the Authorization Header to the IMoviesApi.cs

```csharp
[Headers("Authorization: Bearer")]
public interface IMoviesApi
{
    ...
}
```

Implement AuthTokenProvider.cs as a simple token provider

```csharp
public class AuthTokenProvider
{
    public async Task<string> GetTokenAsync()
    {
        // Call the Identity API to get the JWT token
        ...
    }
}
```

Then register that in the DI Container and configure Refit to use that for Authorization

```csharp
services
    ...
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => 
        new RefitSettings()
        {
            AuthorizationHeaderValueGetter = async (requestMessage, cancellationToken) =>
            {
                var tokenProvider = s.GetRequiredService<AuthTokenProvider>();
                var token = await tokenProvider.GetTokenAsync();
                return token;
            }
        }).ConfigureHttpClient(...);
```

### Migrating to Minimal APIs

Minimal API concept,

Dedicate 1 class for each endpoint

Structure:

Endpoints
    Movies
        GetMovieEndpoint.cs
        MovieEndpointExtensions.cs
        ...
    Ratings
        ...
EndpointsExtensions.cs

Create EndpointsExtensions.cs

```csharp
public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMovieEndpoints();
        app.MapRoutingEndpoints();
        return app;
    }
}
```

Then register that in Program.cs

```csharp
app.MapApiEndpoints();
```

Create GetMovieEndpoint.cs for replacing the MoviesController.GetV1 method

```csharp
public static class GetMovieEndpoint
{
    public const string Name = "GetMovie";

    public static IEndpointRouteBuilder MapGetMovie(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Get, async (string idOrSlug, IMovieService movieService, 
            HttpContext context, CancellationToken cancellationToken) =>
        {
            // The usual endpoint logic goes here
            ...
        }).WithName(Name);;
        return app;
    }
}
```

Adding Authorization to Minimal API Endpoints with `RequireAuthorization()`

```csharp
...
app.MapDelete(ApiEndpoints.Movies.Delete, async (
    Guid id, IMovieService movieService, IOutputCacheStore outputCacheStore, 
    CancellationToken cancellationToken) =>
{
    ...
}).WithName(Name)
.RequireAuthorization(AuthConstants.AdminUserPolicyName);
```







(THIS IS STILL A WORK IN PROGRESS. MORE TO COME SOON)
