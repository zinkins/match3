# Документация

В этой папке собраны архитектурные заметки, справочные материалы по реализации и Mermaid-диаграммы проекта.

## Актуальные заметки по рантайму

- `Match3.Presentation` использует фазовый animation runtime, построенный вокруг `TurnAnimationBuilder`, `GameplayAnimationRuntime`, `GameplayVisualEffectsTimeline` и `Animation.Engine`
- общие длительности анимаций находятся в `Match3/Match3.Presentation/Animation/GameplayEffectTimings.cs`
- общие визуальные коэффициенты анимаций находятся в `Match3/Match3.Presentation/Animation/GameplayEffectStyle.cs`
- общие константы рендера поля и UI находятся в `Match3/Match3.Presentation/Rendering/BoardRenderStyle.cs`, `Match3/Match3.Presentation/Rendering/UiRenderStyle.cs` и `Match3/Match3.Presentation/UI/LayoutMetrics.cs`

## Доступные диаграммы

- [Project Class Diagram](./ProjectClassDiagram.md) - подробный сквозной runtime-view через platform, presentation, game flow и game core
- [Simplified Project Class Diagram](./ProjectClassDiagram.Simple.md) - компактный обзор основных runtime-зависимостей
- [Presentation Class Diagram](./PresentationClassDiagram.md) - screens, input, rendering, UI и animation runtime
- [Game Flow Class Diagram](./GameFlowClassDiagram.md) - selection, state machine, turn pipeline и domain events
- [Game Core Class Diagram](./GameCoreClassDiagram.md) - модель поля, value objects, pieces, random source, matching, gravity, refill и bonuses

## Связанные документы

- [Architecture](./Architecture.md)
- [Solution Structure](./SolutionStructure.md)
- [Game Design Document](./GameDesignDocument.md)
- [Implementation Plan](./ImplementationPlan.md)
- [Test Strategy](./TestStrategy.md)
- [Android Setup](./AndroidSetup.md)
