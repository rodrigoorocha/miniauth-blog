namespace MiniAuth.Domain.Events;

public class CommentCreatedEvent : DomainEvent
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public string PostAuthorEmail { get; set; } = string.Empty;
    public string CommenterName { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
}
