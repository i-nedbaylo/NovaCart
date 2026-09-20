using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Application.Consumers;
using NovaCart.Services.Payment.Application.Options;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Domain.ValueObjects;
namespace NovaCart.Tests.Payment.UnitTests.Application;

public class CancellationAndFreeOrderTests
{
    private readonly IPaymentRepository payments = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork uow = Substitute.For<IUnitOfWork>();
    private readonly IOutboxEventCollector events = Substitute.For<IOutboxEventCollector>();
    private static ConsumeContext<T> Context<T>(T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        return context;
    }
    [Fact]
    public async Task FreeOrder_SucceedsEvenWhenProviderWouldDecline()
    {
        var message = new OrderCreatedIntegrationEvent { OrderId = Guid.NewGuid(), TotalAmount = 0, Currency = "EUR" };
        var consumer = new OrderCreatedIntegrationEventConsumer(payments, uow, events,
            NullLogger<OrderCreatedIntegrationEventConsumer>.Instance,
            Options.Create(new PaymentSimulationOptions { ProcessingDelay = TimeSpan.Zero, SuccessRatePercent = 0 }));
        await consumer.Consume(Context(message));
        events.Received(1).Add(Arg.Is<PaymentSucceededIntegrationEvent>(e => e.Amount == 0 && e.Currency == "EUR"));
        events.DidNotReceive().Add(Arg.Any<PaymentFailedIntegrationEvent>());
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancellation_ConfirmsOnlyAfterVoidingOrRefunding(bool paid)
    {
        var payment = PaymentRecord.Create(Guid.NewGuid(), 20, "EUR");
        if (paid) payment.MarkAsSucceeded();
        payments.GetByOrderIdAsync(payment.OrderId, Arg.Any<CancellationToken>()).Returns(payment);
        var consumer = new OrderCancellationRequestedConsumer(payments, uow, events);
        await consumer.Consume(Context(new OrderCancellationRequestedIntegrationEvent { OrderId = payment.OrderId, Amount = 20, Currency = "EUR" }));
        payment.Status.Should().Be(paid ? PaymentStatus.Refunded : PaymentStatus.Cancelled);
        events.Received(1).Add(Arg.Is<PaymentCancelledIntegrationEvent>(e => e.OrderId == payment.OrderId));
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task CancellationBeforeCreation_PreventsDelayedPayment()
    {
        PaymentRecord? stored = null;
        payments.When(p => p.Add(Arg.Any<PaymentRecord>())).Do(c => stored = c.Arg<PaymentRecord>());
        payments.GetByOrderIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_ => stored);
        var id = Guid.NewGuid();
        await new OrderCancellationRequestedConsumer(payments, uow, events).Consume(Context(
            new OrderCancellationRequestedIntegrationEvent { OrderId = id, Amount = 10, Currency = "USD" }));
        stored!.Status.Should().Be(PaymentStatus.Cancelled);
        var consumer = new OrderCreatedIntegrationEventConsumer(payments, uow, events,
            NullLogger<OrderCreatedIntegrationEventConsumer>.Instance,
            Options.Create(new PaymentSimulationOptions { ProcessingDelay = TimeSpan.Zero, SuccessRatePercent = 100 }));
        await consumer.Consume(Context(new OrderCreatedIntegrationEvent { OrderId = id, TotalAmount = 10, Currency = "USD" }));
        events.DidNotReceive().Add(Arg.Any<PaymentSucceededIntegrationEvent>());
        // Retrying cancellation does not create another confirmation/refund.
        await new OrderCancellationRequestedConsumer(payments, uow, events).Consume(Context(
            new OrderCancellationRequestedIntegrationEvent { OrderId = id, Amount = 10, Currency = "USD" }));
        events.Received(1).Add(Arg.Any<PaymentCancelledIntegrationEvent>());
    }
}
