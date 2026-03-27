# NovaCart

**NovaCart** — учебно-демонстрационный проект уровня production-grade, реализующий e-commerce платформу на основе микросервисной архитектуры .NET 10.

## Архитектура

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
└───────┘ └────────┘ └──────────┘

            Aspire Dashboard
         (logs, traces, metrics)
```

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
│   ├── Services/           # Микросервисы (Catalog, Ordering, Identity)
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

## Документация

- [Чек-лист реализации](docs/implementation-checklist.md)
- [Copilot Instructions](.github/copilot-instructions.md)
