# Настройка Android SDK

Проект не хранит путь к Android SDK в git и ожидает один из стандартных способов настройки локальной среды.

## Поддерживаемые варианты

В порядке приоритета:

1. MSBuild property: `-p:AndroidSdkDirectory=C:\Android\Sdk`
2. локальный файл `Match3/Directory.Build.local.props`
3. переменная окружения `ANDROID_SDK_ROOT`
4. переменная окружения `ANDROID_HOME`

## Рекомендуемый способ

Для постоянной настройки машины лучше использовать `ANDROID_SDK_ROOT`.

Пример для PowerShell:

```powershell
$env:ANDROID_SDK_ROOT = 'D:\Distribs\AndroidSDK'
dotnet build Match3/Match3.Android/Match3.Android.csproj
```

Пример для `cmd.exe`:

```bat
set ANDROID_SDK_ROOT=D:\Distribs\AndroidSDK
dotnet build Match3\Match3.Android\Match3.Android.csproj
```

## Локальный override-файл

Если не хочется менять глобальные переменные окружения, можно создать untracked-файл:

- скопируйте `Match3/Directory.Build.local.props.example` в `Match3/Directory.Build.local.props`
- укажите локальный путь в `AndroidSdkDirectory`

Этот файл добавлен в `.gitignore` и не должен коммититься.

## Разовая сборка

Для CI, одноразовой проверки или нестандартной машины можно передать путь прямо в команду:

```bash
dotnet build Match3/Match3.Android/Match3.Android.csproj -p:AndroidSdkDirectory="D:\Distribs\AndroidSDK"
```
