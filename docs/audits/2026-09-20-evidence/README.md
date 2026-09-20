# Доказательства аудита NovaCart

Архив состояния до исправлений. Диагностические проекты `repro-order` и `repro-basket` рассчитаны на указанный ниже HEAD и не входят в solution. После изменения контрактов проверяйте регрессии в `tests/`; этот архив сохраняет первоначальные доказательства.

Дата: 20.09.2026. Проверенный HEAD: `edf2ba89a5bd5f872d54b5289418987df9884fab`. Все команды выполнялись из `D:\Repos\NovaCart`.

| Материал | Что подтверждает |
|---|---|
| `build.log` | Первоначальную ошибку доступа в песочнице; не дефект исходников |
| `build-verified.log` | Успешную сборку Debug, 0 ошибок, 27 предупреждений |
| `unit-architecture-tests.log`, `test-results/*.trx` | 167 выбранных тестов прошли; интеграционные исключены фильтром |
| `repro-order/`, `repro-order.log` | Три воспроизведённых сценария обработчиков Ordering/Payment |
| `repro-basket/`, `repro-basket.log` | Два события от параллельного оформления одной корзины |
| `vulnerable-packages-nuget.json` | Свежий ответ NuGet.org по 43 проектам, включая транзитивные зависимости |

Команды завершились с кодом 0, кроме первоначальной сборки в песочнице. Для сборки, тестов и диагностических примеров потребовался доступ вне песочницы. Проверка Docker вне песочницы вернула код 1: pipe `dockerDesktopLinuxEngine` отсутствует. Docker не запускался.

```powershell
dotnet build NovaCart.slnx --no-restore --verbosity minimal
dotnet test NovaCart.slnx --no-build --no-restore --filter 'Category!=Integration' --logger trx --results-directory docs/audits/2026-09-20-evidence/test-results
dotnet run --project docs/audits/2026-09-20-evidence/repro-order/Repro.Order.csproj --configuration Debug -p:NuGetAudit=false
dotnet run --project docs/audits/2026-09-20-evidence/repro-basket/ReproBasket.csproj --configuration Debug -p:NuGetAudit=false
dotnet list NovaCart.slnx package --vulnerable --include-transitive --no-restore --source https://api.nuget.org/v3/index.json --format json
```

Проверка пакетов по всем источникам локального NuGet.Config была остановлена без результата. Приведённая команда с явным NuGet.org успешно завершилась. `NuGetAudit=false` применён только к воспроизводящим примерам, чтобы не смешивать их выполнение с отдельным сетевым аудитом.

В примерах код завершения **0 означает воспроизведение существующего дефекта**. Они используют реальные классы проекта, но подставные хранилища/транспорт. Они не являются доказательством реальных транзакций, доставки RabbitMQ, HTTP-ответов или списания средств. Примеры не добавлены в основной solution.

Для ссылок и приоритетов см. [основной отчёт](D:/Repos/NovaCart/docs/audits/2026-09-20-project-audit.md).
