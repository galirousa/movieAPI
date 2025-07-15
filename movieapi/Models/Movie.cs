using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using movieapi;

public class Movie
{
    
    [JsonPropertyName("id")]
    public int TmdbId { get; set; }
    
    [JsonPropertyName("adult")]
    public bool Adult { get; set; }
    
    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }
    
    [JsonPropertyName("genre_ids")]
    public List<int>? GenreIds { get; set; }
    
    [JsonPropertyName("original_language")]
    public string? OriginalLanguage { get; set; }
    
    [JsonPropertyName("original_title")]
    public string? OriginalTitle { get; set; }
    
    [JsonPropertyName("overview")]
    public string? Overview { get; set; }
    
    [JsonPropertyName("popularity")]
    public decimal Popularity { get; set; }
    
    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
    
    [JsonPropertyName("release_date")]
    public string? ReleaseDateString { get; set; }
    
    // Database-specific property for proper date handling
    [NotMapped]
    public DateTime? ReleaseDate 
    { 
        get => DateTime.TryParse(ReleaseDateString, out var date) ? date : null;
        set => ReleaseDateString = value?.ToString("yyyy-MM-dd");
    }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("video")]
    public bool Video { get; set; }
    
    [JsonPropertyName("vote_average")]
    public decimal VoteAverage { get; set; }
    
    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }
    
    // Database-specific tracking properties
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}