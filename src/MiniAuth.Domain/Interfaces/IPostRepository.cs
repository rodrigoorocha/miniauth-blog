using MiniAuth.Domain.Entities;
using MiniAuth.Domain.QueryObjects;

namespace MiniAuth.Domain.Interfaces;

public interface IPostRepository : IReadRepository<Post, Guid>, IWriteRepository<Post>
{
    Task<PaginatedResult<Post>> GetPaginatedAsync(PostQuery query, int pageIndex, int pageSize, CancellationToken ct = default);
}
