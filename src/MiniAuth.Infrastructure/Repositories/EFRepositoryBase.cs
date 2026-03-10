using Microsoft.EntityFrameworkCore;
using MiniAuth.Domain.Entities;
using MiniAuth.Domain.Interfaces;
using MiniAuth.Infrastructure.Data;

namespace MiniAuth.Infrastructure.Repositories;

public abstract class EFRepositoryBase<TEntity, TKey> : IReadRepository<TEntity, TKey>, IWriteRepository<TEntity>
    where TEntity : Entity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected EFRepositoryBase(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id! }, ct);
    }

    public virtual IQueryable<TEntity> GetAll()
    {
        return DbSet.AsNoTracking();
    }

    public void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await Context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
