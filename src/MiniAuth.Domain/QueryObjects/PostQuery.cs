namespace MiniAuth.Domain.QueryObjects;

public class PostQuery
{
    public string? AuthorId { get; set; }
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
