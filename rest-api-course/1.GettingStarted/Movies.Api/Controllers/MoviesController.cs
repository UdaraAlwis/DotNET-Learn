using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers
{
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            this._movieService = movieService;
        }

        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
        {
            var movieToCreate = request.ToMovie();
            await _movieService.CreateAsync(movieToCreate);
            return CreatedAtAction(nameof(Get), new { idOrSlug = movieToCreate.Id }, movieToCreate);
        }

        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug)
        {
            var movie = Guid.TryParse(idOrSlug, out var id) ? await _movieService.GetByIdAsync(id) 
                : await _movieService.GetBySlugAsync(idOrSlug);
            if(movie is null)
            {
                return NotFound();
            }

            var movieResponse = movie.ToMovieResponse();
            return Ok(movieResponse);
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _movieService.GetAllAsync();
            return Ok(movies);
        }

        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
        {
            var movieToUpdate = request.ToMovie(id);
            var updatedMovie = await _movieService.UpdateAsync(movieToUpdate);
            if (updatedMovie == null)
                return NotFound();

            var response = movieToUpdate.ToMovieResponse();
            return Ok(response);
        }

        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _movieService.DeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return Ok(result);
        }
    }
}
