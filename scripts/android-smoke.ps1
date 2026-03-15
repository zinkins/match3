param(
    [string]$AndroidSdkDirectory = "D:\Distribs\AndroidSDK",
    [string]$Project = "Match3/Match3.Android/Match3.Android.csproj",
    [string]$Configuration = "Debug",
    [string]$DeviceId = "",
    [switch]$SkipBuild,
    [switch]$KeepInstalled
)

$ErrorActionPreference = "Stop"

$packageName = "com.companyname.Match3"
$activityName = "crc6418798866befd615c.MainActivity"
$apkPath = Join-Path $PSScriptRoot "..\Match3\Match3.Android\bin\$Configuration\net9.0-android\com.companyname.Match3-Signed.apk"

function Wait-ForDevice {
    param([int]$TimeoutSeconds = 30)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $devices = adb devices
        $connectedDevices = @($devices -split "`r?`n" | Where-Object { $_ -match "\tdevice$" })
        if ($connectedDevices.Count -gt 0) {
            if ([string]::IsNullOrWhiteSpace($script:DeviceId)) {
                $script:DeviceId = ($connectedDevices[0] -split "\s+")[0]
            }

            return
        }

        Start-Sleep -Seconds 1
    }

    throw "Android device is not available."
}

function Invoke-Adb {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    Wait-ForDevice | Out-Null

    if ([string]::IsNullOrWhiteSpace($DeviceId)) {
        & adb @Arguments
    }
    else {
        & adb -s $DeviceId @Arguments
    }
}

function Invoke-Step {
    param(
        [string]$Message,
        [scriptblock]$Action
    )

    Write-Host "> $Message"
    & $Action
}

function Get-ViewportSize {
    $output = Invoke-Adb shell wm size
    if ($output -match "Physical size:\s*(\d+)x(\d+)") {
        return [pscustomobject]@{
            Width = [int]$Matches[1]
            Height = [int]$Matches[2]
        }
    }

    throw "Unable to determine device viewport size. Output: $output"
}

function Get-SafeBounds {
    param([int]$Width, [int]$Height)

    $horizontalPadding = [Math]::Max(16.0, $Width * 0.04)
    $verticalPadding = [Math]::Max(16.0, $Height * 0.04)

    return [pscustomobject]@{
        X = $horizontalPadding
        Y = $verticalPadding
        Width = [Math]::Max(0.0, $Width - ($horizontalPadding * 2.0))
        Height = [Math]::Max(0.0, $Height - ($verticalPadding * 2.0))
    }
}

function Get-PlayButtonCenter {
    param([int]$Width, [int]$Height)

    $safe = Get-SafeBounds -Width $Width -Height $Height
    $buttonWidth = [Math]::Min(220.0, $safe.Width)
    $buttonHeight = 70.0
    $buttonX = $safe.X + (($safe.Width - $buttonWidth) / 2.0)
    $buttonY = $safe.Y + ($safe.Height * 0.28)

    return [pscustomobject]@{
        X = [int][Math]::Round($buttonX + ($buttonWidth / 2.0))
        Y = [int][Math]::Round($buttonY + ($buttonHeight / 2.0))
    }
}

function Get-BoardCellCenter {
    param(
        [int]$Width,
        [int]$Height,
        [int]$Row,
        [int]$Column
    )

    $safe = Get-SafeBounds -Width $Width -Height $Height
    $hudHeight = 48.0
    $hudSpacing = 16.0
    $availableHeight = [Math]::Max(0.0, $safe.Height - $hudHeight - $hudSpacing)
    $cellSize = [Math]::Floor([Math]::Min($safe.Width / 8.0, $availableHeight / 8.0))
    if ($cellSize -le 0.0) {
        $cellSize = 1.0
    }

    $boardWidth = $cellSize * 8.0
    $boardHeight = $cellSize * 8.0
    $originX = $safe.X + (($safe.Width - $boardWidth) / 2.0)
    $originY = $safe.Y + $hudHeight + $hudSpacing + (($availableHeight - $boardHeight) / 2.0)

    return [pscustomobject]@{
        X = [int][Math]::Round($originX + ($Column * $cellSize) + ($cellSize / 2.0))
        Y = [int][Math]::Round($originY + ($Row * $cellSize) + ($cellSize / 2.0))
    }
}

function Assert-AppRunning {
    param([string]$Stage)

    $processId = Invoke-Adb shell pidof $packageName
    if ([string]::IsNullOrWhiteSpace($processId)) {
        Write-Host "Application is not running after: $Stage"
        Write-Host "--- crash buffer ---"
        Invoke-Adb logcat -d -b crash
        Write-Host "--- filtered main log ---"
        Invoke-Adb logcat -d -b main | rg -n "com\.companyname\.Match3|AndroidRuntime|FATAL|SIG|Exception|monodroid|MonoGame|ContentLoadException"
        throw "Application crashed after: $Stage"
    }
}

Invoke-Step "Checking connected devices" {
    Wait-ForDevice
    $devices = adb devices
    Write-Host $devices
    $connectedDevices = @($devices -split "`r?`n" | Where-Object { $_ -match "\tdevice$" })
    if ($connectedDevices.Count -eq 0) {
        throw "No connected Android device found."
    }

    if ([string]::IsNullOrWhiteSpace($DeviceId)) {
        $DeviceId = ($connectedDevices[0] -split "\s+")[0]
        Write-Host "Using device: $DeviceId"
    }
}

if (-not $SkipBuild) {
    Invoke-Step "Building Android project" {
        dotnet build $Project -p:AndroidSdkDirectory="$AndroidSdkDirectory" -p:AndroidFastDeploymentType=None -p:EmbedAssembliesIntoApk=true
    }
}

if (-not (Test-Path $apkPath)) {
    throw "APK not found: $apkPath"
}

Invoke-Step "Installing APK" {
    if (-not $KeepInstalled) {
        Invoke-Adb uninstall $packageName | Out-Null
    }
    Invoke-Adb install -r $apkPath
}

Invoke-Step "Preparing logcat" {
    Invoke-Adb shell am force-stop $packageName | Out-Null
    Invoke-Adb logcat -b crash -c
    Invoke-Adb logcat -b main -c
}

Invoke-Step "Launching application" {
    Invoke-Adb shell monkey -p $packageName -c android.intent.category.LAUNCHER 1 | Out-Null
}

Start-Sleep -Seconds 3
Assert-AppRunning -Stage "launch"

$viewport = Get-ViewportSize
$play = Get-PlayButtonCenter -Width $viewport.Width -Height $viewport.Height
$firstCell = Get-BoardCellCenter -Width $viewport.Width -Height $viewport.Height -Row 0 -Column 0
$secondCell = Get-BoardCellCenter -Width $viewport.Width -Height $viewport.Height -Row 0 -Column 1

Invoke-Step "Tapping Play at ($($play.X), $($play.Y))" {
    Invoke-Adb shell input tap $play.X $play.Y
}

Start-Sleep -Seconds 2
Assert-AppRunning -Stage "play button tap"

Invoke-Step "Tapping first board cell at ($($firstCell.X), $($firstCell.Y))" {
    Invoke-Adb shell input tap $firstCell.X $firstCell.Y
}

Start-Sleep -Seconds 1
Assert-AppRunning -Stage "first board tap"

Invoke-Step "Tapping adjacent board cell at ($($secondCell.X), $($secondCell.Y))" {
    Invoke-Adb shell input tap $secondCell.X $secondCell.Y
}

Start-Sleep -Seconds 2
Assert-AppRunning -Stage "second board tap"

Write-Host "Android smoke test passed."
