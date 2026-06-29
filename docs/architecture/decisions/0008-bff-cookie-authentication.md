# ADR-008: Аутентификация через BFF с HttpOnly-cookie

**Статус:** Принято
**Дата:** 2026-06-28
**Связанные ADR:** [ADR-003 (Blazor Web App + Interactive Auto)](#), [ADR-006 (ASP.NET Core Identity + JWT)](#)

## Контекст

Identity-сервис выпускает JWT (access + refresh). Фронтенд — Blazor Web App в режиме
**Interactive Auto**: один и тот же компонент может рендериться на сервере (Blazor Server
circuit) или в браузере (WebAssembly). Нужно, чтобы:

1. Пользователь логинился, и его личность сохранялась между запросами.
2. Вызовы к микросервисам (Basket, Ordering) шли **от имени пользователя** — сервисы теперь
   валидируют JWT и привязывают данные к `sub` из токена.
3. Access-токен **не попадал в браузер** (защита от XSS-кражи токена).

Прямое хранение JWT в WASM (`localStorage`) противоречит ADR-«не ходить из WASM напрямую в
сервисы» и небезопасно. Нужен паттерн, работающий в обоих режимах рендеринга.

## Решение

Серверная часть Blazor Web App выступает **BFF** и хранит токен в **HttpOnly-cookie**.

- **Логин/регистрация** — SSR-страницы (`NovaCart.Web/Components/Pages/Login.razor`,
  `Register.razor`). Только серверный рендеринг имеет доступ к `HttpContext.SignInAsync`,
  поэтому форма постится на сервер, который вызывает Identity через Gateway, декодирует JWT и
  кладёт его в cookie-принципал (claims `access_token`/`refresh_token` — серверные, наружу не
  отдаются).
- **Проброс токена вниз** — токен добавляется как `Authorization: Bearer` в двух точках:
  - `BffTokenHandler` (`DelegatingHandler`) — для вызовов, выполняемых на сервере
    (SSR/Interactive Server), читает токен из `AuthenticationStateProvider`.
  - `BffProxy` (`/api/{**path}`) — для вызовов из WASM: токен берётся из cookie-принципала
    запроса. Браузер токен не видит и не передаёт.
- **Личность для WASM** — `IUserService` с двумя реализациями (паттерн location-specific
  service для Auto-режима): `ServerUserService` (из `AuthenticationStateProvider`) и
  `ClientUserService` (через `GET /bff/user`). Компоненты получают `buyerId` единообразно, не
  зная токена.
- **Сервисы** валидируют JWT общим `AddJwtAuthentication` (в `ServiceDefaults`), используя
  единые `Jwt:Secret/Issuer/Audience`. Gateway остаётся тонким pass-through (YARP пробрасывает
  `Authorization`), каждый сервис сам энфорсит авторизацию и берёт `buyerId` из токена, а не из
  тела запроса.

```
Браузер ──cookie──▶ BFF (NovaCart.Web)
                      │  access_token (HttpOnly cookie, server-only)
                      ├─ BffTokenHandler / BffProxy → Bearer
                      ▼
                   YARP Gateway ──Authorization──▶ Basket / Ordering API ──validate JWT──▶ buyerId = sub
```

## Последствия

**Плюсы**
- Access-токен никогда не покидает сервер — устойчивость к XSS.
- Корзина и заказы строго привязаны к аутентифицированному пользователю (сервис игнорирует
  `buyerId` из клиента; для Basket маршрут `{buyerId}` обязан совпасть с `sub`).
- Единая логика валидации во всех сервисах, Gateway не усложняется.

**Минусы / упрощения (demo)**
- Ротация refresh-токена в BFF не реализована: время жизни cookie выставлено в 60 минут под
  lifetime access-токена. В production нужен silent refresh.
- JWT-секрет захардкожен в `appsettings.json` каждого сервиса (помечено `// NOTE`); в
  production — vault / Aspire-параметры.
- Состояние `<AuthorizeView>` в layout берётся из SSR-принципала и обновляется при полной
  перезагрузке (после логина/логаута делается `forceLoad`).

## Проверка

`dotnet build` и `dotnet test` (unit + architecture) проходят. Сквозной сценарий
«логин → корзина → checkout → заказ → оплата → статус» требует запуска через Aspire
(PostgreSQL/RabbitMQ/Redis в Docker) и проверяется вручную в Aspire Dashboard.
