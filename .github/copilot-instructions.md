# NovaCart — Copilot Instructions

## Обзор проекта

**NovaCart** — учебно-демонстрационный проект уровня production-grade, реализующий e-commerce платформу на основе микросервисной архитектуры .NET 10. Проект демонстрирует современные паттерны, инструменты и практики enterprise-разработки.

**Репозиторий:** https://github.com/i-nedbaylo/NovaCart.git

---

## Архитектурные решения (ADR)

### ADR-001: Monorepo

Все сервисы, shared-библиотеки, инфраструктура и фронтенд находятся в одном репозитории. Это упрощает навигацию, рефакторинг и демонстрацию проекта.

### ADR-002: .NET Aspire как оркестратор

Используется .NET Aspire для:

- локальной оркестрации всех сервисов через `AppHost`
- service discovery (без жёстких адресов)
- observability (OpenTelemetry, Aspire Dashboard)
- управления зависимостями (PostgreSQL, Redis, RabbitMQ)

Docker-compose **не используется** на начальных фазах. Aspire заменяет его для локальной разработки.

### ADR-003: Blazor Web App с Interactive Auto render mode

Фронтенд — **Blazor Web App** (.NET 10), а **не** standalone Blazor WebAssembly.

- **Interactive render mode:** Auto (Server and WebAssembly)
- **Interactivity location:** Per page/component
- Серверная часть Blazor Web App выполняет роль **BFF** (Backend for Frontend)
- SSR-страницы (каталог, лендинг) — статический рендеринг для SEO и скорости
- Интерактивные компоненты (корзина, оформление заказа) — `@rendermode InteractiveAuto`

### ADR-004: YARP вместо Ocelot

API Gateway реализуется на **YARP** (Yet Another Reverse Proxy). Ocelot — устаревший и практически заброшенный проект. YARP активно поддерживается Microsoft и естественно интегрируется с Aspire.

### ADR-005: PostgreSQL как единственная СУБД

Все сервисы используют **PostgreSQL**. MongoDB не используется — каталог товаров с категориями это реляционные данные. PostgreSQL покрывает все кейсы, включая JSONB для гибких атрибутов и полнотекстовый поиск.

### ADR-006: ASP.NET Core Identity + JWT

Аутентификация реализуется через встроенный **ASP.NET Core Identity** с JWT-токенами. Не писать свою аутентификацию. Keycloak — опциональное расширение на поздних фазах.

### ADR-007: Количество сервисов — инкрементально

Начинаем с 3 сервисов (Catalog, Ordering, Identity), постепенно добавляем. Лучше 3 качественно реализованных сервиса, чем 10 заглушек.

---

## Технологический стек

### Backend

| Технология | Назначение |
|---|---|
| .NET 10 | Целевой фреймворк |
| ASP.NET Core Web API | REST API для сервисов |
| gRPC | Внутренняя коммуникация между сервисами |
| YARP | API Gateway / Reverse Proxy |
| .NET Aspire | Оркестрация, service discovery, observability |
| Entity Framework Core 10 | ORM |
| MassTransit | Абстракция над RabbitMQ (вместо прямого использования) |
| Polly | Retry, Circuit Breaker, Resilience |
| FluentValidation | Валидация команд и запросов |
| MediatR | CQRS — медиатор для Commands/Queries |

### Data

| Технология | Назначение |
|---|---|
| PostgreSQL | Основная БД для всех сервисов |
| Redis | Кэширование, хранение корзины |
| RabbitMQ | Брокер сообщений |

### Frontend

| Технология | Назначение |
|---|---|
| Blazor Web App (.NET 10) | Фронтенд (Auto render mode, Per page/component) |
| MudBlazor | UI-компонентная библиотека |

### Observability

| Технология | Назначение |
|---|---|
| Aspire Dashboard | Метрики, логи, трейсы (Phase 1–3) |
| OpenTelemetry | Телеметрия |
| Prometheus + Grafana | Мониторинг (Phase 4) |
| Jaeger | Распределённая трассировка (Phase 4) |

### Инфраструктура

| Технология | Назначение |
|---|---|
| Docker | Контейнеризация |
| GitHub Actions | CI/CD |
| Kubernetes | Опционально, Phase 4 |

---

## Структура репозитория

```
NovaCart/
│
├── .github/
│   ├── copilot-instructions.md      # Этот файл
│   └── workflows/
│        ├── build.yml
│        ├── test.yml
│        └── deploy.yml
│
├── src/
│   ├── Services/                     # Микросервисы
│   │   ├── Catalog/
│   │   │   ├── Catalog.API/          # ASP.NET Core Web API
│   │   │   ├── Catalog.Application/  # CQRS, Handlers, DTO, Interfaces
│   │   │   ├── Catalog.Domain/       # Entities, Value Objects, Domain Events
│   │   │   ├── Catalog.Infrastructure/ # EF Core, Repository implementations
│   │   │   └── Catalog.Contracts/    # Integration Events, Public DTO
│   │   │
│   │   ├── Ordering/
│   │   │   ├── Ordering.API/
│   │   │   ├── Ordering.Application/
│   │   │   ├── Ordering.Domain/
│   │   │   ├── Ordering.Infrastructure/
│   │   │   └── Ordering.Contracts/
│   │   │
│   │   ├── Identity/
│   │   │   ├── Identity.API/
│   │   │   ├── Identity.Application/
│   │   │   ├── Identity.Domain/
│   │   │   ├── Identity.Infrastructure/
│   │   │   └── Identity.Contracts/
│   │   │
│   │   ├── Basket/                   # Phase 2
│   │   │   ├── Basket.API/
│   │   │   ├── Basket.Application/
│   │   │   ├── Basket.Domain/
│   │   │   ├── Basket.Infrastructure/
│   │   │   └── Basket.Contracts/
│   │   │
│   │   ├── Payment/                  # Phase 2
│   │   │   ├── Payment.API/
│   │   │   ├── Payment.Application/
│   │   │   ├── Payment.Domain/
│   │   │   ├── Payment.Infrastructure/
│   │   │   └── Payment.Contracts/
│   │   │
│   │   ├── Notification/             # Phase 3
│   │   │   ├── Notification.API/
│   │   │   ├── Notification.Application/
│   │   │   ├── Notification.Infrastructure/
│   │   │   └── Notification.Contracts/
│   │   │
│   │   └── Search/                   # Phase 3
│   │       ├── Search.API/
│   │       ├── Search.Application/
│   │       ├── Search.Infrastructure/
│   │       └── Search.Contracts/
│   │
│   ├── BuildingBlocks/               # Переиспользуемые библиотеки
│   │   ├── BuildingBlocks.Common/    # BaseEntity, Result pattern, Exceptions
│   │   ├── BuildingBlocks.EventBus/  # Абстракция MassTransit, IntegrationEvent base
│   │   ├── BuildingBlocks.CQRS/     # ICommand, IQuery, базовые хендлеры
│   │   └── BuildingBlocks.Persistence/ # Generic Repository, UnitOfWork, Outbox
│   │
│   ├── ApiGateway/
│   │   └── Gateway.Yarp/             # YARP reverse proxy
│   │
│   └── Web/
│       ├── NovaCart.Web/              # Blazor Web App (Server host = BFF)
│       │   ├── Components/
│       │   │   ├── App.razor
│       │   │   ├── Routes.razor
│       │   │   ├── Layout/
│       │   │   └── Pages/            # SSR pages
│       │   ├── Services/             # BFF services (server-side API calls)
│       │   ├── Aggregators/          # BFF aggregators (combine data from multiple services)
│       │   └── Program.cs
│       │
│       └── NovaCart.Web.Client/       # Blazor WASM client
│           ├── Pages/                 # Interactive pages (@rendermode InteractiveAuto)
│           ├── Components/            # Reusable interactive components
│           ├── Services/              # HTTP client services
│           └── Program.cs
│
├── tests/
│   ├── UnitTests/
│   │   ├── Catalog.UnitTests/
│   │   ├── Ordering.UnitTests/
│   │   └── Identity.UnitTests/
│   ├── IntegrationTests/
│   │   ├── Catalog.IntegrationTests/
│   │   └── Ordering.IntegrationTests/
│   └── ArchitectureTests/             # Проверка зависимостей между слоями
│
├── docs/
│   ├── architecture/
│   │   ├── c4-context.md
│   │   ├── c4-containers.md
│   │   └── decisions/                 # ADR (Architecture Decision Records)
│   ├── api/
│   └── diagrams/
│
├── NovaCart.AppHost/                   # Aspire AppHost
│   └── Program.cs
│
├── NovaCart.ServiceDefaults/           # Aspire ServiceDefaults
│   └── Extensions.cs
│
├── NovaCart.slnx                       # Solution file
└── README.md
```

---

## Архитектура

### Общая схема

```
┌──────────────────────────────────────┐
│         Blazor Web App               │
│  (SSR + Interactive Auto per page)   │
│         ┌──────────┐                 │
│         │   BFF    │ (серверная часть)│
│         └────┬─────┘                 │
└──────────────┼───────────────────────┘
               │
       ┌───────▼────────┐
       │  YARP Gateway   │
       └───────┬────────┘
               │
    ┌──────────┼──────────────┐
    │          │              │
┌───▼───┐ ┌───▼────┐ ┌───────▼──┐
│Catalog│ │Ordering│ │ Identity │
│  API  │ │  API   │ │   API    │
└───┬───┘ └───┬────┘ └──────────┘
    │         │
    │    ┌────▼─────┐
    │    │  Basket  │ (Phase 2)
    │    │   API    │
    │    └──────────┘
    │
    │    ┌──────────┐
    │    │ Payment  │ (Phase 2)
    │    │   API    │
    │    └──────────┘
    │
    └───── RabbitMQ (Integration Events) ─────
               │
    ┌──────────┼──────────┐
    │          │          │
┌───▼──────┐ ┌▼────────┐ ┌▼──────┐
│Notification│ │ Search │ │ ...  │
│  (Phase 3) │ │(Phase 3)│ │      │
└────────────┘ └─────────┘ └──────┘

            Aspire Dashboard
         (logs, traces, metrics)
```

### Clean Architecture (внутри каждого сервиса)

```
API → Application → Domain
        ↓
   Infrastructure
```

- **Domain** — не зависит ни от чего. Entities, Value Objects, Aggregates, Domain Events, интерфейсы репозиториев.
- **Application** — зависит от Domain. CQRS (Commands/Queries через MediatR), Handlers, DTO, интерфейсы внешних сервисов.
- **Infrastructure** — зависит от Application и Domain. EF Core DbContext, реализации репозиториев, внешние интеграции.
- **API** — зависит от Application и Infrastructure. Controllers/Minimal API, DI, Middleware, Swagger.
- **Contracts** — не зависит ни от чего. Integration Events и Public DTO, используемые другими сервисами.

### Правила зависимостей

- Domain **НЕ** ссылается на Application, Infrastructure, API.
- Application **НЕ** ссылается на Infrastructure, API.
- Infrastructure **НЕ** ссылается на API.
- Contracts **НЕ** ссылается ни на что в сервисе. Может ссылаться только на BuildingBlocks.
- Сервисы **НЕ** ссылаются друг на друга напрямую (только через Contracts и EventBus).

---

## Паттерны для реализации

### Phase 1

- **Clean Architecture** — в каждом сервисе
- **CQRS** — разделение Commands и Queries через MediatR
- **Repository Pattern** — абстракция доступа к данным
- **Result Pattern** — вместо исключений для бизнес-ошибок (Result<T>)

### Phase 2

- **Integration Events** — асинхронное взаимодействие через RabbitMQ/MassTransit
- **Outbox Pattern** — гарантия доставки событий (транзакция БД + событие атомарно)
- **Retry + Circuit Breaker** — через Polly

### Phase 3

- **Saga Pattern** — оркестрация заказа (Order → Payment → Confirmation)
- **BFF Pattern** — агрегация данных на серверной стороне Blazor Web App

### Phase 4

- **Feature Flags** — постепенное включение функционала
- **Rate Limiting** — ограничение запросов на API Gateway

---

## Фазы реализации

### 🟢 Phase 1 — Foundation (Фундамент)

**Цель:** работающая система end-to-end с 3 сервисами.

#### 1.1 Инфраструктура проекта

- [ ] Создать структуру папок (`src/`, `tests/`, `docs/`)
- [ ] Настроить `NovaCart.AppHost` (Aspire) — оркестрация сервисов
- [ ] Настроить `NovaCart.ServiceDefaults` — общие Aspire-настройки
- [ ] Создать `BuildingBlocks.Common` — BaseEntity, Result<T>, общие исключения
- [ ] Создать `BuildingBlocks.CQRS` — ICommand<T>, IQuery<T>, базовые абстракции
- [ ] Создать `BuildingBlocks.Persistence` — Generic Repository, IUnitOfWork
- [ ] Создать `README.md` с описанием проекта и архитектуры

#### 1.2 Identity Service

- [ ] ASP.NET Core Identity + EF Core + PostgreSQL
- [ ] JWT-аутентификация (access token + refresh token)
- [ ] Эндпоинты: Register, Login, Refresh, Me
- [ ] Роли: Admin, Customer
- [ ] Регистрация в Aspire AppHost

#### 1.3 Catalog Service

- [ ] Clean Architecture (API / Application / Domain / Infrastructure / Contracts)
- [ ] Domain: Product (entity), Category (entity), Price (value object)
- [ ] CQRS: CreateProduct, UpdateProduct, GetProducts, GetProductById
- [ ] EF Core + PostgreSQL
- [ ] Пагинация, фильтрация, сортировка
- [ ] Seed data (начальные данные)
- [ ] Регистрация в Aspire AppHost

#### 1.4 Ordering Service

- [ ] Clean Architecture
- [ ] Domain: Order (aggregate root), OrderItem (entity), OrderStatus (enum/value object)
- [ ] Statuses: Created, Confirmed, Paid, Shipped, Delivered, Cancelled
- [ ] CQRS: CreateOrder, GetOrders, GetOrderById, CancelOrder
- [ ] EF Core + PostgreSQL
- [ ] Авторизация: пользователь видит только свои заказы
- [ ] Регистрация в Aspire AppHost

#### 1.5 API Gateway (YARP)

- [ ] YARP Reverse Proxy
- [ ] Маршрутизация к Catalog, Ordering, Identity
- [ ] Интеграция с Aspire service discovery
- [ ] Прокидывание JWT-токена

#### 1.6 Blazor Web App (Фронтенд)

- [ ] Blazor Web App (Server host + WASM client)
- [ ] MudBlazor — установка и базовая тема
- [ ] Layout: навигация, header, footer
- [ ] SSR-страницы: главная, каталог товаров, страница товара
- [ ] Интерактивные компоненты: логин/регистрация
- [ ] BFF-слой: серверные сервисы для вызова микросервисов
- [ ] HttpClient → YARP Gateway → сервисы
- [ ] Регистрация в Aspire AppHost

#### 1.7 Тесты (Phase 1)

- [ ] Unit-тесты для Domain-слоёв (Catalog, Ordering)
- [ ] Unit-тесты для Application-хендлеров
- [ ] Architecture-тесты (проверка зависимостей между слоями)

---

### 🟡 Phase 2 — Microservices Communication (Коммуникация)

**Цель:** сервисы общаются через события, появляется полный цикл заказа.

#### 2.1 Messaging инфраструктура

- [ ] Создать `BuildingBlocks.EventBus` — абстракция MassTransit
- [ ] IntegrationEvent (базовый класс)
- [ ] Подключить RabbitMQ через Aspire
- [ ] Настроить MassTransit в каждом сервисе

#### 2.2 Basket Service

- [ ] Redis (через Aspire)
- [ ] Domain: Basket, BasketItem
- [ ] Эндпоинты: GetBasket, UpdateBasket, DeleteBasket, Checkout
- [ ] TTL для корзины
- [ ] При Checkout → публикация `BasketCheckoutEvent`

#### 2.3 Payment Service

- [ ] Симуляция оплаты (fake payment provider)
- [ ] Подписка на `OrderCreatedIntegrationEvent`
- [ ] Публикация `PaymentSucceededEvent` / `PaymentFailedEvent`
- [ ] Статусы платежа

#### 2.4 Integration Events

- [ ] `BasketCheckoutEvent` → Ordering Service создаёт заказ
- [ ] `OrderCreatedEvent` → Payment Service обрабатывает оплату
- [ ] `PaymentSucceededEvent` → Ordering Service обновляет статус
- [ ] `PaymentFailedEvent` → Ordering Service отменяет заказ

#### 2.5 Outbox Pattern

- [ ] Реализовать в `BuildingBlocks.Persistence`
- [ ] Событие сохраняется в БД в одной транзакции с бизнес-данными
- [ ] Фоновый процесс отправляет события в RabbitMQ

#### 2.6 Resilience

- [ ] Polly: Retry, Circuit Breaker для HTTP-вызовов между сервисами
- [ ] Конфигурация через Aspire ServiceDefaults

#### 2.7 Blazor Web App — расширение

- [ ] Интерактивные страницы: корзина, оформление заказа, мои заказы
- [ ] Real-time обновление статуса заказа (SignalR через BFF)

#### 2.8 Тесты (Phase 2)

- [ ] Integration-тесты с Testcontainers (PostgreSQL, RabbitMQ, Redis)
- [ ] Unit-тесты для новых сервисов

---

### 🟠 Phase 3 — Advanced Patterns (Продвинутые паттерны)

**Цель:** архитектура уровня production.

#### 3.1 Saga Pattern

- [ ] Оркестрация заказа: Order → Payment → Confirmation
- [ ] Компенсационные действия при ошибках
- [ ] Реализация через MassTransit State Machine (Automatonymous)

#### 3.2 Notification Service

- [ ] Подписка на события (OrderConfirmed, PaymentSucceeded)
- [ ] Email-уведомления (mock / MailDev в Docker)
- [ ] Real-time уведомления через SignalR

#### 3.3 Search Service

- [ ] Elasticsearch (через Aspire)
- [ ] Индексация товаров по событию `ProductCreatedEvent` / `ProductUpdatedEvent`
- [ ] Полнотекстовый поиск, фильтры, автокомплит
- [ ] Blazor: поисковая строка с автокомплитом

#### 3.4 gRPC для внутренней коммуникации

- [ ] gRPC-контракты между сервисами (например, Ordering → Catalog для получения деталей товара)
- [ ] Protobuf-файлы в Contracts-проекте

#### 3.5 BFF-агрегация

- [ ] Агрегатор в BFF: страница заказа собирает данные из Ordering + Catalog + Payment
- [ ] Параллельные вызовы, кэширование ответов

#### 3.6 Тесты (Phase 3)

- [ ] Contract-тесты между сервисами
- [ ] End-to-end тесты (Playwright для Blazor UI)

---

### 🔴 Phase 4 — Production-Ready (Продакшен)

**Цель:** полноценная enterprise-система.

#### 4.1 Observability (замена Aspire Dashboard)

- [ ] Prometheus — сбор метрик
- [ ] Grafana — визуализация
- [ ] Jaeger — распределённая трассировка
- [ ] Centralized logging (Seq или ELK)

#### 4.2 CI/CD

- [ ] GitHub Actions: build, test, Docker image, deploy
- [ ] Multi-stage Dockerfile для каждого сервиса
- [ ] Docker Compose для staging-окружения

#### 4.3 Безопасность

- [ ] Rate Limiting на YARP Gateway
- [ ] CORS-политики
- [ ] API Key для внешних интеграций
- [ ] Audit log

#### 4.4 Дополнительные фичи

- [ ] Feature Flags
- [ ] Multi-tenant (опционально)
- [ ] Shipment Service (имитация доставки)
- [ ] Admin Panel (Blazor, отдельные страницы с ролевым доступом)

#### 4.5 Kubernetes (опционально)

- [ ] Helm charts
- [ ] Deployment, Service, Ingress для каждого сервиса
- [ ] Aspire manifest → K8s

#### 4.6 Документация

- [ ] C4-диаграммы (Context, Container, Component)
- [ ] ADR (Architecture Decision Records)
- [ ] API-документация (OpenAPI / Swagger)
- [ ] README с инструкцией запуска

---

## Конвенции кода

### Общие правила

- Язык кода и комментариев: **английский**
- Документация проекта: **русский** (README, docs/, ADR)
- Nullable reference types: **включены** (`<Nullable>enable</Nullable>`)
- Implicit usings: **включены**
- Все проекты: **.NET 10**

### Именование

- Solution: `NovaCart.slnx`
- Проекты сервисов: `NovaCart.Services.{ServiceName}.{Layer}` (например, `NovaCart.Services.Catalog.API`)
- BuildingBlocks: `NovaCart.BuildingBlocks.{Name}`
- Тесты: `NovaCart.Tests.{ServiceName}.{TestType}` (например, `NovaCart.Tests.Catalog.UnitTests`)
- Namespaces соответствуют структуре папок

### API-эндпоинты

- Использовать **Minimal API** (не Controllers)
- Группировка через `MapGroup`
- Версионирование: `/api/v1/...`
- Возвращать `Results<Ok<T>, NotFound, BadRequest<ProblemDetails>>`

### Entity Framework Core

- Code-first migrations
- Конфигурация через `IEntityTypeConfiguration<T>` (отдельные файлы)
- Имена таблиц: snake_case (PostgreSQL convention)
- DbContext per service (каждый сервис — своя БД)

### CQRS

- Commands: `Create{Entity}Command`, `Update{Entity}Command`, `Delete{Entity}Command`
- Queries: `Get{Entity}ByIdQuery`, `Get{Entities}Query`
- Handlers: `Create{Entity}Handler`, etc.
- Все handlers возвращают `Result<T>` (не бросают исключений для бизнес-ошибок)

### Integration Events

- Именование: `{Entity}{Action}IntegrationEvent` (например, `OrderCreatedIntegrationEvent`)
- Располагаются в `Contracts`-проекте сервиса-источника
- Наследуются от `IntegrationEvent` (из BuildingBlocks.EventBus)

### Blazor

- SSR-страницы — в `NovaCart.Web/Components/Pages/`
- Интерактивные страницы — в `NovaCart.Web.Client/Pages/`
- Shared-компоненты — в соответствующем проекте по необходимости
- BFF-сервисы (server-side) — в `NovaCart.Web/Services/`
- Client HTTP-сервисы — в `NovaCart.Web.Client/Services/`
- Использовать `@rendermode InteractiveAuto` только где необходима интерактивность

### Тесты

- xUnit
- FluentAssertions
- NSubstitute (для моков)
- Testcontainers (для integration-тестов)
- Naming: `{Method}_Should_{ExpectedResult}_When_{Condition}`

---

## Aspire AppHost — целевая конфигурация

```csharp
// NovaCart.AppHost/Program.cs — Phase 2 target
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");
var identityDb = postgres.AddDatabase("identitydb");

var redis = builder.AddRedis("redis");
var rabbitmq = builder.AddRabbitMQ("rabbitmq");

// Services
var identityApi = builder.AddProject<Projects.NovaCart_Services_Identity_API>("identity-api")
    .WithReference(identityDb)
    .WithReference(rabbitmq);

var catalogApi = builder.AddProject<Projects.NovaCart_Services_Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(rabbitmq);

var orderingApi = builder.AddProject<Projects.NovaCart_Services_Ordering_API>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(rabbitmq);

var basketApi = builder.AddProject<Projects.NovaCart_Services_Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitmq);

// API Gateway
var gateway = builder.AddProject<Projects.NovaCart_ApiGateway_Yarp>("gateway")
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(identityApi)
    .WithReference(basketApi);

// Web (BFF)
builder.AddProject<Projects.NovaCart_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(gateway);

builder.Build().Run();
```

---

## Пример сценария (end-to-end)

### Пользователь оформляет заказ

1. **Blazor Web App** → пользователь нажимает "Оформить заказ"
2. **BFF** (серверная часть Blazor) → вызывает YARP Gateway
3. **YARP Gateway** → маршрутизирует в Basket API
4. **Basket API** → Checkout → публикует `BasketCheckoutIntegrationEvent` в RabbitMQ
5. **Ordering API** → подписчик, создаёт Order со статусом `Created` → публикует `OrderCreatedIntegrationEvent`
6. **Payment API** → подписчик, симулирует оплату → публикует `PaymentSucceededIntegrationEvent`
7. **Ordering API** → подписчик, обновляет статус на `Paid`
8. **Notification Service** → подписчик, отправляет email (Phase 3)
9. **Blazor Web App** → получает обновление статуса через SignalR (Phase 3)

---

## Что НЕ делать

- ❌ Не использовать Ocelot (устаревший, использовать YARP)
- ❌ Не использовать MongoDB (PostgreSQL покрывает все кейсы)
- ❌ Не писать свою аутентификацию (использовать ASP.NET Core Identity)
- ❌ Не ходить из Blazor WASM напрямую в микросервисы (всегда через BFF → Gateway)
- ❌ Не создавать shared моделей между сервисами (только Contracts — Integration Events и Public DTO)
- ❌ Не добавлять все 10 сервисов сразу (инкрементально по фазам)
- ❌ Не использовать Controllers (использовать Minimal API)
- ❌ Не ставить `@rendermode InteractiveAuto` глобально (только Per page/component)
- ❌ Не использовать standalone Blazor WebAssembly (использовать Blazor Web App)

---

## Принципы и правила разработки

### Clean Architecture — строгое соблюдение

При реализации каждого сервиса **обязательно** соблюдать разделение на слои:

1. **Domain** — ядро. Не зависит ни от чего. Содержит только:
   - Entities, Aggregates, Value Objects
   - Domain Events
   - Интерфейсы репозиториев (определение, не реализация)
   - Бизнес-правила и инварианты внутри сущностей
   - **Запрещено:** ссылки на EF Core, MediatR, FluentValidation, ASP.NET Core, любые NuGet-пакеты инфраструктуры

2. **Application** — оркестрация. Зависит только от Domain. Содержит:
   - Commands, Queries, Handlers (CQRS через MediatR)
   - DTO и маппинг
   - Validators (FluentValidation)
   - Интерфейсы внешних сервисов (определение, не реализация)
   - **Запрещено:** ссылки на EF Core, конкретные реализации репозиториев, ASP.NET Core

3. **Infrastructure** — реализация. Зависит от Domain и Application. Содержит:
   - Реализации репозиториев (EF Core)
   - DbContext и конфигурации сущностей
   - Реализации внешних сервисов
   - **Запрещено:** ссылки на API-слой, бизнес-логика

4. **API** — точка входа. Зависит от Application и Infrastructure. Содержит:
   - Minimal API endpoints
   - DI-регистрацию всех слоёв
   - Middleware
   - **Запрещено:** бизнес-логика, прямая работа с DbContext

5. **Contracts** — изолированный. Не зависит ни от чего внутри сервиса. Содержит:
   - Integration Events
   - Public DTO для внешних потребителей

### SOLID — обязательное соблюдение

- **S (Single Responsibility):** Каждый класс имеет одну причину для изменения. Handler обрабатывает одну команду/запрос. Validator валидирует одну команду. Entity управляет только своими инвариантами.
- **O (Open/Closed):** Расширение через новые Handlers, Validators, Behaviors — без модификации существующих. Использовать MediatR Pipeline Behaviors для cross-cutting concerns.
- **L (Liskov Substitution):** Все реализации интерфейсов полностью заменяемы. `IProductRepository` в тестах заменяется на mock без изменения поведения.
- **I (Interface Segregation):** Интерфейсы маленькие и специфичные. `IProductRepository` не содержит методы, не связанные с Product. Не создавать "god interfaces".
- **D (Dependency Inversion):** Зависимости всегда направлены внутрь (API → Application → Domain). Application зависит от абстракций (`IProductRepository`), а не от реализаций (`ProductRepository`). DI-регистрация только в API-слое.

### Паттерны — правила применения

- **Repository Pattern:** Репозиторий инкапсулирует доступ к данным. Интерфейс — в Domain, реализация — в Infrastructure. Не возвращать `IQueryable` из репозитория (возвращать материализованные данные).
- **Unit of Work:** `IUnitOfWork.SaveChangesAsync()` вызывается в Handler, не в Repository. Одна транзакция на одну команду.
- **CQRS:** Commands изменяют состояние и не возвращают данные (кроме Id созданной сущности). Queries не изменяют состояние. Не смешивать команды и запросы в одном Handler.
- **Result Pattern:** Handlers возвращают `Result<T>`, не бросают исключений для бизнес-ошибок. Исключения — только для действительно исключительных ситуаций (инфраструктурные сбои).
- **Domain Events:** Бизнес-события генерируются внутри Domain-сущностей. Dispatch происходит при SaveChanges. Не создавать domain events в Application-слое.
- **Rich Domain Model:** Бизнес-логика находится внутри сущностей и value objects, а не в сервисах. Сущности защищают свои инварианты через private setters и методы. Anemic Domain Model — антипаттерн.
- **Value Objects:** Иммутабельны. Сравниваются по значению. Инкапсулируют валидацию (например, `Price` не допускает отрицательных значений).

### Запреты

- ❌ **Не размещать бизнес-логику в Handlers** — логика принадлежит Domain-сущностям. Handler оркестрирует вызовы, а не содержит `if/else` бизнес-правил.
- ❌ **Не использовать anemic entities** — сущности с только публичными свойствами и без методов. Все изменения состояния — через методы сущности.
- ❌ **Не создавать циклических зависимостей** между проектами.
- ❌ **Не обращаться к DbContext напрямую** из Application-слоя — только через интерфейсы репозиториев.
- ❌ **Не использовать статические классы** для бизнес-логики или хранения состояния.
- ❌ **Не помещать `using` инфраструктурных пакетов** в Domain или Application проекты.

---

## Чек-лист реализации

Детализированный план реализации ведётся в файле **`docs/implementation-checklist.md`**.

### Правила работы с чек-листом

1. **При выполнении задачи** — Copilot **обязан** отметить соответствующий пункт как выполненный (`- [x]`) в файле `docs/implementation-checklist.md`.
2. **При обнаружении недостающих шагов** — добавить их в соответствующий раздел чек-листа.
3. **При изменении плана** — обновить чек-лист, добавив пометку об изменении.
4. **Порядок выполнения** — строго сверху вниз внутри каждой фазы. Не переходить к следующей фазе, пока не завершена текущая.
5. **Каждая сессия работы** — начинать с проверки текущего состояния чек-листа (`docs/implementation-checklist.md`), чтобы определить, какая задача следующая.
6. **Финальная валидация фазы** — перед переходом к следующей фазе убедиться, что все пункты раздела "Финальная валидация" отмечены.

### Правила работы с Git

1. **Ветвление по фазам:** для каждой фазы создаётся отдельная ветка от основной (`main`):
   - `feature/phase-1-foundation`
   - `feature/phase-2-communication`
   - `feature/phase-3-advanced-patterns`
   - `feature/phase-4-production-ready`
2. **Коммиты по завершении пунктов:** после выполнения каждого логически завершённого пункта (или группы связанных пунктов) чек-листа — создать коммит с описательным сообщением.
3. **Формат коммит-сообщений:** `feat(scope): description` — например:
   - `feat(infra): add folder structure and AppHost project`
   - `feat(building-blocks): implement Result pattern and BaseEntity`
   - `feat(catalog): add domain entities and value objects`
   - `feat(catalog): implement CQRS handlers and validators`
   - `feat(tests): add Catalog domain unit tests`
4. **Push после коммита:** после каждого коммита выполнять `git push` для синхронизации с удалённым репозиторием.
5. **Не коммитить сломанный код:** перед коммитом убедиться, что solution собирается (`dotnet build`).
6. **Завершение фазы:** по завершении всех задач фазы — ветка остаётся (merge в `main` выполняет пользователь вручную).
