param(
    [string[]]$Controls = @("TeachingTip", "Button", "CheckBox", "ComboBox", "RadioButton", "Slider", "ColorPicker", "HyperlinkButton", "RatingControl", "RepeatButton", "ToggleButton", "DropDownButton", "SplitButton", "ToggleSplitButton", "ToggleSwitch", "NumberBox", "AutoSuggestBox", "SplitView", "PersonPicture", "ParallaxView", "IconElement", "ThemeShadow", "TitleBar", "InfoBadge", "InfoBar", "ProgressRing", "PipsPager", "AnnotatedScrollBar", "PullToRefresh", "GridView", "ItemsRepeater", "BreadcrumbBar", "Pivot", "SelectorBar", "NavigationView", "ContentDialog", "Flyout", "Popup", "MenuBar", "MenuFlyout", "SwipeControl", "AppBarButton", "AppBarSeparator", "AppBarToggleButton", "CommandBar", "CommandBarFlyout"),
    [ValidateSet("Light", "Dark", "Default")]
    [string]$Theme = "Light",
    [string]$GalleryExe,
    [string]$OutputRoot = "artifacts\gallery-recordings",
    [int]$WindowLeft = 220,
    [int]$WindowTop = 180,
    [int]$Width = 1180,
    [int]$Height = 820,
    [int]$CaptureMargin = 220,
    [int]$TimeoutSeconds = 30,
    [int]$DurationSeconds = 6,
    [int]$FrameRate = 10,
    [ValidateSet("Auto", "Ffmpeg", "Avi")]
    [string]$Recorder = "Auto",
    [ValidateSet("Rendered", "Screen")]
    [string]$CaptureMode = "Rendered",
    [switch]$Build,
    [switch]$SkipFrameExtraction
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

$RecordWindowRenderedScript = Join-Path $PSScriptRoot "Record-WindowRendered.ps1"
if (!(Test-Path $RecordWindowRenderedScript)) {
    throw "Record-WindowRendered.ps1 was not found beside this script."
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
try {
    Add-Type -AssemblyName System.Drawing.Common
}
catch {
    Add-Type -AssemblyName System.Drawing
}
try {
    Add-Type -AssemblyName System.Windows.Forms
}
catch {
}

if (-not ("GalleryRecordingNative" -as [type])) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class GalleryRecordingNative
{
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
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    private const int SW_RESTORE = 9;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_SHIFT = 0x10;
    private const byte VK_ESCAPE = 0x1B;
    private const byte VK_RETURN = 0x0D;
    private const byte VK_DOWN = 0x28;
    private const byte VK_SPACE = 0x20;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public static bool Move(IntPtr hWnd, int x, int y, int width, int height)
    {
        ShowWindow(hWnd, SW_RESTORE);
        return MoveWindow(hWnd, x, y, width, height, true);
    }

    public static RECT GetRect(IntPtr hWnd)
    {
        RECT rect;
        if (!GetWindowRect(hWnd, out rect))
        {
            rect = new RECT();
        }

        return rect;
    }

    public static void Activate(IntPtr hWnd)
    {
        ShowWindow(hWnd, SW_RESTORE);
        SetForegroundWindow(hWnd);
    }

    public static void SetTopMost(IntPtr hWnd, bool topMost)
    {
        SetWindowPos(hWnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    public static void Click(int x, int y)
    {
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    }

    public static void HoldClick(int x, int y, int holdMilliseconds)
    {
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        System.Threading.Thread.Sleep(holdMilliseconds);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    }

    public static void Wheel(int x, int y, int delta)
    {
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)delta), UIntPtr.Zero);
    }

    public static void Escape()
    {
        keybd_event(VK_ESCAPE, 0, 0, UIntPtr.Zero);
        keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    public static void Space()
    {
        keybd_event(VK_SPACE, 0, 0, UIntPtr.Zero);
        keybd_event(VK_SPACE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    public static void Down()
    {
        KeyPress(VK_DOWN);
    }

    public static void Enter()
    {
        KeyPress(VK_RETURN);
    }

    private static void KeyPress(byte virtualKey)
    {
        keybd_event(virtualKey, 0, 0, UIntPtr.Zero);
        keybd_event(virtualKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    public static void PressCtrlA()
    {
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        KeyPress(0x41);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    public static void TypeText(string text)
    {
        if (text == null)
        {
            return;
        }

        foreach (char ch in text)
        {
            short scan = VkKeyScan(ch);
            if (scan == -1)
            {
                continue;
            }

            byte virtualKey = (byte)(scan & 0xff);
            bool shift = (scan & 0x0100) != 0;
            if (shift)
            {
                keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);
            }

            KeyPress(virtualKey);

            if (shift)
            {
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }

            System.Threading.Thread.Sleep(15);
        }
    }
}
"@
}

function Get-RootElement {
    return [System.Windows.Automation.AutomationElement]::RootElement
}

function Wait-Until([scriptblock]$Probe, [int]$TimeoutSeconds, [string]$Description) {
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $value = & $Probe
        if ($null -ne $value -and $false -ne $value) {
            return $value
        }

        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $Description."
}

function Find-WindowByProcessId([int]$processId) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        try {
            if ($window.Current.NativeWindowHandle -ne 0) {
                return $window
            }
        }
        catch {
        }
    }

    return $null
}

function Find-DescendantByAutomationId($root, [string]$automationId) {
    if ($null -eq $root -or [string]::IsNullOrWhiteSpace($automationId)) {
        return $null
    }

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
        $automationId)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Find-DescendantByName($root, [string]$name) {
    if ($null -eq $root -or [string]::IsNullOrWhiteSpace($name)) {
        return $null
    }

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $name)
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

function Find-DescendantButtonByName($root, [string]$name) {
    if ($null -eq $root -or [string]::IsNullOrWhiteSpace($name)) {
        return $null
    }

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
    if ($null -eq $root) {
        return $null
    }

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        $controlType)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
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

function Find-InteractiveElementByNameInProcess([int]$processId, [string[]]$names) {
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
                try {
                    $controlType = $element.Current.ControlType
                    if ($controlType -ne [System.Windows.Automation.ControlType]::Button -and
                        $controlType -ne [System.Windows.Automation.ControlType]::CheckBox -and
                        $controlType -ne [System.Windows.Automation.ControlType]::MenuItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::ListItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::RadioButton -and
                        $controlType -ne [System.Windows.Automation.ControlType]::TabItem) {
                        continue
                    }

                    if ($element.Current.IsOffscreen) {
                        continue
                    }

                    $rect = $element.Current.BoundingRectangle
                    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
                        return $element
                    }
                }
                catch {
                }
            }
        }
    }

    return $null
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

function Wait-ModernWpfReady($window, [string]$route, [string]$artifactDir) {
    return Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf route '$route' to become ready" -Probe {
        $status = Read-ModernWpfStatusFile $artifactDir
        if ($null -ne $status -and $status.ReadyState -eq "Ready:$route") {
            return $status
        }

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

function Get-RequiredSampleAutomationId([string]$control) {
    switch ($control) {
        "TeachingTip" { return "GallerySample_TeachingTip_ShowButton" }
        "Button" { return "GallerySample_Button_PrimaryButton" }
        "CheckBox" { return "GallerySample_CheckBox_CheckBox" }
        "ComboBox" { return "GallerySample_ComboBox_ComboBox" }
        "ColorPicker" { return "GallerySample_ColorPicker_Root" }
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
        "SplitView" { return "GallerySample_SplitView_IsPaneOpenToggle" }
        "PersonPicture" { return "GallerySample_PersonPicture_PersonPicture" }
        "ParallaxView" { return "GallerySample_ParallaxView_Root" }
        "IconElement" { return "GallerySample_IconElement_SlicesIcon" }
        "ThemeShadow" { return "GallerySample_ThemeShadow_ShadowRect" }
        "TitleBar" { return "GallerySample_TitleBar_TitleBarControl" }
        "InfoBadge" { return "GallerySample_InfoBadge_InfoBadge" }
        "InfoBar" { return "GallerySample_InfoBar_InfoBar" }
        "ProgressRing" { return "GallerySample_ProgressRing_ProgressRing" }
        "PipsPager" { return "GallerySample_PipsPager_PipsPager" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_ScrollViewer" }
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
        "RadioButton" { return $true }
        "GridView" { return $true }
        "PipsPager" { return $true }
        "Pivot" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsOptionInteraction([string]$control) {
    switch ($control) {
        "Button" { return $true }
        "ColorPicker" { return $true }
        "SplitView" { return $true }
        "InfoBar" { return $true }
        "ProgressRing" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsScrollInteraction([string]$control) {
    switch ($control) {
        "ParallaxView" { return $true }
        "AnnotatedScrollBar" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsTextInteraction([string]$control) {
    return $control -eq "AutoSuggestBox"
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
    return $control -eq "RepeatButton"
}

function Get-SelectionInteractionTriggerName([string]$control) {
    switch ($control) {
        "RadioButton" { return "Default Radio Option 2" }
        "GridView" { return "Item 1" }
        "PipsPager" { return "Page 2" }
        "Pivot" { return "Unread" }
        default { return "" }
    }
}

function Get-SelectionInteractionExpectedOutputName([string]$control) {
    switch ($control) {
        "GridView" { return "You clicked Item 1." }
        "PipsPager" { return "LandscapeImage2.jpg" }
        "Pivot" { return "unread emails go here." }
        default { return "" }
    }
}

function Get-SelectionInteractionOutputAutomationId([string]$control) {
    switch ($control) {
        "GridView" { return "GallerySample_GridView_ClickOutput0" }
        default { return "" }
    }
}

function Get-ControlInteractionKind([string]$control) {
    if (Test-ControlSupportsOpenInteraction $control) { return "OpenRepeat" }
    if (Test-ControlSupportsStateInteraction $control) { return "State" }
    if (Test-ControlSupportsValueInteraction $control) { return "Value" }
    if (Test-ControlSupportsSelectionInteraction $control) { return "Selection" }
    if (Test-ControlSupportsOptionInteraction $control) { return "Option" }
    if (Test-ControlSupportsTextInteraction $control) { return "Text" }
    if (Test-ControlSupportsOutputInteraction $control) { return "Output" }
    if (Test-ControlSupportsScrollInteraction $control) { return "Scroll" }
    return "Static"
}

function Get-ElementCenter($element) {
    if ($null -eq $element) {
        return $null
    }

    try {
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return $null
        }

        return [pscustomobject]@{
            X = [int][Math]::Round($rect.X + ($rect.Width / 2.0))
            Y = [int][Math]::Round($rect.Y + ($rect.Height / 2.0))
        }
    }
    catch {
        return $null
    }
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

function Get-SelectionItemStateName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($null -ne $pattern) {
            if ($pattern.Current.IsSelected) {
                return "Selected"
            }

            return "Unselected"
        }
    }
    catch {
    }

    return ""
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

function Get-IsEnabledStateName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        if ($element.Current.IsEnabled) {
            return "Enabled"
        }

        return "Disabled"
    }
    catch {
    }

    return ""
}

function Get-NumericValue($element) {
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

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $pattern) {
            $value = 0.0
            if ([double]::TryParse(
                    $pattern.Current.Value,
                    [System.Globalization.NumberStyles]::Float,
                    [System.Globalization.CultureInfo]::InvariantCulture,
                    [ref]$value)) {
                return $value
            }
        }
    }
    catch {
    }

    return $null
}

function Get-ElementText($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern)
        if ($null -ne $pattern) {
            $text = $pattern.DocumentRange.GetText(-1)
            if ($null -ne $text) {
                return ($text -replace "[\r\n]+$", "")
            }
        }
    }
    catch {
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $pattern) {
            return $pattern.Current.Value
        }
    }
    catch {
    }

    try {
        return $element.Current.Name
    }
    catch {
        return ""
    }
}

function Get-ElementItemStatus($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $value = $element.GetCurrentPropertyValue([System.Windows.Automation.AutomationElement]::ItemStatusProperty)
        if ($null -ne $value) {
            return [string]$value
        }
    }
    catch {
    }

    return ""
}

function Invoke-SplitButtonSecondaryOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return $false
        }

        $x = [int][Math]::Round($rect.Right - [Math]::Min(12.0, [Math]::Max(6.0, $rect.Width * 0.18)))
        $y = [int][Math]::Round($rect.Y + ($rect.Height / 2.0))
        [GalleryRecordingNative]::Click($x, $y)
        Start-Sleep -Milliseconds 150
        return $true
    }
    catch {
        return $false
    }
}

function Invoke-ElementOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        if ($null -ne $pattern -and $pattern.Current.ExpandCollapseState -ne [System.Windows.Automation.ExpandCollapseState]::Expanded) {
            $pattern.Expand()
            Start-Sleep -Milliseconds 120
            return $true
        }
    }
    catch {
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            Start-Sleep -Milliseconds 120
            return $true
        }
    }
    catch {
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Toggle()
            Start-Sleep -Milliseconds 120
            return $true
        }
    }
    catch {
    }

    $center = Get-ElementCenter $element
    if ($null -ne $center) {
        [GalleryRecordingNative]::Click($center.X, $center.Y)
        Start-Sleep -Milliseconds 120
        return $true
    }

    return $false
}

function Select-ElementOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80
    $target = Find-SelectionInvokeTarget $element
    if ($null -eq $target) {
        $target = $element
    }

    try {
        $pattern = $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Select()
            Start-Sleep -Milliseconds 150
            [void](Invoke-ElementOnce $window $target)
            return $true
        }
    }
    catch {
    }

    return Invoke-ElementOnce $window $target
}

function Click-ElementOnce($element) {
    if ($null -eq $element) {
        return $false
    }

    $center = Get-ElementCenter $element
    if ($null -ne $center) {
        [GalleryRecordingNative]::Click($center.X, $center.Y)
        Start-Sleep -Milliseconds 250
        return $true
    }

    return $false
}

function Click-FirstSuggestionBelowEdit($edit) {
    if ($null -eq $edit) {
        return $false
    }

    try {
        $rect = $edit.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return $false
        }

        $x = [int][Math]::Round($rect.X + [Math]::Min(48.0, [Math]::Max(24.0, $rect.Width * 0.15)))
        $y = [int][Math]::Round($rect.Y + $rect.Height + 28.0)
        [GalleryRecordingNative]::Click($x, $y)
        Start-Sleep -Milliseconds 350
        return $true
    }
    catch {
        return $false
    }
}

function Invoke-OptionElementOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Toggle()
            Start-Sleep -Milliseconds 250
            return $true
        }
    }
    catch {
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            Start-Sleep -Milliseconds 250
            return $true
        }
    }
    catch {
    }

    try {
        $element.SetFocus()
        Start-Sleep -Milliseconds 100
        [GalleryRecordingNative]::Space()
        Start-Sleep -Milliseconds 250
        return $true
    }
    catch {
    }

    $center = Get-ElementCenter $element
    if ($null -ne $center) {
        [GalleryRecordingNative]::Click($center.X, $center.Y)
        Start-Sleep -Milliseconds 250
        return $true
    }

    return Invoke-ElementOnce $window $element
}

function Invoke-OpenElementOnce($window, [string]$control, $element) {
    if ($control -eq "SplitButton" -or $control -eq "ToggleSplitButton") {
        $invoked = Invoke-SplitButtonSecondaryOnce $window $element
        Start-Sleep -Milliseconds 150
        if ((Get-ExpandCollapseStateName $element) -ne "Expanded") {
            try {
                $pattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
                if ($null -ne $pattern) {
                    $pattern.Expand()
                    $invoked = $true
                    Start-Sleep -Milliseconds 150
                }
            }
            catch {
            }
        }

        return $invoked
    }

    return Invoke-ElementOnce $window $element
}

function Hold-Element($window, $element, [int]$milliseconds) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    $center = Get-ElementCenter $element
    if ($null -eq $center) {
        return $false
    }

    [GalleryRecordingNative]::HoldClick($center.X, $center.Y, $milliseconds)
    Start-Sleep -Milliseconds 120
    return $true
}

function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement) {
    if ($control -eq "MenuBar") {
        $trigger = Find-DescendantByAnyName $sampleElement @("File")
        if ($null -eq $trigger) {
            $trigger = Find-ElementByNameInProcess $window.Current.ProcessId @("File")
        }

        if ($null -ne $trigger) {
            $button = Find-DescendantButtonByAnyName $trigger @("File")
            if ($null -ne $button) {
                return $button
            }

            return $trigger
        }
    }

    return $sampleElement
}

function Find-OpenInteractionElement($window, $element, [string[]]$openNames, [string]$control) {
    if ($openNames.Count -eq 0) {
        return $null
    }

    if ($control -eq "SplitButton" -or $control -eq "ToggleSplitButton") {
        if ($null -eq $element -or (Get-ExpandCollapseStateName $element) -ne "Expanded") {
            return $null
        }
    }

    return Find-InteractiveElementByNameInProcess $window.Current.ProcessId $openNames
}

function Open-CommandBarFlyoutSecondaryCommands($window) {
    $moreButton = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "MoreButton"
    if ($null -eq $moreButton) {
        return $false
    }

    if (Invoke-ElementOnce $window $moreButton) {
        Start-Sleep -Milliseconds 350
        return $null -ne (Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move"))
    }

    return $false
}

function Invoke-OpenRepeatInteraction($window, [string]$control, $sampleElement) {
    $trigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    $openNames = Get-OpenInteractionNames $control
    $firstOpen = Invoke-OpenElementOnce $window $control $trigger
    Start-Sleep -Milliseconds 650
    $firstOpenExpandState = Get-ExpandCollapseStateName $trigger
    $firstOpenElementFound = $openNames.Count -eq 0 -or $null -ne (Find-OpenInteractionElement $window $trigger $openNames $control)
    $secondaryExpanded = $false
    if ($control -eq "CommandBarFlyout") {
        $secondaryExpanded = Open-CommandBarFlyoutSecondaryCommands $window
        Start-Sleep -Milliseconds 450
    }

    [GalleryRecordingNative]::Escape()
    Start-Sleep -Milliseconds 650
    $secondOpen = Invoke-OpenElementOnce $window $control $trigger
    Start-Sleep -Milliseconds 650
    $secondOpenExpandState = Get-ExpandCollapseStateName $trigger
    $secondOpenElementFound = $openNames.Count -eq 0 -or $null -ne (Find-OpenInteractionElement $window $trigger $openNames $control)
    if ($control -eq "CommandBarFlyout") {
        $secondaryExpanded = (Open-CommandBarFlyoutSecondaryCommands $window) -or $secondaryExpanded
        Start-Sleep -Milliseconds 450
    }

    return [ordered]@{
        Invoked = $firstOpen -and $secondOpen -and $firstOpenElementFound -and $secondOpenElementFound
        FirstOpen = $firstOpen
        SecondOpen = $secondOpen
        FirstOpenElementFound = $firstOpenElementFound
        SecondOpenElementFound = $secondOpenElementFound
        FirstOpenExpandState = $firstOpenExpandState
        SecondOpenExpandState = $secondOpenExpandState
        CommandBarFlyoutSecondaryExpanded = $secondaryExpanded
    }
}

function Invoke-StateInteraction($window, $sampleElement) {
    $before = Get-ToggleStateName $sampleElement
    $invoked = Invoke-ElementOnce $window $sampleElement
    Start-Sleep -Milliseconds 150
    $after = Get-ToggleStateName $sampleElement
    return [ordered]@{
        Invoked = $invoked
        BeforeState = $before
        AfterState = $after
        StateChanged = ![string]::IsNullOrWhiteSpace($before) -and $before -ne $after
    }
}

function Invoke-ValueInteraction($window, [string]$control, $sampleElement) {
    if ($null -eq $sampleElement) {
        return [ordered]@{ Invoked = $false }
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    $before = Get-NumericValue $sampleElement
    try {
        $pattern = $sampleElement.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
        if ($null -ne $pattern) {
            $target = switch ($control) {
                "RatingControl" { 3.0 }
                "Slider" { 50.0 }
                default { [double]$pattern.Current.Value + 10.0 }
            }
            $pattern.SetValue($target)
            Start-Sleep -Milliseconds 250
            $after = Get-NumericValue $sampleElement
            return [ordered]@{
                Invoked = $true
                BeforeValue = $before
                AfterValue = $after
                TargetValue = $target
                TargetReached = $null -ne $after -and [Math]::Abs(([double]$after) - ([double]$target)) -lt 0.001
            }
        }
    }
    catch {
    }

    if ($control -eq "NumberBox") {
        $increase = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Increase", "Increase value", "Up")
        $invoked = Invoke-ElementOnce $window $increase
        Start-Sleep -Milliseconds 150
        $after = Get-NumericValue $sampleElement
        return [ordered]@{
            Invoked = $invoked
            BeforeValue = $before
            AfterValue = $after
            TargetValue = $null
            TargetReached = $null -ne $before -and $null -ne $after -and [double]$after -ne [double]$before
        }
    }

    return [ordered]@{
        Invoked = $false
        BeforeValue = $before
        AfterValue = $null
        TargetValue = $null
        TargetReached = $false
    }
}

function Invoke-GridViewItemClickOnce($window) {
    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)

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
            Start-Sleep -Milliseconds 120
        }
    }
    catch {
    }

    if (!$invoked) {
        $invoked = Invoke-ElementOnce $window $target
    }

    return $invoked
}

function Invoke-SelectionInteraction($window, [string]$control, $sampleElement) {
    $name = Get-SelectionInteractionTriggerName $control
    $expectedOutputName = Get-SelectionInteractionExpectedOutputName $control
    $outputAutomationId = Get-SelectionInteractionOutputAutomationId $control
    $target = if (![string]::IsNullOrWhiteSpace($name)) {
        Find-InteractiveElementByNameInProcess $window.Current.ProcessId @($name)
    }
    else {
        $null
    }
    $outputElement = if (![string]::IsNullOrWhiteSpace($outputAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    }
    else {
        $null
    }

    $beforeSampleState = Get-SelectionItemStateName $sampleElement
    $beforeTargetState = Get-SelectionItemStateName $target
    $beforeSampleStatus = Get-ElementItemStatus $sampleElement
    $beforeOutput = Get-ElementText $outputElement
    $invoked = if ($control -eq "GridView") {
        Invoke-GridViewItemClickOnce $window
    }
    else {
        Select-ElementOnce $window $target
    }
    Start-Sleep -Milliseconds 200
    $afterSampleState = Get-SelectionItemStateName $sampleElement
    $afterTargetState = Get-SelectionItemStateName $target
    $afterSampleStatus = Get-ElementItemStatus $sampleElement
    $outputElement = if (![string]::IsNullOrWhiteSpace($outputAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    }
    else {
        $null
    }
    $afterOutput = Get-ElementText $outputElement
    $outputMatched = (![string]::IsNullOrWhiteSpace($expectedOutputName) -and $afterOutput -eq $expectedOutputName)
    if (!$outputMatched -and ![string]::IsNullOrWhiteSpace($expectedOutputName) -and $afterSampleStatus -eq $expectedOutputName) {
        $outputMatched = $true
    }

    if (!$outputMatched -and ![string]::IsNullOrWhiteSpace($expectedOutputName) -and [string]::IsNullOrWhiteSpace($outputAutomationId)) {
        $outputMatched = $null -ne (Find-ElementByNameInProcess $window.Current.ProcessId @($expectedOutputName))
    }

    return [ordered]@{
        Invoked = $invoked
        TargetName = $name
        ExpectedOutputName = $expectedOutputName
        OutputAutomationId = $outputAutomationId
        BeforeSampleSelection = $beforeSampleState
        AfterSampleSelection = $afterSampleState
        BeforeTargetSelection = $beforeTargetState
        AfterTargetSelection = $afterTargetState
        BeforeSampleStatus = $beforeSampleStatus
        AfterSampleStatus = $afterSampleStatus
        BeforeOutput = $beforeOutput
        AfterOutput = $afterOutput
        OutputChanged = ($beforeOutput -ne $afterOutput) -or ($beforeSampleStatus -ne $afterSampleStatus)
        OutputMatched = $outputMatched
        SelectionChanged = (
            (![string]::IsNullOrWhiteSpace($beforeSampleState) -and $beforeSampleState -ne $afterSampleState) -or
            (![string]::IsNullOrWhiteSpace($beforeTargetState) -and $beforeTargetState -ne $afterTargetState) -or
            ($beforeOutput -ne $afterOutput) -or
            ($beforeSampleStatus -ne $afterSampleStatus) -or
            $outputMatched)
    }
}

function Get-OptionInteractionTriggerName([string]$control) {
    switch ($control) {
        "Button" { return "Disable button" }
        "ColorPicker" { return "IsMoreButtonVisible" }
        "SplitView" { return "IsPaneOpen" }
        "InfoBar" { return "Is Open" }
        "ProgressRing" { return "Progress Options" }
        default { return "" }
    }
}

function Get-OptionInteractionTriggerAutomationId([string]$control) {
    switch ($control) {
        "SplitView" { return "GallerySample_SplitView_IsPaneOpenToggle" }
        "InfoBar" { return "GallerySample_InfoBar_IsOpenCheckBox" }
        "ProgressRing" { return "GallerySample_ProgressRing_ProgressToggle" }
        default { return "" }
    }
}

function Invoke-OptionInteraction($window, [string]$control, $sampleElement) {
    $name = Get-OptionInteractionTriggerName $control
    $automationId = Get-OptionInteractionTriggerAutomationId $control
    $target = if (![string]::IsNullOrWhiteSpace($automationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $automationId
    }
    else {
        $null
    }
    if ($null -eq $target -and ![string]::IsNullOrWhiteSpace($name)) {
        $target = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @($name)
    }

    $beforeState = Get-ToggleStateName $target
    $beforeSampleEnabled = Get-IsEnabledStateName $sampleElement
    $invoked = Invoke-OptionElementOnce $window $target
    Start-Sleep -Milliseconds 300
    $afterState = Get-ToggleStateName $target
    $afterSampleEnabled = Get-IsEnabledStateName $sampleElement

    return [ordered]@{
        Invoked = $invoked
        OptionName = $name
        OptionAutomationId = $automationId
        BeforeState = $beforeState
        AfterState = $afterState
        BeforeSampleEnabled = $beforeSampleEnabled
        AfterSampleEnabled = $afterSampleEnabled
        OptionChanged = (
            (![string]::IsNullOrWhiteSpace($beforeState) -and $beforeState -ne $afterState) -or
            (![string]::IsNullOrWhiteSpace($beforeSampleEnabled) -and $beforeSampleEnabled -ne $afterSampleEnabled))
    }
}

function Get-ScrollInteractionTargetAutomationId([string]$control) {
    switch ($control) {
        "ParallaxView" { return "GallerySample_ParallaxView_ListView" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_ScrollViewer" }
        default { return "" }
    }
}

function Get-ScrollVerticalPercent($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
        if ($null -ne $pattern) {
            $percent = $pattern.Current.VerticalScrollPercent
            if ($percent -ne [System.Windows.Automation.ScrollPattern]::NoScroll) {
                return [Math]::Round([double]$percent, 3).ToString([Globalization.CultureInfo]::InvariantCulture)
            }
        }
    }
    catch {
    }

    return ""
}

function Set-ScrollVerticalPercent($element, [double]$verticalPercent) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, $verticalPercent)
            return $true
        }
    }
    catch {
    }

    return $false
}

function Invoke-WheelScroll($window, $element, [int]$steps) {
    $center = Get-ElementCenter $element
    if ($null -eq $center) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    [GalleryRecordingNative]::Click($center.X, $center.Y)
    Start-Sleep -Milliseconds 100
    for ($i = 0; $i -lt $steps; $i++) {
        [GalleryRecordingNative]::Wheel($center.X, $center.Y, -480)
        Start-Sleep -Milliseconds 120
    }

    return $true
}

function Invoke-ScrollInteraction($window, [string]$control, $sampleElement) {
    $targetId = Get-ScrollInteractionTargetAutomationId $control
    $target = if (![string]::IsNullOrWhiteSpace($targetId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $targetId
    }
    else {
        $null
    }
    if ($null -eq $target) {
        $target = $sampleElement
    }

    $beforePercent = Get-ScrollVerticalPercent $target
    $patternInvoked = Set-ScrollVerticalPercent $target 55.0
    Start-Sleep -Milliseconds 400
    $afterPercent = Get-ScrollVerticalPercent $target

    $wheelInvoked = $false
    if ([string]::IsNullOrWhiteSpace($afterPercent) -or $beforePercent -eq $afterPercent) {
        $wheelInvoked = Invoke-WheelScroll $window $target 5
        Start-Sleep -Milliseconds 400
        $afterPercent = Get-ScrollVerticalPercent $target
    }

    return [ordered]@{
        Invoked = $patternInvoked -or $wheelInvoked
        TargetAutomationId = $targetId
        BeforeVerticalScrollPercent = $beforePercent
        AfterVerticalScrollPercent = $afterPercent
        PatternInvoked = $patternInvoked
        WheelInvoked = $wheelInvoked
        ScrollChanged = (![string]::IsNullOrWhiteSpace($beforePercent) -and $beforePercent -ne $afterPercent)
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

function Get-TextInteractionOutputAutomationId([string]$control) {
    switch ($control) {
        "AutoSuggestBox" { return "GallerySample_AutoSuggestBox_SuggestionOutput" }
        default { return "" }
    }
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

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $edit.SetFocus()
        Start-Sleep -Milliseconds 50
    }
    catch {
    }

    try {
        $rect = $edit.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            [GalleryRecordingNative]::Click(
                [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
                [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
            Start-Sleep -Milliseconds 80
            [GalleryRecordingNative]::PressCtrlA()
            Start-Sleep -Milliseconds 50
            if ("System.Windows.Forms.SendKeys" -as [type]) {
                [System.Windows.Forms.SendKeys]::SendWait($text)
                Start-Sleep -Milliseconds 250
                if ((Get-ElementText $edit) -eq $text) {
                    return $true
                }
            }

            [GalleryRecordingNative]::TypeText($text)
            Start-Sleep -Milliseconds 350
            if ((Get-ElementText $edit) -eq $text) {
                return $true
            }
        }
    }
    catch {
    }

    try {
        $pattern = $edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.SetValue("")
            Start-Sleep -Milliseconds 50
            $pattern.SetValue($text)
            Start-Sleep -Milliseconds 350
            return $true
        }
    }
    catch {
    }

    return $false
}

function Wait-ForInteractiveElementByNameInProcess([int]$processId, [string[]]$names, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $element = Find-InteractiveElementByNameInProcess $processId $names
        if ($null -ne $element) {
            return $element
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Wait-ForTextOutput([int]$processId, [string]$automationId, [string]$expectedText, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $element = Find-ElementByAutomationIdInProcess $processId $automationId
        $text = Get-ElementText $element
        if ($text -eq $expectedText) {
            return $element
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Invoke-TextInteraction($window, [string]$control, $sampleElement) {
    if ($null -eq $sampleElement) {
        return [ordered]@{ Invoked = $false }
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    $inputText = Get-TextInteractionInput $control
    $suggestionNames = Get-TextInteractionSuggestionNames $control
    $expectedOutput = Get-TextInteractionExpectedOutputName $control
    $outputAutomationId = Get-TextInteractionOutputAutomationId $control
    $outputElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    $beforeOutput = Get-ElementText $outputElement

    $typed = Set-EditableElementText $window $sampleElement $inputText
    $editElement = Find-EditableDescendant $sampleElement
    $suggestionElement = if ($typed) {
        Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId $suggestionNames 2500
    }
    else {
        $null
    }

    $suggestionInvoked = $false
    $suggestionInvokeMethod = ""
    $outputElement = $null
    if ($null -ne $suggestionElement) {
        $suggestionInvoked = Click-FirstSuggestionBelowEdit $editElement
        $suggestionInvokeMethod = "GeometryClick"
        $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200

        if ($null -eq $outputElement) {
            [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
            $sentKeys = $false
            try {
                if ("System.Windows.Forms.SendKeys" -as [type]) {
                    [System.Windows.Forms.SendKeys]::SendWait("{DOWN}{DOWN}{ENTER}")
                    $sentKeys = $true
                }
            }
            catch {
                $sentKeys = $false
            }

            if (!$sentKeys) {
                [GalleryRecordingNative]::Down()
                Start-Sleep -Milliseconds 100
                [GalleryRecordingNative]::Down()
                Start-Sleep -Milliseconds 100
                [GalleryRecordingNative]::Enter()
            }
            Start-Sleep -Milliseconds 400
            $suggestionInvoked = $true
            $suggestionInvokeMethod = "Keyboard"
            $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
        }

        if ($null -eq $outputElement) {
            $suggestionInvoked = Click-ElementOnce $suggestionElement
            $suggestionInvokeMethod = "Click"
            $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
        }

        if ($null -eq $outputElement) {
            $suggestionInvoked = Select-ElementOnce $window $suggestionElement
            $suggestionInvokeMethod = "SelectionItem"
            Start-Sleep -Milliseconds 400
            $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
        }
    }

    $outputElement = if ($suggestionInvoked -and $null -eq $outputElement) {
        Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 2500
    }
    else {
        $outputElement
    }
    if ($null -eq $outputElement) {
        $outputElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    }
    $afterOutput = Get-ElementText $outputElement

    return [ordered]@{
        Invoked = $typed -and $null -ne $suggestionElement -and $suggestionInvoked
        Typed = $typed
        InputText = $inputText
        SuggestionNames = $suggestionNames
        SuggestionFound = $null -ne $suggestionElement
        SuggestionName = $(if ($null -ne $suggestionElement) { $suggestionElement.Current.Name } else { "" })
        SuggestionInvoked = $suggestionInvoked
        SuggestionInvokeMethod = $suggestionInvokeMethod
        OutputAutomationId = $outputAutomationId
        BeforeOutput = $beforeOutput
        AfterOutput = $afterOutput
        ExpectedOutput = $expectedOutput
        OutputMatched = $afterOutput -eq $expectedOutput
    }
}

function Invoke-OutputInteraction($window, $sampleElement) {
    $output = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "GallerySample_RepeatButton_Output"
    $before = Get-ElementText $output
    $invoked = Hold-Element $window $sampleElement 700
    Start-Sleep -Milliseconds 250
    $after = Get-ElementText $output

    if ($before -eq $after) {
        try {
            $pattern = $sampleElement.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            if ($null -ne $pattern) {
                for ($i = 0; $i -lt 3 -and $before -eq $after; $i++) {
                    $pattern.Invoke()
                    $invoked = $true
                    Start-Sleep -Milliseconds 150
                    $after = Get-ElementText $output
                }
            }
        }
        catch {
        }
    }

    return [ordered]@{
        Invoked = $invoked
        BeforeOutput = $before
        AfterOutput = $after
        OutputChanged = $before -ne $after
    }
}

function Invoke-RecordedInteraction($window, [string]$control, $sampleElement) {
    $kind = Get-ControlInteractionKind $control
    switch ($kind) {
        "OpenRepeat" { return Invoke-OpenRepeatInteraction $window $control $sampleElement }
        "State" { return Invoke-StateInteraction $window $sampleElement }
        "Value" { return Invoke-ValueInteraction $window $control $sampleElement }
        "Selection" { return Invoke-SelectionInteraction $window $control $sampleElement }
        "Option" { return Invoke-OptionInteraction $window $control $sampleElement }
        "Text" { return Invoke-TextInteraction $window $control $sampleElement }
        "Output" { return Invoke-OutputInteraction $window $sampleElement }
        "Scroll" { return Invoke-ScrollInteraction $window $control $sampleElement }
        default { return [ordered]@{ Invoked = $true } }
    }
}

function Get-ExpandedCaptureRect([IntPtr]$windowHandle) {
    $rect = [GalleryRecordingNative]::GetRect($windowHandle)
    $left = [Math]::Max(0, $rect.Left - $CaptureMargin)
    $top = [Math]::Max(0, $rect.Top - $CaptureMargin)
    $right = $rect.Right + $CaptureMargin
    $bottom = $rect.Bottom + $CaptureMargin
    return [ordered]@{
        Left = [int]$left
        Top = [int]$top
        Width = [int][Math]::Max(1, $right - $left)
        Height = [int][Math]::Max(1, $bottom - $top)
    }
}

function Start-RecordingJob([int]$processId, [IntPtr]$windowHandle, [string]$outputPath, [string]$captureMode) {
    $captureRect = Get-ExpandedCaptureRect $windowHandle
    Start-Job -ScriptBlock {
        param($scriptPath, $targetProcessId, $handleValue, $left, $top, $width, $height, $output, $duration, $frameRate, $mode)
        $handle = [IntPtr]::new([int64]$handleValue)
        & $scriptPath -ProcessId $targetProcessId -WindowHandle $handle -Left $left -Top $top -Width $width -Height $height -Output $output -DurationSeconds $duration -FrameRate $frameRate -CaptureMode $mode
    } -ArgumentList $RecordWindowRenderedScript, $processId, ([int64]$windowHandle), $captureRect.Left, $captureRect.Top, $captureRect.Width, $captureRect.Height, $outputPath, $DurationSeconds, $FrameRate, $captureMode
}

function Wait-RecordingJob($job) {
    $timeout = [Math]::Max($DurationSeconds + 20, 25)
    $completed = Wait-Job -Job $job -Timeout $timeout
    if ($null -eq $completed) {
        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
        throw "Recorder did not finish within $timeout seconds."
    }

    try {
        $result = Receive-Job -Job $job -ErrorAction Stop
        return $result | Select-Object -Last 1
    }
    finally {
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }
}

function Get-FfmpegPath {
    $command = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        return ""
    }

    return $command.Source
}

function Get-FfprobePath {
    $command = Get-Command ffprobe -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        return ""
    }

    return $command.Source
}

function Get-VideoDurationSeconds([string]$videoPath) {
    $ffprobe = Get-FfprobePath
    if ([string]::IsNullOrWhiteSpace($ffprobe) -or !(Test-Path $videoPath)) {
        return $null
    }

    try {
        $output = & $ffprobe -v error -show_entries format=duration -of default=nw=1:nk=1 $videoPath 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($output)) {
            return $null
        }

        return [double]::Parse(($output | Select-Object -First 1), [Globalization.CultureInfo]::InvariantCulture)
    }
    catch {
        return $null
    }
}

function Get-RecordingExtension {
    return ".mp4"
}

function Get-ImageStats([string]$path) {
    if (!(Test-Path $path)) {
        return $null
    }

    $bitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path $path).Path)
    try {
        $width = $bitmap.Width
        $height = $bitmap.Height
        $step = [Math]::Max(1, [int][Math]::Floor([Math]::Max($width, $height) / 220.0))
        $count = 0
        $sum = 0.0
        $sumSquares = 0.0
        for ($y = 0; $y -lt $height; $y += $step) {
            for ($x = 0; $x -lt $width; $x += $step) {
                $pixel = $bitmap.GetPixel($x, $y)
                $luma = (0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B)
                $sum += $luma
                $sumSquares += ($luma * $luma)
                $count++
            }
        }

        $mean = if ($count -gt 0) { $sum / $count } else { 0.0 }
        $variance = if ($count -gt 0) { [Math]::Max(0.0, ($sumSquares / $count) - ($mean * $mean)) } else { 0.0 }
        $stdDev = [Math]::Sqrt($variance)
        return [ordered]@{
            Width = $width
            Height = $height
            Mean = [Math]::Round($mean, 3)
            StdDev = [Math]::Round($stdDev, 3)
            NonBlank = ($mean -gt 2.0 -and $stdDev -gt 1.0) -or $stdDev -gt 4.0
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Compare-ImageMeanDelta([string]$firstPath, [string]$secondPath) {
    if (!(Test-Path $firstPath) -or !(Test-Path $secondPath)) {
        return $null
    }

    $first = [System.Drawing.Bitmap]::FromFile((Resolve-Path $firstPath).Path)
    $second = [System.Drawing.Bitmap]::FromFile((Resolve-Path $secondPath).Path)
    try {
        $width = [Math]::Min($first.Width, $second.Width)
        $height = [Math]::Min($first.Height, $second.Height)
        if ($width -le 0 -or $height -le 0) {
            return $null
        }

        $step = [Math]::Max(1, [int][Math]::Floor([Math]::Max($width, $height) / 220.0))
        $sum = 0.0
        $count = 0
        for ($y = 0; $y -lt $height; $y += $step) {
            for ($x = 0; $x -lt $width; $x += $step) {
                $a = $first.GetPixel($x, $y)
                $b = $second.GetPixel($x, $y)
                $la = (0.2126 * $a.R) + (0.7152 * $a.G) + (0.0722 * $a.B)
                $lb = (0.2126 * $b.R) + (0.7152 * $b.G) + (0.0722 * $b.B)
                $sum += [Math]::Abs($la - $lb)
                $count++
            }
        }

        if ($count -eq 0) {
            return $null
        }

        return [Math]::Round($sum / $count, 3)
    }
    finally {
        $first.Dispose()
        $second.Dispose()
    }
}

function Export-PosterFrames([string]$videoPath, [string]$caseDir) {
    if ($SkipFrameExtraction) {
        return @()
    }

    $ffmpeg = Get-FfmpegPath
    if ([string]::IsNullOrWhiteSpace($ffmpeg)) {
        return @()
    }

    $frameDir = Join-Path $caseDir "frames"
    New-Item -ItemType Directory -Force -Path $frameDir | Out-Null
    $actualDuration = Get-VideoDurationSeconds $videoPath
    $effectiveDuration = if ($null -ne $actualDuration -and $actualDuration -gt 0.5) { $actualDuration } else { [double]$DurationSeconds }
    $frameSpecs = New-Object System.Collections.Generic.List[object]
    $sampleTime = 0.5
    $index = 0
    while ($sampleTime -lt ($effectiveDuration - 0.2)) {
        $frameSpecs.Add(@{
            Name = ("t{0:0000}" -f [int][Math]::Round($sampleTime * 1000.0))
            Seconds = $sampleTime
        })
        $sampleTime += 0.5
        $index++
    }

    if ($frameSpecs.Count -eq 0) {
        $frameSpecs.Add(@{
            Name = "t0000"
            Seconds = [Math]::Max(0.1, $effectiveDuration / 2.0)
        })
    }
    $frames = New-Object System.Collections.Generic.List[object]
    foreach ($spec in $frameSpecs) {
        $path = Join-Path $frameDir ($spec.Name + ".png")
        $seconds = ([double]$spec.Seconds).ToString("0.###", [Globalization.CultureInfo]::InvariantCulture)
        $output = & $ffmpeg -hide_banner -loglevel error -y -i $videoPath -ss $seconds -frames:v 1 $path 2>&1
        if ($LASTEXITCODE -ne 0 -or !(Test-Path $path)) {
            $frames.Add([ordered]@{
                Name = $spec.Name
                Path = $path
                Extracted = $false
                Error = ($output | Select-Object -Last 3) -join " "
            })
            continue
        }

        $frames.Add([ordered]@{
            Name = $spec.Name
            Path = $path
            Extracted = $true
            Stats = Get-ImageStats $path
        })
    }

    return $frames.ToArray()
}

function Get-MaxFrameDelta($frames) {
    $paths = @($frames | Where-Object { $_.Extracted } | ForEach-Object { $_.Path })
    if ($paths.Count -lt 2) {
        return $null
    }

    $deltas = New-Object System.Collections.Generic.List[double]
    for ($i = 0; $i -lt $paths.Count; $i++) {
        for ($j = $i + 1; $j -lt $paths.Count; $j++) {
            $delta = Compare-ImageMeanDelta $paths[$i] $paths[$j]
            if ($null -ne $delta) {
                $deltas.Add([double]$delta)
            }
        }
    }

    if ($deltas.Count -eq 0) {
        return $null
    }

    return [Math]::Round(($deltas | Measure-Object -Maximum).Maximum, 3)
}

function Get-NonBlankFrameCount($frames) {
    $count = 0
    foreach ($frame in $frames) {
        if ($frame.Extracted -and $null -ne $frame.Stats -and $frame.Stats.NonBlank) {
            $count++
        }
    }

    return $count
}

function Test-OpenRepeatEvidence($interactionResult) {
    if ($null -eq $interactionResult) {
        return $false
    }

    if (!$interactionResult.Contains("FirstOpenElementFound") -or
        !$interactionResult.Contains("SecondOpenElementFound")) {
        return $false
    }

    return $interactionResult.FirstOpenElementFound -and $interactionResult.SecondOpenElementFound
}

function Test-StateEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("StateChanged")) {
        return $false
    }

    return [bool]$interactionResult.StateChanged
}

function Test-ValueEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("TargetReached")) {
        return $false
    }

    return [bool]$interactionResult.TargetReached
}

function Test-SelectionEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("SelectionChanged")) {
        return $false
    }

    return [bool]$interactionResult.SelectionChanged
}

function Test-OptionEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("OptionChanged")) {
        return $false
    }

    return [bool]$interactionResult.OptionChanged
}

function Test-OutputEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("OutputChanged")) {
        return $false
    }

    return [bool]$interactionResult.OutputChanged
}

function Test-TextEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("OutputMatched")) {
        return $false
    }

    return [bool]$interactionResult.OutputMatched
}

function Test-ScrollEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("ScrollChanged")) {
        return $false
    }

    return [bool]$interactionResult.ScrollChanged
}

function Format-RelativePath([string]$path) {
    if ([string]::IsNullOrWhiteSpace($path)) {
        return ""
    }

    $fullPath = [IO.Path]::GetFullPath($path)
    if ($fullPath.StartsWith($RepoRoot, [StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($RepoRoot.Length).TrimStart('\')
    }

    return $fullPath
}

function Write-Report([string]$runDir, $results) {
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# Gallery Control Recording Audit")
    $lines.Add("")
    $lines.Add(("Generated: {0:yyyy-MM-dd HH:mm:ss zzz}" -f (Get-Date)))
    $lines.Add(("Theme: ``{0}``" -f $Theme))
    $recorders = @($results | ForEach-Object {
            if ($null -ne $_.RecorderResult -and ![string]::IsNullOrWhiteSpace($_.RecorderResult.Recorder)) {
                $_.RecorderResult.Recorder
            }
        } | Sort-Object -Unique)
    if ($recorders.Count -eq 0) {
        $lines.Add('Recorder: `Unknown`')
    }
    else {
        $lines.Add(("Recorder: ``{0}``" -f ($recorders -join ", ")))
    }
    $lines.Add(("Duration: ``{0}s`` at ``{1}fps``" -f $DurationSeconds, $FrameRate))
    $lines.Add("")
    $lines.Add("| Control | Status | Interaction | Recording | Max frame delta | Notes |")
    $lines.Add("| --- | --- | --- | --- | ---: | --- |")
    foreach ($result in $results) {
        $recording = Format-RelativePath $result.Recording
        $delta = if ($null -eq $result.MaxFrameDelta) { "" } else { $result.MaxFrameDelta.ToString([Globalization.CultureInfo]::InvariantCulture) }
        $notes = ($result.Notes -replace "\|", "\|")
        $lines.Add(("| {0} | {1} | {2} | ``{3}`` | {4} | {5} |" -f $result.Control, $result.Status, $result.InteractionKind, $recording, $delta, $notes))
    }

    $reportPath = Join-Path $runDir "report.md"
    Set-Content -Path $reportPath -Value $lines -Encoding UTF8
    return $reportPath
}

$stamp = Get-Date -Format "yyyyMMdd-HHmmss-fff"
$runDir = Join-Path (Join-Path $RepoRoot $OutputRoot) $stamp
New-Item -ItemType Directory -Force -Path $runDir | Out-Null

$extension = Get-RecordingExtension
$results = New-Object System.Collections.Generic.List[object]

foreach ($control in $Controls) {
    $caseDir = Join-Path $runDir $control
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null

    $route = "item/$control"
    $process = $null
    $recordingJob = $null
    $notes = New-Object System.Collections.Generic.List[string]
    $interactionResult = $null
    $recordingResult = $null
    $frames = @()
    $status = "Passed"
    $recordingPath = Join-Path $caseDir ("{0}-{1}{2}" -f $Theme.ToLowerInvariant(), $control.ToLowerInvariant(), $extension)
    $interactionKind = Get-ControlInteractionKind $control

    Write-Host ("Recording {0} ({1})..." -f $control, $interactionKind)

    try {
        $args = @("--visual-test", "--route", $route, "--theme", $Theme, "--visual-artifact-dir", $artifactDir)
        $process = Start-Process -FilePath $GalleryExe -ArgumentList $args -PassThru
        $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf Gallery window for $control" -Probe {
            $process.Refresh()
            if ($process.HasExited) {
                throw "ModernWpf Gallery exited while loading $control."
            }

            Find-WindowByProcessId $process.Id
        }

        [void][GalleryRecordingNative]::Move($window.Current.NativeWindowHandle, $WindowLeft, $WindowTop, $Width, $Height)
        [GalleryRecordingNative]::SetTopMost($window.Current.NativeWindowHandle, $true)
        [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
        Wait-ModernWpfReady $window $route $artifactDir | Out-Null
        Start-Sleep -Milliseconds 700

        $sampleId = Get-RequiredSampleAutomationId $control
        $sampleElement = Find-DescendantByAutomationId $window $sampleId
        if ($null -eq $sampleElement) {
            $notes.Add("Sample '$sampleId' not found; recording still captured route.")
        }

        $recordingJob = Start-RecordingJob $window.Current.ProcessId ([IntPtr]$window.Current.NativeWindowHandle) $recordingPath $CaptureMode
        Start-Sleep -Milliseconds 1500
        $interactionResult = Invoke-RecordedInteraction $window $control $sampleElement
        $process.Refresh()
        if ($process.HasExited) {
            throw "ModernWpf Gallery exited during $control interaction."
        }

        $recordingResult = Wait-RecordingJob $recordingJob
        $recordingJob = $null
        if ($null -eq $recordingResult -or !(Test-Path $recordingPath)) {
            throw "Recorder did not produce '$recordingPath'."
        }

        $frames = Export-PosterFrames $recordingPath $caseDir
        $nonBlankFrameCount = Get-NonBlankFrameCount $frames
        if ($nonBlankFrameCount -lt 2 -and !$SkipFrameExtraction) {
            $status = "Failed"
            $notes.Add("Fewer than two extracted poster frames were nonblank.")
        }

        if ($null -ne $interactionResult -and $interactionResult.Contains("Invoked") -and !$interactionResult.Invoked) {
            $status = "Failed"
            $notes.Add("Interaction could not be invoked.")
        }

        if ($control -eq "CommandBarFlyout" -and
            $null -ne $interactionResult -and
            $interactionResult.Contains("CommandBarFlyoutSecondaryExpanded") -and
            !$interactionResult.CommandBarFlyoutSecondaryExpanded) {
            $status = "Failed"
            $notes.Add("CommandBarFlyout MoreButton did not expose secondary commands during recording.")
        }
    }
    catch {
        $status = "Failed"
        $notes.Add($_.Exception.Message)
        if ($null -ne $recordingJob) {
            Stop-Job -Job $recordingJob -ErrorAction SilentlyContinue
            Remove-Job -Job $recordingJob -Force -ErrorAction SilentlyContinue
            $recordingJob = $null
        }
    }
    finally {
        try {
            if ($null -ne $process) {
                $process.Refresh()
                if (!$process.HasExited) {
                    $process.CloseMainWindow() | Out-Null
                    if (!$process.WaitForExit(3000)) {
                        $process.Kill()
                    }
                }
            }
        }
        catch {
        }
    }

    $maxFrameDelta = Get-MaxFrameDelta $frames
    $openRepeatEvidence = Test-OpenRepeatEvidence $interactionResult
    $stateEvidence = Test-StateEvidence $interactionResult
    $valueEvidence = Test-ValueEvidence $interactionResult
    $selectionEvidence = Test-SelectionEvidence $interactionResult
    $optionEvidence = Test-OptionEvidence $interactionResult
    $outputEvidence = Test-OutputEvidence $interactionResult
    $textEvidence = if ($interactionKind -eq "Text") { Test-TextEvidence $interactionResult } else { $false }
    $scrollEvidence = Test-ScrollEvidence $interactionResult
    if ($status -eq "Passed" -and $interactionKind -eq "Selection" -and !$selectionEvidence) {
        $status = "NeedsReview"
        $notes.Add("Machine-readable selection or output evidence did not change; manual frame review is required.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "State" -and !$stateEvidence) {
        $status = "Failed"
        $notes.Add("State interaction did not change the target toggle state.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Value" -and !$valueEvidence) {
        $status = "Failed"
        $notes.Add("Value interaction did not reach the expected target value.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Option" -and !$optionEvidence) {
        $status = "Failed"
        $notes.Add("Option interaction did not change the option or sample state.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Text" -and !$textEvidence) {
        $status = "Failed"
        $notes.Add("Text interaction did not expose the expected output.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Output" -and !$outputEvidence) {
        $status = "NeedsReview"
        $notes.Add("Machine-readable output text did not change; manual frame review is required.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Scroll" -and !$scrollEvidence) {
        $status = "Failed"
        $notes.Add("Scroll interaction did not change the target scroll percent.")
    }

    if ($status -eq "Passed" -and
        $interactionKind -ne "Static" -and
        $null -ne $maxFrameDelta -and
        $maxFrameDelta -lt 0.35) {
        if ($interactionKind -eq "OpenRepeat" -and $openRepeatEvidence) {
            $notes.Add("Expected open elements were detected on both opens despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "State" -and $stateEvidence) {
            $notes.Add("Before/after toggle state changed despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "Value" -and $valueEvidence) {
            $notes.Add("Target value was reached despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "Selection" -and $selectionEvidence) {
            $notes.Add("Selection or output evidence changed despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "Option" -and $optionEvidence) {
            $notes.Add("Option state changed despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "Output" -and $outputEvidence) {
            $notes.Add("Output text changed despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "Text" -and $textEvidence) {
            $notes.Add("Expected text output was detected despite low full-frame delta.")
        }
        elseif ($interactionKind -eq "Scroll" -and $scrollEvidence) {
            $notes.Add("Scroll percent changed despite low full-frame delta.")
        }
        else {
            $status = "NeedsReview"
            $notes.Add("Interactive recording produced low poster-frame delta.")
        }
    }

    $result = [ordered]@{
        Control = $control
        Theme = $Theme
        Route = $route
        Status = $status
        InteractionKind = $interactionKind
        Recording = if (Test-Path $recordingPath) { (Resolve-Path $recordingPath).Path } else { $recordingPath }
        RecorderResult = $recordingResult
        Frames = $frames
        MaxFrameDelta = $maxFrameDelta
        OpenRepeatEvidence = $openRepeatEvidence
        StateEvidence = $stateEvidence
        ValueEvidence = $valueEvidence
        SelectionEvidence = $selectionEvidence
        OptionEvidence = $optionEvidence
        OutputEvidence = $outputEvidence
        TextEvidence = $textEvidence
        ScrollEvidence = $scrollEvidence
        InteractionResult = $interactionResult
        Notes = ($notes.ToArray() -join " ")
    }
    $results.Add($result)
}

$manifestPath = Join-Path $runDir "recording-manifest.json"
$results | ConvertTo-Json -Depth 8 | Set-Content -Path $manifestPath -Encoding UTF8
$reportPath = Write-Report $runDir $results

[pscustomobject]@{
    RunDirectory = $runDir
    Manifest = $manifestPath
    Report = $reportPath
    Total = $results.Count
    Passed = @($results | Where-Object { $_.Status -eq "Passed" }).Count
    NeedsReview = @($results | Where-Object { $_.Status -eq "NeedsReview" }).Count
    Failed = @($results | Where-Object { $_.Status -eq "Failed" }).Count
}
