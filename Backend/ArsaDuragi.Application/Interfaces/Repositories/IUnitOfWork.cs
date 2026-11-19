namespace ArsaDuragi.Application.Interfaces.Repositories;

/// <summary>Commits all pending changes made through the repositories in one database transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
