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

T101 [x] Написать test `MatchFinder_FindsHorizontalMatchOfThree`.
T102 [x] Создать `MatchFinder`.
T103 [x] Реализовать поиск horizontal match длины 3.
T104 [x] Прогнать tests.
T105 [x] Написать test `MatchFinder_FindsVerticalMatchOfThree`.
T106 [x] Реализовать поиск vertical match длины 3.
T107 [x] Прогнать tests.
T108 [x] Написать test `MatchFinder_DoesNotReturnMatch_WhenNoSequenceExists`.
T109 [x] Реализовать сценарий без match.
T110 [x] Прогнать tests.
T111 [x] Написать test на возврат нескольких match groups.
T112 [x] Реализовать сбор нескольких match groups.
T113 [x] Прогнать tests.

---

## Фаза 6. Swap и rollback

T114 [x] Написать test `TurnProcessor_PerformsSwap_WhenMatchExists`.
T115 [x] Создать `TurnProcessor`.
T116 [x] Реализовать базовый swap.
T117 [x] Прогнать tests.
T118 [x] Написать test `TurnProcessor_RevertsSwap_WhenNoMatchExists`.
T119 [x] Реализовать rollback swap.
T120 [x] Прогнать tests.
T121 [x] Написать test на сохранение корректного состояния поля после rollback.
T122 [x] Дополнить `TurnProcessor`.
T123 [x] Прогнать tests.

---

## Фаза 7. Gravity, refill и cascade

T124 [x] Написать test `GravityResolver_DropsPieceIntoEmptyCell`.
T125 [x] Создать `GravityResolver`.
T126 [x] Реализовать падение одной фишки вниз.
T127 [x] Прогнать tests.
T128 [x] Написать test на падение нескольких фишек в колонке.
T129 [x] Дополнить `GravityResolver`.
T130 [x] Прогнать tests.
T131 [x] Написать test `RefillResolver_FillsTopEmptyCells`.
T132 [x] Создать `RefillResolver`.
T133 [x] Реализовать заполнение верхних пустых клеток.
T134 [x] Прогнать tests.
T135 [x] Написать test на повторную проверку board после gravity и refill.
T136 [x] Добавить базовый cascade recheck.
T137 [x] Прогнать tests.

---

## Фаза 8. Score и timer

T138 [x] Написать test `ScoreCalculator_AddsPointsPerDestroyedPiece`.
T139 [x] Создать `ScoreCalculator`.
T140 [x] Реализовать начисление очков за уничтоженные фишки.
T141 [x] Прогнать tests.
T142 [x] Написать test на инициализацию timer в `GameSession`.
T143 [x] Создать `GameSession`.
T144 [x] Реализовать стартовое значение timer = 60 секунд.
T145 [x] Прогнать tests.
T146 [x] Написать test на уменьшение timer.
T147 [x] Реализовать обновление timer.
T148 [x] Прогнать tests.
T149 [x] Написать test на условие `GameOver` по timer.
T150 [x] Реализовать проверку конца игры по timer.
T151 [x] Прогнать tests.
T152 [x] Написать test `GameSession_BlocksNewInput_AfterTimerExpires`.
T153 [x] Реализовать блокировку нового input после истечения timer.
T154 [x] Прогнать tests.

---

## Фаза 9. State machine и pipeline хода

T155 [x] Написать test `GameplayStateMachine_StartsInIdle`.
T156 [x] Создать `GameplayStateMachine`.
T157 [x] Реализовать состояние `Idle`.
T158 [x] Прогнать tests.
T159 [x] Написать test перехода `Idle -> Selecting`.
T160 [x] Реализовать переход в `Selecting`.
T161 [x] Прогнать tests.
T162 [x] Написать test перехода `Selecting -> Swapping`.
T163 [x] Реализовать переход в `Swapping`.
T164 [x] Прогнать tests.
T165 [x] Написать test перехода `Swapping -> Resolving`.
T166 [x] Реализовать переход в `Resolving`.
T167 [x] Прогнать tests.
T168 [x] Написать test перехода `Resolving -> ApplyingGravity`.
T169 [x] Реализовать переход в `ApplyingGravity`.
T170 [x] Прогнать tests.
T171 [x] Написать test перехода `ApplyingGravity -> Refilling`.
T172 [x] Реализовать переход в `Refilling`.
T173 [x] Прогнать tests.
T174 [x] Написать test перехода `Refilling -> CheckingEndGame`.
T175 [x] Реализовать переход в `CheckingEndGame`.
T176 [x] Прогнать tests.
T177 [x] Написать test перехода `CheckingEndGame -> GameOver`.
T178 [x] Реализовать переход в `GameOver`.
T179 [x] Прогнать tests.
T180 [x] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringSwapping`.
T181 [x] Реализовать проверку timer после фазы `Swapping`.
T182 [x] Прогнать tests.
T183 [x] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringResolving`.
T184 [x] Реализовать проверку timer после фазы `Resolving`.
T185 [x] Прогнать tests.
T186 [x] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringApplyingGravity`.
T187 [x] Реализовать проверку timer после фазы `ApplyingGravity`.
T188 [x] Прогнать tests.
T189 [x] Написать test `GameplayStateMachine_TransitionsToCheckingEndGame_WhenTimerExpiresDuringRefilling`.
T190 [x] Реализовать проверку timer после фазы `Refilling`.
T191 [x] Прогнать tests.
T192 [x] Написать test `TurnProcessor_FinishesCurrentAtomicResolution_BeforeGameOver`.
T193 [x] Реализовать policy завершения текущего atomic step перед `GameOver`.
T194 [x] Прогнать tests.
T195 [x] Написать test на полный pipeline одного хода.
T196 [x] Связать `TurnProcessor`, state machine, gravity и refill в единый pipeline.
T197 [x] Прогнать tests.

---

## Фаза 10. Domain Events

T198 [x] Написать test `TurnProcessor_ReturnsPiecesSwappedEvent`.
T199 [x] Создать `PiecesSwapped` event.
T200 [x] Вернуть `PiecesSwapped` из `TurnProcessor`.
T201 [x] Прогнать tests.
T202 [x] Написать test `TurnProcessor_ReturnsSwapRevertedEvent`.
T203 [x] Создать `SwapReverted` event.
T204 [x] Вернуть `SwapReverted` из `TurnProcessor`.
T205 [x] Прогнать tests.
T206 [x] Написать test `TurnProcessor_ReturnsMatchResolvedEvent`.
T207 [x] Создать `MatchResolved` event.
T208 [x] Вернуть `MatchResolved` из pipeline.
T209 [x] Прогнать tests.
T210 [x] Написать test `TurnProcessor_ReturnsPiecesFellEvent`.
T211 [x] Создать `PiecesFell` event.
T212 [x] Вернуть `PiecesFell` после gravity.
T213 [x] Прогнать tests.
T214 [x] Написать test `TurnProcessor_ReturnsPiecesSpawnedEvent`.
T215 [x] Создать `PiecesSpawned` event.
T216 [x] Вернуть `PiecesSpawned` после refill.
T217 [x] Прогнать tests.
T218 [x] Написать test `TurnProcessor_ReturnsScoreAddedEvent`.
T219 [x] Создать `ScoreAdded` event.
T220 [x] Вернуть `ScoreAdded` после начисления очков.
T221 [x] Прогнать tests.
T222 [x] Написать test `TurnProcessor_ReturnsGameEndedEvent`.
T223 [x] Создать `GameEnded` event.
T224 [x] Вернуть `GameEnded` при завершении игры.
T225 [x] Прогнать tests.
T226 [x] Написать test `TurnProcessor_ReturnsGameEndedEvent_WhenTimerExpiresMidPipeline`.
T227 [x] Дополнить pipeline публикацией `GameEnded` при истечении timer во время промежуточной фазы.
T228 [x] Прогнать tests.

---

## Фаза 11. Бонусы

T229 [x] Написать test `BonusFactory_CreatesLine_ForMatchOfFour`.
T230 [x] Создать `BonusFactory`.
T231 [x] Реализовать создание `Line` для match of four.
T232 [x] Прогнать tests.
T233 [x] Написать test `BonusFactory_CreatesLine_OnLastMovedCell`.
T234 [x] Реализовать placement rule для `Line` на last moved cell.
T235 [x] Прогнать tests.
T236 [x] Написать test `LineBonus_HasSameColorAsMatchedPieces`.
T237 [x] Реализовать наследование цвета для `Line`.
T238 [x] Прогнать tests.
T239 [x] Написать test `LineBonus_HasOrientation`.
T240 [x] Реализовать horizontal/vertical orientation для `Line`.
T241 [x] Прогнать tests.
T242 [x] Написать test `BonusFactory_CreatesBomb_ForMatchOfFive`.
T243 [x] Реализовать создание `Bomb` для match of five.
T244 [x] Прогнать tests.
T245 [x] Написать test `BonusFactory_CreatesBomb_OnLastMovedCell_ForLinearMatch`.
T246 [x] Реализовать placement rule для `Bomb` на last moved cell.
T247 [x] Прогнать tests.
T248 [x] Написать test `BonusFactory_CreatesBomb_ForCrossMatch`.
T249 [x] Реализовать создание `Bomb` для cross match в точке пересечения.
T250 [x] Прогнать tests.
T251 [x] Написать test `BombBonus_HasSameColorAsMatchedPieces`.
T252 [x] Реализовать наследование цвета для `Bomb`.
T253 [x] Прогнать tests.
T254 [x] Написать test `LineBonus_ActivatesAndProducesDestroyers`.
T255 [x] Создать `LineBonusBehavior`.
T256 [x] Реализовать активацию `Line`.
T257 [x] Прогнать tests.
T258 [x] Написать test `Destroyer_DestroysPiecesOnPath`.
T259 [x] Реализовать разрушение элементов на траектории `Destroyer`.
T260 [x] Прогнать tests.
T261 [x] Написать test `Destroyer_ActivatesOtherBonusesOnPath`.
T262 [x] Реализовать активацию бонусов на траектории `Destroyer`.
T263 [x] Прогнать tests.
T264 [x] Написать test `BombBonus_ActivatesAndExplodesArea`.
T265 [x] Создать `BombBonusBehavior`.
T266 [x] Реализовать активацию `Bomb`.
T267 [x] Прогнать tests.
T268 [x] Написать test `BombBonus_ActivatesOtherBonusesInsideExplosionArea`.
T269 [x] Реализовать активацию бонусов внутри области взрыва.
T270 [x] Прогнать tests.
T271 [x] Написать test на chain reaction бонусов.
T272 [x] Создать `BonusActivationResolver`.
T273 [x] Реализовать chain reaction.
T274 [x] Прогнать tests.

---

## Фаза 12. Shared Presentation и обязательные экраны

T275 [x] Создать структуру папок `Screens/`, `Rendering/`, `Animation/`, `Input/`, `UI/` в `Match3.Presentation`.
T276 [x] [P] Создать `MainMenuScreen`.
T277 [x] [P] Создать `GameOverScreen`.
T278 [x] [P] Создать `GameplayScreen`.
T279 [x] Создать `BoardRenderer`.
T280 [x] Создать `HudRenderer`.
T281 [x] Создать `GameplayPresenter`.
T282 [x] Подключить `GameplayPresenter` к `Game Flow`.
T283 [x] Создать `AnimationQueue`.
T284 [x] Подключить `Domain Events` к `AnimationQueue`.
T285 [x] Реализовать на `MainMenuScreen` единственную кнопку `Play`.
T286 [x] Подключить кнопку `Play` к запуску игровой сессии.
T287 [x] Реализовать на `GameOverScreen` сообщение `Game Over`.
T288 [x] Реализовать на `GameOverScreen` единственную кнопку `Ok`.
T289 [x] Подключить кнопку `Ok` к возврату в `MainMenuScreen`.
T290 [x] Реализовать отображение score в `HudRenderer`.
T291 [x] Реализовать отображение remaining time в `HudRenderer`.
T292 [x] Реализовать policy: `Game Over` показывается после завершения уже начатых анимаций текущего atomic step.

---

## Фаза 13. Игровая математика, input и анимации

T293 [x] Создать `BoardTransform`.
T294 [x] Реализовать `Grid -> World` преобразование.
T295 [x] Прогнать локальную проверку отображения поля.
T296 [x] Реализовать `World -> Grid` преобразование.
T297 [x] Прогнать локальную проверку hit testing.
T298 [x] Реализовать визуальный рендер 5 типов цветных квадратов.
T299 [x] Проверить отображение всех типов элементов вручную.
T300 [x] Реализовать mouse click selection на игровом поле.
T301 [x] Проверить manual selection первого элемента.
T302 [x] Реализовать reset selection при выборе non-adjacent второго элемента.
T303 [x] Проверить manual reset selection.
T304 [x] Реализовать `Vector2`-анимацию swap.
T305 [x] Проверить swap animation вручную.
T306 [x] Реализовать `Vector2`-анимацию падения.
T307 [x] Проверить fall animation вручную.
T308 [x] Реализовать анимацию появления новых элементов сверху.
T309 [x] Проверить spawn animation вручную.
T310 [x] Реализовать interpolation/easing для movement animations.
T311 [x] Проверить плавность анимаций вручную.
T312 [x] [P] Добавить selection highlight.
T313 [x] [P] Реализовать визуальное отличие выбранного элемента.
T314 [x] Реализовать анимацию движения `Destroyer`.
T315 [x] Проверить `Destroyer` animation вручную.
T316 [x] [P] При необходимости добавить `Matrix`-transform для selection effect.

---

## Фаза 14. Platform integration

T317 [x] Подключить `Match3.Presentation` в `Match3.DesktopGL`.
T318 [x] Подключить `Match3.Presentation` в `Match3.Android`.
T319 [x] Подключить `Match3.Presentation` в `Match3.iOS`.
T320 [x] Настроить desktop composition root.
T321 [x] Настроить Android composition root.
T322 [x] Настроить iOS composition root.
T323 [x] Проверить запуск `DesktopGL`.
T324 [x] Проверить компиляцию `Android`.
T325 [x] Проверить компиляцию `iOS`.

---

## Фаза 15. Runtime rendering и screen loop

T326 [x] Написать test `BoardRenderer_ProducesRenderableBoardSnapshot`.
T327 [x] Дополнить `BoardRenderer` построением render snapshot для всего поля.
T328 [x] Прогнать tests.
T329 [x] Написать test `HudRenderer_ProducesHudSnapshot`.
T330 [x] Дополнить `HudRenderer` render snapshot для score и timer.
T331 [x] Прогнать tests.
T332 [x] Создать `SpriteBatchRenderer`.
T333 [x] Реализовать отрисовку квадрата клетки и элемента через `SpriteBatch`.
T334 [x] Выполнить manual smoke check рендера одной клетки.
T335 [x] Реализовать отрисовку полного поля 8x8 через `BoardRenderer`.
T336 [x] Выполнить manual smoke check полного поля.
T337 [x] Реализовать отрисовку HUD через `HudRenderer`.
T338 [x] Выполнить manual smoke check HUD.
T339 [x] Реализовать отрисовку `MainMenuScreen`.
T340 [x] Проверить manual render `MainMenuScreen`.
T341 [x] Реализовать отрисовку `GameplayScreen`.
T342 [x] Проверить manual render `GameplayScreen`.
T343 [x] Реализовать отрисовку `GameOverScreen`.
T344 [x] Проверить manual render `GameOverScreen`.
T345 [x] Подключить screen update/draw loop в `Match3Game`.
T346 [x] Проверить переходы экранов в runtime.
T347 [x] Подключить render selection highlight.
T348 [ ] Проверить visual selection highlight вручную.
T349 [ ] Подключить проигрывание animation queue к render loop.
T350 [ ] Проверить визуально swap/fall/spawn/destroyer animations вручную.
T350a [ ] Реализовать визуальный рендер `Line` bonus как приплюснутого ромба с направлением по оси действия.
T350b [ ] Реализовать визуальный рендер `Bomb` bonus как круга.
T350c [ ] Проверить визуально отличие бонусов от обычных элементов.
T351 [x] Написать test `MouseInputRouter_MapsLeftClickToBoardSelection`.
T352 [x] Создать `MouseInputRouter`.
T353 [x] Реализовать runtime обработку mouse input для игрового поля.
T354 [x] Прогнать tests.
T355 [x] Написать test `TouchInputRouter_MapsTapToBoardSelection`.
T356 [x] Создать `TouchInputRouter`.
T357 [x] Реализовать runtime обработку touch input для игрового поля.
T358 [x] Прогнать tests.
T359 [x] Подключить input routing к `GameplayScreen`.
T360 [ ] Проверить manual input на игровом поле для desktop и mobile.
T361 [x] Реализовать обработку input для кнопки `Play`.
T362 [x] Реализовать обработку input для кнопки `Ok`.
T363 [ ] Проверить manual input для menu/game over экранов.
T364 [ ] Написать test `LayoutCalculator_ProducesStableGameplayLayout_ForDifferentViewportSizes`.
T365 [ ] Создать `LayoutCalculator`.
T366 [ ] Реализовать адаптивный layout поля и HUD для разных desktop resolution.
T367 [ ] Прогнать tests.
T368 [ ] Написать test `LayoutCalculator_ProducesStableGameplayLayout_ForSupportedMobileOrientations`.
T369 [ ] Реализовать адаптацию layout для mobile landscape orientations.
T370 [ ] Прогнать tests.
T371 [ ] Реализовать layout policy с учетом safe area и отступов экрана для menu/game over/gameplay экранов.
T372 [ ] Проверить manual smoke test UI на нескольких aspect ratio.

---

## Фаза 16. Конфигурация и настраиваемые параметры

T373 [ ] Написать test `BoardSize_CannotBeCreated_WithNonPositiveDimensions`.
T374 [ ] Создать value object `BoardSize`.
T375 [ ] Реализовать инварианты `BoardSize`.
T376 [ ] Прогнать tests.
T377 [ ] Написать test `GameConfig_LoadsBoardSizeFromFile`.
T378 [ ] Создать `GameConfig`.
T379 [ ] Создать файл конфигурации приложения.
T380 [ ] Реализовать чтение `BoardSize` из конфигурации.
T381 [ ] Прогнать tests.
T382 [ ] Написать test `GameConfig_LoadsGameplayTimingValues`.
T383 [ ] Добавить в конфиг timer/cell size/animation duration.
T384 [ ] Реализовать чтение gameplay/render параметров из файла.
T385 [ ] Прогнать tests.
T386 [ ] Перевести `BoardState` на использование `BoardSize`.
T387 [ ] Перевести `BoardTransform` на размеры из конфига.
T388 [ ] Перевести `GameSession` на стартовый timer из конфига.
T389 [ ] Прогнать tests.
T390 [ ] Подключить загрузку конфига в platform startup.
T391 [ ] Проверить runtime override параметров через конфиг файл.

---

## Фаза 17. DI container и composition root

T392 [ ] Написать test `ServiceRegistration_CanResolveGameplayScreen`.
T393 [ ] Подключить `Microsoft.Extensions.DependencyInjection`.
T394 [ ] Создать `AddMatch3Core(...)`.
T395 [ ] Создать `AddMatch3Presentation(...)`.
T396 [ ] Прогнать tests.
T397 [ ] Зарегистрировать config как `Singleton`.
T398 [ ] Зарегистрировать stateless services как `Singleton`.
T399 [ ] Зарегистрировать session state как scoped lifetime.
T400 [ ] Прогнать tests.
T401 [ ] Перевести platform composition root на `IServiceCollection`.
T402 [ ] Создать session scope для игрового матча.
T403 [ ] Подключить пересоздание session scope при старте новой игры.
T404 [ ] Прогнать tests.
T405 [ ] Проверить, что `Game Core` не зависит от DI-контейнера.
T406 [ ] Проверить, что platform projects остаются composition root.

---

## Фаза 18. Финализация и проверка по ТЗ

T407 [ ] Прогнать полный test suite.
T408 [ ] Выполнить manual smoke test в `DesktopGL`.
T409 [ ] Отдельно проверить сценарий истечения timer во время `Swapping`.
T410 [ ] Отдельно проверить сценарий истечения timer во время `Resolving`.
T411 [ ] Отдельно проверить сценарий истечения timer во время `ApplyingGravity`.
T412 [ ] Отдельно проверить сценарий истечения timer во время `Refilling`.
T413 [ ] Отдельно проверить `Main Menu` с одной кнопкой `Play`.
T414 [ ] Отдельно проверить `Game Over` с одной кнопкой `Ok`.
T415 [ ] Отдельно проверить отображение score на игровом экране.
T416 [ ] Отдельно проверить отображение remaining time на игровом экране.
T417 [ ] Отдельно проверить reset selection для non-adjacent second click.
T418 [ ] Отдельно проверить создание `Line` на last moved cell.
T419 [ ] Отдельно проверить создание `Bomb` на last moved cell.
T420 [ ] Отдельно проверить создание `Bomb` в точке пересечения match groups.
T421 [ ] Отдельно проверить анимацию движения `Destroyer`.
T422 [ ] Отдельно проверить UI на desktop/mobile viewport с разным aspect ratio.
T423 [ ] Отдельно проверить supported mobile orientations.
T424 [ ] Отдельно проверить mouse input на desktop.
T425 [ ] Отдельно проверить touch input на mobile.
T426 [ ] Сверить реализацию с `GameDesignDocument.md`.
T427 [ ] Сверить реализацию с `Architecture.md`.
T428 [ ] Сверить реализацию с `ADR-001-architecture.md`.
T429 [ ] Сверить реализацию с `TZ.txt`.
T430 [ ] Обновить `README.md`, если фактическая структура solution изменилась.
T431 [ ] Обновить `AGENTS.md`, если workflow contributors изменился.
T432 [ ] Обновить `SolutionStructure.md`, если решение было скорректировано по ходу реализации.
T433 [ ] Подготовить итоговый список реализованных фич и покрытых tests для demo.
