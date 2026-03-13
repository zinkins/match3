# Match3

Проект в жанре match-3 на базе MonoGame с целевыми платформами DesktopGL, Android и iOS.

## Требования
- установленный `.NET 9 SDK`, доступный через `PATH`
- desktop-среда, способная запускать сборки MonoGame DesktopGL
- Android workload/tooling для сборок `net9.0-android`
- Apple build tooling на macOS для сборок `net9.0-ios`

Desktop-разработка является самым быстрым способом локальной проверки; mobile targets требуют platform SDK и настройки signing.

## Быстрый старт
- собрать весь solution: `dotnet build Match3/Match3.sln`
- запустить desktop-сборку локально: `dotnet run --project Match3/Match3.DesktopGL/Match3.DesktopGL.csproj`

## Структура репозитория
- `Match3/`: solution и platform projects
- `Match3/Match3.Core/`: shared gameplay logic, localization и assets content pipeline
- `Match3/Match3.Presentation/`: общий presentation layer, включая screens, rendering, HUD и новую runtime animation system
- `docs/`: проектная документация, включая `docs/GameDesignDocument.md`

## Архитектурные документы
- `docs/README.md`: индекс диаграмм и связанных архитектурных документов
- `docs/Architecture.md`: целевой architectural approach (`Game Core + Game Flow + Presentation`)
- `docs/ADR-001-architecture.md`: architecture decision record для выбранного подхода
- `docs/SolutionStructure.md`: рекомендуемая будущая структура solution/project split
- `docs/ImplementationPlan.md`: пошаговый implementation plan
- `docs/TestStrategy.md`: test-first strategy и порядок добавления tests

## Анимационная система
- runtime-анимации живут в `Match3/Match3.Presentation/Animation/` и `Match3/Match3.Presentation/Animation/Engine/`
- gameplay-specific orchestration строится через `TurnAnimationBuilder`, `GameplayAnimationRuntime` и `GameplayVisualEffectsTimeline`
- low-level runtime управляется `AnimationPlayer`, `IAnimation`, `SequenceAnimation`, `ParallelAnimation` и `PropertyTween`
- shared animation timings и visual coefficients вынесены в `GameplayEffectTimings`, `GameplayEffectStyle`, `BoardRenderStyle`, `UiRenderStyle` и `LayoutMetrics`

## Вклад в проект
Workflow, style, testing и ожидания по PR описаны в [AGENTS.md](AGENTS.md).
