# Фаза 15. Runtime rendering и screen loop

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
T343 [x] Реализовать отрисовку `Game Over` overlay на `GameplayScreen`.
T344 [x] Проверить manual render `Game Over` overlay.
T345 [x] Подключить screen update/draw loop в `Match3Game`.
T346 [x] Проверить переходы экранов в runtime.
T347 [x] Подключить render selection highlight.
T348 [x] Проверить visual selection highlight вручную.
T349 [x] Подключить проигрывание animation queue к render loop.
T350 [ ] Проверить визуально swap/fall/spawn/destroyer animations вручную.
T350a [x] Реализовать визуальный рендер `Line` bonus как приплюснутого ромба с направлением по оси действия.
T350b [x] Реализовать визуальный рендер `Bomb` bonus как круга.
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
T364 [x] Написать test `LayoutCalculator_ProducesStableGameplayLayout_ForDifferentViewportSizes`.
T365 [x] Создать `LayoutCalculator`.
T366 [x] Реализовать адаптивный layout поля и HUD для разных desktop resolution.
T367 [x] Прогнать tests.
T368 [x] Написать test `LayoutCalculator_ProducesStableGameplayLayout_ForSupportedMobileOrientations`.
T369 [x] Реализовать адаптацию layout для mobile landscape orientations.
T370 [x] Прогнать tests.
T371 [x] Реализовать layout policy с учетом safe area и отступов экрана для menu/game over/gameplay экранов.
T372 [ ] Проверить manual smoke test UI на нескольких aspect ratio.
