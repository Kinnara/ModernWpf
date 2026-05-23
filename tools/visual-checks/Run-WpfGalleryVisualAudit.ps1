param(
    [string[]]$Cases = @(),
    [ValidateSet("None", "OfficialWpfGallery")]
    [string]$Reference = "OfficialWpfGallery",
    [ValidateSet("Light", "Dark", "Default")]
    [string]$Theme = "Light",
    [string]$ModernGalleryExe,
    [string]$WpfGalleryExe,
    [string]$OfficialDirectHostExe,
    [string]$OutputRoot = "artifacts\wpf-gallery-visual-audit",
    [int]$Width = 1180,
    [int]$Height = 820,
    [int]$TimeoutSeconds = 30,
    [switch]$BuildModern,
    [switch]$BuildOfficial,
    [switch]$ListCases,
    [switch]$FailOnDifference
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$OfficialWpfGalleryRoot = "D:\repos\WPF-Samples\Sample Applications\WPFGallery"

if ([string]::IsNullOrWhiteSpace($ModernGalleryExe)) {
    $ModernGalleryExe = Join-Path $RepoRoot "ModernWpf.Gallery\bin\Debug\net8.0-windows7.0\ModernWpf.Gallery.exe"
}

if ([string]::IsNullOrWhiteSpace($WpfGalleryExe)) {
    $WpfGalleryExe = Join-Path $OfficialWpfGalleryRoot "bin\Debug\net10.0-windows\WPFGallery.exe"
}
$OfficialWpfGalleryOutput = Split-Path -Parent $WpfGalleryExe

if ([string]::IsNullOrWhiteSpace($OfficialDirectHostExe)) {
    $OfficialDirectHostExe = Join-Path $RepoRoot "tools\visual-checks\OfficialWpfGalleryDirectHost\bin\Debug\net10.0-windows\OfficialWpfGalleryDirectHost.exe"
}

function New-Case([string]$id, [string]$modernRoute, [string[]]$officialPath) {
    return [ordered]@{
        Id = $id
        ModernRoute = $modernRoute
        OfficialPath = $officialPath
    }
}

$CaseCatalog = @(
    New-Case "Home" "home" @("Home")
    New-Case "WhatsNew" "WhatsNew" @("What's New")
    New-Case "AllControls" "AllControls" @("All Controls")
    New-Case "DesignGuidance" "category/DesignGuidance" @("Design Guidance")
    New-Case "Color" "item/Color" @("Design Guidance", "Colors")
    New-Case "Typography" "item/Typography" @("Design Guidance", "Typography")
    New-Case "Spacing" "item/Spacing" @("Design Guidance", "Spacing")
    New-Case "Geometry" "item/Geometry" @("Design Guidance", "Geometry")
    New-Case "Iconography" "item/Iconography" @("Design Guidance", "Icons")
    New-Case "Samples" "category/Samples" @("Samples")
    New-Case "UserDashboard" "item/UserDashboard" @("Samples", "User Dashboard")
    New-Case "BasicInput" "category/BasicInput" @("Basic Input")
    New-Case "Button" "item/Button" @("Basic Input", "Button")
    New-Case "CheckBox" "item/CheckBox" @("Basic Input", "CheckBox")
    New-Case "ComboBox" "item/ComboBox" @("Basic Input", "ComboBox")
    New-Case "RadioButton" "item/RadioButton" @("Basic Input", "RadioButton")
    New-Case "Slider" "item/Slider" @("Basic Input", "Slider")
    New-Case "Collections" "category/Collections" @("Collections")
    New-Case "DataGrid" "item/DataGrid" @("Collections", "DataGrid")
    New-Case "ListBox" "item/ListBox" @("Collections", "ListBox")
    New-Case "ListView" "item/ListView" @("Collections", "ListView")
    New-Case "TreeView" "item/TreeView" @("Collections", "TreeView")
    New-Case "DateAndCalendar" "category/DateAndCalendar" @("Date & Calendar")
    New-Case "Calendar" "item/Calendar" @("Date & Calendar", "Calendar")
    New-Case "DatePicker" "item/DatePicker" @("Date & Calendar", "DatePicker")
    New-Case "Layout" "category/Layout" @("Layout")
    New-Case "Expander" "item/Expander" @("Layout", "Expander")
    New-Case "Grid" "item/Grid" @("Layout", "Grid")
    New-Case "ResizeGrip" "item/ResizeGrip" @("Layout", "ResizeGrip")
    New-Case "GridSplitter" "item/GridSplitter" @("Layout", "GridSplitter")
    New-Case "GroupBox" "item/GroupBox" @("Layout", "GroupBox")
    New-Case "StackPanel" "item/StackPanel" @("Layout", "StackPanel")
    New-Case "Border" "item/Border" @("Layout", "Border")
    New-Case "Media" "category/Media" @("Media")
    New-Case "Canvas" "item/Canvas" @("Media", "Canvas")
    New-Case "Image" "item/Image" @("Media", "Image")
    New-Case "Navigation" "category/Navigation" @("Navigation")
    New-Case "Menu" "item/Menu" @("Navigation", "Menu")
    New-Case "TabControl" "item/TabControl" @("Navigation", "TabControl")
    New-Case "Frame" "item/Frame" @("Navigation", "Frame")
    New-Case "NavigationWindow" "item/NavigationWindow" @("Navigation", "NavigationWindow")
    New-Case "StatusAndInfo" "category/StatusAndInfo" @("Status & Info")
    New-Case "ProgressBar" "item/ProgressBar" @("Status & Info", "ProgressBar")
    New-Case "ToolTip" "item/ToolTip" @("Status & Info", "ToolTip")
    New-Case "Text" "category/Text" @("Text")
    New-Case "Label" "item/Label" @("Text", "Label")
    New-Case "TextBox" "item/TextBox" @("Text", "TextBox")
    New-Case "TextBlock" "item/TextBlock" @("Text", "TextBlock")
    New-Case "RichTextEdit" "item/RichTextEdit" @("Text", "RichTextEdit")
    New-Case "PasswordBox" "item/PasswordBox" @("Text", "PasswordBox")
    New-Case "Hyperlink" "item/Hyperlink" @("Text", "Hyperlink")
    New-Case "System" "category/System" @("System")
    New-Case "FileAndFolderDialogs" "item/FileAndFolderDialogs" @("System", "File and Folder Dialogs")
    New-Case "MessageBox" "item/MessageBox" @("System", "MessageBox")
    New-Case "Clipboard" "item/Clipboard" @("System", "Clipboard")
    New-Case "Settings" "settings" @("Settings")
)

$OfficialDirectReferenceCaseIds = @(
    "WhatsNew",
    "AllControls",
    "DesignGuidance",
    "Color",
    "Typography",
    "Spacing",
    "Geometry",
    "Iconography",
    "UserDashboard",
    "Button",
    "CheckBox",
    "ComboBox",
    "RadioButton",
    "Slider",
    "Calendar",
    "DatePicker",
    "DataGrid",
    "ListBox",
    "ListView",
    "TreeView",
    "Expander",
    "Grid",
    "ResizeGrip",
    "GridSplitter",
    "GroupBox",
    "StackPanel",
    "ProgressBar",
    "ToolTip",
    "Label",
    "TextBox",
    "TextBlock",
    "RichTextEdit",
    "PasswordBox",
    "Hyperlink",
    "Border",
    "FileAndFolderDialogs",
    "MessageBox",
    "Clipboard",
    "Canvas",
    "Image"
)

function Select-Cases {
    if ($Cases.Count -eq 0) {
        return $CaseCatalog
    }

    $selected = New-Object System.Collections.Generic.List[object]
    foreach ($caseId in $Cases) {
        $match = $CaseCatalog | Where-Object {
            $_.Id -eq $caseId -or
            $_.ModernRoute -eq $caseId -or
            ($_.OfficialPath -join "/") -eq $caseId
        } | Select-Object -First 1

        if ($null -eq $match) {
            throw "Unknown WPF Gallery visual audit case '$caseId'. Run with -ListCases to see valid case IDs."
        }

        $selected.Add($match)
    }

    return $selected.ToArray()
}

function Test-OfficialDirectReferenceCase($case) {
    return $OfficialDirectReferenceCaseIds -contains $case.Id
}

function Ensure-OfficialDirectHostBuilt {
    $projectPath = Join-Path $RepoRoot "tools\visual-checks\OfficialWpfGalleryDirectHost\OfficialWpfGalleryDirectHost.csproj"
    & dotnet build $projectPath -c Debug -p:OfficialWpfGalleryOutput="$OfficialWpfGalleryOutput"
    if ($LASTEXITCODE -ne 0) {
        throw "Official WPF Gallery direct reference host build failed."
    }
}

if ($ListCases) {
    Select-Cases |
        ForEach-Object {
            [pscustomobject]@{
                Id = $_.Id
                ModernRoute = $_.ModernRoute
                OfficialPath = $_.OfficialPath -join " > "
            }
        } |
        Format-Table Id, ModernRoute, OfficialPath -AutoSize
    return
}

if ($BuildModern) {
    & dotnet build (Join-Path $RepoRoot "ModernWpf.Gallery\ModernWpf.Gallery.csproj") -f net8.0-windows7.0 -c Debug --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "ModernWpf.Gallery build failed."
    }
}

if ($BuildOfficial) {
    & dotnet build (Join-Path $OfficialWpfGalleryRoot "WPFGallery.csproj") -f net10.0-windows -c Debug
    if ($LASTEXITCODE -ne 0) {
        throw "Official WPF Gallery build failed."
    }
}

if (!(Test-Path $ModernGalleryExe)) {
    throw "ModernWpf Gallery executable was not found at '$ModernGalleryExe'. Build first or pass -ModernGalleryExe."
}

if ($Reference -eq "OfficialWpfGallery" -and !(Test-Path $WpfGalleryExe)) {
    throw "Official WPF Gallery executable was not found at '$WpfGalleryExe'. Build D:\repos\WPF-Samples\Sample Applications\WPFGallery or pass -WpfGalleryExe."
}

$selectedCases = Select-Cases
if ($Reference -eq "OfficialWpfGallery" -and ($selectedCases | Where-Object { Test-OfficialDirectReferenceCase $_ } | Select-Object -First 1)) {
    Ensure-OfficialDirectHostBuilt
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

public static class WpfGalleryVisualNative
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
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

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
        System.Threading.Thread.Sleep(80);
        mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
        System.Threading.Thread.Sleep(80);
        mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
    }

    public static void PressEnter()
    {
        keybd_event(0x0D, 0, 0, UIntPtr.Zero);
        keybd_event(0x0D, 0, 0x0002, UIntPtr.Zero);
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

function Join-ProcessArguments([string[]]$arguments) {
    return ($arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + ($_ -replace '"', '\"') + '"'
        }
        else {
            $_
        }
    }) -join " "
}

function Start-AppProcess([string]$exe, [string[]]$arguments) {
    return Start-Process -FilePath $exe -ArgumentList (Join-ProcessArguments $arguments) -PassThru
}

function Close-AppProcess($process) {
    if ($null -eq $process) {
        return
    }

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

function Get-RootElement {
    return [System.Windows.Automation.AutomationElement]::RootElement
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

        if ($window.Current.Name -like "*Gallery*") {
            return $window
        }
    }

    return $fallback
}

function Find-ElementByNameAndTypeInProcess([int]$processId, [string]$name, $controlType) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        $match = Find-DescendantByNameAndType $window $name $controlType
        if ($null -ne $match) {
            return $match
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

function Find-DescendantByNameAndType($root, [string]$name, $controlType) {
    $nameCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $name)
    $typeCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        $controlType)
    $condition = New-Object System.Windows.Automation.AndCondition($nameCondition, $typeCondition)
    return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Find-TreeItemByName($root, [string]$name) {
    $exact = Find-DescendantByNameAndType $root $name ([System.Windows.Automation.ControlType]::TreeItem)
    if ($null -ne $exact) {
        return $exact
    }

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::TreeItem)
    $items = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    foreach ($item in $items) {
        if ($item.Current.Name -like "*$name*") {
            return $item
        }
    }

    return $null
}

function Invoke-Element($element) {
    if ($null -eq $element) {
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
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Select()
            Start-Sleep -Milliseconds 100
        }
    }
    catch {
    }

    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
        [WpfGalleryVisualNative]::Click(
            [int][Math]::Round($rect.X + ($rect.Width / 2)),
            [int][Math]::Round($rect.Y + ($rect.Height / 2)))
        return $true
    }

    return $false
}

function Invoke-LegacyDefaultAction($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.LegacyIAccessiblePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.DoDefaultAction()
            Start-Sleep -Milliseconds 250
            return $true
        }
    }
    catch {
    }

    return $false
}

function Click-Element($element) {
    if ($null -eq $element) {
        return $false
    }

    $rect = $element.Current.BoundingRectangle
    if ($rect.Width -le 0 -or $rect.Height -le 0) {
        return $false
    }

    [WpfGalleryVisualNative]::Click(
        [int][Math]::Round($rect.X + ($rect.Width / 2)),
        [int][Math]::Round($rect.Y + ($rect.Height / 2)))
    return $true
}

function Click-TreeItemHeader($item, [string]$name) {
    if ($null -eq $item) {
        return $false
    }

    $text = Find-DescendantByNameAndType $item $name ([System.Windows.Automation.ControlType]::Text)
    if ($null -eq $text) {
        return $false
    }

    return Click-Element $text
}


function Expand-Element($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        if ($null -ne $pattern -and $pattern.Current.ExpandCollapseState -ne [System.Windows.Automation.ExpandCollapseState]::Expanded) {
            $pattern.Expand()
            Start-Sleep -Milliseconds 250
        }

        return $true
    }
    catch {
        return Invoke-Element $element
    }
}

function Navigate-OfficialWpfGallery($window, $case) {
    [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $path = $case.OfficialPath
    if ($path.Count -eq 1 -and $path[0] -eq "Settings") {
        $settings = Find-DescendantByNameAndType $window "Settings" ([System.Windows.Automation.ControlType]::Button)
        if ($null -eq $settings -or !(Invoke-Element $settings)) {
            throw "Could not invoke official WPF Gallery Settings button."
        }

        Start-Sleep -Milliseconds 800
        return
    }

    for ($i = 0; $i -lt $path.Count; $i++) {
        $name = $path[$i]
        $item = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "official WPF Gallery navigation item '$name'" -Probe {
            Find-TreeItemByName $window $name
        }

        if ($i -lt ($path.Count - 1)) {
            [void](Expand-Element $item)
        }
        else {
            [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
            try {
                $item.SetFocus()
                $selectionPattern = $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
                if ($null -ne $selectionPattern) {
                    $selectionPattern.Select()
                }

                $tree = Find-DescendantByAutomationId $window "ControlsList"
                if ($null -ne $tree) {
                    $tree.SetFocus()
                }
            }
            catch {
            }

            [WpfGalleryVisualNative]::PressEnter()
            Start-Sleep -Milliseconds 250
            $clicked = Click-TreeItemHeader $item $name
            if (!$clicked) {
                $clicked = Click-Element $item
            }

            $legacyInvoked = Invoke-LegacyDefaultAction $item
            if (!$clicked -and !$legacyInvoked) {
                throw "Could not invoke official WPF Gallery navigation item '$name'."
            }
        }
    }

    try {
        Wait-OfficialWpfGalleryContentReady $window $case
    }
    catch {
        $treeNavigationException = $_.Exception.Message
        try {
            Navigate-OfficialWpfGalleryByCards $window $case
            Wait-OfficialWpfGalleryContentReady $window $case
        }
        catch {
            throw "$treeNavigationException; card fallback: $($_.Exception.Message)"
        }
    }

    Start-Sleep -Milliseconds 1000
}

function Navigate-OfficialWpfGalleryByCards($window, $case) {
    $path = $case.OfficialPath
    if ($path.Count -eq 0 -or $path.Count -gt 2) {
        throw "No card navigation fallback is defined for '$($path -join " > ")'."
    }

    if ($path.Count -eq 1) {
        Invoke-OfficialContentButton $window ($path[0] + "Page")
        return
    }

    Invoke-OfficialContentButton $window ($path[0] + "Page")
    $parentCase = New-Case $path[0] "" @($path[0])
    Wait-OfficialWpfGalleryContentReady $window $parentCase
    Invoke-OfficialContentButton $window ($path[1] + "Page")
}

function Invoke-OfficialContentButton($window, [string]$name) {
    $button = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "official WPF Gallery content button '$name'" -Probe {
        $frame = Find-DescendantByAutomationId $window "RootContentFrame"
        if ($null -eq $frame) {
            return $null
        }

        return Find-DescendantByNameAndType $frame $name ([System.Windows.Automation.ControlType]::Button)
    }

    if (!(Invoke-Element $button)) {
        throw "Could not invoke official WPF Gallery content button '$name'."
    }

    Start-Sleep -Milliseconds 700
}

function Get-OfficialWpfGalleryReadyText($case) {
    switch ($case.Id) {
        "Home" { return ".NET 10" }
        "WhatsNew" { return "What's new in WPF Page" }
        "Media" { return "Media Controls Page" }
        "NavigationWindow" { return "Navigation Window Page" }
        "Settings" { return "Settings Page" }
        default { return "$($case.OfficialPath[$case.OfficialPath.Count - 1]) Page" }
    }
}

function Wait-OfficialWpfGalleryContentReady($window, $case, [int]$waitTimeoutSeconds = $TimeoutSeconds) {
    if ($case.Id -eq "UserDashboard") {
        Wait-Until -TimeoutSeconds $waitTimeoutSeconds -Description "official WPF Gallery content 'User Dashboard'" -Probe {
            $frame = Find-DescendantByAutomationId $window "RootContentFrame"
            if ($null -eq $frame) {
                return $null
            }

            $users = Find-DescendantByAutomationId $frame "UserList"
            if ($null -ne $users) {
                return $users
            }

            return Find-DescendantByAutomationId $frame "NewUserButton"
        } | Out-Null
        return
    }

    $readyText = Get-OfficialWpfGalleryReadyText $case
    if ([string]::IsNullOrWhiteSpace($readyText)) {
        return
    }

    Wait-Until -TimeoutSeconds $waitTimeoutSeconds -Description "official WPF Gallery content '$readyText'" -Probe {
        $frame = Find-DescendantByAutomationId $window "RootContentFrame"
        if ($null -eq $frame) {
            return $null
        }

        return Find-DescendantByNameAndType $frame $readyText ([System.Windows.Automation.ControlType]::Text)
    } | Out-Null
}

function Return-OfficialWpfGalleryToHome($window) {
    [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
    $homeCase = New-Case "Home" "home" @("Home")
    $backButton = Find-DescendantByAutomationId $window "BackButton"
    if ($null -ne $backButton) {
        [void](Invoke-Element $backButton)
        [void](Click-Element $backButton)
        Start-Sleep -Milliseconds 700
    }

    try {
        Wait-OfficialWpfGalleryContentReady $window $homeCase 3
        return
    }
    catch {
    }

    $homeItem = Find-TreeItemByName $window "Home"
    if ($null -ne $homeItem) {
        try {
            $homeItem.SetFocus()
        }
        catch {
        }

        [WpfGalleryVisualNative]::PressEnter()
        [void](Click-Element $homeItem)
        Wait-OfficialWpfGalleryContentReady $window $homeCase
    }
}

function Ensure-OfficialWpfGalleryTheme([int]$processId, $window) {
    if ($Theme -eq "Default") {
        return [ordered]@{
            RequestedTheme = $Theme
            Status = "Skipped"
            LastException = "Default theme requested."
        }
    }

    try {
        [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
        $settings = Find-DescendantByNameAndType $window "Settings" ([System.Windows.Automation.ControlType]::Button)
        if ($null -eq $settings -or !(Invoke-Element $settings)) {
            throw "Could not invoke official WPF Gallery Settings button."
        }

        $comboBox = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "official WPF Gallery theme ComboBox" -Probe {
            Find-DescendantByNameAndType $window "Change ThemeMode" ([System.Windows.Automation.ControlType]::ComboBox)
        }

        [void](Expand-Element $comboBox)
        $item = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "official WPF Gallery theme item '$Theme'" -Probe {
            $match = Find-DescendantByNameAndType $comboBox $Theme ([System.Windows.Automation.ControlType]::ListItem)
            if ($null -ne $match) {
                return $match
            }

            return Find-ElementByNameAndTypeInProcess $processId $Theme ([System.Windows.Automation.ControlType]::ListItem)
        }

        [void](Invoke-Element $item)
        [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
        [void](Click-Element $item)

        Start-Sleep -Milliseconds 700
        return [ordered]@{
            RequestedTheme = $Theme
            Status = "Passed"
            LastException = ""
        }
    }
    catch {
        return [ordered]@{
            RequestedTheme = $Theme
            Status = "Failed"
            LastException = $_.Exception.Message
        }
    }
}

function Get-AutomationText($root, [string]$automationId) {
    $element = Find-DescendantByAutomationId $root $automationId
    if ($null -eq $element) {
        return ""
    }

    return $element.Current.Name
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
    $colors = New-Object "System.Collections.Generic.Dictionary[int,int]"
    $visibleSamples = 0
    $nonBlackSamples = 0
    $stepX = [Math]::Max(1, [int]($bitmap.Width / 32))
    $stepY = [Math]::Max(1, [int]($bitmap.Height / 32))
    for ($x = 0; $x -lt $bitmap.Width; $x += $stepX) {
        for ($y = 0; $y -lt $bitmap.Height; $y += $stepY) {
            $pixel = $bitmap.GetPixel($x, $y)
            if ($pixel.A -gt 16) {
                $visibleSamples++
                $colorKey = ($pixel.R -shl 16) -bor ($pixel.G -shl 8) -bor $pixel.B
                if (!$colors.ContainsKey($colorKey)) {
                    $colors[$colorKey] = 0
                }

                $colors[$colorKey]++
                if (($pixel.R + $pixel.G + $pixel.B) -gt 36) {
                    $nonBlackSamples++
                }
            }
        }
    }

    $dominantSamples = 0
    foreach ($count in $colors.Values) {
        $dominantSamples = [Math]::Max($dominantSamples, $count)
    }

    return $colors.Count -gt 4 -and
        $visibleSamples -gt 0 -and
        ($nonBlackSamples / [double]$visibleSamples) -gt 0.1 -and
        ($dominantSamples / [double]$visibleSamples) -lt 0.95
}

function Test-ImageNotBlank([string]$path) {
    if (!(Test-Path $path)) {
        return $false
    }

    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        return Test-BitmapNotBlank $bitmap
    }
    finally {
        $bitmap.Dispose()
    }
}

function Get-ImageArtifactInfo([string]$path, [string]$source) {
    if ([string]::IsNullOrWhiteSpace($path) -or !(Test-Path $path)) {
        return [ordered]@{
            Found = $false
            Source = $source
            Screenshot = ""
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        return [ordered]@{
            Found = $true
            Source = $source
            Screenshot = $path
            Width = $bitmap.Width
            Height = $bitmap.Height
            NonBlank = Test-BitmapNotBlank $bitmap
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Test-ModernRenderedContentArtifact([string]$artifactDir) {
    foreach ($fileName in @("ContentRootGrid.png", "ContentPagePane.png", "GalleryContentHost.png")) {
        $path = Join-Path $artifactDir $fileName
        try {
            if (Test-ImageNotBlank $path) {
                return $true
            }
        }
        catch {
        }
    }

    return $false
}

function Capture-Window([IntPtr]$hwnd, [string]$path) {
    [WpfGalleryVisualNative]::Activate($hwnd)
    Start-Sleep -Milliseconds 300
    $rect = [WpfGalleryVisualNative]::GetRect($hwnd)
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
                    $printed = [WpfGalleryVisualNative]::PrintWindow($hwnd, $hdc, [uint32]$flags)
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
            $copied = [WpfGalleryVisualNative]::CopyWindowSurface($hwnd, $hdc, $width, $height)
        }
        finally {
            $graphics.ReleaseHdc($hdc)
        }

        if ($copied -and (Test-BitmapNotBlank $bitmap)) {
            $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
            return
        }
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }

    throw "Could not capture a nonblank window surface for handle $hwnd."
}

function Capture-ScreenRect([IntPtr]$hwnd, [string]$path) {
    $rect = [WpfGalleryVisualNative]::GetRect($hwnd)
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

function Save-Crop([string]$screenshot, [string]$path, [int]$left, [int]$top, [int]$width, [int]$height, [string]$source) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($screenshot)
    try {
        $x = [Math]::Max(0, $left)
        $y = [Math]::Max(0, $top)
        $right = [Math]::Min($bitmap.Width, $left + $width)
        $bottom = [Math]::Min($bitmap.Height, $top + $height)
        $cropWidth = [Math]::Max(0, $right - $x)
        $cropHeight = [Math]::Max(0, $bottom - $y)
        if ($cropWidth -le 0 -or $cropHeight -le 0) {
            return [ordered]@{
                Found = $false
                Source = $source
                Screenshot = ""
                Width = 0
                Height = 0
                NonBlank = $false
            }
        }

        $crop = [System.Drawing.Bitmap]::new($cropWidth, $cropHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($crop)
        try {
            $graphics.DrawImage(
                $bitmap,
                [System.Drawing.Rectangle]::new(0, 0, $cropWidth, $cropHeight),
                [System.Drawing.Rectangle]::new($x, $y, $cropWidth, $cropHeight),
                [System.Drawing.GraphicsUnit]::Pixel)
            $nonBlank = Test-BitmapNotBlank $crop
            $crop.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
            return [ordered]@{
                Found = $true
                Source = $source
                Screenshot = $path
                Width = $cropWidth
                Height = $cropHeight
                NonBlank = $nonBlank
            }
        }
        finally {
            $graphics.Dispose()
            $crop.Dispose()
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Save-ElementCrop($window, [string]$screenshot, [string]$path, $element, [string]$source, [int]$padding) {
    if ($null -eq $element) {
        return [ordered]@{
            Found = $false
            Source = $source
            Screenshot = ""
            Width = 0
            Height = 0
            NonBlank = $false
        }
    }

    $windowRect = [WpfGalleryVisualNative]::GetRect($window.Current.NativeWindowHandle)
    $elementRect = $element.Current.BoundingRectangle
    return Save-Crop `
        $screenshot `
        $path `
        ([int][Math]::Floor($elementRect.X - $windowRect.Left - $padding)) `
        ([int][Math]::Floor($elementRect.Y - $windowRect.Top - $padding)) `
        ([int][Math]::Ceiling($elementRect.Width + ($padding * 2))) `
        ([int][Math]::Ceiling($elementRect.Height + ($padding * 2))) `
        $source
}

function Save-OfficialContentCrop($window, [string]$screenshot, [string]$path, $case) {
    if ($case.Id -eq "Home") {
        $frame = Find-DescendantByAutomationId $window "RootContentFrame"
        if ($null -ne $frame) {
            $paneCondition = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Pane)
            $panes = $frame.FindAll([System.Windows.Automation.TreeScope]::Children, $paneCondition)
            foreach ($pane in $panes) {
                $paneRect = $pane.Current.BoundingRectangle
                if ($paneRect.Width -gt 0 -and $paneRect.Height -gt 0) {
                    $paneCrop = Save-ElementCrop $window $screenshot $path $pane "OfficialHomeContentRootPane" 0
                    if ($paneCrop.NonBlank) {
                        return $paneCrop
                    }
                }
            }
        }
    }

    if ($case.Id -ne "Home") {
        $frame = Find-DescendantByAutomationId $window "RootContentFrame"
        if ($null -ne $frame) {
            $frameCrop = Save-ElementCrop $window $screenshot $path $frame "OfficialRootContentFrame" 0
            if ($frameCrop.NonBlank) {
                return $frameCrop
            }
        }
    }

    $windowRect = [WpfGalleryVisualNative]::GetRect($window.Current.NativeWindowHandle)
    $windowWidth = $windowRect.Right - $windowRect.Left
    $windowHeight = $windowRect.Bottom - $windowRect.Top
    $left = 260
    $top = 44

    $navigationPane = Find-DescendantByNameAndType $window "Navigation Pane" ([System.Windows.Automation.ControlType]::Tree)
    if ($null -ne $navigationPane) {
        $paneRect = $navigationPane.Current.BoundingRectangle
        if ($paneRect.Width -gt 0) {
            $left = [int][Math]::Round(($paneRect.X + $paneRect.Width) - $windowRect.Left + 4)
        }
    }

    return Save-Crop $screenshot $path $left $top ($windowWidth - $left) ($windowHeight - $top) "OfficialContentRegion"
}

function Save-ModernContentCrop($window, [string]$screenshot, [string]$path, $case) {
    if ($null -eq $case -or $case.Id -eq "Home") {
        $content = Find-DescendantByAutomationId $window "GalleryContentHost"
        if ($null -ne $content) {
            return Save-ElementCrop $window $screenshot $path $content "GalleryContentHost" 0
        }
    }

    $windowRect = [WpfGalleryVisualNative]::GetRect($window.Current.NativeWindowHandle)
    $windowWidth = $windowRect.Right - $windowRect.Left
    $windowHeight = $windowRect.Bottom - $windowRect.Top
    $root = Find-DescendantByAutomationId $window "GalleryNavigationRoot"
    $menu = Find-DescendantByAutomationId $window "MenuItemsHost"

    # Normal section pages compare against the official RootContentFrame, so trim
    # ModernWpf's navigation host padding instead of using the broader Home crop.
    $normalContentLeftInset = 31
    $normalContentTopInset = 17
    $normalContentRightInset = 25
    $normalContentBottomInset = 1
    if (($null -eq $case -or $case.Id -ne "Home") -and $null -ne $root -and $null -ne $menu) {
        $rootRect = $root.Current.BoundingRectangle
        $menuRect = $menu.Current.BoundingRectangle
        $left = [int][Math]::Round(($menuRect.X + $menuRect.Width) - $windowRect.Left + $normalContentLeftInset)
        $top = [int][Math]::Round($rootRect.Y - $windowRect.Top + $normalContentTopInset)
        $right = [int][Math]::Round(($rootRect.X + $rootRect.Width) - $windowRect.Left - $normalContentRightInset)
        $bottom = [int][Math]::Round(($rootRect.Y + $rootRect.Height) - $windowRect.Top - $normalContentBottomInset)
        return Save-Crop $screenshot $path $left $top ($right - $left) ($bottom - $top) "ModernWpfGalleryContentHost"
    }

    $homeContentLeftInset = 7
    $homeContentTopInset = 2
    $homeContentRightInset = 1
    $homeContentBottomInset = 1
    if ($null -ne $root -and $null -ne $menu) {
        $rootRect = $root.Current.BoundingRectangle
        $menuRect = $menu.Current.BoundingRectangle
        $left = [int][Math]::Round(($menuRect.X + $menuRect.Width) - $windowRect.Left + $homeContentLeftInset)
        $top = [int][Math]::Round($rootRect.Y - $windowRect.Top + $homeContentTopInset)
        $right = [int][Math]::Round(($rootRect.X + $rootRect.Width) - $windowRect.Left - $homeContentRightInset)
        $bottom = [int][Math]::Round(($rootRect.Y + $rootRect.Height) - $windowRect.Top - $homeContentBottomInset)
        return Save-Crop $screenshot $path $left $top ($right - $left) ($bottom - $top) "ModernWpfContentRegion"
    }

    if ($null -eq $case -or $case.Id -ne "Home") {
        $fallbackLeft = 287
        $fallbackTop = 61
        $fallbackRightInset = 312
        $fallbackBottomInset = 62
        return Save-Crop $screenshot $path $fallbackLeft $fallbackTop ($windowWidth - $fallbackRightInset) ($windowHeight - $fallbackBottomInset) "ModernWpfContentFallback"
    }

    return Save-Crop $screenshot $path 320 40 ($windowWidth - 320) 760 "ModernWpfContentFallback"
}

function Compare-ImagesNormalized([string]$leftPath, [string]$rightPath, [int]$sampleStep = 4) {
    if ([string]::IsNullOrWhiteSpace($leftPath) -or [string]::IsNullOrWhiteSpace($rightPath) -or
        !(Test-Path $leftPath) -or !(Test-Path $rightPath)) {
        return [ordered]@{
            Comparable = $false
            MeanDelta = $null
            LeftSize = ""
            RightSize = ""
        }
    }

    $left = [System.Drawing.Bitmap]::FromFile($leftPath)
    $right = [System.Drawing.Bitmap]::FromFile($rightPath)
    try {
        $width = [Math]::Min($left.Width, $right.Width)
        $height = [Math]::Min($left.Height, $right.Height)
        if ($width -le 0 -or $height -le 0) {
            return [ordered]@{
                Comparable = $false
                MeanDelta = $null
                LeftSize = "$($left.Width)x$($left.Height)"
                RightSize = "$($right.Width)x$($right.Height)"
            }
        }

        $delta = 0.0
        $samples = 0
        for ($y = 0; $y -lt $height; $y += $sampleStep) {
            for ($x = 0; $x -lt $width; $x += $sampleStep) {
                $leftX = [Math]::Min($left.Width - 1, [int][Math]::Floor($x * $left.Width / [double]$width))
                $leftY = [Math]::Min($left.Height - 1, [int][Math]::Floor($y * $left.Height / [double]$height))
                $rightX = [Math]::Min($right.Width - 1, [int][Math]::Floor($x * $right.Width / [double]$width))
                $rightY = [Math]::Min($right.Height - 1, [int][Math]::Floor($y * $right.Height / [double]$height))
                $leftPixel = $left.GetPixel($leftX, $leftY)
                $rightPixel = $right.GetPixel($rightX, $rightY)
                $delta += ([Math]::Abs($leftPixel.R - $rightPixel.R) +
                    [Math]::Abs($leftPixel.G - $rightPixel.G) +
                    [Math]::Abs($leftPixel.B - $rightPixel.B)) / 3.0
                $samples++
            }
        }

        return [ordered]@{
            Comparable = $samples -gt 0
            MeanDelta = $(if ($samples -gt 0) { [Math]::Round($delta / $samples, 2) } else { $null })
            LeftSize = "$($left.Width)x$($left.Height)"
            RightSize = "$($right.Width)x$($right.Height)"
        }
    }
    finally {
        $left.Dispose()
        $right.Dispose()
    }
}

function Capture-ModernWpf($case, [string]$caseDir) {
    $artifactDir = Join-Path $caseDir "modernwpf-artifacts"
    New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
    $process = Start-AppProcess $ModernGalleryExe @(
        "--visual-test",
        "--route", $case.ModernRoute,
        "--theme", $Theme,
        "--visual-artifact-dir", $artifactDir)

    try {
        $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf Gallery window for $($case.Id)" -Probe {
            Find-WindowByProcessId $process.Id
        }
        [void][WpfGalleryVisualNative]::Move($window.Current.NativeWindowHandle, 60, 60, $Width, $Height)
        Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "ModernWpf route '$($case.ModernRoute)' to become ready" -Probe {
            $readyElement = Find-DescendantByAutomationId $window "GalleryVisualTestReadyState"
            if ($null -ne $readyElement -and $readyElement.Current.Name -eq "Ready:$($case.ModernRoute)") {
                return $readyElement
            }

            if ($null -ne $readyElement -and $readyElement.Current.Name -like "Failed:*") {
                return $null
            }

            if (Test-ModernRenderedContentArtifact $artifactDir) {
                return [pscustomobject]@{
                    Source = "RenderedContentArtifact"
                }
            }

            return $null
        } | Out-Null

        Start-Sleep -Milliseconds 500
        $screenshot = Join-Path $caseDir "modernwpf-$($case.Id).png"
        $treePath = Join-Path $caseDir "modernwpf-$($case.Id).uia.txt"
        $contentCropPath = Join-Path $caseDir "modernwpf-$($case.Id)-content.png"
        Capture-Window $window.Current.NativeWindowHandle $screenshot
        Write-UiaTree $window $treePath 7
        $windowNonBlank = Test-ImageNotBlank $screenshot

        $contentCrop = $null
        if ($case.Id -ne "Home") {
            $renderedContentArtifact = Join-Path $artifactDir "ContentRootGrid.png"
            $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "ContentRootGridRenderedArtifact"
            if (!$contentCrop.NonBlank) {
                $renderedContentArtifact = Join-Path $artifactDir "ContentPagePane.png"
                $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "ContentPagePaneRenderedArtifact"
            }
            if (!$contentCrop.NonBlank) {
                $renderedContentArtifact = Join-Path $artifactDir "GalleryContentHost.png"
                $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "GalleryContentHostRenderedArtifact"
            }
        }

        if (($null -eq $contentCrop -or !$contentCrop.NonBlank) -and
            !(Test-OfficialDirectReferenceCase $case) -and
            $windowNonBlank) {
            $contentCrop = Save-ModernContentCrop $window $screenshot $contentCropPath $case
        }

        if ($null -eq $contentCrop -or !$contentCrop.NonBlank) {
            $renderedContentArtifact = Join-Path $artifactDir "ContentRootGrid.png"
            $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "ContentRootGridRenderedArtifact"
            if (!$contentCrop.NonBlank) {
                $renderedContentArtifact = Join-Path $artifactDir "ContentPagePane.png"
                $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "ContentPagePaneRenderedArtifact"
            }
            if (!$contentCrop.NonBlank) {
                $renderedContentArtifact = Join-Path $artifactDir "GalleryContentHost.png"
                $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "GalleryContentHostRenderedArtifact"
            }
        }

        if ($null -eq $contentCrop -or !$contentCrop.NonBlank) {
            $contentCrop = Save-ModernContentCrop $window $screenshot $contentCropPath $case
        }

        $lastException = Get-AutomationText $window "GalleryVisualTestLastException"

        return [ordered]@{
            App = "ModernWpf"
            Case = $case.Id
            Route = $case.ModernRoute
            Status = $(if (($windowNonBlank -or $contentCrop.NonBlank) -and $contentCrop.NonBlank -and [string]::IsNullOrWhiteSpace($lastException)) { "Passed" } else { "Failed" })
            Screenshot = $screenshot
            ContentCrop = $contentCrop
            WindowNonBlank = $windowNonBlank
            UiaTree = $treePath
            LastException = $lastException
        }
    }
    finally {
        Close-AppProcess $process
    }
}

function Capture-OfficialWpfGalleryDirectHost($case, [string]$caseDir) {
    $artifactDir = Join-Path $caseDir "official-wpf-artifacts"
    New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
    $process = Start-AppProcess $OfficialDirectHostExe @(
        "--page", $case.Id,
        "--theme", $Theme,
        "--official-output", $OfficialWpfGalleryOutput,
        "--width", $Width,
        "--height", $Height,
        "--visual-artifact-dir", $artifactDir)
    $window = $null
    try {
        $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "official WPF Gallery direct reference window for $($case.Id)" -Probe {
            Find-WindowByProcessId $process.Id
        }
        [void][WpfGalleryVisualNative]::Move($window.Current.NativeWindowHandle, 60, 60, $Width, $Height)
        [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
        Wait-OfficialWpfGalleryContentReady $window $case
        Start-Sleep -Milliseconds 500

        $screenshot = Join-Path $caseDir "official-wpf-$($case.Id).png"
        $treePath = Join-Path $caseDir "official-wpf-$($case.Id).uia.txt"
        $contentCropPath = Join-Path $caseDir "official-wpf-$($case.Id)-content.png"
        Capture-Window $window.Current.NativeWindowHandle $screenshot
        Write-UiaTree $window $treePath 7
        $frame = Find-DescendantByAutomationId $window "RootContentFrame"
        $renderedContentArtifact = Join-Path $artifactDir "RootContentFrame.png"
        $contentCrop = Get-ImageArtifactInfo $renderedContentArtifact "OfficialDirectRootContentFrameRenderedArtifact"
        if (!$contentCrop.NonBlank) {
            $contentCrop = Save-ElementCrop $window $screenshot $contentCropPath $frame "OfficialDirectRootContentFrame" 0
        }

        return [ordered]@{
            App = "OfficialWpfGallery"
            Case = $case.Id
            Route = "Direct reference host: $($case.Id)"
            Status = $(if ((Test-ImageNotBlank $screenshot) -and $contentCrop.NonBlank) { "Passed" } else { "Failed" })
            Screenshot = $screenshot
            ContentCrop = $contentCrop
            UiaTree = $treePath
            ThemeProbe = [ordered]@{
                RequestedTheme = $Theme
                Status = "Passed"
                LastException = "Direct reference host"
            }
            LastException = ""
        }
    }
    finally {
        Close-AppProcess $process
    }
}

function Capture-OfficialWpfGallery($case, [string]$caseDir) {
    if ($Reference -eq "None") {
        return [ordered]@{
            App = "OfficialWpfGallery"
            Case = $case.Id
            Route = $case.OfficialPath -join " > "
            Status = "Skipped"
            Screenshot = ""
            ContentCrop = $null
            UiaTree = ""
            LastException = "Reference=None"
        }
    }

    if (Test-OfficialDirectReferenceCase $case) {
        return Capture-OfficialWpfGalleryDirectHost $case $caseDir
    }

    $process = Start-AppProcess $WpfGalleryExe @()
    $window = $null
    $themeProbe = $null
    try {
        $window = Wait-Until -TimeoutSeconds $TimeoutSeconds -Description "official WPF Gallery window for $($case.Id)" -Probe {
            Find-WindowByProcessId $process.Id
        }
        [void][WpfGalleryVisualNative]::Move($window.Current.NativeWindowHandle, 60, 60, $Width, $Height)
        [WpfGalleryVisualNative]::Activate($window.Current.NativeWindowHandle)
        Start-Sleep -Milliseconds 700
        $themeProbe = Ensure-OfficialWpfGalleryTheme $process.Id $window
        if ($case.Id -ne "Settings") {
            Return-OfficialWpfGalleryToHome $window
        }
        Navigate-OfficialWpfGallery $window $case
        Start-Sleep -Milliseconds 500

        $screenshot = Join-Path $caseDir "official-wpf-$($case.Id).png"
        $treePath = Join-Path $caseDir "official-wpf-$($case.Id).uia.txt"
        $contentCropPath = Join-Path $caseDir "official-wpf-$($case.Id)-content.png"
        Capture-Window $window.Current.NativeWindowHandle $screenshot
        Write-UiaTree $window $treePath 7
        $contentCrop = Save-OfficialContentCrop $window $screenshot $contentCropPath $case

        $status = if ((Test-ImageNotBlank $screenshot) -and $contentCrop.NonBlank -and $themeProbe.Status -ne "Failed") { "Passed" } else { "Failed" }
        return [ordered]@{
            App = "OfficialWpfGallery"
            Case = $case.Id
            Route = $case.OfficialPath -join " > "
            Status = $status
            Screenshot = $screenshot
            ContentCrop = $contentCrop
            UiaTree = $treePath
            ThemeProbe = $themeProbe
            LastException = $(if ($themeProbe.Status -eq "Failed") { $themeProbe.LastException } else { "" })
        }
    }
    catch {
        $lastException = $_.Exception.Message
        $screenshot = ""
        $treePath = ""
        $contentCrop = $null

        if ($null -ne $window) {
            try {
                $screenshot = Join-Path $caseDir "official-wpf-$($case.Id)-failure.png"
                $treePath = Join-Path $caseDir "official-wpf-$($case.Id)-failure.uia.txt"
                $contentCropPath = Join-Path $caseDir "official-wpf-$($case.Id)-failure-content.png"
                Capture-Window $window.Current.NativeWindowHandle $screenshot
                Write-UiaTree $window $treePath 7
                $contentCrop = Save-OfficialContentCrop $window $screenshot $contentCropPath $case
            }
            catch {
                if ([string]::IsNullOrWhiteSpace($lastException)) {
                    $lastException = $_.Exception.Message
                }
                else {
                    $lastException = "$lastException; failure capture: $($_.Exception.Message)"
                }
            }
        }

        return [ordered]@{
            App = "OfficialWpfGallery"
            Case = $case.Id
            Route = $case.OfficialPath -join " > "
            Status = "Failed"
            Screenshot = $screenshot
            ContentCrop = $contentCrop
            UiaTree = $treePath
            ThemeProbe = $themeProbe
            LastException = $lastException
        }
    }
    finally {
        Close-AppProcess $process
    }
}

$runDir = New-RunDirectory
$results = New-Object System.Collections.Generic.List[object]

foreach ($case in $selectedCases) {
    $caseDir = Join-Path $runDir (ConvertTo-SafeName $case.Id)
    New-Item -ItemType Directory -Force -Path $caseDir | Out-Null

    $modernResult = $null
    try {
        $modernResult = Capture-ModernWpf $case $caseDir
    }
    catch {
        $modernResult = [ordered]@{
            App = "ModernWpf"
            Case = $case.Id
            Route = $case.ModernRoute
            Status = "Failed"
            Screenshot = ""
            ContentCrop = $null
            UiaTree = ""
            LastException = $_.Exception.Message
        }
    }
    $results.Add($modernResult)

    $officialResult = $null
    try {
        $officialResult = Capture-OfficialWpfGallery $case $caseDir
    }
    catch {
        $officialResult = [ordered]@{
            App = "OfficialWpfGallery"
            Case = $case.Id
            Route = $case.OfficialPath -join " > "
            Status = "Failed"
            Screenshot = ""
            ContentCrop = $null
            UiaTree = ""
            LastException = $_.Exception.Message
        }
    }
    $results.Add($officialResult)

    if ($null -ne $modernResult.ContentCrop -and $null -ne $officialResult.ContentCrop -and
        $modernResult.ContentCrop.Found -and $officialResult.ContentCrop.Found) {
        $comparison = Compare-ImagesNormalized $modernResult.ContentCrop.Screenshot $officialResult.ContentCrop.Screenshot
        $modernResult["OfficialContentComparison"] = $comparison
        if ($FailOnDifference -and $comparison.Comparable -and $comparison.MeanDelta -gt 24) {
            $modernResult["Status"] = "Failed"
            $modernResult["LastException"] = "Mean content crop delta $($comparison.MeanDelta) exceeded visual threshold 24."
        }
    }
}

$reportJson = Join-Path $runDir "report.json"
$reportMarkdown = Join-Path $runDir "report.md"
$results.ToArray() | ConvertTo-Json -Depth 7 | Set-Content -Path $reportJson -Encoding UTF8

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add("# WPF Gallery Visual Audit Report")
$markdown.Add("")
$markdown.Add("- Theme: $Theme")
$markdown.Add("- Size: ${Width}x${Height}")
$markdown.Add("- Reference: $Reference")
$markdown.Add("- ModernWpf executable: $ModernGalleryExe")
if ($Reference -eq "OfficialWpfGallery") {
    $markdown.Add("- Official WPF Gallery executable: $WpfGalleryExe")
    if ($selectedCases | Where-Object { Test-OfficialDirectReferenceCase $_ } | Select-Object -First 1) {
        $markdown.Add("- Official WPF Gallery direct reference host: $OfficialDirectHostExe")
    }
}
$markdown.Add("")
$markdown.Add("| Case | Modern status | Official status | Content delta | Modern crop | Official crop | Notes |")
$markdown.Add("| --- | --- | --- | ---: | --- | --- | --- |")

foreach ($case in $selectedCases) {
    $modern = $results | Where-Object { $_.App -eq "ModernWpf" -and $_.Case -eq $case.Id } | Select-Object -First 1
    $official = $results | Where-Object { $_.App -eq "OfficialWpfGallery" -and $_.Case -eq $case.Id } | Select-Object -First 1
    $comparison = if ($modern.Contains("OfficialContentComparison")) { $modern.OfficialContentComparison } else { $null }
    $delta = if ($null -ne $comparison -and $comparison.Comparable) { $comparison.MeanDelta } else { "" }
    $modernCrop = if ($null -ne $modern.ContentCrop -and $modern.ContentCrop.Found) { "$($modern.ContentCrop.Width)x$($modern.ContentCrop.Height)" } else { "" }
    $officialCrop = if ($null -ne $official.ContentCrop -and $official.ContentCrop.Found) { "$($official.ContentCrop.Width)x$($official.ContentCrop.Height)" } else { "" }
    $notes = @($modern.LastException, $official.LastException) | Where-Object { ![string]::IsNullOrWhiteSpace($_) }
    $markdown.Add("| $($case.Id) | $($modern.Status) | $($official.Status) | $delta | $modernCrop | $officialCrop | $($notes -join '; ') |")
}

$markdown.Add("")
$markdown.Add("Screenshots, content crops, UIA trees, and JSON results are beside this report.")
$markdown | Set-Content -Path $reportMarkdown -Encoding UTF8

$failed = @($results | Where-Object { $_.Status -eq "Failed" })
Write-Host "WPF Gallery visual audit artifacts: $runDir"
Write-Host "Report: $reportMarkdown"
if ($failed.Count -gt 0) {
    $failed |
        ForEach-Object {
            [pscustomobject]@{
                App = $_.App
                Case = $_.Case
                Status = $_.Status
                LastException = $_.LastException
            }
        } |
        Format-Table App, Case, Status, LastException -AutoSize
    exit 1
}
