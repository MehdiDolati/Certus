using Certus.Domain.Execution.Entities;
using Certus.Domain.Execution.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Execution;

public class OrderRepository : IOrderRepository
{
    private readonly CertusDbContext _db;

    public OrderRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<Order?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<Order?>(null);
    }

    public Task<IReadOnlyList<Order>> GetByTradeIdAsync(Guid tradeId)
    {
        return Task.FromResult<IReadOnlyList<Order>>(new List<Order>());
    }

    public Task AddAsync(Order order)
    {
        return Task.CompletedTask;
    }

    public void Update(Order order) { }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
