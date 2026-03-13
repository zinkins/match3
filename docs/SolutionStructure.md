# Рекомендуемая структура solution

## 1. Состав solution

Связанные диаграммы:

- [Project Class Diagram](./ProjectClassDiagram.md)
- [Simplified Project Class Diagram](./ProjectClassDiagram.Simple.md)
- [Presentation Class Diagram](./PresentationClassDiagram.md)
- [Game Flow Class Diagram](./GameFlowClassDiagram.md)
- [Game Core Class Diagram](./GameCoreClassDiagram.md)

Рекомендуемый состав проектов:

```text
Match3.sln
  Match3.Core
  Match3.Presentation
  Match3.DesktopGL
  Match3.Android
  Match3.iOS
  Match3.Tests
```

---

## 2. Назначение проектов

### 2.1 Match3.Core

Содержит платформо-независимую игровую логику:

- `Game Core` - правила игры, состояние поля, матчинг, бонусы, очки, таймер;
- `Game Flow` - orchestration хода, state machine, pipeline, игровые сценарии.

Проект не должен зависеть от MonoGame.

### 2.2 Match3.Presentation

Содержит общий presentation layer на MonoGame:

- экраны;
- рендеринг;
- HUD;
- runtime-анимации и visual effects;
- input mapping;
- преобразование `Domain Events` в визуальные сценарии.

Этот проект нужен, чтобы не дублировать общую UI-логику в `DesktopGL`, `Android` и `iOS`.

### 2.3 Match3.DesktopGL / Match3.Android / Match3.iOS

Платформенные проекты должны оставаться тонкими launcher-проектами:

- entrypoint;
- composition root;
- platform lifecycle;
- платформенные адаптеры;
- platform-specific services.

### 2.4 Match3.Tests

Содержит unit tests для `Game Core` и `Game Flow`.

Минимальная цель:

- проверка правил обмена;
- поиск матчей;
- создание бонусов;
- каскадное разрешение хода;
- условия завершения игры.

---

## 3. Зависимости между проектами

```text
Match3.DesktopGL  -> Match3.Presentation -> Match3.Core
Match3.Android    -> Match3.Presentation -> Match3.Core
Match3.iOS        -> Match3.Presentation -> Match3.Core
Match3.Tests      -> Match3.Core
```

`Match3.Core` должен быть самым нижним уровнем и не ссылаться на `Presentation` или платформенные проекты.

---

## 4. Структура внутри Match3.Core

```text
Match3.Core/
  GameCore/
    Board/
    Pieces/
    Bonuses/
    Matching/
    Rules/
    Events/
    ValueObjects/
  GameFlow/
    Sessions/
    StateMachine/
    Commands/
    Pipeline/
    Results/
```

### 4.1 Что относится к Game Core

- `BoardState`
- `GridPosition`
- `Move`
- `CellContent`
- `PieceType`
- `PieceColor`
- `BonusKind`
- `MatchFinder`
- `BonusFactory`
- `BonusActivationResolver`

### 4.2 Что относится к Game Flow

- `GameSession`
- `GameplayStateMachine`
- `TurnProcessor`
- `TurnPipelineResult`
- `TurnPipelineCascadeStep`
- `SelectionController`

---

## 5. Структура внутри Match3.Presentation

```text
Match3.Presentation/
  Screens/
  Rendering/
  Animation/
    Engine/
  Input/
  UI/
  ViewModels/
  Composition/
```

Примеры классов:

- `GameplayScreen`
- `MainMenuScreen`
- `GameplayScreen` c `Game Over` overlay
- `BoardRenderer`
- `HudRenderer`
- `AnimationPlayer`
- `TurnAnimationBuilder`
- `GameplayAnimationRuntime`
- `GameplayVisualEffectsTimeline`
- `BoardViewState`
- `PieceNodeRenderer`
- `BoardTransform`
- `GameplayPresenter`

### 5.1 Обязанности animation layer

`Match3.Presentation/Animation/` делится на два уровня:

- корневой `Animation/` - gameplay-specific сценарии и orchestration (`TurnAnimationBuilder`, `GameplayAnimationRuntime`, `GameplayVisualEffectsTimeline`, legacy-compatible adapters при необходимости);
- `Animation/Engine/` - переиспользуемый runtime (`IAnimation`, `SequenceAnimation`, `ParallelAnimation`, `DelayAnimation`, `CallbackAnimation`, `PropertyTween`, `Anim`, `AnimationPlayer`, `BoardViewState`, `PieceNode`, `EffectNode`).

Такое разделение нужно, чтобы gameplay-сценарии не смешивались с базовой механикой проигрывания animation graph.

Сопутствующие shared constants разделены так:

- `GameplayEffectTimings` - длительности фаз и эффектов;
- `GameplayEffectStyle` - визуальные коэффициенты selection/pop/explosion/destroyer effects;
- `BoardRenderStyle` - размеры и отступы для клеток и бонусов;
- `UiRenderStyle` - размеры текста, outline и HUD overlay helpers;
- `LayoutMetrics` - safe bounds, HUD, popup и initial transform metrics.

### 5.2 Памятка по `Anim.Sequence/Join`

Используйте `Anim.Sequence()` как явную фазовую ленту, где `Append(...)` добавляет следующий шаг, а `Join(...)` запускает дополнительную анимацию внутри текущего шага параллельно с уже добавленной.

Короткое правило:

- `Append(...)` - когда следующий эффект должен начаться только после завершения предыдущего шага;
- `Join(...)` - когда несколько tween-ов должны стартовать одновременно в рамках одной фазы;
- `Anim.Parallel(...)` - когда удобнее заранее собрать независимую параллельную группу и передать её как один child animation.

Пример для нового эффекта:

```csharp
var animation = Anim.Sequence()
    .Append(Anim.MoveTo(node, targetPosition, 0.18f, blocksInput: true))
    .Join(Anim.ScaleTo(node, highlightedScale, 0.18f))
    .Append(Anim.FadeTo(node, 0f, 0.12f));
```

В примере movement и scale идут в одной фазе, а fade начинается только после завершения этой фазы.

---

## 6. Namespace

Рекомендуемая схема namespace:

```csharp
Match3.Core.GameCore.*
Match3.Core.GameFlow.*
Match3.Presentation.*
```

---

## 7. Почему это решение рекомендуется

Такое разделение даёт следующие преимущества:

- `Match3.Core` остаётся чистым и тестируемым;
- общий UI не дублируется между платформами;
- платформенные проекты остаются тонкими;
- gameplay rules, Game Flow и rendering разделены по ответственности;
- архитектура выглядит зрелой и хорошо объясняется на техническом интервью.

---

## 8. Правило именования

- в тексте документа используются `Game Core` и `Game Flow`;
- в коде, namespace и путях используются `GameCore` и `GameFlow`.
