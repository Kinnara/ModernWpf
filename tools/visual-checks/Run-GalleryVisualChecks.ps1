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
    [switch]$IncludeInteractions,
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

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    public static bool Move(IntPtr hWnd, int x, int y, int width, int height)
    {
        ShowWindow(hWnd, 9);
        return MoveWindow(hWnd, x, y, width, height, true);
    }

    public static void Activate(IntPtr hWnd)
    {
        ShowWindow(hWnd, 9);
        SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, 0x0043);
        SetForegroundWindow(hWnd);
        SetWindowPos(hWnd, new IntPtr(-2), 0, 0, 0, 0, 0x0043);
    }

    public static void Click(int x, int y)
    {
        SetCursorPos(x, y);
        mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
        mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
    }

    public static void PressSpace()
    {
        keybd_event(0x20, 0, 0, UIntPtr.Zero);
        keybd_event(0x20, 0, 0x0002, UIntPtr.Zero);
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

    public static bool CopyWindowSurface(IntPtr hWnd, IntPtr hdcDest, int width, int height)
    {
        IntPtr hdcSource = GetWindowDC(hWnd);
        if (hdcSource == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            return BitBlt(hdcDest, 0, 0, width, height, hdcSource, 0, 0, 0x00CC0020);
        }
        finally
        {
            ReleaseDC(hWnd, hdcSource);
        }
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

function Find-DescendantButtonByName($root, [string]$name) {
    $nameCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $name)
    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $condition = New-Object System.Windows.Automation.AndCondition($nameCondition, $buttonCondition)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Find-DescendantByAnyName($root, [string[]]$names) {
    foreach ($name in $names) {
        $element = Find-DescendantByName $root $name
        if ($null -ne $element) {
            return $element
        }
    }

    return $null
}

function Find-ElementByNameInProcess([int]$processId, [string[]]$names) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        $match = Find-DescendantByAnyName $window $names
        if ($null -ne $match) {
            return $match
        }
    }

    return $null
}

function Find-ElementByAutomationIdInProcess([int]$processId, [string]$automationId) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        $match = Find-DescendantByAutomationId $window $automationId
        if ($null -ne $match) {
            return $match
        }
    }

    return $null
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

function Get-SampleRootAutomationId([string]$control) {
    return "GallerySample_${control}_Root"
}

function Get-ModernPrimaryCropAutomationId([string]$control) {
    switch ($control) {
        "InfoBar" { return "GallerySample_InfoBar_InfoBar" }
        default { return Get-RequiredSampleAutomationId $control }
    }
}

function Get-ReferencePrimaryAutomationId([string]$control) {
    switch ($control) {
        "TeachingTip" { return "TestButton1" }
        "Button" { return "Button1" }
        "ComboBox" { return "Combo1" }
        "InfoBar" { return "TestInfoBar1" }
        "NavigationView" { return "nvSample5" }
        "ContentDialog" { return "ShowDialog" }
        default { return "" }
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

function Test-BitmapNotBlank([System.Drawing.Bitmap]$bitmap) {
    $colors = New-Object "System.Collections.Generic.HashSet[int]"
    $visibleSamples = 0
    $nonBlackSamples = 0
    $stepX = [Math]::Max(1, [int]($bitmap.Width / 32))
    $stepY = [Math]::Max(1, [int]($bitmap.Height / 32))
    for ($x = 0; $x -lt $bitmap.Width; $x += $stepX) {
        for ($y = 0; $y -lt $bitmap.Height; $y += $stepY) {
            $pixel = $bitmap.GetPixel($x, $y)
            if ($pixel.A -gt 16) {
                $visibleSamples++
                [void]$colors.Add(($pixel.R -shl 16) -bor ($pixel.G -shl 8) -bor $pixel.B)
                if (($pixel.R + $pixel.G + $pixel.B) -gt 36) {
                    $nonBlackSamples++
                }
            }
        }
    }

    return $colors.Count -gt 4 -and $visibleSamples -gt 0 -and $nonBlackSamples -gt 0
}

function Test-BitmapHasClientContent([System.Drawing.Bitmap]$bitmap) {
    if (!(Test-BitmapNotBlank $bitmap)) {
        return $false
    }

    $colors = New-Object "System.Collections.Generic.HashSet[int]"
    $sampleCount = 0
    $nonBlackCount = 0
    $startY = [Math]::Min($bitmap.Height - 1, 48)
    $endY = [Math]::Max($startY, $bitmap.Height - 12)
    $stepX = [Math]::Max(1, [int]($bitmap.Width / 40))
    $stepY = [Math]::Max(1, [int](($endY - $startY + 1) / 32))

    for ($x = 8; $x -lt ($bitmap.Width - 8); $x += $stepX) {
        for ($y = $startY; $y -lt $endY; $y += $stepY) {
            $pixel = $bitmap.GetPixel($x, $y)
            [void]$colors.Add($pixel.ToArgb())
            $sampleCount++
            if (($pixel.R + $pixel.G + $pixel.B) -gt 36) {
                $nonBlackCount++
            }
        }
    }

    return $colors.Count -gt 8 -and $sampleCount -gt 0 -and ($nonBlackCount / [double]$sampleCount) -gt 0.015
}

function Test-ImageNotBlank([string]$path) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        return Test-BitmapNotBlank $bitmap
    }
    finally {
        $bitmap.Dispose()
    }
}

function Get-ImageSize([string]$path) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        return [ordered]@{
            Width = $bitmap.Width
            Height = $bitmap.Height
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Capture-Window([IntPtr]$hwnd, [string]$path) {
    [GalleryVisualNative]::Activate($hwnd)
    Start-Sleep -Milliseconds 300
    $rect = [GalleryVisualNative]::GetRect($hwnd)
    $width = [Math]::Max(1, $rect.Right - $rect.Left)
    $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        foreach ($attempt in 1..5) {
            foreach ($flags in @(2, 0)) {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $hdc = $graphics.GetHdc()
                $printed = $false
                try {
                    $printed = [GalleryVisualNative]::PrintWindow($hwnd, $hdc, [uint32]$flags)
                }
                finally {
                    $graphics.ReleaseHdc($hdc)
                }

                if ($printed -and (Test-BitmapNotBlank $bitmap)) {
                    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
                    return
                }
            }

            Start-Sleep -Milliseconds 250
        }

        $graphics.Clear([System.Drawing.Color]::Transparent)
        $hdc = $graphics.GetHdc()
        $copied = $false
        try {
            $copied = [GalleryVisualNative]::CopyWindowSurface($hwnd, $hdc, $width, $height)
        }
        finally {
            $graphics.ReleaseHdc($hdc)
        }

        if ($copied -and (Test-BitmapNotBlank $bitmap)) {
            $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
            return
        }

        throw "PrintWindow did not produce a valid app-content capture for window handle $hwnd."
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Capture-ScreenRect([IntPtr]$hwnd, [string]$path) {
    $rect = [GalleryVisualNative]::GetRect($hwnd)
    $width = [Math]::Max(1, $rect.Right - $rect.Left)
    $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Find-DifferenceBounds([string]$beforePath, [string]$afterPath, [int]$threshold = 32, [int]$step = 2) {
    $before = [System.Drawing.Bitmap]::FromFile($beforePath)
    $after = [System.Drawing.Bitmap]::FromFile($afterPath)
    try {
        if ($before.Width -ne $after.Width -or $before.Height -ne $after.Height) {
            return [ordered]@{
                Found = $false
                Reason = "Image dimensions differ."
                X = 0
                Y = 0
                Width = 0
                Height = 0
                ChangedSamples = 0
            }
        }

        $gridWidth = [int][Math]::Ceiling($before.Width / [double]$step)
        $gridHeight = [int][Math]::Ceiling($before.Height / [double]$step)
        $changed = New-Object 'bool[,]' $gridWidth, $gridHeight

        for ($gridX = 0; $gridX -lt $gridWidth; $gridX++) {
            for ($gridY = 0; $gridY -lt $gridHeight; $gridY++) {
                $x = [Math]::Min($before.Width - 1, $gridX * $step)
                $y = [Math]::Min($before.Height - 1, $gridY * $step)
                $a = $before.GetPixel($x, $y)
                $b = $after.GetPixel($x, $y)
                $delta = [Math]::Abs($a.R - $b.R) + [Math]::Abs($a.G - $b.G) + [Math]::Abs($a.B - $b.B)
                if ($delta -gt $threshold) {
                    $changed[$gridX, $gridY] = $true
                }
            }
        }

        $visited = New-Object 'bool[,]' $gridWidth, $gridHeight
        $components = New-Object System.Collections.Generic.List[object]
        $queueX = New-Object System.Collections.Generic.Queue[int]
        $queueY = New-Object System.Collections.Generic.Queue[int]

        for ($startX = 0; $startX -lt $gridWidth; $startX++) {
            for ($startY = 0; $startY -lt $gridHeight; $startY++) {
                if (!$changed[$startX, $startY] -or $visited[$startX, $startY]) {
                    continue
                }

                $visited[$startX, $startY] = $true
                $queueX.Enqueue($startX)
                $queueY.Enqueue($startY)
                $minGridX = $startX
                $maxGridX = $startX
                $minGridY = $startY
                $maxGridY = $startY
                $componentSamples = 0

                while ($queueX.Count -gt 0) {
                    $currentX = $queueX.Dequeue()
                    $currentY = $queueY.Dequeue()
                    $componentSamples++
                    if ($currentX -lt $minGridX) { $minGridX = $currentX }
                    if ($currentX -gt $maxGridX) { $maxGridX = $currentX }
                    if ($currentY -lt $minGridY) { $minGridY = $currentY }
                    if ($currentY -gt $maxGridY) { $maxGridY = $currentY }

                    for ($offsetX = -1; $offsetX -le 1; $offsetX++) {
                        for ($offsetY = -1; $offsetY -le 1; $offsetY++) {
                            if ($offsetX -eq 0 -and $offsetY -eq 0) {
                                continue
                            }

                            $nextX = $currentX + $offsetX
                            $nextY = $currentY + $offsetY
                            if ($nextX -lt 0 -or $nextY -lt 0 -or $nextX -ge $gridWidth -or $nextY -ge $gridHeight) {
                                continue
                            }

                            if ($changed[$nextX, $nextY] -and !$visited[$nextX, $nextY]) {
                                $visited[$nextX, $nextY] = $true
                                $queueX.Enqueue($nextX)
                                $queueY.Enqueue($nextY)
                            }
                        }
                    }
                }

                $componentX = $minGridX * $step
                $componentY = $minGridY * $step
                $componentRight = [Math]::Min($before.Width, ($maxGridX + 1) * $step)
                $componentBottom = [Math]::Min($before.Height, ($maxGridY + 1) * $step)
                $components.Add([pscustomobject]@{
                    X = $componentX
                    Y = $componentY
                    Width = $componentRight - $componentX
                    Height = $componentBottom - $componentY
                    Right = $componentRight
                    Bottom = $componentBottom
                    Count = $componentSamples
                    Merged = $false
                })
            }
        }

        if ($components.Count -eq 0) {
            return [ordered]@{
                Found = $false
                Reason = "No changed pixels exceeded threshold $threshold."
                X = 0
                Y = 0
                Width = 0
                Height = 0
                ChangedSamples = 0
            }
        }

        $primary = $components | Sort-Object Count -Descending | Select-Object -First 1
        $primary.Merged = $true
        $clusterX = $primary.X
        $clusterY = $primary.Y
        $clusterRight = $primary.Right
        $clusterBottom = $primary.Bottom
        $changedSamples = $primary.Count
        $mergeGap = 36
        $mergedAny = $true

        while ($mergedAny) {
            $mergedAny = $false
            foreach ($component in $components) {
                if ($component.Merged) {
                    continue
                }

                $horizontalGap = if ($component.X -gt $clusterRight) {
                    $component.X - $clusterRight
                }
                elseif ($clusterX -gt $component.Right) {
                    $clusterX - $component.Right
                }
                else {
                    0
                }

                $verticalGap = if ($component.Y -gt $clusterBottom) {
                    $component.Y - $clusterBottom
                }
                elseif ($clusterY -gt $component.Bottom) {
                    $clusterY - $component.Bottom
                }
                else {
                    0
                }

                if ($horizontalGap -le $mergeGap -and $verticalGap -le $mergeGap) {
                    $component.Merged = $true
                    $clusterX = [Math]::Min($clusterX, $component.X)
                    $clusterY = [Math]::Min($clusterY, $component.Y)
                    $clusterRight = [Math]::Max($clusterRight, $component.Right)
                    $clusterBottom = [Math]::Max($clusterBottom, $component.Bottom)
                    $changedSamples += $component.Count
                    $mergedAny = $true
                }
            }
        }

        return [ordered]@{
            Found = $true
            Reason = ""
            X = $clusterX
            Y = $clusterY
            Width = $clusterRight - $clusterX
            Height = $clusterBottom - $clusterY
            ChangedSamples = $changedSamples
        }
    }
    finally {
        $before.Dispose()
        $after.Dispose()
    }
}

function Expand-Bounds($bounds, [int]$imageWidth, [int]$imageHeight, [int]$padding) {
    if ($null -eq $bounds -or !$bounds.Found) {
        return $bounds
    }

    $x = [Math]::Max(0, $bounds.X - $padding)
    $y = [Math]::Max(0, $bounds.Y - $padding)
    $right = [Math]::Min($imageWidth, $bounds.X + $bounds.Width + $padding)
    $bottom = [Math]::Min($imageHeight, $bounds.Y + $bounds.Height + $padding)
    return [ordered]@{
        Found = $true
        Reason = ""
        X = $x
        Y = $y
        Width = [Math]::Max(1, $right - $x)
        Height = [Math]::Max(1, $bottom - $y)
        ChangedSamples = $bounds.ChangedSamples
    }
}

function Trim-DifferenceBoundsToContentRoot($bounds, $targetBounds, [int]$tailLength = 8) {
    if ($null -eq $bounds -or !$bounds.Found -or $null -eq $targetBounds -or !$targetBounds.Found) {
        return $bounds
    }

    $x = [int]$bounds.X
    $y = [int]$bounds.Y
    $width = [int]$bounds.Width
    $height = [int]$bounds.Height
    $centerX = $x + ($width / 2.0)
    $centerY = $y + ($height / 2.0)
    $targetCenterX = [int]$targetBounds.X + ([int]$targetBounds.Width / 2.0)
    $targetCenterY = [int]$targetBounds.Y + ([int]$targetBounds.Height / 2.0)

    if ([Math]::Abs($targetCenterY - $centerY) -ge [Math]::Abs($targetCenterX - $centerX)) {
        if ($height -gt ($tailLength * 2) -and $targetCenterY -gt $centerY) {
            $height -= $tailLength
        }
        elseif ($height -gt ($tailLength * 2) -and $targetCenterY -lt $centerY) {
            $y += $tailLength
            $height -= $tailLength
        }
    }
    else {
        if ($width -gt ($tailLength * 2) -and $targetCenterX -gt $centerX) {
            $width -= $tailLength
        }
        elseif ($width -gt ($tailLength * 2) -and $targetCenterX -lt $centerX) {
            $x += $tailLength
            $width -= $tailLength
        }
    }

    return [ordered]@{
        Found = $true
        Reason = ""
        X = $x
        Y = $y
        Width = [Math]::Max(1, $width)
        Height = [Math]::Max(1, $height)
        ChangedSamples = $bounds.ChangedSamples
    }
}

function Save-Crop([string]$sourcePath, $bounds, [string]$path, [int]$padding = 12) {
    $source = [System.Drawing.Bitmap]::FromFile($sourcePath)
    try {
        $expandedBounds = Expand-Bounds $bounds $source.Width $source.Height $padding
        $rectangle = [System.Drawing.Rectangle]::new(
            [int]$expandedBounds.X,
            [int]$expandedBounds.Y,
            [int]$expandedBounds.Width,
            [int]$expandedBounds.Height)
        $crop = $source.Clone($rectangle, $source.PixelFormat)
        try {
            $crop.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $crop.Dispose()
        }

        return $expandedBounds
    }
    finally {
        $source.Dispose()
    }
}

function Save-ElementCrop($window, [string]$screenshot, [string]$path, $element, [string]$source, [int]$padding = 8) {
    $bounds = Get-ElementWindowBounds $window $element
    if ($null -eq $bounds -or !$bounds.Found) {
        return [ordered]@{
            Found = $false
            Source = $source
            Screenshot = ""
            Bounds = $bounds
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    $expandedBounds = Save-Crop $screenshot $bounds $path $padding
    return [ordered]@{
        Found = $true
        Source = $source
        Screenshot = $path
        Bounds = $expandedBounds
        Width = $expandedBounds.Width
        Height = $expandedBounds.Height
        NonBlank = Test-ImageNotBlank $path
    }
}

function New-RenderedArtifactCrop([string]$path, [string]$source, $bounds) {
    if (!(Test-Path $path)) {
        return $null
    }

    $size = Get-ImageSize $path
    return [ordered]@{
        Found = $true
        Source = $source
        Screenshot = $path
        Bounds = $bounds
        Width = $size.Width
        Height = $size.Height
        NonBlank = Test-ImageNotBlank $path
    }
}

function Capture-StaticCrops([string]$app, [string]$control, [string]$caseDir, $window, [string]$screenshot) {
    $primaryElement = $null
    $primarySource = ""
    $sampleElement = $null
    $sampleSource = ""

    if ($app -eq "ModernWpf") {
        $primarySource = Get-ModernPrimaryCropAutomationId $control
        $primaryElement = Find-DescendantByAutomationId $window $primarySource
        $sampleSource = Get-SampleRootAutomationId $control
        $sampleElement = Find-DescendantByAutomationId $window $sampleSource
    }
    else {
        $primarySource = Get-ReferencePrimaryAutomationId $control
        if (![string]::IsNullOrEmpty($primarySource)) {
            $primaryElement = Find-DescendantByAutomationId $window $primarySource
        }
        $sampleSource = "svPanel"
        $sampleElement = Find-DescendantByAutomationId $window $sampleSource
    }

    $primaryPath = Join-Path $caseDir ("{0}-{1}-primary-crop.png" -f $app.ToLowerInvariant(), $control)
    $samplePath = Join-Path $caseDir ("{0}-{1}-sample-crop.png" -f $app.ToLowerInvariant(), $control)
    $primaryBounds = Get-ElementWindowBounds $window $primaryElement
    $sampleBounds = Get-ElementWindowBounds $window $sampleElement

    if ($app -eq "ModernWpf") {
        $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
        $primaryArtifact = Join-Path $artifactDir ($primarySource + ".png")
        $sampleArtifact = Join-Path $artifactDir ($sampleSource + ".png")
        $primaryCrop = New-RenderedArtifactCrop $primaryArtifact $primarySource $primaryBounds
        $sampleCrop = New-RenderedArtifactCrop $sampleArtifact $sampleSource $sampleBounds

        return [ordered]@{
            Primary = $(if ($null -ne $primaryCrop) { $primaryCrop } else { Save-ElementCrop $window $screenshot $primaryPath $primaryElement $primarySource 0 })
            Sample = $(if ($null -ne $sampleCrop) { $sampleCrop } else { Save-ElementCrop $window $screenshot $samplePath $sampleElement $sampleSource 10 })
        }
    }

    return [ordered]@{
        Primary = Save-ElementCrop $window $screenshot $primaryPath $primaryElement $primarySource 0
        Sample = Save-ElementCrop $window $screenshot $samplePath $sampleElement $sampleSource 10
    }
}

function Get-ElementWindowBounds($window, $element) {
    if ($null -eq $window -or $null -eq $element) {
        return $null
    }

    $windowRect = [GalleryVisualNative]::GetRect($window.Current.NativeWindowHandle)
    $windowUiaRect = $window.Current.BoundingRectangle
    $rect = $element.Current.BoundingRectangle
    $x = [double]$rect.X
    $y = [double]$rect.Y
    $width = [double]$rect.Width
    $height = [double]$rect.Height

    if ([double]::IsInfinity($x) -or [double]::IsInfinity($y) -or
        [double]::IsInfinity($width) -or [double]::IsInfinity($height) -or
        [double]::IsNaN($x) -or [double]::IsNaN($y) -or
        [double]::IsNaN($width) -or [double]::IsNaN($height) -or
        $width -le 0 -or $height -le 0) {
        return $null
    }

    $nativeWidth = [Math]::Max(1, $windowRect.Right - $windowRect.Left)
    $nativeHeight = [Math]::Max(1, $windowRect.Bottom - $windowRect.Top)
    $windowUiaWidth = [double]$windowUiaRect.Width
    $windowUiaHeight = [double]$windowUiaRect.Height
    $scaleX = if ($windowUiaWidth -gt 0 -and ![double]::IsInfinity($windowUiaWidth) -and ![double]::IsNaN($windowUiaWidth)) {
        $nativeWidth / $windowUiaWidth
    }
    else {
        1.0
    }
    $scaleY = if ($windowUiaHeight -gt 0 -and ![double]::IsInfinity($windowUiaHeight) -and ![double]::IsNaN($windowUiaHeight)) {
        $nativeHeight / $windowUiaHeight
    }
    else {
        1.0
    }

    return [ordered]@{
        Found = $true
        Reason = ""
        X = [Math]::Max(0, [int][Math]::Round(($x - $windowUiaRect.X) * $scaleX))
        Y = [Math]::Max(0, [int][Math]::Round(($y - $windowUiaRect.Y) * $scaleY))
        Width = [Math]::Max(1, [int][Math]::Round($width * $scaleX))
        Height = [Math]::Max(1, [int][Math]::Round($height * $scaleY))
        ChangedSamples = 0
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

function Compare-ImagesNormalized([string]$leftPath, [string]$rightPath) {
    $left = [System.Drawing.Bitmap]::FromFile($leftPath)
    $right = [System.Drawing.Bitmap]::FromFile($rightPath)
    try {
        $width = [Math]::Max(1, [Math]::Max($left.Width, $right.Width))
        $height = [Math]::Max(1, [Math]::Max($left.Height, $right.Height))
        $leftNormalized = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $rightNormalized = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $leftGraphics = [System.Drawing.Graphics]::FromImage($leftNormalized)
        $rightGraphics = [System.Drawing.Graphics]::FromImage($rightNormalized)
        try {
            $leftGraphics.Clear([System.Drawing.Color]::Transparent)
            $rightGraphics.Clear([System.Drawing.Color]::Transparent)
            $leftGraphics.DrawImage($left, 0, 0, $width, $height)
            $rightGraphics.DrawImage($right, 0, 0, $width, $height)

            $samples = 0
            $delta = 0.0
            $stepX = [Math]::Max(1, [int]($width / 80))
            $stepY = [Math]::Max(1, [int]($height / 80))
            for ($x = 0; $x -lt $width; $x += $stepX) {
                for ($y = 0; $y -lt $height; $y += $stepY) {
                    $a = $leftNormalized.GetPixel($x, $y)
                    $b = $rightNormalized.GetPixel($x, $y)
                    $delta += ([Math]::Abs($a.R - $b.R) + [Math]::Abs($a.G - $b.G) + [Math]::Abs($a.B - $b.B)) / 3.0
                    $samples++
                }
            }

            return [ordered]@{
                Comparable = $true
                Reason = ""
                MeanDelta = [Math]::Round($delta / [Math]::Max(1, $samples), 2)
                NormalizedWidth = $width
                NormalizedHeight = $height
            }
        }
        finally {
            $leftGraphics.Dispose()
            $rightGraphics.Dispose()
            $leftNormalized.Dispose()
            $rightNormalized.Dispose()
        }
    }
    finally {
        $left.Dispose()
        $right.Dispose()
    }
}

function Invoke-Element($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    $invoked = $false
    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        [GalleryVisualNative]::Click(
            [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
            [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
        $invoked = $true
        Start-Sleep -Milliseconds 50
    }

    try {
        $element.SetFocus()
        [GalleryVisualNative]::PressSpace()
        $invoked = $true
        Start-Sleep -Milliseconds 50
    }
    catch {
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            $invoked = $true
        }
    }
    catch {
    }

    return $invoked
}

function Capture-TeachingTipInteraction([string]$app, [string]$control, [string]$caseDir, $window, $showButton, [string[]]$openNames) {
    if (!$IncludeInteractions -or $control -ne "TeachingTip") {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250
    $baselinePath = Join-Path $caseDir ("{0}-{1}-closed.png" -f $app.ToLowerInvariant(), $control)
    Capture-Window $window.Current.NativeWindowHandle $baselinePath
    $invoked = Invoke-Element $window $showButton
    $frames = New-Object System.Collections.Generic.List[object]
    $frameDelays = @(0, 150, 300, 450)
    $previousDelay = 0

    foreach ($delay in $frameDelays) {
        if ($delay -gt $previousDelay) {
            Start-Sleep -Milliseconds ($delay - $previousDelay)
        }
        $previousDelay = $delay

        $framePath = Join-Path $caseDir ("{0}-{1}-open-{2:D3}ms.png" -f $app.ToLowerInvariant(), $control, $delay)
        Capture-ScreenRect $window.Current.NativeWindowHandle $framePath
        $frames.Add([ordered]@{
            DelayMs = $delay
            Screenshot = $framePath
            NonBlank = Test-ImageNotBlank $framePath
        })
    }

    $openElement = Find-ElementByNameInProcess $window.Current.ProcessId $openNames
    if ($null -ne $openElement) {
        $treePath = Join-Path $caseDir ("{0}-{1}-open.uia.txt" -f $app.ToLowerInvariant(), $control)
        Write-UiaTree $openElement $treePath 3
        $cropElement = Find-DescendantByAutomationId $openElement "ContentRootGrid"
        if ($null -eq $cropElement) {
            $cropElement = $openElement
        }
    }
    else {
        $treePath = ""
        $cropElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "ContentRootGrid"
    }

    $openDelta = $null
    $visualOpened = $false
    $crop = $null
    if ($frames.Count -gt 0) {
        $lastFrame = $frames[$frames.Count - 1]
        $openDelta = Compare-Images $baselinePath $lastFrame.Screenshot
        $elementBounds = Get-ElementWindowBounds $window $cropElement
        if ($null -ne $elementBounds -and $elementBounds.Found) {
            $cropPath = Join-Path $caseDir ("{0}-{1}-open-crop.png" -f $app.ToLowerInvariant(), $control)
            $expandedBounds = Save-Crop $lastFrame.Screenshot $elementBounds $cropPath
            $crop = [ordered]@{
                Found = $true
                Screenshot = $cropPath
                Bounds = $expandedBounds
                Width = $expandedBounds.Width
                Height = $expandedBounds.Height
                ChangedSamples = 0
                Source = "UIA"
            }
        }
        else {
            $differenceBounds = Find-DifferenceBounds $baselinePath $lastFrame.Screenshot
            if ($differenceBounds.Found) {
                $targetBounds = Get-ElementWindowBounds $window $showButton
                $differenceBounds = Trim-DifferenceBoundsToContentRoot $differenceBounds $targetBounds
                $cropPath = Join-Path $caseDir ("{0}-{1}-open-crop.png" -f $app.ToLowerInvariant(), $control)
                $expandedBounds = Save-Crop $lastFrame.Screenshot $differenceBounds $cropPath
                $crop = [ordered]@{
                    Found = $true
                    Screenshot = $cropPath
                    Bounds = $expandedBounds
                    Width = $expandedBounds.Width
                    Height = $expandedBounds.Height
                    ChangedSamples = $differenceBounds.ChangedSamples
                    Source = "Difference"
                }
            }
            else {
                $crop = [ordered]@{
                    Found = $false
                    Screenshot = ""
                    Bounds = $differenceBounds
                    Width = 0
                    Height = 0
                    ChangedSamples = 0
                    Source = "None"
                }
            }
        }

        $visualOpened = $openDelta.Comparable -and $openDelta.MeanDelta -gt 1.0
    }

    $status = if (!$invoked) { "Failed" } elseif ($null -ne $openElement -or $visualOpened) { "Passed" } else { "Failed" }
    $notes = if (!$invoked) { "Could not invoke the TeachingTip sample button." } elseif ($null -eq $openElement -and !$visualOpened) { "TeachingTip did not produce UIA or visual evidence of opening." } elseif ($null -eq $openElement) { "TeachingTip open content was not found in UIA; visual delta verified." } else { "" }

    return [ordered]@{
        Status = $status
        Invoked = $invoked
        BaselineScreenshot = $baselinePath
        OpenElementFound = $null -ne $openElement
        OpenElementName = $(if ($null -ne $openElement) { $openElement.Current.Name } else { "" })
        UiaTree = $treePath
        Frames = $frames.ToArray()
        OpenDelta = $openDelta
        Crop = $crop
        Notes = $notes
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
        Start-Sleep -Milliseconds 600

        $lastException = Get-AutomationText $window "GalleryVisualTestLastException"
        $title = Get-AutomationText $window "GalleryItemPageTitle"
        $requiredSampleAutomationId = Get-RequiredSampleAutomationId $control
        $sample = Find-DescendantByAutomationId $window $requiredSampleAutomationId
        $interaction = Capture-TeachingTipInteraction "ModernWpf" $control $caseDir $window $sample @("This is the title", "Try compact mode", "And this is the subtitle")
        $screenshot = Join-Path $caseDir "modernwpf-$control.png"
        $treePath = Join-Path $caseDir "modernwpf-$control.uia.txt"

        Write-UiaTree $window $treePath 6
        $windowCaptureError = ""
        try {
            Capture-Window $window.Current.NativeWindowHandle $screenshot
        }
        catch {
            $windowCaptureError = $_.Exception.Message
        }
        $staticCrops = Capture-StaticCrops "ModernWpf" $control $caseDir $window $screenshot
        $hasRenderedCrops = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and $staticCrops.Primary.NonBlank
        $notBlank = if (Test-Path $screenshot) { Test-ImageNotBlank $screenshot } elseif ($hasRenderedCrops) { $true } else { $false }
        $primaryCropBlank = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and !$staticCrops.Primary.NonBlank
        $status = if ($lastException) { "Failed" } elseif (!$notBlank) { "Failed" } elseif ($primaryCropBlank) { "Failed" } elseif ($null -eq $sample) { "Failed" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { "Failed" } else { "Passed" }
        if ($primaryCropBlank -and [string]::IsNullOrEmpty($lastException)) {
            $lastException = "Primary crop '$($staticCrops.Primary.Source)' was blank."
        }
        if (!$notBlank -and ![string]::IsNullOrEmpty($windowCaptureError) -and [string]::IsNullOrEmpty($lastException)) {
            $lastException = $windowCaptureError
        }
        if ($null -ne $interaction -and $interaction.Status -ne "Passed" -and [string]::IsNullOrEmpty($lastException)) {
            $lastException = $interaction.Notes
        }
        $screenshotResult = if (Test-Path $screenshot) { $screenshot } else { "" }

        return [ordered]@{
            App = "ModernWpf"
            Control = $control
            Route = $route
            Status = $status
            Title = $title
            Screenshot = $screenshotResult
            UiaTree = $treePath
            LastException = $lastException
            NonBlank = $notBlank
            RequiredSampleAutomationId = $requiredSampleAutomationId
            RequiredSampleElementFound = $null -ne $sample
            StaticCrops = $staticCrops
            Interaction = $interaction
            WindowCaptureError = $windowCaptureError
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

    try {
        [void][GalleryVisualNative]::Move($window.Current.NativeWindowHandle, 1280, 60, $Width, $Height)
        Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "WinUI 3 Gallery page title '$control'" -Probe {
            Find-DescendantByName $window $control
        } | Out-Null
        Start-Sleep -Milliseconds 600

        $showButton = if ($control -eq "TeachingTip") { Find-DescendantButtonByName $window "Show TeachingTip" } else { $null }
        $interaction = Capture-TeachingTipInteraction "WinUI3" $control $caseDir $window $showButton @("This is the title", "And this is the subtitle")
        $screenshot = Join-Path $caseDir "winui3-$control.png"
        $treePath = Join-Path $caseDir "winui3-$control.uia.txt"
        Write-UiaTree $window $treePath 6
        Capture-Window $window.Current.NativeWindowHandle $screenshot
        $staticCrops = Capture-StaticCrops "WinUI3" $control $caseDir $window $screenshot
        $notBlank = Test-ImageNotBlank $screenshot
        $primaryCropBlank = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and !$staticCrops.Primary.NonBlank

        return [ordered]@{
            App = "WinUI3Gallery"
            Control = $control
            Route = $route
            Status = $(if (!$notBlank) { "Failed" } elseif ($primaryCropBlank) { "Failed" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { "Failed" } else { "Passed" })
            Title = $control
            Screenshot = $screenshot
            UiaTree = $treePath
            LastException = $(if ($primaryCropBlank) { "Primary crop '$($staticCrops.Primary.Source)' was blank." } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { $interaction.Notes } else { "" })
            NonBlank = $notBlank
            RequiredSampleAutomationId = ""
            RequiredSampleElementFound = $true
            StaticCrops = $staticCrops
            Interaction = $interaction
        }
    }
    finally {
        Close-AutomationWindow $window
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
    if ($null -ne $modern -and $null -ne $referenceCapture -and
        $modern.Contains("StaticCrops") -and $referenceCapture.Contains("StaticCrops") -and
        $null -ne $modern.StaticCrops -and $null -ne $referenceCapture.StaticCrops -and
        $modern.StaticCrops.Primary.Found -and $referenceCapture.StaticCrops.Primary.Found) {
        $modern["PrimaryCropReferenceComparison"] = Compare-ImagesNormalized $modern.StaticCrops.Primary.Screenshot $referenceCapture.StaticCrops.Primary.Screenshot
        $modern["PrimaryCropSize"] = [ordered]@{
            ModernWpfWidth = $modern.StaticCrops.Primary.Width
            ModernWpfHeight = $modern.StaticCrops.Primary.Height
            ReferenceWidth = $referenceCapture.StaticCrops.Primary.Width
            ReferenceHeight = $referenceCapture.StaticCrops.Primary.Height
        }
    }
    if ($null -ne $modern -and $null -ne $referenceCapture -and
        $modern.Contains("Interaction") -and $referenceCapture.Contains("Interaction") -and
        $null -ne $modern.Interaction -and $null -ne $referenceCapture.Interaction -and
        $modern.Interaction.Frames.Count -gt 0 -and $referenceCapture.Interaction.Frames.Count -gt 0) {
        $modernFrame = $modern.Interaction.Frames[$modern.Interaction.Frames.Count - 1]
        $referenceFrame = $referenceCapture.Interaction.Frames[$referenceCapture.Interaction.Frames.Count - 1]
        if ($modernFrame.Screenshot -and $referenceFrame.Screenshot) {
            $modern["InteractionReferenceComparison"] = Compare-Images $modernFrame.Screenshot $referenceFrame.Screenshot
        }

        if ($null -ne $modern.Interaction.Crop -and $null -ne $referenceCapture.Interaction.Crop -and
            $modern.Interaction.Crop.Found -and $referenceCapture.Interaction.Crop.Found) {
            $modern["InteractionCropReferenceComparison"] = Compare-ImagesNormalized $modern.Interaction.Crop.Screenshot $referenceCapture.Interaction.Crop.Screenshot
            $modern["InteractionCropSize"] = [ordered]@{
                ModernWpfWidth = $modern.Interaction.Crop.Width
                ModernWpfHeight = $modern.Interaction.Crop.Height
                ReferenceWidth = $referenceCapture.Interaction.Crop.Width
                ReferenceHeight = $referenceCapture.Interaction.Crop.Height
            }
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

$controlScores = @{}
foreach ($result in $results) {
    if ($result.App -ne "ModernWpf") {
        continue
    }

    $score = 0.0
    if ($result.Contains("PrimaryCropReferenceComparison") -and $result.PrimaryCropReferenceComparison.Comparable) {
        $score = [Math]::Max($score, [double]$result.PrimaryCropReferenceComparison.MeanDelta)
    }
    if ($result.Contains("PrimaryCropSize")) {
        $sizeDelta = [Math]::Abs([int]$result.PrimaryCropSize.ModernWpfWidth - [int]$result.PrimaryCropSize.ReferenceWidth) +
            [Math]::Abs([int]$result.PrimaryCropSize.ModernWpfHeight - [int]$result.PrimaryCropSize.ReferenceHeight)
        $score += ($sizeDelta / 10.0)
    }
    if ($result.Contains("InteractionCropReferenceComparison") -and $result.InteractionCropReferenceComparison.Comparable) {
        $score = [Math]::Max($score, [double]$result.InteractionCropReferenceComparison.MeanDelta)
    }

    $controlScores[$result.Control] = [Math]::Round($score, 2)
}

$rankedModernResults = @(
    $results |
        Where-Object { $_.App -eq "ModernWpf" } |
        Sort-Object -Property @{ Expression = { if ($controlScores.ContainsKey($_.Control)) { -1.0 * [double]$controlScores[$_.Control] } else { 0 } } }, Control
)

$markdown.Add("## Crop Ranking")
$markdown.Add("")
$markdown.Add("| Control | Score | Primary crop delta | Primary crop sizes | Interaction crop delta |")
$markdown.Add("| --- | ---: | ---: | --- | ---: |")
foreach ($result in $rankedModernResults) {
    $score = if ($controlScores.ContainsKey($result.Control)) { $controlScores[$result.Control] } else { "" }
    $primaryDelta = if ($result.Contains("PrimaryCropReferenceComparison")) { $result.PrimaryCropReferenceComparison.MeanDelta } else { "" }
    $primarySize = if ($result.Contains("PrimaryCropSize")) { "$($result.PrimaryCropSize.ModernWpfWidth)x$($result.PrimaryCropSize.ModernWpfHeight) vs $($result.PrimaryCropSize.ReferenceWidth)x$($result.PrimaryCropSize.ReferenceHeight)" } else { "" }
    $interactionDelta = if ($result.Contains("InteractionCropReferenceComparison")) { $result.InteractionCropReferenceComparison.MeanDelta } else { "" }
    $markdown.Add("| $($result.Control) | $score | $primaryDelta | $primarySize | $interactionDelta |")
}
$markdown.Add("")

$markdown.Add("| App | Control | Status | Nonblank | Required sample element | Notes |")
$markdown.Add("| --- | --- | --- | --- | --- | --- |")
foreach ($result in ($results | Sort-Object -Property @{ Expression = { if ($controlScores.ContainsKey($_.Control)) { -1.0 * [double]$controlScores[$_.Control] } else { 0 } } }, Control, @{ Expression = { if ($_.App -eq "ModernWpf") { 0 } else { 1 } } })) {
    $notes = $result.LastException
    if ($result.Contains("ReferenceComparison")) {
        $notes = "Mean delta: " + $result.ReferenceComparison.MeanDelta
    }
    if ($result.Contains("PrimaryCropReferenceComparison")) {
        $notes = "$notes; primary crop delta: " + $result.PrimaryCropReferenceComparison.MeanDelta
    }
    if ($result.Contains("PrimaryCropSize")) {
        $notes = "$notes; primary crop sizes: $($result.PrimaryCropSize.ModernWpfWidth)x$($result.PrimaryCropSize.ModernWpfHeight) vs $($result.PrimaryCropSize.ReferenceWidth)x$($result.PrimaryCropSize.ReferenceHeight)"
    }
    if ($result.Contains("InteractionReferenceComparison")) {
        $notes = "$notes; interaction delta: " + $result.InteractionReferenceComparison.MeanDelta
    }
    if ($result.Contains("InteractionCropReferenceComparison")) {
        $notes = "$notes; crop delta: " + $result.InteractionCropReferenceComparison.MeanDelta
    }
    if ($result.Contains("InteractionCropSize")) {
        $notes = "$notes; crop sizes: $($result.InteractionCropSize.ModernWpfWidth)x$($result.InteractionCropSize.ModernWpfHeight) vs $($result.InteractionCropSize.ReferenceWidth)x$($result.InteractionCropSize.ReferenceHeight)"
    }
    if ($result.Contains("Interaction") -and $null -ne $result.Interaction -and $result.Interaction.Status -ne "Passed") {
        $notes = "$notes; interaction: " + $result.Interaction.Notes
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
