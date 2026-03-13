using MiniAuth.Application.DTOs;
using MiniAuth.Application.Interfaces;
using MiniAuth.Application.Requests;
using MiniAuth.Domain.Entities;
using MiniAuth.Domain.Exceptions;
using MiniAuth.Domain.Interfaces;

namespace MiniAuth.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IEventPublisher _eventPublisher;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        ICurrentUser currentUser,
        IEventPublisher eventPublisher)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _currentUser = currentUser;
        _eventPublisher = eventPublisher;
    }

    public async Task<CommentDto> CreateAsync(Guid postId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, ct)
            ?? throw new NotFoundException(nameof(Post), postId);

        var comment = new Comment
        {
            Content = request.Content,
            PostId = post.Id,
            AuthorId = _currentUser.UserId
        };

        _commentRepository.Add(comment);
        await _commentRepository.SaveChangesAsync(ct);

        // Publica evento pro Worker enviar email ao autor do post
        await _eventPublisher.PublishAsync(new Domain.Events.CommentCreatedEvent
        {
            CommentId = comment.Id,
            PostId = post.Id,
            PostTitle = post.Title,
            PostAuthorEmail = post.Author?.Email ?? "",
            CommenterName = _currentUser.FullName
        }, ct);

        return CommentDto.FromEntity(comment);
    }

    public async Task<IEnumerable<CommentDto>> ListByPostAsync(Guid postId, CancellationToken ct = default)
    {
        var comments = await _commentRepository.GetByPostIdAsync(postId, ct);

        return comments.Select(CommentDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Comment), id);

        if (comment.AuthorId != _currentUser.UserId)
            throw new DomainException("You can only delete your own comments.");

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync(ct);
    }
}
