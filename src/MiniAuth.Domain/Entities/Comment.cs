using System.Text.Json.Serialization;

namespace MiniAuth.Domain.Entities;

public class Comment : Entity
{
    public string Content { get; set; } = string.Empty;

    public Guid PostId { get; set; }

    [JsonIgnore]
    public virtual Post Post { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;

    [JsonIgnore]
    public virtual ApplicationUser Author { get; set; } = null!;
}
