namespace MiniAuth.Domain.Interfaces;

public interface IReadRepository<TEntity, in TKey> : IDisposable where TEntity : class
{
    ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    IQueryable<TEntity> GetAll();
}
