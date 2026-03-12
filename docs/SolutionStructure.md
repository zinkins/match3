# Рекомендуемая структура solution

## 1. Состав solution

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
- анимации;
- input mapping;
- преобразование `Domain Events` в визуальные действия.

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
- `Piece`
- `PieceColor`
- `BonusKind`
- `MatchFinder`
- `BonusFactory`
- `GameEvent`

### 4.2 Что относится к Game Flow

- `GameSession`
- `GameplayStateMachine`
- `TurnProcessor`
- `CascadeProcessor`
- `SelectionController`
- `TrySwapCommand`
- `TurnResult`

---

## 5. Структура внутри Match3.Presentation

```text
Match3.Presentation/
  Screens/
  Rendering/
  Animation/
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
- `AnimationQueue`
- `BoardTransform`
- `GameplayPresenter`

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
