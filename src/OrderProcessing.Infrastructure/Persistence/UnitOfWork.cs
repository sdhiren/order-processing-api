using OrderProcessing.Application.Interfaces;
using OrderProcessing.Infrastructure.Persistence.Repositories;

namespace OrderProcessing.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private IOrderRepository? _orders;

    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
