# Match3

Проект в жанре match-3 на базе MonoGame с целевыми платформами DesktopGL, Android и iOS.

## Prerequisites
- установленный `.NET 9 SDK`, доступный через `PATH`
- desktop environment, способный запускать сборки MonoGame DesktopGL
- Android workload/tooling для сборок `net9.0-android`
- Apple build tooling на macOS для сборок `net9.0-ios`

Desktop development является самым быстрым способом локальной проверки; mobile targets требуют platform SDKs и настройки signing.

## Getting Started
- собрать весь solution: `dotnet build Match3/Match3.sln`
- запустить desktop build локально: `dotnet run --project Match3/Match3.DesktopGL/Match3.DesktopGL.csproj`

## Repository Layout
- `Match3/`: solution и platform projects
- `Match3/Match3.Core/`: shared gameplay logic, localization и assets content pipeline
- `docs/`: design references, включая `docs/GameDesignDocument.md`

## Architecture Docs
- `docs/Architecture.md`: целевой architectural approach (`Game Core + Game Flow + Presentation`)
- `docs/ADR-001-architecture.md`: architecture decision record для выбранного подхода
- `docs/SolutionStructure.md`: рекомендуемая будущая структура solution/project split
- `docs/ImplementationPlan.md`: пошаговый implementation plan
- `docs/TestStrategy.md`: test-first strategy и порядок добавления tests

## Contributing
Contributor workflow, style, testing и ожидания по PR описаны в [AGENTS.md](AGENTS.md).
