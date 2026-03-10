# Implementation Plan

## Принцип работы

Для каждой задачи используется один и тот же цикл:

1. написать или обновить test;
2. убедиться, что test падает по ожидаемой причине;
3. реализовать минимальный код;
4. прогнать targeted tests;
5. прогнать весь test suite;
6. переходить к следующей задаче только после зелёного прогона.

---

## Фаза 1. Подготовка solution и test infrastructure

T001 [x] Создать проект `Match3.Presentation`.
T002 [x] Создать проект `Match3.Tests`.
T003 [x] Добавить `Match3.Presentation` в `Match3.sln`.
T004 [x] Добавить `Match3.Tests` в `Match3.sln`.
T005 [x] Настроить reference `Match3.Presentation -> Match3.Core`.
T006 [x] Настроить reference `Match3.DesktopGL -> Match3.Presentation`.
T007 [x] Настроить reference `Match3.Android -> Match3.Presentation`.
T008 [x] Настроить reference `Match3.iOS -> Match3.Presentation`.
T009 [x] Настроить reference `Match3.Tests -> Match3.Core`.
T010 [x] Подключить test framework в `Match3.Tests`.
T011 [x] Подключить test runner packages в `Match3.Tests`.
T012 [x] [P] Добавить базовый smoke test `Tests_Project_BuildsAndRuns`.
T013 [x] [P] Проверить, что `Match3.Tests` не зависит от `MonoGame`.
T014 [x] Прогнать весь test suite и убедиться, что инфраструктура работает.

---

## Фаза 2. Базовая структура Game Core и Game Flow

T015 [x] Создать папку `GameCore/` в `Match3.Core`.
T016 [x] Создать папку `GameFlow/` в `Match3.Core`.
T017 [x] [P] Создать подпапку `GameCore/ValueObjects/`.
T018 [x] [P] Создать подпапку `GameCore/Board/`.
T019 [x] [P] Создать подпапку `GameCore/Pieces/`.
T020 [x] [P] Создать подпапку `GameCore/Bonuses/`.
T021 [x] [P] Создать подпапку `GameCore/Events/`.
T022 [x] [P] Создать подпапку `GameFlow/Sessions/`.
T023 [x] [P] Создать подпапку `GameFlow/StateMachine/`.
T024 [x] [P] Создать подпапку `GameFlow/Pipeline/`.
T025 [x] Написать test на создание `GridPosition`.
T026 [x] Реализовать `GridPosition`.
T027 [x] Прогнать tests.
T028 [x] Написать test на создание `Move`.
T029 [x] Реализовать `Move`.
T030 [x] Прогнать tests.
T031 [x] Написать test на создание `Piece`.
T032 [x] Реализовать `Piece`.
T033 [x] Прогнать tests.
T034 [x] Написать test на создание `PieceColor`.
T035 [x] Реализовать `PieceColor`.
T036 [x] Прогнать tests.
T037 [x] Написать test на создание `BonusKind`.
T038 [x] Реализовать `BonusKind`.
T039 [x] Прогнать tests.

---

## Фаза 3. Pieces, BoardState и случайная генерация поля

T040 [x] Написать test `PieceCatalog_ContainsExactlyFivePieceTypes`.
T041 [x] Зафиксировать ровно 5 типов элементов.
T042 [x] Прогнать tests.
T043 [x] Написать test `PieceType_HasColor`.
T044 [x] Зафиксировать цвет у каждого типа элемента.
T045 [x] Прогнать tests.
T046 [x] Написать test `BoardState_HasWidthEight`.
T047 [x] Написать test `BoardState_HasHeightEight`.
T048 [x] Реализовать базовый `BoardState`.
T049 [x] Прогнать tests.
T050 [x] Написать test на чтение пустой клетки.
T051 [x] Реализовать чтение клетки.
T052 [x] Прогнать tests.
T053 [x] Написать test на запись фишки в клетку.
T054 [x] Реализовать запись фишки в клетку.
T055 [x] Прогнать tests.
T056 [x] Написать test на выход за границы поля.
T057 [x] Реализовать проверку границ.
T058 [x] Прогнать tests.
T059 [x] Написать test на инвариант "поле всегда 8x8".
T060 [x] Зафиксировать инвариант в `BoardState`.
T061 [x] Прогнать tests.
T062 [x] Написать test `BoardGenerator_FillsEveryCell`.
T063 [x] Создать `BoardGenerator`.
T064 [x] Реализовать заполнение всех клеток стартового поля.
T065 [x] Прогнать tests.
T066 [x] Написать test `BoardGenerator_UsesRandomPieceTypes`.
T067 [x] Реализовать случайную генерацию типов элементов.
T068 [x] Прогнать tests.
T069 [x] Написать test `RefillResolver_UsesRandomPieceTypes`.
T070 [x] Зафиксировать случайную генерацию новых элементов при refill.
T071 [x] Прогнать tests.
T071a [x] Добавить `IRandomSource` и реализацию по умолчанию.
T071b [x] Перевести `BoardGenerator` и `RefillResolver` на `IRandomSource`.
T071c [x] Добавить тест на детерминизм генерации через фиксированную последовательность.
T071d [x] Прогнать tests.

---

## Фаза 4. Валидация хода и selection logic

T072 [x] Написать test `GridPosition_IsAdjacentTo_HorizontalNeighbor`.
T073 [x] Реализовать `GridPosition.IsAdjacentTo`.
T074 [x] Прогнать tests.
T075 [x] Написать test `GridPosition_IsAdjacentTo_VerticalNeighbor`.
T076 [x] Дополнить реализацию `IsAdjacentTo`.
T077 [x] Прогнать tests.
T078 [x] Написать test `GridPosition_IsAdjacentTo_DiagonalCell_ReturnsFalse`.
T079 [x] Дополнить реализацию `IsAdjacentTo`.
T080 [x] Прогнать tests.
T081 [x] Написать test `MoveValidator_AllowsAdjacentSwap`.
T082 [x] Создать `MoveValidator`.
T083 [x] Реализовать проверку соседнего swap.
T084 [x] Прогнать tests.
T085 [x] Написать test `MoveValidator_RejectsNonAdjacentSwap`.
T086 [x] Реализовать отказ для non-adjacent swap.
T087 [x] Прогнать tests.
T088 [x] Написать test `MoveValidator_RejectsSameCellSwap`.
T089 [x] Реализовать отказ для swap в ту же клетку.
T090 [x] Прогнать tests.
T091 [x] Написать test `SelectionController_StoresFirstSelectedCell`.
T092 [x] Создать `SelectionController`.
T093 [x] Реализовать выбор первого элемента.
T094 [x] Прогнать tests.
T095 [x] Написать test `SelectionController_ResetsSelection_WhenSecondCellIsNotAdjacent`.
T096 [x] Реализовать сброс selection для non-adjacent second click.
T097 [x] Прогнать tests.
T098 [x] Написать test `SelectionController_CreatesMove_WhenSecondCellIsAdjacent`.
T099 [x] Реализовать формирование `Move` при соседнем second click.
T100 [x] Прогнать tests.

---

## Фаза 5. Поиск матчей

T101 [ ] Написать test `MatchFinder_FindsHorizontalMatchOfThree`.
T102 [ ] Создать `MatchFinder`.
T103 [ ] Реализовать поиск horizontal match длины 3.
T104 [ ] Прогнать tests.
T105 [ ] Написать test `MatchFinder_FindsVerticalMatchOfThree`.
T106 [ ] Реализовать поиск vertical match длины 3.
T107 [ ] Прогнать tests.
T108 [ ] Написать test `MatchFinder_DoesNotReturnMatch_WhenNoSequenceExists`.
T109 [ ] Реализовать сценарий без match.
T110 [ ] Прогнать tests.
T111 [ ] Написать test на возврат нескольких match groups.
T112 [ ] Реализовать сбор нескольких match groups.
T113 [ ] Прогнать tests.

---

## Фаза 6. Swap и rollback

T114 [ ] Написать test `TurnProcessor_PerformsSwap_WhenMatchExists`.
T115 [ ] Создать `TurnProcessor`.
T116 [ ] Реализовать базовый swap.
T117 [ ] Прогнать tests.
T118 [ ] Написать test `TurnProcessor_RevertsSwap_WhenNoMatchExists`.
T119 [ ] Реализовать rollback swap.
T120 [ ] Прогнать tests.
T121 [ ] Написать test на сохранение корректного состояния поля после rollback.
T122 [ ] Дополнить `TurnProcessor`.
T123 [ ] Прогнать tests.

---

## Фаза 7. Gravity, refill и cascade

T124 [ ] Написать test `GravityResolver_DropsPieceIntoEmptyCell`.
T125 [ ] Создать `GravityResolver`.
T126 [ ] Реализовать падение одной фишки вниз.
T127 [ ] Прогнать tests.
T128 [ ] Написать test на падение нескольких фишек в колонке.
T129 [ ] Дополнить `GravityResolver`.
T130 [ ] Прогнать tests.
T131 [ ] Написать test `RefillResolver_FillsTopEmptyCells`.
T132 [ ] Создать `RefillResolver`.
T133 [ ] Реализовать заполнение верхних пустых клеток.
T134 [ ] Прогнать tests.
T135 [ ] Написать test на повторную проверку board после gravity и refill.
T136 [ ] Добавить базовый cascade recheck.
T137 [ ] Прогнать tests.

---

## Фаза 8. Score и timer

T138 [ ] Написать test `ScoreCalculator_AddsPointsPerDestroyedPiece`.
T139 [ ] Создать `ScoreCalculator`.
T140 [ ] Реализовать начисление очков за уничтоженные фишки.
T141 [ ] Прогнать tests.
T142 [ ] Написать test на инициализацию timer в `GameSession`.
T143 [ ] Создать `GameSession`.
T144 [ ] Реализовать стартовое значение timer = 60 секунд.
T145 [ ] Прогнать tests.
T146 [ ] Написать test на уменьшение timer.
T147 [ ] Реализовать обновление timer.
T148 [ ] Прогнать tests.
T149 [ ] Написать test на условие `GameOver` по timer.
T150 [ ] Реализовать проверку конца игры по timer.
T151 [ ] Прогнать tests.
T152 [ ] Написать test `GameSession_BlocksNewInput_AfterTimerExpires`.
T153 [ ] Реализовать блокировку нового input после истечения timer.
T154 [ ] Прогнать tests.

---

## Фаза 9. State machine и pipeline хода

T155 [ ] Написать test `GameplayStateMachine_StartsInIdle`.
T156 [ ] Создать `GameplayStateMachine`.
T157 [ ] Реализовать состояние `Idle`.
T158 [ ] Прогнать tests.
T159 [ ] Написать test перехода `Idle -> Selecting`.
T160 [ ] Реализовать переход в `Selecting`.
T161 [ ] Прогнать tests.
T162 [ ] Написать test перехода `Selecting -> Swapping`.
T163 [ ] Реализовать переход в `Swapping`.
T164 [ ] Прогнать tests.
T165 [ ] Написать test перехода `Swapping -> Resolving`.
T166 [ ] Реализовать переход в `Resolving`.
T167 [ ] Прогнать tests.
T168 [ ] Написать test перехода `Resolving -> ApplyingGravity`.
T169 [ ] Реализовать переход в `ApplyingGravity`.
T170 [ ] Прогнать tests.
T171 [ ] Написать test перехода `ApplyingGravity -> Refilling`.
T172 [ ] Реализовать переход в `Refilling`.
T173 [ ] Прогнать tests.
T174 [ ] Написать test перехода `Refilling -> CheckingEndGame`.
T175 [ ] Реализовать переход в `CheckingEndGame`.
T176 [ ] Прогнать tests.
T177 [ ] Написать test перехода `CheckingEndGame -> GameOver`.
T178 [ ] Реализовать переход в `GameOver`.
T179 [ ] Прогнать tests.
T180 [ ] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringSwapping`.
T181 [ ] Реализовать проверку timer после фазы `Swapping`.
T182 [ ] Прогнать tests.
T183 [ ] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringResolving`.
T184 [ ] Реализовать проверку timer после фазы `Resolving`.
T185 [ ] Прогнать tests.
T186 [ ] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringApplyingGravity`.
T187 [ ] Реализовать проверку timer после фазы `ApplyingGravity`.
T188 [ ] Прогнать tests.
T189 [ ] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringRefilling`.
T190 [ ] Реализовать проверку timer после фазы `Refilling`.
T191 [ ] Прогнать tests.
T192 [ ] Написать test `TurnProcessor_FinishesCurrentAtomicResolution_BeforeGameOver`.
T193 [ ] Реализовать policy завершения текущего atomic step перед `GameOver`.
T194 [ ] Прогнать tests.
T195 [ ] Написать test на полный pipeline одного хода.
T196 [ ] Связать `TurnProcessor`, state machine, gravity и refill в единый pipeline.
T197 [ ] Прогнать tests.

---

## Фаза 10. Domain Events

T198 [ ] Написать test `TurnProcessor_ReturnsPiecesSwappedEvent`.
T199 [ ] Создать `PiecesSwapped` event.
T200 [ ] Вернуть `PiecesSwapped` из `TurnProcessor`.
T201 [ ] Прогнать tests.
T202 [ ] Написать test `TurnProcessor_ReturnsSwapRevertedEvent`.
T203 [ ] Создать `SwapReverted` event.
T204 [ ] Вернуть `SwapReverted` из `TurnProcessor`.
T205 [ ] Прогнать tests.
T206 [ ] Написать test `TurnProcessor_ReturnsMatchResolvedEvent`.
T207 [ ] Создать `MatchResolved` event.
T208 [ ] Вернуть `MatchResolved` из pipeline.
T209 [ ] Прогнать tests.
T210 [ ] Написать test `TurnProcessor_ReturnsPiecesFellEvent`.
T211 [ ] Создать `PiecesFell` event.
T212 [ ] Вернуть `PiecesFell` после gravity.
T213 [ ] Прогнать tests.
T214 [ ] Написать test `TurnProcessor_ReturnsPiecesSpawnedEvent`.
T215 [ ] Создать `PiecesSpawned` event.
T216 [ ] Вернуть `PiecesSpawned` после refill.
T217 [ ] Прогнать tests.
T218 [ ] Написать test `TurnProcessor_ReturnsScoreAddedEvent`.
T219 [ ] Создать `ScoreAdded` event.
T220 [ ] Вернуть `ScoreAdded` после начисления очков.
T221 [ ] Прогнать tests.
T222 [ ] Написать test `TurnProcessor_ReturnsGameEndedEvent`.
T223 [ ] Создать `GameEnded` event.
T224 [ ] Вернуть `GameEnded` при завершении игры.
T225 [ ] Прогнать tests.
T226 [ ] Написать test `TurnProcessor_ReturnsGameEndedEvent_WhenTimerExpiresMidPipeline`.
T227 [ ] Дополнить pipeline публикацией `GameEnded` при истечении timer во время промежуточной фазы.
T228 [ ] Прогнать tests.

---

## Фаза 11. Бонусы

T229 [ ] Написать test `BonusFactory_CreatesLine_ForMatchOfFour`.
T230 [ ] Создать `BonusFactory`.
T231 [ ] Реализовать создание `Line` для match of four.
T232 [ ] Прогнать tests.
T233 [ ] Написать test `BonusFactory_CreatesLine_OnLastMovedCell`.
T234 [ ] Реализовать placement rule для `Line` на last moved cell.
T235 [ ] Прогнать tests.
T236 [ ] Написать test `LineBonus_HasSameColorAsMatchedPieces`.
T237 [ ] Реализовать наследование цвета для `Line`.
T238 [ ] Прогнать tests.
T239 [ ] Написать test `LineBonus_HasOrientation`.
T240 [ ] Реализовать horizontal/vertical orientation для `Line`.
T241 [ ] Прогнать tests.
T242 [ ] Написать test `BonusFactory_CreatesBomb_ForMatchOfFive`.
T243 [ ] Реализовать создание `Bomb` для match of five.
T244 [ ] Прогнать tests.
T245 [ ] Написать test `BonusFactory_CreatesBomb_OnLastMovedCell_ForLinearMatch`.
T246 [ ] Реализовать placement rule для `Bomb` на last moved cell.
T247 [ ] Прогнать tests.
T248 [ ] Написать test `BonusFactory_CreatesBomb_ForCrossMatch`.
T249 [ ] Реализовать создание `Bomb` для cross match в точке пересечения.
T250 [ ] Прогнать tests.
T251 [ ] Написать test `BombBonus_HasSameColorAsMatchedPieces`.
T252 [ ] Реализовать наследование цвета для `Bomb`.
T253 [ ] Прогнать tests.
T254 [ ] Написать test `LineBonus_ActivatesAndProducesDestroyers`.
T255 [ ] Создать `LineBonusBehavior`.
T256 [ ] Реализовать активацию `Line`.
T257 [ ] Прогнать tests.
T258 [ ] Написать test `Destroyer_DestroysPiecesOnPath`.
T259 [ ] Реализовать разрушение элементов на траектории `Destroyer`.
T260 [ ] Прогнать tests.
T261 [ ] Написать test `Destroyer_ActivatesOtherBonusesOnPath`.
T262 [ ] Реализовать активацию бонусов на траектории `Destroyer`.
T263 [ ] Прогнать tests.
T264 [ ] Написать test `BombBonus_ActivatesAndExplodesArea`.
T265 [ ] Создать `BombBonusBehavior`.
T266 [ ] Реализовать активацию `Bomb`.
T267 [ ] Прогнать tests.
T268 [ ] Написать test `BombBonus_ActivatesOtherBonusesInsideExplosionArea`.
T269 [ ] Реализовать активацию бонусов внутри области взрыва.
T270 [ ] Прогнать tests.
T271 [ ] Написать test на chain reaction бонусов.
T272 [ ] Создать `BonusActivationResolver`.
T273 [ ] Реализовать chain reaction.
T274 [ ] Прогнать tests.

---

## Фаза 12. Shared Presentation и обязательные экраны

T275 [ ] Создать структуру папок `Screens/`, `Rendering/`, `Animation/`, `Input/`, `UI/` в `Match3.Presentation`.
T276 [ ] [P] Создать `MainMenuScreen`.
T277 [ ] [P] Создать `GameOverScreen`.
T278 [ ] [P] Создать `GameplayScreen`.
T279 [ ] Создать `BoardRenderer`.
T280 [ ] Создать `HudRenderer`.
T281 [ ] Создать `GameplayPresenter`.
T282 [ ] Подключить `GameplayPresenter` к `Game Flow`.
T283 [ ] Создать `AnimationQueue`.
T284 [ ] Подключить `Domain Events` к `AnimationQueue`.
T285 [ ] Реализовать на `MainMenuScreen` единственную кнопку `Play`.
T286 [ ] Подключить кнопку `Play` к запуску игровой сессии.
T287 [ ] Реализовать на `GameOverScreen` сообщение `Game Over`.
T288 [ ] Реализовать на `GameOverScreen` единственную кнопку `Ok`.
T289 [ ] Подключить кнопку `Ok` к возврату в `MainMenuScreen`.
T290 [ ] Реализовать отображение score в `HudRenderer`.
T291 [ ] Реализовать отображение remaining time в `HudRenderer`.
T292 [ ] Реализовать policy: `Game Over` показывается после завершения уже начатых анимаций текущего atomic step.

---

## Фаза 13. Игровая математика, input и анимации

T293 [ ] Создать `BoardTransform`.
T294 [ ] Реализовать `Grid -> World` преобразование.
T295 [ ] Прогнать локальную проверку отображения поля.
T296 [ ] Реализовать `World -> Grid` преобразование.
T297 [ ] Прогнать локальную проверку hit testing.
T298 [ ] Реализовать визуальный рендер 5 типов цветных геометрических фигур.
T299 [ ] Проверить отображение всех типов элементов вручную.
T300 [ ] Реализовать mouse click selection на игровом поле.
T301 [ ] Проверить manual selection первого элемента.
T302 [ ] Реализовать reset selection при выборе non-adjacent второго элемента.
T303 [ ] Проверить manual reset selection.
T304 [ ] Реализовать `Vector2`-анимацию swap.
T305 [ ] Проверить swap animation вручную.
T306 [ ] Реализовать `Vector2`-анимацию падения.
T307 [ ] Проверить fall animation вручную.
T308 [ ] Реализовать анимацию появления новых элементов сверху.
T309 [ ] Проверить spawn animation вручную.
T310 [ ] Реализовать interpolation/easing для movement animations.
T311 [ ] Проверить плавность анимаций вручную.
T312 [ ] [P] Добавить selection highlight.
T313 [ ] [P] Реализовать визуальное отличие выбранного элемента.
T314 [ ] Реализовать анимацию движения `Destroyer`.
T315 [ ] Проверить `Destroyer` animation вручную.
T316 [ ] [P] При необходимости добавить `Matrix`-transform для selection effect.

---

## Фаза 14. Platform integration

T317 [ ] Подключить `Match3.Presentation` в `Match3.DesktopGL`.
T318 [ ] Подключить `Match3.Presentation` в `Match3.Android`.
T319 [ ] Подключить `Match3.Presentation` в `Match3.iOS`.
T320 [ ] Настроить desktop composition root.
T321 [ ] Настроить Android composition root.
T322 [ ] Настроить iOS composition root.
T323 [ ] Проверить запуск `DesktopGL`.
T324 [ ] Проверить компиляцию `Android`.
T325 [ ] Проверить компиляцию `iOS`.

---

## Фаза 15. Финализация и проверка по ТЗ

T326 [ ] Прогнать полный test suite.
T327 [ ] Выполнить manual smoke test в `DesktopGL`.
T328 [ ] Отдельно проверить сценарий истечения timer во время `Swapping`.
T329 [ ] Отдельно проверить сценарий истечения timer во время `Resolving`.
T330 [ ] Отдельно проверить сценарий истечения timer во время `ApplyingGravity`.
T331 [ ] Отдельно проверить сценарий истечения timer во время `Refilling`.
T332 [ ] Отдельно проверить `Main Menu` с одной кнопкой `Play`.
T333 [ ] Отдельно проверить `Game Over` с одной кнопкой `Ok`.
T334 [ ] Отдельно проверить отображение score на игровом экране.
T335 [ ] Отдельно проверить отображение remaining time на игровом экране.
T336 [ ] Отдельно проверить reset selection для non-adjacent second click.
T337 [ ] Отдельно проверить создание `Line` на last moved cell.
T338 [ ] Отдельно проверить создание `Bomb` на last moved cell.
T339 [ ] Отдельно проверить создание `Bomb` в точке пересечения match groups.
T340 [ ] Отдельно проверить анимацию движения `Destroyer`.
T341 [ ] Сверить реализацию с `TZ.txt`.
T342 [ ] Сверить реализацию с `GameDesignDocument.md`.
T343 [ ] Сверить реализацию с `Architecture.md`.
T344 [ ] Сверить реализацию с `ADR-001-architecture.md`.
T345 [ ] Обновить `README.md`, если фактическая структура solution изменилась.
T346 [ ] Обновить `AGENTS.md`, если workflow contributors изменился.
T347 [ ] Обновить `SolutionStructure.md`, если решение было скорректировано по ходу реализации.
T348 [ ] Подготовить итоговый список реализованных фич и покрытых tests для demo.
