using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers
{
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;

        public MovieController(IMovieRepository movieRepository)
        {
            this._movieRepository = movieRepository;
        }

        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
        {
            var movieToCreate = request.ToMovie();
            await _movieRepository.CreateMovieAsync(movieToCreate);
            return CreatedAtAction(nameof(Get), new { id = movieToCreate.Id }, movieToCreate);
        }

        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug)
        {
            var movie = Guid.TryParse(idOrSlug, out var id) ? await _movieRepository.GetMovieByIdAsync(id) 
                : await _movieRepository.GetMovieBySlugAsync(idOrSlug);
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
            var movies = await _movieRepository.GetAllAsync();
            return Ok(movies);
        }

        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
        {
            var movieToUpdate = request.ToMovie(id);
            var result = await _movieRepository.UpdateMovieAsync(movieToUpdate);
            if (!result)
                return NotFound();

            var updatedMovie = movieToUpdate.ToMovieResponse();
            return Ok(updatedMovie);
        }

        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _movieRepository.DeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return Ok(result);
        }
    }
}
