public class Response
{
    public Movie Movie { get; set; } = new Movie();
    public List<string> SimilarMovies { get; set; } = new List<string>();
}