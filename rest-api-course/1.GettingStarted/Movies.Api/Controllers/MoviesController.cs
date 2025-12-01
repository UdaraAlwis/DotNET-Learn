using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [ApiVersion(2.0)]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody]CreateMovieRequest request, 
            CancellationToken cancellationToken)
        {
            var movieToCreate = request.ToMovie();
            await _movieService.CreateAsync(movieToCreate, cancellationToken);
            return CreatedAtAction(nameof(GetV2), new { idOrSlug = movieToCreate.Id }, movieToCreate);
        }

        [MapToApiVersion(1.0)]
        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> GetV1([FromRoute] string idOrSlug,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var movie = Guid.TryParse(idOrSlug, out var id) ? 
                await _movieService.GetByIdAsync(id, userId, cancellationToken) :
                await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
            if(movie is null)
            {
                return NotFound();
            }

            var movieResponse = movie.MapToResponse();
            return Ok(movieResponse);
        }

        [MapToApiVersion(2.0)]
        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> GetV2([FromRoute] string idOrSlug,
         CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var movie = Guid.TryParse(idOrSlug, out var id) ?
                await _movieService.GetByIdAsync(id, userId, cancellationToken) :
                await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
            if (movie is null)
            {
                return NotFound();
            }

            var movieResponse = movie.MapToResponse();
            return Ok(movieResponse);
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, 
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var options = request.MapToOptions()
                                .WithUser(userId);
            var movies = await _movieService.GetAllAsync(options, cancellationToken);
            var moviesCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);

            var moviesResponse = movies?.MapToResponse(request.Page, request.PageSize, moviesCount);
            return Ok(moviesResponse);
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request,
                CancellationToken cancellationToken)
        {
            var movieToUpdate = request.ToMovie(id);
            var userId = HttpContext.GetUserId();
            var updatedMovie = await _movieService.UpdateAsync(movieToUpdate, userId, cancellationToken);
            if (updatedMovie == null)
                return NotFound();

            var response = movieToUpdate.MapToResponse();
            return Ok(response);
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _movieService.DeleteByIdAsync(id, cancellationToken);
            if (!result)
                return NotFound();

            return Ok(result);
        }
    }
}
