# Match3

Проект в жанре match-3 на базе MonoGame с целевыми платформами DesktopGL, Android и iOS.

## Пререквизиты

### Desktop

Для локального запуска desktop-версии нужны:
- установленный `.NET 9 SDK`, доступный через `PATH`
- восстановленные NuGet-пакеты (`dotnet restore Match3/Match3.sln`)
- среда, способная запускать MonoGame DesktopGL-приложения:
  - Windows: актуальные драйверы OpenGL и системные зависимости для DesktopGL
  - Linux/macOS: OpenGL и нативные библиотеки, необходимые MonoGame DesktopGL

Команды:
- восстановить пакеты: `dotnet restore Match3/Match3.sln`
- запустить desktop-версию: `dotnet run --project Match3/Match3.DesktopGL/Match3.DesktopGL.csproj`

### Android

Для сборки и запуска Android-версии нужны:
- установленный `.NET 9 SDK`
- workload `android` для .NET
- Android SDK и Platform Tools
- настроенная среда для target framework `net9.0-android`

Команды:
- установить workload: `dotnet workload install android`
- собрать Android-проект: `dotnet build Match3/Match3.Android/Match3.Android.csproj`
- прогнать smoke-test на подключенном устройстве: `powershell -ExecutionPolicy Bypass -File .\scripts\android-smoke.ps1`

Путь к Android SDK рекомендуется задавать через `ANDROID_SDK_ROOT` или `ANDROID_HOME`; также поддерживаются `Match3/Directory.Build.local.props` и `-p:AndroidSdkDirectory=...`. Короткая памятка: `docs/AndroidSetup.md`.

### iOS

Для сборки и запуска iOS-версии нужны:
- установленный `.NET 9 SDK`
- macOS с установленным Xcode
- workload `ios` для .NET
- настроенный signing для `net9.0-ios`

Команды:
- установить workload: `dotnet workload install ios`
- собрать iOS-проект: `dotnet build Match3/Match3.iOS/Match3.iOS.csproj`

Desktop-разработка является самым быстрым способом локальной проверки; mobile targets требуют platform SDK и настройки signing.

### Запуск на Windows

Минимальная последовательность для первого запуска на Windows:
- установить `.NET 9 SDK`
- в корне репозитория выполнить `dotnet restore Match3/Match3.sln`
- убедиться, что установлены актуальные видеодрайверы с поддержкой OpenGL
- запустить приложение командой `dotnet run --project Match3/Match3.DesktopGL/Match3.DesktopGL.csproj`

Для Android-сборки на Windows дополнительно нужны `dotnet workload install android` и настроенный Android SDK.

## Типичные проблемы

- `dotnet workload install` завершается ошибкой: обновите `.NET SDK` до версии 9 и повторите установку workload
- Android-проект не собирается: проверьте, что установлен Android SDK и доступны platform tools
- iOS-проект не собирается на Windows: сборка `Match3.iOS` поддерживается только на macOS с Xcode
- DesktopGL-приложение не запускается или открывает пустое окно: проверьте драйверы видеокарты и поддержку OpenGL
- Не находятся NuGet-зависимости: выполните `dotnet restore Match3/Match3.sln` перед сборкой или запуском

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
- `docs/AndroidSetup.md`: способы локальной настройки Android SDK без hard-coded path в репозитории

## Анимационная система
- runtime-анимации живут в `Match3/Match3.Presentation/Animation/` и `Match3/Match3.Presentation/Animation/Engine/`
- gameplay-specific orchestration строится через `TurnAnimationBuilder`, `GameplayAnimationRuntime` и `GameplayVisualEffectsTimeline`
- low-level runtime управляется `AnimationPlayer`, `IAnimation`, `SequenceAnimation`, `ParallelAnimation` и `PropertyTween`
- shared animation timings и visual coefficients вынесены в `GameplayEffectTimings`, `GameplayEffectStyle`, `BoardRenderStyle`, `UiRenderStyle` и `LayoutMetrics`
