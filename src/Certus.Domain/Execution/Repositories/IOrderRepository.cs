using Certus.Domain.Execution.Entities;

namespace Certus.Domain.Execution.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Order>> GetByTradeIdAsync(Guid tradeId);
    Task AddAsync(Order order);
    void Update(Order order);
    Task<int> SaveChangesAsync();
}
