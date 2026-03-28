using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Infrastructure.Persistence;

namespace NovaCart.Services.Payment.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;

    public PaymentRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<PaymentRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(PaymentRecord entity)
    {
        _dbContext.Payments.Add(entity);
    }

    public void Update(PaymentRecord entity)
    {
        _dbContext.Payments.Update(entity);
    }

    public void Delete(PaymentRecord entity)
    {
        _dbContext.Payments.Remove(entity);
    }

    public async Task<PaymentRecord?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }
}
