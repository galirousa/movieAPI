using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using movieapi.Data;
using movieapi;

namespace movieapi.Services
{
    public interface IMovieService
    {
        Task<Response> SearchMoviesAsync(string query);
    }

    public class MovieService : IMovieService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.themoviedb.org/3/";
        private readonly MovieDbContext _context;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IHttpClientFactory httpClientFactory, IConfiguration configuration,
                          MovieDbContext context, ILogger<MovieService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["MovieApiAccessToken"] ?? throw new ArgumentNullException(nameof(configuration), "MovieApiAccessToken es requerido");
            _context = context;
            _logger = logger;
        }

        public async Task<Response> SearchMoviesAsync(string query)
        {
            try
            {
                // Primero buscar en la base de datos
                var cachedMovie = await SearchMoviesInDatabaseAsync(query);
                if (cachedMovie != null )
                {
                    _logger.LogInformation($"Se encontro {cachedMovie.Title} en caché para la búsqueda: {query}");
                    // Buscar películas similares
                    var similar = await SearchSimilarMovies(cachedMovie!.TmdbId.ToString());
                    if (similar == null || !similar.Any())
                    {
                        _logger.LogWarning($"No se encontraron películas similares para: {cachedMovie.TmdbId}");
                        return new Response { Movie = cachedMovie };
                    }

                    var similarToCached = new List<string>();
                    similarToCached.AddRange(similar);
                    return new Response { Movie = cachedMovie, SimilarMovies = similarToCached };
                }

                // Si no hay películas en caché o están desactualizadas, hacer petición a la API
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_baseUrl}search/movie?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page=1"),
                    Headers = {
                        { "accept", "application/json" },
                        { "Authorization", $"Bearer {_apiKey}" },
                    },
                };

                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadFromJsonAsync<MovieResponse>();

                if (body?.Results == null) return new Response();

                // Guardar película en la base de datos
                var savedMovie = await SaveOrUpdateMovieAsync(body.Results.FirstOrDefault()!);
                _logger.LogInformation($"Se obtuvo y guardo: {query}");

                var result = new List<Movie> { savedMovie };
                // Buscar películas similares
                var similarMovies = await SearchSimilarMovies(savedMovie.TmdbId.ToString());
                if (similarMovies == null || !similarMovies.Any())
                {
                    _logger.LogWarning($"No se encontraron películas similares para: {savedMovie.TmdbId}");
                    return new Response { Movie = savedMovie };
                }

                return new Response { Movie = savedMovie, SimilarMovies = similarMovies };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error buscando películas con la consulta: {query}");
                throw;
            }
        }

        private async Task<List<string>> SearchSimilarMovies(string movieId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_baseUrl}/movie/{Uri.EscapeDataString(movieId)}/similar?include_adult=false&language=en-US&page=1"),
                    Headers = {
                        { "accept", "application/json" },
                        { "Authorization", $"Bearer {_apiKey}" },
                    },
                };

                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadFromJsonAsync<MovieResponse>();

                if (body?.Results == null) return new List<string>();

                // Guardar película en la base de datos
                var similarMovies = new List<string>();
                foreach (var movie in body.Results.Take(5)) // Limitar a 5 resultados
                {
                    var savedMovie = await SaveOrUpdateMovieAsync(movie);
                    similarMovies.Add(savedMovie.Title + " (" + savedMovie.ReleaseDate?.Year + ")");
                }

                _logger.LogInformation($"Se obtuvieron {similarMovies.Count} películas similares");
                return similarMovies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error buscando películas con la consulta: {movieId}");
                throw;
            }
        }

        private async Task<Movie?> SearchMoviesInDatabaseAsync(string query)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-24);

                var recentMovie = await _context.Movies
                    .Where(m => (m.Title.ToLower().Contains(query.ToLower()) ||
                                (m.OriginalTitle != null && m.OriginalTitle.ToLower().Contains(query.ToLower()))) &&
                               m.LastUpdated >= cutoffTime)
                    .OrderByDescending(m => m.LastUpdated)
                    .Take(1)
                    .FirstOrDefaultAsync();
                return recentMovie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error buscando películas en la base de datos para la consulta: {query}");
                return null;
            }
        }

        private async Task<Movie> SaveOrUpdateMovieAsync(Movie movie)
        {
            try
            {
                var existingMovie = await _context.Movies
                    .FirstOrDefaultAsync(m => m.TmdbId == movie.TmdbId);

                if (existingMovie != null)
                {
                    // Actualizar película existente
                    existingMovie.Title = movie.Title;
                    existingMovie.OriginalTitle = movie.OriginalTitle;
                    existingMovie.Overview = movie.Overview;
                    existingMovie.ReleaseDateString = movie.ReleaseDateString;
                    existingMovie.PosterPath = movie.PosterPath;
                    existingMovie.BackdropPath = movie.BackdropPath;
                    existingMovie.Adult = movie.Adult;
                    existingMovie.GenreIds = movie.GenreIds;
                    existingMovie.OriginalLanguage = movie.OriginalLanguage;
                    existingMovie.Popularity = movie.Popularity;
                    existingMovie.VoteAverage = movie.VoteAverage;
                    existingMovie.VoteCount = movie.VoteCount;
                    existingMovie.Video = movie.Video;
                    existingMovie.LastUpdated = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return existingMovie;
                }
                else
                {
                    // Crear nueva película
                    movie.CreatedAt = DateTime.UtcNow;
                    movie.LastUpdated = DateTime.UtcNow;
                    _context.Movies.Add(movie);
                    await _context.SaveChangesAsync();
                    return movie;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error guardando película con ID de TMDb: {movie.TmdbId}");
                throw;
            }
        }
    }
}