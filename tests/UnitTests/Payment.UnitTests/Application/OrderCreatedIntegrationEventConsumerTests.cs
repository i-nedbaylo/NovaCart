using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
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

public class OrderCreatedIntegrationEventConsumerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxEventCollector _outboxEventCollector;
    private readonly ILogger<OrderCreatedIntegrationEventConsumer> _logger;
    private readonly OrderCreatedIntegrationEventConsumer _consumer;

    public OrderCreatedIntegrationEventConsumerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _outboxEventCollector = Substitute.For<IOutboxEventCollector>();
        _logger = Substitute.For<ILogger<OrderCreatedIntegrationEventConsumer>>();

        var options = Options.Create(new PaymentSimulationOptions
        {
            ProcessingDelay = TimeSpan.Zero,
            SuccessRatePercent = 80
        });

        _consumer = new OrderCreatedIntegrationEventConsumer(
            _paymentRepository, _unitOfWork, _outboxEventCollector, _logger, options);
    }

    [Fact]
    public async Task Consume_Should_CreatePaymentAndSave_When_NewOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new OrderCreatedIntegrationEvent
        {
            OrderId = orderId,
            BuyerId = Guid.NewGuid(),
            TotalAmount = 999.99m,
            Currency = "USD",
            CorrelationId = orderId
        };

        _paymentRepository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((PaymentRecord?)null);

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _paymentRepository.Received(1).Add(Arg.Any<PaymentRecord>());
        // SaveChanges is called at least twice: once after creating, once after processing
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.Received(1).Add(Arg.Any<IntegrationEvent>());
    }

    [Fact]
    public async Task Consume_Should_PublishSucceededOrFailedEvent_When_NewOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new OrderCreatedIntegrationEvent
        {
            OrderId = orderId,
            BuyerId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "USD",
            CorrelationId = orderId
        };

        _paymentRepository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((PaymentRecord?)null);

        IntegrationEvent? capturedEvent = null;
        _outboxEventCollector.When(c => c.Add(Arg.Any<IntegrationEvent>()))
            .Do(ci => capturedEvent = ci.Arg<IntegrationEvent>());

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent.Should().BeAssignableTo<IntegrationEvent>();

        // Must be one of the two result types
        (capturedEvent is PaymentSucceededIntegrationEvent or PaymentFailedIntegrationEvent)
            .Should().BeTrue();

        if (capturedEvent is PaymentSucceededIntegrationEvent succeeded)
        {
            succeeded.OrderId.Should().Be(orderId);
            succeeded.Amount.Should().Be(100m);
            succeeded.Currency.Should().Be("USD");
            succeeded.CorrelationId.Should().Be(orderId);
        }
        else if (capturedEvent is PaymentFailedIntegrationEvent failed)
        {
            failed.OrderId.Should().Be(orderId);
            failed.Amount.Should().Be(100m);
            failed.Reason.Should().NotBeNullOrWhiteSpace();
            failed.CorrelationId.Should().Be(orderId);
        }
    }

    [Fact]
    public async Task Consume_Should_SkipProcessing_When_PaymentAlreadySucceeded()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var existingPayment = PaymentRecord.Create(orderId, 100m, "USD");
        existingPayment.MarkAsSucceeded();

        _paymentRepository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(existingPayment);

        var message = new OrderCreatedIntegrationEvent
        {
            OrderId = orderId,
            BuyerId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "USD"
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert — idempotent skip: no new payment, no save, no event
        _paymentRepository.DidNotReceive().Add(Arg.Any<PaymentRecord>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.DidNotReceive().Add(Arg.Any<IntegrationEvent>());
    }

    [Fact]
    public async Task Consume_Should_SkipProcessing_When_PaymentAlreadyFailed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var existingPayment = PaymentRecord.Create(orderId, 100m, "USD");
        existingPayment.MarkAsFailed("Previous failure");

        _paymentRepository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(existingPayment);

        var message = new OrderCreatedIntegrationEvent
        {
            OrderId = orderId,
            BuyerId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "USD"
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _paymentRepository.DidNotReceive().Add(Arg.Any<PaymentRecord>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _outboxEventCollector.DidNotReceive().Add(Arg.Any<IntegrationEvent>());
    }

    private static ConsumeContext<T> CreateConsumeContext<T>(T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
