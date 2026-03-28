# NovaCart — Чек-лист реализации

> **Правила ведения чек-листа:**
> - [x] — задача выполнена
> - [ ] — задача ожидает выполнения
> - При выполнении задачи Copilot **обязан** отметить её как выполненную
> - При обнаружении недостающих шагов — добавить их в соответствующий раздел
> - Порядок выполнения задач — сверху вниз внутри каждой фазы

---

## 🟢 Phase 1 — Foundation (Фундамент)

### 1.1 Структура решения и инфраструктурные проекты

#### 1.1.1 Структура папок

- [x] Создать папку `src/`
- [x] Создать папку `src/Services/`
- [x] Создать папку `src/BuildingBlocks/`
- [x] Создать папку `src/ApiGateway/`
- [x] Создать папку `src/Web/`
- [x] Создать папку `tests/`
- [x] Создать папку `tests/UnitTests/`
- [x] Создать папку `tests/IntegrationTests/`
- [x] Создать папку `tests/ArchitectureTests/`
- [x] Создать папку `docs/`
- [x] Создать папку `docs/architecture/`
- [x] Создать папку `docs/architecture/decisions/`
- [x] Создать папку `docs/api/`
- [x] Создать папку `docs/diagrams/`

#### 1.1.2 Aspire AppHost

- [x] Создать проект `NovaCart.AppHost` (Aspire AppHost SDK)
- [x] Добавить проект в `NovaCart.slnx`
- [x] Настроить `Program.cs` — минимальная конфигурация `DistributedApplication.CreateBuilder`
- [x] Добавить ресурс PostgreSQL (`builder.AddPostgres`)
- [x] Добавить базы данных: `catalogdb`, `orderingdb`, `identitydb`
- [x] Убедиться, что AppHost запускается без ошибок

#### 1.1.3 Aspire ServiceDefaults

- [x] Создать проект `NovaCart.ServiceDefaults` (Class Library)
- [x] Добавить проект в `NovaCart.slnx`
- [x] Реализовать `Extensions.cs` — метод `AddServiceDefaults`
- [x] Настроить OpenTelemetry (логи, метрики, трейсы)
- [x] Настроить Health Checks (`/health`, `/alive`)
- [x] Настроить Service Discovery
- [x] Настроить Resilience (стандартный resilience handler для HttpClient)

#### 1.1.4 README.md

- [x] Создать `README.md` в корне репозитория
- [x] Описание проекта (на русском)
- [x] Архитектурная схема (текстовая)
- [x] Технологический стек
- [x] Инструкция запуска (`dotnet run` через AppHost)
- [x] Структура репозитория

---

### 1.2 BuildingBlocks

#### 1.2.1 BuildingBlocks.Common

- [x] Создать проект `NovaCart.BuildingBlocks.Common` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `src/BuildingBlocks`)
- [x] Реализовать `BaseEntity` (Id: Guid, CreatedAt, UpdatedAt)
- [x] Реализовать `AggregateRoot` (наследуется от BaseEntity, список Domain Events)
- [x] Реализовать `IDomainEvent` (интерфейс-маркер)
- [x] Реализовать `Result<T>` (Success/Failure, Error messages)
- [x] Реализовать `Result` (без generic — для void-операций)
- [x] Реализовать `Error` (код ошибки + сообщение)
- [x] Реализовать базовые исключения: `NotFoundException`, `ValidationException`, `ConflictException`
- [x] Реализовать `IDateTimeProvider` (абстракция для тестируемости)
- [x] Реализовать `DateTimeProvider` (реализация, использует `DateTimeOffset.UtcNow`)
- [x] Убедиться, что проект собирается

#### 1.2.2 BuildingBlocks.CQRS

- [x] Создать проект `NovaCart.BuildingBlocks.CQRS` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить NuGet-зависимость: `MediatR`
- [x] Реализовать `ICommand` (наследуется от `IRequest<Result>`)
- [x] Реализовать `ICommand<TResponse>` (наследуется от `IRequest<Result<TResponse>>`)
- [x] Реализовать `IQuery<TResponse>` (наследуется от `IRequest<Result<TResponse>>`)
- [x] Реализовать `ICommandHandler<TCommand>` (наследуется от `IRequestHandler`)
- [x] Реализовать `ICommandHandler<TCommand, TResponse>`
- [x] Реализовать `IQueryHandler<TQuery, TResponse>`
- [x] Добавить NuGet-зависимость: `FluentValidation`
- [x] Реализовать `ValidationBehavior<TRequest, TResponse>` (MediatR Pipeline Behavior)
- [x] Убедиться, что проект собирается

#### 1.2.3 BuildingBlocks.Persistence

- [x] Создать проект `NovaCart.BuildingBlocks.Persistence` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимость на `BuildingBlocks.Common`
- [x] Реализовать `IRepository<T>` (generic interface: GetById, Add, Update, Delete)
- [x] Реализовать `IUnitOfWork` (SaveChangesAsync)
- [x] Реализовать `IPagedResult<T>` (Items, TotalCount, PageNumber, PageSize)
- [x] Реализовать `PagedResult<T>`
- [x] Убедиться, что проект собирается

---

### 1.3 Catalog Service

#### 1.3.1 Catalog.Domain

- [x] Создать проект `NovaCart.Services.Catalog.Domain` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `src/Services/Catalog`)
- [x] Добавить зависимость на `BuildingBlocks.Common`
- [x] Реализовать entity `Category` (Id, Name, Description, ParentCategoryId?, Slug)
- [x] Реализовать entity `Product` (Id, Name, Description, Slug, ImageUrl)
- [x] Реализовать value object `Price` (Amount: decimal, Currency: string)
- [x] Реализовать value object `ProductStatus` (Draft, Active, Discontinued)
- [x] Связать Product с Category (CategoryId, навигационное свойство)
- [x] Связать Product с Price (value object, owned type)
- [x] Реализовать domain events: `ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent`
- [x] Реализовать интерфейс `IProductRepository` (наследуется от `IRepository<Product>`, + специфичные методы)
- [x] Реализовать интерфейс `ICategoryRepository`
- [x] Убедиться, что проект собирается

#### 1.3.2 Catalog.Application

- [x] Создать проект `NovaCart.Services.Catalog.Application` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Catalog.Domain`, `BuildingBlocks.CQRS`, `BuildingBlocks.Persistence`
- [x] Создать папку `Products/Commands/`
- [x] Реализовать `CreateProductCommand` (record)
- [x] Реализовать `CreateProductHandler` → возвращает `Result<Guid>`
- [x] Реализовать `CreateProductValidator` (FluentValidation)
- [x] Реализовать `UpdateProductCommand`
- [x] Реализовать `UpdateProductHandler`
- [x] Реализовать `UpdateProductValidator`
- [x] Реализовать `DeleteProductCommand`
- [x] Реализовать `DeleteProductHandler`
- [x] Создать папку `Products/Queries/`
- [x] Реализовать `GetProductByIdQuery` → `Result<ProductDto>`
- [x] Реализовать `GetProductByIdHandler`
- [x] Реализовать `GetProductsQuery` (с пагинацией, фильтрацией, сортировкой)
- [x] Реализовать `GetProductsHandler` → `Result<PagedResult<ProductDto>>`
- [x] Создать папку `Products/Dtos/`
- [x] Реализовать `ProductDto`
- [x] Реализовать `CreateProductRequest`
- [x] Реализовать `UpdateProductRequest`
- [x] Создать папку `Categories/Commands/`
- [x] Реализовать `CreateCategoryCommand`, Handler, Validator
- [x] Реализовать `UpdateCategoryCommand`, Handler, Validator
- [x] Создать папку `Categories/Queries/`
- [x] Реализовать `GetCategoriesQuery`, Handler
- [x] Реализовать `GetCategoryByIdQuery`, Handler
- [x] Реализовать `CategoryDto`
- [x] Настроить DI-регистрацию: `AddCatalogApplication` (extension method)
- [x] Убедиться, что проект собирается

#### 1.3.3 Catalog.Infrastructure

- [x] Создать проект `NovaCart.Services.Catalog.Infrastructure` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Catalog.Domain`, `Catalog.Application`, `BuildingBlocks.Persistence`
- [x] Добавить NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Tools`
- [x] Реализовать `CatalogDbContext` (наследуется от `DbContext`)
- [x] Реализовать `ProductConfiguration` (`IEntityTypeConfiguration<Product>`) — snake_case таблицы
- [x] Реализовать `CategoryConfiguration` (`IEntityTypeConfiguration<Category>`) — snake_case таблицы
- [x] Настроить Price как Owned Type в конфигурации Product
- [x] Реализовать `ProductRepository` (имплементация `IProductRepository`)
- [x] Реализовать `CategoryRepository` (имплементация `ICategoryRepository`)
- [x] Реализовать `UnitOfWork` (имплементация `IUnitOfWork`, обёртка над `DbContext.SaveChangesAsync`)
- [x] Создать seed data: начальные категории (Electronics, Clothing, Books, Home & Garden)
- [x] Создать seed data: начальные товары (10–15 товаров)
- [x] Настроить DI-регистрацию: `AddCatalogInfrastructure` (extension method, принимает connection string)
- [x] Создать initial migration
- [x] Убедиться, что проект собирается

#### 1.3.4 Catalog.Contracts

- [x] Создать проект `NovaCart.Services.Catalog.Contracts` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Реализовать `ProductDto` (public DTO для внешних потребителей)
- [x] Реализовать `CategoryDto` (public DTO)
- [x] Убедиться, что проект собирается

#### 1.3.5 Catalog.API

- [x] Создать проект `NovaCart.Services.Catalog.API` (ASP.NET Core Web API, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Catalog.Application`, `Catalog.Infrastructure`, `ServiceDefaults`
- [x] Настроить `Program.cs`: AddServiceDefaults, AddCatalogApplication, AddCatalogInfrastructure
- [x] Настроить Swagger/OpenAPI
- [x] Реализовать Minimal API эндпоинты — Products:
  - [x] `GET /api/v1/products` — список с пагинацией
  - [x] `GET /api/v1/products/{id}` — по ID
  - [x] `POST /api/v1/products` — создание
  - [x] `PUT /api/v1/products/{id}` — обновление
  - [x] `DELETE /api/v1/products/{id}` — удаление
- [x] Реализовать Minimal API эндпоинты — Categories:
  - [x] `GET /api/v1/categories` — список
  - [x] `GET /api/v1/categories/{id}` — по ID
  - [x] `POST /api/v1/categories` — создание
  - [x] `PUT /api/v1/categories/{id}` — обновление
- [x] Настроить глобальную обработку ошибок (Exception Middleware → ProblemDetails)
- [x] Настроить автоматическое применение миграций при запуске (development only)
- [x] Зарегистрировать в Aspire AppHost с подключением к `catalogdb`
- [x] Убедиться, что API запускается и отвечает через Aspire

---

### 1.4 Ordering Service

#### 1.4.1 Ordering.Domain

- [x] Создать проект `NovaCart.Services.Ordering.Domain` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `src/Services/Ordering`)
- [x] Добавить зависимость на `BuildingBlocks.Common`
- [x] Реализовать value object `OrderStatus` (Created, Confirmed, Paid, Shipped, Delivered, Cancelled)
- [x] Реализовать value object `Address` (Street, City, State, Country, ZipCode)
- [x] Реализовать entity `OrderItem` (ProductId, ProductName, UnitPrice, Quantity)
- [x] Реализовать aggregate root `Order` (BuyerId, OrderDate, Status, ShippingAddress, Items)
- [x] Реализовать бизнес-методы в Order: `AddItem`, `RemoveItem`, `Cancel`, `Confirm`, `MarkAsPaid`, `Ship`, `Deliver`
- [x] Реализовать валидацию состояний (нельзя отменить доставленный заказ и т.д.)
- [x] Реализовать domain events: `OrderCreatedDomainEvent`, `OrderCancelledDomainEvent`, `OrderStatusChangedDomainEvent`
- [x] Реализовать интерфейс `IOrderRepository`
- [x] Убедиться, что проект собирается

#### 1.4.2 Ordering.Application

- [x] Создать проект `NovaCart.Services.Ordering.Application` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Ordering.Domain`, `BuildingBlocks.CQRS`, `BuildingBlocks.Persistence`
- [x] Реализовать `CreateOrderCommand`, Handler, Validator
- [x] Реализовать `CancelOrderCommand`, Handler, Validator
- [x] Реализовать `GetOrderByIdQuery`, Handler
- [x] Реализовать `GetOrdersQuery` (с фильтрацией по BuyerId, пагинацией), Handler
- [x] Реализовать `OrderDto`, `OrderItemDto`
- [x] Реализовать `CreateOrderRequest` (с items и shipping address)
- [x] Настроить DI-регистрацию: `AddOrderingApplication`
- [x] Убедиться, что проект собирается

#### 1.4.3 Ordering.Infrastructure

- [x] Создать проект `NovaCart.Services.Ordering.Infrastructure` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Ordering.Domain`, `Ordering.Application`, `BuildingBlocks.Persistence`
- [x] Добавить NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Tools`
- [x] Реализовать `OrderingDbContext`
- [x] Реализовать `OrderConfiguration` (IEntityTypeConfiguration) — snake_case, owned types для Address
- [x] Реализовать `OrderItemConfiguration`
- [x] Реализовать `OrderRepository`
- [x] Реализовать `UnitOfWork`
- [x] Настроить DI-регистрацию: `AddOrderingInfrastructure`
- [x] Создать initial migration
- [x] Убедиться, что проект собирается

#### 1.4.4 Ordering.Contracts

- [x] Создать проект `NovaCart.Services.Ordering.Contracts` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Реализовать `OrderDto` (public DTO)
- [x] Реализовать `OrderItemDto` (public DTO)
- [x] Убедиться, что проект собирается

#### 1.4.5 Ordering.API

- [x] Создать проект `NovaCart.Services.Ordering.API` (ASP.NET Core Web API, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Ordering.Application`, `Ordering.Infrastructure`, `ServiceDefaults`
- [x] Настроить `Program.cs`
- [x] Реализовать Minimal API эндпоинты:
  - [x] `GET /api/v1/orders` — список заказов (фильтр по buyerId)
  - [x] `GET /api/v1/orders/{id}` — заказ по ID
  - [x] `POST /api/v1/orders` — создание заказа
  - [x] `PUT /api/v1/orders/{id}/cancel` — отмена заказа
- [x] Настроить глобальную обработку ошибок
- [x] Настроить автоматическое применение миграций (development only)
- [x] Зарегистрировать в Aspire AppHost с подключением к `orderingdb`
- [x] Убедиться, что API запускается и отвечает через Aspire

---

### 1.5 Identity Service

#### 1.5.1 Identity.Domain

- [x] Создать проект `NovaCart.Services.Identity.Domain` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `src/Services/Identity`)
- [x] Добавить зависимость на `BuildingBlocks.Common`
- [x] Реализовать `ApplicationUser` (наследуется от `IdentityUser`, добавить FirstName, LastName)
- [x] Реализовать `UserRole` (enum или constants: Admin, Customer)
- [x] Убедиться, что проект собирается

#### 1.5.2 Identity.Application

- [x] Создать проект `NovaCart.Services.Identity.Application` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Identity.Domain`, `BuildingBlocks.CQRS`
- [x] Реализовать `RegisterCommand`, Handler, Validator
- [x] Реализовать `LoginCommand`, Handler, Validator → возвращает `Result<TokenResponse>`
- [x] Реализовать `RefreshTokenCommand`, Handler
- [x] Реализовать `GetCurrentUserQuery`, Handler → возвращает `Result<UserDto>`
- [x] Реализовать `TokenResponse` (AccessToken, RefreshToken, ExpiresAt)
- [x] Реализовать `UserDto` (Id, Email, FirstName, LastName, Roles)
- [x] Реализовать `RegisterRequest`, `LoginRequest`
- [x] Реализовать интерфейс `ITokenService` (GenerateAccessToken, GenerateRefreshToken, ValidateRefreshToken)
- [x] Настроить DI-регистрацию: `AddIdentityApplication`
- [x] Убедиться, что проект собирается

#### 1.5.3 Identity.Infrastructure

- [x] Создать проект `NovaCart.Services.Identity.Infrastructure` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Identity.Domain`, `Identity.Application`
- [x] Добавить NuGet: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`
- [x] Реализовать `IdentityAppDbContext` (наследуется от `IdentityDbContext<ApplicationUser>`)
- [x] Реализовать `TokenService` (имплементация `ITokenService`, генерация JWT)
- [x] Настроить JWT-параметры (Issuer, Audience, Secret, Expiration) — `JwtSettings` class
- [x] Создать seed data: Admin user (admin@novacart.com) — runtime seed via `SeedAdminUserAsync`
- [x] Создать seed data: роли (Admin, Customer) — migration seed via `HasData`
- [x] Реализовать `UserRepository` (имплементация `IUserRepository`)
- [x] Настроить DI-регистрацию: `AddIdentityInfrastructure`
- [x] Создать initial migration
- [x] Убедиться, что проект собирается

#### 1.5.4 Identity.Contracts

- [x] Создать проект `NovaCart.Services.Identity.Contracts` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Реализовать `UserDto` (public DTO)
- [x] Убедиться, что проект собирается

#### 1.5.5 Identity.API

- [x] Создать проект `NovaCart.Services.Identity.API` (ASP.NET Core Web API, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить зависимости: `Identity.Application`, `Identity.Infrastructure`, `ServiceDefaults`
- [x] Настроить `Program.cs`: ASP.NET Core Identity, JWT Bearer authentication
- [x] Реализовать Minimal API эндпоинты:
  - [x] `POST /api/v1/auth/register` — регистрация
  - [x] `POST /api/v1/auth/login` — вход
  - [x] `POST /api/v1/auth/refresh` — обновление токена
  - [x] `GET /api/v1/auth/me` — текущий пользователь (требует авторизации)
- [x] Настроить глобальную обработку ошибок
- [x] Настроить автоматическое применение миграций (development only)
- [x] Зарегистрировать в Aspire AppHost с подключением к `identitydb`
- [x] Убедиться, что API запускается и отвечает через Aspire

---

### 1.6 API Gateway (YARP)

- [x] Создать проект `NovaCart.ApiGateway.Yarp` (ASP.NET Core Web API, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `src/ApiGateway`)
- [x] Добавить NuGet: `Yarp.ReverseProxy`
- [x] Добавить зависимость на `ServiceDefaults`
- [x] Настроить `Program.cs`: AddReverseProxy, LoadFromConfig
- [x] Настроить `appsettings.json` — маршруты:
  - [x] `/api/v1/products/**` → `catalog-api`
  - [x] `/api/v1/categories/**` → `catalog-api`
  - [x] `/api/v1/orders/**` → `ordering-api`
  - [x] `/api/v1/auth/**` → `identity-api`
- [x] Настроить Aspire service discovery в маршрутах (использовать имена сервисов, а не адреса)
- [x] Настроить прокидывание заголовков (Authorization, Content-Type)
- [x] Зарегистрировать в Aspire AppHost с `WithReference` на все API-сервисы
- [x] Убедиться, что Gateway запускается и проксирует запросы

---

### 1.7 Blazor Web App (Фронтенд + BFF)

#### 1.7.1 NovaCart.Web (Server — BFF)

- [x] Создать проект `NovaCart.Web` (Blazor Web App, .NET 10, Auto render mode, Per page/component)
- [x] Добавить в `NovaCart.slnx` (solution folder `src/Web`)
- [x] Добавить зависимость на `ServiceDefaults`
- [x] Добавить NuGet: `MudBlazor`
- [x] Настроить `Program.cs`: AddServiceDefaults, MudBlazor services
- [x] Настроить `App.razor`: MudBlazor providers, HeadOutlet
- [x] Настроить Layout: `MainLayout.razor` с MudBlazor (AppBar, NavMenu, Footer)
- [x] Создать BFF-сервисы (server-side, папка `Services/`):
  - [x] `CatalogService` — вызовы к Gateway для товаров и категорий
  - [x] `OrderService` — вызовы к Gateway для заказов
  - [x] `AuthService` — вызовы к Gateway для аутентификации
- [x] Настроить HttpClient для BFF → Gateway (через Aspire service discovery)
- [x] Создать SSR-страницы (папка `Components/Pages/`):
  - [x] `Home.razor` — главная страница (лендинг)
  - [x] `CatalogPage.razor` — каталог товаров с пагинацией
  - [x] `ProductPage.razor` — страница отдельного товара
- [x] Зарегистрировать в Aspire AppHost с `WithReference(gateway)`, `WithExternalHttpEndpoints`
- [x] Убедиться, что Web App запускается через Aspire

#### 1.7.2 NovaCart.Web.Client (WASM — интерактивные компоненты)

- [x] Создать проект `NovaCart.Web.Client` (Blazor WebAssembly, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить NuGet: `MudBlazor`, `Microsoft.AspNetCore.Components.WebAssembly`
- [x] Настроить `Program.cs`: HttpClient, MudBlazor services
- [x] Создать client-side сервисы (папка `Services/`):
  - [x] `AuthClientService` — HTTP-вызовы для логина/регистрации
- [x] Создать интерактивные компоненты (папка `Pages/`):
  - [x] `Login.razor` (`@rendermode InteractiveAuto`) — форма логина
  - [x] `Register.razor` (`@rendermode InteractiveAuto`) — форма регистрации
- [x] Убедиться, что интерактивные компоненты работают

---

### 1.8 Aspire AppHost — финальная конфигурация Phase 1

- [x] AppHost `Program.cs` содержит:
  - [x] PostgreSQL с 3 базами (catalogdb, orderingdb, identitydb)
  - [x] Catalog API с подключением к catalogdb
  - [x] Ordering API с подключением к orderingdb
  - [x] Identity API с подключением к identitydb
  - [x] YARP Gateway с references на все API
  - [x] Web App с reference на Gateway
- [x] Все сервисы запускаются через единый `dotnet run` из AppHost
- [x] Aspire Dashboard доступен и показывает все сервисы
- [x] Health checks работают для всех сервисов

---

### 1.9 Тесты (Phase 1)

#### 1.9.1 Unit-тесты — Catalog

- [x] Создать проект `NovaCart.Tests.Catalog.UnitTests` (xUnit, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `tests/UnitTests`)
- [x] Добавить NuGet: `FluentAssertions`, `NSubstitute`
- [x] Тесты для `Product` entity:
  - [x] Создание продукта с валидными данными
  - [x] Создание продукта с невалидной ценой
- [x] Тесты для `Price` value object:
  - [x] Equality
  - [x] Невалидные значения
- [x] Тесты для `CreateProductHandler`:
  - [x] Успешное создание
  - [x] Валидация — пустое имя
  - [x] Валидация — отрицательная цена
- [x] Тесты для `GetProductByIdHandler`:
  - [x] Продукт найден
  - [x] Продукт не найден → NotFound

#### 1.9.2 Unit-тесты — Ordering

- [x] Создать проект `NovaCart.Tests.Ordering.UnitTests` (xUnit, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Тесты для `Order` aggregate:
  - [x] Создание заказа
  - [x] Добавление элементов
  - [x] Отмена заказа
  - [x] Нельзя отменить доставленный заказ
  - [x] Переход статусов (state machine)
- [x] Тесты для `CreateOrderHandler`
- [x] Тесты для `CancelOrderHandler`

#### 1.9.3 Architecture-тесты

- [x] Создать проект `NovaCart.Tests.ArchitectureTests` (xUnit, .NET 10)
- [x] Добавить в `NovaCart.slnx` (solution folder `tests/ArchitectureTests`)
- [x] Добавить NuGet: `NetArchTest.Rules`
- [x] Тест: Domain не ссылается на Application
- [x] Тест: Domain не ссылается на Infrastructure
- [x] Тест: Domain не ссылается на API
- [x] Тест: Application не ссылается на Infrastructure
- [x] Тест: Application не ссылается на API
- [x] Тест: Infrastructure не ссылается на API
- [x] Все тесты проходят

---

### 1.10 Финальная валидация Phase 1

- [x] Solution собирается без ошибок (`dotnet build`)
- [x] Все сервисы запускаются через Aspire AppHost
- [x] Catalog API: CRUD товаров работает
- [x] Catalog API: пагинация работает
- [x] Catalog API: seed data загружены
- [x] Ordering API: создание и отмена заказов работает
- [x] Identity API: регистрация и логин работают
- [x] Identity API: JWT-токен генерируется корректно
- [x] YARP Gateway: проксирует запросы ко всем сервисам
- [x] Blazor Web App: SSR-страницы отображают данные из каталога
- [x] Blazor Web App: интерактивный логин работает
- [x] Все unit-тесты проходят
- [x] Все architecture-тесты проходят
- [x] Aspire Dashboard показывает логи и трейсы всех сервисов

---

## 🟡 Phase 2 — Microservices Communication (Коммуникация)

### 2.1 BuildingBlocks.EventBus

- [x] Создать проект `NovaCart.BuildingBlocks.EventBus` (Class Library, .NET 10)
- [x] Добавить в `NovaCart.slnx`
- [x] Добавить NuGet: `MassTransit`, `MassTransit.RabbitMQ`
- [x] Реализовать `IntegrationEvent` (базовый класс: Id, CreatedAt, CorrelationId)
- [x] Реализовать extension method `AddEventBus` (настройка MassTransit + RabbitMQ)
- [x] Убедиться, что проект собирается

### 2.2 Aspire — добавить RabbitMQ и Redis

- [x] Добавить `builder.AddRabbitMQ("rabbitmq")` в AppHost
- [x] Добавить `builder.AddRedis("redis")` в AppHost
- [x] Подключить rabbitmq ко всем API-сервисам

### 2.3 Basket Service

#### 2.3.1 Basket.Domain

- [x] Создать проект `NovaCart.Services.Basket.Domain`
- [x] Реализовать `ShoppingCart` entity (BuyerId, Items, TotalPrice)
- [x] Реализовать `BasketItem` (ProductId, ProductName, Price, Quantity)
- [x] Бизнес-методы: AddItem, RemoveItem, UpdateItemQuantity, Clear
- [x] JSON-сериализация для Redis (JsonConstructor, JsonInclude)

#### 2.3.2 Basket.Application

- [x] Создать проект `NovaCart.Services.Basket.Application`
- [x] Реализовать `GetBasketQuery`, Handler
- [x] Реализовать `UpdateBasketCommand`, Handler, Validator
- [x] Реализовать `DeleteBasketCommand`, Handler, Validator
- [x] Реализовать `CheckoutBasketCommand`, Handler, Validator → публикует `BasketCheckoutIntegrationEvent`
- [x] Реализовать DTOs: `BasketDto`, `BasketItemDto`, `UpdateBasketRequest`
- [x] Настроить DI-регистрацию: `AddBasketApplication`

#### 2.3.3 Basket.Infrastructure

- [x] Создать проект `NovaCart.Services.Basket.Infrastructure`
- [x] Реализовать `RedisBasketRepository` — Redis (IDistributedCache, JSON serialization)
- [x] Настроить TTL для корзины (30-day sliding expiration)
- [x] Настроить DI-регистрацию: `AddBasketInfrastructure`

#### 2.3.4 Basket.Contracts

- [x] Создать проект `NovaCart.Services.Basket.Contracts`
- [x] Реализовать `BasketCheckoutIntegrationEvent`

#### 2.3.5 Basket.API

- [x] Создать проект `NovaCart.Services.Basket.API`
- [x] Эндпоинты: GET, PUT, DELETE `/api/v1/baskets/{buyerId}`, POST `/api/v1/baskets/checkout`
- [x] Зарегистрировать в Aspire AppHost с Redis и RabbitMQ
- [x] Добавить маршрут в YARP Gateway

### 2.4 Payment Service

#### 2.4.1 Payment.Domain

- [x] Создать проект `NovaCart.Services.Payment.Domain`
- [x] Реализовать `Payment` entity (OrderId, Amount, Status, ProcessedAt)
- [x] Реализовать `PaymentStatus` (Pending, Succeeded, Failed)

#### 2.4.2 Payment.Application

- [x] Создать проект `NovaCart.Services.Payment.Application`
- [x] Реализовать consumer `OrderCreatedIntegrationEventHandler`
- [x] Реализовать симуляцию оплаты (random success/failure)
- [x] Публикация `PaymentSucceededIntegrationEvent` / `PaymentFailedIntegrationEvent`

#### 2.4.3 Payment.Infrastructure

- [x] Создать проект `NovaCart.Services.Payment.Infrastructure`
- [x] `PaymentDbContext` + PostgreSQL (paymentdb)
- [x] Реализовать `PaymentRepository`

#### 2.4.4 Payment.Contracts

- [x] Создать проект `NovaCart.Services.Payment.Contracts`
- [x] `PaymentSucceededIntegrationEvent`
- [x] `PaymentFailedIntegrationEvent`

#### 2.4.5 Payment.API

- [x] Создать проект `NovaCart.Services.Payment.API`
- [x] Зарегистрировать в Aspire AppHost

### 2.5 Integration Events — связать сервисы

- [x] Ordering Service: consumer для `BasketCheckoutIntegrationEvent` → создание Order
- [x] Ordering Service: consumer для `PaymentSucceededIntegrationEvent` → статус Paid
- [x] Ordering Service: consumer для `PaymentFailedIntegrationEvent` → статус Cancelled
- [x] Ordering Service: публикует `OrderCreatedIntegrationEvent` при создании заказа
- [x] Ordering.Contracts: добавить `OrderCreatedIntegrationEvent`
- [ ] Протестировать полный цикл: Basket Checkout → Order Created → Payment → Order Updated

### 2.6 Outbox Pattern

- [ ] Добавить таблицу `outbox_messages` в `BuildingBlocks.Persistence`
- [ ] Реализовать `OutboxMessage` entity
- [ ] Реализовать `OutboxInterceptor` (SaveChanges → сохранить события в outbox)
- [ ] Реализовать `OutboxProcessor` (BackgroundService → отправить события в RabbitMQ)
- [ ] Подключить к Ordering и Payment сервисам

### 2.7 Resilience

- [ ] Настроить Polly retry + circuit breaker в ServiceDefaults для всех HttpClient
- [ ] Добавить timeout-ы
- [ ] Проверить поведение при недоступности сервиса

### 2.8 Blazor Web App — расширение

- [ ] Страница корзины (`@rendermode InteractiveAuto`)
- [ ] Страница оформления заказа
- [ ] Страница "Мои заказы"
- [ ] BFF-сервисы для Basket и Orders

### 2.9 Тесты Phase 2

- [ ] Integration-тесты с Testcontainers (PostgreSQL, RabbitMQ, Redis)
- [ ] Unit-тесты для Basket, Payment сервисов
- [ ] Тест полного цикла заказа

### 2.10 Финальная валидация Phase 2

- [ ] Полный цикл: добавление в корзину → checkout → заказ → оплата → статус обновлён
- [ ] Outbox гарантирует доставку событий
- [ ] Resilience: система устойчива к временным сбоям
- [ ] Все тесты проходят

---

## 🟠 Phase 3 — Advanced Patterns (Продвинутые паттерны)

### 3.1 Saga Pattern

- [ ] Реализовать `OrderSaga` через MassTransit State Machine
- [ ] Состояния: Submitted → AwaitingPayment → Paid → Completed / Failed
- [ ] Компенсационные действия при ошибке оплаты
- [ ] Тесты для Saga

### 3.2 Notification Service

- [ ] Создать сервис (Application + Infrastructure + API + Contracts)
- [ ] Consumer для `OrderConfirmedIntegrationEvent`, `PaymentSucceededIntegrationEvent`
- [ ] Email-уведомления (MailDev в Docker через Aspire)
- [ ] SignalR hub для real-time уведомлений
- [ ] Зарегистрировать в Aspire

### 3.3 Search Service

- [ ] Создать сервис (Application + Infrastructure + API + Contracts)
- [ ] Elasticsearch через Aspire
- [ ] Consumer для `ProductCreatedIntegrationEvent`, `ProductUpdatedIntegrationEvent`
- [ ] Эндпоинты: полнотекстовый поиск, автокомплит, фильтры
- [ ] Blazor: поисковая строка с автокомплитом

### 3.4 gRPC для внутренней коммуникации

- [ ] gRPC-контракт: Ordering → Catalog (получение деталей товара)
- [ ] Protobuf-файлы в Catalog.Contracts
- [ ] gRPC-сервер в Catalog.API
- [ ] gRPC-клиент в Ordering.Infrastructure

### 3.5 BFF-агрегация

- [ ] Агрегатор: страница заказа собирает данные из Ordering + Catalog + Payment
- [ ] Параллельные вызовы (`Task.WhenAll`)
- [ ] Кэширование ответов каталога

### 3.6 Тесты Phase 3

- [ ] Contract-тесты между сервисами
- [ ] E2E-тесты (Playwright)
- [ ] Тесты Saga

### 3.7 Финальная валидация Phase 3

- [ ] Saga корректно обрабатывает успешные и неуспешные сценарии
- [ ] Поиск работает и обновляется при изменении товаров
- [ ] Уведомления приходят
- [ ] Все тесты проходят

---

## 🔴 Phase 4 — Production-Ready (Продакшен)

### 4.1 Observability

- [ ] Prometheus — сбор метрик
- [ ] Grafana — дашборд
- [ ] Jaeger — трассировка
- [ ] Centralized logging (Seq или ELK)

### 4.2 CI/CD

- [ ] GitHub Actions: build workflow
- [ ] GitHub Actions: test workflow
- [ ] GitHub Actions: deploy workflow
- [ ] Multi-stage Dockerfile для каждого сервиса
- [ ] Docker Compose для staging

### 4.3 Безопасность

- [ ] Rate Limiting на YARP
- [ ] CORS-политики
- [ ] API Key для внешних интеграций
- [ ] Audit log

### 4.4 Дополнительные фичи

- [ ] Feature Flags
- [ ] Admin Panel (Blazor, ролевой доступ)
- [ ] Shipment Service (имитация доставки)

### 4.5 Kubernetes (опционально)

- [ ] Helm charts
- [ ] Deployment, Service, Ingress
- [ ] Aspire manifest → K8s

### 4.6 Документация

- [ ] C4-диаграммы
- [ ] ADR (отдельные файлы в docs/architecture/decisions/)
- [ ] API-документация (OpenAPI)
- [ ] Финальный README

### 4.7 Финальная валидация Phase 4

- [ ] CI/CD pipeline работает
- [ ] Observability: метрики, логи, трейсы доступны
- [ ] Система запускается в Docker
- [ ] Документация полная и актуальная
