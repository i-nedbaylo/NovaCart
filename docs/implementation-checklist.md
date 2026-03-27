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

- [ ] Создать проект `NovaCart.BuildingBlocks.Common` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `src/BuildingBlocks`)
- [ ] Реализовать `BaseEntity` (Id: Guid, CreatedAt, UpdatedAt)
- [ ] Реализовать `AggregateRoot` (наследуется от BaseEntity, список Domain Events)
- [ ] Реализовать `IDomainEvent` (интерфейс-маркер)
- [ ] Реализовать `Result<T>` (Success/Failure, Error messages)
- [ ] Реализовать `Result` (без generic — для void-операций)
- [ ] Реализовать `Error` (код ошибки + сообщение)
- [ ] Реализовать базовые исключения: `NotFoundException`, `ValidationException`, `ConflictException`
- [ ] Реализовать `IDateTimeProvider` (абстракция для тестируемости)
- [ ] Реализовать `DateTimeProvider` (реализация, использует `DateTimeOffset.UtcNow`)
- [ ] Убедиться, что проект собирается

#### 1.2.2 BuildingBlocks.CQRS

- [ ] Создать проект `NovaCart.BuildingBlocks.CQRS` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить NuGet-зависимость: `MediatR`
- [ ] Реализовать `ICommand` (наследуется от `IRequest<Result>`)
- [ ] Реализовать `ICommand<TResponse>` (наследуется от `IRequest<Result<TResponse>>`)
- [ ] Реализовать `IQuery<TResponse>` (наследуется от `IRequest<Result<TResponse>>`)
- [ ] Реализовать `ICommandHandler<TCommand>` (наследуется от `IRequestHandler`)
- [ ] Реализовать `ICommandHandler<TCommand, TResponse>`
- [ ] Реализовать `IQueryHandler<TQuery, TResponse>`
- [ ] Добавить NuGet-зависимость: `FluentValidation`
- [ ] Реализовать `ValidationBehavior<TRequest, TResponse>` (MediatR Pipeline Behavior)
- [ ] Убедиться, что проект собирается

#### 1.2.3 BuildingBlocks.Persistence

- [ ] Создать проект `NovaCart.BuildingBlocks.Persistence` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимость на `BuildingBlocks.Common`
- [ ] Реализовать `IRepository<T>` (generic interface: GetById, Add, Update, Delete)
- [ ] Реализовать `IUnitOfWork` (SaveChangesAsync)
- [ ] Реализовать `IPagedResult<T>` (Items, TotalCount, PageNumber, PageSize)
- [ ] Реализовать `PagedResult<T>`
- [ ] Убедиться, что проект собирается

---

### 1.3 Catalog Service

#### 1.3.1 Catalog.Domain

- [ ] Создать проект `NovaCart.Services.Catalog.Domain` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `src/Services/Catalog`)
- [ ] Добавить зависимость на `BuildingBlocks.Common`
- [ ] Реализовать entity `Category` (Id, Name, Description, ParentCategoryId?, Slug)
- [ ] Реализовать entity `Product` (Id, Name, Description, Slug, ImageUrl)
- [ ] Реализовать value object `Price` (Amount: decimal, Currency: string)
- [ ] Реализовать value object `ProductStatus` (Draft, Active, Discontinued)
- [ ] Связать Product с Category (CategoryId, навигационное свойство)
- [ ] Связать Product с Price (value object, owned type)
- [ ] Реализовать domain events: `ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent`
- [ ] Реализовать интерфейс `IProductRepository` (наследуется от `IRepository<Product>`, + специфичные методы)
- [ ] Реализовать интерфейс `ICategoryRepository`
- [ ] Убедиться, что проект собирается

#### 1.3.2 Catalog.Application

- [ ] Создать проект `NovaCart.Services.Catalog.Application` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Catalog.Domain`, `BuildingBlocks.CQRS`, `BuildingBlocks.Persistence`
- [ ] Создать папку `Products/Commands/`
- [ ] Реализовать `CreateProductCommand` (record)
- [ ] Реализовать `CreateProductHandler` → возвращает `Result<Guid>`
- [ ] Реализовать `CreateProductValidator` (FluentValidation)
- [ ] Реализовать `UpdateProductCommand`
- [ ] Реализовать `UpdateProductHandler`
- [ ] Реализовать `UpdateProductValidator`
- [ ] Реализовать `DeleteProductCommand`
- [ ] Реализовать `DeleteProductHandler`
- [ ] Создать папку `Products/Queries/`
- [ ] Реализовать `GetProductByIdQuery` → `Result<ProductDto>`
- [ ] Реализовать `GetProductByIdHandler`
- [ ] Реализовать `GetProductsQuery` (с пагинацией, фильтрацией, сортировкой)
- [ ] Реализовать `GetProductsHandler` → `Result<PagedResult<ProductDto>>`
- [ ] Создать папку `Products/Dtos/`
- [ ] Реализовать `ProductDto`
- [ ] Реализовать `CreateProductRequest`
- [ ] Реализовать `UpdateProductRequest`
- [ ] Создать папку `Categories/Commands/`
- [ ] Реализовать `CreateCategoryCommand`, Handler, Validator
- [ ] Реализовать `UpdateCategoryCommand`, Handler, Validator
- [ ] Создать папку `Categories/Queries/`
- [ ] Реализовать `GetCategoriesQuery`, Handler
- [ ] Реализовать `GetCategoryByIdQuery`, Handler
- [ ] Реализовать `CategoryDto`
- [ ] Настроить DI-регистрацию: `AddCatalogApplication` (extension method)
- [ ] Убедиться, что проект собирается

#### 1.3.3 Catalog.Infrastructure

- [ ] Создать проект `NovaCart.Services.Catalog.Infrastructure` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Catalog.Domain`, `Catalog.Application`, `BuildingBlocks.Persistence`
- [ ] Добавить NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Tools`
- [ ] Реализовать `CatalogDbContext` (наследуется от `DbContext`)
- [ ] Реализовать `ProductConfiguration` (`IEntityTypeConfiguration<Product>`) — snake_case таблицы
- [ ] Реализовать `CategoryConfiguration` (`IEntityTypeConfiguration<Category>`) — snake_case таблицы
- [ ] Настроить Price как Owned Type в конфигурации Product
- [ ] Реализовать `ProductRepository` (имплементация `IProductRepository`)
- [ ] Реализовать `CategoryRepository` (имплементация `ICategoryRepository`)
- [ ] Реализовать `UnitOfWork` (имплементация `IUnitOfWork`, обёртка над `DbContext.SaveChangesAsync`)
- [ ] Создать seed data: начальные категории (Electronics, Clothing, Books, Home & Garden)
- [ ] Создать seed data: начальные товары (10–15 товаров)
- [ ] Настроить DI-регистрацию: `AddCatalogInfrastructure` (extension method, принимает connection string)
- [ ] Создать initial migration
- [ ] Убедиться, что проект собирается

#### 1.3.4 Catalog.Contracts

- [ ] Создать проект `NovaCart.Services.Catalog.Contracts` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Реализовать `ProductDto` (public DTO для внешних потребителей)
- [ ] Реализовать `CategoryDto` (public DTO)
- [ ] Убедиться, что проект собирается

#### 1.3.5 Catalog.API

- [ ] Создать проект `NovaCart.Services.Catalog.API` (ASP.NET Core Web API, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Catalog.Application`, `Catalog.Infrastructure`, `ServiceDefaults`
- [ ] Настроить `Program.cs`: AddServiceDefaults, AddCatalogApplication, AddCatalogInfrastructure
- [ ] Настроить Swagger/OpenAPI
- [ ] Реализовать Minimal API эндпоинты — Products:
  - [ ] `GET /api/v1/products` — список с пагинацией
  - [ ] `GET /api/v1/products/{id}` — по ID
  - [ ] `POST /api/v1/products` — создание
  - [ ] `PUT /api/v1/products/{id}` — обновление
  - [ ] `DELETE /api/v1/products/{id}` — удаление
- [ ] Реализовать Minimal API эндпоинты — Categories:
  - [ ] `GET /api/v1/categories` — список
  - [ ] `GET /api/v1/categories/{id}` — по ID
  - [ ] `POST /api/v1/categories` — создание
  - [ ] `PUT /api/v1/categories/{id}` — обновление
- [ ] Настроить глобальную обработку ошибок (Exception Middleware → ProblemDetails)
- [ ] Настроить автоматическое применение миграций при запуске (development only)
- [ ] Зарегистрировать в Aspire AppHost с подключением к `catalogdb`
- [ ] Убедиться, что API запускается и отвечает через Aspire

---

### 1.4 Ordering Service

#### 1.4.1 Ordering.Domain

- [ ] Создать проект `NovaCart.Services.Ordering.Domain` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `src/Services/Ordering`)
- [ ] Добавить зависимость на `BuildingBlocks.Common`
- [ ] Реализовать value object `OrderStatus` (Created, Confirmed, Paid, Shipped, Delivered, Cancelled)
- [ ] Реализовать value object `Address` (Street, City, State, Country, ZipCode)
- [ ] Реализовать entity `OrderItem` (ProductId, ProductName, UnitPrice, Quantity)
- [ ] Реализовать aggregate root `Order` (BuyerId, OrderDate, Status, ShippingAddress, Items)
- [ ] Реализовать бизнес-методы в Order: `AddItem`, `RemoveItem`, `Cancel`, `Confirm`, `MarkAsPaid`, `Ship`, `Deliver`
- [ ] Реализовать валидацию состояний (нельзя отменить доставленный заказ и т.д.)
- [ ] Реализовать domain events: `OrderCreatedDomainEvent`, `OrderCancelledDomainEvent`, `OrderStatusChangedDomainEvent`
- [ ] Реализовать интерфейс `IOrderRepository`
- [ ] Убедиться, что проект собирается

#### 1.4.2 Ordering.Application

- [ ] Создать проект `NovaCart.Services.Ordering.Application` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Ordering.Domain`, `BuildingBlocks.CQRS`, `BuildingBlocks.Persistence`
- [ ] Реализовать `CreateOrderCommand`, Handler, Validator
- [ ] Реализовать `CancelOrderCommand`, Handler, Validator
- [ ] Реализовать `GetOrderByIdQuery`, Handler
- [ ] Реализовать `GetOrdersQuery` (с фильтрацией по BuyerId, пагинацией), Handler
- [ ] Реализовать `OrderDto`, `OrderItemDto`
- [ ] Реализовать `CreateOrderRequest` (с items и shipping address)
- [ ] Настроить DI-регистрацию: `AddOrderingApplication`
- [ ] Убедиться, что проект собирается

#### 1.4.3 Ordering.Infrastructure

- [ ] Создать проект `NovaCart.Services.Ordering.Infrastructure` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Ordering.Domain`, `Ordering.Application`, `BuildingBlocks.Persistence`
- [ ] Добавить NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Tools`
- [ ] Реализовать `OrderingDbContext`
- [ ] Реализовать `OrderConfiguration` (IEntityTypeConfiguration) — snake_case, owned types для Address
- [ ] Реализовать `OrderItemConfiguration`
- [ ] Реализовать `OrderRepository`
- [ ] Реализовать `UnitOfWork`
- [ ] Настроить DI-регистрацию: `AddOrderingInfrastructure`
- [ ] Создать initial migration
- [ ] Убедиться, что проект собирается

#### 1.4.4 Ordering.Contracts

- [ ] Создать проект `NovaCart.Services.Ordering.Contracts` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Реализовать `OrderDto` (public DTO)
- [ ] Реализовать `OrderItemDto` (public DTO)
- [ ] Убедиться, что проект собирается

#### 1.4.5 Ordering.API

- [ ] Создать проект `NovaCart.Services.Ordering.API` (ASP.NET Core Web API, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Ordering.Application`, `Ordering.Infrastructure`, `ServiceDefaults`
- [ ] Настроить `Program.cs`
- [ ] Реализовать Minimal API эндпоинты:
  - [ ] `GET /api/v1/orders` — список заказов (фильтр по buyerId)
  - [ ] `GET /api/v1/orders/{id}` — заказ по ID
  - [ ] `POST /api/v1/orders` — создание заказа
  - [ ] `PUT /api/v1/orders/{id}/cancel` — отмена заказа
- [ ] Настроить глобальную обработку ошибок
- [ ] Настроить автоматическое применение миграций (development only)
- [ ] Зарегистрировать в Aspire AppHost с подключением к `orderingdb`
- [ ] Убедиться, что API запускается и отвечает через Aspire

---

### 1.5 Identity Service

#### 1.5.1 Identity.Domain

- [ ] Создать проект `NovaCart.Services.Identity.Domain` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `src/Services/Identity`)
- [ ] Добавить зависимость на `BuildingBlocks.Common`
- [ ] Реализовать `ApplicationUser` (наследуется от `IdentityUser`, добавить FirstName, LastName)
- [ ] Реализовать `UserRole` (enum или constants: Admin, Customer)
- [ ] Убедиться, что проект собирается

#### 1.5.2 Identity.Application

- [ ] Создать проект `NovaCart.Services.Identity.Application` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Identity.Domain`, `BuildingBlocks.CQRS`
- [ ] Реализовать `RegisterCommand`, Handler, Validator
- [ ] Реализовать `LoginCommand`, Handler, Validator → возвращает `Result<TokenResponse>`
- [ ] Реализовать `RefreshTokenCommand`, Handler
- [ ] Реализовать `GetCurrentUserQuery`, Handler → возвращает `Result<UserDto>`
- [ ] Реализовать `TokenResponse` (AccessToken, RefreshToken, ExpiresAt)
- [ ] Реализовать `UserDto` (Id, Email, FirstName, LastName, Roles)
- [ ] Реализовать `RegisterRequest`, `LoginRequest`
- [ ] Реализовать интерфейс `ITokenService` (GenerateAccessToken, GenerateRefreshToken, ValidateRefreshToken)
- [ ] Настроить DI-регистрацию: `AddIdentityApplication`
- [ ] Убедиться, что проект собирается

#### 1.5.3 Identity.Infrastructure

- [ ] Создать проект `NovaCart.Services.Identity.Infrastructure` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Identity.Domain`, `Identity.Application`
- [ ] Добавить NuGet: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] Реализовать `IdentityDbContext` (наследуется от `IdentityDbContext<ApplicationUser>`)
- [ ] Реализовать `TokenService` (имплементация `ITokenService`, генерация JWT)
- [ ] Настроить JWT-параметры (Issuer, Audience, Secret, Expiration)
- [ ] Создать seed data: Admin user (admin@novacart.com)
- [ ] Создать seed data: роли (Admin, Customer)
- [ ] Настроить DI-регистрацию: `AddIdentityInfrastructure`
- [ ] Создать initial migration
- [ ] Убедиться, что проект собирается

#### 1.5.4 Identity.Contracts

- [ ] Создать проект `NovaCart.Services.Identity.Contracts` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Реализовать `UserDto` (public DTO)
- [ ] Убедиться, что проект собирается

#### 1.5.5 Identity.API

- [ ] Создать проект `NovaCart.Services.Identity.API` (ASP.NET Core Web API, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить зависимости: `Identity.Application`, `Identity.Infrastructure`, `ServiceDefaults`
- [ ] Настроить `Program.cs`: ASP.NET Core Identity, JWT Bearer authentication
- [ ] Реализовать Minimal API эндпоинты:
  - [ ] `POST /api/v1/auth/register` — регистрация
  - [ ] `POST /api/v1/auth/login` — вход
  - [ ] `POST /api/v1/auth/refresh` — обновление токена
  - [ ] `GET /api/v1/auth/me` — текущий пользователь (требует авторизации)
- [ ] Настроить глобальную обработку ошибок
- [ ] Настроить автоматическое применение миграций (development only)
- [ ] Зарегистрировать в Aspire AppHost с подключением к `identitydb`
- [ ] Убедиться, что API запускается и отвечает через Aspire

---

### 1.6 API Gateway (YARP)

- [ ] Создать проект `NovaCart.ApiGateway.Yarp` (ASP.NET Core Web API, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `src/ApiGateway`)
- [ ] Добавить NuGet: `Yarp.ReverseProxy`
- [ ] Добавить зависимость на `ServiceDefaults`
- [ ] Настроить `Program.cs`: AddReverseProxy, LoadFromConfig
- [ ] Настроить `appsettings.json` — маршруты:
  - [ ] `/api/v1/products/**` → `catalog-api`
  - [ ] `/api/v1/categories/**` → `catalog-api`
  - [ ] `/api/v1/orders/**` → `ordering-api`
  - [ ] `/api/v1/auth/**` → `identity-api`
- [ ] Настроить Aspire service discovery в маршрутах (использовать имена сервисов, а не адреса)
- [ ] Настроить прокидывание заголовков (Authorization, Content-Type)
- [ ] Зарегистрировать в Aspire AppHost с `WithReference` на все API-сервисы
- [ ] Убедиться, что Gateway запускается и проксирует запросы

---

### 1.7 Blazor Web App (Фронтенд + BFF)

#### 1.7.1 NovaCart.Web (Server — BFF)

- [ ] Создать проект `NovaCart.Web` (Blazor Web App, .NET 10, Auto render mode, Per page/component)
- [ ] Добавить в `NovaCart.slnx` (solution folder `src/Web`)
- [ ] Добавить зависимость на `ServiceDefaults`
- [ ] Добавить NuGet: `MudBlazor`
- [ ] Настроить `Program.cs`: AddServiceDefaults, MudBlazor services
- [ ] Настроить `App.razor`: MudBlazor providers, HeadOutlet
- [ ] Настроить Layout: `MainLayout.razor` с MudBlazor (AppBar, NavMenu, Footer)
- [ ] Создать BFF-сервисы (server-side, папка `Services/`):
  - [ ] `CatalogService` — вызовы к Gateway для товаров и категорий
  - [ ] `OrderService` — вызовы к Gateway для заказов
  - [ ] `AuthService` — вызовы к Gateway для аутентификации
- [ ] Настроить HttpClient для BFF → Gateway (через Aspire service discovery)
- [ ] Создать SSR-страницы (папка `Components/Pages/`):
  - [ ] `Home.razor` — главная страница (лендинг)
  - [ ] `CatalogPage.razor` — каталог товаров с пагинацией
  - [ ] `ProductPage.razor` — страница отдельного товара
- [ ] Зарегистрировать в Aspire AppHost с `WithReference(gateway)`, `WithExternalHttpEndpoints`
- [ ] Убедиться, что Web App запускается через Aspire

#### 1.7.2 NovaCart.Web.Client (WASM — интерактивные компоненты)

- [ ] Создать проект `NovaCart.Web.Client` (Blazor WebAssembly, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить NuGet: `MudBlazor`, `Microsoft.AspNetCore.Components.WebAssembly`
- [ ] Настроить `Program.cs`: HttpClient, MudBlazor services
- [ ] Создать client-side сервисы (папка `Services/`):
  - [ ] `AuthClientService` — HTTP-вызовы для логина/регистрации
- [ ] Создать интерактивные компоненты (папка `Pages/`):
  - [ ] `Login.razor` (`@rendermode InteractiveAuto`) — форма логина
  - [ ] `Register.razor` (`@rendermode InteractiveAuto`) — форма регистрации
- [ ] Убедиться, что интерактивные компоненты работают

---

### 1.8 Aspire AppHost — финальная конфигурация Phase 1

- [ ] AppHost `Program.cs` содержит:
  - [ ] PostgreSQL с 3 базами (catalogdb, orderingdb, identitydb)
  - [ ] Catalog API с подключением к catalogdb
  - [ ] Ordering API с подключением к orderingdb
  - [ ] Identity API с подключением к identitydb
  - [ ] YARP Gateway с references на все API
  - [ ] Web App с reference на Gateway
- [ ] Все сервисы запускаются через единый `dotnet run` из AppHost
- [ ] Aspire Dashboard доступен и показывает все сервисы
- [ ] Health checks работают для всех сервисов

---

### 1.9 Тесты (Phase 1)

#### 1.9.1 Unit-тесты — Catalog

- [ ] Создать проект `NovaCart.Tests.Catalog.UnitTests` (xUnit, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `tests/UnitTests`)
- [ ] Добавить NuGet: `FluentAssertions`, `NSubstitute`
- [ ] Тесты для `Product` entity:
  - [ ] Создание продукта с валидными данными
  - [ ] Создание продукта с невалидной ценой
- [ ] Тесты для `Price` value object:
  - [ ] Equality
  - [ ] Невалидные значения
- [ ] Тесты для `CreateProductHandler`:
  - [ ] Успешное создание
  - [ ] Валидация — пустое имя
  - [ ] Валидация — отрицательная цена
- [ ] Тесты для `GetProductByIdHandler`:
  - [ ] Продукт найден
  - [ ] Продукт не найден → NotFound

#### 1.9.2 Unit-тесты — Ordering

- [ ] Создать проект `NovaCart.Tests.Ordering.UnitTests` (xUnit, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Тесты для `Order` aggregate:
  - [ ] Создание заказа
  - [ ] Добавление элементов
  - [ ] Отмена заказа
  - [ ] Нельзя отменить доставленный заказ
  - [ ] Переход статусов (state machine)
- [ ] Тесты для `CreateOrderHandler`
- [ ] Тесты для `CancelOrderHandler`

#### 1.9.3 Architecture-тесты

- [ ] Создать проект `NovaCart.Tests.ArchitectureTests` (xUnit, .NET 10)
- [ ] Добавить в `NovaCart.slnx` (solution folder `tests/ArchitectureTests`)
- [ ] Добавить NuGet: `NetArchTest.Rules`
- [ ] Тест: Domain не ссылается на Application
- [ ] Тест: Domain не ссылается на Infrastructure
- [ ] Тест: Domain не ссылается на API
- [ ] Тест: Application не ссылается на Infrastructure
- [ ] Тест: Application не ссылается на API
- [ ] Тест: Infrastructure не ссылается на API
- [ ] Все тесты проходят

---

### 1.10 Финальная валидация Phase 1

- [ ] Solution собирается без ошибок (`dotnet build`)
- [ ] Все сервисы запускаются через Aspire AppHost
- [ ] Catalog API: CRUD товаров работает
- [ ] Catalog API: пагинация работает
- [ ] Catalog API: seed data загружены
- [ ] Ordering API: создание и отмена заказов работает
- [ ] Identity API: регистрация и логин работают
- [ ] Identity API: JWT-токен генерируется корректно
- [ ] YARP Gateway: проксирует запросы ко всем сервисам
- [ ] Blazor Web App: SSR-страницы отображают данные из каталога
- [ ] Blazor Web App: интерактивный логин работает
- [ ] Все unit-тесты проходят
- [ ] Все architecture-тесты проходят
- [ ] Aspire Dashboard показывает логи и трейсы всех сервисов

---

## 🟡 Phase 2 — Microservices Communication (Коммуникация)

### 2.1 BuildingBlocks.EventBus

- [ ] Создать проект `NovaCart.BuildingBlocks.EventBus` (Class Library, .NET 10)
- [ ] Добавить в `NovaCart.slnx`
- [ ] Добавить NuGet: `MassTransit`, `MassTransit.RabbitMQ`
- [ ] Реализовать `IntegrationEvent` (базовый класс: Id, CreatedAt, CorrelationId)
- [ ] Реализовать extension method `AddEventBus` (настройка MassTransit + RabbitMQ)
- [ ] Убедиться, что проект собирается

### 2.2 Aspire — добавить RabbitMQ и Redis

- [ ] Добавить `builder.AddRabbitMQ("rabbitmq")` в AppHost
- [ ] Добавить `builder.AddRedis("redis")` в AppHost
- [ ] Подключить rabbitmq ко всем API-сервисам

### 2.3 Basket Service

#### 2.3.1 Basket.Domain

- [ ] Создать проект `NovaCart.Services.Basket.Domain`
- [ ] Реализовать `Basket` entity (BuyerId, Items, TotalPrice)
- [ ] Реализовать `BasketItem` (ProductId, ProductName, Price, Quantity)
- [ ] Бизнес-методы: AddItem, RemoveItem, UpdateQuantity, Clear

#### 2.3.2 Basket.Application

- [ ] Создать проект `NovaCart.Services.Basket.Application`
- [ ] Реализовать `GetBasketQuery`, Handler
- [ ] Реализовать `UpdateBasketCommand`, Handler
- [ ] Реализовать `DeleteBasketCommand`, Handler
- [ ] Реализовать `CheckoutBasketCommand`, Handler → публикует `BasketCheckoutIntegrationEvent`

#### 2.3.3 Basket.Infrastructure

- [ ] Создать проект `NovaCart.Services.Basket.Infrastructure`
- [ ] Реализовать `BasketRepository` — Redis (JSON serialization)
- [ ] Настроить TTL для корзины

#### 2.3.4 Basket.Contracts

- [ ] Создать проект `NovaCart.Services.Basket.Contracts`
- [ ] Реализовать `BasketCheckoutIntegrationEvent`

#### 2.3.5 Basket.API

- [ ] Создать проект `NovaCart.Services.Basket.API`
- [ ] Эндпоинты: GET, PUT, DELETE `/api/v1/baskets/{buyerId}`, POST `/api/v1/baskets/checkout`
- [ ] Зарегистрировать в Aspire AppHost с Redis и RabbitMQ

### 2.4 Payment Service

#### 2.4.1 Payment.Domain

- [ ] Создать проект `NovaCart.Services.Payment.Domain`
- [ ] Реализовать `Payment` entity (OrderId, Amount, Status, ProcessedAt)
- [ ] Реализовать `PaymentStatus` (Pending, Succeeded, Failed)

#### 2.4.2 Payment.Application

- [ ] Создать проект `NovaCart.Services.Payment.Application`
- [ ] Реализовать consumer `OrderCreatedIntegrationEventHandler`
- [ ] Реализовать симуляцию оплаты (random success/failure)
- [ ] Публикация `PaymentSucceededIntegrationEvent` / `PaymentFailedIntegrationEvent`

#### 2.4.3 Payment.Infrastructure

- [ ] Создать проект `NovaCart.Services.Payment.Infrastructure`
- [ ] `PaymentDbContext` + PostgreSQL (paymentdb)
- [ ] Реализовать `PaymentRepository`

#### 2.4.4 Payment.Contracts

- [ ] Создать проект `NovaCart.Services.Payment.Contracts`
- [ ] `PaymentSucceededIntegrationEvent`
- [ ] `PaymentFailedIntegrationEvent`

#### 2.4.5 Payment.API

- [ ] Создать проект `NovaCart.Services.Payment.API`
- [ ] Зарегистрировать в Aspire AppHost

### 2.5 Integration Events — связать сервисы

- [ ] Ordering Service: consumer для `BasketCheckoutIntegrationEvent` → создание Order
- [ ] Ordering Service: consumer для `PaymentSucceededIntegrationEvent` → статус Paid
- [ ] Ordering Service: consumer для `PaymentFailedIntegrationEvent` → статус Cancelled
- [ ] Ordering Service: публикует `OrderCreatedIntegrationEvent` при создании заказа
- [ ] Ordering.Contracts: добавить `OrderCreatedIntegrationEvent`
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
