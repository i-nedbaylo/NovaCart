# NovaCart

**NovaCart** — учебно-демонстрационный проект уровня production-grade, реализующий e-commerce платформу на основе микросервисной архитектуры .NET 10.

## Архитектура

```
┌────────────────────────────────────────────┐
│             Blazor Web App                  │
│      (SSR + Interactive Auto per page)      │
│   ┌──────────────────────────────────────┐ │
│   │  BFF (серверная часть)               │ │
│   │  HttpOnly-cookie хранит JWT,         │ │
│   │  пробрасывает Bearer вниз (ADR-008)  │ │
│   └──────────────┬───────────────────────┘ │
└──────────────────┼──────────────────────────┘
                   │
           ┌───────▼────────┐
           │  YARP Gateway   │ (тонкий pass-through, форвардит Authorization)
           └───────┬────────┘
                   │
   ┌──────────┬────┴─────┬───────────┬───────────┐
   │          │          │           │           │
┌──▼───┐ ┌───▼────┐ ┌───▼────┐ ┌────▼────┐ ┌────▼────┐
│Catalog│ │Ordering│ │Identity│ │ Basket  │ │ Payment │
│  API  │ │  API   │ │  API   │ │  API    │ │  API    │
└──┬────┘ └───┬────┘ └────────┘ └────┬────┘ └────┬────┘
   │          │                      │           │
PostgreSQL  PostgreSQL            Redis      PostgreSQL
(catalogdb) (orderingdb)        (корзина)   (paymentdb)
   │          │                      │           │
   └──────────┴───── RabbitMQ (MassTransit, Integration Events) ──────┘
                        + Outbox Pattern (гарантия доставки)

            Aspire Dashboard (logs, traces, metrics)
```

Каждый сервис валидирует JWT самостоятельно (общий `AddJwtAuthentication` в ServiceDefaults);
данные привязываются к пользователю из токена (`sub`), а не из клиентского ввода. Полный
событийный цикл заказа: `BasketCheckout → OrderCreated → Payment(Succeeded/Failed) → Order
статус Paid/Cancelled`.

## Технологический стек

| Категория | Технологии |
|-----------|-----------|
| Backend | .NET 10, ASP.NET Core Minimal API, MediatR, FluentValidation |
| Data | PostgreSQL, Entity Framework Core 10 |
| Frontend | Blazor Web App (Auto render mode), MudBlazor |
| Gateway | YARP Reverse Proxy |
| Оркестрация | .NET Aspire |
| Observability | OpenTelemetry, Aspire Dashboard |

## Запуск

```bash
# Клонировать репозиторий
git clone https://github.com/i-nedbaylo/NovaCart.git
cd NovaCart

# Запустить через Aspire AppHost
dotnet run --project NovaCart.AppHost
```

Aspire Dashboard будет доступен по адресу, указанному в выводе консоли.

## Структура репозитория

```
NovaCart/
├── src/
│   ├── Services/           # Микросервисы (Catalog, Ordering, Identity, Basket, Payment)
│   ├── BuildingBlocks/     # Переиспользуемые библиотеки
│   ├── ApiGateway/         # YARP Reverse Proxy
│   └── Web/                # Blazor Web App (BFF + WASM client)
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── ArchitectureTests/
├── docs/                   # Документация
├── NovaCart.AppHost/        # Aspire AppHost
├── NovaCart.ServiceDefaults/ # Aspire ServiceDefaults
└── NovaCart.slnx
```

## Аутентификация

- Регистрация/вход — SSR-страницы `/register` и `/login`. Сервер (BFF) получает JWT от Identity
  и кладёт его в **HttpOnly-cookie**; токен не попадает в браузер.
- Все вызовы к корзине и заказам идут с `Authorization: Bearer` (через `BffTokenHandler` для
  серверного рендеринга и через BFF-прокси для WASM). Сервисы валидируют токен и берут
  `buyerId` из него.
- Seed-админ: `admin@novacart.com` (см. `IdentityDbContextSeed`).

Подробнее — [ADR-008: BFF cookie authentication](docs/architecture/decisions/0008-bff-cookie-authentication.md).

## Документация

- [Чек-лист реализации](docs/implementation-checklist.md)
- [Архитектурные решения (ADR)](docs/architecture/decisions/)
- [Copilot Instructions](.github/copilot-instructions.md)
