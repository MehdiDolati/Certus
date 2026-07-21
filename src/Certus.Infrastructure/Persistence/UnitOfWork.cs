using Certus.Domain.SharedKernel;

namespace Certus.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CertusDbContext _db;

    public UnitOfWork(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
