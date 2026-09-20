using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Abstractions;
using NovaCart.Services.Ordering.Application.Commands;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
using NovaCart.Services.Payment.Application.Consumers;
using NovaCart.Services.Payment.Application.Options;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.Repositories;
using NovaCart.Services.Payment.Domain.ValueObjects;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Аудит NovaCart: воспроизведение дефектов настоящими обработчиками.");
Console.WriteLine("Репозитории, каталог, UnitOfWork и транспорт заменены NSubstitute; БД и брокер не запускаются.");

// Сценарий 1: фактическая валюта Catalog теряется при создании заказа.
var priced = await CreateFromCatalog(100m, "EUR");
Require(priced.Event.TotalAmount == 100m, "В событии ожидается исходная числовая цена 100.");
Require(priced.Event.Currency == "USD", "Ожидалось воспроизведение подмены EUR на USD.");
var successfulPayment = PaymentRecord.Create(priced.Event.OrderId, priced.Event.TotalAmount, priced.Event.Currency);
successfulPayment.MarkAsSucceeded();
Console.WriteLine($"ВОСПРОИЗВЕДЕНО: Catalog=100 EUR; OrderCreated={priced.Event.TotalAmount} {priced.Event.Currency}; Payment={successfulPayment.Amount} {successfulPayment.Currency}.");

// Сценарий 2: оформление допускает нулевую сумму, а настоящий Payment consumer её отвергает.
var free = await CreateFromCatalog(0m, "USD");
var paymentRepository = Substitute.For<IPaymentRepository>();
paymentRepository.GetByOrderIdAsync(free.Event.OrderId, Arg.Any<CancellationToken>())
    .Returns((PaymentRecord?)null);
var paymentUnitOfWork = Substitute.For<IUnitOfWork>();
var paymentEvents = Substitute.For<IOutboxEventCollector>();
var paymentConsumer = new OrderCreatedIntegrationEventConsumer(
    paymentRepository,
    paymentUnitOfWork,
    paymentEvents,
    NullLogger<OrderCreatedIntegrationEventConsumer>.Instance,
    Options.Create(new PaymentSimulationOptions { ProcessingDelay = TimeSpan.Zero, SuccessRatePercent = 100 }));
var consumeContext = Substitute.For<ConsumeContext<OrderCreatedIntegrationEvent>>();
consumeContext.Message.Returns(free.Event);
consumeContext.CancellationToken.Returns(CancellationToken.None);

ArgumentException? zeroAmountError = null;
try
{
    await paymentConsumer.Consume(consumeContext);
}
catch (ArgumentException ex) when (ex.ParamName == "amount")
{
    zeroAmountError = ex;
}

Require(zeroAmountError is not null, "Ожидалось исключение Payment consumer для принятого заказа с нулевой суммой.");
Require(free.Order.Status == OrderStatus.Created, "Прямой CreateOrderHandler должен оставить заказ Created.");
paymentRepository.DidNotReceive().Add(Arg.Any<PaymentRecord>());
await paymentUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
paymentEvents.DidNotReceive().Add(Arg.Any<IntegrationEvent>());
Console.WriteLine($"ВОСПРОИЗВЕДЕНО: заказ на 0 сохранён в статусе {free.Order.Status}; Payment consumer: {zeroAmountError!.Message}; результат оплаты не создан.");

// Сценарий 3: настоящий CancelOrderHandler принимает отмену после Paid.
priced.Order.Confirm();
priced.Order.MarkAsPaid();
var cancellationRepository = Substitute.For<IOrderRepository>();
cancellationRepository.GetByIdAsync(priced.Order.Id, Arg.Any<CancellationToken>()).Returns(priced.Order);
var cancellationUnitOfWork = Substitute.For<IUnitOfWork>();
cancellationUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
var cancelHandler = new CancelOrderHandler(cancellationRepository, cancellationUnitOfWork);
var cancelled = await cancelHandler.Handle(
    new CancelOrderCommand(priced.Order.Id, priced.Order.BuyerId), CancellationToken.None);

Require(cancelled.IsSuccess, "Ожидалось принятие отмены уже оплаченного заказа.");
Require(priced.Order.Status == OrderStatus.Cancelled, "Ожидался переход Paid -> Cancelled.");
Require(successfulPayment.Status.Equals(PaymentStatus.Succeeded), "Запись оплаты должна остаться Succeeded.");
await cancellationUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
Console.WriteLine($"ВОСПРОИЗВЕДЕНО: CancelOrderHandler принял Paid -> {priced.Order.Status}; запись Payment осталась {successfulPayment.Status}.");

Console.WriteLine("ИТОГО: 3 сценария воспроизведены. Это подтверждение дефектов на уровне обработчиков, не сквозная проверка PostgreSQL/RabbitMQ.");

static async Task<(Order Order, OrderCreatedIntegrationEvent Event)> CreateFromCatalog(decimal amount, string currency)
{
    var product = new CatalogProduct(Guid.NewGuid(), "Audit product", amount, currency);
    IReadOnlyDictionary<Guid, CatalogProduct> products = new Dictionary<Guid, CatalogProduct> { [product.Id] = product };
    var catalog = Substitute.For<ICatalogProductReader>();
    catalog.GetActiveProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(products);
    var orders = Substitute.For<IOrderRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    var events = Substitute.For<IOutboxEventCollector>();
    Order? capturedOrder = null;
    OrderCreatedIntegrationEvent? capturedEvent = null;
    orders.When(x => x.Add(Arg.Any<Order>())).Do(call => capturedOrder = call.Arg<Order>());
    events.When(x => x.Add(Arg.Any<IntegrationEvent>())).Do(call => capturedEvent = call.Arg<IntegrationEvent>() as OrderCreatedIntegrationEvent);
    var handler = new CreateOrderHandler(orders, unitOfWork, events, catalog);
    var result = await handler.Handle(new CreateOrderCommand(
        Guid.NewGuid(),
        new AddressDto("Audit street", "City", "State", "Country", "12345"),
        [new CreateOrderItemRequest(product.Id, 1)]), CancellationToken.None);
    Require(result.IsSuccess && capturedOrder is not null && capturedEvent is not null, "CreateOrderHandler должен принять подготовленный заказ.");
    await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    return (capturedOrder!, capturedEvent!);
}

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
