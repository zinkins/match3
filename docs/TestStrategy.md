# Test Strategy

## 1. Цель

Обеспечить реализацию gameplay logic по принципу `test-first`: сначала tests, затем минимальная реализация, затем обязательный прогон tests.

---

## 2. Основной принцип

Для каждого нового инкремента используется цикл:

1. написать unit test;
2. убедиться, что test падает по ожидаемой причине;
3. реализовать минимальный код;
4. прогнать tests;
5. только после зелёного прогона переходить к следующему шагу.

---

## 3. Приоритет test coverage

Tests добавляются в следующем порядке.

### 3.1 Value Objects

Первые tests:

- `GridPosition_IsAdjacentTo_ReturnsTrue_ForHorizontalNeighbor`
- `GridPosition_IsAdjacentTo_ReturnsTrue_ForVerticalNeighbor`
- `GridPosition_IsAdjacentTo_ReturnsFalse_ForDiagonalCell`
- `Move_StoresFromAndToPositions`

### 3.2 BoardState

Следующие tests:

- `BoardState_HasExpectedSize`
- `BoardState_CanStorePieceInCell`
- `BoardState_ThrowsOrRejects_OutOfBoundsAccess`
- `PieceCatalog_ContainsExactlyFivePieceTypes`
- `PieceType_HasColor`
- `BoardGenerator_FillsEveryCell`
- `BoardGenerator_UsesRandomPieceTypes`
- `RefillResolver_UsesRandomPieceTypes`

### 3.3 Move validation

- `MoveValidator_AllowsAdjacentSwap`
- `MoveValidator_RejectsNonAdjacentSwap`
- `MoveValidator_RejectsSameCellSwap`
- `SelectionController_StoresFirstSelectedCell`
- `SelectionController_ResetsSelection_WhenSecondCellIsNotAdjacent`
- `SelectionController_CreatesMove_WhenSecondCellIsAdjacent`

### 3.4 Match detection

- `MatchFinder_FindsHorizontalMatchOfThree`
- `MatchFinder_FindsVerticalMatchOfThree`
- `MatchFinder_DoesNotReturnMatch_WhenNoSequenceExists`

### 3.5 Swap and rollback

- `TurnProcessor_PerformsSwap_WhenMatchExists`
- `TurnProcessor_RevertsSwap_WhenNoMatchExists`

### 3.6 Gravity and refill

- `GravityResolver_DropsPiecesIntoEmptyCells`
- `RefillResolver_FillsEmptyTopCells`
- `Cascade_RechecksBoard_AfterGravityAndRefill`

### 3.7 Bonuses

- `BonusFactory_CreatesLine_ForMatchOfFour`
- `BonusFactory_CreatesLine_OnLastMovedCell`
- `LineBonus_HasSameColorAsMatchedPieces`
- `LineBonus_HasOrientation`
- `BonusFactory_CreatesBomb_ForMatchOfFive`
- `BonusFactory_CreatesBomb_OnLastMovedCell_ForLinearMatch`
- `BonusFactory_CreatesBomb_ForCrossMatch`
- `BombBonus_HasSameColorAsMatchedPieces`
- `LineBonus_ActivatesAndProducesDestroyers`
- `Destroyer_DestroysPiecesOnPath`
- `Destroyer_ActivatesOtherBonusesOnPath`
- `BombBonus_ActivatesAndExplodesArea`
- `BombBonus_ActivatesOtherBonusesInsideExplosionArea`

### 3.8 Game Flow and state machine

- `GameplayStateMachine_StartsInIdle`
- `GameplayStateMachine_TransitionsToSwapping`
- `GameplayStateMachine_TransitionsToResolving`
- `GameplayStateMachine_TransitionsToGameOver_WhenTimerExpires`
- `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringSwapping`
- `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringResolving`
- `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringApplyingGravity`
- `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringRefilling`
- `GameSession_BlocksNewInput_AfterTimerExpires`
- `TurnProcessor_FinishesCurrentAtomicResolution_BeforeGameOver`

### 3.9 Domain Events

- `TurnProcessor_ReturnsPiecesSwappedEvent`
- `TurnProcessor_ReturnsSwapRevertedEvent`
- `TurnProcessor_ReturnsMatchResolvedEvent`
- `TurnProcessor_ReturnsScoreAddedEvent`
- `TurnProcessor_ReturnsGameEndedEvent`
- `TurnProcessor_ReturnsGameEndedEvent_WhenTimerExpiresMidPipeline`

---

## 4. Что должно тестироваться только unit tests

Через unit tests должны проверяться:

- инварианты `Game Core`;
- случайная генерация типов элементов;
- валидация ходов;
- логика выбора и сброса выбора;
- матчинг;
- бонусы;
- placement rules бонусов;
- поведение `Destroyer`;
- каскады;
- state machine;
- domain events.

Эти tests не должны зависеть от:

- `MonoGame`
- `SpriteBatch`
- `Texture2D`
- platform projects

---

## 5. Что можно оставить на manual verification

Manual verification допустима для:

- rendering;
- animation timing;
- selection highlight;
- screen transitions;
- platform-specific input details;
- visual polish.

---

## 6. Regression policy

После каждого завершённого шага:

- прогоняются targeted tests для текущего изменения;
- затем прогоняется весь test suite;
- если хотя бы один test падает, следующий шаг не начинается.

Отдельно обязательно прогоняются tests на истечение timer во время промежуточных фаз pipeline.

---

## 7. Policy для timer expiration

Если timer истекает во время выполнения хода:

- новый input больше не принимается;
- текущий atomic gameplay step должен завершиться;
- после завершения текущего шага state machine переходит в `CheckingEndGame`;
- затем публикуется `GameEnded`;
- после этого игра переходит в `GameOver`.

Это правило защищает инварианты поля и не допускает обрыва resolve/cascade посередине.

---

## 8. Минимальный milestone для demo

К моменту первой демонстрации должны быть зелёными tests на:

- `GridPosition`
- `Move`
- `BoardState`
- `BoardGenerator`
- `MoveValidator`
- `SelectionController`
- `MatchFinder`
- `TurnProcessor`
- `GravityResolver`
- `RefillResolver`
- `BonusFactory`
- `GameplayStateMachine`

---

## 9. Ожидаемый результат

Такой подход показывает:

- инженерную дисциплину;
- умение строить testable architecture;
- контроль над gameplay complexity;
- способность безопасно развивать механику без частых регрессий.
