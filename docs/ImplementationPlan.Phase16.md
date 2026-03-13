# Фаза 16. Рефакторинг runtime-анимаций и visual effects

T373 [x] [MVP] Написать test `AnimationPlayer_StartsWithoutActiveAnimations`.
T374 [x] [MVP] Создать папку `Animation/Engine/` в `Match3.Presentation`.
T375 [x] [MVP] Создать интерфейс `IAnimation`.
T376 [x] [MVP] Создать `AnimationHandle`.
T377 [x] [MVP] Создать каркас `AnimationPlayer`.
T378 [x] [MVP] Прогнать tests.
T379 [x] [MVP] Написать test `SequenceAnimation_RunsAppendedAnimationsInOrder`.
T380 [x] [MVP] Создать `SequenceAnimation`.
T381 [x] [MVP] Реализовать `Append(...)` для последовательного выполнения.
T382 [x] [MVP] Прогнать tests.
T383 [x] [MVP] Написать test `ParallelAnimation_CompletesAfterLongestChild`.
T384 [x] [MVP] Создать `ParallelAnimation`.
T385 [x] [MVP] Реализовать параллельное обновление дочерних анимаций.
T386 [x] [MVP] Прогнать tests.
T387 [x] [MVP] Написать test `DelayAnimation_CompletesOnlyAfterConfiguredDuration`.
T388 [x] [MVP] Создать `DelayAnimation`.
T389 [x] [MVP] Реализовать ожидание по времени.
T390 [x] [MVP] Прогнать tests.
T391 [x] [MVP] Написать test `CallbackAnimation_InvokesActionOnlyOnce`.
T392 [x] [MVP] Создать `CallbackAnimation`.
T393 [x] [MVP] Реализовать single-shot callback при проигрывании.
T394 [x] [MVP] Прогнать tests.
T395 [x] [MVP] Написать test `PropertyTween_InterpolatesFloatValue`.
T396 [x] [MVP] Создать enum `AnimationChannel`.
T397 [x] [MVP] Создать базовый `PropertyTween<T>`.
T398 [x] [MVP] Реализовать interpolation для float-based tween.
T399 [x] [MVP] Прогнать tests.
T400 [x] [MVP] Написать test `AnimationPlayer_RejectsConflictingTweens_OnSameNodeAndChannel`.
T401 [x] [MVP] Создать enum `ChannelConflictPolicy`.
T402 [x] [MVP] Реализовать в `AnimationPlayer` резервирование канала по `node + channel`.
T403 [x] [MVP] Прогнать tests.
T404 [x] [MVP] Написать test `SequenceAnimation_JoinRunsAnimationsInParallelWithinSingleStep`.
T405 [x] [MVP] Добавить поддержку `Join(...)` в `SequenceAnimation`.
T406 [x] [MVP] Реализовать группировку параллельных шагов внутри sequence.
T407 [x] [MVP] Прогнать tests.
T408 [x] [MVP] Подключить `AnimationPlayer` к render loop без удаления старой системы.
T409 [x] [MVP] Написать test `PieceNode_KeepsStableId_WhenLogicalCellChanges`.
T410 [x] [MVP] Создать интерфейс `IAnimatableNode`.
T411 [x] [MVP] Создать value object `NodeId`.
T412 [x] [MVP] Создать `PieceNode`.
T413 [x] [MVP] Прогнать tests.
T414 [x] [MVP] Написать test `BoardViewState_CanResolvePieceNodeByGridPosition`.
T415 [x] [MVP] Создать `BoardViewState`.
T416 [x] [MVP] Реализовать хранение активных `PieceNode`.
T417 [x] [MVP] Реализовать поиск узла по логической клетке.
T418 [x] [MVP] Прогнать tests.
T419 [x] [MVP] Написать test `Anim_MoveTo_ProducesPositionTween`.
T420 [x] [MVP] Создать fluent API `Anim`.
T421 [x] [MVP] [P] Добавить фабрику `MoveTo(...)`.
T422 [x] [MVP] [P] Добавить фабрику `ScaleTo(...)`.
T423 [x] [MVP] [P] Добавить фабрику `FadeTo(...)`.
T424 [x] [MVP] Прогнать tests.
T425 [x] [MVP] Написать test `Anim_Sequence_ComposesAppendAndJoinCalls`.
T426 [x] [MVP] Добавить фабрику `Sequence()`.
T427 [x] [MVP] Добавить фабрику `Parallel(...)`.
T428 [x] [MVP] Реализовать fluent-композицию для `Append/Join`.
T429 [x] [MVP] Прогнать tests.
T430 [x] [MVP] Написать test `TurnAnimationBuilder_BuildsRollbackSequence_ForRejectedSwap`.
T431 [x] [MVP] Создать `TurnAnimationContext`.
T432 [x] [MVP] Создать `TurnAnimationBuilder`.
T433 [x] [MVP] Реализовать построение сценария для invalid swap.
T434 [x] [MVP] Прогнать tests.
T435 [x] [MVP] Написать test `TurnAnimationBuilder_BuildsSwapThenSettleSequence_ForAppliedSwap`.
T436 [x] [MVP] Реализовать полноценный порядок фаз `swap -> resolve -> gravity -> spawn -> settle`.
T436a [x] [MVP] Выделить отдельный phase step для `resolve` внутри `TurnAnimationBuilder`.
T436b [x] [MVP] Выделить отдельный phase step для `gravity` внутри `TurnAnimationBuilder`.
T436c [x] [MVP] Выделить отдельный phase step для `spawn` внутри `TurnAnimationBuilder`.
T436d [x] [MVP] Выделить отдельный phase step для `settle` внутри `TurnAnimationBuilder`.
T436e [x] [MVP] Зафиксировать явный порядок phase steps для applied swap scenario.
T437 [x] [MVP] Прогнать tests.
T438 [x] [MVP] Написать test `PresentationScreenHost_UsesTurnAnimationBuilder_InsteadOfManualQueueCalls`.
T439 [x] [MVP] Перевести `PresentationScreenHost` на вызов `TurnAnimationBuilder`.
T440 [x] [MVP] Удалить из `PresentationScreenHost` прямой orchestration `QueueSwap/QueueBoardSettle/QueueVisualEvents`.
T441 [x] [MVP] Прогнать tests.
T442 [x] [MVP] Написать test `AnimationPlayer_BlocksInput_WhileBlockingScenarioIsRunning`.
T443 [x] [MVP] Перевести policy блокировки input на `AnimationPlayer`.
T444 [x] [MVP] Прогнать tests.
T445 [x] [MVP] Написать test `PieceNodeRenderer_UsesAnimatedNodeState_ForDrawing`.
T446 [x] [MVP] Перевести render path на чтение состояния из `BoardViewState`.
T447 [x] [MVP] Прогнать tests.
T448 [x] [MVP] Написать test `SwapAnimationScenario_MovesBothPiecesToTargetCells`.
T449 [x] [MVP] Перевести `QueueSwap` на `Sequence/Parallel` поверх `PieceNode`.
T450 [x] [MVP] Удалить старый special-case код swap overlays.
T451 [x] [MVP] Прогнать tests.
T452 [x] [MVP] [P] Написать test `DestroyerScenario_SpawnsTransientEffectNode_AndClearsPathOverTime`.
T453 [x] [MVP] [P] Создать transient `EffectNode` для runtime visual effects.
T454 [x] [MVP] Перевести `QueueDestroyer` на сценарий поверх `EffectNode`.
T455 [x] [MVP] Прогнать tests.
T456 [x] [MVP] [P] Написать test `ExplosionScenario_HidesAffectedCells_OnlyWhileEffectIsActive`.
T457 [x] [MVP] Перевести `QueueExplosion` на сценарий поверх `EffectNode`.
T458 [x] [MVP] Удалить отдельный список `hiddenCells`.
T459 [x] [MVP] Прогнать tests.
T460 [x] [MVP] Написать test `GravityScenario_ReusesExistingPieceNodes_ForFallingPieces`.
T461 [x] [MVP] Перевести `QueueBoardSettle` для survivors на `MoveTo(...)` поверх `PieceNode`.
T462 [x] [MVP] Зафиксировать, что падение не создаёт дубликаты визуальных узлов.
T463 [x] [MVP] Прогнать tests.
T464 [x] [MVP] Написать test `SpawnScenario_CreatesNewPieceNodes_AboveBoard_AndMovesThemDown`.
T465 [x] [MVP] Реализовать сценарий spawn/refill через новые `PieceNode`.
T466 [x] [MVP] Удалить special-case логику `hideBasePieceBeforeStart`.
T467 [x] [MVP] Прогнать tests.
T468 [x] [MVP] Написать test `SelectionEffect_CanStackWithMovementWithoutOverwritingPositionChannel`.
T469 [x] [MVP] Перевести selection/highlight effect на channel-based tweens.
T470 [x] [MVP] Прогнать tests.
T471 [x] [MVP] Написать test `CreatedBonusScenario_StartsFromCreationCell_InsteadOfSpawnLane`.
T472 [x] [MVP] Перевести анимацию создания бонуса на `TurnAnimationBuilder`.
T473 [x] [MVP] Прогнать tests.
T474 [x] [MVP] Написать test `GameplayScreen_ShowsGameOverOnlyAfterBlockingScenarioCompletes`.
T475 [x] [MVP] Перевести логику `ShouldShowGameOverOverlay` на состояние `AnimationPlayer`.
T476 [x] [MVP] Прогнать tests.
T477 [x] [MVP] Удалить `AnimationQueue`.
T478 [x] [MVP] Удалить `GameplayEffectsController`.
T479 [x] [MVP] Очистить `GameplayScreen` от ссылок на старую animation system.
T480 [x] [MVP] Прогнать tests.
T481 [x] [Nice] [P] Проверить визуально `swap` после миграции на новый runtime.
T482 [x] [Nice] [P] Проверить визуально `destroyer` и `explosion` после миграции на новый runtime.
T483 [x] [Nice] [P] Проверить визуально `fall` и `spawn` после миграции на новый runtime.
T484 [x] [Nice] Проверить визуально, что последовательность фаз хода считывается однозначно.
T485 [x] [MVP] Написать test `TurnAnimationBuilder_BuildsDistinctPhaseBoundaries_ForCascadeStep`.
T486 [x] [MVP] Добавить в `TurnAnimationBuilder` явное представление phase boundaries.
T487 [x] [MVP] Прогнать tests.
T488 [x] [MVP] Написать test `CascadeScenario_WaitsForPreviousSpawnBeforeStartingNextResolve`.
T489 [x] [MVP] Реализовать последовательный запуск каскадов через общий scenario.
T490 [x] [MVP] Прогнать tests.
T491 [x] [MVP] Написать test `ChainReactionScenario_PlaysBonusEffectsInDeterministicOrder`.
T492 [x] [MVP] Зафиксировать порядок визуализации chain reaction бонусов.
T493 [x] [MVP] Прогнать tests.
T494 [ ] [Nice] [P] Написать test `AnimationPlayer_CanCancelTransientEffectHandle`.
T495 [ ] [Nice] Добавить отмену анимации через `AnimationHandle`.
T496 [ ] [Nice] Прогнать tests.
T497 [ ] [Nice] [P] Написать test `AnimationPlayer_ReleasesReservedChannel_WhenAnimationIsCancelled`.
T498 [ ] [Nice] Освобождать channel reservations при отмене и завершении анимации.
T499 [ ] [Nice] Прогнать tests.
T500 [x] [MVP] Написать test `AnimationPlayer_DoesNotLeakCompletedTransientNodes`.
T501 [x] [MVP] Добавить явный cleanup transient `EffectNode` в сценариях `destroyer` и `explosion`.
T502 [x] [MVP] Прогнать tests.
T503 [x] [MVP] Написать test `BoardViewState_RemovesConsumedPieceNodes_AfterResolvePhase`.
T504 [x] [MVP] Реализовать удаление визуальных узлов уничтоженных элементов после завершения resolve phase.
T505 [x] [MVP] Прогнать tests.
T506 [x] [MVP] Написать test `BoardViewState_CreatesNodesForPostCascadeBoardState_WithoutFullReset`.
T507 [x] [MVP] Зафиксировать инкрементальное обновление `BoardViewState` между каскадами.
T508 [x] [MVP] Прогнать tests.
T509 [ ] [Nice] [P] Написать test `AnimationPlayer_ExposesCurrentBlockingState_ForUiAndFlowControl`.
T510 [ ] [Nice] [P] Добавить read-only API состояния player для UI и screen flow.
T511 [ ] [Nice] Прогнать tests.
T512 [ ] [Nice] [P] Написать test `AnimationDebugSnapshot_ListsActiveAnimationsAndReservedChannels`.
T513 [ ] [Nice] [P] Создать debug snapshot для диагностики активных анимаций.
T514 [ ] [Nice] Прогнать tests.
T515 [ ] [Nice] [P] Написать test `AnimationDebugSnapshot_ContainsPhaseName_ForCurrentScenarioStep`.
T516 [ ] [Nice] [P] Добавить имена фаз в debug metadata сценария.
T517 [ ] [Nice] Прогнать tests.
T518 [x] [MVP] Написать test `TurnAnimationBuilder_DoesNotScheduleSpawnForCreatedBonusCell`.
T519 [x] [MVP] Зафиксировать правило приоритета created bonus над обычным spawn.
T520 [x] [MVP] Прогнать tests.
T521 [x] [MVP] Написать test `TurnAnimationBuilder_PreservesVisualContinuity_ForBonusActivatedByDestroyer`.
T522 [x] [MVP] Реализовать непрерывность визуального объекта при активации бонуса destroyer-ом.
T523 [x] [MVP] Прогнать tests.
T524 [x] [MVP] Написать test `SelectionEffect_IsSuppressed_WhenPieceNodeIsConsumedByResolvePhase`.
T525 [x] [MVP] Убрать lingering selection/highlight с уничтоженных узлов.
T526 [x] [MVP] Прогнать tests.
T527 [ ] [Nice] [P] Написать test `AnimationPlayer_SkipsZeroDurationAnimationsWithoutBreakingSequence`.
T528 [ ] [Nice] Обработать zero-duration animations и callbacks внутри sequence.
T529 [ ] [Nice] Прогнать tests.
T530 [ ] [Nice] [P] Написать test `AnimationPlayer_UsesConfiguredDurations_ForSwapFallSpawnAndBonusPhases`.
T531 [ ] [Nice] [P] Централизовать animation timings в одном runtime-конфиге presentation слоя.
T532 [ ] [Nice] Прогнать tests.
T533 [x] [MVP] Написать test `PresentationScreenHost_DoesNotAcceptBoardInput_DuringCascadeScenario`.
T534 [x] [MVP] Зафиксировать блокировку ввода на весь blocking scenario, а не на отдельные legacy overlays.
T535 [x] [MVP] Прогнать tests.
T536 [x] [MVP] Написать test `ScreenFlowController_WaitsForAnimationPlayerBeforeShowingGameOver`.
T537 [x] [MVP] Перевести `ScreenFlowController` на состояние нового animation runtime.
T538 [x] [MVP] Прогнать tests.
T539 [x] [MVP] Удалить legacy tests, проверяющие `AnimationQueue`, после замены на новый runtime.
T540 [x] [MVP] Обновить tests под `AnimationPlayer` и `TurnAnimationBuilder`.
T541 [x] [MVP] Прогнать tests.
T542 [x] [Nice] [P] Обновить `docs/Architecture.md` описанием новой animation architecture.
T543 [x] [Nice] [P] Обновить `docs/SolutionStructure.md` новыми папками и ответственностями animation layer.
T544 [x] [Nice] [P] Добавить краткую памятку по использованию `Anim.Sequence/Join` для новых эффектов.
T545 [ ] [MVP] Выполнить финальный manual smoke test полного хода: swap -> resolve -> bonus -> fall -> spawn -> cascade.
