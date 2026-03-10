using Microsoft.EntityFrameworkCore;
using MiniAuth.Domain.Entities;
using MiniAuth.Domain.Interfaces;
using MiniAuth.Infrastructure.Data;

namespace MiniAuth.Infrastructure.Repositories;

public class CommentRepository : EFRepositoryBase<Comment, Guid>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context) { }

    public override async ValueTask<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Author)
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Author)
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
