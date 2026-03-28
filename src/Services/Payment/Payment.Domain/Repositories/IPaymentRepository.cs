using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Payment.Domain.Entities;

namespace NovaCart.Services.Payment.Domain.Repositories;

public interface IPaymentRepository : IRepository<PaymentRecord>
{
    Task<PaymentRecord?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
