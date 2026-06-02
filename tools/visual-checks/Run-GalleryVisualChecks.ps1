param(
    [string[]]$Controls = @("TeachingTip", "Button", "CheckBox", "ComboBox", "RadioButton", "Slider", "ColorPicker", "HyperlinkButton", "RatingControl", "RepeatButton", "ToggleButton", "DropDownButton", "SplitButton", "ToggleSplitButton", "ToggleSwitch", "NumberBox", "AutoSuggestBox", "SplitView", "PersonPicture", "ParallaxView", "IconElement", "ThemeShadow", "TitleBar", "InfoBadge", "InfoBar", "ProgressRing", "PipsPager", "AnnotatedScrollBar", "PullToRefresh", "GridView", "ItemsRepeater", "BreadcrumbBar", "Pivot", "SelectorBar", "NavigationView", "ContentDialog", "Flyout", "Popup", "MenuBar", "MenuFlyout", "SwipeControl", "AppBarButton", "AppBarSeparator", "AppBarToggleButton", "CommandBar", "CommandBarFlyout"),
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
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

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

    public static void SetTopMost(IntPtr hWnd, bool topMost)
    {
        ShowWindow(hWnd, 9);
        SetWindowPos(hWnd, topMost ? new IntPtr(-1) : new IntPtr(-2), 0, 0, 0, 0, 0x0043);
        SetForegroundWindow(hWnd);
    }

    public static void Click(int x, int y)
    {
        SetCursorPos(x, y);
        System.Threading.Thread.Sleep(35);
        mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
        System.Threading.Thread.Sleep(80);
        mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
    }

    public static void PressSpace()
    {
        keybd_event(0x20, 0, 0, UIntPtr.Zero);
        keybd_event(0x20, 0, 0x0002, UIntPtr.Zero);
    }

    public static void PressCtrlA()
    {
        keybd_event(0x11, 0, 0, UIntPtr.Zero);
        keybd_event(0x41, 0, 0, UIntPtr.Zero);
        keybd_event(0x41, 0, 0x0002, UIntPtr.Zero);
        keybd_event(0x11, 0, 0x0002, UIntPtr.Zero);
    }

    public static void TypeText(string text)
    {
        foreach (char ch in text ?? string.Empty)
        {
            short key = VkKeyScan(ch);
            if (key == -1)
            {
                continue;
            }

            byte virtualKey = (byte)(key & 0xff);
            bool shift = (key & 0x0100) != 0;
            if (shift)
            {
                keybd_event(0x10, 0, 0, UIntPtr.Zero);
            }

            keybd_event(virtualKey, 0, 0, UIntPtr.Zero);
            keybd_event(virtualKey, 0, 0x0002, UIntPtr.Zero);

            if (shift)
            {
                keybd_event(0x10, 0, 0x0002, UIntPtr.Zero);
            }

            System.Threading.Thread.Sleep(25);
        }
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

    public static bool CopyScreenSurface(IntPtr hdcDest, int sourceX, int sourceY, int width, int height)
    {
        IntPtr hdcSource = GetDC(IntPtr.Zero);
        if (hdcSource == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            return BitBlt(hdcDest, 0, 0, width, height, hdcSource, sourceX, sourceY, 0x00CC0020);
        }
        finally
        {
            ReleaseDC(IntPtr.Zero, hdcSource);
        }
    }
}
"@

function New-RunDirectory {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss-fff"
    $root = Join-Path $RepoRoot $OutputRoot
    $baseName = "$timestamp-$PID"
    New-Item -ItemType Directory -Force -Path $root | Out-Null

    for ($attempt = 0; $attempt -lt 100; $attempt++) {
        $name = if ($attempt -eq 0) { $baseName } else { "$baseName-$attempt" }
        $path = Join-Path $root $name
        try {
            New-Item -ItemType Directory -Path $path -ErrorAction Stop | Out-Null
            return $path
        }
        catch {
            if (Test-Path -LiteralPath $path) {
                continue
            }

            throw
        }
    }

    throw "Could not create a unique visual audit output directory under '$root'."
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

function TryFind-DescendantByAutomationId($root, [string]$automationId) {
    try {
        return Find-DescendantByAutomationId $root $automationId
    }
    catch {
        return $null
    }
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

function Find-DescendantButtonByAnyName($root, [string[]]$names) {
    foreach ($name in $names) {
        $button = Find-DescendantButtonByName $root $name
        if ($null -ne $button) {
            return $button
        }
    }

    return $null
}

function Find-DescendantByControlType($root, $controlType) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        $controlType)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Find-ReferencePrimaryByName($root, [string]$control, [string]$name) {
    if ($control -eq "Pivot") {
        return Find-DescendantByName $root $name
    }

    return Find-DescendantButtonByName $root $name
}

function Reset-WinUIReferenceSampleScroll($window, [string]$control) {
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

function Test-ElementNameMatches($element, [string[]]$names) {
    if ($null -eq $element) {
        return $false
    }

    try {
        return $names -contains $element.Current.Name
    }
    catch {
        return $false
    }
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

function Find-ElementByNameInPopupWindows($window, [string[]]$names) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $window.Current.ProcessId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($candidateWindow in $windows) {
        try {
            if ($candidateWindow.Current.NativeWindowHandle -eq $window.Current.NativeWindowHandle) {
                continue
            }
        }
        catch {
            continue
        }

        $match = Find-DescendantByAnyName $candidateWindow $names
        if ($null -ne $match) {
            return $match
        }
    }

    return $null
}

function Find-ElementsByNameInProcess([int]$processId, [string[]]$names) {
    $matches = New-Object System.Collections.Generic.List[object]
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        foreach ($name in $names) {
            $nameCondition = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::NameProperty,
                $name)
            $found = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $nameCondition)
            foreach ($element in $found) {
                $matches.Add($element)
            }
        }
    }

    return $matches.ToArray()
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

function Read-ModernWpfStatusFile([string]$artifactDir) {
    $path = Join-Path $artifactDir "modernwpf-gallery-status.txt"
    if (!(Test-Path $path)) {
        return $null
    }

    try {
        $lines = @(Get-Content -Path $path -ErrorAction Stop)
        if ($lines.Count -lt 2) {
            return $null
        }

        $lastException = ""
        if ($lines.Count -ge 3 -and ![string]::IsNullOrEmpty($lines[2])) {
            try {
                $lastException = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($lines[2]))
            }
            catch {
                $lastException = $lines[2]
            }
        }

        return [ordered]@{
            CurrentRoute = $lines[0]
            ReadyState = $lines[1]
            LastException = $lastException
            Path = $path
        }
    }
    catch {
        return $null
    }
}

function Get-ToggleStateName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        if ($null -ne $pattern) {
            return $pattern.Current.ToggleState.ToString()
        }
    }
    catch {
    }

    return ""
}

function Toggle-Element($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Toggle()
            Start-Sleep -Milliseconds 80
            return $true
        }
    }
    catch {
    }

    return Invoke-ElementOnce $window $element
}

function Set-ToggleElementState($window, $element, [string]$desiredState) {
    if ($null -eq $element) {
        return $false
    }

    for ($attempt = 0; $attempt -lt 3; $attempt++) {
        $state = Get-ToggleStateName $element
        if ($state -eq $desiredState) {
            return $true
        }

        if (!(Toggle-Element $window $element)) {
            return $false
        }
    }

    return (Get-ToggleStateName $element) -eq $desiredState
}

function Reset-ProgressRingAnimationPhase($window, [string]$control) {
    if ($control -ne "ProgressRing") {
        return $false
    }

    $toggle = TryFind-DescendantByAutomationId $window "ProgressToggle"
    if ($null -eq $toggle) {
        return $false
    }

    [void](Set-ToggleElementState $window $toggle "Off")
    Start-Sleep -Milliseconds 150
    $reset = Set-ToggleElementState $window $toggle "On"
    Start-Sleep -Milliseconds 350
    return $reset
}

function Wait-ModernWpfReady($window, [string]$route, [string]$artifactDir) {
    return Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf route '$route' to become ready" -Probe {
        $status = Read-ModernWpfStatusFile $artifactDir
        if ($null -ne $status -and $status.ReadyState -eq "Ready:$route") {
            return $status
        }

        $readyElement = TryFind-DescendantByAutomationId $window "GalleryVisualTestReadyState"
        if ($null -eq $readyElement) {
            return $null
        }

        if ($readyElement.Current.Name -eq "Ready:$route") {
            return $readyElement
        }

        return $null
    }
}

function Wait-WinUIReferenceReady($window, [string]$control) {
    Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "WinUI 3 Gallery test content for '$control' to load" -Probe {
        $loadedElement = Find-DescendantByAutomationId $window "__TestContentLoadedCheckBox"
        if ($null -eq $loadedElement) {
            return $true
        }

        if ((Get-ToggleStateName $loadedElement) -eq "On") {
            return $loadedElement
        }

        return $null
    } | Out-Null

    $idleInvoker = Find-DescendantByAutomationId $window "__WaitForIdleInvoker"
    if ($null -eq $idleInvoker -or !(Invoke-ElementPatternOnce $window $idleInvoker)) {
        return
    }

    Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "WinUI 3 Gallery idle state for '$control'" -Probe {
        $idleElement = Find-DescendantByAutomationId $window "__IdleStateEnteredCheckBox"
        if ($null -eq $idleElement) {
            return $true
        }

        if ((Get-ToggleStateName $idleElement) -eq "On") {
            return $idleElement
        }

        return $null
    } | Out-Null
}

function Get-AutomationText($root, [string]$automationId) {
    $element = TryFind-DescendantByAutomationId $root $automationId
    if ($null -eq $element) {
        return ""
    }

    return $element.Current.Name
}

function Get-RequiredSampleAutomationId([string]$control) {
    switch ($control) {
        "TeachingTip" { return "GallerySample_TeachingTip_ShowButton" }
        "Button" { return "GallerySample_Button_PrimaryButton" }
        "CheckBox" { return "GallerySample_CheckBox_CheckBox" }
        "ComboBox" { return "GallerySample_ComboBox_ComboBox" }
        "ColorPicker" { return "GallerySample_ColorPicker_ColorPicker" }
        "HyperlinkButton" { return "GallerySample_HyperlinkButton_HyperlinkButton" }
        "RatingControl" { return "GallerySample_RatingControl_RatingControl" }
        "RepeatButton" { return "GallerySample_RepeatButton_RepeatButton" }
        "ToggleButton" { return "GallerySample_ToggleButton_ToggleButton" }
        "DropDownButton" { return "GallerySample_DropDownButton_DropDownButton" }
        "SplitButton" { return "GallerySample_SplitButton_SplitButton" }
        "ToggleSplitButton" { return "GallerySample_ToggleSplitButton_ToggleSplitButton" }
        "ToggleSwitch" { return "GallerySample_ToggleSwitch_ToggleSwitch" }
        "NumberBox" { return "GallerySample_NumberBox_SpinButtonNumberBox" }
        "RadioButton" { return "GallerySample_RadioButton_RadioButton" }
        "AutoSuggestBox" { return "GallerySample_AutoSuggestBox_AutoSuggestBox" }
        "Slider" { return "GallerySample_Slider_Slider" }
        "SplitView" { return "GallerySample_SplitView_SplitView" }
        "PersonPicture" { return "GallerySample_PersonPicture_PersonPicture" }
        "ParallaxView" { return "GallerySample_ParallaxView_Root" }
        "IconElement" { return "GallerySample_IconElement_SlicesIcon" }
        "ThemeShadow" { return "GallerySample_ThemeShadow_ShadowRect" }
        "TitleBar" { return "GallerySample_TitleBar_TitleBarControl" }
        "InfoBadge" { return "GallerySample_InfoBadge_InfoBadge" }
        "InfoBar" { return "GallerySample_InfoBar_InfoBar" }
        "ProgressRing" { return "GallerySample_ProgressRing_ProgressRing" }
        "PipsPager" { return "GallerySample_PipsPager_PipsPager" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar" }
        "PullToRefresh" { return "GallerySample_PullToRefresh_RefreshContainer" }
        "GridView" { return "GallerySample_GridView_BasicGridView" }
        "ItemsRepeater" { return "GallerySample_ItemsRepeater_ItemsRepeater" }
        "BreadcrumbBar" { return "GallerySample_BreadcrumbBar_BreadcrumbBar" }
        "Pivot" { return "GallerySample_Pivot_Pivot" }
        "SelectorBar" { return "GallerySample_SelectorBar_SelectorBar" }
        "NavigationView" { return "GallerySample_NavigationView_NavigationView" }
        "ContentDialog" { return "GallerySample_ContentDialog_ShowButton" }
        "Flyout" { return "GallerySample_Flyout_Button" }
        "Popup" { return "GallerySample_Popup_Button" }
        "MenuBar" { return "GallerySample_MenuBar_MenuBar" }
        "MenuFlyout" { return "GallerySample_MenuFlyout_AppBarButton" }
        "SwipeControl" { return "GallerySample_SwipeControl_SwipeControl" }
        "AppBarButton" { return "GallerySample_AppBarButton_AppBarButton" }
        "AppBarSeparator" { return "GallerySample_AppBarSeparator_CommandBar" }
        "AppBarToggleButton" { return "GallerySample_AppBarToggleButton_AppBarToggleButton" }
        "CommandBar" { return "GallerySample_CommandBar_CommandBar" }
        "CommandBarFlyout" { return "GallerySample_CommandBarFlyout_ShowButton" }
        default { return "GallerySample_${control}_Root" }
    }
}

function Get-SampleRootAutomationId([string]$control) {
    return "GallerySample_${control}_Root"
}

function Get-PrimaryCropMinimumVisibleStdDev([string]$control) {
    switch ($control) {
        "NavigationView" { return 45.0 }
        "AutoSuggestBox" { return 1.0 }
        default { return 6.0 }
    }
}

function Get-ModernPrimaryCropAutomationId([string]$control) {
    switch ($control) {
        "InfoBar" { return "GallerySample_InfoBar_InfoBar" }
        "CheckBox" { return "GallerySample_CheckBox_CheckBox" }
        "ColorPicker" { return "GallerySample_ColorPicker_ColorPicker" }
        "HyperlinkButton" { return "GallerySample_HyperlinkButton_HyperlinkButton" }
        "RatingControl" { return "GallerySample_RatingControl_RatingControl" }
        "RepeatButton" { return "GallerySample_RepeatButton_RepeatButton" }
        "ToggleButton" { return "GallerySample_ToggleButton_ToggleButton" }
        "DropDownButton" { return "GallerySample_DropDownButton_DropDownButton" }
        "SplitButton" { return "GallerySample_SplitButton_SplitButton" }
        "ToggleSplitButton" { return "GallerySample_ToggleSplitButton_ToggleSplitButton" }
        "ToggleSwitch" { return "GallerySample_ToggleSwitch_ToggleSwitch" }
        "NumberBox" { return "GallerySample_NumberBox_SpinButtonNumberBox" }
        "RadioButton" { return "GallerySample_RadioButton_RadioButton" }
        "AutoSuggestBox" { return "GallerySample_AutoSuggestBox_AutoSuggestBox" }
        "Slider" { return "GallerySample_Slider_Slider" }
        "SplitView" { return "GallerySample_SplitView_SplitView" }
        "PersonPicture" { return "GallerySample_PersonPicture_PersonPicture" }
        "ParallaxView" { return "GallerySample_ParallaxView_Root" }
        "IconElement" { return "GallerySample_IconElement_Root" }
        "ThemeShadow" { return "GallerySample_ThemeShadow_Root" }
        "TitleBar" { return "GallerySample_TitleBar_TitleBarControl" }
        "InfoBadge" { return "GallerySample_InfoBadge_InfoBadge" }
        "ProgressRing" { return "GallerySample_ProgressRing_ProgressRing" }
        "PipsPager" { return "GallerySample_PipsPager_PipsPager" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_Root" }
        "PullToRefresh" { return "GallerySample_PullToRefresh_Root" }
        "GridView" { return "GallerySample_GridView_BasicGridView" }
        "ItemsRepeater" { return "GallerySample_ItemsRepeater_ItemsRepeater" }
        "BreadcrumbBar" { return "GallerySample_BreadcrumbBar_BreadcrumbBar" }
        "Pivot" { return "GallerySample_Pivot_Pivot" }
        "SelectorBar" { return "GallerySample_SelectorBar_SelectorBar" }
        "Flyout" { return "GallerySample_Flyout_Button" }
        "Popup" { return "GallerySample_Popup_Button" }
        "MenuBar" { return "GallerySample_MenuBar_MenuBar" }
        "MenuFlyout" { return "GallerySample_MenuFlyout_AppBarButton" }
        "SwipeControl" { return "GallerySample_SwipeControl_SwipeControl" }
        "AppBarButton" { return "GallerySample_AppBarButton_AppBarButton" }
        "AppBarSeparator" { return "GallerySample_AppBarSeparator_CommandBar" }
        "AppBarToggleButton" { return "GallerySample_AppBarToggleButton_AppBarToggleButton" }
        "CommandBar" { return "GallerySample_CommandBar_CommandBar" }
        "CommandBarFlyout" { return "GallerySample_CommandBarFlyout_ShowButton" }
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
        "Flyout" { return "Control1" }
        "MenuBar" { return "Example1" }
        "AppBarButton" { return "Button1" }
        "AppBarSeparator" { return "Control1" }
        "AppBarToggleButton" { return "Button1" }
        "CommandBar" { return "PrimaryCommandBar" }
        "CommandBarFlyout" { return "myImageButton" }
        "HyperlinkButton" { return "Control1" }
        "RatingControl" { return "RatingControl1" }
        "ToggleButton" { return "Toggle1" }
        "SplitButton" { return "myColorButton" }
        "ToggleSplitButton" { return "myListButton" }
        "NumberBox" { return "NumberBoxSpinButtonPlacementExample" }
        "AutoSuggestBox" { return "Control1" }
        "SplitView" { return "NavLinksList" }
        "PersonPicture" { return "ProfileImageRadio" }
        "ParallaxView" { return "listView" }
        "IconElement" { return "svPanel" }
        "ThemeShadow" { return "svPanel" }
        "TitleBar" { return "TitleBarControl" }
        "ProgressRing" { return "ProgressRing1" }
        "PipsPager" { return "FlipViewPipsPager" }
        "AnnotatedScrollBar" { return "svPanel" }
        "GridView" { return "BasicGridView" }
        "BreadcrumbBar" { return "BreadcrumbBar1" }
        "SelectorBar" { return "PART_ItemsView" }
        default { return "" }
    }
}

function Get-ReferencePrimaryName([string]$control) {
    switch ($control) {
        "CheckBox" { return "Two-state CheckBox" }
        "DropDownButton" { return "Email" }
        "MenuFlyout" { return "Sort" }
        "Popup" { return "Show Popup (using Offset)" }
        "Pivot" { return "EMAIL" }
        "RepeatButton" { return "Click and hold" }
        "ToggleSwitch" { return "simple ToggleSwitch" }
        default { return "" }
    }
}

function Test-ControlSupportsOpenInteraction([string]$control) {
    switch ($control) {
        "TeachingTip" { return $true }
        "ComboBox" { return $true }
        "ContentDialog" { return $true }
        "Flyout" { return $true }
        "Popup" { return $true }
        "MenuBar" { return $true }
        "MenuFlyout" { return $true }
        "DropDownButton" { return $true }
        "SplitButton" { return $true }
        "ToggleSplitButton" { return $true }
        "CommandBarFlyout" { return $true }
        default { return $false }
    }
}

function Get-OpenInteractionNames([string]$control) {
    switch ($control) {
        "TeachingTip" { return @("This is the title", "Try compact mode", "And this is the subtitle") }
        "ComboBox" { return @("Blue", "Green", "Red", "Yellow") }
        "ContentDialog" { return @("Save your work?", "Upload your content to the cloud.", "Save", "Don't Save", "Cancel") }
        "Flyout" { return @("All items will be removed. Do you want to continue?", "Yes, empty my cart") }
        "Popup" { return @("Simple Popup", "Close") }
        "MenuBar" { return @("New", "Open...", "Save", "Exit") }
        "MenuFlyout" { return @("By rating", "By match", "By distance") }
        "DropDownButton" { return @("Send", "Reply", "Reply All") }
        "SplitButton" { return @("Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray") }
        "ToggleSplitButton" { return @("Bulleted list", "Roman numerals list") }
        "CommandBarFlyout" { return @("Share", "Save", "Delete", "Resize", "Move") }
        default { return @() }
    }
}

function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement) {
    switch ($control) {
        "MenuBar" {
            $trigger = Find-DescendantByAnyName $sampleElement @("File")
            if ($null -eq $trigger) {
                $trigger = Find-ElementByNameInProcess $window.Current.ProcessId @("File")
            }

            if ($null -ne $trigger) {
                $button = Find-DescendantButtonByAnyName $trigger @("File")
                if ($null -ne $button) {
                    return $button
                }

                return Find-OpenInteractionTriggerTarget $trigger
            }
        }
    }

    return $sampleElement
}

function Find-OpenInteractionTriggerTarget($element) {
    $candidate = $element
    for ($depth = 0; $depth -lt 8 -and $null -ne $candidate; $depth++) {
        try {
            if ($candidate.Current.ControlType -eq [System.Windows.Automation.ControlType]::MenuItem -or
                $candidate.Current.ControlType -eq [System.Windows.Automation.ControlType]::Button) {
                return $candidate
            }
        }
        catch {
        }

        try {
            [void]$candidate.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            return $candidate
        }
        catch {
        }

        try {
            [void]$candidate.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            return $candidate
        }
        catch {
        }

        try {
            $candidate = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)
        }
        catch {
            return $element
        }
    }

    return $element
}

function Test-ControlSupportsStateInteraction([string]$control) {
    switch ($control) {
        "CheckBox" { return $true }
        "ToggleButton" { return $true }
        "ToggleSwitch" { return $true }
        "AppBarToggleButton" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsSelectionInteraction([string]$control) {
    switch ($control) {
        "GridView" { return $true }
        "PipsPager" { return $true }
        "Pivot" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsTextInteraction([string]$control) {
    switch ($control) {
        "AutoSuggestBox" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsValueInteraction([string]$control) {
    switch ($control) {
        "RatingControl" { return $true }
        "Slider" { return $true }
        "NumberBox" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsOutputInteraction([string]$control) {
    switch ($control) {
        "RepeatButton" { return $true }
        default { return $false }
    }
}

function Get-OutputInteractionTriggerNames([string]$control) {
    switch ($control) {
        "RepeatButton" { return @("Click and hold") }
        default { return @() }
    }
}

function Get-OutputInteractionCropAutomationId([string]$control) {
    switch ($control) {
        "RepeatButton" { return "GallerySample_RepeatButton_Root" }
        default { return "" }
    }
}

function Get-OutputInteractionMinimumDelta([string]$control) {
    switch ($control) {
        "RepeatButton" { return 0.5 }
        default { return 0.5 }
    }
}

function Get-ValueInteractionStep([string]$control) {
    switch ($control) {
        "RatingControl" { return 3.0 }
        "Slider" { return 50.0 }
        "NumberBox" { return 10.0 }
        default { return 0.0 }
    }
}

function Get-ValueInteractionTargetValue([string]$control, $baselineValue) {
    switch ($control) {
        "RatingControl" { return 3.0 }
        "Slider" { return 50.0 }
        default {
            if ($null -eq $baselineValue) {
                return $null
            }

            return [double]$baselineValue + [double](Get-ValueInteractionStep $control)
        }
    }
}

function Get-ValueInteractionCropAutomationId([string]$control) {
    switch ($control) {
        "Slider" { return "GallerySample_Slider_Root" }
        default { return "" }
    }
}

function Get-ValueInteractionIncreaseButtonNames([string]$control) {
    switch ($control) {
        "NumberBox" { return @("Increase", "Increase value", "Up") }
        default { return @() }
    }
}

function Get-TextInteractionInput([string]$control) {
    switch ($control) {
        "AutoSuggestBox" { return "ae" }
        default { return "" }
    }
}

function Get-TextInteractionSuggestionNames([string]$control) {
    switch ($control) {
        "AutoSuggestBox" { return @("Aegean") }
        default { return @() }
    }
}

function Get-TextInteractionExpectedOutputName([string]$control) {
    switch ($control) {
        "AutoSuggestBox" { return "Aegean" }
        default { return "" }
    }
}

function Get-SelectionInteractionTriggerName([string]$control) {
    switch ($control) {
        "GridView" { return "Item 1" }
        "PipsPager" { return "Page 2" }
        "Pivot" { return "Unread" }
        default { return "" }
    }
}

function Get-SelectionInteractionExpectedName([string]$control) {
    switch ($control) {
        "GridView" { return "You clicked Item 1." }
        "Pivot" { return "unread emails go here." }
        default { return "" }
    }
}

function Get-SelectionInteractionCropAutomationId([string]$control) {
    switch ($control) {
        "GridView" { return "GallerySample_GridView_ClickOutput0" }
        "PipsPager" { return "GallerySample_PipsPager_Root" }
        "Pivot" { return "GallerySample_Pivot_Pivot" }
        default { return "" }
    }
}

function Find-ReferenceInteractionTrigger($window, [string]$control) {
    if ($control -eq "TeachingTip") {
        return Find-DescendantButtonByName $window "Show TeachingTip"
    }

    $automationId = Get-ReferencePrimaryAutomationId $control
    if (![string]::IsNullOrEmpty($automationId)) {
        return Find-DescendantByAutomationId $window $automationId
    }

    $name = Get-ReferencePrimaryName $control
    if (![string]::IsNullOrEmpty($name)) {
        return Find-ReferencePrimaryByName $window $control $name
    }

    return $null
}

function Get-WinUIReferencePageTitle([string]$control) {
    switch ($control) {
        default { return $control }
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

    function Get-ControlTypeName($node) {
        try {
            $controlType = $node.Current.ControlType
            if ($null -ne $controlType -and $null -ne $controlType.ProgrammaticName) {
                return $controlType.ProgrammaticName
            }
        }
        catch {
        }

        return ""
    }

    function Append-Element($node, [int]$depth) {
        if ($null -eq $node -or $depth -gt $maxDepth) {
            return
        }

        $indent = "  " * $depth
        $rect = $node.Current.BoundingRectangle
        $line = "{0}{1} name='{2}' id='{3}' rect='{4},{5},{6},{7}'" -f `
            $indent,
            (Get-ControlTypeName $node),
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

function Get-ImageVisibleStdDev([string]$path, [int]$step = 3) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        $sum = 0.0
        $sumSquared = 0.0
        $samples = 0
        for ($y = 0; $y -lt $bitmap.Height; $y += $step) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $step) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -le 16) {
                    continue
                }

                $luminance = (0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B)
                $sum += $luminance
                $sumSquared += $luminance * $luminance
                $samples++
            }
        }

        if ($samples -eq 0) {
            return 0.0
        }

        $mean = $sum / $samples
        $variance = ($sumSquared / $samples) - ($mean * $mean)
        return [Math]::Round([Math]::Sqrt([Math]::Max(0.0, $variance)), 2)
    }
    finally {
        $bitmap.Dispose()
    }
}

function Get-ImageMeanLuminance([string]$path, [int]$step = 3) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        $sum = 0.0
        $samples = 0
        for ($y = 0; $y -lt $bitmap.Height; $y += $step) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $step) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -le 16) {
                    continue
                }

                $sum += (0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B)
                $samples++
            }
        }

        if ($samples -eq 0) {
            return $null
        }

        return [Math]::Round($sum / $samples, 2)
    }
    finally {
        $bitmap.Dispose()
    }
}

function Capture-Window([IntPtr]$hwnd, [string]$path, [switch]$SkipActivate) {
    if (!$SkipActivate) {
        [GalleryVisualNative]::Activate($hwnd)
        Start-Sleep -Milliseconds 300
    }
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

        try {
            Capture-ScreenRect $hwnd $path
            if (Test-ImageNotBlank $path) {
                return
            }
        }
        catch {
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
        try {
            $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
        }
        catch {
            $copyFromScreenError = $_.Exception.Message
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $hdc = $graphics.GetHdc()
            $copied = $false
            try {
                $copied = [GalleryVisualNative]::CopyScreenSurface($hdc, $rect.Left, $rect.Top, $width, $height)
            }
            finally {
                $graphics.ReleaseHdc($hdc)
            }

            if (!$copied) {
                throw "CopyFromScreen failed and native screen capture fallback failed: $copyFromScreenError"
            }
        }
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
        VisibleStdDev = Get-ImageVisibleStdDev $path
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
        VisibleStdDev = Get-ImageVisibleStdDev $path
    }
}

function Get-ModernRenderedElementArtifactPath([string]$caseDir, $element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $automationId = $element.Current.AutomationId
        if ([string]::IsNullOrWhiteSpace($automationId)) {
            return ""
        }

        $path = Join-Path $caseDir ("modernwpf-artifacts\{0}.png" -f $automationId)
        if (Test-Path $path) {
            return $path
        }
    }
    catch {
    }

    return ""
}

function Copy-RenderedArtifactCrop([string]$sourcePath, [string]$destinationPath, [string]$source) {
    if ([string]::IsNullOrWhiteSpace($sourcePath) -or !(Test-Path $sourcePath)) {
        return $null
    }

    Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
    return New-RenderedArtifactCrop $destinationPath $source $null
}

function Refresh-ModernWpfVisualArtifacts($window) {
    $refreshButton = TryFind-DescendantByAutomationId $window "GalleryVisualTestRefreshArtifacts"
    if ($null -eq $refreshButton) {
        return $false
    }

    if (Invoke-ElementPatternOnce $window $refreshButton) {
        Start-Sleep -Milliseconds 150
        return $true
    }

    return $false
}

function New-RenderedArtifactSliceCrop([string]$sourcePath, [string]$path, [string]$source, [int]$width, [int]$height) {
    if (!(Test-Path $sourcePath) -or $width -le 0 -or $height -le 0) {
        return $null
    }

    $sourceSize = Get-ImageSize $sourcePath
    $bounds = [ordered]@{
        Found = $true
        Reason = ""
        X = 0
        Y = 0
        Width = [Math]::Min($width, $sourceSize.Width)
        Height = [Math]::Min($height, $sourceSize.Height)
        ChangedSamples = 0
    }
    Save-Crop $sourcePath $bounds $path 0 | Out-Null
    return New-RenderedArtifactCrop $path $source $bounds
}

function New-ProgressRingModernPrimaryCrop([string]$caseDir, [int]$width, [int]$height) {
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $sampleArtifact = Join-Path $artifactDir "GallerySample_ProgressRing_Root.png"
    if (!(Test-Path $sampleArtifact) -or $width -le 0 -or $height -le 0) {
        return $null
    }

    $sampleSize = Get-ImageSize $sampleArtifact
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped ProgressRing from the rendered sample root because the control-only VisualBrush crop misses the arc."
        X = [Math]::Min(10, [Math]::Max(0, $sampleSize.Width - $width))
        Y = [Math]::Min(6, [Math]::Max(0, $sampleSize.Height - $height))
        Width = [Math]::Min($width, $sampleSize.Width)
        Height = [Math]::Min($height, $sampleSize.Height)
        ChangedSamples = 0
    }
    $path = Join-Path $artifactDir "GallerySample_ProgressRing_ProgressRing_fromRoot.png"
    $savedBounds = Save-Crop $sampleArtifact $bounds $path 0
    return New-RenderedArtifactCrop $path "GallerySample_ProgressRing_ProgressRing" $savedBounds
}

function New-TitleBarModernPrimaryCrop([string]$caseDir) {
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $sampleArtifact = Join-Path $artifactDir "GallerySample_TitleBar_Root.png"
    if (!(Test-Path $sampleArtifact)) {
        return $null
    }

    $sampleSize = Get-ImageSize $sampleArtifact
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the ModernWpf TitleBarControl from the rendered sample root because the control-only VisualBrush crop is blank."
        X = 0
        Y = [Math]::Max(0, [int][Math]::Round(($sampleSize.Height - 48) / 2.0))
        Width = [Math]::Min(470, $sampleSize.Width)
        Height = [Math]::Min(48, $sampleSize.Height)
        ChangedSamples = 0
    }

    $path = Join-Path $artifactDir "GallerySample_TitleBar_TitleBarControl_fromRoot.png"
    $savedBounds = Save-Crop $sampleArtifact $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "GallerySample_TitleBar_TitleBarControl" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function New-AnnotatedScrollBarReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernSampleArtifact = Join-Path $modernArtifactDir "GallerySample_AnnotatedScrollBar_Root.png"
    if (!(Test-Path $modernSampleArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernSampleArtifact
    $scrollPresenter = Find-DescendantByAutomationId $window "PART_ScrollPresenter"
    $scrollBounds = Get-ElementWindowBounds $window $scrollPresenter
    $sampleBounds = Get-ElementWindowBounds $window $sampleElement
    if ($null -eq $scrollBounds -or $null -eq $sampleBounds) {
        return $null
    }

    $x = [Math]::Max(0, $scrollBounds.X - 13)
    $y = [Math]::Max(0, $scrollBounds.Y - 12)
    $right = $sampleBounds.X + $sampleBounds.Width
    $bottom = $sampleBounds.Y + $sampleBounds.Height
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the WinUI AnnotatedScrollBar example content to match the ModernWpf rendered sample root."
        X = $x
        Y = $y
        Width = [Math]::Max(1, [Math]::Min($modernSize.Width, $right - $x))
        Height = [Math]::Max(1, [Math]::Min($modernSize.Height, $bottom - $y))
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-AnnotatedScrollBar-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "svPanel content" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function New-IconElementReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernSampleArtifact = Join-Path $modernArtifactDir "GallerySample_IconElement_Root.png"
    if (!(Test-Path $modernSampleArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernSampleArtifact
    $bodyText = Find-DescendantByName $window "The ShowAsMonochrome property (true by default) will result in a solid block of the foreground color if the property is set to true and the icon is more than one color. This behavior can be ignored by setting the ShowAsMonochrome property to false."
    $bodyBounds = Get-ElementWindowBounds $window $bodyText
    if ($null -eq $bodyBounds) {
        return $null
    }

    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the WinUI IconElement first example content to match the ModernWpf rendered sample root."
        X = $bodyBounds.X
        Y = $bodyBounds.Y
        Width = $modernSize.Width
        Height = $modernSize.Height
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-IconElement-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "Example1 content" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function Capture-StaticCrops([string]$app, [string]$control, [string]$caseDir, $window, [string]$screenshot) {
    $primaryElement = $null
    $primarySource = ""
    $sampleElement = $null
    $sampleSource = ""

    if ($app -eq "ModernWpf") {
        $primarySource = Get-ModernPrimaryCropAutomationId $control
        $sampleSource = Get-SampleRootAutomationId $control

        $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
        $primaryArtifact = Join-Path $artifactDir ($primarySource + ".png")
        $sampleArtifact = Join-Path $artifactDir ($sampleSource + ".png")
        $primaryCrop = New-RenderedArtifactCrop $primaryArtifact $primarySource $null
        $sampleCrop = New-RenderedArtifactCrop $sampleArtifact $sampleSource $null
        $menuBarArtifactCrop = $null

        if ($control -eq "MenuBar") {
            $menuBarArtifactCrop = $primaryCrop
            $primaryCrop = $null
        }

        if ($control -eq "InfoBar" -and $null -ne $primaryCrop -and !$primaryCrop.NonBlank) {
            $rootSlicePath = Join-Path $artifactDir ($primarySource + "_fromRoot.png")
            $rootSlice = New-RenderedArtifactSliceCrop $sampleArtifact $rootSlicePath $primarySource $primaryCrop.Width $primaryCrop.Height
            if ($null -ne $rootSlice -and $rootSlice.NonBlank) {
                $primaryCrop = $rootSlice
            }
            else {
                $primaryCrop = $null
            }
        }

        if ($control -eq "ProgressRing" -and $null -ne $primaryCrop) {
            $progressRingPrimary = New-ProgressRingModernPrimaryCrop $caseDir $primaryCrop.Width $primaryCrop.Height
            if ($null -ne $progressRingPrimary -and $progressRingPrimary.NonBlank) {
                $primaryCrop = $progressRingPrimary
            }
        }

        if ($control -eq "PipsPager" -and $null -ne $primaryCrop -and !$primaryCrop.NonBlank -and (Test-Path $sampleArtifact)) {
            $sampleSize = Get-ImageSize $sampleArtifact
            $fallbackBounds = [ordered]@{
                Found = $true
                Reason = "Cropped PipsPager from the rendered sample root because the control-only VisualBrush crop is blank."
                X = [Math]::Max(0, [int](($sampleSize.Width - $primaryCrop.Width) / 2))
                Y = [Math]::Max(0, $sampleSize.Height - $primaryCrop.Height)
                Width = $primaryCrop.Width
                Height = $primaryCrop.Height
                ChangedSamples = 0
            }
            $fallbackPath = Join-Path $artifactDir ($primarySource + "_fromRoot.png")
            $savedBounds = Save-Crop $sampleArtifact $fallbackBounds $fallbackPath 0
            $fallbackCrop = New-RenderedArtifactCrop $fallbackPath $primarySource $savedBounds
            if ($null -ne $fallbackCrop -and $fallbackCrop.NonBlank) {
                $primaryCrop = $fallbackCrop
            }
            else {
                $primaryCrop = $null
            }
        }

        if ($control -eq "ItemsRepeater" -and $null -ne $primaryCrop -and !$primaryCrop.NonBlank -and $primaryCrop.VisibleStdDev -ge (Get-PrimaryCropMinimumVisibleStdDev $control)) {
            $primaryCrop["NonBlank"] = $true
        }

        if ($control -eq "BreadcrumbBar") {
            if ($null -ne $primaryCrop -and !$primaryCrop.NonBlank) {
                $primaryCrop = $null
            }

            if ($null -ne $sampleCrop -and !$sampleCrop.NonBlank) {
                $sampleCrop = $null
            }
        }

        if ($control -eq "TitleBar" -and $null -ne $primaryCrop -and !$primaryCrop.NonBlank) {
            $titleBarPrimary = New-TitleBarModernPrimaryCrop $caseDir
            if ($null -ne $titleBarPrimary) {
                $primaryCrop = $titleBarPrimary
            }
            else {
                $primaryCrop = $null
            }
        }

        if ($null -eq $primaryCrop) {
            $primaryElement = TryFind-DescendantByAutomationId $window $primarySource
        }

        if ($null -eq $sampleCrop) {
            $sampleElement = TryFind-DescendantByAutomationId $window $sampleSource
        }
    }
    else {
        $primarySource = Get-ReferencePrimaryAutomationId $control
        if (![string]::IsNullOrEmpty($primarySource)) {
            $primaryElement = Find-DescendantByAutomationId $window $primarySource
        }
        else {
            $primarySource = Get-ReferencePrimaryName $control
            if (![string]::IsNullOrEmpty($primarySource)) {
                $primaryElement = Find-ReferencePrimaryByName $window $control $primarySource
            }
        }
        $sampleSource = "svPanel"
        $sampleElement = Find-DescendantByAutomationId $window $sampleSource
    }

    $primaryPath = Join-Path $caseDir ("{0}-{1}-primary-crop.png" -f $app.ToLowerInvariant(), $control)
    $samplePath = Join-Path $caseDir ("{0}-{1}-sample-crop.png" -f $app.ToLowerInvariant(), $control)
    $primaryBounds = Get-ElementWindowBounds $window $primaryElement
    $sampleBounds = Get-ElementWindowBounds $window $sampleElement

    if ($app -eq "ModernWpf") {
        if ($null -ne $primaryCrop -and $null -ne $primaryBounds) {
            $primaryCrop["Bounds"] = $primaryBounds
        }

        if ($null -ne $sampleCrop -and $null -ne $sampleBounds) {
            $sampleCrop["Bounds"] = $sampleBounds
        }

        $primaryResult = if ($null -ne $primaryCrop) { $primaryCrop } else { Save-ElementCrop $window $screenshot $primaryPath $primaryElement $primarySource 0 }
        if ($control -eq "MenuBar" -and $primaryResult.Found -and !$primaryResult.NonBlank -and $null -ne $menuBarArtifactCrop -and $menuBarArtifactCrop.NonBlank) {
            $primaryResult = $menuBarArtifactCrop
        }
        if ($control -eq "InfoBar" -and $primaryResult.Found -and !$primaryResult.NonBlank -and $null -ne $primaryBounds -and (Test-Path $screenshot)) {
            $fallbackBounds = [ordered]@{
                Found = $true
                Reason = "Adjusted below blank InfoBar automation bounds."
                X = $primaryBounds.X
                Y = $primaryBounds.Y + $primaryBounds.Height + 6
                Width = $primaryBounds.Width
                Height = $primaryBounds.Height
                ChangedSamples = $primaryBounds.ChangedSamples
            }
            $fallbackPath = Join-Path $caseDir ("modernwpf-{0}-primary-visible-crop.png" -f $control)
            $savedBounds = Save-Crop $screenshot $fallbackBounds $fallbackPath 0
            $fallbackResult = [ordered]@{
                Found = $true
                Source = $primarySource
                Screenshot = $fallbackPath
                Bounds = $savedBounds
                Width = $savedBounds.Width
                Height = $savedBounds.Height
                NonBlank = Test-ImageNotBlank $fallbackPath
                VisibleStdDev = Get-ImageVisibleStdDev $fallbackPath
            }
            if ($fallbackResult.NonBlank) {
                $primaryResult = $fallbackResult
            }
        }

        return [ordered]@{
            Primary = $primaryResult
            Sample = $(if ($null -ne $sampleCrop) { $sampleCrop } else { Save-ElementCrop $window $screenshot $samplePath $sampleElement $sampleSource 10 })
        }
    }

    $primaryResult = Save-ElementCrop $window $screenshot $primaryPath $primaryElement $primarySource 0
    if ($control -eq "AnnotatedScrollBar") {
        $annotatedScrollBarPrimary = New-AnnotatedScrollBarReferencePrimaryCrop $caseDir $window $screenshot $sampleElement
        if ($null -ne $annotatedScrollBarPrimary) {
            $primaryResult = $annotatedScrollBarPrimary
        }
    }
    elseif ($control -eq "IconElement") {
        $iconElementPrimary = New-IconElementReferencePrimaryCrop $caseDir $window $screenshot
        if ($null -ne $iconElementPrimary) {
            $primaryResult = $iconElementPrimary
        }
    }

    return [ordered]@{
        Primary = $primaryResult
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

function Invoke-ElementOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        $keptTopMost = $false
        try {
            [GalleryVisualNative]::SetTopMost($window.Current.NativeWindowHandle, $true)
            $keptTopMost = $true
            [GalleryVisualNative]::Click(
                [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
                [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
            Start-Sleep -Milliseconds 50
            return $true
        }
        finally {
            if ($keptTopMost) {
                [GalleryVisualNative]::SetTopMost($window.Current.NativeWindowHandle, $false)
            }
        }
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            Start-Sleep -Milliseconds 50
            return $true
        }
    }
    catch {
    }

    return $false
}

function Invoke-ElementPatternOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            Start-Sleep -Milliseconds 50
            return $true
        }
    }
    catch {
    }

    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        [GalleryVisualNative]::Click(
            [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
            [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
        Start-Sleep -Milliseconds 50
        return $true
    }

    return $false
}

function Test-ElementSupportsPattern($element, $pattern) {
    if ($null -eq $element) {
        return $false
    }

    try {
        return $null -ne $element.GetCurrentPattern($pattern)
    }
    catch {
        return $false
    }
}

function Find-SelectionInvokeTarget($element) {
    $candidate = $element
    for ($depth = 0; $depth -lt 8 -and $null -ne $candidate; $depth++) {
        if ((Test-ElementSupportsPattern $candidate ([System.Windows.Automation.SelectionItemPattern]::Pattern)) -or
            (Test-ElementSupportsPattern $candidate ([System.Windows.Automation.InvokePattern]::Pattern))) {
            return $candidate
        }

        try {
            $candidate = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)
        }
        catch {
            return $null
        }
    }

    return $element
}

function Invoke-SelectionElementOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $target = Find-SelectionInvokeTarget $element

    try {
        $pattern = $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Select()
            Start-Sleep -Milliseconds 80
            [void](Invoke-ElementOnce $window $target)
            return $true
        }
    }
    catch {
    }

    try {
        $pattern = $target.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            Start-Sleep -Milliseconds 80
            return $true
        }
    }
    catch {
    }

    return Invoke-ElementOnce $window $target
}

function Invoke-GridViewItemClickOnce([string]$app, $window) {
    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)

    $item = Find-ElementByNameInProcess $window.Current.ProcessId @("Item 1")
    if ($null -eq $item) {
        return $false
    }

    $target = Find-SelectionInvokeTarget $item
    if ($null -eq $target) {
        return $false
    }

    $invoked = $false
    try {
        $pattern = $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Select()
            $invoked = $true
            Start-Sleep -Milliseconds 80
        }
    }
    catch {
    }

    try {
        $pattern = $target.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            $invoked = $true
            Start-Sleep -Milliseconds 80
        }
    }
    catch {
    }

    return $invoked
}

function Expand-ElementPatternOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Expand()
            Start-Sleep -Milliseconds 50
            return $true
        }
    }
    catch {
    }

    return $false
}

function Get-ExpandCollapseStateName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        if ($null -ne $pattern) {
            return $pattern.Current.ExpandCollapseState.ToString()
        }
    }
    catch {
    }

    return ""
}

function Find-ComboBoxOpenElement($window, $element, [string[]]$openNames) {
    if ($null -eq $element -or (Get-ExpandCollapseStateName $element) -ne "Expanded") {
        return $null
    }

    $comboRect = $element.Current.BoundingRectangle
    if ($comboRect.Width -le 0 -or $comboRect.Height -le 0) {
        return $null
    }

    $matches = Find-ElementsByNameInProcess $window.Current.ProcessId $openNames
    foreach ($match in $matches) {
        try {
            if ($match.Current.ControlType -ne [System.Windows.Automation.ControlType]::ListItem) {
                continue
            }

            $rect = $match.Current.BoundingRectangle
            if ($rect.Width -le 0 -or $rect.Height -le 0) {
                continue
            }

            $overlapsHorizontally = $rect.Right -gt $comboRect.X -and $rect.X -lt $comboRect.Right
            $outsideClosedCombo = $rect.Y -ge ($comboRect.Bottom - 1) -or $rect.Bottom -le ($comboRect.Y + 1)
            if ($overlapsHorizontally -and $outsideClosedCombo) {
                return $match
            }
        }
        catch {
        }
    }

    return $null
}

function Find-OpenInteractionElement($window, $element, [string[]]$openNames, [string]$control) {
    if ($control -eq "ComboBox") {
        return Find-ComboBoxOpenElement $window $element $openNames
    }

    if ($control -eq "SplitButton" -or $control -eq "ToggleSplitButton") {
        return Find-ElementByNameInPopupWindows $window $openNames
    }

    return Find-ElementByNameInProcess $window.Current.ProcessId $openNames
}

function Test-ControlPrefersScreenOpenCapture([string]$control) {
    switch ($control) {
        "TeachingTip" { return $true }
        "MenuBar" { return $true }
        default { return $false }
    }
}

function Test-ControlRequiresPopupWindowOpenProof([string]$control) {
    switch ($control) {
        "MenuFlyout" { return $true }
        "DropDownButton" { return $true }
        "SplitButton" { return $true }
        "ToggleSplitButton" { return $true }
        default { return $false }
    }
}

function Close-PreparedOpenInteractionState($window, [string]$control) {
    if ($control -ne "TeachingTip") {
        return
    }

    $tip = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "GallerySample_TeachingTip_TeachingTip"
    if ($null -eq $tip) {
        return
    }

    try {
        $pattern = $tip.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Close()
            Start-Sleep -Milliseconds 250
        }
    }
    catch {
    }
}

function Get-ElementNativeWindowHandle($element) {
    $candidate = $element
    for ($depth = 0; $depth -lt 16 -and $null -ne $candidate; $depth++) {
        try {
            $handle = [int]$candidate.Current.NativeWindowHandle
            if ($handle -ne 0) {
                return [IntPtr]$handle
            }

            $candidate = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)
        }
        catch {
            return [IntPtr]::Zero
        }
    }

    return [IntPtr]::Zero
}

function Find-EditableDescendant($element) {
    if ($null -eq $element) {
        return $null
    }

    try {
        if ($element.Current.ControlType -eq [System.Windows.Automation.ControlType]::Edit) {
            return $element
        }
    }
    catch {
    }

    return Find-DescendantByControlType $element ([System.Windows.Automation.ControlType]::Edit)
}

function Set-EditableElementText($window, $element, [string]$text) {
    $edit = Find-EditableDescendant $element
    if ($null -eq $edit) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $edit.SetFocus()
        Start-Sleep -Milliseconds 50
    }
    catch {
    }

    try {
        $pattern = $edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.SetValue("")
            Start-Sleep -Milliseconds 50
            $pattern.SetValue($text)
            Start-Sleep -Milliseconds 250
            return $true
        }
    }
    catch {
    }

    try {
        $rect = $edit.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            [GalleryVisualNative]::Click(
                [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
                [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
            Start-Sleep -Milliseconds 50
            [GalleryVisualNative]::PressCtrlA()
            Start-Sleep -Milliseconds 50
            [GalleryVisualNative]::TypeText($text)
            Start-Sleep -Milliseconds 250
            return $true
        }
    }
    catch {
    }

    return $false
}

function Try-ParseDouble([string]$text) {
    $value = 0.0
    if ([double]::TryParse(
            $text,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$value)) {
        return $value
    }

    if ([double]::TryParse(
            $text,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::CurrentCulture,
            [ref]$value)) {
        return $value
    }

    return $null
}

function Get-ElementNumericValue($element) {
    if ($null -eq $element) {
        return $null
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
        if ($null -ne $pattern) {
            return [double]$pattern.Current.Value
        }
    }
    catch {
    }

    $edit = Find-EditableDescendant $element
    if ($null -eq $edit) {
        return $null
    }

    try {
        $pattern = $edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $pattern) {
            return Try-ParseDouble $pattern.Current.Value
        }
    }
    catch {
    }

    try {
        return Try-ParseDouble $edit.Current.Name
    }
    catch {
        return $null
    }
}

function Test-DoubleApproximatelyEqual($actual, $expected) {
    if ($null -eq $actual -or $null -eq $expected) {
        return $false
    }

    return [Math]::Abs(([double]$actual) - ([double]$expected)) -lt 0.001
}

function Get-ElementCenterHitSummary($element) {
    if ($null -eq $element) {
        return $null
    }

    try {
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return $null
        }

        $x = [double]($rect.X + ($rect.Width / 2.0))
        $y = [double]($rect.Y + ($rect.Height / 2.0))
        $point = New-Object System.Windows.Point -ArgumentList $x, $y
        $hit = [System.Windows.Automation.AutomationElement]::FromPoint($point)
        if ($null -eq $hit) {
            return [ordered]@{
                X = $x
                Y = $y
                Found = $false
                Name = ""
                AutomationId = ""
                ControlType = ""
            }
        }

        return [ordered]@{
            X = $x
            Y = $y
            Found = $true
            Name = $hit.Current.Name
            AutomationId = $hit.Current.AutomationId
            ControlType = $hit.Current.ControlType.ProgrammaticName
        }
    }
    catch {
        return [ordered]@{
            X = 0
            Y = 0
            Found = $false
            Name = ""
            AutomationId = ""
            ControlType = ""
            Error = $_.Exception.Message
        }
    }
}

function Invoke-ValueIncreaseOnce($window, [string]$control, $element, $expectedValue) {
    if ($null -eq $element) {
        return $false
    }

    if ($control -eq "RatingControl" -or $control -eq "Slider") {
        try {
            $rangePattern = $element.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
            if ($null -ne $rangePattern) {
                $rangePattern.SetValue([double]$expectedValue)
                Start-Sleep -Milliseconds 50
                return $true
            }
        }
        catch {
        }

        try {
            $valuePattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
            if ($null -ne $valuePattern) {
                $valuePattern.SetValue(([double]$expectedValue).ToString([System.Globalization.CultureInfo]::InvariantCulture))
                Start-Sleep -Milliseconds 50
                return $true
            }
        }
        catch {
        }
    }

    $buttonNames = Get-ValueInteractionIncreaseButtonNames $control
    $button = Find-DescendantButtonByAnyName $element $buttonNames
    if ($null -eq $button -and $buttonNames.Count -gt 0) {
        $candidate = Find-ElementByNameInProcess $window.Current.ProcessId $buttonNames
        try {
            if ($null -ne $candidate -and $candidate.Current.ControlType -eq [System.Windows.Automation.ControlType]::Button) {
                $button = $candidate
            }
        }
        catch {
        }
    }

    if ($null -ne $button) {
        return Invoke-ElementPatternOnce $window $button
    }

    if ($control -eq "NumberBox") {
        [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            $edit = Find-EditableDescendant $element
            $editRect = if ($null -ne $edit) { $edit.Current.BoundingRectangle } else { $rect }
            $x = [int][Math]::Round($rect.Right - [Math]::Min(46.0, [Math]::Max(18.0, $rect.Width * 0.35)))
            $y = [int][Math]::Round($editRect.Y + ($editRect.Height / 2.0))
            [GalleryVisualNative]::Click($x, $y)
            Start-Sleep -Milliseconds 50
            return $true
        }
    }

    return $false
}

function Find-ListItemOutsideElementBounds($window, $element, [string[]]$names) {
    if ($null -eq $element) {
        return $null
    }

    $anchorRect = $element.Current.BoundingRectangle
    if ($anchorRect.Width -le 0 -or $anchorRect.Height -le 0) {
        return $null
    }

    $matches = Find-ElementsByNameInProcess $window.Current.ProcessId $names
    foreach ($match in $matches) {
        try {
            if ($match.Current.ControlType -ne [System.Windows.Automation.ControlType]::ListItem) {
                continue
            }

            $rect = $match.Current.BoundingRectangle
            if ($rect.Width -le 0 -or $rect.Height -le 0) {
                continue
            }

            $overlapsHorizontally = $rect.Right -gt $anchorRect.X -and $rect.X -lt $anchorRect.Right
            $outsideAnchor = $rect.Y -ge ($anchorRect.Bottom - 1) -or $rect.Bottom -le ($anchorRect.Y + 1)
            if ($overlapsHorizontally -and $outsideAnchor) {
                return $match
            }
        }
        catch {
        }
    }

    return $null
}

function Wait-ForListItemOutsideElementBounds($window, $element, [string[]]$names, [int]$timeoutMs = 2500) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMs)
    do {
        $match = Find-ListItemOutsideElementBounds $window $element $names
        if ($null -ne $match) {
            return $match
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Find-OutputTextOutsideElementBounds($window, $element, [string]$name) {
    if ($null -eq $element -or [string]::IsNullOrEmpty($name)) {
        return $null
    }

    $anchorRect = $element.Current.BoundingRectangle
    $matches = Find-ElementsByNameInProcess $window.Current.ProcessId @($name)
    foreach ($match in $matches) {
        try {
            if ($match.Current.ControlType -ne [System.Windows.Automation.ControlType]::Text) {
                continue
            }

            $rect = $match.Current.BoundingRectangle
            if ($rect.Width -le 0 -or $rect.Height -le 0) {
                continue
            }

            if ($rect.X -ge ($anchorRect.Right - 2) -or $rect.Y -gt ($anchorRect.Bottom + 2)) {
                return $match
            }
        }
        catch {
        }
    }

    return $null
}

function Wait-ForOutputTextOutsideElementBounds($window, $element, [string]$name, [int]$timeoutMs = 2500) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMs)
    do {
        $match = Find-OutputTextOutsideElementBounds $window $element $name
        if ($null -ne $match) {
            return $match
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Capture-OpenInteractionFrame($window, [string]$path, [bool]$preferScreenCapture, [switch]$SkipActivate) {
    if (!$SkipActivate) {
        [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
        Start-Sleep -Milliseconds 100
    }

    if ($preferScreenCapture) {
        try {
            Capture-ScreenRect $window.Current.NativeWindowHandle $path
            return ""
        }
        catch {
            $screenError = $_.Exception.Message
            try {
                Capture-Window $window.Current.NativeWindowHandle $path -SkipActivate:$SkipActivate
                return ""
            }
            catch {
                return $screenError + "; fallback Capture-Window failed: " + $_.Exception.Message
            }
        }
    }

    try {
        Capture-Window $window.Current.NativeWindowHandle $path -SkipActivate:$SkipActivate
        return ""
    }
    catch {
        $windowError = $_.Exception.Message
        try {
            Capture-ScreenRect $window.Current.NativeWindowHandle $path
            return ""
        }
        catch {
            return $windowError + "; fallback Capture-ScreenRect failed: " + $_.Exception.Message
        }
    }
}

function Invoke-SplitButtonSecondaryOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -le 0 -or $rect.Height -le 0) {
        return $false
    }

    $x = [int][Math]::Round($rect.Right - [Math]::Min(12.0, [Math]::Max(6.0, $rect.Width * 0.18)))
    $y = [int][Math]::Round($rect.Y + ($rect.Height / 2.0))
    [GalleryVisualNative]::Click($x, $y)
    Start-Sleep -Milliseconds 80
    return $true
}

function Invoke-ElementUntilOpen($window, $element, [string[]]$openNames, [string]$control = "") {
    if ($control -eq "SplitButton" -or $control -eq "ToggleSplitButton") {
        $invoked = Invoke-SplitButtonSecondaryOnce $window $element
        Start-Sleep -Milliseconds 150
        if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
            return $invoked
        }

        $invoked = (Expand-ElementPatternOnce $window $element) -or $invoked
        Start-Sleep -Milliseconds 150
        return $invoked
    }

    if ($control -eq "MenuBar") {
        $invoked = Invoke-MenuBarTriggerOnce $window $element
        Start-Sleep -Milliseconds 200
        if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
            return $invoked
        }

        try {
            $element.SetFocus()
            [GalleryVisualNative]::PressSpace()
            $invoked = $true
            Start-Sleep -Milliseconds 200
        }
        catch {
        }

        return $invoked
    }

    if ($control -eq "ComboBox") {
        $invoked = Expand-ElementPatternOnce $window $element
        Start-Sleep -Milliseconds 150
        if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
            return $invoked
        }

        try {
            $element.SetFocus()
            [GalleryVisualNative]::PressSpace()
            $invoked = $true
            Start-Sleep -Milliseconds 150
            if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
                return $invoked
            }
        }
        catch {
        }

        $invoked = (Invoke-ElementPatternOnce $window $element) -or $invoked
        Start-Sleep -Milliseconds 150
        return $invoked
    }

    $invoked = Invoke-ElementOnce $window $element
    Start-Sleep -Milliseconds 150
    if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
        return $invoked
    }

    $invoked = (Expand-ElementPatternOnce $window $element) -or $invoked
    Start-Sleep -Milliseconds 150
    if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
        return $invoked
    }

    $invoked = (Invoke-ElementPatternOnce $window $element) -or $invoked
    Start-Sleep -Milliseconds 150
    if ($null -ne (Find-OpenInteractionElement $window $element $openNames $control)) {
        return $invoked
    }

    try {
        $element.SetFocus()
        [GalleryVisualNative]::PressSpace()
        $invoked = $true
        Start-Sleep -Milliseconds 150
    }
    catch {
    }

    return $invoked
}

function Invoke-MenuBarTriggerOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $clicked = $false
    try {
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            [GalleryVisualNative]::Click(
                [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
                [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
            Start-Sleep -Milliseconds 100
            $clicked = $true
        }
    }
    catch {
    }

    if (Invoke-ElementPatternOnce $window $element) {
        return $true
    }

    return $clicked
}

function Capture-OpenInteraction([string]$app, [string]$control, [string]$caseDir, $window, $showButton, [string[]]$openNames) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsOpenInteraction $control)) {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250
    $baselinePath = Join-Path $caseDir ("{0}-{1}-closed.png" -f $app.ToLowerInvariant(), $control)
    $preferScreenOpenCapture = Test-ControlPrefersScreenOpenCapture $control
    if ($preferScreenOpenCapture) {
        [GalleryVisualNative]::SetTopMost($window.Current.NativeWindowHandle, $true)
        Start-Sleep -Milliseconds 100
    }
    $triggerElement = Get-OpenInteractionTriggerElement $window $control $showButton
    $triggerTreePath = Join-Path $caseDir ("{0}-{1}-open-trigger.uia.txt" -f $app.ToLowerInvariant(), $control)
    try {
        Write-UiaTree $triggerElement $triggerTreePath 5
    }
    catch {
        Set-Content -Path $triggerTreePath -Value ("Open trigger UIA tree capture failed: " + $_.Exception.Message) -Encoding UTF8
    }
    Close-PreparedOpenInteractionState $window $control
    $screenCaptureTrustReference = ""
    $screenCaptureTrustDelta = $null
    $screenCaptureTrusted = $true
    if ($preferScreenOpenCapture) {
        $screenCaptureTrustReference = Join-Path $caseDir ("{0}-{1}-screen-trust-reference.png" -f $app.ToLowerInvariant(), $control)
        try {
            Capture-Window $window.Current.NativeWindowHandle $screenCaptureTrustReference
        }
        catch {
            $screenCaptureTrustReference = ""
        }
    }
    [void](Capture-OpenInteractionFrame $window $baselinePath $preferScreenOpenCapture)
    $baselineNonBlank = if (Test-Path $baselinePath) { Test-ImageNotBlank $baselinePath } else { $false }
    $baselineControlCropPath = Join-Path $caseDir ("{0}-{1}-closed-control-crop.png" -f $app.ToLowerInvariant(), $control)
    $baselineControlCrop = if (Test-Path $baselinePath) {
        Save-ElementCrop $window $baselinePath $baselineControlCropPath $showButton "UIA" 10
    }
    else {
        $null
    }
    $baselineControlNonBlank = $null -ne $baselineControlCrop -and $baselineControlCrop.Contains("NonBlank") -and $baselineControlCrop.NonBlank
    if (!$baselineNonBlank -or !$baselineControlNonBlank) {
        Start-Sleep -Milliseconds 300
        [void](Capture-OpenInteractionFrame $window $baselinePath $preferScreenOpenCapture)
        $baselineNonBlank = if (Test-Path $baselinePath) { Test-ImageNotBlank $baselinePath } else { $false }
        $baselineControlCrop = if (Test-Path $baselinePath) {
            Save-ElementCrop $window $baselinePath $baselineControlCropPath $showButton "UIA" 10
        }
        else {
            $null
        }
        $baselineControlNonBlank = $null -ne $baselineControlCrop -and $baselineControlCrop.Contains("NonBlank") -and $baselineControlCrop.NonBlank
    }
    if (!$baselineControlNonBlank) {
        $existingBaselinePath = Join-Path $caseDir ("{0}-{1}.png" -f $app.ToLowerInvariant(), $control)
        if (Test-Path $existingBaselinePath) {
            try {
                Copy-Item -LiteralPath $existingBaselinePath -Destination $baselinePath -Force
                $baselineNonBlank = if (Test-Path $baselinePath) { Test-ImageNotBlank $baselinePath } else { $false }
                $baselineControlCrop = Save-ElementCrop $window $baselinePath $baselineControlCropPath $showButton "UIA" 10
                $baselineControlNonBlank = $null -ne $baselineControlCrop -and $baselineControlCrop.Contains("NonBlank") -and $baselineControlCrop.NonBlank
            }
            catch {
            }
        }
    }
    if (!$baselineControlNonBlank -and $app -eq "ModernWpf" -and $null -ne $showButton) {
        try {
            $artifactId = $showButton.Current.AutomationId
            if (![string]::IsNullOrWhiteSpace($artifactId)) {
                $artifactCropPath = Join-Path $caseDir ("modernwpf-artifacts\{0}.png" -f $artifactId)
                $artifactCrop = New-RenderedArtifactCrop $artifactCropPath $artifactId ([ordered]@{
                    Found = $true
                    Reason = ""
                    X = 0
                    Y = 0
                    Width = 0
                    Height = 0
                    ChangedSamples = 0
                })
                if ($null -ne $artifactCrop -and $artifactCrop.NonBlank) {
                    $baselineControlCrop = $artifactCrop
                    $baselineControlNonBlank = $true
                }
            }
        }
        catch {
        }
    }
    if ($preferScreenOpenCapture -and
        ![string]::IsNullOrEmpty($screenCaptureTrustReference) -and
        (Test-Path $screenCaptureTrustReference) -and
        (Test-Path $baselinePath)) {
        $screenCaptureTrustDelta = Compare-Images $screenCaptureTrustReference $baselinePath
        $screenCaptureTrusted = $screenCaptureTrustDelta.Comparable -and $screenCaptureTrustDelta.MeanDelta -lt 25.0
    }
    $invoked = Invoke-ElementUntilOpen $window $triggerElement $openNames $control
    $frames = New-Object System.Collections.Generic.List[object]
    $frameDelays = @(0, 150, 300, 450)
    $previousDelay = 0

    foreach ($delay in $frameDelays) {
        if ($delay -gt $previousDelay) {
            Start-Sleep -Milliseconds ($delay - $previousDelay)
        }
        $previousDelay = $delay

        $framePath = Join-Path $caseDir ("{0}-{1}-open-{2:D3}ms.png" -f $app.ToLowerInvariant(), $control, $delay)
        $frameError = Capture-OpenInteractionFrame $window $framePath $preferScreenOpenCapture -SkipActivate
        $frames.Add([ordered]@{
            DelayMs = $delay
            Screenshot = $(if (Test-Path $framePath) { $framePath } else { "" })
            NonBlank = $(if (Test-Path $framePath) { Test-ImageNotBlank $framePath } else { $false })
            Error = $frameError
        })
    }

    $openDelta = $null
    $visualOpened = $false
    $crop = $null
    $selectedFrame = $null
    $comboBoxOpenVisualDelta = $null
    $comboBoxOpenBaselineCrop = ""
    $comboBoxPopupScreenshot = ""
    $comboBoxPopupNonBlank = $false
    $comboBoxPopupSize = $null
    $menuBarPopupNonBlank = $false
    $menuBarPopupScreenshot = ""
    $menuBarPopupSize = $null
    $openPopupNonBlank = $false
    $openPopupScreenshot = ""
    $openPopupSize = $null
    $openElement = Find-OpenInteractionElement $window $showButton $openNames $control
    if ($null -ne $openElement) {
        $treePath = Join-Path $caseDir ("{0}-{1}-open.uia.txt" -f $app.ToLowerInvariant(), $control)
        Write-UiaTree $openElement $treePath 3
        if ($control -eq "ComboBox") {
            $popupHandle = Get-ElementNativeWindowHandle $openElement
            if ($popupHandle -ne [IntPtr]::Zero -and $popupHandle -ne $window.Current.NativeWindowHandle) {
                $comboBoxPopupScreenshot = Join-Path $caseDir ("{0}-{1}-popup-window.png" -f $app.ToLowerInvariant(), $control)
                try {
                    Capture-Window $popupHandle $comboBoxPopupScreenshot -SkipActivate
                    $comboBoxPopupNonBlank = Test-ImageNotBlank $comboBoxPopupScreenshot
                    if ($comboBoxPopupNonBlank) {
                        $comboBoxPopupSize = Get-ImageSize $comboBoxPopupScreenshot
                    }
                }
                catch {
                    $comboBoxPopupScreenshot = ""
                    $comboBoxPopupNonBlank = $false
                    $comboBoxPopupSize = $null
                }
            }
        }
        elseif ($control -eq "MenuBar") {
            $popupHandle = Get-ElementNativeWindowHandle $openElement
            if ($popupHandle -ne [IntPtr]::Zero -and $popupHandle -ne $window.Current.NativeWindowHandle) {
                $menuBarPopupScreenshot = Join-Path $caseDir ("{0}-{1}-popup-window.png" -f $app.ToLowerInvariant(), $control)
                try {
                    Capture-Window $popupHandle $menuBarPopupScreenshot -SkipActivate
                    $menuBarPopupNonBlank = Test-ImageNotBlank $menuBarPopupScreenshot
                    if (!$menuBarPopupNonBlank) {
                        Capture-ScreenRect $popupHandle $menuBarPopupScreenshot
                        $menuBarPopupNonBlank = Test-ImageNotBlank $menuBarPopupScreenshot
                    }

                    if ($menuBarPopupNonBlank) {
                        $menuBarPopupSize = Get-ImageSize $menuBarPopupScreenshot
                    }
                    else {
                        $menuBarPopupScreenshot = ""
                    }
                }
                catch {
                    $menuBarPopupScreenshot = ""
                    $menuBarPopupNonBlank = $false
                    $menuBarPopupSize = $null
                }
            }
        }
        elseif (Test-ControlRequiresPopupWindowOpenProof $control) {
            $popupHandle = Get-ElementNativeWindowHandle $openElement
            if ($popupHandle -ne [IntPtr]::Zero -and $popupHandle -ne $window.Current.NativeWindowHandle) {
                $openPopupScreenshot = Join-Path $caseDir ("{0}-{1}-popup-window.png" -f $app.ToLowerInvariant(), $control)
                try {
                    Capture-Window $popupHandle $openPopupScreenshot -SkipActivate
                    $openPopupNonBlank = Test-ImageNotBlank $openPopupScreenshot
                    if (!$openPopupNonBlank) {
                        Capture-ScreenRect $popupHandle $openPopupScreenshot
                        $openPopupNonBlank = Test-ImageNotBlank $openPopupScreenshot
                    }

                    if ($openPopupNonBlank) {
                        $openPopupSize = Get-ImageSize $openPopupScreenshot
                    }
                    else {
                        $openPopupScreenshot = ""
                    }
                }
                catch {
                    $openPopupScreenshot = ""
                    $openPopupNonBlank = $false
                    $openPopupSize = $null
                }
            }
        }
        $cropElement = Find-DescendantByAutomationId $openElement "GalleryItemPageRoot"
        if ($null -eq $cropElement) {
            $cropElement = Find-DescendantByAutomationId $openElement "ContentRootGrid"
        }
        if ($null -eq $cropElement) {
            $cropElement = $openElement
        }
    }
    else {
        $treePath = ""
        $cropElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "GalleryItemPageRoot"
        if ($null -eq $cropElement) {
            $cropElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "ContentRootGrid"
        }
    }

    $usableFrames = @($frames.ToArray() | Where-Object { $_.NonBlank -and ![string]::IsNullOrEmpty($_.Screenshot) })
    if ($usableFrames.Count -gt 0) {
        $selectedFrame = $usableFrames[$usableFrames.Count - 1]
        foreach ($frame in $usableFrames) {
            $frameDelta = Compare-Images $baselinePath $frame.Screenshot
            if ($frameDelta.Comparable -and ($null -eq $openDelta -or $frameDelta.MeanDelta -gt $openDelta.MeanDelta)) {
                $openDelta = $frameDelta
                $selectedFrame = $frame
            }
        }

        $elementBounds = Get-ElementWindowBounds $window $cropElement
        $windowRect = [GalleryVisualNative]::GetRect($window.Current.NativeWindowHandle)
        $windowWidth = [Math]::Max(1, $windowRect.Right - $windowRect.Left)
        $windowHeight = [Math]::Max(1, $windowRect.Bottom - $windowRect.Top)
        $elementBoundsUsable = $null -ne $elementBounds -and
            $elementBounds.Found -and
            $elementBounds.Width -lt ($windowWidth * 0.75) -and
            $elementBounds.Height -lt ($windowHeight * 0.75)
        if ($elementBoundsUsable) {
            $cropPath = Join-Path $caseDir ("{0}-{1}-open-crop.png" -f $app.ToLowerInvariant(), $control)
            $expandedBounds = Save-Crop $selectedFrame.Screenshot $elementBounds $cropPath
            if ($control -eq "ComboBox") {
                $comboBoxOpenBaselineCrop = Join-Path $caseDir ("{0}-{1}-open-baseline-crop.png" -f $app.ToLowerInvariant(), $control)
                [void](Save-Crop $baselinePath $elementBounds $comboBoxOpenBaselineCrop)
                $comboBoxOpenVisualDelta = Compare-ImagesNormalized $comboBoxOpenBaselineCrop $cropPath
            }
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
            $differenceBounds = Find-DifferenceBounds $baselinePath $selectedFrame.Screenshot
            if ($differenceBounds.Found) {
                $targetBounds = Get-ElementWindowBounds $window $showButton
                $differenceBounds = Trim-DifferenceBoundsToContentRoot $differenceBounds $targetBounds
                $cropPath = Join-Path $caseDir ("{0}-{1}-open-crop.png" -f $app.ToLowerInvariant(), $control)
                $expandedBounds = Save-Crop $selectedFrame.Screenshot $differenceBounds $cropPath
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

        $deltaOpened = $null -ne $openDelta -and $openDelta.Comparable -and $openDelta.MeanDelta -gt 1.0
        $visualOpened = $deltaOpened -or
            ($null -ne $crop -and $crop.Found -and $crop.Source -eq "Difference" -and $crop.ChangedSamples -gt 0)
        if ($control -eq "ComboBox") {
            $visualOpened = $comboBoxPopupNonBlank -or
                ($screenCaptureTrusted -and
                    $null -ne $comboBoxOpenVisualDelta -and
                    $comboBoxOpenVisualDelta.Comparable -and
                    $comboBoxOpenVisualDelta.MeanDelta -gt 5.0)
            if ($comboBoxPopupNonBlank) {
                $crop = [ordered]@{
                    Found = $true
                    Screenshot = $comboBoxPopupScreenshot
                    Bounds = [ordered]@{
                        Found = $true
                        Reason = ""
                        X = 0
                        Y = 0
                        Width = $comboBoxPopupSize.Width
                        Height = $comboBoxPopupSize.Height
                        ChangedSamples = 0
                    }
                    Width = $comboBoxPopupSize.Width
                    Height = $comboBoxPopupSize.Height
                    ChangedSamples = 0
                    Source = "PopupWindow"
                }
            }
        }
        elseif ($control -eq "MenuBar") {
            $visualOpened = $menuBarPopupNonBlank
            if ($menuBarPopupNonBlank) {
                $crop = [ordered]@{
                    Found = $true
                    Screenshot = $menuBarPopupScreenshot
                    Bounds = [ordered]@{
                        Found = $true
                        Reason = ""
                        X = 0
                        Y = 0
                        Width = $menuBarPopupSize.Width
                        Height = $menuBarPopupSize.Height
                        ChangedSamples = 0
                    }
                    Width = $menuBarPopupSize.Width
                    Height = $menuBarPopupSize.Height
                    ChangedSamples = 0
                    Source = "PopupWindow"
                }
            }
        }
        elseif (Test-ControlRequiresPopupWindowOpenProof $control) {
            $visualOpened = $openPopupNonBlank
            if ($openPopupNonBlank) {
                $crop = [ordered]@{
                    Found = $true
                    Screenshot = $openPopupScreenshot
                    Bounds = [ordered]@{
                        Found = $true
                        Reason = ""
                        X = 0
                        Y = 0
                        Width = $openPopupSize.Width
                        Height = $openPopupSize.Height
                        ChangedSamples = 0
                    }
                    Width = $openPopupSize.Width
                    Height = $openPopupSize.Height
                    ChangedSamples = 0
                    Source = "PopupWindow"
                }
            }
        }
    }

    if ($app -eq "ModernWpf" -and $null -eq $crop) {
        $artifactCropSource = "GalleryItemPageRoot"
        $artifactCropPath = Join-Path $caseDir "modernwpf-artifacts\GalleryItemPageRoot.png"
        if (!(Test-Path $artifactCropPath)) {
            $artifactCropSource = "ContentRootGrid"
            $artifactCropPath = Join-Path $caseDir "modernwpf-artifacts\ContentRootGrid.png"
        }
        $artifactCrop = New-RenderedArtifactCrop $artifactCropPath $artifactCropSource ([ordered]@{
            Found = $true
            Reason = ""
            X = 0
            Y = 0
            Width = 0
            Height = 0
            ChangedSamples = 0
        })
        if ($null -ne $artifactCrop -and $artifactCrop.NonBlank) {
            $crop = $artifactCrop
            $visualOpened = $true
        }
    }

    if ($control -eq "ComboBox") {
        $status = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "Failed" } elseif (!$invoked) { "Failed" } elseif ($null -ne $openElement -and $visualOpened) { "Passed" } else { "Failed" }
        $notes = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "$control open interaction baseline screenshot or control crop was blank." } elseif (!$invoked) { "Could not invoke the $control sample button." } elseif ($null -eq $openElement) { "$control did not expose an expanded dropdown item." } elseif (!$screenCaptureTrusted -and !$comboBoxPopupNonBlank) { "$control screen capture did not match the Gallery window, and the popup window could not be captured." } elseif (!$visualOpened) { "$control exposed dropdown UIA but no changed dropdown pixels were captured." } else { "" }
    }
    elseif ($control -eq "MenuBar") {
        $status = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "Failed" } elseif (!$invoked) { "Failed" } elseif ($null -ne $openElement -and $visualOpened) { "Passed" } else { "Failed" }
        $notes = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "$control open interaction baseline screenshot or control crop was blank." } elseif (!$invoked) { "Could not invoke the $control sample button." } elseif ($null -eq $openElement) { "$control did not expose an opened menu item." } elseif (!$visualOpened) { "$control exposed opened menu UIA but no nonblank popup item pixels were captured." } else { "" }
    }
    elseif (Test-ControlRequiresPopupWindowOpenProof $control) {
        $status = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "Failed" } elseif (!$invoked) { "Failed" } elseif ($null -ne $openElement -and $visualOpened) { "Passed" } else { "Failed" }
        $notes = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "$control open interaction baseline screenshot or control crop was blank." } elseif (!$invoked) { "Could not invoke the $control sample button." } elseif ($null -eq $openElement) { "$control did not expose opened popup content." } elseif (!$visualOpened) { "$control exposed opened popup UIA but no nonblank popup window was captured." } else { "" }
    }
    else {
        $status = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "Failed" } elseif (!$invoked) { "Failed" } elseif ($null -ne $openElement -or $visualOpened) { "Passed" } else { "Failed" }
        $notes = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "$control open interaction baseline screenshot or control crop was blank." } elseif (!$invoked) { "Could not invoke the $control sample button." } elseif ($null -eq $openElement -and !$visualOpened) { "$control did not produce UIA or visual evidence of opening." } elseif ($null -eq $openElement -and $null -ne $crop -and ($crop.Source -eq "GalleryItemPageRoot" -or $crop.Source -eq "ContentRootGrid")) { "$control open content was verified from the in-app rendered artifact." } elseif ($null -eq $openElement) { "$control open content was not found in UIA; visual delta verified." } else { "" }
    }

    if ($preferScreenOpenCapture) {
        [GalleryVisualNative]::SetTopMost($window.Current.NativeWindowHandle, $false)
    }

    return [ordered]@{
        Status = $status
        Invoked = $invoked
        TriggerName = $(if ($null -ne $triggerElement) { $triggerElement.Current.Name } else { "" })
        TriggerAutomationId = $(if ($null -ne $triggerElement) { $triggerElement.Current.AutomationId } else { "" })
        TriggerUiaTree = $triggerTreePath
        BaselineScreenshot = $baselinePath
        BaselineNonBlank = $baselineNonBlank
        BaselineControlCrop = $baselineControlCrop
        BaselineControlNonBlank = $baselineControlNonBlank
        OpenElementFound = $null -ne $openElement
        OpenElementName = $(if ($null -ne $openElement) { $openElement.Current.Name } else { "" })
        UiaTree = $treePath
        Frames = $frames.ToArray()
        OpenDelta = $openDelta
        Crop = $crop
        ScreenCaptureTrustReference = $screenCaptureTrustReference
        ScreenCaptureTrustDelta = $screenCaptureTrustDelta
        ScreenCaptureTrusted = $screenCaptureTrusted
        ComboBoxOpenBaselineCrop = $comboBoxOpenBaselineCrop
        ComboBoxOpenVisualDelta = $comboBoxOpenVisualDelta
        ComboBoxPopupScreenshot = $comboBoxPopupScreenshot
        ComboBoxPopupNonBlank = $comboBoxPopupNonBlank
        ComboBoxPopupSize = $comboBoxPopupSize
        MenuBarPopupScreenshot = $menuBarPopupScreenshot
        MenuBarPopupNonBlank = $menuBarPopupNonBlank
        MenuBarPopupSize = $menuBarPopupSize
        OpenPopupScreenshot = $openPopupScreenshot
        OpenPopupNonBlank = $openPopupNonBlank
        OpenPopupSize = $openPopupSize
        SelectedFrameDelayMs = $(if ($null -ne $selectedFrame) { $selectedFrame.DelayMs } else { $null })
        SelectedFrameScreenshot = $(if ($null -ne $selectedFrame) { $selectedFrame.Screenshot } else { "" })
        Notes = $notes
    }
}

function Capture-StateInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsStateInteraction $control)) {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250

    $baselineState = Get-ToggleStateName $element
    $desiredState = if ($baselineState -eq "On") { "Off" } else { "On" }
    $renderedArtifactPath = if ($app -eq "ModernWpf") { Get-ModernRenderedElementArtifactPath $caseDir $element } else { "" }
    $renderedArtifactSource = if ($app -eq "ModernWpf" -and $null -ne $element) { $element.Current.AutomationId } else { "" }
    $baselinePath = Join-Path $caseDir ("{0}-{1}-state-before.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $baselinePath
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $baselinePath
    }

    $baselineCropPath = Join-Path $caseDir ("{0}-{1}-state-before-crop.png" -f $app.ToLowerInvariant(), $control)
    $baselineCrop = Copy-RenderedArtifactCrop $renderedArtifactPath $baselineCropPath $renderedArtifactSource
    if ($null -eq $baselineCrop -and (Test-Path $baselinePath)) {
        $baselineCrop = Save-ElementCrop $window $baselinePath $baselineCropPath $element "UIA" 10
    }

    $invoked = $false
    if (![string]::IsNullOrEmpty($baselineState)) {
        $invoked = Set-ToggleElementState $window $element $desiredState
    }
    Start-Sleep -Milliseconds 180

    $afterState = Get-ToggleStateName $element
    if ($app -eq "ModernWpf" -and ![string]::IsNullOrWhiteSpace($renderedArtifactPath)) {
        [void](Refresh-ModernWpfVisualArtifacts $window)
    }
    $afterPath = Join-Path $caseDir ("{0}-{1}-state-after.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $afterPath
    }

    $afterCropPath = Join-Path $caseDir ("{0}-{1}-state-after-crop.png" -f $app.ToLowerInvariant(), $control)
    $afterCrop = Copy-RenderedArtifactCrop $renderedArtifactPath $afterCropPath $renderedArtifactSource
    if ($null -eq $afterCrop -and (Test-Path $afterPath)) {
        $afterCrop = Save-ElementCrop $window $afterPath $afterCropPath $element "UIA" 10
    }

    $stateDelta = $null
    if ($null -ne $baselineCrop -and $null -ne $afterCrop -and
        $baselineCrop.Found -and $afterCrop.Found -and
        ![string]::IsNullOrEmpty($baselineCrop.Screenshot) -and
        ![string]::IsNullOrEmpty($afterCrop.Screenshot)) {
        $stateDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot
    }

    $stateChanged = ![string]::IsNullOrEmpty($baselineState) -and
        ![string]::IsNullOrEmpty($afterState) -and
        $baselineState -ne $afterState -and
        $afterState -eq $desiredState
    $visualChanged = $null -ne $stateDelta -and $stateDelta.Comparable -and $stateDelta.MeanDelta -gt 0.5
    $status = if (!$invoked) { "Failed" } elseif (!$stateChanged) { "Failed" } elseif (!$visualChanged) { "Failed" } else { "Passed" }
    $notes = if ([string]::IsNullOrEmpty($baselineState)) {
        "$control did not expose a UIA TogglePattern state."
    }
    elseif (!$invoked) {
        "Could not toggle the $control sample from $baselineState to $desiredState."
    }
    elseif (!$stateChanged) {
        "$control toggle state did not change from $baselineState to $desiredState; observed '$afterState'."
    }
    elseif (!$visualChanged) {
        "$control toggle state changed, but the cropped control image did not visibly change."
    }
    else {
        ""
    }

    return [ordered]@{
        Status = $status
        Kind = "State"
        Invoked = $invoked
        BaselineState = $baselineState
        DesiredState = $desiredState
        StateAfter = $afterState
        BaselineScreenshot = $baselinePath
        Frames = @(
            [ordered]@{
                DelayMs = 180
                Screenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
                NonBlank = $(if (Test-Path $afterPath) { Test-ImageNotBlank $afterPath } else { $false })
                Error = ""
            }
        )
        StateDelta = $stateDelta
        Crop = $afterCrop
        BaselineCrop = $baselineCrop
        SelectedFrameDelayMs = 180
        SelectedFrameScreenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
        Notes = $notes
    }
}

function Capture-SelectionInteraction([string]$app, [string]$control, [string]$caseDir, $window, $sampleElement) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsSelectionInteraction $control)) {
        return $null
    }

    $triggerName = Get-SelectionInteractionTriggerName $control
    if ([string]::IsNullOrWhiteSpace($triggerName)) {
        return [ordered]@{
            Status = "Failed"
            Kind = "Selection"
            Invoked = $false
            BaselineScreenshot = ""
            Frames = @()
            Crop = $null
            Notes = "$control does not have a configured selection interaction trigger."
        }
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250

    $cropAutomationId = Get-SelectionInteractionCropAutomationId $control
    $cropElement = if (![string]::IsNullOrWhiteSpace($cropAutomationId)) {
        TryFind-DescendantByAutomationId $window $cropAutomationId
    }
    else {
        $null
    }
    if ($null -eq $cropElement) {
        $cropElement = $sampleElement
    }

    $baselinePath = Join-Path $caseDir ("{0}-{1}-selection-before.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $baselinePath
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $baselinePath
    }

    $baselineCropPath = Join-Path $caseDir ("{0}-{1}-selection-before-crop.png" -f $app.ToLowerInvariant(), $control)
    $baselineCrop = if (Test-Path $baselinePath) {
        Save-ElementCrop $window $baselinePath $baselineCropPath $cropElement "UIA" 10
    }
    else {
        $null
    }

    $trigger = if ($null -ne $cropElement) { Find-DescendantByName $cropElement $triggerName } else { $null }
    if ($null -eq $trigger) {
        $trigger = Find-DescendantByName $window $triggerName
    }
    if ($null -eq $trigger) {
        $trigger = Find-ElementByNameInProcess $window.Current.ProcessId @($triggerName)
    }
    $invoked = if ($control -eq "GridView") {
        Invoke-GridViewItemClickOnce $app $window
    }
    else {
        Invoke-SelectionElementOnce $window $trigger
    }
    Start-Sleep -Milliseconds 250

    $afterPath = Join-Path $caseDir ("{0}-{1}-selection-after.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $afterPath
    }

    $afterCropPath = Join-Path $caseDir ("{0}-{1}-selection-after-crop.png" -f $app.ToLowerInvariant(), $control)
    $afterCrop = if (Test-Path $afterPath) {
        Save-ElementCrop $window $afterPath $afterCropPath $cropElement "UIA" 10
    }
    else {
        $null
    }
    if (($null -eq $baselineCrop -or !$baselineCrop.Found) -and
        $null -ne $afterCrop -and $afterCrop.Found -and
        $null -ne $afterCrop.Bounds -and $afterCrop.Bounds.Found -and
        (Test-Path $baselinePath)) {
        $savedBounds = Save-Crop $baselinePath $afterCrop.Bounds $baselineCropPath 0
        $baselineCrop = New-RenderedArtifactCrop $baselineCropPath "UIA" $savedBounds
    }

    $selectionDelta = $null
    if ($null -ne $baselineCrop -and $null -ne $afterCrop -and
        $baselineCrop.Found -and $afterCrop.Found -and
        ![string]::IsNullOrEmpty($baselineCrop.Screenshot) -and
        ![string]::IsNullOrEmpty($afterCrop.Screenshot)) {
        $selectionDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot
    }

    $expectedName = Get-SelectionInteractionExpectedName $control
    $expectedFound = [string]::IsNullOrWhiteSpace($expectedName) -or
        $null -ne (Find-ElementByNameInProcess $window.Current.ProcessId @($expectedName))
    $visualChanged = $null -ne $selectionDelta -and $selectionDelta.Comparable -and $selectionDelta.MeanDelta -gt 0.5
    $status = if (!$invoked) { "Failed" } elseif (!$expectedFound) { "Failed" } elseif (!$visualChanged) { "Failed" } else { "Passed" }
    $notes = if ($null -eq $trigger) {
        "$control selection trigger '$triggerName' was not found."
    }
    elseif (!$invoked) {
        "Could not invoke the $control selection trigger '$triggerName'."
    }
    elseif (!$expectedFound) {
        "$control selection did not expose expected content '$expectedName'."
    }
    elseif (!$visualChanged) {
        "$control selection click did not visibly change the cropped sample image."
    }
    else {
        ""
    }

    return [ordered]@{
        Status = $status
        Kind = "Selection"
        Invoked = $invoked
        TriggerName = $triggerName
        ExpectedName = $expectedName
        ExpectedFound = $expectedFound
        BaselineScreenshot = $baselinePath
        Frames = @(
            [ordered]@{
                DelayMs = 250
                Screenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
                NonBlank = $(if (Test-Path $afterPath) { Test-ImageNotBlank $afterPath } else { $false })
                Error = ""
            }
        )
        SelectionDelta = $selectionDelta
        Crop = $afterCrop
        BaselineCrop = $baselineCrop
        SelectedFrameDelayMs = 250
        SelectedFrameScreenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
        Notes = $notes
    }
}

function Capture-ValueInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsValueInteraction $control)) {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250

    $baselineValue = Get-ElementNumericValue $element
    $step = Get-ValueInteractionStep $control
    $expectedValue = Get-ValueInteractionTargetValue $control $baselineValue
    $cropAutomationId = Get-ValueInteractionCropAutomationId $control
    $cropElement = if (![string]::IsNullOrWhiteSpace($cropAutomationId)) {
        TryFind-DescendantByAutomationId $window $cropAutomationId
    }
    else {
        $null
    }
    if ($null -eq $cropElement) {
        $cropElement = $element
    }
    $treePath = Join-Path $caseDir ("{0}-{1}-value.uia.txt" -f $app.ToLowerInvariant(), $control)
    try {
        Write-UiaTree $element $treePath 5
    }
    catch {
        Set-Content -Path $treePath -Value ("UIA tree capture failed: " + $_.Exception.Message) -Encoding UTF8
    }
    $increaseButtonNames = Get-ValueInteractionIncreaseButtonNames $control
    $increaseButton = Find-DescendantButtonByAnyName $element $increaseButtonNames
    $baselinePath = Join-Path $caseDir ("{0}-{1}-value-before.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $baselinePath
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $baselinePath
    }

    $baselineCropPath = Join-Path $caseDir ("{0}-{1}-value-before-crop.png" -f $app.ToLowerInvariant(), $control)
    $baselineCrop = if (Test-Path $baselinePath) {
        Save-ElementCrop $window $baselinePath $baselineCropPath $cropElement "UIA" 10
    }
    else {
        $null
    }
    if ($null -ne $baselineCrop -and $baselineCrop.Contains("NonBlank") -and !$baselineCrop.NonBlank) {
        Start-Sleep -Milliseconds 300
        try {
            Capture-Window $window.Current.NativeWindowHandle $baselinePath
            $baselineCrop = Save-ElementCrop $window $baselinePath $baselineCropPath $cropElement "UIA" 10
        }
        catch {
        }
    }

    $invoked = Invoke-ValueIncreaseOnce $window $control $element $expectedValue
    Start-Sleep -Milliseconds 250

    $afterValue = Get-ElementNumericValue $element
    $afterPath = Join-Path $caseDir ("{0}-{1}-value-after.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $afterPath
    }

    $afterCropPath = Join-Path $caseDir ("{0}-{1}-value-after-crop.png" -f $app.ToLowerInvariant(), $control)
    $afterCrop = if (Test-Path $afterPath) {
        Save-ElementCrop $window $afterPath $afterCropPath $cropElement "UIA" 10
    }
    else {
        $null
    }
    if ($null -ne $afterCrop -and $afterCrop.Contains("NonBlank") -and !$afterCrop.NonBlank) {
        Start-Sleep -Milliseconds 300
        try {
            Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
            $afterCrop = Save-ElementCrop $window $afterPath $afterCropPath $cropElement "UIA" 10
        }
        catch {
        }
    }

    $valueDelta = $null
    if ($null -ne $baselineCrop -and $null -ne $afterCrop -and
        $baselineCrop.Found -and $afterCrop.Found -and
        ![string]::IsNullOrEmpty($baselineCrop.Screenshot) -and
        ![string]::IsNullOrEmpty($afterCrop.Screenshot)) {
        $valueDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot
    }

    $valueChanged = (Test-DoubleApproximatelyEqual $afterValue $expectedValue)
    $visualChanged = $null -ne $valueDelta -and $valueDelta.Comparable -and $valueDelta.MeanDelta -gt 0.1
    $baselineNonBlank = $null -ne $baselineCrop -and $baselineCrop.Contains("NonBlank") -and $baselineCrop.NonBlank
    $afterNonBlank = $null -ne $afterCrop -and $afterCrop.Contains("NonBlank") -and $afterCrop.NonBlank
    $status = if ($null -eq $baselineValue) { "Failed" } elseif (!$invoked) { "Failed" } elseif (!$valueChanged) { "Failed" } elseif (!$baselineNonBlank -or !$afterNonBlank) { "Failed" } else { "Passed" }
    $notes = if ($null -eq $baselineValue) {
        "$control did not expose a readable numeric value."
    }
    elseif (!$invoked) {
        "Could not invoke the $control increase button."
    }
    elseif (!$valueChanged) {
        "$control value did not change from $baselineValue to expected $expectedValue; observed '$afterValue'."
    }
    elseif (!$baselineNonBlank -or !$afterNonBlank) {
        "$control value interaction crop was blank before or after activation."
    }
    else {
        ""
    }

    return [ordered]@{
        Status = $status
        Kind = "Value"
        Invoked = $invoked
        BaselineValue = $baselineValue
        Step = $step
        ExpectedValue = $expectedValue
        ValueAfter = $afterValue
        CropAutomationId = $cropAutomationId
        BaselineScreenshot = $baselinePath
        UiaTree = $treePath
        IncreaseButtonFound = $null -ne $increaseButton
        IncreaseButtonName = $(if ($null -ne $increaseButton) { $increaseButton.Current.Name } else { "" })
        IncreaseButtonAutomationId = $(if ($null -ne $increaseButton) { $increaseButton.Current.AutomationId } else { "" })
        IncreaseButtonCenterHit = Get-ElementCenterHitSummary $increaseButton
        Frames = @(
            [ordered]@{
                DelayMs = 250
                Screenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
                NonBlank = $(if (Test-Path $afterPath) { Test-ImageNotBlank $afterPath } else { $false })
                Error = ""
            }
        )
        ValueDelta = $valueDelta
        VisualChanged = $visualChanged
        Crop = $afterCrop
        BaselineCrop = $baselineCrop
        SelectedFrameDelayMs = 250
        SelectedFrameScreenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
        Notes = $notes
    }
}

function Capture-OutputInteraction([string]$app, [string]$control, [string]$caseDir, $window, $sampleElement) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsOutputInteraction $control)) {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250

    $triggerNames = Get-OutputInteractionTriggerNames $control
    $cropAutomationId = Get-OutputInteractionCropAutomationId $control
    $cropElement = if (![string]::IsNullOrWhiteSpace($cropAutomationId)) {
        TryFind-DescendantByAutomationId $window $cropAutomationId
    }
    else {
        $null
    }
    if ($null -eq $cropElement) {
        $cropElement = $sampleElement
    }

    $trigger = if (Test-ElementNameMatches $sampleElement $triggerNames) { $sampleElement } else { $null }
    if ($null -eq $trigger -and $null -ne $sampleElement) {
        $trigger = Find-DescendantByAnyName $sampleElement $triggerNames
    }
    if ($null -eq $trigger) {
        $trigger = Find-DescendantByAnyName $window $triggerNames
    }
    if ($null -eq $trigger -and $triggerNames.Count -gt 0) {
        $trigger = Find-ElementByNameInProcess $window.Current.ProcessId $triggerNames
    }

    $baselinePath = Join-Path $caseDir ("{0}-{1}-output-before.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $baselinePath
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $baselinePath
    }

    $baselineCropPath = Join-Path $caseDir ("{0}-{1}-output-before-crop.png" -f $app.ToLowerInvariant(), $control)
    $baselineCrop = if (Test-Path $baselinePath) {
        Save-ElementCrop $window $baselinePath $baselineCropPath $cropElement "UIA" 20
    }
    else {
        $null
    }
    if ($null -ne $baselineCrop -and $baselineCrop.Contains("NonBlank") -and !$baselineCrop.NonBlank) {
        Start-Sleep -Milliseconds 300
        try {
            Capture-Window $window.Current.NativeWindowHandle $baselinePath
            $baselineCrop = Save-ElementCrop $window $baselinePath $baselineCropPath $cropElement "UIA" 20
        }
        catch {
        }
    }

    $invoked = Invoke-ElementPatternOnce $window $trigger
    Start-Sleep -Milliseconds 250

    $afterPath = Join-Path $caseDir ("{0}-{1}-output-after.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $afterPath
    }

    $afterCropPath = Join-Path $caseDir ("{0}-{1}-output-after-crop.png" -f $app.ToLowerInvariant(), $control)
    $afterCrop = if (Test-Path $afterPath) {
        Save-ElementCrop $window $afterPath $afterCropPath $cropElement "UIA" 20
    }
    else {
        $null
    }
    if ($null -ne $afterCrop -and $afterCrop.Contains("NonBlank") -and !$afterCrop.NonBlank) {
        Start-Sleep -Milliseconds 300
        try {
            Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
            $afterCrop = Save-ElementCrop $window $afterPath $afterCropPath $cropElement "UIA" 20
        }
        catch {
        }
    }

    $outputDelta = $null
    if ($null -ne $baselineCrop -and $null -ne $afterCrop -and
        $baselineCrop.Found -and $afterCrop.Found -and
        ![string]::IsNullOrEmpty($baselineCrop.Screenshot) -and
        ![string]::IsNullOrEmpty($afterCrop.Screenshot)) {
        $outputDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot
    }

    $minimumDelta = Get-OutputInteractionMinimumDelta $control
    $visualChanged = $null -ne $outputDelta -and $outputDelta.Comparable -and $outputDelta.MeanDelta -gt $minimumDelta
    $baselineNonBlank = $null -ne $baselineCrop -and $baselineCrop.Contains("NonBlank") -and $baselineCrop.NonBlank
    $afterNonBlank = $null -ne $afterCrop -and $afterCrop.Contains("NonBlank") -and $afterCrop.NonBlank
    $status = if ($null -eq $trigger) { "Failed" } elseif (!$invoked) { "Failed" } elseif (!$baselineNonBlank -or !$afterNonBlank) { "Failed" } elseif (!$visualChanged) { "Failed" } else { "Passed" }
    $notes = if ($null -eq $trigger) {
        "$control output trigger '$($triggerNames -join "', '")' was not found."
    }
    elseif (!$invoked) {
        "Could not invoke the $control output trigger '$($triggerNames[0])'."
    }
    elseif (!$baselineNonBlank -or !$afterNonBlank) {
        "$control output interaction crop was blank before or after activation."
    }
    elseif (!$visualChanged) {
        "$control output interaction did not visibly change the cropped sample image."
    }
    else {
        ""
    }

    return [ordered]@{
        Status = $status
        Kind = "Output"
        Invoked = $invoked
        TriggerFound = $null -ne $trigger
        TriggerName = $(if ($null -ne $trigger) { $trigger.Current.Name } else { "" })
        TriggerAutomationId = $(if ($null -ne $trigger) { $trigger.Current.AutomationId } else { "" })
        CropAutomationId = $cropAutomationId
        BaselineScreenshot = $baselinePath
        Frames = @(
            [ordered]@{
                DelayMs = 250
                Screenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
                NonBlank = $(if (Test-Path $afterPath) { Test-ImageNotBlank $afterPath } else { $false })
                Error = ""
            }
        )
        OutputDelta = $outputDelta
        MinimumDelta = $minimumDelta
        VisualChanged = $visualChanged
        Crop = $afterCrop
        BaselineCrop = $baselineCrop
        SelectedFrameDelayMs = 250
        SelectedFrameScreenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
        Notes = $notes
    }
}

function Capture-TextInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsTextInteraction $control)) {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250

    $inputText = Get-TextInteractionInput $control
    $suggestionNames = Get-TextInteractionSuggestionNames $control
    $expectedOutputName = Get-TextInteractionExpectedOutputName $control

    $baselinePath = Join-Path $caseDir ("{0}-{1}-text-before.png" -f $app.ToLowerInvariant(), $control)
    [void](Capture-OpenInteractionFrame $window $baselinePath $false)

    $typed = Set-EditableElementText $window $element $inputText
    Start-Sleep -Milliseconds 400

    $suggestionElement = if ($typed) {
        Wait-ForListItemOutsideElementBounds $window $element $suggestionNames 3000
    }
    else {
        $null
    }

    $popupScreenshot = ""
    $popupNonBlank = $false
    $popupSize = $null
    $treePath = ""
    if ($null -ne $suggestionElement) {
        $treePath = Join-Path $caseDir ("{0}-{1}-text-suggestions.uia.txt" -f $app.ToLowerInvariant(), $control)
        Write-UiaTree $suggestionElement $treePath 3

        $popupHandle = Get-ElementNativeWindowHandle $suggestionElement
        if ($popupHandle -ne [IntPtr]::Zero -and $popupHandle -ne $window.Current.NativeWindowHandle) {
            $popupScreenshot = Join-Path $caseDir ("{0}-{1}-text-popup-window.png" -f $app.ToLowerInvariant(), $control)
            try {
                Capture-Window $popupHandle $popupScreenshot -SkipActivate
                $popupNonBlank = Test-ImageNotBlank $popupScreenshot
                if ($popupNonBlank) {
                    $popupSize = Get-ImageSize $popupScreenshot
                }
            }
            catch {
                $popupScreenshot = ""
                $popupNonBlank = $false
                $popupSize = $null
            }
        }
    }

    $suggestionInvoked = $false
    if ($null -ne $suggestionElement) {
        $suggestionInvoked = Invoke-ElementOnce $window $suggestionElement
        Start-Sleep -Milliseconds 500
    }

    $outputElement = if ($suggestionInvoked) {
        Wait-ForOutputTextOutsideElementBounds $window $element $expectedOutputName 3000
    }
    else {
        $null
    }

    $afterPath = Join-Path $caseDir ("{0}-{1}-text-after.png" -f $app.ToLowerInvariant(), $control)
    [void](Capture-OpenInteractionFrame $window $afterPath $false -SkipActivate)

    $crop = if ($popupNonBlank) {
        [ordered]@{
            Found = $true
            Screenshot = $popupScreenshot
            Bounds = [ordered]@{
                Found = $true
                Reason = ""
                X = 0
                Y = 0
                Width = $popupSize.Width
                Height = $popupSize.Height
                ChangedSamples = 0
            }
            Width = $popupSize.Width
            Height = $popupSize.Height
            ChangedSamples = 0
            Source = "PopupWindow"
        }
    }
    else {
        [ordered]@{
            Found = $false
            Screenshot = ""
            Bounds = [ordered]@{
                Found = $false
                Reason = "Suggestion popup window was not captured."
                X = 0
                Y = 0
                Width = 0
                Height = 0
                ChangedSamples = 0
            }
            Width = 0
            Height = 0
            ChangedSamples = 0
            Source = "None"
        }
    }

    $status = if (!$typed) { "Failed" } elseif ($null -eq $suggestionElement) { "Failed" } elseif (!$popupNonBlank) { "Failed" } elseif (!$suggestionInvoked) { "Failed" } elseif ($null -eq $outputElement) { "Failed" } else { "Passed" }
    $notes = if (!$typed) { "Could not type '$inputText' into the $control sample." } elseif ($null -eq $suggestionElement) { "$control did not expose expected suggestions for '$inputText'." } elseif (!$popupNonBlank) { "$control exposed suggestions in UIA but the popup window was not captured." } elseif (!$suggestionInvoked) { "Could not invoke the $control suggestion '$($suggestionNames[0])'." } elseif ($null -eq $outputElement) { "$control suggestion '$expectedOutputName' did not update the sample output." } else { "" }

    return [ordered]@{
        Status = $status
        Typed = $typed
        InputText = $inputText
        SuggestionElementFound = $null -ne $suggestionElement
        SuggestionElementName = $(if ($null -ne $suggestionElement) { $suggestionElement.Current.Name } else { "" })
        SuggestionInvoked = $suggestionInvoked
        OutputElementFound = $null -ne $outputElement
        OutputElementName = $(if ($null -ne $outputElement) { $outputElement.Current.Name } else { "" })
        BaselineScreenshot = $baselinePath
        UiaTree = $treePath
        Frames = @([ordered]@{
            DelayMs = 0
            Screenshot = $(if (![string]::IsNullOrEmpty($popupScreenshot)) { $popupScreenshot } else { $afterPath })
            NonBlank = $(if (![string]::IsNullOrEmpty($popupScreenshot) -and (Test-Path $popupScreenshot)) { Test-ImageNotBlank $popupScreenshot } elseif (Test-Path $afterPath) { Test-ImageNotBlank $afterPath } else { $false })
            Error = ""
        })
        Crop = $crop
        PopupScreenshot = $popupScreenshot
        PopupNonBlank = $popupNonBlank
        PopupSize = $popupSize
        SelectedFrameDelayMs = 0
        SelectedFrameScreenshot = $(if (![string]::IsNullOrEmpty($popupScreenshot)) { $popupScreenshot } else { $afterPath })
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

function Stop-WinUIReferenceProcesses {
    try {
        Get-Process -Name "WinUIGallery" -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Milliseconds 300
    }
    catch {
    }
}

function Capture-ModernWpf([string]$control, [string]$caseDir) {
    $route = "item/$control"
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null

    $args = @("--visual-test", "--route", $route, "--theme", $Theme, "--visual-artifact-dir", $artifactDir)
    if ($IncludeInteractions) {
        $args += "--open-interactions"
    }
    $process = Start-Process -FilePath $GalleryExe -ArgumentList $args -PassThru
    try {
        $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf Gallery window" -Probe {
            $process.Refresh()
            Find-WindowByProcessId $process.Id
        }

        [void][GalleryVisualNative]::Move($window.Current.NativeWindowHandle, 60, 60, $Width, $Height)
        Wait-ModernWpfReady $window $route $artifactDir | Out-Null
        Start-Sleep -Milliseconds 600

        $statusFile = Read-ModernWpfStatusFile $artifactDir
        $lastException = if ($null -ne $statusFile) { $statusFile.LastException } else { Get-AutomationText $window "GalleryVisualTestLastException" }
        $title = $control
        $requiredSampleAutomationId = Get-RequiredSampleAutomationId $control
        $requiredSampleArtifact = Join-Path $artifactDir ($requiredSampleAutomationId + ".png")
        $requiredSampleArtifactFound = Test-Path $requiredSampleArtifact
        $needsSampleElement = $IncludeInteractions -and (
            (Test-ControlSupportsOpenInteraction $control) -or
            (Test-ControlSupportsStateInteraction $control) -or
            (Test-ControlSupportsSelectionInteraction $control) -or
            (Test-ControlSupportsValueInteraction $control) -or
            (Test-ControlSupportsOutputInteraction $control) -or
            (Test-ControlSupportsTextInteraction $control))
        $sample = if ($requiredSampleArtifactFound -and !$needsSampleElement) { $null } else { TryFind-DescendantByAutomationId $window $requiredSampleAutomationId }
        $openNames = Get-OpenInteractionNames $control
        $openInteraction = Capture-OpenInteraction "ModernWpf" $control $caseDir $window $sample $openNames
        $stateInteraction = Capture-StateInteraction "ModernWpf" $control $caseDir $window $sample
        $selectionInteraction = Capture-SelectionInteraction "ModernWpf" $control $caseDir $window $sample
        $valueInteraction = Capture-ValueInteraction "ModernWpf" $control $caseDir $window $sample
        $outputInteraction = Capture-OutputInteraction "ModernWpf" $control $caseDir $window $sample
        $textInteraction = Capture-TextInteraction "ModernWpf" $control $caseDir $window $sample
        $interaction = if ($null -ne $openInteraction) { $openInteraction } elseif ($null -ne $stateInteraction) { $stateInteraction } elseif ($null -ne $selectionInteraction) { $selectionInteraction } elseif ($null -ne $valueInteraction) { $valueInteraction } elseif ($null -ne $outputInteraction) { $outputInteraction } else { $textInteraction }
        $screenshot = Join-Path $caseDir "modernwpf-$control.png"
        $treePath = Join-Path $caseDir "modernwpf-$control.uia.txt"

        if ($requiredSampleArtifactFound) {
            Set-Content -Path $treePath -Value "UIA tree skipped because rendered sample artifacts are available." -Encoding UTF8
        }
        else {
            try {
                Write-UiaTree $window $treePath 6
            }
            catch {
                Set-Content -Path $treePath -Value ("UIA tree capture failed: " + $_.Exception.Message) -Encoding UTF8
            }
        }
        $windowCaptureError = ""
        try {
            Capture-Window $window.Current.NativeWindowHandle $screenshot
        }
        catch {
            $windowCaptureError = $_.Exception.Message
        }
        $staticCrops = Capture-StaticCrops "ModernWpf" $control $caseDir $window $screenshot
        $latestStatusFile = Read-ModernWpfStatusFile $artifactDir
        if ($null -ne $latestStatusFile -and ![string]::IsNullOrWhiteSpace($latestStatusFile.LastException)) {
            $lastException = $latestStatusFile.LastException
        }
        elseif ([string]::IsNullOrWhiteSpace($lastException)) {
            $lastException = Get-AutomationText $window "GalleryVisualTestLastException"
        }
        $hasRenderedCrops = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and $staticCrops.Primary.NonBlank
        $windowScreenshotNonBlank = if (Test-Path $screenshot) { Test-ImageNotBlank $screenshot } else { $false }
        $notBlank = $windowScreenshotNonBlank -or $hasRenderedCrops
        $primaryCropBlank = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and !$staticCrops.Primary.NonBlank
        $primaryCropMinimumVisibleStdDev = Get-PrimaryCropMinimumVisibleStdDev $control
        $primaryCropLowVariation = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("VisibleStdDev") -and $staticCrops.Primary.VisibleStdDev -lt $primaryCropMinimumVisibleStdDev
        $requiredSampleFound = $requiredSampleArtifactFound -or $null -ne $sample -or $hasRenderedCrops
        $status = if ($lastException) { "Failed" } elseif (!$notBlank) { "Failed" } elseif ($primaryCropBlank) { "Failed" } elseif ($primaryCropLowVariation) { "Failed" } elseif (!$requiredSampleFound) { "Failed" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { "Failed" } else { "Passed" }
        if ($primaryCropBlank -and [string]::IsNullOrEmpty($lastException)) {
            $lastException = "Primary crop '$($staticCrops.Primary.Source)' was blank."
        }
        if ($primaryCropLowVariation -and [string]::IsNullOrEmpty($lastException)) {
            $lastException = "Primary crop '$($staticCrops.Primary.Source)' had low visible variation ($($staticCrops.Primary.VisibleStdDev), expected at least $primaryCropMinimumVisibleStdDev)."
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
            RequiredSampleElementFound = $requiredSampleFound
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
    $pageTitle = Get-WinUIReferencePageTitle $control
    Stop-WinUIReferenceProcesses
    Start-Process $route

    $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "installed WinUI 3 Gallery window for $control" -Probe {
        Find-WindowByTitle @("WinUI 3 Gallery", "WinUI Gallery")
    }

    try {
        [void][GalleryVisualNative]::Move($window.Current.NativeWindowHandle, 60, 60, $Width, $Height)
        Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "WinUI 3 Gallery page title '$pageTitle'" -Probe {
            Find-DescendantByName $window $pageTitle
        } | Out-Null
        Wait-WinUIReferenceReady $window $control
        Start-Sleep -Milliseconds 1200
        Reset-WinUIReferenceSampleScroll $window $control
        $themeProbe = Ensure-WinUIReferenceTheme $control $caseDir $window
        Reset-WinUIReferenceSampleScroll $window $control
        Reset-ProgressRingAnimationPhase $window $control | Out-Null

        $showButton = Find-ReferenceInteractionTrigger $window $control
        $openNames = Get-OpenInteractionNames $control
        $openInteraction = Capture-OpenInteraction "WinUI3" $control $caseDir $window $showButton $openNames
        $stateInteraction = Capture-StateInteraction "WinUI3" $control $caseDir $window $showButton
        $selectionInteraction = Capture-SelectionInteraction "WinUI3" $control $caseDir $window $showButton
        $valueInteraction = Capture-ValueInteraction "WinUI3" $control $caseDir $window $showButton
        $outputInteraction = Capture-OutputInteraction "WinUI3" $control $caseDir $window $showButton
        $textInteraction = Capture-TextInteraction "WinUI3" $control $caseDir $window $showButton
        $interaction = if ($null -ne $openInteraction) { $openInteraction } elseif ($null -ne $stateInteraction) { $stateInteraction } elseif ($null -ne $selectionInteraction) { $selectionInteraction } elseif ($null -ne $valueInteraction) { $valueInteraction } elseif ($null -ne $outputInteraction) { $outputInteraction } else { $textInteraction }
        $screenshot = Join-Path $caseDir "winui3-$control.png"
        $treePath = Join-Path $caseDir "winui3-$control.uia.txt"
        Write-UiaTree $window $treePath 6
        $staticCrops = $null
        $notBlank = $false
        $primaryCropBlank = $false
        $primaryCropLowVariation = $false
        $primaryCropMinimumVisibleStdDev = Get-PrimaryCropMinimumVisibleStdDev $control
        foreach ($captureAttempt in 1..3) {
            if ($captureAttempt -gt 1) {
                [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
                Start-Sleep -Milliseconds (400 * $captureAttempt)
            }

            Capture-Window $window.Current.NativeWindowHandle $screenshot
            $staticCrops = Capture-StaticCrops "WinUI3" $control $caseDir $window $screenshot
            $notBlank = Test-ImageNotBlank $screenshot
            $primaryCropBlank = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and !$staticCrops.Primary.NonBlank
            $primaryCropLowVariation = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("VisibleStdDev") -and $staticCrops.Primary.VisibleStdDev -lt $primaryCropMinimumVisibleStdDev
            if ($notBlank -and !$primaryCropBlank -and !$primaryCropLowVariation) {
                break
            }
        }

        $themeProbeFailed = -not (Test-WinUIReferenceThemeProbeSucceeded $themeProbe)

        return [ordered]@{
            App = "WinUI3Gallery"
            Control = $control
            Route = $route
            Status = $(if (!$notBlank) { "Failed" } elseif ($primaryCropBlank) { "Failed" } elseif ($primaryCropLowVariation) { "Failed" } elseif ($themeProbeFailed) { "Failed" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { "Failed" } else { "Passed" })
            Title = $pageTitle
            Screenshot = $screenshot
            UiaTree = $treePath
            LastException = $(if ($primaryCropBlank) { "Primary crop '$($staticCrops.Primary.Source)' was blank." } elseif ($primaryCropLowVariation) { "Primary crop '$($staticCrops.Primary.Source)' had low visible variation ($($staticCrops.Primary.VisibleStdDev), expected at least $primaryCropMinimumVisibleStdDev)." } elseif ($themeProbeFailed) { "Reference theme probe did not prove $Theme theme: $($themeProbe.Reason)" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { $interaction.Notes } else { "" })
            NonBlank = $notBlank
            RequiredSampleAutomationId = ""
            RequiredSampleElementFound = $true
            StaticCrops = $staticCrops
            Interaction = $interaction
            ThemeProbe = $themeProbe
        }
    }
    finally {
        Close-AutomationWindow $window
    }
}

function Ensure-WinUIReferenceTheme([string]$control, [string]$caseDir, $window) {
    if ($Theme -eq "Default") {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $null
            Toggled = $false
            Reason = "Default theme requested."
        }
    }

    $primarySource = Get-ReferencePrimaryAutomationId $control
    $primaryName = Get-ReferencePrimaryName $control
    if ([string]::IsNullOrEmpty($primarySource) -and [string]::IsNullOrEmpty($primaryName)) {
        $probeScreenshot = Join-Path $caseDir ("winui3-$control-theme-probe.png")
        Capture-Window $window.Current.NativeWindowHandle $probeScreenshot
        $mean = Get-ImageMeanLuminance $probeScreenshot
        if ($null -eq $mean) {
            return [ordered]@{
                RequestedTheme = $Theme
                MeanLuminance = $null
                Toggled = $false
                Reason = "No reference primary element is configured, and the reference screenshot had no visible pixels."
            }
        }

        $isDark = [double]$mean -lt 128.0
        $wantsDark = $Theme -eq "Dark"
        if ($isDark -eq $wantsDark) {
            return [ordered]@{
                RequestedTheme = $Theme
                MeanLuminance = $mean
                Toggled = $false
                Reason = "Reference window theme already matched without a configured primary element."
            }
        }

        $themeButton = Find-DescendantByAutomationId $window "ThemeButton"
        if ($null -eq $themeButton) {
            return [ordered]@{
                RequestedTheme = $Theme
                MeanLuminance = $mean
                Toggled = $false
                Reason = "No reference primary element is configured, and ThemeButton was not found."
            }
        }

        $toggled = Invoke-ElementPatternOnce $window $themeButton
        if ($toggled) {
            Start-Sleep -Milliseconds 700
        }

        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $mean
            Toggled = $toggled
            Reason = "Reference window theme inferred from full screenshot because no primary element is configured."
        }
    }

    $primaryElement = if (![string]::IsNullOrEmpty($primarySource)) {
        Find-DescendantByAutomationId $window $primarySource
    }
    else {
        $primarySource = $primaryName
        Find-ReferencePrimaryByName $window $control $primaryName
    }
    if ($null -eq $primaryElement) {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $null
            Toggled = $false
            Reason = "Reference primary element '$primarySource' was not found."
        }
    }

    $probeScreenshot = Join-Path $caseDir ("winui3-$control-theme-probe.png")
    $probeCropPath = Join-Path $caseDir ("winui3-$control-theme-probe-crop.png")
    Capture-Window $window.Current.NativeWindowHandle $probeScreenshot
    $probeCrop = Save-ElementCrop $window $probeScreenshot $probeCropPath $primaryElement $primarySource 0
    if (!$probeCrop.Found -or !$probeCrop.NonBlank) {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $null
            Toggled = $false
            Reason = "Reference theme probe crop was unavailable or blank."
        }
    }

    $mean = Get-ImageMeanLuminance $probeScreenshot
    if ($null -eq $mean) {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $null
            Toggled = $false
            Reason = "Reference theme probe screenshot had no visible pixels."
        }
    }

    $isDark = [double]$mean -lt 128.0
    $wantsDark = $Theme -eq "Dark"
    if ($isDark -eq $wantsDark) {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $mean
            Toggled = $false
            Reason = "Reference sample theme already matched."
        }
    }

    $themeButton = Find-DescendantByAutomationId $window "ThemeButton"
    if ($null -eq $themeButton) {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $mean
            Toggled = $false
            Reason = "ThemeButton was not found."
        }
    }

    $toggled = Invoke-ElementPatternOnce $window $themeButton
    if ($toggled) {
        Start-Sleep -Milliseconds 700
    }

    return [ordered]@{
        RequestedTheme = $Theme
        MeanLuminance = $mean
        Toggled = $toggled
        Reason = $(if ($toggled) { "Reference sample theme toggled to match requested theme." } else { "ThemeButton did not invoke." })
    }
}

function Test-WinUIReferenceThemeProbeSucceeded($themeProbe) {
    if ($Theme -eq "Default") {
        return $true
    }

    if ($null -eq $themeProbe) {
        return $false
    }

    if ($themeProbe.Toggled -eq $true) {
        return $true
    }

    $reason = [string]$themeProbe.Reason
    return $reason.Contains("already matched")
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
        $referenceResult = $null
        $lastReferenceError = ""
        foreach ($referenceAttempt in 1..3) {
            try {
                $referenceResult = Capture-WinUIReference $control $caseDir
                if (!$referenceResult.Contains("Attempt")) {
                    $referenceResult.Add("Attempt", $referenceAttempt)
                }
                if ($referenceResult.Status -eq "Passed") {
                    break
                }

                $lastReferenceError = $referenceResult.LastException
                Start-Sleep -Milliseconds (500 * $referenceAttempt)
            }
            catch {
                $lastReferenceError = $_.Exception.Message
                $referenceResult = $null
                Start-Sleep -Milliseconds (500 * $referenceAttempt)
            }
        }

        if ($null -ne $referenceResult) {
            $results.Add($referenceResult)
        }
        else {
            $results.Add([ordered]@{
                App = "WinUI3Gallery"
                Control = $control
                Route = "winui3gallery://item/$control"
                Status = "Failed"
                Title = ""
                Screenshot = ""
                UiaTree = ""
                LastException = $lastReferenceError
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
        $modernFrameScreenshot = $modernFrame.Screenshot
        $referenceFrameScreenshot = $referenceFrame.Screenshot
        if ($modern.Interaction.Contains("SelectedFrameScreenshot") -and $modern.Interaction.SelectedFrameScreenshot) {
            $modernFrameScreenshot = $modern.Interaction.SelectedFrameScreenshot
        }
        if ($referenceCapture.Interaction.Contains("SelectedFrameScreenshot") -and $referenceCapture.Interaction.SelectedFrameScreenshot) {
            $referenceFrameScreenshot = $referenceCapture.Interaction.SelectedFrameScreenshot
        }
        if ($modernFrameScreenshot -and $referenceFrameScreenshot) {
            $modern["InteractionReferenceComparison"] = Compare-Images $modernFrameScreenshot $referenceFrameScreenshot
        }

        if ($null -ne $modern.Interaction.Crop -and $null -ne $referenceCapture.Interaction.Crop -and
            $modern.Interaction.Crop.Found -and $referenceCapture.Interaction.Crop.Found -and
            $modern.Interaction.Crop.Source -eq $referenceCapture.Interaction.Crop.Source) {
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
