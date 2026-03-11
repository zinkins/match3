# Repository Guidelines

## Project Structure & Module Organization
`Match3/` содержит solution и весь application code. `Match3/Match3.Core/` сейчас хранит shared gameplay logic, localization и MonoGame content в `Content/`. Platform entry points находятся в `Match3/Match3.DesktopGL/`, `Match3/Match3.Android/` и `Match3/Match3.iOS/`. Design и architecture references хранятся в `docs/`, включая `docs/GameDesignDocument.md`, `docs/Architecture.md` и `docs/SolutionStructure.md`.

## Build, Test, and Development Commands
- `dotnet build Match3/Match3.sln`: собирает все projects и валидирует изменения в shared code.
- `dotnet run --project Match3/Match3.DesktopGL/Match3.DesktopGL.csproj`: запускает desktop version для локальной gameplay-проверки.
- `dotnet build Match3/Match3.Android/Match3.Android.csproj`: компилирует Android target.
- `dotnet build Match3/Match3.iOS/Match3.iOS.csproj`: компилирует iOS target на машине с необходимым Apple tooling.

Запускайте команды из repository root, чтобы relative content paths резолвились корректно.

## Coding Style & Naming Conventions
Используйте 4-space indentation и стандартный C# brace style. Держите types, methods и public fields в `PascalCase`; используйте `camelCase` для locals и private fields. Придерживайтесь существующего layout namespace, например `Match3.Core.Localization`. Предпочитайте небольшие platform-neutral classes в `Match3.Core/` и держите platform-specific code в соответствующем launcher project. Добавляйте XML documentation только для public API или неочевидного поведения.
Вставляй в текст кода Windows переводы строк.

## Testing Guidelines
Сейчас test project отсутствует. Пока он не создан, считайте `dotnet build Match3/Match3.sln` плюс manual smoke test в `Match3.DesktopGL` минимальной проверкой gameplay-изменений. При добавлении tests размещайте их в sibling project вроде `Match3/Match3.Tests/`, не подтягивайте зависимости `MonoGame` в test assembly и используйте method names вроде `Swap_Rejected_When_NoMatchIsCreated`.

## Commit & Pull Request Guidelines
Текущая history использует короткие imperative subjects, например `Add Game Design Document` и `init commit`. Продолжайте использовать concise commit lines, описывающие одно изменение, например `Implement board refill animation`. Pull requests должны включать краткий summary, затронутые платформы, manual test steps и screenshots или короткие clips для видимых UI или gameplay-изменений.

## Assets & Configuration
Не переименовывайте и не перемещайте файлы в `Match3.Core/Content/` без обновления MonoGame content references. Рассматривайте localization `.resx` files как source artifacts и держите translated keys синхронизированными между cultures.
