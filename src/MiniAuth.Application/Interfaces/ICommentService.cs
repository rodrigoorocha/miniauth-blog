using MiniAuth.Application.DTOs;
using MiniAuth.Application.Requests;

namespace MiniAuth.Application.Interfaces;

public interface ICommentService
{
    Task<CommentDto> CreateAsync(Guid postId, CreateCommentRequest request, CancellationToken ct = default);
    Task<IEnumerable<CommentDto>> ListByPostAsync(Guid postId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
