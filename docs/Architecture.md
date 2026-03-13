# Архитектура проекта

## 1. Цели архитектуры

Архитектура игры должна решать четыре задачи:

- изолировать игровую логику от движка и платформенных деталей;
- сделать механику детерминированной и тестируемой;
- поддерживать расширение правил игры без переписывания базового пайплайна;
- разделить доменные правила и визуальное представление, включая анимации.

Для этого проект строится как `Game Core + Game Flow + Presentation + Platform`.

Связанные диаграммы:

- [Project Class Diagram](./ProjectClassDiagram.md)
- [Simplified Project Class Diagram](./ProjectClassDiagram.Simple.md)
- [Presentation Class Diagram](./PresentationClassDiagram.md)
- [Game Flow Class Diagram](./GameFlowClassDiagram.md)
- [Game Core Class Diagram](./GameCoreClassDiagram.md)

Правило именования:

- в тексте документа используются `Game Core` и `Game Flow`;
- в коде, namespace и путях используются `GameCore` и `GameFlow`.

---

## 2. Слои

### 2.1 Game Core

Слой чистой игровой симуляции. Не зависит от `MonoGame`, `SpriteBatch`, `GameTime`, текстур и платформенных API.

Основные обязанности:

- хранение состояния поля;
- валидация хода;
- поиск матчей;
- создание и активация бонусов;
- каскадное разрешение хода;
- расчёт очков;
- завершение игры.

Типы:

- `BoardState`
- `GridPosition`
- `Move`
- `Piece`
- `PieceColor`
- `BonusKind`
- `MatchGroup`
- `GameEvent`

### 2.2 Game Flow

Оркестрация игровых сценариев. Этот слой управляет фазами хода и соединяет ввод игрока с логикой `Game Core`.

Основные обязанности:

- обработка выбора элементов;
- запуск пайплайна хода;
- управление фазами `Idle -> Swap -> Resolve -> Gravity -> Refill -> Cascade`;
- накопление и публикация событий `Game Core`;
- управление текущей игровой сессией.

Типы:

- `GameSession`
- `GameplayStateMachine`
- `TurnProcessor`
- `CascadeProcessor`
- `SelectionController`

### 2.3 Presentation

Отвечает за рендеринг, runtime-анимации, UI и преобразование доменных событий в визуальные сценарии.

Основные обязанности:

- отрисовка поля и HUD;
- визуализация выделения;
- хранение runtime-состояния визуальных узлов поля и transient-эффектов;
- проигрывание анимаций обмена, падения, взрыва и движения Разрушителей через общий animation runtime;
- преобразование координат поля в экранные координаты;
- обработка пользовательского ввода с учётом blocking animation scenarios.

Типы:

- `GameplayScreen`
- `BoardRenderer`
- `HudRenderer`
- `BoardTransform`
- `AnimationPlayer`
- `TurnAnimationBuilder`
- `BoardViewState`
- `PieceNode`
- `EffectNode`

Правило состояния:

- в `Presentation` допустим mutable runtime state для анимаций и рендера;
- такой state не считается частью доменной модели;
- mutable presentation objects не должны утекать в `Game Core` и `Game Flow`.

Это правило нужно для animation/runtime-слоя, где состояние обновляется каждый кадр и где технически удобнее хранить изменяемые transform/state objects.

#### Runtime animation architecture

Presentation animation layer разделён на три уровня:

- `Animation.Engine` - низкоуровневый runtime, который обновляет `IAnimation`, держит active animations в `AnimationPlayer` и резервирует `node + channel`, чтобы разные tween-ы не перетирали друг друга;
- `BoardViewState` - runtime-представление визуального дерева поля, где `PieceNode` хранит стабильную identity фишки при смене logical cell, а `EffectNode` описывает transient visual effects;
- `TurnAnimationBuilder` - слой orchestration, который переводит результат хода в явную фазовую последовательность `swap -> resolve -> gravity -> spawn -> settle` и сериализует каскады в один scenario.

Ключевые правила:

- `Game Core` и `Game Flow` не меняют animation state напрямую;
- `Presentation` создаёт animation scenario как `SequenceAnimation`/`ParallelAnimation` поверх `Anim` factories;
- `AnimationPlayer` является единственным местом, где принимается решение о blocking input и конфликте каналов;
- renderer читает текущее состояние только из `BoardViewState`, а не из legacy overlay-очередей.

### 2.4 Platform

Платформенные проекты `DesktopGL`, `Android`, `iOS` являются composition root:

- запускают игру;
- подключают ресурсы;
- создают реализацию сервисов;
- не содержат логики `Game Core`.

---

## 3. Структура каталогов

```text
Match3.Core/
  GameCore/
    Board/
    Pieces/
    Bonuses/
    Rules/
    Events/
  GameFlow/
    Sessions/
    StateMachine/
    Commands/
    Pipeline/
  Presentation/
    Screens/
    Rendering/
    Animation/
      Engine/
    Input/
  Localization/
  Content/
```

---

## 4. Модель Game Core

### 4.1 Value Objects

Для устранения `primitive obsession` используются value objects:

- `GridPosition` - координата клетки;
- `Move` - ход игрока;
- `BoardSize` - размер поля;
- `MatchGroup` - найденная комбинация;
- `ScoreValue` - значение очков;
- `Countdown` - оставшееся время.

Их задача:

- фиксировать смысл данных на уровне типов;
- защищать инварианты;
- делать API `Game Core` самодокументируемым.

Важно:

- требование immutable value objects относится прежде всего к `Game Core` и смысловым данным `Game Flow`;
- presentation runtime не обязан копировать этот стиль один в один;
- для animation/render state допустим mutable holder, если он изолирован от доменной логики.

Пример:

```csharp
public readonly record struct GridPosition(int X, int Y)
{
    public bool IsAdjacentTo(GridPosition other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y) == 1;
}
```

### 4.2 Сущности

- `Piece` - игровой элемент с идентификатором, цветом и типом бонуса;
- `BoardState` - состояние поля 8x8;
- `GameSessionState` - счёт, таймер, фаза, состояние выбора.

### 4.3 Правила

`Game Core` реализует правила как отдельные сервисы:

- `MoveValidator`
- `MatchFinder`
- `BonusFactory`
- `BonusActivationResolver`
- `GravityResolver`
- `RefillResolver`
- `ScoreCalculator`

Это позволяет расширять механику без роста одного монолитного класса.

---

## 5. Пайплайн хода

Один ход обрабатывается как последовательность фаз:

1. Игрок выбирает первый элемент.
2. Игрок выбирает второй элемент.
3. `SelectionController` формирует `Move`.
4. `MoveValidator` проверяет соседство.
5. Выполняется обмен.
6. `MatchFinder` ищет комбинации.
7. Если комбинаций нет, выполняется обратный обмен.
8. Если комбинации есть, создаются бонусы и уничтожаются элементы.
9. Применяются эффекты бонусов.
10. Выполняется гравитация.
11. Выполняется дозаполнение поля.
12. Повторно проверяются каскадные комбинации.
13. Обновляются очки и таймер.
14. При необходимости генерируется `GameOver`.

Такой пайплайн обеспечивает предсказуемость логики и упрощает тестирование каждого этапа.

### 5.1 Истечение таймера во время pipeline

Если таймер истекает во время выполнения хода, логика не должна прерывать текущий atomic step посередине.

Правило обработки:

- новый input немедленно блокируется;
- текущий atomic step завершается до консистентного состояния;
- после завершения текущего шага state machine переходит в `CheckingEndGame`;
- затем игра переходит в `GameOver`;
- `Presentation` сначала доигрывает уже начатые анимации текущего шага, после чего показывает `Game Over`.

Это нужно, чтобы не ломать инварианты поля, не обрывать каскад в середине resolve и не создавать визуально некорректные состояния.

---

## 6. Событийная модель

Слой `Game Core` не управляет анимацией напрямую. Вместо этого он публикует события:

- `PiecesSwapped`
- `SwapReverted`
- `MatchResolved`
- `LineBonusCreated`
- `BombBonusCreated`
- `DestroyerSpawned`
- `BombExploded`
- `PiecesFell`
- `PiecesSpawned`
- `ScoreAdded`
- `GameEnded`

`Presentation` слой преобразует эти события в phase-based animation scenario.

Обычно цепочка выглядит так:

1. `PresentationScreenHost` получает результат хода и набор cascade steps.
2. `TurnAnimationBuilder` строит `SequenceAnimation` с явными границами фаз.
3. phase callbacks обновляют `BoardViewState` и ставят в `AnimationPlayer` нужные tween-ы/effect scenarios.
4. renderer каждый кадр читает актуальные `PieceNode` и `EffectNode` из `BoardViewState`.

Преимущества:

- логика не зависит от визуального слоя;
- анимации можно менять без изменения `Game Core`;
- runtime гарантирует детерминированный порядок фаз и каскадов;
- selection, movement и transient effects могут сосуществовать за счёт channel-based tween-ов;
- удобно строить отладочный лог и unit tests.

---

## 7. Бонусы как стратегии

Поведение бонусов должно быть вынесено в отдельные стратегии:

```csharp
public interface IBonusBehavior
{
    IReadOnlyList<GameEvent> Activate(BoardState board, GridPosition position);
}
```

Реализации:

- `LineBonusBehavior`
- `BombBonusBehavior`

Преимущества:

- добавление новых бонусов не требует переписывания центрального пайплайна;
- уменьшается количество `switch` и связанных регрессий;
- правила бонусов тестируются изолированно.

---

## 8. Игровая математика

Математика используется в тех местах, где она реально повышает качество архитектуры и визуала.

### 8.1 Координатные пространства

Используются два основных пространства:

- `Grid space` - координаты клеток;
- `World/Screen space` - координаты отрисовки.

Преобразование выполняет `BoardTransform`:

```csharp
public sealed class BoardTransform
{
    public Vector2 Origin { get; init; }
    public Vector2 CellSize { get; init; }

    public Vector2 GridToWorld(GridPosition p) =>
        Origin + new Vector2(p.X * CellSize.X, p.Y * CellSize.Y);
}
```

### 8.2 Векторы

`Vector2` используется для:

- интерполяции обмена элементов;
- падения элементов;
- движения Разрушителей;
- вычисления направления и скорости.

### 8.3 Матрицы

`Matrix` используется для composable transforms:

- вращение выделенного элемента вокруг центра;
- масштабирование при выделении;
- локальные визуальные эффекты без усложнения доменной модели.

### 8.4 Интерполяции

Для анимаций используются:

- `Lerp`
- `SmoothStep`
- easing functions

Это даёт плавные обмены, падения и взрывы без смешивания анимационной логики с игровой симуляцией.

---

## 9. Детерминированность и тестирование

Источник случайности должен быть абстрагирован:

```csharp
public interface IRandomSource
{
    int Next(int minInclusive, int maxExclusive);
}
```

Это позволяет:

- воспроизводить сценарии через seed;
- стабильно тестировать генерацию и каскады;
- упрощать отладку сложных цепочек бонусов.

Минимальный набор тестов:

- валидный и невалидный обмен;
- откат обмена без матча;
- поиск матчей;
- создание `Line`;
- создание `Bomb`;
- активация бонуса бонусом;
- каскад после падения;
- завершение игры по таймеру.

---

## 10. Причины выбора архитектуры

Такой подход выбран для тестового задания уровня `Middle / Senior`, потому что он демонстрирует:

- умение строить масштабируемую архитектуру;
- отделение логики `Game Core` от визуального слоя;
- понимание событийной и фазовой модели gameplay pipeline;
- использование value objects для защиты инвариантов `Game Core`;
- осмысленное применение векторов, матриц и интерполяций;
- готовность к расширению механик без архитектурного долга.

---

## 11. Обязательные архитектурные решения

Ниже перечислены решения, которые должны быть реализованы в проекте обязательно.

### 11.1 Value Objects для координат и ходов

Координаты клеток и ход игрока не должны передаваться как разрозненные `int x, int y`.

Минимальный набор:

- `GridPosition`
- `Move`

Это нужно для:

- устранения `primitive obsession`;
- защиты от ошибок при передаче аргументов;
- повышения читаемости доменных API;
- удобного тестирования правил соседства и обмена.

### 11.2 Unit tests для слоя Game Core

В проекте должны быть хотя бы базовые unit tests, покрывающие ключевые правила симуляции.

Минимально обязательные сценарии:

- валидный обмен соседних элементов;
- откат обмена без матча;
- поиск комбинации из 3 элементов;
- создание бонуса `Line`;
- создание бонуса `Bomb`.

Тесты должны выполняться без зависимости от MonoGame и проверять только слой `Game Core`.

### 11.3 Отдельный pipeline хода

Обработка хода не должна быть размазана по `Update`, `Screen` или рендер-коду.

Ход должен быть оформлен как отдельный pipeline с явными фазами:

1. выбор элементов;
2. валидация хода;
3. обмен;
4. поиск комбинаций;
5. создание и активация бонусов;
6. удаление элементов;
7. гравитация;
8. дозаполнение;
9. каскадная повторная проверка;
10. завершение хода.

Такой pipeline нужен для:

- детерминированности;
- простого тестирования;
- управляемой сложности;
- прозрачного gameplay flow.

### 11.4 Анимации поверх Domain Events

Анимации не должны напрямую управлять логикой `Game Core` и не должны изменять состояние поля самостоятельно.

Правильный поток:

1. `Game Core/Game Flow` обрабатывает ход;
2. логика публикует `Domain Events`;
3. `Presentation` преобразует эти события в визуальные шаги и анимации.

Практически это означает:

- сценарий хода строится через `TurnAnimationBuilder`, а не через ручной вызов нескольких очередей;
- composable animations собираются из `Anim.Sequence()`, `Append(...)`, `Join(...)` и `Anim.Parallel(...)`;
- visual state живёт в `BoardViewState`, где `PieceNode` отвечает за фишки, а `EffectNode` - за временные эффекты;
- `AnimationPlayer` резервирует `AnimationChannel` на конкретном узле и предотвращает конфликтующие tween-ы.

Примеры событий:

- `PiecesSwapped`
- `SwapReverted`
- `MatchResolved`
- `LineBonusCreated`
- `BombExploded`
- `PiecesFell`
- `GameEnded`

Такой подход нужен для:

- разделения логики и визуализации;
- упрощения отладки;
- повторного использования событий в тестах, логах и replay-сценариях;
- безопасного расширения анимационной системы.
