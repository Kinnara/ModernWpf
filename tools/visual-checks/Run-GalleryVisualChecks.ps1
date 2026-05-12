param(
    [string[]]$Controls = @("TeachingTip", "Button", "ComboBox", "InfoBar", "NavigationView", "ContentDialog"),
    [ValidateSet("Light", "Dark", "Default")]
    [string]$Theme = "Light",
    [ValidateSet("None", "InstalledWinUI3Gallery")]
    [string]$Reference = "InstalledWinUI3Gallery",
    [string]$GalleryExe,
    [string]$OutputRoot = "artifacts\visual-checks",
    [int]$Width = 1180,
    [int]$Height = 820,
    [int]$TimeoutSeconds = 30,
    [int]$ModernWpfRetries = 1,
    [switch]$Build,
    [switch]$FailOnDifference
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
if ([string]::IsNullOrWhiteSpace($GalleryExe)) {
    $GalleryExe = Join-Path $RepoRoot "ModernWpf.Gallery\bin\Debug\net8.0-windows7.0\ModernWpf.Gallery.exe"
}

if ($Build) {
    & dotnet build (Join-Path $RepoRoot "ModernWpf.Gallery\ModernWpf.Gallery.csproj") -f net8.0-windows7.0 -c Debug --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "ModernWpf.Gallery build failed."
    }
}

if (!(Test-Path $GalleryExe)) {
    throw "ModernWpf Gallery executable was not found at '$GalleryExe'. Build first or pass -GalleryExe."
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
try {
    Add-Type -AssemblyName System.Drawing.Common
}
catch {
    Add-Type -AssemblyName System.Drawing
}
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class GalleryVisualNative
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int command);

    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint flags);

    public static bool Move(IntPtr hWnd, int x, int y, int width, int height)
    {
        ShowWindow(hWnd, 9);
        return MoveWindow(hWnd, x, y, width, height, true);
    }

    public static RECT GetRect(IntPtr hWnd)
    {
        RECT rect;
        if (!GetWindowRect(hWnd, out rect))
        {
            throw new InvalidOperationException("GetWindowRect failed.");
        }

        return rect;
    }
}
"@

function New-RunDirectory {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $path = Join-Path $RepoRoot (Join-Path $OutputRoot $timestamp)
    New-Item -ItemType Directory -Force -Path $path | Out-Null
    return $path
}

function ConvertTo-SafeName([string]$name) {
    return ($name -replace '[^A-Za-z0-9_.-]', '_').Trim('_')
}

function Get-RootElement {
    return [System.Windows.Automation.AutomationElement]::RootElement
}

function Find-WindowByProcessId([int]$processId) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    $fallback = $null
    foreach ($window in $windows) {
        if ($null -eq $fallback) {
            $fallback = $window
        }

        if ($window.Current.AutomationId -eq "ModernWpfGalleryMainWindow" -or
            $window.Current.Name -eq "ModernWPF Gallery") {
            return $window
        }
    }

    return $fallback
}

function Find-WindowByTitle([string[]]$titleParts) {
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($window in $windows) {
        foreach ($title in $titleParts) {
            if ($window.Current.Name -like "*$title*") {
                return $window
            }
        }
    }

    return $null
}

function Find-DescendantByAutomationId($root, [string]$automationId) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
        $automationId)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Find-DescendantByName($root, [string]$name) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $name)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Wait-Until([scriptblock]$Probe, [int]$timeoutSeconds, [string]$description) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    do {
        $value = & $Probe
        if ($null -ne $value -and $false -ne $value) {
            return $value
        }

        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $description."
}

function Wait-ModernWpfReady($window, [string]$route) {
    return Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf route '$route' to become ready" -Probe {
        $readyElement = Find-DescendantByAutomationId $window "GalleryVisualTestReadyState"
        if ($null -eq $readyElement) {
            return $null
        }

        if ($readyElement.Current.Name -eq "Ready:$route") {
            return $readyElement
        }

        return $null
    }
}

function Get-AutomationText($root, [string]$automationId) {
    $element = Find-DescendantByAutomationId $root $automationId
    if ($null -eq $element) {
        return ""
    }

    return $element.Current.Name
}

function Get-RequiredSampleAutomationId([string]$control) {
    switch ($control) {
        "TeachingTip" { return "GallerySample_TeachingTip_ShowButton" }
        "Button" { return "GallerySample_Button_PrimaryButton" }
        "ComboBox" { return "GallerySample_ComboBox_ComboBox" }
        "InfoBar" { return "GallerySample_InfoBar_ShowButton" }
        "NavigationView" { return "GallerySample_NavigationView_NavigationView" }
        "ContentDialog" { return "GallerySample_ContentDialog_ShowButton" }
        default { return "GalleryItemPageTitle" }
    }
}

function Write-UiaTree($element, [string]$path, [int]$maxDepth) {
    $lines = New-Object System.Collections.Generic.List[string]
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker

    function Format-RectCoordinate($value) {
        $number = [double]$value
        if ([double]::IsInfinity($number) -or [double]::IsNaN($number)) {
            return $number.ToString([System.Globalization.CultureInfo]::InvariantCulture)
        }

        return ([int][Math]::Round($number)).ToString([System.Globalization.CultureInfo]::InvariantCulture)
    }

    function Append-Element($node, [int]$depth) {
        if ($null -eq $node -or $depth -gt $maxDepth) {
            return
        }

        $indent = "  " * $depth
        $rect = $node.Current.BoundingRectangle
        $line = "{0}{1} name='{2}' id='{3}' rect='{4},{5},{6},{7}'" -f `
            $indent,
            $node.Current.ControlType.ProgrammaticName,
            ($node.Current.Name -replace "'", "''"),
            ($node.Current.AutomationId -replace "'", "''"),
            (Format-RectCoordinate $rect.X),
            (Format-RectCoordinate $rect.Y),
            (Format-RectCoordinate $rect.Width),
            (Format-RectCoordinate $rect.Height)
        $lines.Add($line)

        $child = $walker.GetFirstChild($node)
        while ($null -ne $child) {
            Append-Element $child ($depth + 1)
            $child = $walker.GetNextSibling($child)
        }
    }

    Append-Element $element 0
    Set-Content -Path $path -Value $lines -Encoding UTF8
}

function Test-ImageNotBlank([string]$path) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        $colors = New-Object "System.Collections.Generic.HashSet[int]"
        $stepX = [Math]::Max(1, [int]($bitmap.Width / 32))
        $stepY = [Math]::Max(1, [int]($bitmap.Height / 32))
        for ($x = 0; $x -lt $bitmap.Width; $x += $stepX) {
            for ($y = 0; $y -lt $bitmap.Height; $y += $stepY) {
                [void]$colors.Add($bitmap.GetPixel($x, $y).ToArgb())
            }
        }

        return $colors.Count -gt 4
    }
    finally {
        $bitmap.Dispose()
    }
}

function Capture-Window([IntPtr]$hwnd, [string]$path) {
    $rect = [GalleryVisualNative]::GetRect($hwnd)
    $width = [Math]::Max(1, $rect.Right - $rect.Left)
    $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $hdc = $graphics.GetHdc()
        $printed = $false
        try {
            $printed = [GalleryVisualNative]::PrintWindow($hwnd, $hdc, 2)
        }
        finally {
            $graphics.ReleaseHdc($hdc)
        }

        if (!$printed) {
            $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
        }

        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Compare-Images([string]$leftPath, [string]$rightPath) {
    $left = [System.Drawing.Bitmap]::FromFile($leftPath)
    $right = [System.Drawing.Bitmap]::FromFile($rightPath)
    try {
        if ($left.Width -ne $right.Width -or $left.Height -ne $right.Height) {
            return [ordered]@{
                Comparable = $false
                Reason = "Image dimensions differ."
                MeanDelta = $null
            }
        }

        $samples = 0
        $delta = 0.0
        $stepX = [Math]::Max(1, [int]($left.Width / 80))
        $stepY = [Math]::Max(1, [int]($left.Height / 80))
        for ($x = 0; $x -lt $left.Width; $x += $stepX) {
            for ($y = 0; $y -lt $left.Height; $y += $stepY) {
                $a = $left.GetPixel($x, $y)
                $b = $right.GetPixel($x, $y)
                $delta += ([Math]::Abs($a.R - $b.R) + [Math]::Abs($a.G - $b.G) + [Math]::Abs($a.B - $b.B)) / 3.0
                $samples++
            }
        }

        return [ordered]@{
            Comparable = $true
            Reason = ""
            MeanDelta = [Math]::Round($delta / [Math]::Max(1, $samples), 2)
        }
    }
    finally {
        $left.Dispose()
        $right.Dispose()
    }
}

function Close-AutomationWindow($window) {
    try {
        $pattern = $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Close()
        }
    }
    catch {
    }
}

function Capture-ModernWpf([string]$control, [string]$caseDir) {
    $route = "item/$control"
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null

    $args = @("--visual-test", "--route", $route, "--theme", $Theme, "--visual-artifact-dir", $artifactDir)
    $process = Start-Process -FilePath $GalleryExe -ArgumentList $args -PassThru
    try {
        $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf Gallery window" -Probe {
            $process.Refresh()
            Find-WindowByProcessId $process.Id
        }

        [void][GalleryVisualNative]::Move($window.Current.NativeWindowHandle, 60, 60, $Width, $Height)
        Wait-ModernWpfReady $window $route | Out-Null

        $lastException = Get-AutomationText $window "GalleryVisualTestLastException"
        $title = Get-AutomationText $window "GalleryItemPageTitle"
        $requiredSampleAutomationId = Get-RequiredSampleAutomationId $control
        $sample = Find-DescendantByAutomationId $window $requiredSampleAutomationId
        $screenshot = Join-Path $caseDir "modernwpf-$control.png"
        $treePath = Join-Path $caseDir "modernwpf-$control.uia.txt"

        Capture-Window $window.Current.NativeWindowHandle $screenshot
        Write-UiaTree $window $treePath 6
        $notBlank = Test-ImageNotBlank $screenshot

        return [ordered]@{
            App = "ModernWpf"
            Control = $control
            Route = $route
            Status = $(if ($lastException) { "Failed" } elseif (!$notBlank) { "Failed" } elseif ($null -eq $sample) { "Failed" } else { "Passed" })
            Title = $title
            Screenshot = $screenshot
            UiaTree = $treePath
            LastException = $lastException
            NonBlank = $notBlank
            RequiredSampleAutomationId = $requiredSampleAutomationId
            RequiredSampleElementFound = $null -ne $sample
        }
    }
    finally {
        try {
            $process.Refresh()
            if (!$process.HasExited) {
                $process.CloseMainWindow() | Out-Null
                if (!$process.WaitForExit(3000)) {
                    $process.Kill()
                }
            }
        }
        catch {
        }
    }
}

function Capture-WinUIReference([string]$control, [string]$caseDir) {
    $route = "winui3gallery://item/$control"
    Start-Process $route

    $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "installed WinUI 3 Gallery window for $control" -Probe {
        Find-WindowByTitle @("WinUI 3 Gallery", "WinUI Gallery")
    }

    [void][GalleryVisualNative]::Move($window.Current.NativeWindowHandle, 1280, 60, $Width, $Height)
    Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "WinUI 3 Gallery page title '$control'" -Probe {
        Find-DescendantByName $window $control
    } | Out-Null

    $screenshot = Join-Path $caseDir "winui3-$control.png"
    $treePath = Join-Path $caseDir "winui3-$control.uia.txt"
    Capture-Window $window.Current.NativeWindowHandle $screenshot
    Write-UiaTree $window $treePath 6
    $notBlank = Test-ImageNotBlank $screenshot

    Close-AutomationWindow $window

    return [ordered]@{
        App = "WinUI3Gallery"
        Control = $control
        Route = $route
        Status = $(if ($notBlank) { "Passed" } else { "Failed" })
        Title = $control
        Screenshot = $screenshot
        UiaTree = $treePath
        LastException = ""
        NonBlank = $notBlank
        RequiredSampleAutomationId = ""
        RequiredSampleElementFound = $true
    }
}

$runDir = New-RunDirectory
$results = New-Object System.Collections.Generic.List[object]

foreach ($control in $Controls) {
    $safeControl = ConvertTo-SafeName $control
    $caseDir = Join-Path $runDir $safeControl
    New-Item -ItemType Directory -Force -Path $caseDir | Out-Null

    $modernResult = $null
    $lastModernError = $null
    for ($attempt = 0; $attempt -le $ModernWpfRetries; $attempt++) {
        try {
            $modernResult = Capture-ModernWpf $control $caseDir
            if (!$modernResult.Contains("Attempt")) {
                $modernResult.Add("Attempt", $attempt + 1)
            }
            if ($modernResult.Status -eq "Passed") {
                break
            }
        }
        catch {
            $lastModernError = $_.Exception.Message
            $modernResult = $null
        }
    }

    if ($null -eq $modernResult) {
        $modernResult = [ordered]@{
            App = "ModernWpf"
            Control = $control
            Route = "item/$control"
            Status = "Failed"
            Title = ""
            Screenshot = ""
            UiaTree = ""
            LastException = $lastModernError
            NonBlank = $false
            RequiredSampleAutomationId = Get-RequiredSampleAutomationId $control
            RequiredSampleElementFound = $false
            Attempt = $ModernWpfRetries + 1
        }
    }
    $results.Add($modernResult)

    if ($Reference -eq "InstalledWinUI3Gallery") {
        try {
            $referenceResult = Capture-WinUIReference $control $caseDir
            $results.Add($referenceResult)
        }
        catch {
            $results.Add([ordered]@{
                App = "WinUI3Gallery"
                Control = $control
                Route = "winui3gallery://item/$control"
                Status = "Skipped"
                Title = ""
                Screenshot = ""
                UiaTree = ""
                LastException = $_.Exception.Message
                NonBlank = $false
                RequiredSampleAutomationId = ""
                RequiredSampleElementFound = $false
            })
        }
    }

    $modern = $results | Where-Object { $_.Control -eq $control -and $_.App -eq "ModernWpf" } | Select-Object -Last 1
    $referenceCapture = $results | Where-Object { $_.Control -eq $control -and $_.App -eq "WinUI3Gallery" } | Select-Object -Last 1
    if ($null -ne $modern -and $null -ne $referenceCapture -and $modern.Screenshot -and $referenceCapture.Screenshot) {
        $comparison = Compare-Images $modern.Screenshot $referenceCapture.Screenshot
        $modern["ReferenceComparison"] = $comparison
        if ($FailOnDifference -and $comparison.Comparable -and $comparison.MeanDelta -gt 24) {
            $modern["Status"] = "Failed"
            $modern["LastException"] = "Mean pixel delta $($comparison.MeanDelta) exceeded visual threshold 24."
        }
    }
}

$reportJson = Join-Path $runDir "report.json"
$reportMarkdown = Join-Path $runDir "report.md"
$results.ToArray() | ConvertTo-Json -Depth 6 | Set-Content -Path $reportJson -Encoding UTF8

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add("# Gallery Visual Check Report")
$markdown.Add("")
$markdown.Add("- Theme: $Theme")
$markdown.Add("- Size: ${Width}x${Height}")
$markdown.Add("- Reference: $Reference")
$markdown.Add("")
$markdown.Add("| App | Control | Status | Nonblank | Required sample element | Notes |")
$markdown.Add("| --- | --- | --- | --- | --- | --- |")
foreach ($result in $results) {
    $notes = $result.LastException
    if ($result.Contains("ReferenceComparison")) {
        $notes = "Mean delta: " + $result.ReferenceComparison.MeanDelta
    }

    $markdown.Add("| $($result.App) | $($result.Control) | $($result.Status) | $($result.NonBlank) | $($result.RequiredSampleElementFound) | $notes |")
}
$markdown | Set-Content -Path $reportMarkdown -Encoding UTF8

$failed = @($results | Where-Object { $_.Status -eq "Failed" })
Write-Host "Visual check artifacts: $runDir"
Write-Host "Report: $reportMarkdown"
if ($failed.Count -gt 0) {
    $failed |
        ForEach-Object {
            [pscustomobject]@{
                App = $_.App
                Control = $_.Control
                Status = $_.Status
                LastException = $_.LastException
            }
        } |
        Format-Table App, Control, Status, LastException -AutoSize
    exit 1
}
