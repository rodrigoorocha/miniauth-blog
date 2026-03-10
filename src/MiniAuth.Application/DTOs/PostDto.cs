using MiniAuth.Domain.Entities;

namespace MiniAuth.Application.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CommentsCount { get; set; }

    public static PostDto FromEntity(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            IsPublished = post.IsPublished,
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.FullName ?? string.Empty,
            CommentsCount = post.Comments?.Count ?? 0
        };
    }
}
