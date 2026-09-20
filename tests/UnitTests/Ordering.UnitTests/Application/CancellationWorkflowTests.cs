using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Commands;
using NovaCart.Services.Ordering.Application.Consumers;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
using NovaCart.Services.Payment.Contracts.IntegrationEvents;
namespace NovaCart.Tests.Ordering.UnitTests.Application;

public class CancellationWorkflowTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancellation_WaitsForPaymentConfirmation_AndIgnoresLateSuccess(bool paid)
    {
        var order = Order.Create(Guid.NewGuid(), Address.Create("Street", "City", "State", "Country", "12345"), currency: "EUR");
        order.AddItem(Guid.NewGuid(), "Product", 10, 2);
        order.Confirm();
        if (paid) order.MarkAsPaid();
        var orders = Substitute.For<IOrderRepository>();
        orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var uow = Substitute.For<IUnitOfWork>();
        var events = Substitute.For<IOutboxEventCollector>();
        var handler = new CancelOrderHandler(orders, uow, events);
        (await handler.Handle(new(order.Id, order.BuyerId), default)).IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.CancellationPending);
        events.Received(1).Add(Arg.Is<OrderCancellationRequestedIntegrationEvent>(e => e.Amount == 20 && e.Currency == "EUR"));
        await handler.Handle(new(order.Id, order.BuyerId), default);
        events.Received(1).Add(Arg.Any<OrderCancellationRequestedIntegrationEvent>());
        await new PaymentSucceededIntegrationEventConsumer(orders, uow,
            NullLogger<PaymentSucceededIntegrationEventConsumer>.Instance)
            .Consume(Context(new PaymentSucceededIntegrationEvent { OrderId = order.Id }));
        order.Status.Should().Be(OrderStatus.CancellationPending);
        var confirmation = new PaymentCancelledConsumer(orders, uow);
        await confirmation.Consume(Context(new PaymentCancelledIntegrationEvent { OrderId = order.Id }));
        order.Status.Should().Be(OrderStatus.Cancelled);
        await confirmation.Consume(Context(new PaymentCancelledIntegrationEvent { OrderId = order.Id }));
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
    private static ConsumeContext<T> Context<T>(T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        return context;
    }
}
