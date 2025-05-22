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
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var movie = await _movieRepository.GetMovieByIdAsync(id);
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
    }
}
