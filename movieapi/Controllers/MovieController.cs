using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using movieapi.Services;
using movieapi;

namespace movieapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Movie>>> Search([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Search title cannot be empty");
            }

            var result = await _movieService.SearchMoviesAsync(title);
            return Ok(result);
        }
    }
}