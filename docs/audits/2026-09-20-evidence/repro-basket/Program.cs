using System.Collections.Concurrent;
using System.Text;
using MassTransit;
using NSubstitute;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

Console.OutputEncoding = Encoding.UTF8;

var buyerId = "11111111-1111-4111-8111-111111111111";
var productId = Guid.Parse("22222222-2222-4222-8222-222222222222");
var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var stateLock = new object();
var published = new ConcurrentQueue<BasketCheckoutIntegrationEvent>();
var snapshotsRead = 0;
var deleteCalls = 0;

ShoppingCart? storedBasket = ShoppingCart.Create(buyerId);
storedBasket.AddItem(productId, "Диагностический товар", 10m, 2);

var repository = Substitute.For<IBasketRepository>();
var publisher = Substitute.For<IPublishEndpoint>();

// Каждый вызов получает собственную копию сохранённой корзины. Барьер удерживает
// оба чтения до получения двух снимков, прежде чем разрешить Publish и Delete.
repository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
    .Returns(call => ReadSnapshotAsync(call.ArgAt<CancellationToken>(1)));

repository.DeleteBasketAsync(buyerId, Arg.Any<CancellationToken>())
    .Returns(_ =>
    {
        lock (stateLock)
        {
            storedBasket = null;
        }

        Interlocked.Increment(ref deleteCalls);
        return Task.CompletedTask;
    });

publisher.Publish(Arg.Any<BasketCheckoutIntegrationEvent>(), Arg.Any<CancellationToken>())
    .Returns(call =>
    {
        published.Enqueue(call.Arg<BasketCheckoutIntegrationEvent>());
        return Task.CompletedTask;
    });

var firstHandler = new CheckoutBasketHandler(repository, publisher);
var secondHandler = new CheckoutBasketHandler(repository, publisher);
var command = new CheckoutBasketCommand(buyerId, "Улица 1", "Город", "Регион", "RU", "123456");
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
    var firstAttempt = firstHandler.Handle(command, timeout.Token);
    var secondAttempt = secondHandler.Handle(command, timeout.Token);
    var results = await Task.WhenAll(firstAttempt, secondAttempt);
    var events = published.ToArray();

    Console.WriteLine("Объект проверки: реальный CheckoutBasketHandler; хранилище и публикация заменены NSubstitute.");
    Console.WriteLine($"Снимков корзины до открытия барьера: {snapshotsRead}");
    Console.WriteLine($"Успешных Handle: {results.Count(result => result.IsSuccess)}");
    Console.WriteLine($"Опубликованных событий: {events.Length}; вызовов Delete: {deleteCalls}");

    foreach (var message in events)
    {
        var items = string.Join(", ", message.Items.Select(item => $"{item.ProductId} x {item.Quantity}"));
        Console.WriteLine($"Id={message.Id}; BuyerId={message.BuyerId}; Items=[{items}]");
    }

    var reproduced = snapshotsRead == 2
        && results.All(result => result.IsSuccess)
        && events.Length == 2
        && events.Select(message => message.Id).Distinct().Count() == 2
        && events.All(message => message.BuyerId == buyerId
            && message.Items.Count == 1
            && message.Items[0].ProductId == productId
            && message.Items[0].Quantity == 2)
        && deleteCalls == 2
        && storedBasket is null;

    Console.WriteLine(reproduced
        ? "ДЕФЕКТ ВОСПРОИЗВЕДЁН: одна корзина породила два разных события checkout."
        : "Ожидаемое воспроизведение не подтверждено; требуется проверить вывод и текущую реализацию.");
    Console.WriteLine("Настоящие Redis, RabbitMQ, Ordering и Payment в этой проверке не запускались.");
    return reproduced ? 0 : 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Диагностический запуск завершился ошибкой: {exception}");
    return 2;
}

async Task<ShoppingCart?> ReadSnapshotAsync(CancellationToken cancellationToken)
{
    ShoppingCart? snapshot = null;
    lock (stateLock)
    {
        if (storedBasket is not null)
        {
            snapshot = ShoppingCart.Create(storedBasket.BuyerId);
            foreach (var item in storedBasket.Items)
            {
                snapshot.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);
            }
        }
    }

    if (Interlocked.Increment(ref snapshotsRead) == 2)
    {
        gate.TrySetResult();
    }

    await gate.Task.WaitAsync(cancellationToken);
    return snapshot;
}
