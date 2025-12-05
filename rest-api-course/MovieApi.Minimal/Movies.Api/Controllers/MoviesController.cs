using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers
{
    //[ApiController]
    //[ApiVersion(1.0)]
    //public class MoviesController : ControllerBase
    //{
    //    private readonly IMovieService _movieService;
    //    private readonly IOutputCacheStore _outputCacheStore;

    //    public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
    //    {
    //        _movieService = movieService;
    //        _outputCacheStore = outputCacheStore;
    //    }

    //    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    //    [HttpPost(ApiEndpoints.Movies.Create)]
    //    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    //    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    //    public async Task<IActionResult> Create([FromBody]CreateMovieRequest request, 
    //        CancellationToken cancellationToken)
    //    {
    //        var movieToCreate = request.ToMovie();
    //        await _movieService.CreateAsync(movieToCreate, cancellationToken);
    //        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

    //        var response = movieToCreate.MapToResponse();

    //        return CreatedAtAction(nameof(GetV1), new { idOrSlug = movieToCreate.Id }, response);
    //    }

    //    [HttpGet(ApiEndpoints.Movies.Get)]
    //    [OutputCache]
    //    //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    //    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    public async Task<IActionResult> GetV1([FromRoute] string idOrSlug,
    //        CancellationToken cancellationToken)
    //    {
    //        var userId = HttpContext.GetUserId();
    //        var movie = Guid.TryParse(idOrSlug, out var id) ? 
    //            await _movieService.GetByIdAsync(id, userId, cancellationToken) :
    //            await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
    //        if(movie is null)
    //        {
    //            return NotFound();
    //        }

    //        var movieResponse = movie.MapToResponse();
    //        return Ok(movieResponse);
    //    }

    //    [HttpGet(ApiEndpoints.Movies.GetAll)]
    //    [OutputCache(PolicyName = "MovieCache")]
    //    //[ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "title", "yearOfRelease", "sortBy", "page", "pageSize" }, Location = ResponseCacheLocation.Any)]
    //    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    //    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, 
    //        CancellationToken cancellationToken)
    //    {
    //        var userId = HttpContext.GetUserId();
    //        var options = request.MapToOptions()
    //                            .WithUser(userId);
    //        var movies = await _movieService.GetAllAsync(options, cancellationToken);
    //        var moviesCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);

    //        //var moviesResponse = movies?.MapToResponse(request.Page, request.PageSize, moviesCount);
    //        //return Ok(moviesResponse);

    //        return Ok();
    //    }

    //    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    //    [HttpPut(ApiEndpoints.Movies.Update)]
    //    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    //    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request,
    //            CancellationToken cancellationToken)
    //    {
    //        var movieToUpdate = request.ToMovie(id);
    //        var userId = HttpContext.GetUserId();
    //        var updatedMovie = await _movieService.UpdateAsync(movieToUpdate, userId, cancellationToken);
    //        if (updatedMovie == null)
    //            return NotFound();

    //        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
    //        var response = movieToUpdate.MapToResponse();
    //        return Ok(response);
    //    }

    //    [Authorize(AuthConstants.AdminUserPolicyName)]
    //    [HttpDelete(ApiEndpoints.Movies.Delete)]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    //    {
    //        var result = await _movieService.DeleteByIdAsync(id, cancellationToken);
    //        if (!result)
    //            return NotFound();

    //        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
    //        return Ok(result);
    //    }
    //}
}
