param(
    [string[]]$Controls = @("TeachingTip", "ColorPicker", "HyperlinkButton", "RatingControl", "RepeatButton", "ToggleButton", "DropDownButton", "SplitButton", "ToggleSplitButton", "ToggleSwitch", "NumberBox", "AutoSuggestBox", "SplitView", "PersonPicture", "IconElement", "ThemeShadow", "TitleBar", "InfoBadge", "InfoBar", "ProgressRing", "WinUIProgressBar", "AnnotatedScrollBar", "GridView", "ItemsRepeater", "BreadcrumbBar", "SelectorBar", "NavigationView", "ContentDialog", "Flyout", "Popup", "MenuBar", "MenuFlyout", "AppBarButton", "AppBarSeparator", "AppBarToggleButton", "CommandBar", "CommandBarFlyout"),
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
    [string]$WinUIReferenceRunDir,
    [switch]$Build,
    [switch]$IncludeInteractions,
    [ValidateSet("MoreButton", "Alpha", "Ring")]
    [string]$ColorPickerState = "MoreButton",
    [switch]$FailOnDifference
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

if ($ColorPickerState -eq "Alpha" -and $Controls -contains "ColorPicker") {
    $Height = [Math]::Max($Height, 900)
}

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
if ([string]::IsNullOrWhiteSpace($GalleryExe)) {
    $GalleryExe = Join-Path $RepoRoot "ModernWpf.Gallery\bin\Debug\net8.0-windows7.0\ModernWpf.Gallery.exe"
}

$WpfGalleryOnlyVisualAuditCases = @(
    "Border",
    "Button",
    "Calendar",
    "Canvas",
    "CheckBox",
    "Clipboard",
    "ComboBox",
    "DataGrid",
    "DatePicker",
    "Expander",
    "FileAndFolderDialogs",
    "Frame",
    "Grid",
    "GridSplitter",
    "GroupBox",
    "Hyperlink",
    "Image",
    "Label",
    "ListBox",
    "ListView",
    "Menu",
    "MessageBox",
    "NavigationWindow",
    "PasswordBox",
    "ProgressBar",
    "RadioButton",
    "ResizeGrip",
    "RichTextEdit",
    "Slider",
    "StackPanel",
    "TabControl",
    "TextBlock",
    "TextBox",
    "ToolTip",
    "TreeView"
)

if ($Reference -eq "InstalledWinUI3Gallery") {
    $wrongReferenceControls = @($Controls | Where-Object { $WpfGalleryOnlyVisualAuditCases -contains $_ })
    if ($wrongReferenceControls.Count -gt 0) {
        throw "Run-GalleryVisualChecks.ps1 uses the WinUI Gallery reference. WPF Gallery pages ($($wrongReferenceControls -join ', ')) must use tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases $($wrongReferenceControls -join ',') -Reference OfficialWpfGallery."
    }
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

    public static void MoveCursor(int x, int y)
    {
        SetCursorPos(x, y);
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

function Find-DescendantByNameAndControlType($root, [string]$name, $controlType) {
    $nameCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $name)
    $typeCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        $controlType)
    $condition = New-Object System.Windows.Automation.AndCondition($nameCondition, $typeCondition)
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
    if ($control -eq "CheckBox") {
        return Find-DescendantByNameAndControlType $root $name ([System.Windows.Automation.ControlType]::CheckBox)
    }
    if ($control -eq "CommandBarFlyout") {
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
        if (Test-ElementNameMatches $window $names) {
            return $window
        }

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
                        $controlType -ne [System.Windows.Automation.ControlType]::MenuItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::ListItem) {
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

function Test-AutomationElementUsable($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        if ($element.Current.IsOffscreen) {
            return $false
        }

        $rect = $element.Current.BoundingRectangle
        return $rect.Width -gt 0 -and $rect.Height -gt 0
    }
    catch {
        return $false
    }
}

function Find-ElementsByNameInProcess([int]$processId, [string[]]$names) {
    $matches = New-Object System.Collections.Generic.List[object]
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        if (Test-ElementNameMatches $window $names) {
            $matches.Add($window)
        }

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
        try {
            if ($window.Current.AutomationId -eq $automationId) {
                return $window
            }
        }
        catch {
        }

        $match = Find-DescendantByAutomationId $window $automationId
        if ($null -ne $match) {
            return $match
        }
    }

    return $null
}

function Find-TopLevelElementByNativeWindowHandleInProcess([int]$processId, [int]$nativeWindowHandle) {
    if ($nativeWindowHandle -eq 0) {
        return $null
    }

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($candidateWindow in $windows) {
        try {
            if ([int]$candidateWindow.Current.NativeWindowHandle -eq $nativeWindowHandle) {
                return $candidateWindow
            }
        }
        catch {
        }
    }

    return $null
}

function Find-ElementByAutomationIdInPopupWindows($window, [string]$automationId) {
    if ($null -eq $window) {
        return $null
    }

    $mainHandle = [int]$window.Current.NativeWindowHandle
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $window.Current.ProcessId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($candidateWindow in $windows) {
        try {
            if ([int]$candidateWindow.Current.NativeWindowHandle -eq $mainHandle) {
                continue
            }
        }
        catch {
            continue
        }

        $match = Find-DescendantByAutomationId $candidateWindow $automationId
        if (Test-AutomationElementUsable $match) {
            return $match
        }
    }

    return $null
}

function Find-CommandBarFlyoutMoreButton($window) {
    $primaryCommand = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Share", "Save", "Delete")
    if ($null -ne $primaryCommand) {
        $popupHandle = Get-ElementNativeWindowHandle $primaryCommand
        if ($popupHandle -ne [IntPtr]::Zero -and [int]$popupHandle -ne [int]$window.Current.NativeWindowHandle) {
            $popupWindow = Find-TopLevelElementByNativeWindowHandleInProcess $window.Current.ProcessId ([int]$popupHandle)
            if ($null -ne $popupWindow) {
                $moreButton = Find-DescendantByAutomationId $popupWindow "MoreButton"
                if (Test-AutomationElementUsable $moreButton) {
                    return $moreButton
                }
            }
        }
    }

    $moreButton = Find-ElementByAutomationIdInPopupWindows $window "MoreButton"
    if (Test-AutomationElementUsable $moreButton) {
        return $moreButton
    }

    return Find-ElementByAutomationIdInProcess $window.Current.ProcessId "MoreButton"
}

function Wait-ForCommandBarFlyoutPrimaryCommands($window, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $shareButton = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Share")
        $saveButton = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Save")
        $deleteButton = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Delete")
        $moreButton = Find-CommandBarFlyoutMoreButton $window
        if ($null -ne $shareButton -and
            $null -ne $saveButton -and
            $null -ne $deleteButton -and
            (Test-AutomationElementUsable $moreButton)) {
            return $moreButton
        }

        Start-Sleep -Milliseconds 50
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Wait-ForInteractiveElementByNameInProcess([int]$processId, [string[]]$names, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $element = Find-InteractiveElementByNameInProcess $processId $names
        if ($null -ne $element) {
            return $element
        }

        Start-Sleep -Milliseconds 50
    } while ((Get-Date) -lt $deadline)

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

function Set-ProgressRingDeterminateValue($window, [string]$app, [double]$value) {
    $automationId = if ($app -eq "ModernWpf") {
        "GallerySample_ProgressRing_DeterminateProgressRing"
    }
    else {
        "ProgressRing2"
    }

    $ring = TryFind-DescendantByAutomationId $window $automationId
    if ($null -eq $ring) {
        throw "$app determinate ProgressRing '$automationId' was not found."
    }

    $valueEditor = TryFind-DescendantByAutomationId $window "ProgressValue"
    if ($null -eq $valueEditor) {
        throw "$app ProgressRing value editor 'ProgressValue' was not found."
    }

    try {
        $pattern = $valueEditor.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
        if ($null -eq $pattern) {
            throw "$app ProgressRing value editor 'ProgressValue' did not expose RangeValuePattern."
        }

        $pattern.SetValue($value)
    }
    catch {
        throw "Could not set $app ProgressRing value editor 'ProgressValue' to $value. $($_.Exception.Message)"
    }

    Wait-Until -TimeoutSeconds 3 -Description "$app determinate ProgressRing '$automationId' to reach $value" -Probe {
        $current = Get-ElementNumericValue $ring
        if (Test-DoubleApproximatelyEqual $current $value) {
            return $current
        }

        return $null
    } | Out-Null
    Start-Sleep -Milliseconds 100
    return $true
}

function Set-ProgressBarDeterminateValue($window, [string]$app, [double]$value) {
    $automationId = if ($app -eq "ModernWpf") {
        "GallerySample_WinUIProgressBar_DeterminateProgressBar"
    }
    else {
        "ProgressBar2"
    }

    $progressBar = TryFind-DescendantByAutomationId $window $automationId
    if ($null -eq $progressBar) {
        throw "$app determinate ProgressBar '$automationId' was not found."
    }

    $valueEditor = TryFind-DescendantByAutomationId $window "ProgressValue"
    if ($null -eq $valueEditor) {
        throw "$app ProgressBar value editor 'ProgressValue' was not found."
    }

    try {
        $pattern = $valueEditor.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
        if ($null -eq $pattern) {
            throw "$app ProgressBar value editor 'ProgressValue' did not expose RangeValuePattern."
        }

        $pattern.SetValue($value)
    }
    catch {
        throw "Could not set $app ProgressBar value editor 'ProgressValue' to $value. $($_.Exception.Message)"
    }

    Wait-Until -TimeoutSeconds 3 -Description "$app determinate ProgressBar '$automationId' to reach $value" -Probe {
        $current = Get-ElementNumericValue $progressBar
        if (Test-DoubleApproximatelyEqual $current $value) {
            return $current
        }

        return $null
    }
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
        "IconElement" { return "GallerySample_IconElement_SlicesIcon" }
        "ImageIcon" { return "GallerySample_IconElement_ImageExample1" }
        "ThemeShadow" { return "GallerySample_ThemeShadow_ShadowRect" }
        "TitleBar" { return "GallerySample_TitleBar_TitleBarControl" }
        "InfoBadge" { return "GallerySample_InfoBadge_InfoBadge" }
        "InfoBar" { return "GallerySample_InfoBar_InfoBar" }
        "ProgressRing" { return "GallerySample_ProgressRing_DeterminateProgressRing" }
        "WinUIProgressBar" { return "GallerySample_WinUIProgressBar_DeterminateProgressBar" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar" }
        "GridView" { return "GallerySample_GridView_BasicGridView" }
        "ItemsRepeater" { return "GallerySample_ItemsRepeater_ItemsRepeater" }
        "BreadcrumbBar" { return "GallerySample_BreadcrumbBar_BreadcrumbBar" }
        "SelectorBar" { return "GallerySample_SelectorBar_SelectorBar" }
        "NavigationView" { return "GallerySample_NavigationView_NavigationView" }
        "ContentDialog" { return "GallerySample_ContentDialog_ShowButton" }
        "Flyout" { return "GallerySample_Flyout_Button" }
        "Popup" { return "GallerySample_Popup_Button" }
        "MenuBar" { return "GallerySample_MenuBar_MenuBar" }
        "MenuFlyout" { return "GallerySample_MenuFlyout_AppBarButton" }
        "AppBarButton" { return "GallerySample_AppBarButton_AppBarButton" }
        "AppBarSeparator" { return "GallerySample_AppBarSeparator_CommandBar" }
        "AppBarToggleButton" { return "GallerySample_AppBarToggleButton_AppBarToggleButton" }
        "CommandBar" { return "GallerySample_CommandBar_CommandBar" }
        "CommandBarFlyout" { return "GallerySample_CommandBarFlyout_ShowButton" }
        default { return "GallerySample_${control}_Root" }
    }
}

function Get-SampleRootAutomationId([string]$control) {
    if ($control -eq "ImageIcon") {
        return "GallerySample_IconElement_ImageExample1"
    }

    return "GallerySample_${control}_Root"
}

function Get-PrimaryCropMinimumVisibleStdDev([string]$control) {
    switch ($control) {
        "InfoBadge" { return 8.0 }
        "NavigationView" { return 45.0 }
        "AutoSuggestBox" { return 1.0 }
        "ThemeShadow" { return 4.0 }
        default { return 6.0 }
    }
}

function Test-ControlRequiresPrimaryCrop([string]$control) {
    switch ($control) {
        "AppBarButton" { return $true }
        "AppBarSeparator" { return $true }
        "AppBarToggleButton" { return $true }
        "CommandBar" { return $true }
        "ColorPicker" { return $true }
        "ContentDialog" { return $true }
        "Flyout" { return $true }
        "HyperlinkButton" { return $true }
        "InfoBadge" { return $true }
        "ItemsRepeater" { return $true }
        "ImageIcon" { return $true }
        "MenuBar" { return $true }
        "MenuFlyout" { return $true }
        "NavigationView" { return $true }
        "PersonPicture" { return $true }
        "Popup" { return $true }
        "SplitView" { return $true }
        "ThemeShadow" { return $true }
        "TeachingTip" { return $true }
        "ToggleButton" { return $true }
        "WinUIProgressBar" { return $true }
        default { return $false }
    }
}

function Get-RequiredReferencePrimaryCropSource([string]$control) {
    switch ($control) {
        "AppBarButton" { return "Button1" }
        "AppBarSeparator" { return "Control1" }
        "AppBarToggleButton" { return "Button1" }
        "CommandBar" { return "PrimaryCommandBar" }
        "ColorPicker" { return "ColorPicker editor surface" }
        "ContentDialog" { return "ShowDialog" }
        "Flyout" { return "Control1" }
        "InfoBadge" { return "InfoBadge value badge" }
        "ItemsRepeater" { return "ItemsRepeater source bar rows" }
        "ImageIcon" { return "ImageExample1" }
        "MenuBar" { return "Example1" }
        "MenuFlyout" { return "Sort" }
        "NavigationView" { return "nvSample5" }
        "Popup" { return "Show Popup (using Offset)" }
        "SplitView" { return "SplitView pane and content" }
        "TeachingTip" { return "TestButton1" }
        "ToggleButton" { return "Toggle1" }
        "WinUIProgressBar" { return "ProgressBar2" }
        default { return "" }
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
        "IconElement" { return "GallerySample_IconElement_SlicesIcon" }
        "ImageIcon" { return "GallerySample_IconElement_ImageExample1" }
        "ThemeShadow" { return "GallerySample_ThemeShadow_Root" }
        "TitleBar" { return "GallerySample_TitleBar_TitleBarControl" }
        "InfoBadge" { return "GallerySample_InfoBadge_InfoBadge" }
        "ProgressRing" { return "GallerySample_ProgressRing_DeterminateProgressRing" }
        "WinUIProgressBar" { return "GallerySample_WinUIProgressBar_DeterminateProgressBar" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar" }
        "GridView" { return "GallerySample_GridView_BasicGridView" }
        "ItemsRepeater" { return "GallerySample_ItemsRepeater_ItemsRepeater" }
        "BreadcrumbBar" { return "GallerySample_BreadcrumbBar_BreadcrumbBar" }
        "SelectorBar" { return "GallerySample_SelectorBar_SelectorBar" }
        "Flyout" { return "GallerySample_Flyout_Button" }
        "Popup" { return "GallerySample_Popup_Button" }
        "MenuBar" { return "GallerySample_MenuBar_MenuBar" }
        "MenuFlyout" { return "GallerySample_MenuFlyout_AppBarButton" }
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
        "Slider" { return "Slider1" }
        "ColorPicker" { return "ColorSpectrum" }
        "ToggleButton" { return "Toggle1" }
        "SplitButton" { return "myColorButton" }
        "ToggleSplitButton" { return "myListButton" }
        "NumberBox" { return "NumberBoxSpinButtonPlacementExample" }
        "AutoSuggestBox" { return "Control1" }
        "SplitView" { return "PaneRoot" }
        "PersonPicture" { return "" }
        "IconElement" { return "svPanel" }
        "ImageIcon" { return "ImageExample1" }
        "ThemeShadow" { return "" }
        "TitleBar" { return "TitleBarControl" }
        "ProgressRing" { return "ProgressRing2" }
        "WinUIProgressBar" { return "ProgressBar2" }
        "AnnotatedScrollBar" { return "svPanel" }
        "GridView" { return "BasicGridView" }
        "BreadcrumbBar" { return "BreadcrumbBar1" }
        "SelectorBar" { return "PART_ItemsView" }
        default { return "" }
    }
}

function Get-ReferencePrimaryName([string]$control) {
    switch ($control) {
        "CheckBox" { return "Two-state" }
        "DropDownButton" { return "Email" }
        "MenuFlyout" { return "Sort" }
        "Popup" { return "Show Popup (using Offset)" }
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
        "CommandBar" { return $true }
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
        # "Close" also names both Gallery title-bar buttons. Anchor the open
        # proof to the popup's unique heading so the separate WPF popup HWND is
        # selected instead of unrelated window chrome.
        "Popup" { return @("Simple Popup") }
        "MenuBar" { return @("New", "Open", "Save", "Exit") }
        "MenuFlyout" { return @("By rating", "By match", "By distance") }
        "DropDownButton" { return @("Send", "Reply", "Reply All") }
        "SplitButton" { return @("Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray") }
        "ToggleSplitButton" { return @("Bulleted list", "Roman numerals list") }
        "CommandBar" { return @("Settings") }
        "CommandBarFlyout" { return @("Share", "Save", "Delete", "Resize", "Move") }
        default { return @() }
    }
}

function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement) {
    switch ($control) {
        "CommandBar" {
            $trigger = Find-DescendantButtonByName $window "Open command bar"
            if ($null -ne $trigger) {
                return $trigger
            }
        }
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
        "ColorPicker" { return $true }
        "ToggleButton" { return $true }
        "ToggleSwitch" { return $true }
        "AppBarToggleButton" { return $true }
        default { return $false }
    }
}

function Get-StateInteractionOutputAutomationId([string]$app, [string]$control) {
    if ($control -ne "ToggleButton") {
        return ""
    }

    return $(if ($app -eq "ModernWpf") { "GallerySample_ToggleButton_Output" } else { "Control1Output" })
}

function Get-StateInteractionExpectedOutput([string]$control, [string]$state) {
    if ($control -ne "ToggleButton") {
        return ""
    }

    return $(if ($state -eq "On") { "On" } else { "Off" })
}

function Get-StateInteractionTarget($window, [string]$control, $element) {
    if ($control -eq "ColorPicker") {
        if ($ColorPickerState -eq "Ring") {
            return Find-DescendantByName $window "Ring"
        }

        $automationId = if ($ColorPickerState -eq "Alpha") { "alpha" } else { "moreBtn" }
        return Find-ElementByAutomationIdInProcess $window.Current.ProcessId $automationId
    }

    return $element
}

function Get-StateInteractionStateName([string]$control, $element) {
    if ($control -eq "ColorPicker" -and $ColorPickerState -eq "Ring") {
        if ($null -eq $element) {
            return ""
        }

        try {
            $pattern = $element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            return $(if ($pattern.Current.IsSelected) { "On" } else { "Off" })
        }
        catch {
            return ""
        }
    }

    return Get-ToggleStateName $element
}

function Set-StateInteractionElementState($window, [string]$control, $element, [string]$desiredState) {
    if ($control -eq "ColorPicker" -and $ColorPickerState -eq "Ring") {
        $target = if ($desiredState -eq "On") { $element } else { Find-DescendantByName $window "Box" }
        if ($null -eq $target) {
            return $false
        }

        try {
            $pattern = $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            $pattern.Select()
            Start-Sleep -Milliseconds 80
            return (Get-StateInteractionStateName $control $element) -eq $desiredState
        }
        catch {
            return $false
        }
    }

    return Set-ToggleElementState $window $element $desiredState
}

function Get-StateInteractionSettleDelayMs([string]$control) {
    switch ($control) {
        "ToggleSwitch" { return 220 }
        default { return 180 }
    }
}

function Test-StateInteractionVisual([string]$control, [string]$desiredState, [string]$cropPath) {
    if ($control -ne "ToggleSwitch" -or $desiredState -ne "On") {
        return [ordered]@{
            Passed = $true
            Notes = ""
        }
    }

    if ([string]::IsNullOrEmpty($cropPath) -or !(Test-Path $cropPath)) {
        return [ordered]@{
            Passed = $false
            Notes = "ToggleSwitch state crop was not available for thumb endpoint validation."
        }
    }

    $bitmap = [System.Drawing.Bitmap]::FromFile($cropPath)
    try {
        $blueCount = 0
        $blueMinX = $bitmap.Width
        $blueMaxX = -1
        $blueMinY = $bitmap.Height
        $blueMaxY = -1
        $blueR = 0.0
        $blueG = 0.0
        $blueB = 0.0

        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.B -gt 100 -and
                    ([int]$pixel.B - [int]$pixel.R) -gt 50 -and
                    ([int]$pixel.B - [int]$pixel.G) -gt 20) {
                    $blueCount++
                    $blueMinX = [Math]::Min($blueMinX, $x)
                    $blueMaxX = [Math]::Max($blueMaxX, $x)
                    $blueMinY = [Math]::Min($blueMinY, $y)
                    $blueMaxY = [Math]::Max($blueMaxY, $y)
                    $blueR += $pixel.R
                    $blueG += $pixel.G
                    $blueB += $pixel.B
                }
            }
        }

        if ($blueCount -lt 20 -or $blueMaxX -le $blueMinX -or $blueMaxY -le $blueMinY) {
            return [ordered]@{
                Passed = $false
                Notes = "ToggleSwitch On screenshot did not expose an accent-colored track."
            }
        }

        $blueR /= $blueCount
        $blueG /= $blueCount
        $blueB /= $blueCount
        $trackWidth = $blueMaxX - $blueMinX + 1
        $trackCenterX = ($blueMinX + $blueMaxX) / 2.0
        $centerY = [int][Math]::Round(($blueMinY + $blueMaxY) / 2.0)
        $halfBand = [Math]::Max(2, [int][Math]::Round(($blueMaxY - $blueMinY + 1) * 0.25))
        $candidateCount = 0
        $candidateX = 0.0

        for ($y = [Math]::Max(0, $centerY - $halfBand); $y -le [Math]::Min($bitmap.Height - 1, $centerY + $halfBand); $y++) {
            for ($x = $blueMinX; $x -le $blueMaxX; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                $distanceFromTrack = [Math]::Abs($pixel.R - $blueR) + [Math]::Abs($pixel.G - $blueG) + [Math]::Abs($pixel.B - $blueB)
                $luminance = (0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B)
                if ($distanceFromTrack -gt 120 -and ($luminance -lt 90 -or $luminance -gt 180)) {
                    $candidateCount++
                    $candidateX += $x
                }
            }
        }

        if ($candidateCount -lt 8) {
            return [ordered]@{
                Passed = $false
                Notes = "ToggleSwitch On screenshot did not expose a distinct thumb inside the accent track."
            }
        }

        $thumbCenterX = $candidateX / $candidateCount
        $requiredRightOfCenter = $trackCenterX + [Math]::Max(2.0, $trackWidth * 0.12)
        if ($thumbCenterX -le $requiredRightOfCenter) {
            return [ordered]@{
                Passed = $false
                Notes = "ToggleSwitch On screenshot left the thumb near x=$([Math]::Round($thumbCenterX, 1)); expected it right of x=$([Math]::Round($requiredRightOfCenter, 1))."
            }
        }

        return [ordered]@{
            Passed = $true
            Notes = ""
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Test-ControlSupportsSelectionInteraction([string]$control) {
    switch ($control) {
        "GridView" { return $true }
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
        "AppBarButton" { return $true }
        "CommandBar" { return $true }
        "RepeatButton" { return $true }
        default { return $false }
    }
}

function Get-OutputInteractionTriggerNames([string]$control) {
    switch ($control) {
        "AppBarButton" { return @("SymbolIcon") }
        "CommandBar" { return @("Add") }
        "RepeatButton" { return @("Click and hold") }
        default { return @() }
    }
}

function Get-OutputInteractionCropAutomationId([string]$control) {
    switch ($control) {
        "AppBarButton" { return "Control1Output" }
        "CommandBar" { return "SelectedOptionText" }
        "RepeatButton" { return "GallerySample_RepeatButton_Output" }
        default { return "" }
    }
}

function Get-OutputInteractionExpectedNames([string]$control) {
    switch ($control) {
        "AppBarButton" { return @("You clicked: Button1") }
        "CommandBar" { return @("You clicked: Add") }
        default { return @() }
    }
}

function Get-OutputInteractionMinimumDelta([string]$control) {
    switch ($control) {
        "RepeatButton" { return 0.5 }
        default { return 0.5 }
    }
}

function Test-OutputInteractionAllowsBlankBaseline([string]$control) {
    switch ($control) {
        "AppBarButton" { return $true }
        "CommandBar" { return $true }
        "RepeatButton" { return $true }
        default { return $false }
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
        default { return "" }
    }
}

function Get-SelectionInteractionExpectedName([string]$control) {
    switch ($control) {
        "GridView" { return "You clicked Item 1." }
        default { return "" }
    }
}

function Get-SelectionInteractionCropAutomationId([string]$control) {
    switch ($control) {
        "GridView" { return "GallerySample_GridView_ClickOutput0" }
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
        "ImageIcon" { return "IconElement" }
        "WinUIProgressBar" { return "ProgressBar" }
        default { return $control }
    }
}

function Get-ControlRouteId([string]$control, [string]$app) {
    if ($control -eq "ImageIcon") {
        return "IconElement"
    }

    if ($control -eq "WinUIProgressBar" -and $app -eq "WinUI3") {
        return "ProgressBar"
    }

    return $control
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

function Capture-ScreenBounds([int]$left, [int]$top, [int]$width, [int]$height, [string]$path) {
    $width = [Math]::Max(1, $width)
    $height = [Math]::Max(1, $height)
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        try {
            $graphics.CopyFromScreen($left, $top, 0, 0, [System.Drawing.Size]::new($width, $height))
        }
        catch {
            $copyFromScreenError = $_.Exception.Message
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $hdc = $graphics.GetHdc()
            $copied = $false
            try {
                $copied = [GalleryVisualNative]::CopyScreenSurface($hdc, $left, $top, $width, $height)
            }
            finally {
                $graphics.ReleaseHdc($hdc)
            }

            if (!$copied) {
                throw "CopyFromScreen failed and native screen bounds capture fallback failed: $copyFromScreenError"
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

function Save-RepeatButtonOutputSurfaceCrop($window, [string]$screenshot, [string]$path, $triggerElement) {
    $buttonBounds = Get-ElementWindowBounds $window $triggerElement
    if ($null -eq $buttonBounds -or !$buttonBounds.Found) {
        return [ordered]@{
            Found = $false
            Source = "RepeatButtonOutputRow"
            Screenshot = ""
            Bounds = $buttonBounds
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    # The current WinUI Gallery sample is one horizontal row: a 112x32 stock
    # RepeatButton, an 8-DIP output margin, and the natural-width click count.
    # A common 240x32 viewport measures that source surface without UIA text
    # bounds (which differ by a couple of pixels between WPF and WinUI).
    $surfaceBounds = [ordered]@{
        Found = $true
        Reason = "Derived from the current WinUI Gallery RepeatButton output-row metrics."
        X = [int]$buttonBounds.X
        Y = [int]$buttonBounds.Y
        Width = 240
        Height = 32
        ChangedSamples = 0
    }
    $savedBounds = Save-Crop $screenshot $surfaceBounds $path 0
    return [ordered]@{
        Found = $true
        Source = "RepeatButtonOutputRow"
        Screenshot = $path
        Bounds = $savedBounds
        Width = $savedBounds.Width
        Height = $savedBounds.Height
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
        ButtonBounds = $buttonBounds
    }
}

function Get-ElementScreenBounds($element, [int]$padding = 8, $referenceWindow = $null) {
    if ($null -eq $element) {
        return $null
    }

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

    $left = [int][Math]::Floor($x)
    $top = [int][Math]::Floor($y)
    $right = [int][Math]::Ceiling($x + $width)
    $bottom = [int][Math]::Ceiling($y + $height)
    if ($null -ne $referenceWindow) {
        try {
            $windowRect = [GalleryVisualNative]::GetRect($referenceWindow.Current.NativeWindowHandle)
            $windowUiaRect = $referenceWindow.Current.BoundingRectangle
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

            $left = [int][Math]::Round($windowRect.Left + (($x - $windowUiaRect.X) * $scaleX))
            $top = [int][Math]::Round($windowRect.Top + (($y - $windowUiaRect.Y) * $scaleY))
            $right = [int][Math]::Round($windowRect.Left + (($x + $width - $windowUiaRect.X) * $scaleX))
            $bottom = [int][Math]::Round($windowRect.Top + (($y + $height - $windowUiaRect.Y) * $scaleY))
        }
        catch {
        }
    }

    $left -= $padding
    $top -= $padding
    $right += $padding
    $bottom += $padding
    try {
        Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
        $virtualScreen = [System.Windows.Forms.SystemInformation]::VirtualScreen
        $left = [Math]::Max($virtualScreen.Left, $left)
        $top = [Math]::Max($virtualScreen.Top, $top)
        $right = [Math]::Min($virtualScreen.Right, $right)
        $bottom = [Math]::Min($virtualScreen.Bottom, $bottom)
    }
    catch {
    }

    if ($right -le $left -or $bottom -le $top) {
        return $null
    }

    return [ordered]@{
        Found = $true
        Reason = ""
        X = $left
        Y = $top
        Width = [Math]::Max(1, $right - $left)
        Height = [Math]::Max(1, $bottom - $top)
        ChangedSamples = 0
    }
}

function Save-ScreenElementCrop($element, [string]$path, [string]$source = "ScreenElement", [int]$padding = 8, $referenceWindow = $null) {
    $bounds = Get-ElementScreenBounds $element $padding $referenceWindow
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

    try {
        Capture-ScreenBounds $bounds.X $bounds.Y $bounds.Width $bounds.Height $path
    }
    catch {
        return [ordered]@{
            Found = $false
            Source = $source
            Screenshot = ""
            Bounds = $bounds
            Width = $bounds.Width
            Height = $bounds.Height
            NonBlank = $false
            Error = $_.Exception.Message
        }
    }

    return [ordered]@{
        Found = $true
        Source = $source
        Screenshot = $path
        Bounds = $bounds
        Width = $bounds.Width
        Height = $bounds.Height
        ChangedSamples = 0
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

function New-SplitViewReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement) {
    if ($null -eq $sampleElement) {
        return $null
    }

    $paneRoot = Find-DescendantByAutomationId $sampleElement "PaneRoot"
    $content = Find-DescendantByAutomationId $sampleElement "content"
    $paneBounds = Get-ElementWindowBounds $window $paneRoot
    $contentBounds = Get-ElementWindowBounds $window $content
    if ($null -eq $paneBounds -or $null -eq $contentBounds) {
        return $null
    }

    $right = [Math]::Max($paneBounds.X + $paneBounds.Width, $contentBounds.X + $contentBounds.Width)
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the WinUI SplitView pane and content columns to match the ModernWpf rendered SplitView artifact."
        X = $paneBounds.X
        Y = $paneBounds.Y
        Width = [Math]::Max(1, $right - $paneBounds.X)
        Height = $paneBounds.Height
        ChangedSamples = $paneBounds.ChangedSamples
    }

    $path = Join-Path $caseDir "winui3-SplitView-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "SplitView pane and content" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
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
    $modernControlArtifact = Join-Path $modernArtifactDir "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar.png"
    if (!(Test-Path $modernControlArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernControlArtifact
    $scrollPresenter = Find-DescendantByAutomationId $window "PART_ScrollPresenter"
    $scrollBounds = Get-ElementWindowBounds $window $scrollPresenter
    if ($null -eq $scrollBounds) {
        return $null
    }
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the rendered WinUI AnnotatedScrollBar from its first Azure label."
        X = [Math]::Max(0, $scrollBounds.X + $scrollBounds.Width + 5)
        Y = [Math]::Max(0, $scrollBounds.Y)
        Width = $modernSize.Width
        Height = $modernSize.Height
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-AnnotatedScrollBar-primary-control-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "AnnotatedScrollBar" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function New-ItemsRepeaterModernPrimaryCrop([string]$caseDir, $window, [string]$screenshot) {
    $repeater = Find-DescendantByAutomationId $window "GallerySample_ItemsRepeater_ItemsRepeater"
    $repeaterBounds = Get-ElementWindowBounds $window $repeater
    if ($null -eq $repeaterBounds -or !$repeaterBounds.Found) {
        return $null
    }

    # The current Gallery sample contains three 425x24 HorizontalBarTemplate
    # rows separated by the StackLayout's two 8px gaps. Crop that source-owned
    # visual instead of the WPF VisualBrush artifact, whose parent offset can
    # clip the right edge and leave a misleading mostly-blank viewbox.
    $contentInsetX = [Math]::Max(0, [int][Math]::Round(($repeaterBounds.Width - 425) / 2.0))
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the three source ItemsRepeater bar rows from live ModernWpf element bounds."
        X = $repeaterBounds.X + $contentInsetX
        Y = $repeaterBounds.Y
        Width = [Math]::Min(425, $repeaterBounds.Width)
        Height = [Math]::Min(88, $repeaterBounds.Height)
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "modernwpf-ItemsRepeater-primary-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "ItemsRepeater source bar rows" $savedBounds
    if ($null -ne $crop -and $crop.VisibleStdDev -ge (Get-PrimaryCropMinimumVisibleStdDev "ItemsRepeater")) {
        $crop["NonBlank"] = $true
        return $crop
    }

    return $null
}

function New-ItemsRepeaterReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement) {
    if ($null -eq $sampleElement) {
        return $null
    }

    $addButton = Find-DescendantByAutomationId $sampleElement "AddBtn"
    $addButtonBounds = Get-ElementWindowBounds $window $addButton
    if ($null -eq $addButtonBounds -or !$addButtonBounds.Found) {
        return $null
    }

    $paneCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Pane)
    $panes = $sampleElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, $paneCondition)
    $barHostBounds = $null
    foreach ($pane in $panes) {
        try {
            if (![string]::IsNullOrEmpty([string]$pane.Current.AutomationId)) {
                continue
            }

            $candidateBounds = Get-ElementWindowBounds $window $pane
            if ($null -ne $candidateBounds -and $candidateBounds.Found -and
                $candidateBounds.Width -ge 420 -and $candidateBounds.Width -le 440 -and
                $candidateBounds.Height -ge 88 -and
                $candidateBounds.X -lt $addButtonBounds.X -and
                $candidateBounds.Y -le $addButtonBounds.Y -and
                ($candidateBounds.Y + $candidateBounds.Height) -gt $addButtonBounds.Y) {
                $barHostBounds = $candidateBounds
                break
            }
        }
        catch {
        }
    }

    if ($null -eq $barHostBounds) {
        return $null
    }

    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the three source ItemsRepeater bar rows from the first WinUI ControlExample pane."
        X = $barHostBounds.X
        Y = $barHostBounds.Y
        Width = [Math]::Min(425, $barHostBounds.Width)
        Height = 88
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-ItemsRepeater-primary-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "ItemsRepeater source bar rows" $savedBounds
    if ($null -ne $crop -and $crop.VisibleStdDev -ge (Get-PrimaryCropMinimumVisibleStdDev "ItemsRepeater")) {
        $crop["NonBlank"] = $true
        return $crop
    }

    return $null
}

function New-IconElementReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernIconArtifact = Join-Path $modernArtifactDir "GallerySample_IconElement_SlicesIcon.png"
    if (!(Test-Path $modernIconArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernIconArtifact
    $bodyText = Find-DescendantByName $window "The ShowAsMonochrome property (true by default) will result in a solid block of the foreground color if the property is set to true and the icon is more than one color. This behavior can be ignored by setting the ShowAsMonochrome property to false."
    $bodyBounds = Get-ElementWindowBounds $window $bodyText
    if ($null -eq $bodyBounds) {
        return $null
    }

    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the rendered WinUI SlicesIcon below the first example body text."
        X = $bodyBounds.X
        Y = $bodyBounds.Y + $bodyBounds.Height + 12
        Width = $modernSize.Width
        Height = $modernSize.Height
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-IconElement-primary-icon-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "SlicesIcon" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function New-ThemeShadowModernPrimaryCrop([string]$caseDir) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernSampleArtifact = Join-Path $modernArtifactDir "GallerySample_ThemeShadow_Root.png"
    if (!(Test-Path $modernSampleArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernSampleArtifact
    $demoBodySize = [Math]::Min($modernSize.Width, $modernSize.Height)
    if ($demoBodySize -le 0) {
        return $null
    }

    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the source 272px ThemeShadow example grid from the stretched ModernWpf sample root."
        X = 0
        Y = 0
        Width = $demoBodySize
        Height = $demoBodySize
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "modernwpf-ThemeShadow-primary-demo-body-crop.png"
    $savedBounds = Save-Crop $modernSampleArtifact $bounds $path 0
    return New-RenderedArtifactCrop $path "ThemeShadow demo body" $savedBounds
}

function New-ThemeShadowReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernSampleArtifact = Join-Path $modernArtifactDir "GallerySample_ThemeShadow_Root.png"
    if (!(Test-Path $modernSampleArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernSampleArtifact
    $sampleBounds = Get-ElementWindowBounds $window $sampleElement
    $headerText = Find-DescendantByName $window "ThemeShadow applied to a Border"
    $headerBounds = Get-ElementWindowBounds $window $headerText
    if ($null -eq $sampleBounds -or !$sampleBounds.Found -or $null -eq $headerBounds -or !$headerBounds.Found) {
        return $null
    }

    # WinUI ControlExample contributes its own content-column inset outside the
    # source Example3Grid. Remove it so both crops begin at the 36px-padded grid.
    $controlExampleContentInsetX = 13
    $controlExampleContentInsetY = 12
    $contentY = $headerBounds.Y + $headerBounds.Height + 13
    $demoBodySize = [Math]::Min($modernSize.Width, $modernSize.Height)
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the WinUI ThemeShadow source example grid to match the ModernWpf demo body."
        X = $headerBounds.X + $controlExampleContentInsetX
        Y = [Math]::Max($sampleBounds.Y, $contentY) + $controlExampleContentInsetY
        Width = $demoBodySize
        Height = $demoBodySize
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-ThemeShadow-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "ThemeShadow demo body" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function New-PersonPictureReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernPrimaryArtifact = Join-Path $modernArtifactDir "GallerySample_PersonPicture_PersonPicture.png"
    if (!(Test-Path $modernPrimaryArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernPrimaryArtifact
    $sampleBounds = Get-ElementWindowBounds $window $sampleElement
    if ($null -eq $sampleBounds -or !$sampleBounds.Found) {
        return $null
    }

    $profileImageRadio = Find-DescendantByAutomationId $window "ProfileImageRadio"
    $profileImageRadioBounds = Get-ElementWindowBounds $window $profileImageRadio
    $searchRight = if ($null -ne $profileImageRadioBounds -and $profileImageRadioBounds.Found) {
        [Math]::Min($profileImageRadioBounds.X, $sampleBounds.X + 320)
    } else {
        $sampleBounds.X + 320
    }
    $searchBounds = [ordered]@{
        Found = $true
        Reason = "Searches the first example body for the rendered PersonPicture image."
        X = $sampleBounds.X
        Y = $sampleBounds.Y + 70
        Width = [Math]::Max($modernSize.Width, $searchRight - $sampleBounds.X)
        Height = 170
        ChangedSamples = 0
    }

    $bounds = Find-ColorfulContentCropBounds $screenshot $searchBounds $modernSize.Width $modernSize.Height
    if ($null -eq $bounds -or !$bounds.Found) {
        return $null
    }

    $bounds.Reason = "Cropped the WinUI PersonPicture avatar from the first example content."

    $path = Join-Path $caseDir "winui3-PersonPicture-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "PersonPicture avatar" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function Find-ColorfulContentCropBounds([string]$screenshot, $searchBounds, [int]$targetWidth, [int]$targetHeight) {
    if ($null -eq $searchBounds -or !$searchBounds.Found -or !(Test-Path $screenshot)) {
        return $null
    }

    $source = [System.Drawing.Bitmap]::FromFile($screenshot)
    try {
        $left = [Math]::Max(0, [int]$searchBounds.X)
        $top = [Math]::Max(0, [int]$searchBounds.Y)
        $right = [Math]::Min($source.Width, [int]($searchBounds.X + $searchBounds.Width))
        $bottom = [Math]::Min($source.Height, [int]($searchBounds.Y + $searchBounds.Height))
        if ($right -le $left -or $bottom -le $top) {
            return $null
        }

        $minX = $source.Width
        $minY = $source.Height
        $maxX = -1
        $maxY = -1
        $pixelCount = 0
        for ($x = $left; $x -lt $right; $x++) {
            for ($y = $top; $y -lt $bottom; $y++) {
                $color = $source.GetPixel($x, $y)
                $maxChannel = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
                $minChannel = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
                $luminance = (0.2126 * $color.R) + (0.7152 * $color.G) + (0.0722 * $color.B)
                if (($maxChannel - $minChannel) -gt 22 -and $luminance -gt 45) {
                    $minX = [Math]::Min($minX, $x)
                    $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $y)
                    $maxY = [Math]::Max($maxY, $y)
                    $pixelCount++
                }
            }
        }

        if ($pixelCount -lt 200 -or $maxX -lt $minX -or $maxY -lt $minY) {
            return $null
        }

        return [ordered]@{
            Found = $true
            Reason = "Cropped the colorful rendered content within the search bounds."
            X = [Math]::Max(0, [Math]::Min($source.Width - $targetWidth, $minX))
            Y = [Math]::Max(0, [Math]::Min($source.Height - $targetHeight, $minY))
            Width = $targetWidth
            Height = $targetHeight
            ChangedSamples = 0
        }
    }
    finally {
        $source.Dispose()
    }
}

function Find-AccentComponentBounds([string]$screenshot, $searchBounds) {
    if ($null -eq $searchBounds -or !$searchBounds.Found -or !(Test-Path $screenshot)) {
        return $null
    }

    $source = [System.Drawing.Bitmap]::FromFile($screenshot)
    try {
        $left = [Math]::Max(0, [int]$searchBounds.X)
        $top = [Math]::Max(0, [int]$searchBounds.Y)
        $right = [Math]::Min($source.Width, [int]($searchBounds.X + $searchBounds.Width))
        $bottom = [Math]::Min($source.Height, [int]($searchBounds.Y + $searchBounds.Height))
        $width = $right - $left
        $height = $bottom - $top
        if ($width -le 0 -or $height -le 0) {
            return $null
        }

        $accentPixels = New-Object 'bool[,]' $width, $height
        for ($x = 0; $x -lt $width; $x++) {
            for ($y = 0; $y -lt $height; $y++) {
                $color = $source.GetPixel($left + $x, $top + $y)
                if ($color.B -gt 140 -and
                    $color.G -gt 70 -and
                    $color.R -lt 120 -and
                    ($color.B - $color.R) -gt 70 -and
                    ($color.B - $color.G) -gt 20) {
                    $accentPixels[$x, $y] = $true
                }
            }
        }

        $visited = New-Object 'bool[,]' $width, $height
        $components = New-Object System.Collections.Generic.List[object]
        for ($startX = 0; $startX -lt $width; $startX++) {
            for ($startY = 0; $startY -lt $height; $startY++) {
                if (!$accentPixels[$startX, $startY] -or $visited[$startX, $startY]) {
                    continue
                }

                $queueX = New-Object System.Collections.Generic.Queue[int]
                $queueY = New-Object System.Collections.Generic.Queue[int]
                $queueX.Enqueue($startX)
                $queueY.Enqueue($startY)
                $visited[$startX, $startY] = $true
                $minX = $startX
                $maxX = $startX
                $minY = $startY
                $maxY = $startY
                $count = 0

                while ($queueX.Count -gt 0) {
                    $x = $queueX.Dequeue()
                    $y = $queueY.Dequeue()
                    $count++
                    $minX = [Math]::Min($minX, $x)
                    $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $y)
                    $maxY = [Math]::Max($maxY, $y)

                    for ($dx = -1; $dx -le 1; $dx++) {
                        for ($dy = -1; $dy -le 1; $dy++) {
                            if ($dx -eq 0 -and $dy -eq 0) {
                                continue
                            }

                            $nextX = $x + $dx
                            $nextY = $y + $dy
                            if ($nextX -lt 0 -or $nextY -lt 0 -or $nextX -ge $width -or $nextY -ge $height) {
                                continue
                            }

                            if ($accentPixels[$nextX, $nextY] -and !$visited[$nextX, $nextY]) {
                                $visited[$nextX, $nextY] = $true
                                $queueX.Enqueue($nextX)
                                $queueY.Enqueue($nextY)
                            }
                        }
                    }
                }

                $componentWidth = $maxX - $minX + 1
                $componentHeight = $maxY - $minY + 1
                if ($count -ge 20 -and
                    $componentWidth -ge 8 -and
                    $componentWidth -le 24 -and
                    $componentHeight -ge 8 -and
                    $componentHeight -le 24 -and
                    $minY -lt ($height * 0.65)) {
                    $components.Add([ordered]@{
                        Count = $count
                        X = $minX
                        Y = $minY
                        Width = $componentWidth
                        Height = $componentHeight
                    })
                }
            }
        }

        $component = @($components.ToArray() | Sort-Object @{ Expression = "Y"; Ascending = $true }, @{ Expression = "X"; Ascending = $true } | Select-Object -First 1)
        if ($component.Count -eq 0) {
            return $null
        }

        $match = $component[0]
        return [ordered]@{
            Found = $true
            Reason = "Found the first small badge-sized accent component inside the search bounds."
            X = $left + $match.X
            Y = $top + $match.Y
            Width = $match.Width
            Height = $match.Height
            ChangedSamples = 0
        }
    }
    finally {
        $source.Dispose()
    }
}

function New-ColorPickerModernPrimaryCrop([string]$caseDir) {
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $sourcePath = Join-Path $artifactDir "GallerySample_ColorPicker_ColorPicker.png"
    if (!(Test-Path $sourcePath)) {
        return $null
    }

    $sourceSize = Get-ImageSize $sourcePath
    $topPadding = 4
    $bottomPaddingAndMargin = 16
    if ($sourceSize.Width -le 0 -or $sourceSize.Height -le ($topPadding + $bottomPaddingAndMargin)) {
        return $null
    }

    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the ModernWpf ColorPicker editor surface inside the source-equivalent root padding and bottom margin."
        X = 0
        Y = $topPadding
        Width = $sourceSize.Width
        Height = $sourceSize.Height - $topPadding - $bottomPaddingAndMargin
        ChangedSamples = 0
    }
    $path = Join-Path $artifactDir "GallerySample_ColorPicker_ColorPicker_editor-surface.png"
    $savedBounds = Save-Crop $sourcePath $bounds $path 0
    return New-RenderedArtifactCrop $path "ColorPicker editor surface" $savedBounds
}

function New-ColorPickerReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot) {
    $elementIds = @(
        "ColorSpectrum",
        "ThirdDimensionSlider",
        "ColorRepresentationComboBox",
        "HexTextBox",
        "BlueTextBox",
        "BlueLabel"
    )
    $boundsList = @()
    $boundsById = @{}
    foreach ($elementId in $elementIds) {
        $element = Find-DescendantByAutomationId $window $elementId
        $bounds = Get-ElementWindowBounds $window $element
        if ($null -eq $bounds -or !$bounds.Found) {
            return $null
        }

        $boundsList += $bounds
        $boundsById[$elementId] = $bounds
    }

    # ComboBox UIA bounds include a four-pixel focus overhang. Anchor the
    # horizontal crop to the spectrum/slider content so the reference surface
    # matches the rendered ModernWpf editor rather than including that overhang.
    $left = $boundsById["ColorSpectrum"].X
    $top = $boundsById["ColorSpectrum"].Y
    $right = $boundsById["ThirdDimensionSlider"].X + $boundsById["ThirdDimensionSlider"].Width
    $bottom = @($boundsList | ForEach-Object { $_.Y + $_.Height } | Measure-Object -Maximum).Maximum
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the WinUI ColorPicker editor surface from stable child bounds."
        X = $left
        Y = $top
        Width = $right - $left
        Height = $bottom - $top
        ChangedSamples = 0
    }

    $path = Join-Path $caseDir "winui3-ColorPicker-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "ColorPicker editor surface" $savedBounds
    if ($null -ne $crop -and $crop.NonBlank) {
        return $crop
    }

    return $null
}

function New-InfoBadgeReferencePrimaryCrop([string]$caseDir, $window, [string]$screenshot, $sampleElement) {
    $modernArtifactDir = Join-Path $caseDir "modernwpf-artifacts"
    $modernPrimaryArtifact = Join-Path $modernArtifactDir "GallerySample_InfoBadge_InfoBadge.png"
    if (!(Test-Path $modernPrimaryArtifact)) {
        return $null
    }

    $modernSize = Get-ImageSize $modernPrimaryArtifact
    $sampleBounds = Get-ElementWindowBounds $window $sampleElement
    $referenceAccent = Find-AccentComponentBounds $screenshot $sampleBounds
    $bounds = $null
    if ($null -ne $referenceAccent) {
        $referenceCenterX = $referenceAccent.X + ($referenceAccent.Width / 2.0)
        $referenceCenterY = $referenceAccent.Y + ($referenceAccent.Height / 2.0)
        $bounds = [ordered]@{
            Found = $true
            Reason = "Cropped the first rendered WinUI InfoBadge value badge from the sample pixels."
            X = [Math]::Max($sampleBounds.X, [int][Math]::Round($referenceCenterX - ($modernSize.Width / 2.0)))
            Y = [Math]::Max($sampleBounds.Y, [int][Math]::Round($referenceCenterY - ($modernSize.Height / 2.0)))
            Width = $modernSize.Width
            Height = $modernSize.Height
            ChangedSamples = 0
        }
    }
    if ($null -eq $bounds) {
        return $null
    }

    $path = Join-Path $caseDir "winui3-InfoBadge-primary-content-crop.png"
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    $crop = New-RenderedArtifactCrop $path "InfoBadge value badge" $savedBounds
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

        if ($control -eq "ImageIcon") {
            # A detached VisualBrush retains the nested Button's parent offset
            # and clips the lower half of ImageExample1. Crop the visible UIA
            # button from the real Gallery window instead.
            $primaryCrop = $null
            $sampleCrop = $null
        }

        if ($control -eq "WinUIProgressBar") {
            # The source control is only three physical pixels high. WPF's
            # detached VisualBrush loses its one-pixel track/indicator strokes,
            # so crop the visible UIA element from the real Gallery window.
            $primaryCrop = $null
            $sampleCrop = $null
        }

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

        if ($control -eq "ColorPicker") {
            $colorPickerPrimary = New-ColorPickerModernPrimaryCrop $caseDir
            if ($null -ne $colorPickerPrimary -and $colorPickerPrimary.NonBlank) {
                $primaryCrop = $colorPickerPrimary
            }
        }

        if ($control -eq "ThemeShadow") {
            $themeShadowPrimary = New-ThemeShadowModernPrimaryCrop $caseDir
            if ($null -ne $themeShadowPrimary -and $themeShadowPrimary.NonBlank) {
                $primaryCrop = $themeShadowPrimary
            }
        }

        if ($control -eq "ItemsRepeater") {
            $itemsRepeaterPrimary = New-ItemsRepeaterModernPrimaryCrop $caseDir $window $screenshot
            if ($null -ne $itemsRepeaterPrimary -and $itemsRepeaterPrimary.NonBlank) {
                $primaryCrop = $itemsRepeaterPrimary
            }
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

        if ($null -ne $primaryResult -and $primaryResult.Found -and
            ![string]::IsNullOrWhiteSpace([string]$primaryResult.Screenshot) -and
            (Test-Path -LiteralPath $primaryResult.Screenshot) -and
            !([string]$primaryResult.Screenshot).Equals($primaryPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            Copy-Item -LiteralPath $primaryResult.Screenshot -Destination $primaryPath -Force
            $primaryResult = New-RenderedArtifactCrop $primaryPath $primaryResult.Source $primaryResult.Bounds
        }

        $sampleResult = if ($null -ne $sampleCrop) { $sampleCrop } else { Save-ElementCrop $window $screenshot $samplePath $sampleElement $sampleSource 10 }
        if ($null -ne $sampleResult -and $sampleResult.Found -and
            ![string]::IsNullOrWhiteSpace([string]$sampleResult.Screenshot) -and
            (Test-Path -LiteralPath $sampleResult.Screenshot) -and
            !([string]$sampleResult.Screenshot).Equals($samplePath, [System.StringComparison]::OrdinalIgnoreCase)) {
            Copy-Item -LiteralPath $sampleResult.Screenshot -Destination $samplePath -Force
            $sampleResult = New-RenderedArtifactCrop $samplePath $sampleResult.Source $sampleResult.Bounds
        }

        return [ordered]@{
            Primary = $primaryResult
            Sample = $sampleResult
        }
    }

    $primaryResult = Save-ElementCrop $window $screenshot $primaryPath $primaryElement $primarySource 0
    if ($control -eq "SplitView") {
        $splitViewPrimary = New-SplitViewReferencePrimaryCrop $caseDir $window $screenshot $sampleElement
        if ($null -ne $splitViewPrimary) {
            $primaryResult = $splitViewPrimary
        }
    }
    elseif ($control -eq "AnnotatedScrollBar") {
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
    elseif ($control -eq "ThemeShadow") {
        $themeShadowPrimary = New-ThemeShadowReferencePrimaryCrop $caseDir $window $screenshot $sampleElement
        if ($null -ne $themeShadowPrimary) {
            $primaryResult = $themeShadowPrimary
        }
    }
    elseif ($control -eq "PersonPicture") {
        $personPicturePrimary = New-PersonPictureReferencePrimaryCrop $caseDir $window $screenshot $sampleElement
        if ($null -ne $personPicturePrimary) {
            $primaryResult = $personPicturePrimary
        }
    }
    elseif ($control -eq "ItemsRepeater") {
        $itemsRepeaterPrimary = New-ItemsRepeaterReferencePrimaryCrop $caseDir $window $screenshot $sampleElement
        if ($null -ne $itemsRepeaterPrimary) {
            $primaryResult = $itemsRepeaterPrimary
        }
    }
    elseif ($control -eq "InfoBadge") {
        $infoBadgePrimary = New-InfoBadgeReferencePrimaryCrop $caseDir $window $screenshot $sampleElement
        if ($null -ne $infoBadgePrimary) {
            $primaryResult = $infoBadgePrimary
        }
    }
    elseif ($control -eq "ColorPicker") {
        $colorPickerPrimary = New-ColorPickerReferencePrimaryCrop $caseDir $window $screenshot
        if ($null -ne $colorPickerPrimary) {
            $primaryResult = $colorPickerPrimary
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

function Merge-WindowBounds($left, $right) {
    if ($null -eq $left -or !$left.Found) {
        return $right
    }

    if ($null -eq $right -or !$right.Found) {
        return $left
    }

    $x = [Math]::Min([int]$left.X, [int]$right.X)
    $y = [Math]::Min([int]$left.Y, [int]$right.Y)
    $rightEdge = [Math]::Max([int]$left.X + [int]$left.Width, [int]$right.X + [int]$right.Width)
    $bottomEdge = [Math]::Max([int]$left.Y + [int]$left.Height, [int]$right.Y + [int]$right.Height)
    return [ordered]@{
        Found = $true
        Reason = ""
        X = $x
        Y = $y
        Width = [Math]::Max(1, $rightEdge - $x)
        Height = [Math]::Max(1, $bottomEdge - $y)
        ChangedSamples = 0
    }
}

function Get-ContentDialogOpenSurfaceElements($window) {
    $processId = $window.Current.ProcessId
    $anchor = $null
    $anchorArea = [double]::MaxValue
    foreach ($candidate in @(Find-ElementsByNameInProcess $processId @("Save your work?"))) {
        if (!(Test-AutomationElementUsable $candidate)) {
            continue
        }

        try {
            $rect = $candidate.Current.BoundingRectangle
            $area = [double]$rect.Width * [double]$rect.Height
            if ($candidate.Current.ControlType -eq [System.Windows.Automation.ControlType]::Text -and $area -lt $anchorArea) {
                $anchor = $candidate
                $anchorArea = $area
            }
        }
        catch {
        }
    }

    if ($null -eq $anchor) {
        return @()
    }

    $anchorRect = $anchor.Current.BoundingRectangle
    $anchorCenterX = $anchorRect.X + ($anchorRect.Width / 2.0)
    $anchorCenterY = $anchorRect.Y + ($anchorRect.Height / 2.0)
    $elements = New-Object System.Collections.Generic.List[object]
    $elements.Add($anchor)
    foreach ($name in @(
        "Lorem ipsum dolor sit amet, adipisicing elit.",
        "Upload your content to the cloud.",
        "Save",
        "Don't Save",
        "Cancel")) {
        $best = $null
        $bestDistance = [double]::MaxValue
        foreach ($candidate in @(Find-ElementsByNameInProcess $processId @($name))) {
            if (!(Test-AutomationElementUsable $candidate)) {
                continue
            }

            try {
                $rect = $candidate.Current.BoundingRectangle
                $centerX = $rect.X + ($rect.Width / 2.0)
                $centerY = $rect.Y + ($rect.Height / 2.0)
                if ($centerX -lt ($anchorRect.X - 48) -or
                    $centerX -gt ($anchorRect.Right + 320) -or
                    $centerY -lt ($anchorRect.Y - 12) -or
                    $centerY -gt ($anchorRect.Bottom + 240)) {
                    continue
                }

                $distance = [Math]::Abs($centerX - $anchorCenterX) + [Math]::Abs($centerY - $anchorCenterY)
                if ($distance -lt $bestDistance) {
                    $best = $candidate
                    $bestDistance = $distance
                }
            }
            catch {
            }
        }

        if ($null -ne $best) {
            $elements.Add($best)
        }
    }

    return $elements.ToArray()
}

function Find-ImageRowTransition([System.Drawing.Bitmap]$bitmap, [int]$left, [int]$width, [int]$minimumY, [int]$maximumY) {
    $sampleLeft = [Math]::Max(0, $left + 20)
    $sampleRight = [Math]::Min($bitmap.Width - 1, $left + $width - 21)
    $minimumY = [Math]::Max(1, $minimumY)
    $maximumY = [Math]::Min($bitmap.Height - 1, $maximumY)
    $bestY = -1
    $bestDelta = -1.0
    foreach ($y in $minimumY..$maximumY) {
        $delta = 0.0
        $samples = 0
        for ($x = $sampleLeft; $x -le $sampleRight; $x += 8) {
            $above = $bitmap.GetPixel($x, $y - 1)
            $below = $bitmap.GetPixel($x, $y)
            $delta += [Math]::Abs([int]$above.R - [int]$below.R) +
                [Math]::Abs([int]$above.G - [int]$below.G) +
                [Math]::Abs([int]$above.B - [int]$below.B)
            $samples++
        }

        if ($samples -gt 0) {
            $delta /= (3.0 * $samples)
        }
        if ($delta -gt $bestDelta) {
            $bestDelta = $delta
            $bestY = $y
        }
    }

    return [ordered]@{
        Found = $bestY -ge 0
        Y = $bestY
        MeanDelta = [Math]::Round($bestDelta, 2)
    }
}

function Find-TeachingTipMarginRowTransition(
    [System.Drawing.Bitmap]$bitmap,
    [int]$left,
    [int]$width,
    [int]$minimumY,
    [int]$maximumY,
    [bool]$preferLast) {
    $minimumY = [Math]::Max(1, $minimumY)
    $maximumY = [Math]::Min($bitmap.Height - 1, $maximumY)
    $offsets = @(8, 16, 24, 32, ($width - 33), ($width - 25), ($width - 17), ($width - 9))
    $candidateY = -1
    $candidateDelta = 0.0
    foreach ($y in $minimumY..$maximumY) {
        $delta = 0.0
        $samples = 0
        foreach ($offset in $offsets) {
            $x = $left + $offset
            if ($x -lt 0 -or $x -ge $bitmap.Width) {
                continue
            }

            $above = $bitmap.GetPixel($x, $y - 1)
            $below = $bitmap.GetPixel($x, $y)
            $delta += [Math]::Abs([int]$above.R - [int]$below.R) +
                [Math]::Abs([int]$above.G - [int]$below.G) +
                [Math]::Abs([int]$above.B - [int]$below.B)
            $samples++
        }

        if ($samples -gt 0) {
            $delta /= (3.0 * $samples)
        }
        if ($delta -ge 8.0 -and ($candidateY -lt 0 -or $preferLast)) {
            $candidateY = $y
            $candidateDelta = $delta
        }
    }

    return [ordered]@{
        Found = $candidateY -ge 0
        Y = $candidateY
        MeanDelta = [Math]::Round($candidateDelta, 2)
    }
}

function Save-TeachingTipOpenSurfaceCrop(
    $window,
    $openElement,
    $triggerElement,
    [string]$baselineScreenshot,
    [string]$openScreenshot,
    [string]$path) {
    $contentRoot = if ($null -ne $openElement) {
        Find-DescendantByAutomationId $openElement "ContentRootGrid"
    }
    else {
        $null
    }
    if ($null -eq $contentRoot -and $null -ne $openElement) {
        try {
            if ($openElement.Current.AutomationId -eq "ContentRootGrid") {
                $contentRoot = $openElement
            }
        }
        catch {
        }
    }

    $contentBounds = Get-ElementWindowBounds $window $contentRoot
    $geometrySource = "UIA"
    $topTransition = $null
    $bottomTransition = $null
    $differenceBounds = $null
    if ($null -eq $contentBounds -or !$contentBounds.Found) {
        $geometrySource = "PixelTransition"
        $differenceBounds = Find-DifferenceBounds $baselineScreenshot $openScreenshot
        $targetBounds = Get-ElementWindowBounds $window $triggerElement
        if ($null -eq $differenceBounds -or !$differenceBounds.Found -or
            $null -eq $targetBounds -or !$targetBounds.Found) {
            return [ordered]@{
                Found = $false
                Source = "TeachingTipSurface"
                Screenshot = ""
                Bounds = $differenceBounds
                Width = 0
                Height = 0
                NonBlank = $false
                GeometrySource = $geometrySource
            }
        }

        $differenceCenterY = [int]$differenceBounds.Y + ([int]$differenceBounds.Height / 2.0)
        $targetCenterY = [int]$targetBounds.Y + ([int]$targetBounds.Height / 2.0)
        if ($targetCenterY -le $differenceCenterY) {
            return [ordered]@{
                Found = $false
                Source = "TeachingTipSurface"
                Screenshot = ""
                Bounds = $differenceBounds
                Width = 0
                Height = 0
                NonBlank = $false
                GeometrySource = $geometrySource
                Reason = "The audited TeachingTip sample was not placed above its target."
            }
        }

        $bitmap = [System.Drawing.Bitmap]::FromFile($openScreenshot)
        try {
            $topTransition = Find-TeachingTipMarginRowTransition `
                $bitmap `
                ([int]$differenceBounds.X) `
                ([int]$differenceBounds.Width) `
                ([int]$differenceBounds.Y - 8) `
                ([int]$differenceBounds.Y + 8) `
                $false
            $bottomTransition = Find-TeachingTipMarginRowTransition `
                $bitmap `
                ([int]$differenceBounds.X) `
                ([int]$differenceBounds.Width) `
                ([int]$targetBounds.Y - 24) `
                ([int]$targetBounds.Y - 1) `
                $true
        }
        finally {
            $bitmap.Dispose()
        }

        if (!$topTransition.Found -or !$bottomTransition.Found -or
            $bottomTransition.Y -lt $topTransition.Y) {
            return [ordered]@{
                Found = $false
                Source = "TeachingTipSurface"
                Screenshot = ""
                Bounds = $differenceBounds
                Width = 0
                Height = 0
                NonBlank = $false
                GeometrySource = $geometrySource
                TopTransition = $topTransition
                BottomTransition = $bottomTransition
            }
        }

        $contentBounds = [ordered]@{
            Found = $true
            Reason = ""
            X = [int]$differenceBounds.X
            Y = [int]$topTransition.Y
            Width = [int]$differenceBounds.Width
            Height = [int]$bottomTransition.Y - [int]$topTransition.Y + 1
            ChangedSamples = [int]$differenceBounds.ChangedSamples
        }
    }

    # Compare ContentRootGrid itself so that unrelated Gallery page pixels behind
    # the source-defined 10-DIP tail and platform-specific shadow do not dominate
    # the control delta. Tail geometry remains recorded by the source audit/tests.
    $savedBounds = Save-Crop $openScreenshot $contentBounds $path 0
    return [ordered]@{
        Found = $true
        Source = "TeachingTipSurface"
        Screenshot = $path
        Bounds = $savedBounds
        Width = $savedBounds.Width
        Height = $savedBounds.Height
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
        GeometrySource = $geometrySource
        ContentBounds = $contentBounds
        DifferenceBounds = $differenceBounds
        TopTransition = $topTransition
        BottomTransition = $bottomTransition
    }
}

function Save-PopupOpenSurfaceCrop($window, $openElement, [string]$path) {
    $titleBounds = Get-ElementScreenBounds $openElement 0 $window
    if ($null -eq $titleBounds -or !$titleBounds.Found) {
        return [ordered]@{
            Found = $false
            Source = "PopupSurface"
            Screenshot = ""
            Bounds = $titleBounds
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    # Current WinUI Gallery source defines a 240-DIP minimum-width Border with
    # 1-DIP stroke, 16-DIP padding, 8-DIP StackPanel spacing, and a 32-DIP
    # standard Close button. The unique heading's UIA rectangle therefore gives
    # stable screen-space bounds for the complete surface in both the in-window
    # WinUI Popup and WPF's separate transparent popup HWND.
    $edgeInset = 17
    $surfaceBounds = [ordered]@{
        Found = $true
        Reason = "Derived from the current WinUI Gallery Popup surface metrics."
        X = [int]$titleBounds.X - $edgeInset
        Y = [int]$titleBounds.Y - $edgeInset
        Width = 240
        Height = $edgeInset + [int]$titleBounds.Height + 8 + 32 + $edgeInset
        ChangedSamples = 0
    }

    try {
        Capture-ScreenBounds `
            $surfaceBounds.X `
            $surfaceBounds.Y `
            $surfaceBounds.Width `
            $surfaceBounds.Height `
            $path
    }
    catch {
        return [ordered]@{
            Found = $false
            Source = "PopupSurface"
            Screenshot = ""
            Bounds = $surfaceBounds
            Width = $surfaceBounds.Width
            Height = $surfaceBounds.Height
            NonBlank = $false
            Error = $_.Exception.Message
        }
    }

    return [ordered]@{
        Found = $true
        Source = "PopupSurface"
        Screenshot = $path
        Bounds = $surfaceBounds
        Width = $surfaceBounds.Width
        Height = $surfaceBounds.Height
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
        TitleBounds = $titleBounds
    }
}

function Save-FlyoutOpenSurfaceCrop($window, $openElement, [string]$path) {
    $surfaceElement = Get-PopupScreenCropElement $window $openElement
    if ($null -eq $surfaceElement) {
        return [ordered]@{
            Found = $false
            Source = "FlyoutOpenSurface"
            Screenshot = ""
            Bounds = $null
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    return Save-ScreenElementCrop $surfaceElement $path "FlyoutOpenSurface" 0 $window
}

function Save-ContentDialogOpenSurfaceCrop($window, [string]$screenshot, [string]$path) {
    $elements = @(Get-ContentDialogOpenSurfaceElements $window)
    $bounds = $null
    $elementEvidence = New-Object System.Collections.Generic.List[object]
    foreach ($element in $elements) {
        $elementBounds = Get-ElementWindowBounds $window $element
        if ($null -ne $elementBounds -and $elementBounds.Found) {
            $bounds = Merge-WindowBounds $bounds $elementBounds
            $elementEvidence.Add([ordered]@{
                Name = [string]$element.Current.Name
                AutomationId = [string]$element.Current.AutomationId
                ControlType = [string]$element.Current.ControlType.ProgrammaticName
                X = $elementBounds.X
                Y = $elementBounds.Y
                Width = $elementBounds.Width
                Height = $elementBounds.Height
            })
        }
    }

    if ($elements.Count -lt 4 -or $null -eq $bounds -or !$bounds.Found) {
        return [ordered]@{
            Found = $false
            Source = "ContentDialogSurface"
            Screenshot = ""
            Bounds = $bounds
            Width = 0
            Height = 0
            NonBlank = $false
            ElementCount = $elements.Count
        }
    }

    # Current WinUI ContentDialog source uses 24-DIP horizontal/content padding and
    # a 1-DIP border. UIA's vertical client origin differs between WPF and WinUI, so
    # snap the source-derived top/bottom search windows to the two surface edges.
    $surfaceX = [Math]::Max(0, [int]$bounds.X - 25)
    $surfaceWidth = [Math]::Max(1, [int]$bounds.Width + 50)
    $approximateTop = [int]$bounds.Y - 33
    $approximateBottom = [int]$bounds.Y + [int]$bounds.Height + 25
    $bitmap = [System.Drawing.Bitmap]::FromFile($screenshot)
    try {
        $topTransition = Find-ImageRowTransition $bitmap $surfaceX $surfaceWidth ($approximateTop - 4) ($approximateTop + 16)
        $bottomTransition = Find-ImageRowTransition $bitmap $surfaceX $surfaceWidth ($approximateBottom - 4) ($approximateBottom + 16)
    }
    finally {
        $bitmap.Dispose()
    }
    if (!$topTransition.Found -or !$bottomTransition.Found -or $bottomTransition.Y -le $topTransition.Y) {
        return [ordered]@{
            Found = $false
            Source = "ContentDialogSurface"
            Screenshot = ""
            Bounds = $bounds
            Width = 0
            Height = 0
            NonBlank = $false
            ElementCount = $elements.Count
        }
    }

    $surfaceBounds = [ordered]@{
        Found = $true
        Reason = ""
        X = $surfaceX
        Y = [int]$topTransition.Y
        Width = $surfaceWidth
        Height = [Math]::Max(1, [int]$bottomTransition.Y - [int]$topTransition.Y)
        ChangedSamples = 0
    }
    $savedBounds = Save-Crop $screenshot $surfaceBounds $path 0
    return [ordered]@{
        Found = $true
        Source = "ContentDialogSurface"
        Screenshot = $path
        Bounds = $savedBounds
        Width = $savedBounds.Width
        Height = $savedBounds.Height
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
        ElementCount = $elements.Count
        Elements = $elementEvidence.ToArray()
        RawElementBounds = $bounds
        TopTransition = $topTransition
        BottomTransition = $bottomTransition
    }
}

function Get-CommandBarFlyoutOpenSurfaceElements($window) {
    $elements = New-Object System.Collections.Generic.List[object]
    foreach ($name in @("Share", "Save", "Delete", "Resize", "Move")) {
        $element = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @($name)
        if (Test-AutomationElementUsable $element) {
            $elements.Add($element)
        }
    }

    $moreButton = Find-CommandBarFlyoutMoreButton $window
    if (Test-AutomationElementUsable $moreButton) {
        $elements.Add($moreButton)
    }

    return $elements.ToArray()
}

function Get-CommandBarFlyoutOpenSurfaceElementEvidence($window, $elements = $null) {
    $evidence = New-Object System.Collections.Generic.List[object]
    if ($null -eq $elements) {
        $elements = @(Get-CommandBarFlyoutOpenSurfaceElements $window)
    }

    foreach ($element in @($elements)) {
        try {
            $rect = $element.Current.BoundingRectangle
            $evidence.Add([ordered]@{
                Name = [string]$element.Current.Name
                AutomationId = [string]$element.Current.AutomationId
                ControlType = [string]$element.Current.ControlType.ProgrammaticName
                X = [Math]::Round($rect.X, 2)
                Y = [Math]::Round($rect.Y, 2)
                Width = [Math]::Round($rect.Width, 2)
                Height = [Math]::Round($rect.Height, 2)
            })
        }
        catch {
        }
    }

    return $evidence.ToArray()
}

function Get-CommandBarFlyoutOpenSurfaceBounds($window) {
    $bounds = $null
    foreach ($element in @(Get-CommandBarFlyoutOpenSurfaceElements $window)) {
        $elementBounds = Get-ElementWindowBounds $window $element
        if ($null -ne $elementBounds -and $elementBounds.Found) {
            $bounds = Merge-WindowBounds $bounds $elementBounds
        }
    }

    return $bounds
}

function Get-CommandBarFlyoutOpenSurfaceScreenBounds($window, $elements = $null) {
    $bounds = $null
    if ($null -eq $elements) {
        $elements = @(Get-CommandBarFlyoutOpenSurfaceElements $window)
    }

    foreach ($element in @($elements)) {
        $elementBounds = Get-ElementScreenBounds $element 0
        if ($null -ne $elementBounds -and $elementBounds.Found) {
            $bounds = Merge-WindowBounds $bounds $elementBounds
        }
    }

    return $bounds
}

function Save-CommandBarFlyoutOpenSurfaceCrop($window, [string]$screenshot, [string]$path) {
    $bounds = Get-CommandBarFlyoutOpenSurfaceBounds $window
    if ($null -eq $bounds -or !$bounds.Found) {
        return [ordered]@{
            Found = $false
            Source = "CommandBarFlyoutOpenSurface"
            Screenshot = ""
            Bounds = $bounds
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    $expandedBounds = Save-Crop $screenshot $bounds $path 6
    return [ordered]@{
        Found = $true
        Source = "CommandBarFlyoutOpenSurface"
        Screenshot = $path
        Bounds = $expandedBounds
        Width = $expandedBounds.Width
        Height = $expandedBounds.Height
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
    }
}

function Save-CommandBarFlyoutOpenSurfaceWindowCompositeCrop($window, $bounds, [string]$path, [string]$screenCaptureError) {
    if ($null -eq $bounds -or !$bounds.Found) {
        return [ordered]@{
            Found = $false
            Source = "CommandBarFlyoutOpenSurfaceScreen"
            Screenshot = ""
            Bounds = $bounds
            Width = 0
            Height = 0
            NonBlank = $false
            CaptureMethod = "WindowComposite"
            Error = $screenCaptureError
        }
    }

    $elements = @(Get-CommandBarFlyoutOpenSurfaceElements $window)
    $handles = New-Object System.Collections.Generic.List[IntPtr]
    $seenHandles = @{}
    foreach ($element in $elements) {
        $handle = Get-ElementNativeWindowHandle $element
        if ($handle -eq [IntPtr]::Zero) {
            continue
        }

        $key = $handle.ToInt64().ToString()
        if (!$seenHandles.ContainsKey($key)) {
            $seenHandles[$key] = $true
            $handles.Add($handle)
        }
    }

    if ($handles.Count -eq 0) {
        return [ordered]@{
            Found = $false
            Source = "CommandBarFlyoutOpenSurfaceScreen"
            Screenshot = ""
            Bounds = $bounds
            Width = $bounds.Width
            Height = $bounds.Height
            NonBlank = $false
            CaptureMethod = "WindowComposite"
            Error = if ([string]::IsNullOrWhiteSpace($screenCaptureError)) { "No popup window handles were found for the CommandBarFlyout open surface." } else { $screenCaptureError }
        }
    }

    $bitmap = [System.Drawing.Bitmap]::new($bounds.Width, $bounds.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $tempPaths = New-Object System.Collections.Generic.List[string]
    $drawn = $false
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        for ($index = 0; $index -lt $handles.Count; $index++) {
            $handle = $handles[$index]
            $tempPath = Join-Path (Split-Path $path -Parent) ("{0}-window-{1}.png" -f [System.IO.Path]::GetFileNameWithoutExtension($path), $index)
            $tempPaths.Add($tempPath)
            try {
                Capture-Window $handle $tempPath -SkipActivate
                if (!(Test-Path $tempPath)) {
                    continue
                }

                $windowRect = [GalleryVisualNative]::GetRect($handle)
                $sourceLeft = [Math]::Max([int]$bounds.X, [int]$windowRect.Left)
                $sourceTop = [Math]::Max([int]$bounds.Y, [int]$windowRect.Top)
                $sourceRight = [Math]::Min([int]$bounds.X + [int]$bounds.Width, [int]$windowRect.Right)
                $sourceBottom = [Math]::Min([int]$bounds.Y + [int]$bounds.Height, [int]$windowRect.Bottom)
                if ($sourceRight -le $sourceLeft -or $sourceBottom -le $sourceTop) {
                    continue
                }

                $windowBitmap = [System.Drawing.Bitmap]::FromFile($tempPath)
                try {
                    $sourceRect = [System.Drawing.Rectangle]::new(
                        $sourceLeft - [int]$windowRect.Left,
                        $sourceTop - [int]$windowRect.Top,
                        $sourceRight - $sourceLeft,
                        $sourceBottom - $sourceTop)
                    $destinationRect = [System.Drawing.Rectangle]::new(
                        $sourceLeft - [int]$bounds.X,
                        $sourceTop - [int]$bounds.Y,
                        $sourceRight - $sourceLeft,
                        $sourceBottom - $sourceTop)
                    $graphics.DrawImage($windowBitmap, $destinationRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
                    $drawn = $true
                }
                finally {
                    $windowBitmap.Dispose()
                }
            }
            catch {
            }
        }

        if ($drawn) {
            $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        }
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
        foreach ($tempPath in $tempPaths) {
            try {
                Remove-Item -LiteralPath $tempPath -Force -ErrorAction SilentlyContinue
            }
            catch {
            }
        }
    }

    if (!$drawn -or !(Test-Path $path)) {
        return [ordered]@{
            Found = $false
            Source = "CommandBarFlyoutOpenSurfaceScreen"
            Screenshot = ""
            Bounds = $bounds
            Width = $bounds.Width
            Height = $bounds.Height
            NonBlank = $false
            CaptureMethod = "WindowComposite"
            Error = if ([string]::IsNullOrWhiteSpace($screenCaptureError)) { "No popup window pixels intersected the CommandBarFlyout open surface bounds." } else { $screenCaptureError }
        }
    }

    return [ordered]@{
        Found = $true
        Source = "CommandBarFlyoutOpenSurfaceScreen"
        Screenshot = $path
        Bounds = $bounds
        Width = $bounds.Width
        Height = $bounds.Height
        ChangedSamples = 0
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
        CaptureMethod = "WindowComposite"
        Error = $screenCaptureError
    }
}

function Save-CommandBarFlyoutOpenSurfaceScreenCrop($window, [string]$path) {
    $elements = @(Get-CommandBarFlyoutOpenSurfaceElements $window)
    $bounds = Get-CommandBarFlyoutOpenSurfaceScreenBounds $window $elements
    $elementEvidence = @(Get-CommandBarFlyoutOpenSurfaceElementEvidence $window $elements)
    if ($null -eq $bounds -or !$bounds.Found) {
        return [ordered]@{
            Found = $false
            Source = "CommandBarFlyoutOpenSurfaceScreen"
            Screenshot = ""
            Bounds = $bounds
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    $left = [int]$bounds.X - 6
    $top = [int]$bounds.Y - 6
    $right = [int]$bounds.X + [int]$bounds.Width + 6
    $bottom = [int]$bounds.Y + [int]$bounds.Height + 6
    try {
        Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
        $virtualScreen = [System.Windows.Forms.SystemInformation]::VirtualScreen
        $left = [Math]::Max($virtualScreen.Left, $left)
        $top = [Math]::Max($virtualScreen.Top, $top)
        $right = [Math]::Min($virtualScreen.Right, $right)
        $bottom = [Math]::Min($virtualScreen.Bottom, $bottom)
    }
    catch {
    }

    $expandedBounds = [ordered]@{
        Found = $true
        Reason = ""
        X = $left
        Y = $top
        Width = [Math]::Max(1, $right - $left)
        Height = [Math]::Max(1, $bottom - $top)
        ChangedSamples = 0
    }

    try {
        Capture-ScreenBounds $expandedBounds.X $expandedBounds.Y $expandedBounds.Width $expandedBounds.Height $path
    }
    catch {
        return Save-CommandBarFlyoutOpenSurfaceWindowCompositeCrop $window $expandedBounds $path $_.Exception.Message
    }

    $result = [ordered]@{
        Found = $true
        Source = "CommandBarFlyoutOpenSurfaceScreen"
        Screenshot = $path
        Bounds = $expandedBounds
        Width = $expandedBounds.Width
        Height = $expandedBounds.Height
        ChangedSamples = 0
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
        CaptureMethod = "ScreenBounds"
        RawElementBounds = $bounds
        Elements = $elementEvidence
    }

    if (Test-ScreenElementPopupCropHasContent $result) {
        return $result
    }

    return Save-CommandBarFlyoutOpenSurfaceWindowCompositeCrop `
        $window `
        $expandedBounds `
        $path `
        "Screen bounds capture did not contain enough visible CommandBarFlyout content."
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

function Invoke-PopupElementFocusOnce($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $element.SetFocus()
        Start-Sleep -Milliseconds 50
        [GalleryVisualNative]::PressSpace()
        Start-Sleep -Milliseconds 120
        return $true
    }
    catch {
    }

    return $false
}

function Invoke-PopupElementClickOnce($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            [GalleryVisualNative]::Click(
                [int][Math]::Round($rect.X + ($rect.Width / 2.0)),
                [int][Math]::Round($rect.Y + ($rect.Height / 2.0)))
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

function Compare-ImagesOnCommonCanvas([string]$leftPath, [string]$rightPath) {
    $left = [System.Drawing.Bitmap]::FromFile($leftPath)
    $right = [System.Drawing.Bitmap]::FromFile($rightPath)
    try {
        $leftBackground = $left.GetPixel(0, 0)
        $rightBackground = $right.GetPixel(0, 0)
        $best = $null
        foreach ($offsetX in -1..1) {
            foreach ($offsetY in -1..1) {
                $leftX = [Math]::Max(0, -$offsetX)
                $leftY = [Math]::Max(0, -$offsetY)
                $rightX = [Math]::Max(0, $offsetX)
                $rightY = [Math]::Max(0, $offsetY)
                $width = [Math]::Max(1, [Math]::Max($leftX + $left.Width, $rightX + $right.Width))
                $height = [Math]::Max(1, [Math]::Max($leftY + $left.Height, $rightY + $right.Height))
                $samples = 0
                $delta = 0.0
                $stepX = [Math]::Max(1, [int]($width / 80))
                $stepY = [Math]::Max(1, [int]($height / 80))
                for ($x = 0; $x -lt $width; $x += $stepX) {
                    for ($y = 0; $y -lt $height; $y += $stepY) {
                        $leftPixelX = $x - $leftX
                        $leftPixelY = $y - $leftY
                        $rightPixelX = $x - $rightX
                        $rightPixelY = $y - $rightY
                        $a = if ($leftPixelX -ge 0 -and $leftPixelY -ge 0 -and $leftPixelX -lt $left.Width -and $leftPixelY -lt $left.Height) {
                            $left.GetPixel($leftPixelX, $leftPixelY)
                        }
                        else {
                            $leftBackground
                        }
                        $b = if ($rightPixelX -ge 0 -and $rightPixelY -ge 0 -and $rightPixelX -lt $right.Width -and $rightPixelY -lt $right.Height) {
                            $right.GetPixel($rightPixelX, $rightPixelY)
                        }
                        else {
                            $rightBackground
                        }
                        $delta += ([Math]::Abs($a.R - $b.R) + [Math]::Abs($a.G - $b.G) + [Math]::Abs($a.B - $b.B)) / 3.0
                        $samples++
                    }
                }

                $meanDelta = $delta / [Math]::Max(1, $samples)
                if ($null -eq $best -or $meanDelta -lt $best.MeanDelta) {
                    $best = [ordered]@{
                        MeanDelta = $meanDelta
                        Width = $width
                        Height = $height
                        OffsetX = $offsetX
                        OffsetY = $offsetY
                    }
                }
            }
        }

        return [ordered]@{
            Comparable = $true
            Reason = ""
            MeanDelta = [Math]::Round($best.MeanDelta, 2)
            NormalizedWidth = $best.Width
            NormalizedHeight = $best.Height
            AlignmentX = $best.OffsetX
            AlignmentY = $best.OffsetY
        }
    }
    finally {
        $left.Dispose()
        $right.Dispose()
    }
}

function Toggle-ElementPatternOnce($window, $element) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Toggle()
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
        if ($null -eq $element -or (Get-ExpandCollapseStateName $element) -ne "Expanded") {
            return $null
        }

        return Find-InteractiveElementByNameInProcess $window.Current.ProcessId $openNames
    }

    if ($control -eq "CommandBar") {
        foreach ($match in @(Find-ElementsByNameInProcess $window.Current.ProcessId $openNames)) {
            try {
                $automationId = [string]$match.Current.AutomationId
                if ($match.Current.ControlType -eq [System.Windows.Automation.ControlType]::Button -and
                    ($automationId -eq "GallerySample_CommandBar_SettingsButton" -or
                     $automationId -eq "settingsButton")) {
                    return $match
                }
            }
            catch {
            }
        }

        return $null
    }

    return Find-ElementByNameInProcess $window.Current.ProcessId $openNames
}

function Test-ControlPrefersScreenOpenCapture([string]$control) {
    switch ($control) {
        "TeachingTip" { return $true }
        "CommandBar" { return $true }
        "CommandBarFlyout" { return $true }
        "MenuBar" { return $true }
        "MenuFlyout" { return $true }
        default { return $false }
    }
}

function Test-ControlRequiresPopupWindowOpenProof([string]$control) {
    switch ($control) {
        "CommandBarFlyout" { return $true }
        "CommandBar" { return $true }
        "Flyout" { return $true }
        "MenuFlyout" { return $true }
        "Popup" { return $true }
        "DropDownButton" { return $true }
        "SplitButton" { return $true }
        "ToggleSplitButton" { return $true }
        default { return $false }
    }
}

function Test-ControlRequiresReferenceInteractionCropParity([string]$control) {
    switch ($control) {
        "AppBarButton" { return $true }
        "AppBarToggleButton" { return $true }
        "ColorPicker" { return $true }
        "CommandBar" { return $true }
        "CommandBarFlyout" { return $true }
        "ContentDialog" { return $true }
        "Flyout" { return $true }
        "GridView" { return $true }
        "MenuBar" { return $true }
        "MenuFlyout" { return $true }
        "Popup" { return $true }
        "RepeatButton" { return $true }
        "TeachingTip" { return $true }
        "ToggleButton" { return $true }
        default { return $false }
    }
}

function Get-ReferencePrimaryCropMeanDeltaThreshold([string]$control) {
    switch ($control) {
        "AppBarButton" { return 5.0 }
        "AppBarSeparator" { return 1.0 }
        "AppBarToggleButton" { return 5.0 }
        "AnnotatedScrollBar" { return 1.5 }
        "AutoSuggestBox" { return 0.1 }
        "BreadcrumbBar" { return 3.0 }
        "CommandBar" { return 2.5 }
        "CommandBarFlyout" { return 6.0 }
        "ColorPicker" { return 4.0 }
        "ContentDialog" { return 4.0 }
        "Flyout" { return 3.0 }
        "DropDownButton" { return 4.0 }
        "GridView" { return 2.0 }
        "HyperlinkButton" { return 1.6 }
        "InfoBadge" { return 5.0 }
        "InfoBar" { return 2.0 }
        "IconElement" { return 0.1 }
        "ImageIcon" { return 2.0 }
        "ItemsRepeater" { return 1.0 }
        "MenuBar" { return 3.0 }
        "MenuFlyout" { return 1.0 }
        "NavigationView" { return 1.2 }
        "NumberBox" { return 2.5 }
        "PersonPicture" { return 0.5 }
        "Popup" { return 4.0 }
        "ProgressRing" { return 1.0 }
        "WinUIProgressBar" { return 2.0 }
        "RatingControl" { return 7.0 }
        "RepeatButton" { return 4.0 }
        "SelectorBar" { return 3.0 }
        "SplitButton" { return 1.0 }
        "SplitView" { return 4.0 }
        "ThemeShadow" { return 0.3 }
        "TeachingTip" { return 4.0 }
        "ToggleButton" { return 3.0 }
        "ToggleSplitButton" { return 2.0 }
        "ToggleSwitch" { return 1.5 }
        "TitleBar" { return 1.0 }
        default { return 24.0 }
    }
}

function Get-ReferencePrimaryCropSizeDeltaThreshold([string]$control) {
    switch ($control) {
        "AppBarButton" { return 0 }
        "AppBarSeparator" { return 0 }
        "AppBarToggleButton" { return 0 }
        "AnnotatedScrollBar" { return 0 }
        "AutoSuggestBox" { return 0 }
        "CommandBar" { return 0 }
        "CommandBarFlyout" { return 2 }
        "ColorPicker" { return 0 }
        "ContentDialog" { return 0 }
        "Flyout" { return 0 }
        "GridView" { return 0 }
        "HyperlinkButton" { return 0 }
        "InfoBadge" { return 0 }
        "InfoBar" { return 0 }
        "ImageIcon" { return 0 }
        "ItemsRepeater" { return 0 }
        "MenuBar" { return 0 }
        "MenuFlyout" { return 0 }
        "NavigationView" { return 0 }
        "NumberBox" { return 0 }
        "PersonPicture" { return 0 }
        "Popup" { return 0 }
        "ProgressRing" { return 0 }
        "RatingControl" { return 0 }
        "RepeatButton" { return 0 }
        "SplitButton" { return 0 }
        "SplitView" { return 0 }
        "ThemeShadow" { return 0 }
        "TeachingTip" { return 0 }
        "TitleBar" { return 0 }
        "ToggleButton" { return 0 }
        "ToggleSplitButton" { return 0 }
        "ToggleSwitch" { return 0 }
        "WinUIProgressBar" { return 0 }
        default { return 24 }
    }
}

function Get-ReferenceInteractionCropMeanDeltaThreshold([string]$control) {
    switch ($control) {
        "AppBarButton" { return 7.0 }
        "AppBarToggleButton" { return 3.0 }
        "ColorPicker" { return 4.0 }
        "CommandBar" { return 2.5 }
        "CommandBarFlyout" { return 9.0 }
        "ContentDialog" { return 7.0 }
        "Flyout" { return 11.0 }
        "GridView" { return 8.0 }
        "MenuBar" { return 9.0 }
        "MenuFlyout" { return 8.0 }
        "NumberBox" { return 2.0 }
        "Popup" { return 3.0 }
        "RatingControl" { return 5.0 }
        "RepeatButton" { return 11.0 }
        "TeachingTip" { return 10.0 }
        "ToggleButton" { return 7.0 }
        default { return 24.0 }
    }
}

function Get-ReferenceInteractionCropSizeDeltaThreshold([string]$control) {
    switch ($control) {
        "AppBarButton" { return 2 }
        "AppBarToggleButton" { return 0 }
        "ColorPicker" { return 0 }
        "CommandBar" { return 0 }
        "CommandBarFlyout" { return 0 }
        "ContentDialog" { return 2 }
        "Flyout" { return 1 }
        "GridView" { return 4 }
        "MenuBar" { return 2 }
        "MenuFlyout" { return 0 }
        "NumberBox" { return 0 }
        "Popup" { return 0 }
        "RatingControl" { return 0 }
        "RepeatButton" { return 0 }
        "TeachingTip" { return 0 }
        "ToggleButton" { return 0 }
        default { return 24 }
    }
}

function Set-VisualCheckReferenceFailure($result, [string]$message) {
    $result["Status"] = "Failed"
    if ($result.Contains("LastException") -and ![string]::IsNullOrWhiteSpace([string]$result.LastException)) {
        $result["LastException"] = "$($result.LastException); $message"
    }
    else {
        $result["LastException"] = $message
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

function Open-CommandBarFlyoutSecondaryCommands($window) {
    $deadline = (Get-Date).AddMilliseconds(2500)
    do {
        $moreButton = Wait-ForCommandBarFlyoutPrimaryCommands $window 1200
        if ($null -eq $moreButton) {
            $moreButton = Find-CommandBarFlyoutMoreButton $window
        }

        if ($null -ne $moreButton) {
            if ((Invoke-ElementPatternOnce $window $moreButton) -and
                ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 600))) {
                return $true
            }

            if ((Expand-ElementPatternOnce $window $moreButton) -and
                ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 600))) {
                return $true
            }

            if ((Toggle-ElementPatternOnce $window $moreButton) -and
                ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 600))) {
                return $true
            }

            if ((Invoke-PopupElementFocusOnce $moreButton) -and
                ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 600))) {
                return $true
            }

            if ((Invoke-PopupElementClickOnce $moreButton) -and
                ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 1200))) {
                return $true
            }

            if ((Invoke-ElementOnce $window $moreButton) -and
                ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 1200))) {
                return $true
            }
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Move-CursorAwayFromInteractionSurface($window) {
    try {
        $rect = [GalleryVisualNative]::GetRect($window.Current.NativeWindowHandle)
        [GalleryVisualNative]::MoveCursor(
            [int]($rect.Left + 20),
            [int]($rect.Top + 20))
        Start-Sleep -Milliseconds 120
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

function Test-PopupScreenCropCandidate($window, $element) {
    $bounds = Get-ElementScreenBounds $element 0
    if ($null -eq $bounds -or !$bounds.Found) {
        return $false
    }

    $windowBounds = Get-ElementScreenBounds $window 0
    if ($null -ne $windowBounds -and $windowBounds.Found) {
        if ($bounds.Width -ge ($windowBounds.Width * 0.85) -or
            $bounds.Height -ge ($windowBounds.Height * 0.85)) {
            return $false
        }
    }

    return $true
}

function Get-PopupScreenCropElement($window, $openElement) {
    if ($null -eq $openElement) {
        return $null
    }

    $best = $openElement
    $candidate = $openElement
    for ($depth = 0; $depth -lt 10 -and $null -ne $candidate; $depth++) {
        try {
            $parent = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)
            if ($null -eq $parent -or !(Test-PopupScreenCropCandidate $window $parent)) {
                break
            }

            $best = $parent
            $candidate = $parent
        }
        catch {
            break
        }
    }

    return $best
}

function Test-ScreenElementPopupCropHasContent($crop) {
    return $null -ne $crop -and
        $crop.Found -and
        $crop.NonBlank -and
        $crop.Contains("VisibleStdDev") -and
        $crop.VisibleStdDev -ge 8.0
}

function Capture-OpenElementPopupCrop($window, $openElement, [IntPtr]$popupHandle, [string]$popupWindowPath, [string]$screenElementPath) {
    $mainHandle = $window.Current.NativeWindowHandle
    if ($popupHandle -ne [IntPtr]::Zero -and $popupHandle -ne $mainHandle) {
        try {
            Capture-Window $popupHandle $popupWindowPath -SkipActivate
            $popupNonBlank = Test-ImageNotBlank $popupWindowPath
            if (!$popupNonBlank) {
                Capture-ScreenRect $popupHandle $popupWindowPath
                $popupNonBlank = Test-ImageNotBlank $popupWindowPath
            }

            if ($popupNonBlank) {
                $popupSize = Get-ImageSize $popupWindowPath
                return [ordered]@{
                    Found = $true
                    Source = "PopupWindow"
                    Screenshot = $popupWindowPath
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
                    NonBlank = $true
                    VisibleStdDev = Get-ImageVisibleStdDev $popupWindowPath
                }
            }
        }
        catch {
        }
    }

    $screenElement = Get-PopupScreenCropElement $window $openElement
    $screenElementCrop = Save-ScreenElementCrop $screenElement $screenElementPath "ScreenElement" 10 $window
    if (Test-ScreenElementPopupCropHasContent $screenElementCrop) {
        return $screenElementCrop
    }

    return $null
}

function Save-MenuBarOpenSurfaceCrop($window, $openElement, $popupCrop, [string]$path) {
    $surfaceElement = Get-PopupScreenCropElement $window $openElement
    return Save-ScreenElementCrop $surfaceElement $path "MenuBarOpenSurface" 0 $window
}

function Save-CommandBarOpenSurfaceCrop($window, $openElement, [string]$path) {
    return Save-ScreenElementCrop $openElement $path "CommandBarOpenSurface" 0 $window
}

function Save-MenuFlyoutOpenSurfaceCrop($window, $openElement, [string]$path) {
    $surfaceElement = Get-PopupScreenCropElement $window $openElement
    $rawPath = [System.IO.Path]::ChangeExtension($path, ".raw.png")
    $rawCrop = Save-ScreenElementCrop $surfaceElement $rawPath "MenuFlyoutOpenSurface" 0 $window
    if ($null -eq $rawCrop -or !$rawCrop.Found) {
        return $rawCrop
    }

    $itemBounds = Get-ElementScreenBounds $openElement 0 $window
    $horizontalInset = 0
    if ($null -ne $itemBounds -and $itemBounds.Found) {
        # WinUI's popup UIA root includes three transparent theme-shadow pixels
        # on each horizontal edge. WPF's ContextMenu HWND bounds expose the
        # equivalent content surface directly. Preserve the full vertical
        # surface while removing only that reference-only shadow reservation.
        $horizontalOverhang = [int]$rawCrop.Width - [int]$itemBounds.Width
        if ($horizontalOverhang -ge 8) {
            $horizontalInset = [int][Math]::Floor(($horizontalOverhang - 4) / 2)
        }
    }

    if ($horizontalInset -le 0) {
        Copy-Item -LiteralPath $rawCrop.Screenshot -Destination $path -Force
        $rawCrop.Screenshot = $path
        return $rawCrop
    }

    $relativeBounds = [ordered]@{
        Found = $true
        Reason = ""
        X = $horizontalInset
        Y = 0
        Width = [int]$rawCrop.Width - (2 * $horizontalInset)
        Height = [int]$rawCrop.Height
        ChangedSamples = 0
    }
    Save-Crop $rawCrop.Screenshot $relativeBounds $path 0 | Out-Null
    $screenBounds = [ordered]@{
        Found = $true
        Reason = ""
        X = [int]$rawCrop.Bounds.X + $horizontalInset
        Y = [int]$rawCrop.Bounds.Y
        Width = $relativeBounds.Width
        Height = $relativeBounds.Height
        ChangedSamples = 0
    }
    return [ordered]@{
        Found = $true
        Source = "MenuFlyoutOpenSurface"
        Screenshot = $path
        Bounds = $screenBounds
        Width = $screenBounds.Width
        Height = $screenBounds.Height
        ChangedSamples = 0
        NonBlank = Test-ImageNotBlank $path
        VisibleStdDev = Get-ImageVisibleStdDev $path
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

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $edit.SetFocus()
        Start-Sleep -Milliseconds 50
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
    if ($control -eq "CommandBar") {
        $invoked = Invoke-ElementPatternOnce $window $element
        Start-Sleep -Milliseconds 200
        return $invoked
    }

    if ($control -eq "SplitButton" -or $control -eq "ToggleSplitButton") {
        $invoked = Invoke-SplitButtonSecondaryOnce $window $element
        Start-Sleep -Milliseconds 150
        if ((Get-ExpandCollapseStateName $element) -ne "Expanded") {
            $invoked = (Expand-ElementPatternOnce $window $element) -or $invoked
            Start-Sleep -Milliseconds 150
        }
        return $invoked
    }

    if ($control -eq "MenuBar") {
        $invoked = Invoke-MenuBarTriggerOnce $window $element $openNames
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

function Invoke-MenuBarTriggerOnce($window, $element, [string[]]$openNames) {
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

    if ($clicked -and $null -ne (Find-OpenInteractionElement $window $element $openNames "MenuBar")) {
        return $true
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
    if (!$baselineControlNonBlank -and $app -eq "ModernWpf" -and $control -eq "CommandBar") {
        $commandBarArtifactPath = Join-Path $caseDir "modernwpf-artifacts\GallerySample_CommandBar_CommandBar.png"
        if (Test-Path $commandBarArtifactPath) {
            $commandBarArtifactSize = Get-ImageSize $commandBarArtifactPath
            $baselineControlCrop = [ordered]@{
                Found = $true
                Source = "GallerySample_CommandBar_CommandBar"
                Screenshot = $commandBarArtifactPath
                Bounds = $null
                Width = $commandBarArtifactSize.Width
                Height = $commandBarArtifactSize.Height
                NonBlank = Test-ImageNotBlank $commandBarArtifactPath
                VisibleStdDev = Get-ImageVisibleStdDev $commandBarArtifactPath
            }
            $baselineControlNonBlank = $baselineControlCrop.NonBlank
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
    $commandBarFlyoutSecondaryExpanded = $false
    $commandBarFlyoutSurfaceCrop = $null
    $commandBarFlyoutSurfaceVisualTrusted = $false
    if ($control -eq "CommandBarFlyout" -and $invoked) {
        $commandBarFlyoutSecondaryExpanded = Open-CommandBarFlyoutSecondaryCommands $window
        if ($commandBarFlyoutSecondaryExpanded) {
            Move-CursorAwayFromInteractionSurface $window
        }
    }

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
    $comboBoxPopupCrop = $null
    $menuBarPopupNonBlank = $false
    $menuBarPopupScreenshot = ""
    $menuBarPopupSize = $null
    $menuBarPopupCrop = $null
    $openPopupNonBlank = $false
    $openPopupScreenshot = ""
    $openPopupSize = $null
    $openPopupCrop = $null
    $openElement = if ($control -eq "CommandBarFlyout") {
        Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move")
    }
    else {
        Find-OpenInteractionElement $window $showButton $openNames $control
    }
    if ($null -eq $openElement) {
        $openElement = Find-OpenInteractionElement $window $showButton $openNames $control
    }
    if ($null -ne $openElement) {
        $treePath = Join-Path $caseDir ("{0}-{1}-open.uia.txt" -f $app.ToLowerInvariant(), $control)
        Write-UiaTree $openElement $treePath 3
        if ($control -eq "ComboBox") {
            $popupHandle = Get-ElementNativeWindowHandle $openElement
            $comboBoxPopupCrop = Capture-OpenElementPopupCrop `
                $window `
                $openElement `
                $popupHandle `
                (Join-Path $caseDir ("{0}-{1}-popup-window.png" -f $app.ToLowerInvariant(), $control)) `
                (Join-Path $caseDir ("{0}-{1}-popup-screen-element.png" -f $app.ToLowerInvariant(), $control))
            if ($null -ne $comboBoxPopupCrop -and $comboBoxPopupCrop.NonBlank) {
                $comboBoxPopupScreenshot = $comboBoxPopupCrop.Screenshot
                $comboBoxPopupNonBlank = $true
                $comboBoxPopupSize = [ordered]@{
                    Width = $comboBoxPopupCrop.Width
                    Height = $comboBoxPopupCrop.Height
                }
            }
        }
        elseif ($control -eq "MenuBar") {
            $popupHandle = Get-ElementNativeWindowHandle $openElement
            $menuBarPopupCrop = Capture-OpenElementPopupCrop `
                $window `
                $openElement `
                $popupHandle `
                (Join-Path $caseDir ("{0}-{1}-popup-window.png" -f $app.ToLowerInvariant(), $control)) `
                (Join-Path $caseDir ("{0}-{1}-popup-screen-element.png" -f $app.ToLowerInvariant(), $control))
            if ($null -ne $menuBarPopupCrop -and $menuBarPopupCrop.NonBlank) {
                $menuBarPopupScreenshot = $menuBarPopupCrop.Screenshot
                $menuBarPopupNonBlank = $true
                $menuBarPopupSize = [ordered]@{
                    Width = $menuBarPopupCrop.Width
                    Height = $menuBarPopupCrop.Height
                }
            }
        }
        elseif (Test-ControlRequiresPopupWindowOpenProof $control) {
            $popupHandle = Get-ElementNativeWindowHandle $openElement
            $openPopupCrop = Capture-OpenElementPopupCrop `
                $window `
                $openElement `
                $popupHandle `
                (Join-Path $caseDir ("{0}-{1}-popup-window.png" -f $app.ToLowerInvariant(), $control)) `
                (Join-Path $caseDir ("{0}-{1}-popup-screen-element.png" -f $app.ToLowerInvariant(), $control))
            if ($null -ne $openPopupCrop -and $openPopupCrop.NonBlank) {
                $openPopupScreenshot = $openPopupCrop.Screenshot
                $openPopupNonBlank = $true
                $openPopupSize = [ordered]@{
                    Width = $openPopupCrop.Width
                    Height = $openPopupCrop.Height
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
        if (Test-ControlRequiresPopupWindowOpenProof $control) {
            $cropElement = $null
        }
        else {
            $cropElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "GalleryItemPageRoot"
            if ($null -eq $cropElement) {
                $cropElement = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "ContentRootGrid"
            }
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
            $comboBoxPopupTrusted = $comboBoxPopupNonBlank -and
                ($null -ne $comboBoxPopupCrop) -and
                ($comboBoxPopupCrop.Source -ne "ScreenElement" -or $screenCaptureTrusted)
            $visualOpened = $comboBoxPopupTrusted -or
                ($screenCaptureTrusted -and
                    $null -ne $comboBoxOpenVisualDelta -and
                    $comboBoxOpenVisualDelta.Comparable -and
                    $comboBoxOpenVisualDelta.MeanDelta -gt 5.0)
            if ($comboBoxPopupTrusted) {
                $crop = $comboBoxPopupCrop
            }
        }
        elseif ($control -eq "MenuBar") {
            $menuBarPopupTrusted = $menuBarPopupNonBlank -and
                ($null -ne $menuBarPopupCrop) -and
                ($menuBarPopupCrop.Source -ne "ScreenElement" -or $screenCaptureTrusted)
            $visualOpened = $menuBarPopupTrusted
            if ($menuBarPopupTrusted) {
                $crop = $menuBarPopupCrop
            }
        }
        elseif (Test-ControlRequiresPopupWindowOpenProof $control) {
            $openPopupTrusted = $openPopupNonBlank -and
                ($null -ne $openPopupCrop) -and
                ($openPopupCrop.Source -ne "ScreenElement" -or $screenCaptureTrusted)
            $visualOpened = $openPopupTrusted
            if ($openPopupTrusted) {
                $crop = $openPopupCrop
            }
        }

        if ($control -eq "ContentDialog") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $contentDialogSurfaceCrop = Save-ContentDialogOpenSurfaceCrop $window $selectedFrame.Screenshot $surfaceCropPath
            if ($contentDialogSurfaceCrop.Found -and $contentDialogSurfaceCrop.NonBlank) {
                $crop = $contentDialogSurfaceCrop
                $visualOpened = $true
            }
        }

        if ($control -eq "TeachingTip") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $teachingTipSurfaceCrop = Save-TeachingTipOpenSurfaceCrop `
                $window `
                $openElement `
                $triggerElement `
                $baselinePath `
                $selectedFrame.Screenshot `
                $surfaceCropPath
            if ($teachingTipSurfaceCrop.Found -and $teachingTipSurfaceCrop.NonBlank) {
                $crop = $teachingTipSurfaceCrop
                $visualOpened = $true
            }
        }

        if ($control -eq "Popup") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $popupSurfaceCrop = Save-PopupOpenSurfaceCrop $window $openElement $surfaceCropPath
            if ($popupSurfaceCrop.Found -and $popupSurfaceCrop.NonBlank) {
                $crop = $popupSurfaceCrop
                $visualOpened = $true
            }
        }

        if ($control -eq "Flyout") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $flyoutSurfaceCrop = Save-FlyoutOpenSurfaceCrop $window $openElement $surfaceCropPath
            if (Test-ScreenElementPopupCropHasContent $flyoutSurfaceCrop) {
                $crop = $flyoutSurfaceCrop
                $visualOpened = $true
            }
            else {
                $crop = $flyoutSurfaceCrop
                $visualOpened = $false
            }
        }

        if ($control -eq "MenuBar") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $menuBarSurfaceCrop = Save-MenuBarOpenSurfaceCrop $window $openElement $menuBarPopupCrop $surfaceCropPath
            if (Test-ScreenElementPopupCropHasContent $menuBarSurfaceCrop) {
                $crop = $menuBarSurfaceCrop
                $visualOpened = $true
            }
            else {
                $crop = $menuBarSurfaceCrop
                $visualOpened = $false
            }
        }

        if ($control -eq "MenuFlyout") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $menuFlyoutSurfaceCrop = Save-MenuFlyoutOpenSurfaceCrop $window $openElement $surfaceCropPath
            if (Test-ScreenElementPopupCropHasContent $menuFlyoutSurfaceCrop) {
                $crop = $menuFlyoutSurfaceCrop
                $visualOpened = $true
            }
            else {
                $crop = $menuFlyoutSurfaceCrop
                $visualOpened = $false
            }
        }

        if ($control -eq "CommandBar") {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $commandBarSurfaceCrop = Save-CommandBarOpenSurfaceCrop $window $openElement $surfaceCropPath
            if (Test-ScreenElementPopupCropHasContent $commandBarSurfaceCrop) {
                $crop = $commandBarSurfaceCrop
                $visualOpened = $true
                $openPopupCrop = $commandBarSurfaceCrop
                $openPopupScreenshot = $commandBarSurfaceCrop.Screenshot
                $openPopupNonBlank = $true
                $openPopupSize = [ordered]@{
                    Width = $commandBarSurfaceCrop.Width
                    Height = $commandBarSurfaceCrop.Height
                }
            }
            else {
                $crop = $commandBarSurfaceCrop
                $visualOpened = $false
            }
        }

        if ($control -eq "CommandBarFlyout" -and $commandBarFlyoutSecondaryExpanded) {
            $surfaceCropPath = Join-Path $caseDir ("{0}-{1}-open-surface-crop.png" -f $app.ToLowerInvariant(), $control)
            $commandBarFlyoutSurfaceCrop = Save-CommandBarFlyoutOpenSurfaceScreenCrop $window $surfaceCropPath
            if (Test-ScreenElementPopupCropHasContent $commandBarFlyoutSurfaceCrop) {
                $crop = $commandBarFlyoutSurfaceCrop
                $visualOpened = $true
            }
            else {
                $crop = $commandBarFlyoutSurfaceCrop
                $visualOpened = $false
            }
            $commandBarFlyoutSurfaceVisualTrusted = $visualOpened
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
    elseif ($control -eq "CommandBarFlyout") {
        $status = if (!$baselineNonBlank) { "Failed" } elseif (!$invoked) { "Failed" } elseif (!$commandBarFlyoutSecondaryExpanded) { "Failed" } elseif ($null -ne $openElement -and $commandBarFlyoutSurfaceVisualTrusted) { "Passed" } else { "Failed" }
        $notes = if (!$baselineNonBlank) { "$control open interaction baseline screenshot was blank." } elseif (!$invoked) { "Could not invoke the $control sample button." } elseif (!$commandBarFlyoutSecondaryExpanded) { "$control primary flyout opened, but the MoreButton did not expose Resize/Move secondary commands." } elseif ($null -eq $openElement) { "$control did not expose secondary command UIA after opening MoreButton." } elseif (!$commandBarFlyoutSurfaceVisualTrusted) { "$control exposed secondary command UIA but no trusted combined primary/secondary screen crop was captured." } else { "" }
    }
    elseif (Test-ControlRequiresPopupWindowOpenProof $control) {
        $status = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "Failed" } elseif (!$invoked) { "Failed" } elseif ($null -ne $openElement -and $visualOpened) { "Passed" } else { "Failed" }
        $notes = if (!$baselineNonBlank -or !$baselineControlNonBlank) { "$control open interaction baseline screenshot or control crop was blank." } elseif (!$invoked) { "Could not invoke the $control sample button." } elseif ($null -eq $openElement) { "$control did not expose opened popup content." } elseif (!$visualOpened) { "$control exposed opened popup UIA but no nonblank popup pixels were captured." } else { "" }
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
        ComboBoxPopupCrop = $comboBoxPopupCrop
        MenuBarPopupScreenshot = $menuBarPopupScreenshot
        MenuBarPopupNonBlank = $menuBarPopupNonBlank
        MenuBarPopupSize = $menuBarPopupSize
        MenuBarPopupCrop = $menuBarPopupCrop
        OpenPopupScreenshot = $openPopupScreenshot
        OpenPopupNonBlank = $openPopupNonBlank
        OpenPopupSize = $openPopupSize
        OpenPopupCrop = $openPopupCrop
        CommandBarFlyoutSecondaryExpanded = $commandBarFlyoutSecondaryExpanded
        CommandBarFlyoutSurfaceCrop = $commandBarFlyoutSurfaceCrop
        CommandBarFlyoutSurfaceVisualTrusted = $commandBarFlyoutSurfaceVisualTrusted
        SelectedFrameDelayMs = $(if ($null -ne $selectedFrame) { $selectedFrame.DelayMs } else { $null })
        SelectedFrameScreenshot = $(if ($null -ne $selectedFrame) { $selectedFrame.Screenshot } else { "" })
        Notes = $notes
    }
}

function New-ColorPickerStateInteractionCrop(
    [string]$app,
    [string]$caseDir,
    $window,
    [string]$screenshot,
    [string]$phase,
    [bool]$stateEnabled) {
    $source = if ($ColorPickerState -eq "Alpha") {
        "ColorPicker alpha surface"
    }
    elseif ($ColorPickerState -eq "Ring") {
        "ColorPicker ring surface"
    }
    else {
        "ColorPicker More-button surface"
    }
    $path = Join-Path $caseDir ("{0}-ColorPicker-state-{1}-crop.png" -f $app.ToLowerInvariant(), $phase)

    # WPF TextBlock labels are not represented in its UIA control view, so the
    # expanded baseline cannot be reconstructed from the same stable child set.
    # Its rendered artifact is exact and already used for the default parity
    # crop; keep live UIA-bound cropping for the toggled More-button state.
    if ($app -eq "ModernWpf" -and !$stateEnabled) {
        $artifactPath = Join-Path (Join-Path $caseDir "modernwpf-artifacts") "GallerySample_ColorPicker_ColorPicker.png"
        if (!(Test-Path $artifactPath)) {
            return $null
        }

        $sourceSize = Get-ImageSize $artifactPath
        $topPadding = 4
        $bottomPaddingAndMargin = 16
        $bounds = [ordered]@{
            Found = $true
            Reason = "Cropped the expanded ModernWpf ColorPicker state from its rendered artifact."
            X = 0
            Y = $topPadding
            Width = $sourceSize.Width
            Height = $sourceSize.Height - $topPadding - $bottomPaddingAndMargin
            ChangedSamples = 0
        }
        $savedBounds = Save-Crop $artifactPath $bounds $path 0
        return New-RenderedArtifactCrop $path $source $savedBounds
    }

    if ([string]::IsNullOrWhiteSpace($screenshot) -or !(Test-Path $screenshot)) {
        return $null
    }

    $elementIds = if ($ColorPickerState -eq "Alpha" -and $stateEnabled) {
        @("ColorSpectrum", "ThirdDimensionSlider", "AlphaSlider", "ColorRepresentationComboBox", "HexTextBox", "BlueTextBox", "AlphaTextBox")
    }
    elseif ($ColorPickerState -eq "Ring") {
        @("ColorSpectrum", "ThirdDimensionSlider", "ColorRepresentationComboBox", "HexTextBox", "BlueTextBox")
    }
    elseif ($ColorPickerState -eq "MoreButton" -and $stateEnabled) {
        @("ColorSpectrum", "ThirdDimensionSlider", "MoreButton")
    }
    else {
        @("ColorSpectrum", "ThirdDimensionSlider", "ColorRepresentationComboBox", "HexTextBox", "BlueTextBox", "BlueLabel")
    }
    $boundsById = @{}
    foreach ($elementId in $elementIds) {
        $child = Find-DescendantByAutomationId $window $elementId
        $childBounds = Get-ElementWindowBounds $window $child
        if ($null -eq $childBounds -or !$childBounds.Found) {
            return $null
        }

        $boundsById[$elementId] = $childBounds
    }

    $left = $boundsById["ColorSpectrum"].X
    $top = $boundsById["ColorSpectrum"].Y
    $right = $boundsById["ThirdDimensionSlider"].X + $boundsById["ThirdDimensionSlider"].Width
    $bottom = @($boundsById.Values | ForEach-Object { $_.Y + $_.Height } | Measure-Object -Maximum).Maximum
    $bounds = [ordered]@{
        Found = $true
        Reason = "Cropped the live ColorPicker state from stable child bounds."
        X = $left
        Y = $top
        Width = $right - $left
        Height = $bottom - $top
        ChangedSamples = 0
    }
    $savedBounds = Save-Crop $screenshot $bounds $path 0
    return New-RenderedArtifactCrop $path $source $savedBounds
}

function Capture-StateInteraction([string]$app, [string]$control, [string]$caseDir, $window, $element) {
    if (!$IncludeInteractions -or !(Test-ControlSupportsStateInteraction $control)) {
        return $null
    }

    [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 250

    $toggleElement = Get-StateInteractionTarget $window $control $element
    $baselineState = Get-StateInteractionStateName $control $toggleElement
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
    $baselineCrop = if ($control -eq "ColorPicker") {
        New-ColorPickerStateInteractionCrop $app $caseDir $window $baselinePath "before" ($baselineState -eq "On")
    }
    elseif (Test-Path $baselinePath) {
        Save-ElementCrop $window $baselinePath $baselineCropPath $element "UIA" 10
    }
    else {
        $null
    }
    if ($null -eq $baselineCrop) {
        $baselineCrop = Copy-RenderedArtifactCrop $renderedArtifactPath $baselineCropPath $renderedArtifactSource
    }

    $invoked = $false
    if (![string]::IsNullOrEmpty($baselineState)) {
        $invoked = Set-StateInteractionElementState $window $control $toggleElement $desiredState
    }
    $settleDelayMs = Get-StateInteractionSettleDelayMs $control
    Start-Sleep -Milliseconds $settleDelayMs

    $afterState = Get-StateInteractionStateName $control $toggleElement
    $stateOutputAutomationId = Get-StateInteractionOutputAutomationId $app $control
    $expectedStateOutput = Get-StateInteractionExpectedOutput $control $desiredState
    $stateOutputElement = if (![string]::IsNullOrWhiteSpace($stateOutputAutomationId)) {
        TryFind-DescendantByAutomationId $window $stateOutputAutomationId
    }
    else {
        $null
    }
    $stateOutput = if ($null -ne $stateOutputElement) { [string]$stateOutputElement.Current.Name } else { "" }
    $stateOutputMatched = [string]::IsNullOrEmpty($expectedStateOutput) -or $stateOutput -eq $expectedStateOutput
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
    $afterCrop = if ($control -eq "ColorPicker") {
        New-ColorPickerStateInteractionCrop $app $caseDir $window $afterPath "after" ($afterState -eq "On")
    }
    elseif (Test-Path $afterPath) {
        Save-ElementCrop $window $afterPath $afterCropPath $element "UIA" 10
    }
    else {
        $null
    }
    if ($null -eq $afterCrop) {
        $afterCrop = Copy-RenderedArtifactCrop $renderedArtifactPath $afterCropPath $renderedArtifactSource
    }

    $stateDelta = $null
    if ($null -ne $baselineCrop -and $null -ne $afterCrop -and
        $baselineCrop.Found -and $afterCrop.Found -and
        ![string]::IsNullOrEmpty($baselineCrop.Screenshot) -and
        ![string]::IsNullOrEmpty($afterCrop.Screenshot)) {
        $stateDelta = Compare-ImagesNormalized $baselineCrop.Screenshot $afterCrop.Screenshot
    }

    $afterCropScreenshot = if ($null -ne $afterCrop -and $afterCrop.Found) { $afterCrop.Screenshot } else { "" }
    $stateVisual = Test-StateInteractionVisual $control $desiredState $afterCropScreenshot
    $stateChanged = ![string]::IsNullOrEmpty($baselineState) -and
        ![string]::IsNullOrEmpty($afterState) -and
        $baselineState -ne $afterState -and
        $afterState -eq $desiredState
    $visualChanged = $null -ne $stateDelta -and $stateDelta.Comparable -and $stateDelta.MeanDelta -gt 0.5
    $status = if (!$invoked) { "Failed" } elseif (!$stateChanged) { "Failed" } elseif (!$stateOutputMatched) { "Failed" } elseif (!$visualChanged) { "Failed" } elseif (!$stateVisual.Passed) { "Failed" } else { "Passed" }
    $notes = if ([string]::IsNullOrEmpty($baselineState)) {
        "$control did not expose its configured UIA state pattern."
    }
    elseif (!$invoked) {
        "Could not toggle the $control sample from $baselineState to $desiredState."
    }
    elseif (!$stateChanged) {
        "$control state did not change from $baselineState to $desiredState; observed '$afterState'."
    }
    elseif (!$stateOutputMatched) {
        "$control output did not change to '$expectedStateOutput'; observed '$stateOutput'."
    }
    elseif (!$visualChanged) {
        "$control state changed, but the cropped control image did not visibly change."
    }
    elseif (!$stateVisual.Passed) {
        $stateVisual.Notes
    }
    else {
        ""
    }

    if ($control -eq "ColorPicker" -and ![string]::IsNullOrEmpty($baselineState)) {
        [void](Set-StateInteractionElementState $window $control $toggleElement $baselineState)
        Start-Sleep -Milliseconds $settleDelayMs
        if ($app -eq "ModernWpf") {
            [void](Refresh-ModernWpfVisualArtifacts $window)
        }
    }

    return [ordered]@{
        Status = $status
        Kind = "State"
        Invoked = $invoked
        BaselineState = $baselineState
        DesiredState = $desiredState
        StateAfter = $afterState
        OutputAutomationId = $stateOutputAutomationId
        ExpectedOutput = $expectedStateOutput
        OutputAfter = $stateOutput
        OutputMatched = $stateOutputMatched
        BaselineScreenshot = $baselinePath
        Frames = @(
            [ordered]@{
                DelayMs = $settleDelayMs
                Screenshot = $(if (Test-Path $afterPath) { $afterPath } else { "" })
                NonBlank = $(if (Test-Path $afterPath) { Test-ImageNotBlank $afterPath } else { $false })
                Error = ""
            }
        )
        StateDelta = $stateDelta
        StateVisualCheck = $stateVisual
        Crop = $afterCrop
        BaselineCrop = $baselineCrop
        SelectedFrameDelayMs = $settleDelayMs
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

    $cropAutomationId = if ($app -eq "WinUI3" -and $control -eq "GridView") {
        "ClickOutput0"
    }
    else {
        Get-SelectionInteractionCropAutomationId $control
    }
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

    if ($control -eq "GridView" -and
        $null -ne $baselineCrop -and $null -ne $afterCrop -and
        $baselineCrop.Found -and $afterCrop.Found -and
        ![string]::IsNullOrEmpty($baselineCrop.Screenshot) -and
        ![string]::IsNullOrEmpty($afterCrop.Screenshot)) {
        # The two galleries give the output TextBlock the remaining sample width, which is
        # intentionally different. Compare only the pixels written by the ItemClick result.
        $outputDifferenceBounds = Find-DifferenceBounds $baselineCrop.Screenshot $afterCrop.Screenshot 32 1
        if ($null -ne $outputDifferenceBounds -and
            $outputDifferenceBounds.Found -and
            $outputDifferenceBounds.ChangedSamples -gt 0) {
            $tightBaselineCropPath = Join-Path $caseDir ("{0}-{1}-selection-before-content-crop.png" -f $app.ToLowerInvariant(), $control)
            $tightAfterCropPath = Join-Path $caseDir ("{0}-{1}-selection-after-content-crop.png" -f $app.ToLowerInvariant(), $control)
            $tightBaselineBounds = Save-Crop $baselineCrop.Screenshot $outputDifferenceBounds $tightBaselineCropPath 4
            $tightAfterBounds = Save-Crop $afterCrop.Screenshot $outputDifferenceBounds $tightAfterCropPath 4
            $baselineCrop = New-RenderedArtifactCrop $tightBaselineCropPath "UIA" $tightBaselineBounds
            $afterCrop = New-RenderedArtifactCrop $tightAfterCropPath "UIA" $tightAfterBounds
        }
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
    if ($app -eq "ModernWpf" -and $control -eq "AppBarButton") {
        $cropAutomationId = "GallerySample_AppBarButton_Output"
    }
    elseif ($app -eq "ModernWpf" -and $control -eq "CommandBar") {
        $cropAutomationId = "GallerySample_CommandBar_Output"
    }
    elseif ($app -eq "WinUI3" -and $control -eq "RepeatButton") {
        # The upstream page exposes x:Name="Control1Output" as its UIA id; the
        # WPF port uses its stable GallerySample_* harness id for the same node.
        $cropAutomationId = "Control1Output"
    }
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
    $baselineCrop = if ((Test-Path $baselinePath) -and $control -eq "RepeatButton") {
        Save-RepeatButtonOutputSurfaceCrop $window $baselinePath $baselineCropPath $trigger
    }
    elseif (Test-Path $baselinePath) {
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

    $afterCropElement = $cropElement
    if (![string]::IsNullOrWhiteSpace($cropAutomationId)) {
        $resolvedAfterCropElement = TryFind-DescendantByAutomationId $window $cropAutomationId
        if ($null -ne $resolvedAfterCropElement) {
            $afterCropElement = $resolvedAfterCropElement
        }
    }
    $expectedOutputNames = @(Get-OutputInteractionExpectedNames $control)
    if ($expectedOutputNames.Count -gt 0) {
        $namedAfterCropElement = Find-ElementByNameInProcess $window.Current.ProcessId $expectedOutputNames
        if ($null -ne $namedAfterCropElement) {
            $afterCropElement = $namedAfterCropElement
        }
    }

    $afterPath = Join-Path $caseDir ("{0}-{1}-output-after.png" -f $app.ToLowerInvariant(), $control)
    try {
        Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
    }
    catch {
        Capture-ScreenRect $window.Current.NativeWindowHandle $afterPath
    }

    $afterCropPath = Join-Path $caseDir ("{0}-{1}-output-after-crop.png" -f $app.ToLowerInvariant(), $control)
    $afterCrop = if ((Test-Path $afterPath) -and $control -eq "RepeatButton") {
        Save-RepeatButtonOutputSurfaceCrop $window $afterPath $afterCropPath $trigger
    }
    elseif (Test-Path $afterPath) {
        Save-ElementCrop $window $afterPath $afterCropPath $afterCropElement "UIA" 20
    }
    else {
        $null
    }
    if ($null -ne $afterCrop -and $afterCrop.Contains("NonBlank") -and !$afterCrop.NonBlank) {
        Start-Sleep -Milliseconds 300
        try {
            Capture-Window $window.Current.NativeWindowHandle $afterPath -SkipActivate
            $afterCrop = Save-ElementCrop $window $afterPath $afterCropPath $afterCropElement "UIA" 20
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
    $allowsBlankBaseline = Test-OutputInteractionAllowsBlankBaseline $control
    $baselineNonBlank = $null -ne $baselineCrop -and $baselineCrop.Contains("NonBlank") -and $baselineCrop.NonBlank
    $afterNonBlank = $null -ne $afterCrop -and $afterCrop.Contains("NonBlank") -and $afterCrop.NonBlank
    $visualChanged = ($null -ne $outputDelta -and $outputDelta.Comparable -and $outputDelta.MeanDelta -gt $minimumDelta) -or
        ($allowsBlankBaseline -and !$baselineNonBlank -and $afterNonBlank)
    $status = if ($null -eq $trigger) { "Failed" } elseif (!$invoked) { "Failed" } elseif ((!$baselineNonBlank -and !$allowsBlankBaseline) -or !$afterNonBlank) { "Failed" } elseif (!$visualChanged) { "Failed" } else { "Passed" }
    $notes = if ($null -eq $trigger) {
        "$control output trigger '$($triggerNames -join "', '")' was not found."
    }
    elseif (!$invoked) {
        "Could not invoke the $control output trigger '$($triggerNames[0])'."
    }
    elseif ((!$baselineNonBlank -and !$allowsBlankBaseline) -or !$afterNonBlank) {
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
    $route = "item/$(Get-ControlRouteId $control "ModernWpf")"
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
        if ($control -eq "ProgressRing") {
            Set-ProgressRingDeterminateValue $window "ModernWpf" 65 | Out-Null
            Refresh-ModernWpfVisualArtifacts $window | Out-Null
        }
        elseif ($control -eq "WinUIProgressBar") {
            Set-ProgressBarDeterminateValue $window "ModernWpf" 65 | Out-Null
            Refresh-ModernWpfVisualArtifacts $window | Out-Null
        }

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
        $openNames = Get-OpenInteractionNames $control
        $openInteraction = Capture-OpenInteraction "ModernWpf" $control $caseDir $window $sample $openNames
        $stateInteraction = Capture-StateInteraction "ModernWpf" $control $caseDir $window $sample
        $selectionInteraction = Capture-SelectionInteraction "ModernWpf" $control $caseDir $window $sample
        $valueInteraction = Capture-ValueInteraction "ModernWpf" $control $caseDir $window $sample
        $outputInteraction = Capture-OutputInteraction "ModernWpf" $control $caseDir $window $sample
        $textInteraction = Capture-TextInteraction "ModernWpf" $control $caseDir $window $sample
        if ($control -eq "CommandBar" -and $null -ne $openInteraction -and $null -ne $outputInteraction) {
            $openInteraction["OutputInteraction"] = $outputInteraction
            if ($outputInteraction.Status -ne "Passed") {
                $openInteraction["Status"] = "Failed"
                $openInteraction["Notes"] = $outputInteraction.Notes
            }
        }
        $interaction = if ($null -ne $openInteraction) { $openInteraction } elseif ($null -ne $stateInteraction) { $stateInteraction } elseif ($null -ne $selectionInteraction) { $selectionInteraction } elseif ($null -ne $valueInteraction) { $valueInteraction } elseif ($null -ne $outputInteraction) { $outputInteraction } else { $textInteraction }
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
        $primaryCropMissing = (Test-ControlRequiresPrimaryCrop $control) -and !$staticCrops.Primary.Found
        $requiredSampleFound = $requiredSampleArtifactFound -or $null -ne $sample -or $hasRenderedCrops
        $status = if ($lastException) { "Failed" } elseif (!$notBlank) { "Failed" } elseif ($primaryCropMissing) { "Failed" } elseif ($primaryCropBlank) { "Failed" } elseif ($primaryCropLowVariation) { "Failed" } elseif (!$requiredSampleFound) { "Failed" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { "Failed" } else { "Passed" }
        if ($primaryCropMissing -and [string]::IsNullOrEmpty($lastException)) {
            $lastException = "Primary crop was required for $control but was not found."
        }
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
    $route = "winui3gallery://item/$(Get-ControlRouteId $control "WinUI3")"
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
        $rootThemeProbe = Ensure-WinUIReferenceRootTheme $control $window
        Wait-WinUIReferenceReady $window $control
        Reset-WinUIReferenceSampleScroll $window $control
        if ($control -eq "ProgressRing") {
            Set-ProgressRingDeterminateValue $window "WinUI3" 65 | Out-Null
        }
        elseif ($control -eq "WinUIProgressBar") {
            Set-ProgressBarDeterminateValue $window "WinUI3" 65 | Out-Null
        }
        $themeProbe = Ensure-WinUIReferenceTheme $control $caseDir $window
        $themeProbe["RootTheme"] = $rootThemeProbe
        Reset-WinUIReferenceSampleScroll $window $control
        if ($control -eq "ProgressRing") {
            Set-ProgressRingDeterminateValue $window "WinUI3" 65 | Out-Null
        }
        elseif ($control -eq "WinUIProgressBar") {
            Set-ProgressBarDeterminateValue $window "WinUI3" 65 | Out-Null
        }

        Move-CursorAwayFromInteractionSurface $window
        $screenshot = Join-Path $caseDir "winui3-$control.png"
        $treePath = Join-Path $caseDir "winui3-$control.uia.txt"
        Write-UiaTree $window $treePath 6
        $staticCrops = $null
        $notBlank = $false
        $primaryCropBlank = $false
        $primaryCropLowVariation = $false
        $primaryCropMinimumVisibleStdDev = Get-PrimaryCropMinimumVisibleStdDev $control
        $primaryCropWrongSource = $false
        $requiredPrimaryCropSource = Get-RequiredReferencePrimaryCropSource $control
        foreach ($captureAttempt in 1..3) {
            if ($captureAttempt -gt 1) {
                [GalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
                Start-Sleep -Milliseconds (400 * $captureAttempt)
            }

            Capture-Window $window.Current.NativeWindowHandle $screenshot
            $staticCrops = Capture-StaticCrops "WinUI3" $control $caseDir $window $screenshot
            $notBlank = Test-ImageNotBlank $screenshot
            $primaryCropMissing = (Test-ControlRequiresPrimaryCrop $control) -and !$staticCrops.Primary.Found
            $primaryCropBlank = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("NonBlank") -and !$staticCrops.Primary.NonBlank
            $primaryCropLowVariation = $staticCrops.Primary.Found -and $staticCrops.Primary.Contains("VisibleStdDev") -and $staticCrops.Primary.VisibleStdDev -lt $primaryCropMinimumVisibleStdDev
            $primaryCropWrongSource = $staticCrops.Primary.Found -and ![string]::IsNullOrEmpty($requiredPrimaryCropSource) -and $staticCrops.Primary.Source -ne $requiredPrimaryCropSource
            if ($notBlank -and !$primaryCropMissing -and !$primaryCropBlank -and !$primaryCropLowVariation -and !$primaryCropWrongSource) {
                break
            }
        }

        $showButton = Find-ReferenceInteractionTrigger $window $control
        $openNames = Get-OpenInteractionNames $control
        $openInteraction = Capture-OpenInteraction "WinUI3" $control $caseDir $window $showButton $openNames
        $stateInteraction = Capture-StateInteraction "WinUI3" $control $caseDir $window $showButton
        $selectionInteraction = Capture-SelectionInteraction "WinUI3" $control $caseDir $window $showButton
        $valueInteraction = Capture-ValueInteraction "WinUI3" $control $caseDir $window $showButton
        $outputInteraction = Capture-OutputInteraction "WinUI3" $control $caseDir $window $showButton
        $textInteraction = Capture-TextInteraction "WinUI3" $control $caseDir $window $showButton
        if ($control -eq "CommandBar" -and $null -ne $openInteraction -and $null -ne $outputInteraction) {
            $openInteraction["OutputInteraction"] = $outputInteraction
            if ($outputInteraction.Status -ne "Passed") {
                $openInteraction["Status"] = "Failed"
                $openInteraction["Notes"] = $outputInteraction.Notes
            }
        }
        $interaction = if ($null -ne $openInteraction) { $openInteraction } elseif ($null -ne $stateInteraction) { $stateInteraction } elseif ($null -ne $selectionInteraction) { $selectionInteraction } elseif ($null -ne $valueInteraction) { $valueInteraction } elseif ($null -ne $outputInteraction) { $outputInteraction } else { $textInteraction }

        $themeProbeFailed = -not (Test-WinUIReferenceThemeProbeSucceeded $themeProbe)

        return [ordered]@{
            App = "WinUI3Gallery"
            Control = $control
            Route = $route
            Status = $(if (!$notBlank) { "Failed" } elseif ($primaryCropMissing) { "Failed" } elseif ($primaryCropBlank) { "Failed" } elseif ($primaryCropLowVariation) { "Failed" } elseif ($primaryCropWrongSource) { "Failed" } elseif ($themeProbeFailed) { "Failed" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { "Failed" } else { "Passed" })
            Title = $pageTitle
            Screenshot = $screenshot
            UiaTree = $treePath
            LastException = $(if ($primaryCropMissing) { "Primary crop was required for $control but was not found." } elseif ($primaryCropBlank) { "Primary crop '$($staticCrops.Primary.Source)' was blank." } elseif ($primaryCropLowVariation) { "Primary crop '$($staticCrops.Primary.Source)' had low visible variation ($($staticCrops.Primary.VisibleStdDev), expected at least $primaryCropMinimumVisibleStdDev)." } elseif ($primaryCropWrongSource) { "Primary crop for $control used '$($staticCrops.Primary.Source)' but expected '$requiredPrimaryCropSource'." } elseif ($themeProbeFailed) { "Reference theme probe did not prove $Theme theme: $($themeProbe.Reason)" } elseif ($null -ne $interaction -and $interaction.Status -ne "Passed") { $interaction.Notes } else { "" })
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

function Get-WinUIReferenceSelectedItemName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern)
        $selection = @($pattern.Current.GetSelection())
        if ($selection.Count -gt 0) {
            return [string]$selection[0].Current.Name
        }
    }
    catch {
    }

    return ""
}

function Ensure-WinUIReferenceRootTheme([string]$control, $window) {
    if (($control -ne "CommandBar" -and $control -ne "CommandBarFlyout") -or $Theme -eq "Default") {
        return [ordered]@{
            RequestedTheme = $Theme
            SelectedTheme = ""
            Verified = $true
            Reason = "A dedicated reference root theme is not required."
        }
    }

    $settings = Find-ElementByNameInProcess $window.Current.ProcessId @("Settings")
    $settingsInvoked = Invoke-SelectionElementOnce $window $settings
    if (!$settingsInvoked) {
        try {
            $windowRect = $window.Current.BoundingRectangle
            [GalleryVisualNative]::Click(
                [int][Math]::Round($windowRect.X + 100),
                [int][Math]::Round($windowRect.Y + $windowRect.Height - 30))
            $settingsInvoked = $true
        }
        catch {
        }
    }

    $themeMode = $null
    if ($settingsInvoked) {
        try {
            $themeMode = Wait-Until -TimeoutSeconds 5 -Description "WinUI 3 Gallery app theme selector" -Probe {
                Find-ElementByAutomationIdInProcess $window.Current.ProcessId "themeModeComboBox"
            }
        }
        catch {
        }
    }
    if ($null -eq $themeMode) {
        return [ordered]@{
            RequestedTheme = $Theme
            SelectedTheme = ""
            Verified = $false
            Reason = "Could not navigate to the WinUI 3 Gallery app theme selector."
        }
    }

    $selectedTheme = Get-WinUIReferenceSelectedItemName $themeMode
    if ($selectedTheme -ne $Theme) {
        try {
            $expandPattern = $themeMode.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            $expandPattern.Expand()
            Start-Sleep -Milliseconds 150
        }
        catch {
        }

        $themeItem = Find-DescendantByName $themeMode $Theme
        if ($null -eq $themeItem) {
            $themeItem = Find-ElementByNameInProcess $window.Current.ProcessId @($Theme)
        }
        if ($null -ne $themeItem) {
            try {
                $selectionPattern = $themeItem.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
                $selectionPattern.Select()
                Start-Sleep -Milliseconds 700
            }
            catch {
                [void](Invoke-ElementOnce $window $themeItem)
                Start-Sleep -Milliseconds 700
            }
        }
        $selectedTheme = Get-WinUIReferenceSelectedItemName $themeMode
    }

    $backInvoker = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "__GoBackInvoker"
    $backInvoked = Invoke-ElementPatternOnce $window $backInvoker
    if ($backInvoked) {
        try {
            Wait-Until -TimeoutSeconds 5 -Description "WinUI 3 Gallery return to $control" -Probe {
                Find-DescendantByName $window $control
            } | Out-Null
        }
        catch {
            $backInvoked = $false
        }
    }

    $verified = $selectedTheme -eq $Theme -and $backInvoked
    return [ordered]@{
        RequestedTheme = $Theme
        SelectedTheme = $selectedTheme
        SettingsInvoked = $settingsInvoked
        BackInvoked = $backInvoked
        Verified = $verified
        Reason = $(if ($verified) { "WinUI 3 Gallery root theme matched the requested popup theme." } else { "WinUI 3 Gallery root theme or return navigation was not verified." })
    }
}

function Ensure-WinUIReferenceTheme([string]$control, [string]$caseDir, $window) {
    if ($Theme -eq "Default") {
        return [ordered]@{
            RequestedTheme = $Theme
            MeanLuminance = $null
            Toggled = $false
            Verified = $true
            Reason = "Default theme requested."
        }
    }

    $primarySource = Get-ReferencePrimaryAutomationId $control
    $primaryName = Get-ReferencePrimaryName $control
    if ($control -eq "CommandBarFlyout") {
        # Its primary control is a dark mountain photo in both themes. Probe the
        # instruction row inside the themed example instead of image content or
        # the description row outside the example's requested-theme boundary.
        $primarySource = ""
        $primaryName = "Click or right click the image to open a CommandBarFlyout"
    }
    elseif ($control -eq "ColorPicker") {
        # The hue spectrum has nearly identical luminance in both themes.
        # Probe a theme-sensitive text input so a persisted Gallery theme is
        # not mistaken for the requested theme.
        $primarySource = "HexTextBox"
        $primaryName = ""
    }
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

    $mean = Get-ImageMeanLuminance $probeCropPath
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
            Verified = $true
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

    $postMean = $null
    $verified = $false
    if ($toggled) {
        $postProbeScreenshot = Join-Path $caseDir ("winui3-$control-theme-probe-after.png")
        $postProbeCropPath = Join-Path $caseDir ("winui3-$control-theme-probe-after-crop.png")
        Capture-Window $window.Current.NativeWindowHandle $postProbeScreenshot
        $postProbeCrop = Save-ElementCrop $window $postProbeScreenshot $postProbeCropPath $primaryElement $primarySource 0
        if ($postProbeCrop.Found -and $postProbeCrop.NonBlank) {
            $postMean = Get-ImageMeanLuminance $postProbeCropPath
            if ($null -ne $postMean) {
                $verified = (([double]$postMean -lt 128.0) -eq $wantsDark)
            }
        }
    }

    return [ordered]@{
        RequestedTheme = $Theme
        MeanLuminance = $postMean
        InitialMeanLuminance = $mean
        Toggled = $toggled
        Verified = $verified
        Reason = $(if ($verified) { "Reference sample theme toggled and the post-toggle crop matched the requested theme." } elseif ($toggled) { "Reference sample theme toggled, but the post-toggle crop did not match the requested theme." } else { "ThemeButton did not invoke." })
    }
}

function Test-WinUIReferenceThemeProbeSucceeded($themeProbe) {
    if ($Theme -eq "Default") {
        return $true
    }

    if ($null -eq $themeProbe) {
        return $false
    }

    if ($themeProbe.Contains("RootTheme") -and !$themeProbe.RootTheme.Verified) {
        return $false
    }

    if ($themeProbe.Contains("Verified")) {
        return $themeProbe.Verified -eq $true
    }

    if ($themeProbe.Toggled -eq $true) {
        return $true
    }

    $reason = [string]$themeProbe.Reason
    return $reason.Contains("already matched")
}

function ConvertTo-OrderedDictionaryFromJsonValue($value) {
    if ($null -eq $value) {
        return $null
    }

    if ($value -is [System.Management.Automation.PSCustomObject]) {
        $map = [ordered]@{}
        foreach ($property in $value.PSObject.Properties) {
            $map[$property.Name] = ConvertTo-OrderedDictionaryFromJsonValue $property.Value
        }
        return $map
    }

    if ($value -is [System.Array]) {
        $items = New-Object System.Collections.Generic.List[object]
        foreach ($item in $value) {
            $items.Add((ConvertTo-OrderedDictionaryFromJsonValue $item))
        }
        return $items.ToArray()
    }

    return $value
}

function Assert-CachedReferenceFileExists([string]$path, [string]$description) {
    if ([string]::IsNullOrWhiteSpace($path) -or !(Test-Path $path)) {
        throw "Cached WinUI reference $description was missing: '$path'. Refresh the WinUI reference run."
    }
}

function Get-CachedWinUIReferenceResult([string]$control) {
    if ([string]::IsNullOrWhiteSpace($WinUIReferenceRunDir)) {
        return $null
    }

    $resolvedRunDir = (Resolve-Path $WinUIReferenceRunDir).Path
    $cachedReport = Join-Path $resolvedRunDir "report.json"
    if (!(Test-Path $cachedReport)) {
        throw "Cached WinUI reference report was not found at '$cachedReport'."
    }

    $cachedResults = @(Get-Content -Path $cachedReport -Raw | ConvertFrom-Json)
    $match = @($cachedResults | Where-Object { $_.App -eq "WinUI3Gallery" -and $_.Control -eq $control } | Select-Object -Last 1)
    if ($match.Count -eq 0) {
        throw "Cached WinUI reference report '$cachedReport' does not contain control '$control'."
    }

    $result = ConvertTo-OrderedDictionaryFromJsonValue $match[0]
    Assert-CachedReferenceFileExists ([string]$result.Screenshot) "$control screenshot"
    if ($result.Contains("StaticCrops") -and $result.StaticCrops.Primary.Found) {
        Assert-CachedReferenceFileExists ([string]$result.StaticCrops.Primary.Screenshot) "$control primary crop"
    }
    if ($result.Contains("Interaction") -and $null -ne $result.Interaction -and
        $result.Interaction.Crop.Found) {
        Assert-CachedReferenceFileExists ([string]$result.Interaction.Crop.Screenshot) "$control interaction crop"
    }

    $result["ReferenceCacheReused"] = $true
    $result["ReferenceSourceRunDir"] = $resolvedRunDir
    return $result
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
        if (![string]::IsNullOrWhiteSpace($WinUIReferenceRunDir)) {
            try {
                $referenceResult = Get-CachedWinUIReferenceResult $control
            }
            catch {
                $lastReferenceError = $_.Exception.Message
            }
        }
        else {
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
    $hasPrimaryCropPair = $null -ne $modern -and $null -ne $referenceCapture -and
        $modern.Contains("StaticCrops") -and $referenceCapture.Contains("StaticCrops") -and
        $null -ne $modern.StaticCrops -and $null -ne $referenceCapture.StaticCrops -and
        $modern.StaticCrops.Primary.Found -and $referenceCapture.StaticCrops.Primary.Found
    if ($null -ne $modern -and $null -ne $referenceCapture -and $modern.Screenshot -and $referenceCapture.Screenshot) {
        $comparison = Compare-Images $modern.Screenshot $referenceCapture.Screenshot
        $modern["ReferenceComparison"] = $comparison
        if ($FailOnDifference -and !$hasPrimaryCropPair -and $comparison.Comparable -and $comparison.MeanDelta -gt 24) {
            Set-VisualCheckReferenceFailure $modern "Mean pixel delta $($comparison.MeanDelta) exceeded visual threshold 24."
        }
    }
    if ($hasPrimaryCropPair) {
        $modern["PrimaryCropReferenceComparison"] = Compare-ImagesNormalized $modern.StaticCrops.Primary.Screenshot $referenceCapture.StaticCrops.Primary.Screenshot
        $modern["PrimaryCropSize"] = [ordered]@{
            ModernWpfWidth = $modern.StaticCrops.Primary.Width
            ModernWpfHeight = $modern.StaticCrops.Primary.Height
            ReferenceWidth = $referenceCapture.StaticCrops.Primary.Width
            ReferenceHeight = $referenceCapture.StaticCrops.Primary.Height
        }

        $primaryThreshold = Get-ReferencePrimaryCropMeanDeltaThreshold $control
        if ($FailOnDifference -and
            $modern.PrimaryCropReferenceComparison.Comparable -and
            [double]$modern.PrimaryCropReferenceComparison.MeanDelta -gt $primaryThreshold) {
            Set-VisualCheckReferenceFailure $modern "Primary crop delta $($modern.PrimaryCropReferenceComparison.MeanDelta) exceeded visual threshold $primaryThreshold."
        }
        if ($FailOnDifference) {
            $primarySizeDelta = [Math]::Abs([int]$modern.PrimaryCropSize.ModernWpfWidth - [int]$modern.PrimaryCropSize.ReferenceWidth) +
                [Math]::Abs([int]$modern.PrimaryCropSize.ModernWpfHeight - [int]$modern.PrimaryCropSize.ReferenceHeight)
            $primarySizeThreshold = Get-ReferencePrimaryCropSizeDeltaThreshold $control
            if ($primarySizeDelta -gt $primarySizeThreshold) {
                Set-VisualCheckReferenceFailure $modern "$control primary crop size delta $primarySizeDelta exceeded visual threshold $primarySizeThreshold."
            }
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
            $modern["InteractionCropReferenceComparison"] = if ($control -eq "GridView") {
                Compare-ImagesOnCommonCanvas $modern.Interaction.Crop.Screenshot $referenceCapture.Interaction.Crop.Screenshot
            }
            else {
                Compare-ImagesNormalized $modern.Interaction.Crop.Screenshot $referenceCapture.Interaction.Crop.Screenshot
            }
            $modern["InteractionCropSize"] = [ordered]@{
                ModernWpfWidth = $modern.Interaction.Crop.Width
                ModernWpfHeight = $modern.Interaction.Crop.Height
                ReferenceWidth = $referenceCapture.Interaction.Crop.Width
                ReferenceHeight = $referenceCapture.Interaction.Crop.Height
            }
        }

        $requiresInteractionCropParity = Test-ControlRequiresReferenceInteractionCropParity $control
        if ($requiresInteractionCropParity) {
            if ($null -eq $modern.Interaction.Crop -or
                $null -eq $referenceCapture.Interaction.Crop -or
                !$modern.Interaction.Crop.Found -or
                !$referenceCapture.Interaction.Crop.Found) {
                Set-VisualCheckReferenceFailure $modern "$control interaction reference crop was required but unavailable."
            }
            elseif ($modern.Interaction.Crop.Source -ne $referenceCapture.Interaction.Crop.Source) {
                Set-VisualCheckReferenceFailure $modern "$control interaction crop sources differed: ModernWpf '$($modern.Interaction.Crop.Source)' vs reference '$($referenceCapture.Interaction.Crop.Source)'."
            }
            elseif (!$modern.Contains("InteractionCropReferenceComparison") -or !$modern.InteractionCropReferenceComparison.Comparable) {
                Set-VisualCheckReferenceFailure $modern "$control interaction crop comparison was not comparable."
            }
            else {
                $interactionCropThreshold = Get-ReferenceInteractionCropMeanDeltaThreshold $control
                if ([double]$modern.InteractionCropReferenceComparison.MeanDelta -gt $interactionCropThreshold) {
                    Set-VisualCheckReferenceFailure $modern "$control interaction crop delta $($modern.InteractionCropReferenceComparison.MeanDelta) exceeded visual threshold $interactionCropThreshold."
                }

                if ($modern.Contains("InteractionCropSize")) {
                    $interactionSizeDelta = [Math]::Abs([int]$modern.InteractionCropSize.ModernWpfWidth - [int]$modern.InteractionCropSize.ReferenceWidth) +
                        [Math]::Abs([int]$modern.InteractionCropSize.ModernWpfHeight - [int]$modern.InteractionCropSize.ReferenceHeight)
                    $interactionSizeThreshold = Get-ReferenceInteractionCropSizeDeltaThreshold $control
                    if ($interactionSizeDelta -gt $interactionSizeThreshold) {
                        Set-VisualCheckReferenceFailure $modern "$control interaction crop size delta $interactionSizeDelta exceeded visual threshold $interactionSizeThreshold."
                    }
                }
            }
        }
        elseif ($FailOnDifference -and
            $modern.Contains("InteractionCropReferenceComparison") -and
            $modern.InteractionCropReferenceComparison.Comparable) {
            $interactionCropThreshold = Get-ReferenceInteractionCropMeanDeltaThreshold $control
            if ([double]$modern.InteractionCropReferenceComparison.MeanDelta -gt $interactionCropThreshold) {
                Set-VisualCheckReferenceFailure $modern "$control interaction crop delta $($modern.InteractionCropReferenceComparison.MeanDelta) exceeded visual threshold $interactionCropThreshold."
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
if (![string]::IsNullOrWhiteSpace($WinUIReferenceRunDir)) {
    $markdown.Add("- WinUI reference run: $((Resolve-Path $WinUIReferenceRunDir).Path)")
}
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
