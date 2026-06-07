param(
    [string]$OutputRoot = "artifacts\winui-reference-recordings",
    [ValidateSet("Light", "Dark", "Default")]
    [string]$Theme = "Default",
    [int]$WindowLeft = 0,
    [int]$WindowTop = 0,
    [int]$Width = 1180,
    [int]$Height = 820,
    [int]$CaptureMargin = 220,
    [int]$DurationSeconds = 14,
    [int]$FrameRate = 30,
    [ValidateSet("Auto", "libx264", "h264_nvenc", "h264_qsv", "h264_amf")]
    [string]$VideoEncoder = "Auto",
    [int]$TimeoutSeconds = 30
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

try {
    Add-Type -AssemblyName System.Windows.Forms
}
catch {
}

if (-not ("WinUIReferenceRecordingNative" -as [type])) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class WinUIReferenceRecordingNative
{
    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);
}
"@
}

function Wait-Until([string]$description, [scriptblock]$probe, [int]$timeoutSeconds = $TimeoutSeconds) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    do {
        $result = & $probe
        if ($null -ne $result -and $result -ne $false) {
            return $result
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $description."
}

function Get-FfmpegPath {
    $command = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        throw "ffmpeg was not found on PATH."
    }

    return $command.Source
}

function Get-VideoEncoderArguments([string]$encoder) {
    switch ($encoder) {
        "h264_nvenc" { return @("-c:v", "h264_nvenc", "-preset", "fast", "-cq", "23", "-b:v", "0") }
        "h264_qsv" { return @("-c:v", "h264_qsv", "-global_quality", "23") }
        "h264_amf" { return @("-c:v", "h264_amf", "-quality", "speed") }
        default { return @("-c:v", "libx264", "-preset", "veryfast", "-crf", "23") }
    }
}

function Find-WindowByTitle([string[]]$titles) {
    $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.Condition]::TrueCondition)

    foreach ($window in $windows) {
        foreach ($title in $titles) {
            if ($window.Current.Name -eq $title) {
                return $window
            }
        }
    }

    return $null
}

function Find-DescendantByAutomationId($root, [string]$automationId) {
    if ($null -eq $root -or [string]::IsNullOrWhiteSpace($automationId)) {
        return $null
    }

    return $root.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
            $automationId))
}

function Find-DescendantByName($root, [string]$name) {
    if ($null -eq $root -or [string]::IsNullOrWhiteSpace($name)) {
        return $null
    }

    return $root.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            $name))
}

function Find-ElementByNameInProcess([int]$processId, [string[]]$names) {
    $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
            $processId))

    foreach ($window in $windows) {
        foreach ($name in $names) {
            $candidate = Find-DescendantByName $window $name
            if (Test-AutomationElementUsable $candidate) {
                return $candidate
            }
        }
    }

    return $null
}

function Find-ElementByAutomationIdInProcess([int]$processId, [string]$automationId) {
    $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
            $processId))

    foreach ($window in $windows) {
        $candidate = Find-DescendantByAutomationId $window $automationId
        if (Test-AutomationElementUsable $candidate) {
            return $candidate
        }
    }

    return $null
}

function Test-AutomationElementUsable($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $rect = $element.Current.BoundingRectangle
        return !$rect.IsEmpty -and $rect.Width -gt 0 -and $rect.Height -gt 0
    }
    catch {
        return $false
    }
}

function Invoke-Element($element) {
    if (!(Test-AutomationElementUsable $element)) {
        return $false
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            return $true
        }
    }
    catch {
    }

    try {
        $rect = $element.Current.BoundingRectangle
        $x = [int][Math]::Round($rect.Left + ($rect.Width / 2.0))
        $y = [int][Math]::Round($rect.Top + ($rect.Height / 2.0))
        [void][WinUIReferenceRecordingNative]::SetCursorPos($x, $y)
        Start-Sleep -Milliseconds 40
        [WinUIReferenceRecordingNative]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 40
        [WinUIReferenceRecordingNative]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
        return $true
    }
    catch {
        return $false
    }
}

function Send-Escape {
    [WinUIReferenceRecordingNative]::keybd_event(0x1B, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 40
    [WinUIReferenceRecordingNative]::keybd_event(0x1B, 0, 0x0002, [UIntPtr]::Zero)
}

function Wait-ForElementByNameInProcess([int]$processId, [string[]]$names, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $element = Find-ElementByNameInProcess $processId $names
        if (Test-AutomationElementUsable $element) {
            return $element
        }

        Start-Sleep -Milliseconds 50
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Wait-ForElementGoneByNameInProcess([int]$processId, [string[]]$names, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        if ($null -eq (Find-ElementByNameInProcess $processId $names)) {
            return $true
        }

        Start-Sleep -Milliseconds 50
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Open-SecondaryCommands($window) {
    $processId = $window.Current.ProcessId
    $moreButton = Wait-Until "WinUI CommandBarFlyout MoreButton" {
        Find-ElementByAutomationIdInProcess $processId "MoreButton"
    } 3

    if (!(Invoke-Element $moreButton)) {
        return $false
    }

    return $null -ne (Wait-ForElementByNameInProcess $processId @("Resize", "Move") 1500)
}

function Start-ReferenceRecordingJob([int]$processId, [IntPtr]$windowHandle, [string]$outputPath) {
    $captureWidth = $Width + $CaptureMargin
    $captureHeight = $Height + $CaptureMargin
    $ffmpeg = Get-FfmpegPath
    $videoSize = "{0}x{1}" -f $captureWidth, $captureHeight

    $job = Start-Job -ScriptBlock {
        param($ffmpegPath, $left, $top, $videoSizeValue, $output, $duration, $frameRate, $encoderArgs)
        $arguments = @(
            "-hide_banner",
            "-loglevel", "error",
            "-nostdin",
            "-y",
            "-f", "gdigrab",
            "-framerate", $frameRate,
            "-offset_x", $left,
            "-offset_y", $top,
            "-video_size", $videoSizeValue,
            "-i", "desktop",
            "-t", $duration,
            "-an",
            "-pix_fmt", "yuv420p"
        ) + @($encoderArgs) + @(
            "-movflags", "+faststart",
            $output
        )
        & $ffmpegPath @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg gdigrab recording failed with exit code $LASTEXITCODE."
        }
    } -ArgumentList $ffmpeg, $WindowLeft, $WindowTop, $videoSize, $outputPath, $DurationSeconds, $FrameRate, (Get-VideoEncoderArguments $VideoEncoder)

    return [ordered]@{
        Job = $job
        RequestedDurationSeconds = $DurationSeconds
        FrameRate = $FrameRate
    }
}

function Stop-ReferenceRecordingJob($recordingJob) {
}

function Wait-ReferenceRecordingJob($recordingJob) {
    $job = $recordingJob.Job
    $completed = Wait-Job -Job $job -Timeout ([Math]::Max($DurationSeconds + 30, 60))
    if ($null -eq $completed) {
        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
        throw "WinUI reference recorder did not finish."
    }

    try {
        Receive-Job -Job $job -ErrorAction Stop | Out-Null
        return [ordered]@{
            Output = $recordingPath
            RequestedDurationSeconds = $DurationSeconds
            FrameRate = $FrameRate
            Recorder = "FfmpegGdigrab"
        }
    }
    finally {
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }
}

function Close-WinUIReferenceWindow($window) {
    if ($null -eq $window) {
        return
    }

    try {
        $pattern = $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
        $pattern.Close()
    }
    catch {
    }
}

$stamp = Get-Date -Format "yyyyMMdd-HHmmss-fff"
$runDirectory = Join-Path $RepoRoot (Join-Path $OutputRoot $stamp)
$caseDirectory = Join-Path $runDirectory "CommandBarFlyout"
New-Item -ItemType Directory -Force -Path $caseDirectory | Out-Null
$recordingPath = Join-Path $caseDirectory ("winui3-{0}-commandbarflyout.mp4" -f $Theme.ToLowerInvariant())
$reportPath = Join-Path $runDirectory "winui-commandbarflyout-reference.json"

$window = $null
$recordingJob = $null
$recordingResult = $null
$status = "Failed"
$notes = New-Object System.Collections.Generic.List[string]
$interaction = [ordered]@{}

try {
    Get-Process -Name "WinUIGallery" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Process "winui3gallery://item/CommandBarFlyout"

    $window = Wait-Until "installed WinUI Gallery window" {
        Find-WindowByTitle @("WinUI 3 Gallery", "WinUI Gallery")
    }
    $handle = [IntPtr]$window.Current.NativeWindowHandle
    [void][WinUIReferenceRecordingNative]::MoveWindow($handle, $WindowLeft, $WindowTop, $Width, $Height, $true)
    [void][WinUIReferenceRecordingNative]::SetForegroundWindow($handle)

    Wait-Until "WinUI CommandBarFlyout page" {
        Find-DescendantByName $window "CommandBarFlyout"
    } | Out-Null
    $trigger = Wait-Until "WinUI CommandBarFlyout trigger" {
        Find-DescendantByAutomationId $window "myImageButton"
    }

    $recordingJob = Start-ReferenceRecordingJob $window.Current.ProcessId $handle $recordingPath
    Start-Sleep -Milliseconds 600

    $firstOpen = Invoke-Element $trigger
    $firstPrimary = $null -ne (Wait-ForElementByNameInProcess $window.Current.ProcessId @("Share", "Save", "Delete") 1800)
    $firstSecondary = if ($firstPrimary) { Open-SecondaryCommands $window } else { $false }
    Start-Sleep -Milliseconds 700
    Send-Escape
    Start-Sleep -Milliseconds 500
    $closed = Wait-ForElementGoneByNameInProcess $window.Current.ProcessId @("Share", "Save", "Delete", "Resize", "Move") 1800

    $trigger = Find-DescendantByAutomationId $window "myImageButton"
    $secondOpen = Invoke-Element $trigger
    $secondPrimary = $null -ne (Wait-ForElementByNameInProcess $window.Current.ProcessId @("Share", "Save", "Delete") 1800)
    $secondSecondary = if ($secondPrimary) { Open-SecondaryCommands $window } else { $false }
    Start-Sleep -Milliseconds 900

    $recordingResult = Wait-ReferenceRecordingJob $recordingJob
    $recordingJob = $null

    $interaction = [ordered]@{
        FirstOpen = $firstOpen
        FirstPrimaryCommandsVisible = $firstPrimary
        FirstSecondaryCommandsExpanded = $firstSecondary
        ClosedBetweenOpens = $closed
        SecondOpen = $secondOpen
        SecondPrimaryCommandsVisible = $secondPrimary
        SecondSecondaryCommandsExpanded = $secondSecondary
    }

    $status = if ($firstOpen -and $firstPrimary -and $firstSecondary -and $closed -and $secondOpen -and $secondPrimary -and $secondSecondary -and (Test-Path $recordingPath)) { "Passed" } else { "Failed" }
}
catch {
    $notes.Add($_.Exception.Message)
    if ($null -ne $recordingJob) {
        Stop-ReferenceRecordingJob $recordingJob
        $job = $recordingJob.Job
        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }
}
finally {
    Close-WinUIReferenceWindow $window
}

$report = [ordered]@{
    App = "WinUI3Gallery"
    Control = "CommandBarFlyout"
    Theme = $Theme
    Route = "winui3gallery://item/CommandBarFlyout"
    Status = $status
    Recording = $recordingPath
    Window = "{0},{1},{2},{3}" -f $WindowLeft, $WindowTop, $Width, $Height
    Capture = "{0},{1},{2},{3}" -f $WindowLeft, $WindowTop, ($Width + $CaptureMargin), ($Height + $CaptureMargin)
    Interaction = $interaction
    RecorderResult = if ($null -ne $recordingResult) { $recordingResult } else { $null }
    Notes = $notes.ToArray()
}

$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportPath -Encoding UTF8
[pscustomobject]@{
    RunDirectory = $runDirectory
    Report = $reportPath
    Status = $status
    Recording = $recordingPath
}
