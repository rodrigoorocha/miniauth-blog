using System.Text.Json.Serialization;

namespace MiniAuth.Domain.Entities;

public class Post : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    public string AuthorId { get; set; } = string.Empty;

    [JsonIgnore]
    public virtual ApplicationUser Author { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
}
