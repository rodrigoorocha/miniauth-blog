using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace MiniAuth.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();

    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
}
