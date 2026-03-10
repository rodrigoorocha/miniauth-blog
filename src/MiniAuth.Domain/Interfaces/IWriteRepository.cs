namespace MiniAuth.Domain.Interfaces;

public interface IWriteRepository<in TEntity> : IDisposable where TEntity : class
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
