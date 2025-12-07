# From Zero to Hero: REST APIs in .NET

**Course:** [From Zero to Hero: REST APIs in ASP.NET Core](https://dometrain.com/course/from-zero-to-hero-rest-apis-in-asp-net-core/)

I undertook this course to learn how to build robust and scalable REST APIs using ASP.NET Core. The course covers a wide range of topics, from the basics of setting up a web API to advanced concepts like authentication, authorization, versioning, and caching. 
Towards the end, we explored creating an SDK for the API using Refit. 
Finally, we migrated the entire API to use Minimal APIs.

I followed along with the course instructor, implementing each feature step-by-step. At the same time, I made sure to take notes and note down important code snippets for future reference. I hope this documentation will be helpful for others looking to learn about building REST APIs with ASP.NET Core.

I highly recommend this course to anyone interested in backend development with .NET, it provides a solid foundation for building REST API services!

So, here we go!

### Sneak Peek: Final Working Solution

![Movie.API My Final Working Solution](Screenshots/1%20Movie.API%20final%20working%20solution.jpg)

#### Starting Project Structure

- Movies.Api - Contains the API Controllers
- Movies.Application - Contains the Business Logic
- Movies.Contracts - Contains the DTOs
- Helpers - Identiy.Api - A simple Identity API for JWT Generation

### Use CreatedAtAction instead of Ok() or Created() for easy Location headers

```csharp
return CreatedAtAction(nameof(Get), new { id = movieToCreate.Id }, movieToCreate);
```

### Register the Services in the Application Layer instead in the API Layer

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddSingleton<IMovieRepository, MovieRepository>();
    return services;
}

// Then Add that in the Program.cs in API

builder.Services.AddApplication();
```

### Maintain Contract (DTO) Mapping at the API Layer
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

### Use API Routes for defining the endpoint paths

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

![CRUD Operations](Screenshots/1%20CRUD%20Operations.jpg)

### Adding a Service Layer

Movies.Api
    - Controllers (will be calling the Services)
Movies.Application
    - Services (will be calling the Repositories)
    - Repositories (will be calling the Database)

### Adding Valiadtion

![Data Validation](Screenshots/2%20Data%20Validation.jpg)

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

![Authentication](Screenshots/3%20Authentication.jpg)

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

![Endpoints with Auth](Screenshots/4%20Endpoints%20with%20Auth.jpg)

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

![Endpoints with Query Params](Screenshots/5%20Endpoints%20with%20Query%20Params.jpg)

![Endpoints with Query Params and Sorting](Screenshots/5%20Endpoints%20with%20Query%20Params%20w%20Sorting.jpg)

![Pagination](Screenshots/5%20Pagination.jpg)

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

![HATEOAS approach](Screenshots/6%20HATEOAS%20approach.jpg)

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

![Basic Versioning](Screenshots/7%20Basic%20Versioning.jpg)

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

![Advanced Versioning](Screenshots/8%20Advanced%20Versioning.jpg)

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

![Advanced Versioning with Swagger](Screenshots/9%20Advanced%20Versioning%20with%20Swagger.jpg)

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

![Endpoint Response types with Swagger](Screenshots/10%20Endpoint%20Response%20types%20with%20Swagger.jpg)

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

![Healthcheck](Screenshots/11%20Healthcheck.jpg)

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

![Response Caching](Screenshots/12%20Response%20Caching.jpg)

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

![Multiple Auth Schemes](Screenshots/13%20Multiple%20Auth%20Schemes.jpg)

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

![Creating an SDK for API and Consuming it with Refit](Screenshots/14%20Creating%20an%20SDK%20for%20API%20and%20Consuming%20it%20with%20Refit.jpg)

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

![Consuming all the endpoints from Console Client app](Screenshots/15%20Consuming%20all%20the%20endpoints%20from%20Console%20Client%20app.jpg)

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

![Migrating to Minimal API Endpoints](Screenshots/16%20Migrating%20to%20Minimal%20API%20Endpoints.jpg)


#### Adding Authorization with `RequireAuthorization()`

```csharp
...
app.MapDelete(ApiEndpoints.Movies.Delete, async (
    ... ) =>
{
    ...
}).WithName(Name)
.RequireAuthorization(AuthConstants.AdminUserPolicyName);
```

#### Adding Swagger support 

Remove Asp.Versioning.Mvc
Add Asp.Versioning.Http package

Then in Program.cs

Remove AddMvc() call and add the following

```csharp
...
builder.Services.AddEndpointsApiExplorer();
...
```

#### Adding return types for Swagger with `Produces<T>()`

```csharp
app.MapPut(ApiEndpoints.Movies.Update, async (
    Guid id, UpdateMovieRequest request, 
    ... ) =>
{
    ...
}).WithName(Name)
.Produces<MoviesResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
.RequireAuthorization(AuthConstants.TrustedMemberPolicyName);
```

#### Handling Versioning

Create ApiVersioning.cs

```csharp
public static class ApiVersioning
{
    public static ApiVersionSet VersionSet { get; private set; }

    public static IEndpointRouteBuilder CreateApiVersionSet(this IEndpointRouteBuilder app)
    {
        VersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1.0))
            .HasApiVersion(new ApiVersion(2.0))
            .ReportApiVersions()
            .Build();

        return app;
    }
}
```

Thn in Program.cs call CreateApiVersionSet before mapping the endpoints

```csharp
...
var app = builder.Build();

app.CreateApiVersionSet();
...
```

Then in the Endpoint declaration add the versioning info

```csharp
public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
{
    app.MapGet(ApiEndpoints.Movies.GetAll, async ([AsParameters] GetAllMoviesRequest request,
        ... ) =>
    {
        ...
    }).WithName($"{Name}V1")
    .WithApiVersionSet(ApiVersioning.VersionSet)
    .HasApiVersion(1.0);

    app.MapGet(ApiEndpoints.Movies.GetAll, async ([AsParameters] GetAllMoviesRequest request,
        ... ) =>
    {
       ...
    }).WithName($"{Name}V2")
    .WithApiVersionSet(ApiVersioning.VersionSet)
    .HasApiVersion(2.0);
    return app;
}
```

#### Adding Caching

```csharp
app.MapGet(ApiEndpoints.Movies.Get, async (string idOrSlug, IMovieService movieService, 
    ...) =>
{
    ...
}).WithName(Name)
.CacheOutput("MovieCache");
```

![Minimal API Structure](Screenshots/17%20Minimal%20API%20Structure.jpg)

I created a separated MovieApi.Minimal project folder for the Minimal API implementation

Also created two separate Build Configurations in the solution,
- Movie.Api + Identity.Api - for the full controller based API
- Movie.Api.Minimal + Identity.Api - for the Minimal API implementation

You can run either of the configurations to test the respective API implementations.

That's the end of the REST API course!

![Course Completion Certificate](Screenshots/18%20Certificate-iZnUZBMkAU9AS1.png)

Cheers!
