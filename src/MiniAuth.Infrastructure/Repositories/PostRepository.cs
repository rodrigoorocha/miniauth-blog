using Microsoft.EntityFrameworkCore;
using MiniAuth.Domain.Entities;
using MiniAuth.Domain.Interfaces;
using MiniAuth.Domain.QueryObjects;
using MiniAuth.Infrastructure.Data;

namespace MiniAuth.Infrastructure.Repositories;

public class PostRepository : EFRepositoryBase<Post, Guid>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context) { }

    public override async ValueTask<Post?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PaginatedResult<Post>> GetPaginatedAsync(PostQuery query, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var queryable = DbSet
            .Include(p => p.Author)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query.AuthorId))
            queryable = queryable.Where(p => p.AuthorId == query.AuthorId);

        if (!string.IsNullOrEmpty(query.Title))
            queryable = queryable.Where(p => p.Title.Contains(query.Title));

        if (query.IsPublished.HasValue)
            queryable = queryable.Where(p => p.IsPublished == query.IsPublished.Value);

        if (query.From.HasValue)
            queryable = queryable.Where(p => p.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            queryable = queryable.Where(p => p.CreatedAt <= query.To.Value);

        var totalCount = await queryable.LongCountAsync(ct);

        var items = await queryable
            .OrderByDescending(p => p.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Post>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount,
            Data = items
        };
    }
}
