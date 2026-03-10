using MiniAuth.Domain.Entities;

namespace MiniAuth.Domain.Interfaces;

public interface ICommentRepository : IReadRepository<Comment, Guid>, IWriteRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, CancellationToken ct = default);
}
