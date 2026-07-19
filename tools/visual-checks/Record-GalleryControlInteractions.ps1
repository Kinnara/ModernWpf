param(
    [string[]]$Controls = @("TeachingTip", "Button", "CheckBox", "ComboBox", "RadioButton", "Slider", "ColorPicker", "HyperlinkButton", "RatingControl", "RepeatButton", "ToggleButton", "DropDownButton", "SplitButton", "ToggleSplitButton", "ToggleSwitch", "NumberBox", "AutoSuggestBox", "SplitView", "PersonPicture", "IconElement", "ThemeShadow", "TitleBar", "InfoBadge", "InfoBar", "ProgressRing", "AnnotatedScrollBar", "GridView", "ItemsRepeater", "BreadcrumbBar", "SelectorBar", "NavigationView", "ContentDialog", "Flyout", "Popup", "MenuBar", "MenuFlyout", "AppBarButton", "AppBarSeparator", "AppBarToggleButton", "CommandBar", "CommandBarFlyout"),
    [ValidateSet("Light", "Dark", "Default")]
    [string]$Theme = "Light",
    [string]$GalleryExe,
    [string]$OutputRoot = "artifacts\gallery-recordings",
    [int]$WindowLeft = 0,
    [int]$WindowTop = 0,
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
    [ValidateSet("Auto", "libx264", "h264_nvenc", "h264_qsv", "h264_amf")]
    [string]$VideoEncoder = "Auto",
    [switch]$BenchmarkEncoders,
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

$script:GalleryVisualSnapshotDirectory = ""
$script:GalleryLiveFrameDirectory = ""
$script:LastEditableTextMethod = ""

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
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT point);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint message, UIntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint message, UIntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYINPUT
    {
        public uint type;
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, INPUT[] inputs, int inputSize);

    [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
    private static extern uint SendKeyboardInput(uint inputCount, KEYINPUT[] inputs, int inputSize);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    private const int SW_RESTORE = 9;
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint WM_MOUSEMOVE = 0x0200;
    private const uint WM_LBUTTONDOWN = 0x0201;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_MOUSEHOVER = 0x02A1;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;
    private const uint WM_CHAR = 0x0102;
    private const uint MK_LBUTTON = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_SHIFT = 0x10;
    private const byte VK_ESCAPE = 0x1B;
    private const byte VK_RETURN = 0x0D;
    private const byte VK_DOWN = 0x28;
    private const byte VK_RIGHT = 0x27;
    private const byte VK_END = 0x23;
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
        HoldClick(x, y, 0);
    }

    public static void MoveCursor(int x, int y)
    {
        SetCursorPos(x, y);
        SendMouseInput(MOUSEEVENTF_MOVE);
        mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, UIntPtr.Zero);
        mouse_event(MOUSEEVENTF_MOVE, unchecked((uint)-1), 0, 0, UIntPtr.Zero);
    }

    public static void MoveCursorOverWindow(IntPtr hWnd, int x, int y)
    {
        MoveCursor(x, y);
        var point = new POINT { X = x, Y = y };
        if (ScreenToClient(hWnd, ref point))
        {
            int packedPoint = unchecked((int)(((point.Y & 0xffff) << 16) | (point.X & 0xffff)));
            SendMessage(hWnd, WM_MOUSEMOVE, UIntPtr.Zero, new IntPtr(packedPoint));
            PostMessage(hWnd, WM_MOUSEMOVE, UIntPtr.Zero, new IntPtr(packedPoint));
        }
    }

    public static void HoverCursorOverWindow(IntPtr hWnd, int x, int y)
    {
        MoveCursorOverWindow(hWnd, x, y);
        var point = new POINT { X = x, Y = y };
        if (ScreenToClient(hWnd, ref point))
        {
            int packedPoint = unchecked((int)(((point.Y & 0xffff) << 16) | (point.X & 0xffff)));
            SendMessage(hWnd, WM_MOUSEHOVER, UIntPtr.Zero, new IntPtr(packedPoint));
            PostMessage(hWnd, WM_MOUSEHOVER, UIntPtr.Zero, new IntPtr(packedPoint));
        }
    }

    public static void HoldClick(int x, int y, int holdMilliseconds)
    {
        SetCursorPos(x, y);
        SendMouseInput(MOUSEEVENTF_LEFTDOWN);
        System.Threading.Thread.Sleep(holdMilliseconds);
        SendMouseInput(MOUSEEVENTF_LEFTUP);
    }

    public static void HoldClickOverWindow(IntPtr hWnd, int x, int y, int holdMilliseconds)
    {
        MoveCursorOverWindow(hWnd, x, y);
        var point = new POINT { X = x, Y = y };
        if (ScreenToClient(hWnd, ref point))
        {
            int packedPoint = unchecked((int)(((point.Y & 0xffff) << 16) | (point.X & 0xffff)));
            SendMessage(hWnd, WM_LBUTTONDOWN, new UIntPtr(MK_LBUTTON), new IntPtr(packedPoint));
            System.Threading.Thread.Sleep(holdMilliseconds);
            SendMessage(hWnd, WM_LBUTTONUP, UIntPtr.Zero, new IntPtr(packedPoint));
        }
        else
        {
            HoldClick(x, y, holdMilliseconds);
        }
    }

    public static void Drag(int startX, int startY, int endX, int endY, int steps, int stepDelayMilliseconds)
    {
        if (steps < 1)
        {
            steps = 1;
        }

        SetCursorPos(startX, startY);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        int previousX = startX;
        int previousY = startY;
        for (int i = 1; i <= steps; i++)
        {
            double progress = (double)i / steps;
            int x = (int)Math.Round(startX + ((endX - startX) * progress));
            int y = (int)Math.Round(startY + ((endY - startY) * progress));
            SetCursorPos(x, y);
            mouse_event(
                MOUSEEVENTF_MOVE,
                unchecked((uint)(x - previousX)),
                unchecked((uint)(y - previousY)),
                0,
                UIntPtr.Zero);
            previousX = x;
            previousY = y;
            if (stepDelayMilliseconds > 0)
            {
                System.Threading.Thread.Sleep(stepDelayMilliseconds);
            }
        }

        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    }

    private static void SendMouseInput(uint flags)
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].mi.dwFlags = flags;
        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
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

    public static void Right()
    {
        KeyPress(VK_RIGHT);
    }

    public static void End()
    {
        KeyPress(VK_END);
    }

    public static void EndOverWindow(IntPtr hWnd)
    {
        PostMessage(hWnd, WM_KEYDOWN, new UIntPtr(VK_END), IntPtr.Zero);
        PostMessage(hWnd, WM_KEYUP, new UIntPtr(VK_END), IntPtr.Zero);
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

    public static void PressCtrlV()
    {
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        KeyPress(0x56);
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
            TypeUnicodeChar(ch);
            System.Threading.Thread.Sleep(15);
        }
    }

    public static void TypeVirtualKeyText(string text)
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

    public static void TypeWindowMessageText(IntPtr hWnd, string text)
    {
        if (text == null)
        {
            return;
        }

        foreach (char ch in text)
        {
            SendMessage(hWnd, WM_CHAR, new UIntPtr(ch), new IntPtr(1));
            System.Threading.Thread.Sleep(15);
        }
    }

    private static void TypeUnicodeChar(char ch)
    {
        var inputs = new KEYINPUT[2];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].ki.wScan = ch;
        inputs[0].ki.dwFlags = KEYEVENTF_UNICODE;
        inputs[1].type = INPUT_KEYBOARD;
        inputs[1].ki.wScan = ch;
        inputs[1].ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
        SendKeyboardInput(2, inputs, Marshal.SizeOf(typeof(KEYINPUT)));
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

function Wait-LiveRecordingWarmupFrames([int]$frameRate, [double]$warmupSeconds, [int]$timeoutSeconds) {
    if ([string]::IsNullOrWhiteSpace($script:GalleryLiveFrameDirectory)) {
        return $false
    }

    $requiredFrameCount = [Math]::Max(2, [int][Math]::Ceiling([Math]::Max(1, $frameRate) * $warmupSeconds))
    [void](Wait-Until -TimeoutSeconds $timeoutSeconds -Description "live recorder warm-up frames" -Probe {
            if (!(Test-Path -LiteralPath $script:GalleryLiveFrameDirectory)) {
                return $false
            }

            $frames = @(Get-ChildItem -LiteralPath $script:GalleryLiveFrameDirectory -Filter "frame-*.png" -File -ErrorAction SilentlyContinue |
                    Where-Object { $_.Length -gt 0 } |
                    Sort-Object Name)
            if ($frames.Count -lt $requiredFrameCount) {
                return $false
            }

            $latestFrame = $frames[$frames.Count - 1]
            try {
                $stats = Get-ImageStats $latestFrame.FullName
                return $null -ne $stats -and $stats.NonBlank
            }
            catch {
                return $false
            }
        })

    return $true
}

function Find-WindowByProcessId([int]$processId) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    $bestWindow = $null
    $bestScore = -1

    foreach ($window in $windows) {
        try {
            $handle = [IntPtr]$window.Current.NativeWindowHandle
            if ($handle -eq [IntPtr]::Zero) {
                continue
            }

            $rect = [GalleryRecordingNative]::GetRect($handle)
            $width = $rect.Right - $rect.Left
            $height = $rect.Bottom - $rect.Top
            if ($width -lt 400 -or $height -lt 300) {
                continue
            }

            $score = [int64]($width * $height)
            if ($window.Current.Name -eq "WPF Gallery") {
                $score += 1000000000
            }

            if ($window.Current.ClassName -eq "Window") {
                $score += 100000000
            }

            if (!$window.Current.IsOffscreen) {
                $score += 1000000
            }

            if ($score -gt $bestScore) {
                $bestWindow = $window
                $bestScore = $score
            }
        }
        catch {
        }
    }

    if ($null -eq $bestWindow) {
        try {
            $process = [System.Diagnostics.Process]::GetProcessById($processId)
            $process.Refresh()
            if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
                $candidate = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
                if ($null -ne $candidate -and $candidate.Current.ProcessId -eq $processId) {
                    $rect = [GalleryRecordingNative]::GetRect([IntPtr]$candidate.Current.NativeWindowHandle)
                    $width = $rect.Right - $rect.Left
                    $height = $rect.Bottom - $rect.Top
                    if ($width -ge 400 -and $height -ge 300) {
                        $bestWindow = $candidate
                    }
                }
            }
        }
        catch {
        }
    }

    return $bestWindow
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

function Find-DescendantByNameAndType($root, [string]$name, $controlType) {
    if ($null -eq $root -or [string]::IsNullOrWhiteSpace($name)) {
        return $null
    }

    $nameCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $name)
    $typeCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        $controlType)
    $condition = New-Object System.Windows.Automation.AndCondition($nameCondition, $typeCondition)
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

function Find-ElementByControlTypeInProcess([int]$processId, $controlType) {
    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    foreach ($window in $windows) {
        $match = Find-DescendantByControlType $window $controlType
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
                        $controlType -ne [System.Windows.Automation.ControlType]::DataItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::MenuItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::ListItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::RadioButton -and
                        $controlType -ne [System.Windows.Automation.ControlType]::TabItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::TreeItem) {
                        continue
                    }

                    if (Test-AutomationElementUsable $element) {
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

        if (!$element.Current.IsEnabled) {
            return $false
        }

        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return $false
        }

        $point = New-Object System.Windows.Point
        return $element.TryGetClickablePoint([ref]$point)
    }
    catch {
        return $false
    }
}

function Get-ElementBoundingRectangle($element) {
    if ($null -eq $element) {
        return $null
    }

    try {
        $rect = $element.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return $null
        }

        return $rect
    }
    catch {
        return $null
    }
}

function Format-BoundingRectangle($rect) {
    if ($null -eq $rect) {
        return ""
    }

    return "{0},{1},{2},{3}" -f `
        [Math]::Round($rect.X, 1), `
        [Math]::Round($rect.Y, 1), `
        [Math]::Round($rect.Width, 1), `
        [Math]::Round($rect.Height, 1)
}

function Get-ElementControlTypeName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        return $element.Current.ControlType.ProgrammaticName
    }
    catch {
        return ""
    }
}

function Get-ElementClassName($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        return $element.Current.ClassName
    }
    catch {
        return ""
    }
}

function Get-ToolTipFallbackBoundsFromTriggerBounds([string]$triggerBounds) {
    $rect = ConvertFrom-BoundingRectangleString $triggerBounds
    if ($null -eq $rect -or $rect.Width -le 0 -or $rect.Height -le 0) {
        return ""
    }

    return "{0},{1},{2},{3}" -f `
        [Math]::Round($rect.X + 10, 1), `
        [Math]::Round($rect.Y + $rect.Height + 9, 1), `
        90, `
        25
}

function ConvertFrom-BoundingRectangleString([string]$bounds) {
    if ([string]::IsNullOrWhiteSpace($bounds)) {
        return $null
    }

    $parts = $bounds -split ","
    if ($parts.Count -ne 4) {
        return $null
    }

    $values = New-Object double[] 4
    for ($i = 0; $i -lt 4; $i++) {
        try {
            $values[$i] = [double]::Parse($parts[$i].Trim(), [Globalization.CultureInfo]::InvariantCulture)
        }
        catch {
            try {
                $values[$i] = [double]::Parse($parts[$i].Trim(), [Globalization.CultureInfo]::CurrentCulture)
            }
            catch {
                return $null
            }
        }
    }

    if ($values[2] -le 0 -or $values[3] -le 0) {
        return $null
    }

    return [pscustomobject]@{
        X = $values[0]
        Y = $values[1]
        Width = $values[2]
        Height = $values[3]
    }
}

function Test-BoundingRectangleStringsNearlyEqual([string]$before, [string]$after, [double]$tolerance) {
    $beforeRect = ConvertFrom-BoundingRectangleString $before
    $afterRect = ConvertFrom-BoundingRectangleString $after
    if ($null -eq $beforeRect -or $null -eq $afterRect) {
        return $false
    }

    return [Math]::Abs($beforeRect.X - $afterRect.X) -le $tolerance -and
        [Math]::Abs($beforeRect.Y - $afterRect.Y) -le $tolerance -and
        [Math]::Abs($beforeRect.Width - $afterRect.Width) -le $tolerance -and
        [Math]::Abs($beforeRect.Height - $afterRect.Height) -le $tolerance
}

function Get-LayoutStabilityTargetAutomationIds([string]$control) {
    switch ($control) {
        "ThemeShadow" {
            return @(
                "GallerySample_ThemeShadow_Root",
                "GallerySample_ThemeShadow_Example3Grid",
                "GallerySample_ThemeShadow_ShadowCastGrid",
                "GallerySample_ThemeShadow_ShadowChrome",
                "GallerySample_ThemeShadow_ShadowRect",
                "GallerySample_ThemeShadow_TranslationSlider")
        }
        default { return @() }
    }
}

function Get-RenderedArtifactBoundsMap($window, [string]$artifactDir, [string[]]$automationIds) {
    $boundsById = [ordered]@{}
    foreach ($automationId in $automationIds) {
        $bounds = Get-RenderedArtifactBounds $artifactDir $automationId
        if ([string]::IsNullOrWhiteSpace($bounds)) {
            $bounds = Format-BoundingRectangle (Get-ElementBoundingRectangle (Find-DescendantByAutomationId $window $automationId))
        }

        $boundsById[$automationId] = $bounds
    }

    return $boundsById
}

function Copy-RenderedArtifactSnapshot([string]$artifactDir, [string]$snapshotName, [string[]]$automationIds) {
    if ([string]::IsNullOrWhiteSpace($artifactDir) -or [string]::IsNullOrWhiteSpace($snapshotName)) {
        return ""
    }

    $snapshotDir = Join-Path $artifactDir $snapshotName
    New-Item -ItemType Directory -Force -Path $snapshotDir | Out-Null
    foreach ($automationId in $automationIds) {
        foreach ($extension in @(".png", ".bounds.txt")) {
            $source = Join-Path $artifactDir ("{0}{1}" -f $automationId, $extension)
            if (Test-Path $source) {
                Copy-Item -LiteralPath $source -Destination (Join-Path $snapshotDir ([IO.Path]::GetFileName($source))) -Force
            }
        }
    }

    return $snapshotDir
}

function Get-RenderedArtifactSnapshotPath([string]$snapshotDir, [string]$automationId, [string]$extension) {
    if ([string]::IsNullOrWhiteSpace($snapshotDir) -or [string]::IsNullOrWhiteSpace($automationId)) {
        return ""
    }

    return Join-Path $snapshotDir ("{0}{1}" -f $automationId, $extension)
}

function ConvertTo-RelativeRectangle($outerBounds, $innerBounds) {
    if ($null -eq $outerBounds -or $null -eq $innerBounds) {
        return $null
    }

    return [pscustomobject]@{
        X = [int][Math]::Round($innerBounds.X - $outerBounds.X)
        Y = [int][Math]::Round($innerBounds.Y - $outerBounds.Y)
        Width = [int][Math]::Round($innerBounds.Width)
        Height = [int][Math]::Round($innerBounds.Height)
    }
}

function Get-ThemeShadowArtifactEvidence([string]$beforeSnapshotDir, [string]$afterSnapshotDir) {
    $rootId = "GallerySample_ThemeShadow_Root"
    $cardId = "GallerySample_ThemeShadow_ShadowRect"
    $beforeRootPath = Get-RenderedArtifactSnapshotPath $beforeSnapshotDir $rootId ".png"
    $afterRootPath = Get-RenderedArtifactSnapshotPath $afterSnapshotDir $rootId ".png"
    $beforeRootBoundsPath = Get-RenderedArtifactSnapshotPath $beforeSnapshotDir $rootId ".bounds.txt"
    $afterRootBoundsPath = Get-RenderedArtifactSnapshotPath $afterSnapshotDir $rootId ".bounds.txt"
    $beforeCardBoundsPath = Get-RenderedArtifactSnapshotPath $beforeSnapshotDir $cardId ".bounds.txt"
    $afterCardBoundsPath = Get-RenderedArtifactSnapshotPath $afterSnapshotDir $cardId ".bounds.txt"

    if (!(Test-Path $beforeRootPath) -or !(Test-Path $afterRootPath) -or
        !(Test-Path $beforeRootBoundsPath) -or !(Test-Path $afterRootBoundsPath) -or
        !(Test-Path $beforeCardBoundsPath) -or !(Test-Path $afterCardBoundsPath)) {
        return [ordered]@{
            Generated = $false
            Reason = "Missing ThemeShadow before/after rendered artifact snapshot files."
        }
    }

    $beforeRootBounds = ConvertFrom-BoundingRectangleString ((Get-Content -LiteralPath $beforeRootBoundsPath -Raw).Trim())
    $afterRootBounds = ConvertFrom-BoundingRectangleString ((Get-Content -LiteralPath $afterRootBoundsPath -Raw).Trim())
    $beforeCardBounds = ConvertFrom-BoundingRectangleString ((Get-Content -LiteralPath $beforeCardBoundsPath -Raw).Trim())
    $afterCardBounds = ConvertFrom-BoundingRectangleString ((Get-Content -LiteralPath $afterCardBoundsPath -Raw).Trim())
    $beforeRegion = ConvertTo-RelativeRectangle $beforeRootBounds $beforeCardBounds
    $afterRegion = ConvertTo-RelativeRectangle $afterRootBounds $afterCardBounds
    if ($null -eq $beforeRegion -or $null -eq $afterRegion) {
        return [ordered]@{
            Generated = $false
            Reason = "ThemeShadow before/after rendered artifact bounds could not be parsed."
        }
    }

    $beforeBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path $beforeRootPath).Path)
    $afterBitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path $afterRootPath).Path)
    try {
        $beforeCardEdgeBounds = Get-ThemeShadowCardEdgeBounds $beforeBitmap $beforeRegion
        $afterCardEdgeBounds = Get-ThemeShadowCardEdgeBounds $afterBitmap $afterRegion
        $edgeShift = Get-FrameRectangleMaxDelta $beforeCardEdgeBounds $afterCardEdgeBounds
        $beforeShadowEnvelopeBounds = Get-ThemeShadowShadowEnvelopeBounds $beforeBitmap $beforeRegion 1
        $afterShadowEnvelopeBounds = Get-ThemeShadowShadowEnvelopeBounds $afterBitmap $afterRegion 1
        $shadowEnvelopeDelta = Get-FrameRectangleMaxDelta $beforeShadowEnvelopeBounds $afterShadowEnvelopeBounds
    }
    finally {
        $beforeBitmap.Dispose()
        $afterBitmap.Dispose()
    }

    $rootMeanDelta = Compare-ImageMeanDelta $beforeRootPath $afterRootPath
    return [ordered]@{
        Generated = $true
        BeforeRoot = $beforeRootPath
        AfterRoot = $afterRootPath
        RootMeanDelta = $rootMeanDelta
        VisualChanged = $null -ne $rootMeanDelta -and $rootMeanDelta -gt 0.1
        BeforeCardRegion = Format-FrameRectangle $beforeRegion
        AfterCardRegion = Format-FrameRectangle $afterRegion
        BeforeCardEdgeBounds = Format-FrameRectangle $beforeCardEdgeBounds
        AfterCardEdgeBounds = Format-FrameRectangle $afterCardEdgeBounds
        CardEdgeShift = $edgeShift
        CardEdgesStable = $null -ne $edgeShift -and $edgeShift -le 1.0
        CardEdgeShiftThreshold = 1.0
        BeforeShadowEnvelopeBounds = Format-FrameRectangle $beforeShadowEnvelopeBounds
        AfterShadowEnvelopeBounds = Format-FrameRectangle $afterShadowEnvelopeBounds
        ShadowEnvelopeDelta = $shadowEnvelopeDelta
        ShadowEnvelopeChanged = $null -ne $shadowEnvelopeDelta -and [double]$shadowEnvelopeDelta -gt 1.0
    }
}

function Get-BoundingRectangleMapValue($boundsById, [string]$automationId) {
    if ($null -eq $boundsById -or [string]::IsNullOrWhiteSpace($automationId)) {
        return ""
    }

    if ($boundsById.Contains($automationId)) {
        return [string]$boundsById[$automationId]
    }

    return ""
}

function Format-BoundingRectangleMap($boundsById) {
    if ($null -eq $boundsById -or $boundsById.Keys.Count -eq 0) {
        return ""
    }

    $parts = New-Object System.Collections.Generic.List[string]
    foreach ($automationId in $boundsById.Keys) {
        $parts.Add(("{0}={1}" -f $automationId, $boundsById[$automationId]))
    }

    return [string]::Join("; ", $parts)
}

function Test-BoundingRectangleMapsNearlyEqual($beforeById, $afterById, [double]$tolerance) {
    if ($null -eq $beforeById -or $null -eq $afterById -or $beforeById.Keys.Count -eq 0) {
        return $false
    }

    foreach ($automationId in $beforeById.Keys) {
        if (!$afterById.Contains($automationId)) {
            return $false
        }

        if (!(Test-BoundingRectangleStringsNearlyEqual $beforeById[$automationId] $afterById[$automationId] $tolerance)) {
            return $false
        }
    }

    return $true
}

function Get-BoundingRectangleGap($first, $second) {
    if ($null -eq $first -or $null -eq $second) {
        return [double]::PositiveInfinity
    }

    $horizontalGap = if (($first.X + $first.Width) -lt $second.X) {
        $second.X - ($first.X + $first.Width)
    }
    elseif (($second.X + $second.Width) -lt $first.X) {
        $first.X - ($second.X + $second.Width)
    }
    else {
        0.0
    }

    $verticalGap = if (($first.Y + $first.Height) -lt $second.Y) {
        $second.Y - ($first.Y + $first.Height)
    }
    elseif (($second.Y + $second.Height) -lt $first.Y) {
        $first.Y - ($second.Y + $second.Height)
    }
    else {
        0.0
    }

    return [Math]::Max([double]$horizontalGap, [double]$verticalGap)
}

function Test-OpenInteractionElementAnchored($trigger, $openElement) {
    $triggerRect = Get-ElementBoundingRectangle $trigger
    $openRect = Get-ElementBoundingRectangle $openElement
    if ($null -eq $triggerRect -or $null -eq $openRect) {
        return $false
    }

    return (Get-BoundingRectangleGap $triggerRect $openRect) -le 320.0
}

function Test-BoundingRectangleStringAnchored($trigger, [string]$openBounds) {
    $triggerRect = Get-ElementBoundingRectangle $trigger
    $openRect = ConvertFrom-BoundingRectangleString $openBounds
    if ($null -eq $triggerRect -or $null -eq $openRect) {
        return $false
    }

    return (Get-BoundingRectangleGap $triggerRect $openRect) -le 320.0
}

function Test-ControlAllowsDetachedOpenRepeatElement([string]$control) {
    return $control -eq "MessageBox"
}

function Test-ControlUsesFastOpenRepeatPopupBounds([string]$control) {
    return $control -eq "SplitButton" -or
        $control -eq "ToggleSplitButton"
}

function Get-FastOpenRepeatPopupBounds($trigger, [string]$control) {
    if (!(Test-ControlUsesFastOpenRepeatPopupBounds $control)) {
        return ""
    }

    $triggerRect = Get-ElementBoundingRectangle $trigger
    if ($null -eq $triggerRect) {
        return ""
    }

    switch ($control) {
        "SplitButton" {
            $x = $triggerRect.X + 23
            $y = $triggerRect.Y + $triggerRect.Height + 26
            $width = 34
            $height = 34
            break
        }
        "ToggleSplitButton" {
            $x = $triggerRect.X + 8
            $y = $triggerRect.Y + $triggerRect.Height + 7
            $width = [Math]::Max(150.0, $triggerRect.Width + 70.0)
            $height = 34
            break
        }
        default {
            $x = $triggerRect.X
            $y = $triggerRect.Y + $triggerRect.Height + 4
            $width = [Math]::Max(120.0, $triggerRect.Width)
            $height = 34
            break
        }
    }

    return "{0},{1},{2},{3}" -f `
        [Math]::Round($x, 1), `
        [Math]::Round($y, 1), `
        [Math]::Round($width, 1), `
        [Math]::Round($height, 1)
}

function Find-AnchoredInteractiveElementByNameInProcess([int]$processId, [string[]]$names, $anchor) {
    $anchorRect = Get-ElementBoundingRectangle $anchor
    if ($null -eq $anchorRect) {
        return $null
    }

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $processId)
    $windows = (Get-RootElement).FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
    $bestElement = $null
    $bestGap = [double]::PositiveInfinity

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
                        $controlType -ne [System.Windows.Automation.ControlType]::DataItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::MenuItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::ListItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::RadioButton -and
                        $controlType -ne [System.Windows.Automation.ControlType]::TabItem -and
                        $controlType -ne [System.Windows.Automation.ControlType]::TreeItem) {
                        continue
                    }

                    if (!(Test-AutomationElementUsable $element)) {
                        continue
                    }

                    $rect = $element.Current.BoundingRectangle
                    $gap = Get-BoundingRectangleGap $anchorRect $rect
                    if ($gap -le 320.0 -and $gap -lt $bestGap) {
                        $bestElement = $element
                        $bestGap = $gap
                    }
                }
                catch {
                }
            }
        }
    }

    return $bestElement
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

function Get-ControlRoute([string]$control) {
    if ($control -eq "ShellNavigation") {
        return "home"
    }

    return "item/$control"
}

function Get-RequiredSampleAutomationId([string]$control) {
    switch ($control) {
        "ShellNavigation" { return "GalleryNavigationView" }
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
        "IconElement" { return "GallerySample_IconElement_ExampleButton1" }
        "ThemeShadow" { return "GallerySample_ThemeShadow_TranslationSlider" }
        "TitleBar" { return "GallerySample_TitleBar_SearchBox" }
        "InfoBadge" { return "GallerySample_InfoBadge_NavigationView" }
        "InfoBar" { return "GallerySample_InfoBar_InfoBar" }
        "ProgressRing" { return "GallerySample_ProgressRing_ProgressRing" }
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_ScrollViewer" }
        "GridView" { return "GallerySample_GridView_BasicGridView" }
        "ItemsRepeater" { return "GallerySample_ItemsRepeater_ItemsRepeater" }
        "BreadcrumbBar" { return "GallerySample_BreadcrumbBar_TemplateBreadcrumbBar" }
        "SelectorBar" { return "GallerySample_SelectorBar_SelectorBarItemShared" }
        "NavigationView" { return "GallerySample_NavigationView_NavigationView" }
        "ContentDialog" { return "GallerySample_ContentDialog_ShowButton" }
        "Flyout" { return "GallerySample_Flyout_Button" }
        "Popup" { return "GallerySample_Popup_Button" }
        "MenuBar" { return "GallerySample_MenuBar_MenuBar" }
        "MenuFlyout" { return "GallerySample_MenuFlyout_AppBarButton" }
        "AppBarButton" { return "GallerySample_AppBarButton_AppBarButton" }
        "AppBarSeparator" { return "GallerySample_AppBarSeparator_AttachCameraButton" }
        "AppBarToggleButton" { return "GallerySample_AppBarToggleButton_AppBarToggleButton" }
        "CommandBar" { return "GallerySample_CommandBar_AddButton" }
        "CommandBarFlyout" { return "GallerySample_CommandBarFlyout_ShowButton" }
        default { return "GallerySample_${control}_Root" }
    }
}

function Test-ControlSupportsRenderedPageArtifactAnchor([string]$control) {
    switch ($control) {
        "Border" { return $true }
        "Calendar" { return $true }
        "Clipboard" { return $true }
        "Color" { return $true }
        "DataGrid" { return $true }
        "DatePicker" { return $true }
        "Expander" { return $true }
        "FileAndFolderDialogs" { return $true }
        "Frame" { return $true }
        "Geometry" { return $true }
        "Grid" { return $true }
        "GridSplitter" { return $true }
        "GroupBox" { return $true }
        "Hyperlink" { return $true }
        "Iconography" { return $true }
        "Label" { return $true }
        "ListBox" { return $true }
        "ListView" { return $true }
        "Menu" { return $true }
        "MessageBox" { return $true }
        "NavigationWindow" { return $true }
        "PasswordBox" { return $true }
        "ProgressBar" { return $true }
        "ResizeGrip" { return $true }
        "RichTextEdit" { return $true }
        "Spacing" { return $true }
        "StackPanel" { return $true }
        "TabControl" { return $true }
        "TextBlock" { return $true }
        "TextBox" { return $true }
        "ToolTip" { return $true }
        "TreeView" { return $true }
        "Typography" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsOpenInteraction([string]$control) {
    switch ($control) {
        "TeachingTip" { return $true }
        "ComboBox" { return $true }
        "DatePicker" { return $true }
        "Menu" { return $true }
        "MessageBox" { return $true }
        "ContentDialog" { return $true }
        "Flyout" { return $true }
        "Popup" { return $true }
        "MenuBar" { return $true }
        "MenuFlyout" { return $true }
        "ToolTip" { return $true }
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
        "TeachingTip" { return @("This is the title", "And this is the subtitle", "Close") }
        "ComboBox" { return @("Blue", "Green", "Red", "Yellow") }
        "DatePicker" { return @("Calendar") }
        "Menu" { return @("New", "New window", "Open", "Save", "Save As", "Exit") }
        "MessageBox" { return @("This is a simple message box!") }
        "ContentDialog" { return @("Save your work?", "Upload your content to the cloud.", "Save", "Don't Save", "Cancel") }
        "Flyout" { return @("All items will be removed. Do you want to continue?", "Yes, empty my cart") }
        "Popup" { return @("Simple Popup", "Close") }
        "MenuBar" { return @("New", "Open", "Save", "Exit") }
        "MenuFlyout" { return @("By rating", "By match", "By distance") }
        "ToolTip" { return @("Simple ToolTip") }
        "DropDownButton" { return @("Send", "Reply", "Reply All") }
        "SplitButton" { return @("Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray") }
        "ToggleSplitButton" { return @("Bulleted list", "Roman numerals list") }
        "CommandBar" { return @("Settings") }
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
        "Calendar" { return $true }
        "DataGrid" { return $true }
        "PersonPicture" { return $true }
        "RadioButton" { return $true }
        "GridView" { return $true }
        "ListBox" { return $true }
        "ListView" { return $true }
        "SelectorBar" { return $true }
        "NavigationView" { return $true }
        "TabControl" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsExpansionInteraction([string]$control) {
    switch ($control) {
        "Expander" { return $true }
        "TreeView" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsOptionInteraction([string]$control) {
    switch ($control) {
        "Button" { return $true }
        "ColorPicker" { return $true }
        "IconElement" { return $true }
        "SplitView" { return $true }
        "TitleBar" { return $true }
        "InfoBadge" { return $true }
        "InfoBar" { return $true }
        "ProgressRing" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsScrollInteraction([string]$control) {
    switch ($control) {
        "AnnotatedScrollBar" { return $true }
        "ItemsRepeater" { return $true }
        default { return $false }
    }
}

function Test-ControlRequiresAnimatedVisualProof([string]$control) {
    return $control -eq "ProgressRing" -or $control -eq "CommandBarFlyout"
}

function Test-ControlRequiresDenseTransitionReview([string]$control, [string]$interactionKind) {
    if ($interactionKind -eq "ShellNavigation") {
        return $true
    }

    if ($interactionKind -eq "PreparedOpen") {
        return $control -eq "ToolTip"
    }

    if ($interactionKind -ne "OpenRepeat") {
        return $false
    }

    switch ($control) {
        "TeachingTip" { return $true }
        "ComboBox" { return $true }
        "DatePicker" { return $true }
        "Menu" { return $true }
        "MessageBox" { return $true }
        "ContentDialog" { return $true }
        "Flyout" { return $true }
        "Popup" { return $true }
        "MenuBar" { return $true }
        "MenuFlyout" { return $true }
        "ToolTip" { return $true }
        "DropDownButton" { return $true }
        "SplitButton" { return $true }
        "ToggleSplitButton" { return $true }
        "CommandBar" { return $true }
        "CommandBarFlyout" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsTextInteraction([string]$control) {
    switch ($control) {
        "AutoSuggestBox" { return $true }
        "RichTextEdit" { return $true }
        "TextBox" { return $true }
        "PasswordBox" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsValueInteraction([string]$control) {
    switch ($control) {
        "RatingControl" { return $true }
        "Slider" { return $true }
        "ThemeShadow" { return $true }
        "NumberBox" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsOutputInteraction([string]$control) {
    switch ($control) {
        "RepeatButton" { return $true }
        "AppBarButton" { return $true }
        default { return $false }
    }
}

function Test-ControlSupportsRouteNavigationInteraction([string]$control) {
    switch ($control) {
        "HyperlinkButton" { return $true }
        default { return $false }
    }
}

function Get-RouteNavigationTriggerAutomationId([string]$control) {
    switch ($control) {
        "HyperlinkButton" { return "GallerySample_HyperlinkButton_ClickHyperlinkButton" }
        default { return "" }
    }
}

function Get-RouteNavigationExpectedRoute([string]$control) {
    switch ($control) {
        "HyperlinkButton" { return "item/ToggleButton" }
        default { return "" }
    }
}

function Get-RouteNavigationExpectedSampleAutomationId([string]$control) {
    switch ($control) {
        "HyperlinkButton" { return "GallerySample_ToggleButton_ToggleButton" }
        default { return "" }
    }
}

function Get-SelectionInteractionTriggerName([string]$control) {
    switch ($control) {
        "ListBox" { return "Green" }
        "PersonPicture" { return "Display Name" }
        "RadioButton" { return "Default Radio Option 2" }
        "GridView" { return "Item 1" }
        "SelectorBar" { return "Shared" }
        "NavigationView" { return "Menu Item2" }
        "TabControl" { return "Hello Tab" }
        default { return "" }
    }
}

function Get-SelectionInteractionTriggerAutomationId([string]$control) {
    switch ($control) {
        "SelectorBar" { return "GallerySample_SelectorBar_SelectorBarItemShared" }
        default { return "" }
    }
}

function Get-SelectionInteractionExpectedOutputName([string]$control) {
    switch ($control) {
        "GridView" { return "You clicked Item 1." }
        "NavigationView" { return "Sample Page 2" }
        "TabControl" { return "World" }
        default { return "" }
    }
}

function Get-SelectionInteractionOutputAutomationId([string]$control) {
    switch ($control) {
        "GridView" { return "GallerySample_GridView_ClickOutput0" }
        default { return "" }
    }
}

function Get-SelectionInteractionContainerName([string]$control) {
    switch ($control) {
        "Calendar" { return "Default" }
        "DataGrid" { return "Sample Data Grid" }
        "ListBox" { return "Color ListBox" }
        "ListView" { return "Basic ListView" }
        default { return "" }
    }
}

function Get-ExpansionInteractionTriggerName([string]$control) {
    switch ($control) {
        "Expander" { return "This text is in the header" }
        "TreeView" { return "Personal Documents" }
        default { return "" }
    }
}

function Get-ExpansionInteractionExpectedChildName([string]$control) {
    switch ($control) {
        "Expander" { return "This is in the content" }
        "TreeView" { return "Contractor contact info" }
        default { return "" }
    }
}

function Get-ControlInteractionKind([string]$control) {
    if ($control -eq "ShellNavigation") { return "ShellNavigation" }
    if ($control -eq "BreadcrumbBar") { return "Breadcrumb" }
    if (Test-ControlSupportsOpenInteraction $control) { return "OpenRepeat" }
    if (Test-ControlSupportsStateInteraction $control) { return "State" }
    if (Test-ControlSupportsExpansionInteraction $control) { return "Expansion" }
    if (Test-ControlSupportsValueInteraction $control) { return "Value" }
    if (Test-ControlSupportsSelectionInteraction $control) { return "Selection" }
    if (Test-ControlSupportsOptionInteraction $control) { return "Option" }
    if (Test-ControlSupportsTextInteraction $control) { return "Text" }
    if (Test-ControlSupportsOutputInteraction $control) { return "Output" }
    if (Test-ControlSupportsRouteNavigationInteraction $control) { return "RouteNavigation" }
    if (Test-ControlSupportsScrollInteraction $control) { return "Scroll" }
    return "Static"
}

function Get-ControlRecordingDurationSeconds([string]$control, [string]$interactionKind) {
    if ($interactionKind -eq "ShellNavigation") {
        return [Math]::Max($DurationSeconds, 18)
    }

    if ($interactionKind -eq "Text" -and $control -eq "AutoSuggestBox") {
        return [Math]::Max($DurationSeconds, 18)
    }

    if ($interactionKind -eq "OpenRepeat") {
        if (Test-ControlUsesFastOpenRepeatPopupBounds $control) {
            return [Math]::Max($DurationSeconds, 18)
        }

        if ($control -eq "ToolTip") {
            return [Math]::Max($DurationSeconds, 18)
        }

        if ($control -eq "MessageBox") {
            return [Math]::Max($DurationSeconds, 18)
        }

        if ($control -eq "MenuBar") {
            return [Math]::Max($DurationSeconds, 18)
        }

        if ($control -eq "CommandBar" -or $control -eq "CommandBarFlyout") {
            return [Math]::Max($DurationSeconds, 24)
        }

        if ($control -eq "ContentDialog" -or $control -eq "Flyout" -or $control -eq "Popup" -or $control -eq "MenuFlyout") {
            return [Math]::Max($DurationSeconds, 24)
        }

        return [Math]::Max($DurationSeconds, 24)
    }

    return $DurationSeconds
}

function Test-ControlRequiresDiagnosticPreparation([string]$control) {
    switch ($control) {
        default { return $false }
    }
}

function Find-ShellNavigationItem($navigationView, [string]$name) {
    $item = Find-DescendantByNameAndType $navigationView $name ([System.Windows.Automation.ControlType]::ListItem)
    if ($null -ne $item) {
        return $item
    }

    return Find-DescendantByName $navigationView $name
}

function Test-ElementVisible($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $rect = $element.Current.BoundingRectangle
        return !$element.Current.IsOffscreen -and $rect.Width -gt 0 -and $rect.Height -gt 0
    }
    catch {
        return $false
    }
}

function Get-ShellNavigationDisclosureClickPoint($item) {
    $rect = $item.Current.BoundingRectangle
    return [ordered]@{
        X = [int][Math]::Round($rect.X + ($rect.Width * 0.5))
        Y = [int][Math]::Round($rect.Y + 20.0)
        Source = "GroupRowBody"
    }
}

function Invoke-ShellNavigationExpandCollapsePattern($item, [string]$targetState) {
    try {
        $patternObject = $null
        if (!$item.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$patternObject)) {
            return $false
        }

        $pattern = [System.Windows.Automation.ExpandCollapsePattern]$patternObject
        if ($targetState -eq "Expanded") {
            $pattern.Expand()
            return $true
        }

        if ($targetState -eq "Collapsed") {
            $pattern.Collapse()
            return $true
        }
    }
    catch {
    }

    return $false
}

function Invoke-ShellNavigationDisclosure($window, $navigationView, [string]$name, [string]$targetState) {
    $item = Find-ShellNavigationItem $navigationView $name
    if ($null -eq $item) {
        return [ordered]@{ Clicked = $false; Name = $name; Failure = "Navigation item was not found." }
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $rect = $item.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            return [ordered]@{ Clicked = $false; Name = $name; Failure = "Navigation item had empty bounds." }
        }

        $point = Get-ShellNavigationDisclosureClickPoint $item
        [GalleryRecordingNative]::HoldClick($point.X, $point.Y, 120)
        Start-Sleep -Milliseconds 250
        $stateAfterClick = Get-ExpandCollapseStateName $item
        $usedAutomationFallback = $false
        if ($targetState -and $stateAfterClick -ne $targetState) {
            $usedAutomationFallback = Invoke-ShellNavigationExpandCollapsePattern $item $targetState
        }

        Start-Sleep -Milliseconds 650
        return [ordered]@{
            Clicked = $true
            Name = $name
            X = $point.X
            Y = $point.Y
            Source = $point.Source
            Bounds = "{0},{1},{2},{3}" -f [Math]::Round($rect.X, 1), [Math]::Round($rect.Y, 1), [Math]::Round($rect.Width, 1), [Math]::Round($rect.Height, 1)
            TargetState = $targetState
            StateAfterClick = $stateAfterClick
            UsedAutomationFallback = $usedAutomationFallback
            StateAfterAction = Get-ExpandCollapseStateName $item
        }
    }
    catch {
        return [ordered]@{ Clicked = $false; Name = $name; Failure = $_.Exception.Message }
    }
}

function Get-ShellNavigationSnapshot(
    $navigationView,
    [string]$name,
    [string]$expectedState,
    [string[]]$childNames,
    [string[]]$hiddenChildNames,
    [string]$followingName,
    [double]$maximumFollowingGap) {
    $failures = New-Object System.Collections.Generic.List[string]
    $item = Find-ShellNavigationItem $navigationView $name
    $height = 0.0
    $bounds = ""
    $followingGap = $null
    $visibleChildren = New-Object System.Collections.Generic.List[string]
    $hiddenChildren = New-Object System.Collections.Generic.List[string]
    $unexpectedVisibleChildren = New-Object System.Collections.Generic.List[string]
    $state = ""

    if ($null -eq $item) {
        $failures.Add("Navigation item '$name' was not found.")
    }
    else {
        $state = Get-ExpandCollapseStateName $item
        if ($state -ne $expectedState) {
            $failures.Add("Navigation item '$name' expected $expectedState, observed '$state'.")
        }

        try {
            $rect = $item.Current.BoundingRectangle
            $height = [Math]::Round($rect.Height, 1)
            $bounds = Format-BoundingRectangle $rect
            if ($followingName) {
                $following = Find-ShellNavigationItem $navigationView $followingName
                if ($null -ne $following) {
                    $followingRect = $following.Current.BoundingRectangle
                    $followingGap = [Math]::Round($followingRect.Y - ($rect.Y + $rect.Height), 1)
                    if ($null -ne $maximumFollowingGap -and $followingGap -gt $maximumFollowingGap) {
                        $failures.Add("Navigation item '$name' had gap $followingGap before '$followingName'.")
                    }
                }
                else {
                    $failures.Add("Following navigation item '$followingName' was not found.")
                }
            }
        }
        catch {
            $failures.Add("Navigation item '$name' bounds were unavailable.")
        }

        foreach ($childName in $childNames) {
            $child = Find-DescendantByNameAndType $item $childName ([System.Windows.Automation.ControlType]::ListItem)
            if (Test-ElementVisible $child) {
                $visibleChildren.Add($childName)
            }
            else {
                $failures.Add("Expanded navigation item '$name' did not expose visible child '$childName'.")
            }
        }

        foreach ($childName in $hiddenChildNames) {
            $child = Find-DescendantByNameAndType $item $childName ([System.Windows.Automation.ControlType]::ListItem)
            if (Test-ElementVisible $child) {
                $unexpectedVisibleChildren.Add($childName)
                $failures.Add("Collapsed navigation item '$name' still exposed visible child '$childName'.")
            }
            else {
                $hiddenChildren.Add($childName)
            }
        }
    }

    return [ordered]@{
        Name = $name
        ExpectedState = $expectedState
        State = $state
        Height = $height
        Bounds = $bounds
        FollowingName = $followingName
        FollowingGap = $followingGap
        VisibleChildren = $visibleChildren.ToArray()
        HiddenChildren = $hiddenChildren.ToArray()
        UnexpectedVisibleChildren = $unexpectedVisibleChildren.ToArray()
        Success = $failures.Count -eq 0
        Failures = $failures.ToArray()
    }
}

function Invoke-ShellNavigationInteraction($window, $navigationView) {
    if ($null -eq $navigationView) {
        return [ordered]@{ Invoked = $false; ShellNavigationChanged = $false; Failures = @("GalleryNavigationView was not found.") }
    }

    $steps = New-Object System.Collections.Generic.List[object]
    $failures = New-Object System.Collections.Generic.List[string]
    $homeItem = Find-ShellNavigationItem $navigationView "Home"
    if (!(Test-ElementVisible $homeItem)) {
        $failures.Add("Home navigation item was not visible before shell interaction.")
    }

    $designExpandedClick = Invoke-ShellNavigationDisclosure $window $navigationView "Design Guidance" "Expanded"
    $designExpanded = Get-ShellNavigationSnapshot `
        $navigationView `
        "Design Guidance" `
        "Expanded" `
        @("Colors", "Typography", "Spacing", "Geometry", "Icons") `
        @() `
        "Samples" `
        48.0
    $steps.Add($designExpanded)

    $samplesExpandedClick = Invoke-ShellNavigationDisclosure $window $navigationView "Samples" "Expanded"
    $samplesExpanded = Get-ShellNavigationSnapshot `
        $navigationView `
        "Samples" `
        "Expanded" `
        @("User Dashboard") `
        @() `
        "All Controls" `
        48.0
    $steps.Add($samplesExpanded)

    $designCollapsedClick = Invoke-ShellNavigationDisclosure $window $navigationView "Design Guidance" "Collapsed"
    $designCollapsed = Get-ShellNavigationSnapshot `
        $navigationView `
        "Design Guidance" `
        "Collapsed" `
        @() `
        @("Colors", "Typography", "Spacing", "Geometry", "Icons") `
        "Samples" `
        48.0
    $steps.Add($designCollapsed)

    $samplesCollapsedClick = Invoke-ShellNavigationDisclosure $window $navigationView "Samples" "Collapsed"
    $samplesCollapsed = Get-ShellNavigationSnapshot `
        $navigationView `
        "Samples" `
        "Collapsed" `
        @() `
        @("User Dashboard") `
        "All Controls" `
        48.0
    $steps.Add($samplesCollapsed)

    foreach ($click in @($designExpandedClick, $samplesExpandedClick, $designCollapsedClick, $samplesCollapsedClick)) {
        if (!$click.Clicked) {
            $failures.Add(("Could not click {0} to {1}." -f $click.Name, $click.TargetState.ToLowerInvariant()))
        }
        elseif ($click.StateAfterClick -ne $click.TargetState) {
            $failures.Add(("{0} pointer disclosure click expected {1}, observed {2}." -f $click.Name, $click.TargetState, $click.StateAfterClick))
        }

        if ($click.UsedAutomationFallback) {
            $failures.Add(("{0} used automation fallback for {1}; pointer disclosure proof is required." -f $click.Name, $click.TargetState))
        }
    }
    foreach ($step in $steps) {
        foreach ($failure in $step.Failures) {
            $failures.Add($failure)
        }
    }

    return [ordered]@{
        Invoked = $designExpandedClick.Clicked -and $samplesExpandedClick.Clicked -and $designCollapsedClick.Clicked -and $samplesCollapsedClick.Clicked
        HomeVisible = Test-ElementVisible $homeItem
        Clicks = @($designExpandedClick, $samplesExpandedClick, $designCollapsedClick, $samplesCollapsedClick)
        Steps = $steps.ToArray()
        ShellNavigationChanged = $failures.Count -eq 0
        Failures = $failures.ToArray()
    }
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

function Get-SelectionContainerSelectedItemNames($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $pattern = $element.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern)
        if ($null -eq $pattern) {
            return ""
        }

        $names = New-Object System.Collections.Generic.List[string]
        foreach ($selectedItem in $pattern.Current.GetSelection()) {
            $name = Get-ElementText $selectedItem
            if (![string]::IsNullOrWhiteSpace($name)) {
                $names.Add($name)
            }
        }

        return ($names -join "; ")
    }
    catch {
        return ""
    }
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

function Find-InvokePatternTarget($element) {
    $candidate = $element
    for ($depth = 0; $depth -lt 8 -and $null -ne $candidate; $depth++) {
        if (Test-ElementSupportsPattern $candidate ([System.Windows.Automation.InvokePattern]::Pattern)) {
            return $candidate
        }

        try {
            $candidate = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)
        }
        catch {
            return $null
        }
    }

    return $null
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

function Find-FirstSelectableDescendant($element, [bool]$preferUnselected = $true) {
    if ($null -eq $element) {
        return $null
    }

    $fallback = $null
    $candidates = New-Object System.Collections.Generic.List[object]
    $candidates.Add($element)
    try {
        $found = $element.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($candidate in $found) {
            $candidates.Add($candidate)
        }
    }
    catch {
    }

    foreach ($candidate in $candidates) {
        try {
            if ($candidate.Current.IsOffscreen) {
                continue
            }

            $rect = $candidate.Current.BoundingRectangle
            if ($rect.Width -le 0 -or $rect.Height -le 0) {
                continue
            }

            $pattern = $candidate.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            if ($null -eq $pattern) {
                continue
            }

            if ($null -eq $fallback) {
                $fallback = $candidate
            }

            if (!$preferUnselected -or !$pattern.Current.IsSelected) {
                return $candidate
            }
        }
        catch {
        }
    }

    return $fallback
}

function Find-ExpandCollapseTarget($element) {
    $candidate = $element
    for ($depth = 0; $depth -lt 8 -and $null -ne $candidate; $depth++) {
        if (Test-ElementSupportsPattern $candidate ([System.Windows.Automation.ExpandCollapsePattern]::Pattern)) {
            return $candidate
        }

        try {
            $candidate = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($candidate)
        }
        catch {
            return $null
        }
    }

    return $null
}

function Find-RawInvokeTarget($element) {
    $candidate = $element
    for ($depth = 0; $depth -lt 8 -and $null -ne $candidate; $depth++) {
        if (Test-ElementSupportsPattern $candidate ([System.Windows.Automation.InvokePattern]::Pattern)) {
            return $candidate
        }

        try {
            $candidate = [System.Windows.Automation.TreeWalker]::RawViewWalker.GetParent($candidate)
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

function Get-ElementHelpText($element) {
    if ($null -eq $element) {
        return ""
    }

    try {
        $value = $element.GetCurrentPropertyValue([System.Windows.Automation.AutomationElement]::HelpTextProperty)
        if ($null -ne $value) {
            return [string]$value
        }
    }
    catch {
    }

    return ""
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
            Start-Sleep -Milliseconds 180
            if ($pattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Expanded) {
                return $true
            }
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

function Refresh-ModernWpfVisualArtifacts($window) {
    $refreshButton = Find-DescendantByAutomationId $window "GalleryVisualTestRefreshArtifacts"
    if ($null -eq $refreshButton) {
        return $false
    }

    if (Invoke-ElementOnce $window $refreshButton) {
        Start-Sleep -Milliseconds 150
        return $true
    }

    return $false
}

function Get-RenderedArtifactBoundsPath([string]$artifactDir, [string]$automationId) {
    if ([string]::IsNullOrWhiteSpace($artifactDir) -or [string]::IsNullOrWhiteSpace($automationId)) {
        return ""
    }

    return Join-Path $artifactDir ("{0}.bounds.txt" -f $automationId)
}

function Get-RenderedArtifactBounds([string]$artifactDir, [string]$automationId) {
    $path = Get-RenderedArtifactBoundsPath $artifactDir $automationId
    if ([string]::IsNullOrWhiteSpace($path) -or !(Test-Path $path)) {
        return ""
    }

    try {
        $value = Get-Content -LiteralPath $path -Raw
        if ($null -eq $value) {
            return ""
        }

        return $value.Trim()
    }
    catch {
        return ""
    }
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

function Invoke-SuggestionElementOnce($window, $element) {
    $invokeTarget = Find-InvokePatternTarget $element
    if ($null -ne $invokeTarget) {
        [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
        Start-Sleep -Milliseconds 80
        try {
            $pattern = $invokeTarget.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            if ($null -ne $pattern) {
                $pattern.Invoke()
                Start-Sleep -Milliseconds 350
                return [ordered]@{
                    Invoked = $true
                    Method = "InvokePattern"
                }
            }
        }
        catch {
        }
    }

    if (Click-ElementOnce $element) {
        return [ordered]@{
            Invoked = $true
            Method = "ElementClick"
        }
    }

    if (Select-ElementOnce $window $element) {
        return [ordered]@{
            Invoked = $true
            Method = "SelectionItem"
        }
    }

    return [ordered]@{
        Invoked = $false
        Method = ""
    }
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
    if ($control -eq "CommandBar") {
        return Invoke-ElementOnce $window $element
    }

    if ($control -eq "MessageBox") {
        if ($null -eq $element) {
            return $false
        }

        [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
        Start-Sleep -Milliseconds 100
        try {
            $element.SetFocus()
            Start-Sleep -Milliseconds 100
            [GalleryRecordingNative]::Space()
            Start-Sleep -Milliseconds 600
            return $true
        }
        catch {
        }

        return Invoke-NativeClickElementOnce $window $element 600
    }

    if ($control -eq "ToolTip") {
        if ($null -eq $element) {
            return $false
        }

        [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
        try {
            $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            if ($null -ne $pattern) {
                $pattern.Invoke()
                Start-Sleep -Milliseconds 550
                return $true
            }
        }
        catch {
        }

        $center = Get-ElementCenter $element
        if ($null -eq $center) {
            return $false
        }

        $windowHandle = [IntPtr]$window.Current.NativeWindowHandle
        $offTargetX = [Math]::Max(1, $center.X - 160)
        $offTargetY = [Math]::Max(1, $center.Y - 160)
        [GalleryRecordingNative]::MoveCursorOverWindow($windowHandle, $offTargetX, $offTargetY)
        Start-Sleep -Milliseconds 250
        [GalleryRecordingNative]::Click($offTargetX, $offTargetY)
        Start-Sleep -Milliseconds 250
        try {
            $element.SetFocus()
        }
        catch {
        }
        Start-Sleep -Milliseconds 250
        $bounds = Get-ElementBoundingRectangle $element
        $entryX = if ($null -eq $bounds) { $center.X - 24 } else { [int][Math]::Floor($bounds.X - 24) }
        $entryY = $center.Y
        for ($step = 0; $step -le 8; $step++) {
            $x = [int][Math]::Round($entryX + (($center.X - $entryX) * ($step / 8.0)))
            [GalleryRecordingNative]::MoveCursorOverWindow($windowHandle, $x, $entryY)
            Start-Sleep -Milliseconds 60
        }
        [GalleryRecordingNative]::HoverCursorOverWindow($windowHandle, $center.X, $center.Y)
        Start-Sleep -Milliseconds 1200
        return $true
    }

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

function Invoke-NativeClickElementOnce($window, $element, [int]$postDelayMilliseconds = 250) {
    if ($null -eq $element) {
        return $false
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80
    $center = Get-ElementCenter $element
    if ($null -eq $center) {
        return $false
    }

    [GalleryRecordingNative]::Click($center.X, $center.Y)
    Start-Sleep -Milliseconds $postDelayMilliseconds
    return $true
}

function Find-MessageBoxDialogElement($window, [string[]]$names) {
    if ($null -eq $window -or $names.Count -eq 0) {
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

        $match = Find-DescendantByAnyName $candidateWindow $names
        if (Test-AutomationElementUsable $match) {
            return $match
        }
    }

    return $null
}

function Find-MessageBoxDialogButton($window, [string[]]$names) {
    if ($null -eq $window -or $names.Count -eq 0) {
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

        $match = Find-DescendantButtonByAnyName $candidateWindow $names
        if (Test-AutomationElementUsable $match) {
            return $match
        }
    }

    return $null
}

function Get-OpenInteractionTriggerElement($window, [string]$control, $sampleElement) {
    if ($control -eq "MenuBar" -or $control -eq "Menu") {
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

    if ($control -eq "DatePicker") {
        return Find-ElementByNameInProcess $window.Current.ProcessId @("Pick a date")
    }

    if ($control -eq "MessageBox") {
        return Find-InteractiveElementByNameInProcess $window.Current.ProcessId @("Simple MessageBox")
    }

    if ($control -eq "ToolTip") {
        return Find-ElementByNameInProcess $window.Current.ProcessId @("TooltipButton")
    }

    if ($control -eq "CommandBar") {
        $trigger = Find-DescendantByAutomationId $sampleElement "MoreButton"
        if ($null -eq $trigger) {
            $trigger = Find-DescendantButtonByAnyName $sampleElement @("More")
        }
        if ($null -eq $trigger) {
            $trigger = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "MoreButton"
        }

        return $trigger
    }

    return $sampleElement
}

function Find-OpenInteractionElement($window, $element, [string[]]$openNames, [string]$control) {
    if ($openNames.Count -eq 0) {
        return $null
    }

    if ($control -eq "TeachingTip") {
        $tip = Find-ElementByAutomationIdInProcess $window.Current.ProcessId "GallerySample_TeachingTip_TeachingTip"
        if ($null -ne $tip) {
            try {
                $rect = $tip.Current.BoundingRectangle
                if (!$tip.Current.IsOffscreen -and $rect.Width -gt 0 -and $rect.Height -gt 0) {
                    return $tip
                }
            }
            catch {
            }
        }

        return Find-ElementByNameInProcess $window.Current.ProcessId $openNames
    }

    if ($control -eq "SplitButton" -or $control -eq "ToggleSplitButton") {
        if ($null -eq $element -or (Get-ExpandCollapseStateName $element) -ne "Expanded") {
            return $null
        }
    }

    if ($control -eq "DatePicker") {
        return Find-ElementByControlTypeInProcess $window.Current.ProcessId ([System.Windows.Automation.ControlType]::Calendar)
    }

    if ($control -eq "MessageBox") {
        return Find-MessageBoxDialogElement $window $openNames
    }

    if ($control -eq "ToolTip") {
        return Find-ElementByNameInProcess $window.Current.ProcessId $openNames
    }

    if ($control -eq "CommandBar") {
        return Find-AnchoredInteractiveElementByNameInProcess $window.Current.ProcessId $openNames $element
    }

    return Find-InteractiveElementByNameInProcess $window.Current.ProcessId $openNames
}

function Wait-ForOpenInteractionElement($window, $element, [string[]]$openNames, [string]$control, [int]$timeoutMilliseconds) {
    if ($openNames.Count -eq 0) {
        return $null
    }

    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $openElement = Find-OpenInteractionElement $window $element $openNames $control
        if ($null -ne $openElement) {
            return $openElement
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $null
}

function Wait-ForOpenInteractionElementGone($window, $element, [string[]]$openNames, [string]$control, [int]$timeoutMilliseconds, $visualCloseContext = $null) {
    if ($openNames.Count -eq 0) {
        return $true
    }

    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $visualCloseResult = Test-OpenRepeatVisualClosed $window $visualCloseContext
        if ($null -ne $visualCloseResult -and $visualCloseResult.Checked) {
            if ($visualCloseResult.Closed) {
                return $true
            }

            Start-Sleep -Milliseconds 100
            continue
        }

        $openElement = if ($control -eq "Flyout" -or
            $control -eq "ContentDialog" -or
            $control -eq "Popup" -or
            $control -eq "MenuFlyout" -or
            $control -eq "CommandBar" -or
            $control -eq "CommandBarFlyout") {
            Find-ElementByNameInProcess $window.Current.ProcessId $openNames
        }
        else {
            Find-OpenInteractionElement $window $element $openNames $control
        }
        if ($null -eq $openElement) {
            if ($null -eq $visualCloseResult -or !$visualCloseResult.Checked -or $visualCloseResult.Closed) {
                return $true
            }
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Click-OpenInteractionDismissPoint($window) {
    $rect = [GalleryRecordingNative]::GetRect([IntPtr]$window.Current.NativeWindowHandle)
    $x = $rect.Left + 320
    $y = $rect.Top + 110
    [GalleryRecordingNative]::Click($x, $y)
}

function Find-CommandBarSampleOption($window, $sampleElement, [string]$name) {
    $button = Find-DescendantButtonByAnyName $sampleElement @($name)
    if ($null -eq $button) {
        $button = Find-InteractiveElementByNameInProcess $window.Current.ProcessId @($name)
    }
    if ($null -eq $button) {
        $button = Find-ElementByNameInProcess $window.Current.ProcessId @($name)
    }
    if ($null -ne $button) {
        $invokeTarget = Find-RawInvokeTarget $button
        if ($null -ne $invokeTarget) {
            $button = $invokeTarget
        }
    }

    return $button
}

function Invoke-CommandBarSampleOption($window, $sampleElement, [string]$name) {
    $button = Find-CommandBarSampleOption $window $sampleElement $name
    if ($null -eq $button) {
        return $false
    }
    return Invoke-OptionElementOnce $window $button
}

function Invoke-SampleOptionCloseAttempt($window, $button, [string]$method) {
    if ($null -eq $button) {
        return $false
    }

    [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80

    if ($method -eq "Invoke") {
        try {
            $pattern = $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            if ($null -ne $pattern) {
                $pattern.Invoke()
                Start-Sleep -Milliseconds 250
                return $true
            }
        }
        catch {
        }

        return $false
    }

    if ($method -eq "FocusSpace") {
        try {
            $button.SetFocus()
            Start-Sleep -Milliseconds 100
            [GalleryRecordingNative]::Space()
            Start-Sleep -Milliseconds 250
            return $true
        }
        catch {
        }

        return $false
    }

    if ($method -eq "Click") {
        return Click-ElementOnce $button
    }

    return Invoke-OptionElementOnce $window $button
}

function Close-WithVerifiedSampleOption($window, $sampleElement, $trigger, [string[]]$openNames, [string]$control, [string]$name, [string]$methodName, $visualCloseContext = $null) {
    $button = Find-CommandBarSampleOption $window $sampleElement $name
    if ($null -eq $button) {
        return [ordered]@{
            Closed = $false
            Method = ("{0}:NotFound" -f $methodName)
        }
    }

    foreach ($method in @("Invoke", "FocusSpace", "Click", "Fallback")) {
        if (Invoke-SampleOptionCloseAttempt $window $button $method) {
            Start-Sleep -Milliseconds 700
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = ("{0}:{1}" -f $methodName, $method)
                }
            }
        }
    }

    return [ordered]@{
        Closed = $false
        Method = ("{0}:NoClose" -f $methodName)
    }
}

function Get-OpenRepeatCloseOptionName([string]$control) {
    switch ($control) {
        "TeachingTip" { return "Close" }
        "ComboBox" { return "Green" }
        "DatePicker" { return "6" }
        "DropDownButton" { return "Send" }
        "SplitButton" { return "Red" }
        "ToggleSplitButton" { return "Bulleted list" }
        "MenuBar" { return "Exit" }
        "Menu" { return "Exit" }
        default { return "" }
    }
}

function Test-ControlSupportsTriggerToggleClose([string]$control) {
    switch ($control) {
        "TeachingTip" { return $true }
        "ComboBox" { return $true }
        "DatePicker" { return $true }
        "DropDownButton" { return $true }
        "SplitButton" { return $true }
        "ToggleSplitButton" { return $true }
        "MenuBar" { return $true }
        "Menu" { return $true }
        default { return $false }
    }
}

function Close-WithVerifiedOpenedElementClick($window, $trigger, [string[]]$openNames, [string]$control, [double]$xFraction, [double]$yFraction, [string]$methodName, $visualCloseContext = $null) {
    $openElement = Find-OpenInteractionElement $window $trigger $openNames $control
    $rect = Get-ElementBoundingRectangle $openElement
    if ($null -eq $rect) {
        return [ordered]@{
            Closed = $false
            Method = ("{0}:NotFound" -f $methodName)
        }
    }

    [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80
    $x = [int][Math]::Round($rect.X + ($rect.Width * $xFraction))
    $y = [int][Math]::Round($rect.Y + ($rect.Height * $yFraction))
    [GalleryRecordingNative]::Click($x, $y)
    Start-Sleep -Milliseconds 700
    if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
        return [ordered]@{
            Closed = $true
            Method = $methodName
        }
    }

    return [ordered]@{
        Closed = $false
        Method = ("{0}:NoClose" -f $methodName)
    }
}

function Close-WithVerifiedBoundsClick($window, $trigger, [string[]]$openNames, [string]$control, [string]$bounds, [string]$methodName, $visualCloseContext = $null) {
    $rect = ConvertFrom-BoundingRectangleString $bounds
    if ($null -eq $rect) {
        return [ordered]@{
            Closed = $false
            Method = ("{0}:NoBounds" -f $methodName)
        }
    }

    [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80
    $x = [int][Math]::Round($rect.X + ($rect.Width * 0.5))
    $y = [int][Math]::Round($rect.Y + ($rect.Height * 0.5))
    [GalleryRecordingNative]::Click($x, $y)
    Start-Sleep -Milliseconds 700
    if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
        return [ordered]@{
            Closed = $true
            Method = $methodName
        }
    }

    return [ordered]@{
        Closed = $false
        Method = ("{0}:NoClose" -f $methodName)
    }
}

function Close-WithVerifiedEscape($window, $trigger, [string[]]$openNames, [string]$control, [string]$methodName, $visualCloseContext = $null) {
    [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 80
    [GalleryRecordingNative]::Escape()
    Start-Sleep -Milliseconds 500
    if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
        return [ordered]@{
            Closed = $true
            Method = $methodName
        }
    }

    return [ordered]@{
        Closed = $false
        Method = ("{0}:NoClose" -f $methodName)
    }
}

function Close-WithVerifiedTriggerToggle($window, $trigger, [string[]]$openNames, [string]$control, [string]$methodName, $visualCloseContext = $null) {
    if (!(Invoke-OpenElementOnce $window $control $trigger)) {
        return [ordered]@{
            Closed = $false
            Method = ("{0}:NoInvoke" -f $methodName)
        }
    }

    Start-Sleep -Milliseconds 500
    if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
        return [ordered]@{
            Closed = $true
            Method = $methodName
        }
    }

    return [ordered]@{
        Closed = $false
        Method = ("{0}:NoClose" -f $methodName)
    }
}

function Close-WithVerifiedKeyboardSelection($window, $trigger, [string[]]$openNames, [string]$control, [int]$downCount, [string]$methodName, $visualCloseContext = $null) {
    [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
    Start-Sleep -Milliseconds 100
    $openElement = Find-OpenInteractionElement $window $trigger $openNames $control
    if ($null -ne $openElement) {
        try {
            $openElement.SetFocus()
            Start-Sleep -Milliseconds 100
        }
        catch {
        }
    }

    for ($i = 0; $i -lt $downCount; $i++) {
        [GalleryRecordingNative]::Down()
        Start-Sleep -Milliseconds 120
    }

    [GalleryRecordingNative]::Enter()
    Start-Sleep -Milliseconds 700
    if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
        return [ordered]@{
            Closed = $true
            Method = $methodName
        }
    }

    return [ordered]@{
        Closed = $false
        Method = ("{0}:NoClose" -f $methodName)
    }
}

function Close-WithVerifiedCollapsePattern($window, $trigger, [string[]]$openNames, [string]$control, $visualCloseContext = $null) {
    $targets = @($trigger, (Find-OpenInteractionElement $window $trigger $openNames $control))
    foreach ($target in $targets) {
        if ($null -eq $target) {
            continue
        }

        try {
            $pattern = $target.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            if ($null -eq $pattern) {
                continue
            }

            $pattern.Collapse()
            Start-Sleep -Milliseconds 700
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "CollapsePattern"
                }
            }
        }
        catch {
        }
    }

    return [ordered]@{
        Closed = $false
        Method = "CollapsePattern:NoClose"
    }
}

function Close-OpenInteractionElement($window, [string]$control, $trigger, [string[]]$openNames, $sampleElement, $visualCloseContext = $null, [string]$openedBoundsHint = "") {
    if ($control -eq "MessageBox") {
        $okButton = Find-MessageBoxDialogButton $window @("OK")
        if ($null -ne $okButton -and (Invoke-NativeClickElementOnce $window $okButton 700)) {
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1600 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "DialogOkButton:Click"
                }
            }
        }

        [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
        [GalleryRecordingNative]::Enter()
        Start-Sleep -Milliseconds 700
        if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1600 $visualCloseContext) {
            return [ordered]@{
                Closed = $true
                Method = "DialogDefaultButton:Enter"
            }
        }

        return [ordered]@{
            Closed = $false
            Method = "DialogOkButton:NoClose"
        }
    }

    if ($control -eq "ContentDialog") {
        $sampleClose = Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control "Cancel" "DialogCancelButton" $visualCloseContext
        if ($sampleClose.Closed) {
            return $sampleClose
        }
    }

    if ($control -eq "Flyout") {
        $sampleClose = Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control "Yes, empty my cart" "SampleConfirmButton" $visualCloseContext
        if ($sampleClose.Closed) {
            return $sampleClose
        }
    }

    if ($control -eq "Popup") {
        $sampleClose = Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control "Close" "SampleCloseButton" $visualCloseContext
        if ($sampleClose.Closed) {
            return $sampleClose
        }
    }

    if ($control -eq "MenuFlyout") {
        $sampleClose = Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control "By rating" "LeafMenuItem" $visualCloseContext
        if ($sampleClose.Closed) {
            return $sampleClose
        }
    }

    if ($control -eq "ComboBox") {
        $collapseClose = Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext
        if ($collapseClose.Closed) {
            return $collapseClose
        }

        $keyboardClose = Close-WithVerifiedKeyboardSelection $window $trigger $openNames $control 1 "KeyboardDownEnter" $visualCloseContext
        if ($keyboardClose.Closed) {
            return $keyboardClose
        }

        $openedElementClose = Close-WithVerifiedOpenedElementClick $window $trigger $openNames $control 0.5 1.65 "SecondItemClick" $visualCloseContext
        if ($openedElementClose.Closed) {
            return $openedElementClose
        }
    }

    if ($control -eq "DatePicker") {
        $collapseClose = Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext
        if ($collapseClose.Closed) {
            return $collapseClose
        }

        $keyboardClose = Close-WithVerifiedKeyboardSelection $window $trigger $openNames $control 1 "KeyboardDownEnter" $visualCloseContext
        if ($keyboardClose.Closed) {
            return $keyboardClose
        }

        $openedElementClose = Close-WithVerifiedOpenedElementClick $window $trigger $openNames $control 0.78 0.46 "DayCellClick" $visualCloseContext
        if ($openedElementClose.Closed) {
            return $openedElementClose
        }
    }

    if ((Test-ControlUsesFastOpenRepeatPopupBounds $control) -and ![string]::IsNullOrWhiteSpace($openedBoundsHint)) {
        $collapseClose = Close-WithVerifiedCollapsePattern $window $trigger $openNames $control $visualCloseContext
        if ($collapseClose.Closed) {
            return $collapseClose
        }

        $triggerClose = Close-WithVerifiedTriggerToggle $window $trigger $openNames $control "FastPopupTriggerToggle" $visualCloseContext
        if ($triggerClose.Closed) {
            return $triggerClose
        }

        $escapeClose = Close-WithVerifiedEscape $window $trigger $openNames $control "FastPopupEscape" $visualCloseContext
        if ($escapeClose.Closed) {
            return $escapeClose
        }

        $boundsClose = Close-WithVerifiedBoundsClick $window $trigger $openNames $control $openedBoundsHint "FastPopupBoundsClick" $visualCloseContext
        if ($boundsClose.Closed) {
            return $boundsClose
        }
    }

    $openRepeatCloseOptionName = Get-OpenRepeatCloseOptionName $control
    if (![string]::IsNullOrWhiteSpace($openRepeatCloseOptionName)) {
        $sampleClose = Close-WithVerifiedSampleOption $window $sampleElement $trigger $openNames $control $openRepeatCloseOptionName "LeafCloseItem" $visualCloseContext
        if ($sampleClose.Closed) {
            return $sampleClose
        }
    }

    if ($control -eq "Flyout" -or $control -eq "Popup" -or $control -eq "MenuFlyout") {
        [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
        for ($i = 1; $i -le 2; $i++) {
            [GalleryRecordingNative]::Escape()
            Start-Sleep -Milliseconds 700
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "Escape$i"
                }
            }
        }

        for ($i = 1; $i -le 2; $i++) {
            Click-OpenInteractionDismissPoint $window
            Start-Sleep -Milliseconds 550
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "DismissPoint$i"
                }
            }
        }

        return [ordered]@{
            Closed = $false
            Method = "DismissPoint2"
        }
    }

    if ($control -eq "CommandBar") {
        if (Invoke-CommandBarSampleOption $window $sampleElement "Close command bar") {
            Start-Sleep -Milliseconds 700
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "SampleCloseButton"
                }
            }
        }

        [void](Invoke-ElementOnce $window $trigger)
        Start-Sleep -Milliseconds 700
        if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
            return [ordered]@{
                Closed = $true
                Method = "TriggerToggle"
            }
        }
    }

    if ($control -eq "CommandBarFlyout") {
        [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)

        $secondaryCommand = Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 900
        if ($null -ne $secondaryCommand -and (Invoke-OptionElementOnce $window $secondaryCommand)) {
            Start-Sleep -Milliseconds 700
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1500 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "SecondaryCommand"
                }
            }
        }

        for ($i = 1; $i -le 2; $i++) {
            [GalleryRecordingNative]::Escape()
            Start-Sleep -Milliseconds 450
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 900 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "Escape$i"
                }
            }
        }

        for ($i = 1; $i -le 3; $i++) {
            Click-OpenInteractionDismissPoint $window
            Start-Sleep -Milliseconds 550
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "DismissPoint$i"
                }
            }
        }

        return [ordered]@{
            Closed = $false
            Method = "DismissPoint3"
        }
    }

    if (Test-ControlSupportsTriggerToggleClose $control) {
        if (Invoke-OpenElementOnce $window $control $trigger) {
            Start-Sleep -Milliseconds 700
            if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext) {
                return [ordered]@{
                    Closed = $true
                    Method = "TriggerToggle"
                }
            }
        }
    }

    [GalleryRecordingNative]::Activate([IntPtr]$window.Current.NativeWindowHandle)
    for ($i = 1; $i -le 2; $i++) {
        [GalleryRecordingNative]::Escape()
        Start-Sleep -Milliseconds 350
    }

    if (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 900 $visualCloseContext) {
        return [ordered]@{
            Closed = $true
            Method = "Escape2"
        }
    }

    Click-OpenInteractionDismissPoint $window
    Start-Sleep -Milliseconds 450
    return [ordered]@{
        Closed = (Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1200 $visualCloseContext)
        Method = "DismissPoint"
    }
}

function Open-CommandBarFlyoutSecondaryCommands($window) {
    $deadline = (Get-Date).AddMilliseconds(2500)
    do {
        $moreButton = Wait-ForCommandBarFlyoutPrimaryCommands $window 1200
        if ($null -eq $moreButton) {
            $moreButton = Find-CommandBarFlyoutMoreButton $window
        }

        if ($null -ne $moreButton -and (Invoke-ElementOnce $window $moreButton)) {
            if ($null -ne (Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 1200)) {
                return $true
            }
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Get-RecordingElapsedSeconds {
    if ($null -eq $script:GalleryRecordingStopwatch) {
        return $null
    }

    return [Math]::Round($script:GalleryRecordingStopwatch.Elapsed.TotalSeconds, 3)
}

function Invoke-MessageBoxButtonWithDelayedClose($trigger, [string[]]$openNames, [int]$processId, [int]$dwellMilliseconds) {
    if ($null -eq $trigger -or $openNames.Count -eq 0) {
        return [ordered]@{
            Invoked = $false
            OpenElementFound = $false
            OpenElementBounds = ""
            Closed = $false
            Method = "DialogOkButton:MissingTrigger"
        }
    }

    $closer = [powershell]::Create()
    $closerScript = {
        param($targetProcessId, $targetMessageName, $targetDwellMilliseconds)

        Add-Type -AssemblyName UIAutomationClient
        Add-Type -AssemblyName UIAutomationTypes
        if (-not ("GalleryMessageBoxCloserNative" -as [type])) {
            Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class GalleryMessageBoxCloserNative
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    private const byte VK_RETURN = 0x0D;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static void Enter()
    {
        keybd_event(VK_RETURN, 0, 0, UIntPtr.Zero);
        keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
"@
        }

        function Format-RectForMessageBoxCloser($rect) {
            if ($null -eq $rect -or $rect.Width -le 0 -or $rect.Height -le 0) {
                return ""
            }

            return "{0},{1},{2},{3}" -f `
                [int][Math]::Round($rect.X), `
                [int][Math]::Round($rect.Y), `
                [int][Math]::Round($rect.Width), `
                [int][Math]::Round($rect.Height)
        }

        $root = [System.Windows.Automation.AutomationElement]::RootElement
        $processCondition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
            $targetProcessId)
        $messageCondition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            $targetMessageName)
        $okCondition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            "OK")
        $deadline = (Get-Date).AddMilliseconds(5000)
        $messageElement = $null
        $okButton = $null

        do {
            $windows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $processCondition)
            foreach ($candidateWindow in $windows) {
                if ($null -eq $messageElement) {
                    $messageElement = $candidateWindow.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $messageCondition)
                }

                if ($null -eq $okButton) {
                    $foundOk = $candidateWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, $okCondition)
                    foreach ($candidateOk in $foundOk) {
                        try {
                            if ($candidateOk.Current.ControlType -eq [System.Windows.Automation.ControlType]::Button -and
                                !$candidateOk.Current.IsOffscreen -and
                                $candidateOk.Current.IsEnabled) {
                                $okButton = $candidateOk
                                break
                            }
                        }
                        catch {
                        }
                    }
                }

                if ($null -ne $messageElement -and $null -ne $okButton) {
                    break
                }
            }

            if ($null -ne $messageElement -and $null -ne $okButton) {
                break
            }

            Start-Sleep -Milliseconds 100
        } while ((Get-Date) -lt $deadline)

        $messageBounds = ""
        if ($null -ne $messageElement) {
            try {
                $messageBounds = Format-RectForMessageBoxCloser $messageElement.Current.BoundingRectangle
            }
            catch {
            }
        }

        Start-Sleep -Milliseconds $targetDwellMilliseconds

        $closed = $false
        $method = "DialogOkButton:NotFound"
        if ($null -ne $okButton) {
            try {
                $okButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
                $closed = $true
                $method = "DialogOkButton:Invoke"
            }
            catch {
                [GalleryMessageBoxCloserNative]::Enter()
                $closed = $true
                $method = "DialogOkButton:EnterFallback"
            }
        }
        else {
            [GalleryMessageBoxCloserNative]::Enter()
            $method = "DialogOkButton:EnterFallbackNoButton"
        }

        [pscustomobject]@{
            OpenElementFound = $null -ne $messageElement
            OpenElementBounds = $messageBounds
            Closed = $closed
            Method = $method
        }
    }

    [void]$closer.AddScript($closerScript).AddArgument($processId).AddArgument($openNames[0]).AddArgument($dwellMilliseconds)
    $asyncResult = $closer.BeginInvoke()
    $invoked = $false
    try {
        $pattern = $trigger.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Invoke()
            $invoked = $true
        }
    }
    catch {
    }

    $closerOutput = $closer.EndInvoke($asyncResult)
    $closer.Dispose()
    $result = @($closerOutput | Select-Object -First 1)
    if ($result.Count -eq 0 -or $null -eq $result[0]) {
        return [ordered]@{
            Invoked = $invoked
            OpenElementFound = $false
            OpenElementBounds = ""
            Closed = $false
            Method = "DialogOkButton:NoCloserResult"
        }
    }

    return [ordered]@{
        Invoked = $invoked
        OpenElementFound = [bool]$result[0].OpenElementFound
        OpenElementBounds = [string]$result[0].OpenElementBounds
        Closed = [bool]$result[0].Closed
        Method = [string]$result[0].Method
    }
}

function Invoke-MessageBoxOpenRepeatInteraction($window, [string]$control, $sampleElement) {
    $trigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    $openNames = @(Get-OpenInteractionNames $control)
    $triggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $trigger)
    $initialVisualSeconds = [Math]::Max(0.1, (Get-RecordingElapsedSeconds) - 0.75)

    $firstOpenStartSeconds = Get-RecordingElapsedSeconds
    $firstOpenResult = Invoke-MessageBoxButtonWithDelayedClose $trigger $openNames $window.Current.ProcessId 1800
    $firstOpenVisualSeconds = [Math]::Round($firstOpenStartSeconds + 0.9, 3)
    Start-Sleep -Milliseconds 1100
    $closedVisualSeconds = Get-RecordingElapsedSeconds

    $secondTrigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    if ($null -eq $secondTrigger) {
        $secondTrigger = $trigger
    }

    $secondTriggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $secondTrigger)
    Start-Sleep -Milliseconds 700
    $secondOpenStartSeconds = Get-RecordingElapsedSeconds
    $secondOpenResult = Invoke-MessageBoxButtonWithDelayedClose $secondTrigger $openNames $window.Current.ProcessId 1800
    $secondOpenVisualSeconds = [Math]::Round($secondOpenStartSeconds + 0.9, 3)

    $firstOpenElementFound = [bool]$firstOpenResult.OpenElementFound
    $secondOpenElementFound = [bool]$secondOpenResult.OpenElementFound
    $closedElementGone = [bool]$firstOpenResult.Closed -and [bool]$secondOpenResult.Closed
    $firstOpenElementBounds = [string]$firstOpenResult.OpenElementBounds
    $secondOpenElementBounds = [string]$secondOpenResult.OpenElementBounds

    if ([string]::IsNullOrWhiteSpace($firstOpenElementBounds) -and ![string]::IsNullOrWhiteSpace($secondOpenElementBounds)) {
        $firstOpenElementBounds = $secondOpenElementBounds
    }
    if ([string]::IsNullOrWhiteSpace($secondOpenElementBounds) -and ![string]::IsNullOrWhiteSpace($firstOpenElementBounds)) {
        $secondOpenElementBounds = $firstOpenElementBounds
    }

    return [ordered]@{
        Invoked = [bool]$firstOpenResult.Invoked -and [bool]$secondOpenResult.Invoked -and $firstOpenElementFound -and $secondOpenElementFound -and $closedElementGone
        FirstOpen = [bool]$firstOpenResult.Invoked
        Closed = $closedElementGone
        CloseMethod = $firstOpenResult.Method
        SecondOpen = [bool]$secondOpenResult.Invoked
        FirstOpenElementFound = $firstOpenElementFound
        SecondOpenElementFound = $secondOpenElementFound
        ClosedElementGone = $closedElementGone
        CloseVisualChecked = $false
        CloseVisualClosed = $false
        CloseVisualDelta = $null
        CloseVisualSnapshot = ""
        FirstOpenElementAnchored = $true
        SecondOpenElementAnchored = $true
        TriggerBounds = $triggerBounds
        SecondTriggerBounds = $secondTriggerBounds
        FirstOpenElementBounds = $firstOpenElementBounds
        SecondOpenElementBounds = $secondOpenElementBounds
        InitialVisualSeconds = $initialVisualSeconds
        FirstOpenStartSeconds = $firstOpenStartSeconds
        FirstOpenVisualSeconds = $firstOpenVisualSeconds
        ClosedVisualSeconds = $closedVisualSeconds
        SecondOpenStartSeconds = $secondOpenStartSeconds
        SecondOpenVisualSeconds = $secondOpenVisualSeconds
        FirstOpenExpandState = ""
        SecondOpenExpandState = ""
        InitialToggleState = ""
        FirstOpenToggleState = ""
        ClosedToggleState = ""
        SecondOpenToggleState = ""
        FirstCommandBarFlyoutSecondaryExpanded = $false
        SecondCommandBarFlyoutSecondaryExpanded = $false
        CommandBarFlyoutSecondaryExpanded = $true
    }
}

function Invoke-OpenRepeatInteraction($window, [string]$control, $sampleElement) {
    if ($control -eq "MessageBox") {
        return Invoke-MessageBoxOpenRepeatInteraction $window $control $sampleElement
    }

    $trigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    $openNames = @(Get-OpenInteractionNames $control)
    $openVisualDwellMilliseconds = switch ($control) {
        "CommandBar" { 700; break }
        "CommandBarFlyout" { 800; break }
        "ContentDialog" { 1200; break }
        "Flyout" { 1200; break }
        "Popup" { 1200; break }
        "MenuFlyout" { 1200; break }
        default { 600; break }
    }
    $closedVisualDwellMilliseconds = switch ($control) {
        "ContentDialog" { 700; break }
        "Flyout" { 700; break }
        "Popup" { 700; break }
        "MenuFlyout" { 700; break }
        default { 450; break }
    }
    $betweenOpenDwellMilliseconds = switch ($control) {
        "ContentDialog" { 450; break }
        "Flyout" { 450; break }
        "Popup" { 450; break }
        "MenuFlyout" { 450; break }
        default { 250; break }
    }
    $openElementTimeoutMilliseconds = if ($control -eq "CommandBar") {
        4000
    }
    elseif ($control -eq "ToolTip") {
        800
    }
    else {
        1200
    }

    if ($control -eq "CommandBar") {
        [void](Invoke-CommandBarSampleOption $window $sampleElement "Close command bar")
        Start-Sleep -Milliseconds 900
        [void](Wait-ForOpenInteractionElementGone $window $trigger $openNames $control 1600)
        $trigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    }

    $triggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $trigger)
    $initialToggleState = Get-ToggleStateName $trigger
    $initialVisualSeconds = [Math]::Max(0.1, (Get-RecordingElapsedSeconds) - 0.75)
    if ($control -eq "CommandBar") {
        Start-Sleep -Milliseconds 700
    }

    $visualCloseContext = New-OpenRepeatVisualCloseContext $window $control
    $firstOpenStartSeconds = Get-RecordingElapsedSeconds
    $firstOpen = if ($control -eq "CommandBar") {
        Invoke-CommandBarSampleOption $window $sampleElement "Open command bar"
    }
    else {
        Invoke-OpenElementOnce $window $control $trigger
    }
    Start-Sleep -Milliseconds 300
    $firstOpenExpandState = Get-ExpandCollapseStateName $trigger
    $firstOpenElementBoundsHint = if ($firstOpen) { Get-FastOpenRepeatPopupBounds $trigger $control } else { "" }
    $firstOpenElement = if ($openNames.Count -eq 0 -or ![string]::IsNullOrWhiteSpace($firstOpenElementBoundsHint)) { $null } else { Wait-ForOpenInteractionElement $window $trigger $openNames $control $openElementTimeoutMilliseconds }
    $firstOpenToggleState = Get-ToggleStateName $trigger
    $firstOpenElementFound = $openNames.Count -eq 0 -or $null -ne $firstOpenElement -or ![string]::IsNullOrWhiteSpace($firstOpenElementBoundsHint)
    $firstOpenElementAnchored = $openNames.Count -eq 0 -or (Test-ControlAllowsDetachedOpenRepeatElement $control) -or (Test-OpenInteractionElementAnchored $trigger $firstOpenElement) -or (Test-BoundingRectangleStringAnchored $trigger $firstOpenElementBoundsHint)
    $firstOpenElementBounds = if (![string]::IsNullOrWhiteSpace($firstOpenElementBoundsHint)) { $firstOpenElementBoundsHint } else { Format-BoundingRectangle (Get-ElementBoundingRectangle $firstOpenElement) }
    if ($control -eq "ToolTip" -and [string]::IsNullOrWhiteSpace($firstOpenElementBounds)) {
        $firstOpenElementBounds = Get-ToolTipFallbackBoundsFromTriggerBounds $triggerBounds
        $firstOpenElementFound = $firstOpen -and ![string]::IsNullOrWhiteSpace($firstOpenElementBounds)
        $firstOpenElementAnchored = $firstOpenElementFound
    }
    $firstCommandBarFlyoutSecondaryExpanded = $false
    $secondCommandBarFlyoutSecondaryExpanded = $false
    $firstOpenVisualSeconds = $null
    if ($control -eq "CommandBarFlyout") {
        $firstCommandBarFlyoutSecondaryExpanded = Open-CommandBarFlyoutSecondaryCommands $window
        if ($firstCommandBarFlyoutSecondaryExpanded) {
            $secondaryOpenElement = Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 1200
            if ($null -ne $secondaryOpenElement) {
                $firstOpenElement = $secondaryOpenElement
                $firstOpenElementFound = $true
                $firstOpenElementAnchored = Test-OpenInteractionElementAnchored $trigger $firstOpenElement
                $firstOpenElementBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $firstOpenElement)
            }
        }
        Start-Sleep -Milliseconds 450
        $firstOpenVisualSeconds = Get-RecordingElapsedSeconds
    }
    if ($null -eq $firstOpenVisualSeconds) {
        $firstOpenVisualSeconds = Get-RecordingElapsedSeconds
    }
    if ($null -ne $visualCloseContext -and
        $visualCloseContext.Contains("Generated") -and
        $visualCloseContext.Generated -and
        ![string]::IsNullOrWhiteSpace($firstOpenElementBounds)) {
        $visualCloseContext["Bounds"] = $firstOpenElementBounds
    }
    Start-Sleep -Milliseconds $openVisualDwellMilliseconds

    $closeResult = Close-OpenInteractionElement $window $control $trigger $openNames $sampleElement $visualCloseContext $firstOpenElementBounds
    Start-Sleep -Milliseconds $closedVisualDwellMilliseconds
    $closedVisualSeconds = Get-RecordingElapsedSeconds
    $closedToggleState = Get-ToggleStateName $trigger
    $closedElementGone = $closeResult.Closed
    $closeVisualChecked = $false
    $closeVisualClosed = $false
    $closeVisualDelta = $null
    $closeVisualSnapshot = ""
    if ($null -ne $visualCloseContext -and $visualCloseContext.Contains("LastCloseVisualChecked")) {
        $closeVisualChecked = $visualCloseContext.LastCloseVisualChecked
        $closeVisualClosed = $visualCloseContext.LastCloseVisualClosed
        $closeVisualDelta = $visualCloseContext.LastCloseVisualDelta
        $closeVisualSnapshot = $visualCloseContext.LastCloseVisualSnapshot
    }
    $secondTrigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    if ($null -eq $secondTrigger) {
        $secondTrigger = $trigger
    }
    $secondTriggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $secondTrigger)
    Start-Sleep -Milliseconds $betweenOpenDwellMilliseconds
    $secondOpenStartSeconds = Get-RecordingElapsedSeconds
    $secondOpen = if ($control -eq "CommandBar") {
        Invoke-CommandBarSampleOption $window $sampleElement "Open command bar"
    }
    else {
        Invoke-OpenElementOnce $window $control $secondTrigger
    }
    Start-Sleep -Milliseconds 300
    $secondOpenExpandState = Get-ExpandCollapseStateName $secondTrigger
    $secondOpenElementBoundsHint = if ($secondOpen) { Get-FastOpenRepeatPopupBounds $secondTrigger $control } else { "" }
    $secondOpenElement = if ($openNames.Count -eq 0 -or ![string]::IsNullOrWhiteSpace($secondOpenElementBoundsHint)) { $null } else { Wait-ForOpenInteractionElement $window $secondTrigger $openNames $control $openElementTimeoutMilliseconds }
    $secondOpenToggleState = Get-ToggleStateName $secondTrigger
    $secondOpenElementFound = $openNames.Count -eq 0 -or $null -ne $secondOpenElement -or ![string]::IsNullOrWhiteSpace($secondOpenElementBoundsHint)
    $secondOpenElementAnchored = $openNames.Count -eq 0 -or (Test-ControlAllowsDetachedOpenRepeatElement $control) -or (Test-OpenInteractionElementAnchored $secondTrigger $secondOpenElement) -or (Test-BoundingRectangleStringAnchored $secondTrigger $secondOpenElementBoundsHint)
    $secondOpenElementBounds = if (![string]::IsNullOrWhiteSpace($secondOpenElementBoundsHint)) { $secondOpenElementBoundsHint } else { Format-BoundingRectangle (Get-ElementBoundingRectangle $secondOpenElement) }
    if ($control -eq "ToolTip" -and [string]::IsNullOrWhiteSpace($secondOpenElementBounds)) {
        $secondOpenElementBounds = Get-ToolTipFallbackBoundsFromTriggerBounds $secondTriggerBounds
        $secondOpenElementFound = $secondOpen -and ![string]::IsNullOrWhiteSpace($secondOpenElementBounds)
        $secondOpenElementAnchored = $secondOpenElementFound
    }
    $secondOpenVisualSeconds = $null
    if ($control -eq "CommandBarFlyout") {
        $secondCommandBarFlyoutSecondaryExpanded = Open-CommandBarFlyoutSecondaryCommands $window
        if ($secondCommandBarFlyoutSecondaryExpanded) {
            $secondaryOpenElement = Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId @("Resize", "Move") 1200
            if ($null -ne $secondaryOpenElement) {
                $secondOpenElement = $secondaryOpenElement
                $secondOpenElementFound = $true
                $secondOpenElementAnchored = Test-OpenInteractionElementAnchored $secondTrigger $secondOpenElement
                $secondOpenElementBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $secondOpenElement)
            }
        }
        Start-Sleep -Milliseconds 450
        $secondOpenVisualSeconds = Get-RecordingElapsedSeconds
    }
    if ($null -eq $secondOpenVisualSeconds) {
        $secondOpenVisualSeconds = Get-RecordingElapsedSeconds
    }
    Start-Sleep -Milliseconds $openVisualDwellMilliseconds

    $commandBarFlyoutSecondaryExpanded = $control -ne "CommandBarFlyout" -or (
        $firstCommandBarFlyoutSecondaryExpanded -and $secondCommandBarFlyoutSecondaryExpanded)

    return [ordered]@{
        Invoked = $firstOpen -and $secondOpen -and $firstOpenElementFound -and $secondOpenElementFound -and $firstOpenElementAnchored -and $secondOpenElementAnchored -and $closedElementGone -and $commandBarFlyoutSecondaryExpanded
        FirstOpen = $firstOpen
        Closed = $closedElementGone
        CloseMethod = $closeResult.Method
        SecondOpen = $secondOpen
        FirstOpenElementFound = $firstOpenElementFound
        SecondOpenElementFound = $secondOpenElementFound
        ClosedElementGone = $closedElementGone
        CloseVisualChecked = $closeVisualChecked
        CloseVisualClosed = $closeVisualClosed
        CloseVisualDelta = $closeVisualDelta
        CloseVisualSnapshot = $closeVisualSnapshot
        FirstOpenElementAnchored = $firstOpenElementAnchored
        SecondOpenElementAnchored = $secondOpenElementAnchored
        TriggerBounds = $triggerBounds
        SecondTriggerBounds = $secondTriggerBounds
        FirstOpenElementBounds = $firstOpenElementBounds
        SecondOpenElementBounds = $secondOpenElementBounds
        InitialVisualSeconds = $initialVisualSeconds
        FirstOpenStartSeconds = $firstOpenStartSeconds
        FirstOpenVisualSeconds = $firstOpenVisualSeconds
        ClosedVisualSeconds = $closedVisualSeconds
        SecondOpenStartSeconds = $secondOpenStartSeconds
        SecondOpenVisualSeconds = $secondOpenVisualSeconds
        FirstOpenExpandState = $firstOpenExpandState
        SecondOpenExpandState = $secondOpenExpandState
        InitialToggleState = $initialToggleState
        FirstOpenToggleState = $firstOpenToggleState
        ClosedToggleState = $closedToggleState
        SecondOpenToggleState = $secondOpenToggleState
        FirstCommandBarFlyoutSecondaryExpanded = $firstCommandBarFlyoutSecondaryExpanded
        SecondCommandBarFlyoutSecondaryExpanded = $secondCommandBarFlyoutSecondaryExpanded
        CommandBarFlyoutSecondaryExpanded = $commandBarFlyoutSecondaryExpanded
    }
}

function Invoke-PreparedOpenInteraction($window, [string]$control, $sampleElement) {
    $trigger = Get-OpenInteractionTriggerElement $window $control $sampleElement
    $openNames = @(Get-OpenInteractionNames $control)
    $openElement = if ($openNames.Count -eq 0) { $null } else { Find-OpenInteractionElement $window $trigger $openNames $control }
    $openElementFound = $openNames.Count -eq 0 -or $null -ne $openElement
    $openElementAnchored = $openNames.Count -eq 0 -or (Test-OpenInteractionElementAnchored $trigger $openElement)

    return [ordered]@{
        Invoked = $openElementFound -and $openElementAnchored
        OpenElementFound = $openElementFound
        OpenElementAnchored = $openElementAnchored
        TriggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $trigger)
        OpenElementBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $openElement)
    }
}

function Invoke-RouteNavigationInteraction($window, [string]$control, [string]$artifactDir) {
    $triggerAutomationId = Get-RouteNavigationTriggerAutomationId $control
    $expectedRoute = Get-RouteNavigationExpectedRoute $control
    $targetSampleAutomationId = Get-RouteNavigationExpectedSampleAutomationId $control
    $trigger = if (![string]::IsNullOrWhiteSpace($triggerAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $triggerAutomationId
    }
    else {
        $null
    }
    if ($null -eq $trigger -and ![string]::IsNullOrWhiteSpace($triggerAutomationId)) {
        $trigger = Find-DescendantByAutomationId $window $triggerAutomationId
    }

    $beforeStatus = if (![string]::IsNullOrWhiteSpace($artifactDir)) { Read-ModernWpfStatusFile $artifactDir } else { $null }
    $beforeRoute = if ($null -ne $beforeStatus -and $beforeStatus.Contains("CurrentRoute")) { [string]$beforeStatus.CurrentRoute } else { "" }
    $triggerBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $trigger)
    $invoked = Invoke-ElementOnce $window $trigger
    $ready = $null
    if ($invoked -and ![string]::IsNullOrWhiteSpace($expectedRoute) -and ![string]::IsNullOrWhiteSpace($artifactDir)) {
        $ready = Wait-ModernWpfReady $window $expectedRoute $artifactDir
    }

    Start-Sleep -Milliseconds 250
    $afterStatus = if (![string]::IsNullOrWhiteSpace($artifactDir)) { Read-ModernWpfStatusFile $artifactDir } else { $null }
    $afterRoute = if ($null -ne $afterStatus -and $afterStatus.Contains("CurrentRoute")) { [string]$afterStatus.CurrentRoute } else { "" }
    $readyState = if ($null -ne $afterStatus -and $afterStatus.Contains("ReadyState")) { [string]$afterStatus.ReadyState } else { "" }
    $targetSample = if (![string]::IsNullOrWhiteSpace($targetSampleAutomationId)) {
        Find-DescendantByAutomationId $window $targetSampleAutomationId
    }
    else {
        $null
    }
    $targetSampleVisible = Test-AutomationElementUsable $targetSample
    $targetSampleBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $targetSample)

    return [ordered]@{
        Invoked = $invoked
        TriggerAutomationId = $triggerAutomationId
        TriggerBounds = $triggerBounds
        BeforeRoute = $beforeRoute
        ExpectedRoute = $expectedRoute
        AfterRoute = $afterRoute
        ReadyState = $readyState
        WaitReadyReturned = $null -ne $ready
        TargetSampleAutomationId = $targetSampleAutomationId
        TargetSampleBounds = $targetSampleBounds
        TargetSampleVisible = $targetSampleVisible
        RouteNavigationChanged = $invoked -and $afterRoute -eq $expectedRoute -and $targetSampleVisible
    }
}

function Invoke-StateInteraction($window, $sampleElement) {
    $before = Get-ToggleStateName $sampleElement
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)
    $invoked = Invoke-ElementOnce $window $sampleElement
    Start-Sleep -Milliseconds 150
    $after = Get-ToggleStateName $sampleElement
    return [ordered]@{
        Invoked = $invoked
        TargetBounds = $targetBounds
        BeforeState = $before
        AfterState = $after
        StateChanged = ![string]::IsNullOrWhiteSpace($before) -and $before -ne $after
    }
}

function Invoke-ExpansionInteraction($window, [string]$control) {
    $name = Get-ExpansionInteractionTriggerName $control
    $expectedChildName = Get-ExpansionInteractionExpectedChildName $control
    $namedElement = Find-ElementByNameInProcess $window.Current.ProcessId @($name)
    $target = Find-ExpandCollapseTarget $namedElement
    $beforeState = Get-ExpandCollapseStateName $target
    $beforeChild = if ([string]::IsNullOrWhiteSpace($expectedChildName)) {
        $false
    }
    else {
        $null -ne (Find-ElementByNameInProcess $window.Current.ProcessId @($expectedChildName))
    }

    $invoked = $false
    try {
        $pattern = $target.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        if ($null -ne $pattern) {
            $pattern.Expand()
            $invoked = $true
        }
    }
    catch {
    }

    if (!$invoked) {
        $invoked = Invoke-ElementOnce $window $target
    }

    Start-Sleep -Milliseconds 450
    $afterState = Get-ExpandCollapseStateName $target
    $afterChildElement = if ([string]::IsNullOrWhiteSpace($expectedChildName)) {
        $null
    }
    else {
        Find-ElementByNameInProcess $window.Current.ProcessId @($expectedChildName)
    }
    $afterChildVisible = Test-AutomationElementUsable $afterChildElement
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $target)
    $childBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $afterChildElement)

    return [ordered]@{
        Invoked = $invoked -and $afterState -eq "Expanded" -and $afterChildVisible
        TargetName = $name
        ExpectedChildName = $expectedChildName
        BeforeExpandState = $beforeState
        AfterExpandState = $afterState
        BeforeChildVisible = $beforeChild
        AfterChildVisible = $afterChildVisible
        TargetBounds = $targetBounds
        ChildBounds = $childBounds
        ExpansionChanged = $afterState -eq "Expanded" -and $afterChildVisible
    }
}

function Invoke-ValueInteraction($window, [string]$control, $sampleElement, [string]$artifactDir = "") {
    if ($null -eq $sampleElement) {
        return [ordered]@{ Invoked = $false }
    }

    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    $layoutStabilityTargetAutomationIds = @(Get-LayoutStabilityTargetAutomationIds $control)
    $hasLayoutStabilityTargets = $layoutStabilityTargetAutomationIds.Count -gt 0
    if ($hasLayoutStabilityTargets) {
        [void](Refresh-ModernWpfVisualArtifacts $window)
    }

    $before = Get-NumericValue $sampleElement
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)
    $beforeLayoutBoundsById = if ($hasLayoutStabilityTargets) { Get-RenderedArtifactBoundsMap $window $artifactDir $layoutStabilityTargetAutomationIds } else { $null }
    $beforeLayoutBounds = Format-BoundingRectangleMap $beforeLayoutBoundsById
    $beforeArtifactSnapshotDir = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets) {
        Copy-RenderedArtifactSnapshot $artifactDir "before-depth-change" $layoutStabilityTargetAutomationIds
    }
    else {
        ""
    }
    $themeShadowVisualBounds = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets -and $null -ne $beforeLayoutBoundsById) {
        Get-BoundingRectangleMapValue $beforeLayoutBoundsById "GallerySample_ThemeShadow_Root"
    }
    else {
        ""
    }
    $themeShadowCasterBeforeBounds = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets) {
        Get-BoundingRectangleMapValue $beforeLayoutBoundsById "GallerySample_ThemeShadow_ShadowRect"
    }
    else {
        ""
    }
    try {
        $pattern = $sampleElement.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
        if ($null -ne $pattern) {
            $target = switch ($control) {
                "RatingControl" { 3.0 }
                "Slider" { 50.0 }
                "ThemeShadow" { 64.0 }
                default { [double]$pattern.Current.Value + 10.0 }
            }
            $valueInputMethod = "RangeValuePattern"
            $dragStartPoint = ""
            $dragEndPoint = ""
            $sliderClickablePoint = ""
            if ($control -eq "ThemeShadow") {
                $range = $pattern.Current
                $minimum = [double]$range.Minimum
                $maximum = [double]$range.Maximum
                $startValue = if ($null -ne $before) { [double]$before } else { [double]$range.Value }
                $targetValue = [Math]::Max($minimum, [Math]::Min($maximum, [double]$target))
                $valueInputMethod = "RangeValuePatternAnimated"
                $steps = 18
                for ($step = 1; $step -le $steps; $step++) {
                    $currentValue = $startValue + (($targetValue - $startValue) * ($step / [double]$steps))
                    $pattern.SetValue($currentValue)
                    Start-Sleep -Milliseconds 45
                }
            }
            else {
                $pattern.SetValue($target)
            }
            Start-Sleep -Milliseconds 250
            $after = Get-NumericValue $sampleElement
            if ($hasLayoutStabilityTargets) {
                [void](Refresh-ModernWpfVisualArtifacts $window)
            }
            $afterLayoutBoundsById = if ($hasLayoutStabilityTargets) { Get-RenderedArtifactBoundsMap $window $artifactDir $layoutStabilityTargetAutomationIds } else { $null }
            $afterLayoutBounds = Format-BoundingRectangleMap $afterLayoutBoundsById
            $afterArtifactSnapshotDir = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets) {
                Copy-RenderedArtifactSnapshot $artifactDir "after-depth-change" $layoutStabilityTargetAutomationIds
            }
            else {
                ""
            }
            $themeShadowArtifactEvidence = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets) {
                Get-ThemeShadowArtifactEvidence $beforeArtifactSnapshotDir $afterArtifactSnapshotDir
            }
            else {
                $null
            }
            $themeShadowCasterAfterBounds = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets) {
                Get-BoundingRectangleMapValue $afterLayoutBoundsById "GallerySample_ThemeShadow_ShadowRect"
            }
            else {
                ""
            }
            $themeShadowCasterStable = if ($control -eq "ThemeShadow" -and $hasLayoutStabilityTargets) {
                Test-BoundingRectangleStringsNearlyEqual $themeShadowCasterBeforeBounds $themeShadowCasterAfterBounds 1.0
            }
            else {
                $false
            }
            return [ordered]@{
                Invoked = $true
                TargetBounds = $targetBounds
                ThemeShadowVisualBounds = $themeShadowVisualBounds
                ThemeShadowCasterBeforeBounds = $themeShadowCasterBeforeBounds
                ThemeShadowCasterAfterBounds = $themeShadowCasterAfterBounds
                ThemeShadowCasterStable = $themeShadowCasterStable
                ThemeShadowSourceGeometryReference = if ($control -eq "ThemeShadow") { "WinUI source-geometry captures show the shadow envelope expands with depth; the caster/card bounds are the layout contract." } else { "" }
                BeforeValue = $before
                AfterValue = $after
                TargetValue = $target
                ValueInputMethod = $valueInputMethod
                DragStartPoint = $dragStartPoint
                DragEndPoint = $dragEndPoint
                SliderClickablePoint = $sliderClickablePoint
                TargetReached = $null -ne $after -and [Math]::Abs(([double]$after) - ([double]$target)) -lt 0.001
                LayoutStabilityTargetAutomationIds = $layoutStabilityTargetAutomationIds
                LayoutStabilitySource = if ($hasLayoutStabilityTargets) { "RenderedArtifactBounds" } else { "" }
                BeforeArtifactSnapshotDir = $beforeArtifactSnapshotDir
                AfterArtifactSnapshotDir = $afterArtifactSnapshotDir
                ThemeShadowArtifactEvidence = $themeShadowArtifactEvidence
                BeforeLayoutBounds = $beforeLayoutBounds
                AfterLayoutBounds = $afterLayoutBounds
                LayoutStable = Test-BoundingRectangleMapsNearlyEqual $beforeLayoutBoundsById $afterLayoutBoundsById 1.0
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
            TargetBounds = $targetBounds
            ThemeShadowVisualBounds = $themeShadowVisualBounds
            ThemeShadowCasterBeforeBounds = $themeShadowCasterBeforeBounds
            ThemeShadowCasterAfterBounds = ""
            ThemeShadowCasterStable = $false
            ThemeShadowSourceGeometryReference = if ($control -eq "ThemeShadow") { "WinUI source-geometry captures show the shadow envelope expands with depth; the caster/card bounds are the layout contract." } else { "" }
            BeforeValue = $before
            AfterValue = $after
            TargetValue = $null
            TargetReached = $null -ne $before -and $null -ne $after -and [double]$after -ne [double]$before
            LayoutStabilityTargetAutomationIds = $layoutStabilityTargetAutomationIds
            LayoutStabilitySource = if ($hasLayoutStabilityTargets) { "RenderedArtifactBounds" } else { "" }
            BeforeLayoutBounds = $beforeLayoutBounds
            AfterLayoutBounds = ""
            LayoutStable = $false
        }
    }

    return [ordered]@{
        Invoked = $false
        TargetBounds = $targetBounds
        ThemeShadowVisualBounds = $themeShadowVisualBounds
        ThemeShadowCasterBeforeBounds = $themeShadowCasterBeforeBounds
        ThemeShadowCasterAfterBounds = ""
        ThemeShadowCasterStable = $false
        ThemeShadowSourceGeometryReference = if ($control -eq "ThemeShadow") { "WinUI source-geometry captures show the shadow envelope expands with depth; the caster/card bounds are the layout contract." } else { "" }
        BeforeValue = $before
        AfterValue = $null
        TargetValue = $null
        TargetReached = $false
        LayoutStabilityTargetAutomationIds = $layoutStabilityTargetAutomationIds
        LayoutStabilitySource = if ($hasLayoutStabilityTargets) { "RenderedArtifactBounds" } else { "" }
        BeforeLayoutBounds = $beforeLayoutBounds
        AfterLayoutBounds = ""
        LayoutStable = $false
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
    $triggerAutomationId = Get-SelectionInteractionTriggerAutomationId $control
    $containerName = Get-SelectionInteractionContainerName $control
    $expectedOutputName = Get-SelectionInteractionExpectedOutputName $control
    $outputAutomationId = Get-SelectionInteractionOutputAutomationId $control
    $target = if (![string]::IsNullOrWhiteSpace($triggerAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $triggerAutomationId
    }
    elseif (![string]::IsNullOrWhiteSpace($name)) {
        Find-InteractiveElementByNameInProcess $window.Current.ProcessId @($name)
    }
    else {
        $null
    }
    $container = $null
    if ($null -eq $target -and ![string]::IsNullOrWhiteSpace($containerName)) {
        $container = Find-ElementByNameInProcess $window.Current.ProcessId @($containerName)
        $target = Find-FirstSelectableDescendant $container $true
    }
    $outputElement = if (![string]::IsNullOrWhiteSpace($outputAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    }
    else {
        $null
    }

    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $target)
    $sampleBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)
    $beforeSampleState = Get-SelectionItemStateName $sampleElement
    $beforeTargetState = Get-SelectionItemStateName $target
    $beforeContainerSelection = Get-SelectionContainerSelectedItemNames $container
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
    $afterContainerSelection = Get-SelectionContainerSelectedItemNames $container
    $afterSampleStatus = Get-ElementItemStatus $sampleElement
    $outputElement = if (![string]::IsNullOrWhiteSpace($outputAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    }
    else {
        $null
    }
    $afterOutput = Get-ElementText $outputElement
    $outputBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $outputElement)
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
        TargetAutomationId = $triggerAutomationId
        ContainerName = $containerName
        ActualTargetName = Get-ElementText $target
        TargetBounds = $targetBounds
        SampleBounds = $sampleBounds
        ExpectedOutputName = $expectedOutputName
        OutputAutomationId = $outputAutomationId
        OutputBounds = $outputBounds
        BeforeSampleSelection = $beforeSampleState
        AfterSampleSelection = $afterSampleState
        BeforeTargetSelection = $beforeTargetState
        AfterTargetSelection = $afterTargetState
        BeforeContainerSelection = $beforeContainerSelection
        AfterContainerSelection = $afterContainerSelection
        BeforeSampleStatus = $beforeSampleStatus
        AfterSampleStatus = $afterSampleStatus
        BeforeOutput = $beforeOutput
        AfterOutput = $afterOutput
        OutputChanged = ($beforeOutput -ne $afterOutput) -or ($beforeSampleStatus -ne $afterSampleStatus)
        OutputMatched = $outputMatched
        SelectionChanged = (
            (![string]::IsNullOrWhiteSpace($beforeSampleState) -and $beforeSampleState -ne $afterSampleState) -or
            (![string]::IsNullOrWhiteSpace($beforeTargetState) -and $beforeTargetState -ne $afterTargetState) -or
            (![string]::IsNullOrWhiteSpace($afterContainerSelection) -and $beforeContainerSelection -ne $afterContainerSelection) -or
            ($beforeOutput -ne $afterOutput) -or
            ($beforeSampleStatus -ne $afterSampleStatus) -or
            $outputMatched)
    }
}

function Get-OptionInteractionTriggerName([string]$control) {
    switch ($control) {
        "Button" { return "Disable button" }
        "ColorPicker" { return "IsMoreButtonVisible" }
        "IconElement" { return "Monochrome" }
        "SplitView" { return "IsPaneOpen" }
        "TitleBar" { return "IsBackButtonVisible" }
        "InfoBar" { return "Is Open" }
        "ProgressRing" { return "Progress Options" }
        default { return "" }
    }
}

function Get-OptionInteractionTriggerAutomationId([string]$control) {
    switch ($control) {
        "SplitView" { return "GallerySample_SplitView_IsPaneOpenToggle" }
        "InfoBadge" { return "ToggleInfoBadgeOpacity" }
        "InfoBar" { return "GallerySample_InfoBar_IsOpenCheckBox" }
        "ProgressRing" { return "GallerySample_ProgressRing_ProgressToggle" }
        default { return "" }
    }
}

function Get-OptionInteractionExpectedElementAutomationId([string]$control) {
    switch ($control) {
        "TitleBar" { return "GallerySample_TitleBar_BackButton" }
        default { return "" }
    }
}

function Invoke-OptionInteraction($window, [string]$control, $sampleElement) {
    $name = Get-OptionInteractionTriggerName $control
    $automationId = Get-OptionInteractionTriggerAutomationId $control
    $expectedElementAutomationId = Get-OptionInteractionExpectedElementAutomationId $control
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
    $optionBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $target)
    $sampleBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)
    $expectedElement = if (![string]::IsNullOrWhiteSpace($expectedElementAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $expectedElementAutomationId
    }
    else {
        $null
    }
    $beforeExpectedElementVisible = Test-AutomationElementUsable $expectedElement
    $invoked = Invoke-OptionElementOnce $window $target
    Start-Sleep -Milliseconds 300
    $afterState = Get-ToggleStateName $target
    $afterSampleEnabled = Get-IsEnabledStateName $sampleElement
    $expectedElement = if (![string]::IsNullOrWhiteSpace($expectedElementAutomationId)) {
        Find-ElementByAutomationIdInProcess $window.Current.ProcessId $expectedElementAutomationId
    }
    else {
        $null
    }
    $afterExpectedElementVisible = Test-AutomationElementUsable $expectedElement
    $expectedElementBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $expectedElement)
    $stateOrSampleChanged = (
        (![string]::IsNullOrWhiteSpace($beforeState) -and $beforeState -ne $afterState) -or
        (![string]::IsNullOrWhiteSpace($beforeSampleEnabled) -and $beforeSampleEnabled -ne $afterSampleEnabled))
    $requiresExpectedElement = ![string]::IsNullOrWhiteSpace($expectedElementAutomationId)
    $expectedElementChanged = $requiresExpectedElement -and !$beforeExpectedElementVisible -and $afterExpectedElementVisible

    return [ordered]@{
        Invoked = $invoked
        OptionName = $name
        OptionAutomationId = $automationId
        ExpectedElementAutomationId = $expectedElementAutomationId
        OptionBounds = $optionBounds
        SampleBounds = $sampleBounds
        ExpectedElementBounds = $expectedElementBounds
        BeforeState = $beforeState
        AfterState = $afterState
        BeforeSampleEnabled = $beforeSampleEnabled
        AfterSampleEnabled = $afterSampleEnabled
        BeforeExpectedElementVisible = $beforeExpectedElementVisible
        AfterExpectedElementVisible = $afterExpectedElementVisible
        StateOrSampleChanged = $stateOrSampleChanged
        ExpectedElementChanged = $expectedElementChanged
        OptionChanged = if ($requiresExpectedElement) { $stateOrSampleChanged -and $expectedElementChanged } else { $stateOrSampleChanged }
    }
}

function Get-ScrollInteractionTargetAutomationId([string]$control) {
    switch ($control) {
        "AnnotatedScrollBar" { return "GallerySample_AnnotatedScrollBar_ScrollViewer" }
        "ItemsRepeater" { return "GallerySample_ItemsRepeater_VirtualizingScrollViewer" }
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

    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $target)
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
        TargetBounds = $targetBounds
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
        "RichTextEdit" { return "ModernWpf rich text" }
        "TextBox" { return "ModernWpf text" }
        "PasswordBox" { return "ModernWpf1!" }
        default { return "" }
    }
}

function Get-TextInteractionTargetName([string]$control) {
    switch ($control) {
        "RichTextEdit" { return "simple rich text editor" }
        "TextBox" { return "simple TextBox" }
        "PasswordBox" { return "Simple Password Box" }
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

    if ((Test-ElementSupportsPattern $element ([System.Windows.Automation.ValuePattern]::Pattern)) -or
        (Test-ElementSupportsPattern $element ([System.Windows.Automation.TextPattern]::Pattern))) {
        return $element
    }

    try {
        if ($element.Current.ControlType -eq [System.Windows.Automation.ControlType]::Edit) {
            return $element
        }
    }
    catch {
    }

    $edit = Find-DescendantByControlType $element ([System.Windows.Automation.ControlType]::Edit)
    if ($null -ne $edit) {
        return $edit
    }

    $document = Find-DescendantByControlType $element ([System.Windows.Automation.ControlType]::Document)
    if ($null -ne $document -and
        ((Test-ElementSupportsPattern $document ([System.Windows.Automation.ValuePattern]::Pattern)) -or
            (Test-ElementSupportsPattern $document ([System.Windows.Automation.TextPattern]::Pattern)))) {
        return $document
    }

    return $null
}

function Set-EditableElementText($window, $element, [string]$text) {
    $script:LastEditableTextMethod = ""
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
            try {
                Set-Clipboard -Value $text
                Start-Sleep -Milliseconds 80
                [GalleryRecordingNative]::PressCtrlV()
                Start-Sleep -Milliseconds 350
                if ((Get-ElementText $edit) -eq $text) {
                    $script:LastEditableTextMethod = "ClipboardPaste"
                    return $true
                }
            }
            catch {
            }

            [GalleryRecordingNative]::PressCtrlA()
            Start-Sleep -Milliseconds 50
            if ("System.Windows.Forms.SendKeys" -as [type]) {
                try {
                    [System.Windows.Forms.SendKeys]::SendWait($text)
                    Start-Sleep -Milliseconds 250
                    if ((Get-ElementText $edit) -eq $text) {
                        $script:LastEditableTextMethod = "SendKeys"
                        return $true
                    }
                }
                catch {
                }
            }

            [GalleryRecordingNative]::TypeText($text)
            Start-Sleep -Milliseconds 350
            if ((Get-ElementText $edit) -eq $text) {
                $script:LastEditableTextMethod = "UnicodeSendInput"
                return $true
            }

            [GalleryRecordingNative]::PressCtrlA()
            Start-Sleep -Milliseconds 50
            [GalleryRecordingNative]::TypeWindowMessageText($window.Current.NativeWindowHandle, $text)
            Start-Sleep -Milliseconds 350
            if ((Get-ElementText $edit) -eq $text) {
                $script:LastEditableTextMethod = "WindowMessage"
                return $true
            }

            [GalleryRecordingNative]::PressCtrlA()
            Start-Sleep -Milliseconds 50
            [GalleryRecordingNative]::TypeVirtualKeyText($text)
            Start-Sleep -Milliseconds 350
            if ((Get-ElementText $edit) -eq $text) {
                $script:LastEditableTextMethod = "VirtualKey"
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
            $script:LastEditableTextMethod = "ValuePattern"
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

function Wait-ForSuggestionClosed([int]$processId, [string[]]$names, [int]$timeoutMilliseconds) {
    $deadline = (Get-Date).AddMilliseconds($timeoutMilliseconds)
    do {
        $element = Find-InteractiveElementByNameInProcess $processId $names
        if ($null -eq $element) {
            return $true
        }

        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Invoke-SuggestionKeyboardCommit($window, $editElement, [bool]$hasMatchedOutput) {
    [GalleryRecordingNative]::Activate($window.Current.NativeWindowHandle)
    try {
        $editElement.SetFocus()
        Start-Sleep -Milliseconds 80
    }
    catch {
    }

    $sentKeys = $false
    try {
        if ("System.Windows.Forms.SendKeys" -as [type]) {
            if ($hasMatchedOutput) {
                [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
            }
            else {
                [System.Windows.Forms.SendKeys]::SendWait("{END}{DOWN}{ENTER}")
            }
            $sentKeys = $true
        }
    }
    catch {
        $sentKeys = $false
    }

    if (!$sentKeys) {
        if (!$hasMatchedOutput) {
            [GalleryRecordingNative]::End()
            Start-Sleep -Milliseconds 100
            [GalleryRecordingNative]::Down()
            Start-Sleep -Milliseconds 100
        }
        [GalleryRecordingNative]::Enter()
    }

    Start-Sleep -Milliseconds 350
    return $true
}

function Invoke-PlainTextInteraction($window, [string]$control) {
    $targetName = Get-TextInteractionTargetName $control
    $inputText = Get-TextInteractionInput $control
    $target = Find-ElementByNameInProcess $window.Current.ProcessId @($targetName)
    $editElement = Find-EditableDescendant $target
    $before = Get-ElementText $editElement
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $target)
    $editBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $editElement)
    $typed = Set-EditableElementText $window $target $inputText
    $inputMethod = $script:LastEditableTextMethod
    Start-Sleep -Milliseconds 350
    $after = Get-ElementText $editElement
    $outputMatched = if ($control -eq "PasswordBox") {
        $typed
    }
    else {
        $after -eq $inputText
    }

    return [ordered]@{
        Invoked = $typed -and $outputMatched
        TargetName = $targetName
        TargetBounds = $targetBounds
        EditBounds = $editBounds
        BeforeOutput = $before
        AfterOutput = $after
        ExpectedOutput = $inputText
        OutputMatched = $outputMatched
        InputMethod = $inputMethod
    }
}

function Invoke-TextInteraction($window, [string]$control, $sampleElement) {
    if ($control -eq "TextBox" -or $control -eq "PasswordBox" -or $control -eq "RichTextEdit") {
        return Invoke-PlainTextInteraction $window $control
    }

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
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)

    $typed = Set-EditableElementText $window $sampleElement $inputText
    $editElement = Find-EditableDescendant $sampleElement
    $editBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $editElement)
    $suggestionElement = if ($typed) {
        Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId $suggestionNames 2500
    }
    else {
        $null
    }
    $initialSuggestionBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $suggestionElement)

    $suggestionInvoked = $false
    $suggestionInvokeMethod = ""
    $suggestionClosed = $false
    $outputElement = $null
    if ($null -ne $suggestionElement) {
        $suggestionInvoked = Click-FirstSuggestionBelowEdit $editElement
        $suggestionInvokeMethod = "GeometryClick"
        $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
        if ($null -ne $outputElement) {
            $suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200
        }

        if ($null -eq $outputElement -or !$suggestionClosed) {
            $suggestionElement = Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId $suggestionNames 500
            if ($null -ne $suggestionElement) {
                $suggestionInvokeResult = Invoke-SuggestionElementOnce $window $suggestionElement
                $suggestionInvoked = $suggestionInvokeResult.Invoked
                $suggestionInvokeMethod = $suggestionInvokeResult.Method
                $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
                if ($null -ne $outputElement) {
                    $suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200
                }
            }
        }

        if ($null -eq $outputElement -or !$suggestionClosed) {
            [void](Invoke-SuggestionKeyboardCommit $window $editElement ($null -ne $outputElement))
            $suggestionInvoked = $true
            $suggestionInvokeMethod = "Keyboard"
            $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
            if ($null -ne $outputElement) {
                $suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200
            }
        }

        if ($null -eq $outputElement -or !$suggestionClosed) {
            $suggestionElement = Wait-ForInteractiveElementByNameInProcess $window.Current.ProcessId $suggestionNames 500
            $suggestionInvokeResult = Invoke-SuggestionElementOnce $window $suggestionElement
            $suggestionInvoked = $suggestionInvokeResult.Invoked
            $suggestionInvokeMethod = $suggestionInvokeResult.Method
            Start-Sleep -Milliseconds 400
            $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
            if ($null -ne $outputElement) {
                $suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200
                if (!$suggestionClosed) {
                    [void](Invoke-SuggestionKeyboardCommit $window $editElement $true)
                    $suggestionInvokeMethod = "SelectionItemKeyboard"
                    $outputElement = Wait-ForTextOutput $window.Current.ProcessId $outputAutomationId $expectedOutput 1200
                    $suggestionClosed = Wait-ForSuggestionClosed $window.Current.ProcessId $suggestionNames 1200
                }
            }
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
    $remainingSuggestionElement = Find-InteractiveElementByNameInProcess $window.Current.ProcessId $suggestionNames
    if ($suggestionInvoked) {
        $suggestionClosed = $null -eq $remainingSuggestionElement
    }
    $suggestionInvokeTarget = Find-InvokePatternTarget $suggestionElement
    $suggestionBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $suggestionElement)
    $suggestionInvokeTargetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $suggestionInvokeTarget)
    $remainingSuggestionBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $remainingSuggestionElement)
    $outputBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $outputElement)

    return [ordered]@{
        Invoked = $typed -and $null -ne $suggestionElement -and $suggestionInvoked
        Typed = $typed
        InputText = $inputText
        TargetBounds = $targetBounds
        EditBounds = $editBounds
        SuggestionNames = $suggestionNames
        SuggestionFound = $null -ne $suggestionElement
        SuggestionName = $(if ($null -ne $suggestionElement) { $suggestionElement.Current.Name } else { "" })
        InitialSuggestionBounds = $initialSuggestionBounds
        SuggestionBounds = $suggestionBounds
        SuggestionControlType = Get-ElementControlTypeName $suggestionElement
        SuggestionClassName = Get-ElementClassName $suggestionElement
        SuggestionSupportsInvoke = Test-ElementSupportsPattern $suggestionElement ([System.Windows.Automation.InvokePattern]::Pattern)
        SuggestionSupportsSelectionItem = Test-ElementSupportsPattern $suggestionElement ([System.Windows.Automation.SelectionItemPattern]::Pattern)
        SuggestionInvokeTargetBounds = $suggestionInvokeTargetBounds
        SuggestionInvokeTargetControlType = Get-ElementControlTypeName $suggestionInvokeTarget
        SuggestionInvokeTargetClassName = Get-ElementClassName $suggestionInvokeTarget
        SuggestionInvoked = $suggestionInvoked
        SuggestionInvokeMethod = $suggestionInvokeMethod
        SuggestionClosed = $suggestionClosed
        RemainingSuggestionBounds = $remainingSuggestionBounds
        OutputAutomationId = $outputAutomationId
        OutputBounds = $outputBounds
        BeforeOutput = $beforeOutput
        AfterOutput = $afterOutput
        ExpectedOutput = $expectedOutput
        OutputMatched = $afterOutput -eq $expectedOutput
    }
}

function Get-OutputInteractionOutputAutomationId([string]$control) {
    switch ($control) {
        "RepeatButton" { return "GallerySample_RepeatButton_Output" }
        "AppBarButton" { return "GallerySample_AppBarButton_Output" }
        default { return "" }
    }
}

function Get-OutputInteractionExpectedOutput([string]$control) {
    switch ($control) {
        "RepeatButton" { return "Number of clicks: 1" }
        "AppBarButton" { return "You clicked: Button1" }
        default { return "" }
    }
}

function Get-OutputInteractionElementText($element, [string]$control) {
    if ($control -eq "RepeatButton") {
        $helpText = Get-ElementHelpText $element
        if (![string]::IsNullOrWhiteSpace($helpText)) {
            return $helpText
        }
    }

    return Get-ElementText $element
}

function Invoke-OutputInteraction($window, [string]$control, $sampleElement) {
    $outputAutomationId = Get-OutputInteractionOutputAutomationId $control
    $expectedOutput = Get-OutputInteractionExpectedOutput $control
    $output = Find-ElementByAutomationIdInProcess $window.Current.ProcessId $outputAutomationId
    $before = Get-OutputInteractionElementText $output $control
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)
    $outputBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $output)
    $invoked = if ($control -eq "RepeatButton") {
        Invoke-ElementOnce $window $sampleElement
    }
    else {
        Invoke-ElementOnce $window $sampleElement
    }
    Start-Sleep -Milliseconds 250
    $after = Get-OutputInteractionElementText $output $control

    if ($before -eq $after) {
        try {
            $pattern = $sampleElement.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            if ($null -ne $pattern) {
                for ($i = 0; $i -lt 3 -and $before -eq $after; $i++) {
                    $pattern.Invoke()
                    $invoked = $true
                    Start-Sleep -Milliseconds 150
                    $after = Get-OutputInteractionElementText $output $control
                }
            }
        }
        catch {
        }
    }

    return [ordered]@{
        Invoked = $invoked
        TargetBounds = $targetBounds
        OutputAutomationId = $outputAutomationId
        OutputBounds = $outputBounds
        BeforeOutput = $before
        AfterOutput = $after
        ExpectedOutput = $expectedOutput
        OutputMatched = ([string]::IsNullOrWhiteSpace($expectedOutput) -or $after -eq $expectedOutput)
        OutputChanged = $before -ne $after
    }
}

function Invoke-BreadcrumbInteraction($window, $sampleElement) {
    $target = Find-DescendantByName $sampleElement "Folder1"
    $invokeTarget = Find-RawInvokeTarget $target
    $targetBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $invokeTarget)
    $sampleBounds = Format-BoundingRectangle (Get-ElementBoundingRectangle $sampleElement)
    $beforeFolder2 = $null -ne (Find-DescendantByName $sampleElement "Folder2")
    $beforeFolder3 = $null -ne (Find-DescendantByName $sampleElement "Folder3")
    $invoked = Invoke-ElementOnce $window $invokeTarget
    Start-Sleep -Milliseconds 500
    $afterFolder2 = $null -ne (Find-DescendantByName $sampleElement "Folder2")
    $afterFolder3 = $null -ne (Find-DescendantByName $sampleElement "Folder3")

    return [ordered]@{
        Invoked = $invoked
        TargetName = "Folder1"
        TargetControlType = if ($null -ne $invokeTarget) { $invokeTarget.Current.ControlType.ProgrammaticName } else { "" }
        TargetAutomationId = if ($null -ne $invokeTarget) { $invokeTarget.Current.AutomationId } else { "" }
        TargetBounds = $targetBounds
        SampleBounds = $sampleBounds
        BeforeFolder2Visible = $beforeFolder2
        BeforeFolder3Visible = $beforeFolder3
        AfterFolder2Visible = $afterFolder2
        AfterFolder3Visible = $afterFolder3
        BreadcrumbChanged = $beforeFolder2 -and $beforeFolder3 -and !$afterFolder2 -and !$afterFolder3
    }
}

function Invoke-RecordedInteraction($window, [string]$control, $sampleElement, [string]$artifactDir = "") {
    $kind = Get-ControlInteractionKind $control
    switch ($kind) {
        "ShellNavigation" { return Invoke-ShellNavigationInteraction $window $sampleElement }
        "Breadcrumb" { return Invoke-BreadcrumbInteraction $window $sampleElement }
        "PreparedOpen" { return Invoke-PreparedOpenInteraction $window $control $sampleElement }
        "OpenRepeat" { return Invoke-OpenRepeatInteraction $window $control $sampleElement }
        "State" { return Invoke-StateInteraction $window $sampleElement }
        "Expansion" { return Invoke-ExpansionInteraction $window $control }
        "Value" { return Invoke-ValueInteraction $window $control $sampleElement $artifactDir }
        "Selection" { return Invoke-SelectionInteraction $window $control $sampleElement }
        "Option" { return Invoke-OptionInteraction $window $control $sampleElement }
        "Text" { return Invoke-TextInteraction $window $control $sampleElement }
        "Output" { return Invoke-OutputInteraction $window $control $sampleElement }
        "RouteNavigation" { return Invoke-RouteNavigationInteraction $window $control $artifactDir }
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

function Format-NativeWindowRectangle($rect) {
    if ($null -eq $rect) {
        return ""
    }

    $width = [Math]::Max(0, $rect.Right - $rect.Left)
    $height = [Math]::Max(0, $rect.Bottom - $rect.Top)
    if ($width -le 0 -or $height -le 0) {
        return ""
    }

    return "{0},{1},{2},{3}" -f $rect.Left, $rect.Top, $width, $height
}

function Start-RecordingJob([int]$processId, [IntPtr]$windowHandle, [string]$outputPath, [string]$captureMode, [int]$durationSeconds, [string]$videoEncoder, [bool]$benchmarkEncoders, [int]$frameRate) {
    $captureRect = Get-ExpandedCaptureRect $windowHandle
    $stopFile = Join-Path (Split-Path -Parent $outputPath) (([IO.Path]::GetFileNameWithoutExtension($outputPath)) + ".stop")
    if (Test-Path -LiteralPath $stopFile) {
        Remove-Item -LiteralPath $stopFile -Force
    }

    $job = Start-Job -ScriptBlock {
        param($scriptPath, $targetProcessId, $handleValue, $left, $top, $width, $height, $output, $duration, $frameRate, $mode, $encoder, $benchmark, $stopSignal)
        $handle = [IntPtr]::new([int64]$handleValue)
        & $scriptPath -ProcessId $targetProcessId -WindowHandle $handle -Left $left -Top $top -Width $width -Height $height -Output $output -DurationSeconds $duration -FrameRate $frameRate -CaptureMode $mode -VideoEncoder $encoder -BenchmarkEncoders:$benchmark -StopFile $stopSignal
    } -ArgumentList $RecordWindowRenderedScript, $processId, ([int64]$windowHandle), $captureRect.Left, $captureRect.Top, $captureRect.Width, $captureRect.Height, $outputPath, $durationSeconds, $frameRate, $captureMode, $videoEncoder, $benchmarkEncoders, $stopFile

    return [ordered]@{
        Job = $job
        StopFile = $stopFile
        RequestedDurationSeconds = $durationSeconds
        RequestedVideoEncoder = $videoEncoder
        BenchmarkEncoders = $benchmarkEncoders
    }
}

function Request-RecordingStop($recordingJob) {
    if ($null -eq $recordingJob -or
        !($recordingJob -is [System.Collections.IDictionary]) -or
        !$recordingJob.Contains("StopFile") -or
        [string]::IsNullOrWhiteSpace($recordingJob.StopFile)) {
        return
    }

    New-Item -ItemType File -Path $recordingJob.StopFile -Force | Out-Null
}

function Normalize-InteractionResultVideoTimestamps($interactionResult, $recordingResult, $recordingStopWallClockSeconds) {
    if ($null -eq $interactionResult -or
        $null -eq $recordingResult -or
        !$recordingResult.PSObject.Properties["DurationSeconds"] -or
        $null -eq $recordingResult.DurationSeconds -or
        $null -eq $recordingStopWallClockSeconds) {
        return
    }

    $videoSeconds = [double]$recordingResult.DurationSeconds
    $wallSeconds = [double]$recordingStopWallClockSeconds
    if ($videoSeconds -le 0 -or $wallSeconds -le 0 -or $videoSeconds -ge ($wallSeconds - 0.25)) {
        return
    }

    $scale = $videoSeconds / $wallSeconds
    foreach ($field in @(
            "InitialVisualSeconds",
            "FirstOpenStartSeconds",
            "FirstOpenVisualSeconds",
            "ClosedVisualSeconds",
            "SecondOpenStartSeconds",
            "SecondOpenVisualSeconds")) {
        if ($interactionResult.Contains($field) -and $null -ne $interactionResult[$field]) {
            $original = [double]$interactionResult[$field]
            $interactionResult["${field}WallClock"] = [Math]::Round($original, 3)
            $interactionResult[$field] = [Math]::Round($original * $scale, 3)
        }
    }

    $interactionResult["VideoTimestampScale"] = [Math]::Round($scale, 4)
    $interactionResult["RecordingStopWallClockSeconds"] = [Math]::Round($wallSeconds, 3)
    $interactionResult["EncodedVideoDurationSeconds"] = [Math]::Round($videoSeconds, 3)
}

function Wait-RecordingJob($recordingJob, [int]$durationSeconds) {
    $job = if ($null -ne $recordingJob -and $recordingJob -is [System.Collections.IDictionary] -and $recordingJob.Contains("Job")) { $recordingJob.Job } else { $recordingJob }
    $timeout = [Math]::Max(($durationSeconds * 3) + 45, 60)
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
        if ($null -ne $recordingJob -and
            $recordingJob -is [System.Collections.IDictionary] -and
            $recordingJob.Contains("StopFile") -and
            ![string]::IsNullOrWhiteSpace($recordingJob.StopFile) -and
            (Test-Path -LiteralPath $recordingJob.StopFile)) {
            Remove-Item -LiteralPath $recordingJob.StopFile -Force
        }
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }
}

function Close-GalleryRecordingProcess($process) {
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

function ConvertTo-FrameRectangle($bounds, $captureRect, [int]$imageWidth, [int]$imageHeight, [int]$margin) {
    if ($null -eq $bounds -or $null -eq $captureRect -or $imageWidth -le 0 -or $imageHeight -le 0) {
        return $null
    }

    $left = [int][Math]::Floor($bounds.X - $captureRect.X - $margin)
    $top = [int][Math]::Floor($bounds.Y - $captureRect.Y - $margin)
    $right = [int][Math]::Ceiling(($bounds.X + $bounds.Width) - $captureRect.X + $margin)
    $bottom = [int][Math]::Ceiling(($bounds.Y + $bounds.Height) - $captureRect.Y + $margin)

    $left = [Math]::Max(0, [Math]::Min($imageWidth, $left))
    $top = [Math]::Max(0, [Math]::Min($imageHeight, $top))
    $right = [Math]::Max(0, [Math]::Min($imageWidth, $right))
    $bottom = [Math]::Max(0, [Math]::Min($imageHeight, $bottom))

    if (($right - $left) -le 1 -or ($bottom - $top) -le 1) {
        return $null
    }

    return [pscustomobject]@{
        X = $left
        Y = $top
        Width = $right - $left
        Height = $bottom - $top
    }
}

function Compare-ImageRegionMeanDelta([string]$firstPath, [string]$secondPath, [string]$boundsText, [string]$captureRectText) {
    if (!(Test-Path $firstPath) -or !(Test-Path $secondPath)) {
        return $null
    }

    $bounds = ConvertFrom-BoundingRectangleString $boundsText
    $captureRect = ConvertFrom-BoundingRectangleString $captureRectText
    if ($null -eq $bounds -or $null -eq $captureRect) {
        return $null
    }

    $first = [System.Drawing.Bitmap]::FromFile((Resolve-Path $firstPath).Path)
    $second = [System.Drawing.Bitmap]::FromFile((Resolve-Path $secondPath).Path)
    try {
        $width = [Math]::Min($first.Width, $second.Width)
        $height = [Math]::Min($first.Height, $second.Height)
        $region = ConvertTo-FrameRectangle $bounds $captureRect $width $height 20
        if ($null -eq $region) {
            return $null
        }

        $step = [Math]::Max(1, [int][Math]::Floor([Math]::Max($region.Width, $region.Height) / 120.0))
        $sum = 0.0
        $count = 0
        for ($y = $region.Y; $y -lt ($region.Y + $region.Height); $y += $step) {
            for ($x = $region.X; $x -lt ($region.X + $region.Width); $x += $step) {
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

function Compare-FrameWindowRegionToAnchorMeanDelta([string]$framePath, [string]$anchorPath, [string]$windowBoundsText, [string]$captureRectText) {
    if (!(Test-Path $framePath) -or !(Test-Path $anchorPath)) {
        return $null
    }

    $windowBounds = ConvertFrom-BoundingRectangleString $windowBoundsText
    $captureRect = ConvertFrom-BoundingRectangleString $captureRectText
    if ($null -eq $windowBounds -or $null -eq $captureRect) {
        return $null
    }

    $frame = [System.Drawing.Bitmap]::FromFile((Resolve-Path $framePath).Path)
    $anchor = [System.Drawing.Bitmap]::FromFile((Resolve-Path $anchorPath).Path)
    try {
        $region = ConvertTo-FrameRectangle $windowBounds $captureRect $frame.Width $frame.Height 0
        if ($null -eq $region) {
            return $null
        }

        $width = [Math]::Min([int]$region.Width, $anchor.Width)
        $height = [Math]::Min([int]$region.Height, $anchor.Height)
        if ($width -le 1 -or $height -le 1) {
            return $null
        }

        $step = [Math]::Max(1, [int][Math]::Floor([Math]::Max($width, $height) / 220.0))
        $sum = 0.0
        $count = 0
        for ($y = 0; $y -lt $height; $y += $step) {
            for ($x = 0; $x -lt $width; $x += $step) {
                $a = $frame.GetPixel([int]$region.X + $x, [int]$region.Y + $y)
                $b = $anchor.GetPixel($x, $y)
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
        $frame.Dispose()
        $anchor.Dispose()
    }
}

function Get-ImageLuminanceSamples([string]$path, $region, [int]$stepDivisor) {
    if (!(Test-Path $path)) {
        return $null
    }

    $bitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path $path).Path)
    try {
        $left = 0
        $top = 0
        $right = $bitmap.Width
        $bottom = $bitmap.Height
        if ($null -ne $region) {
            $left = [Math]::Max(0, [Math]::Min($bitmap.Width, [int]$region.X))
            $top = [Math]::Max(0, [Math]::Min($bitmap.Height, [int]$region.Y))
            $right = [Math]::Max(0, [Math]::Min($bitmap.Width, [int]($region.X + $region.Width)))
            $bottom = [Math]::Max(0, [Math]::Min($bitmap.Height, [int]($region.Y + $region.Height)))
        }

        $width = $right - $left
        $height = $bottom - $top
        if ($width -le 1 -or $height -le 1) {
            return $null
        }

        $step = [Math]::Max(1, [int][Math]::Floor([Math]::Max($width, $height) / [double]$stepDivisor))
        $samples = New-Object System.Collections.Generic.List[double]
        for ($y = $top; $y -lt $bottom; $y += $step) {
            for ($x = $left; $x -lt $right; $x += $step) {
                $pixel = $bitmap.GetPixel($x, $y)
                $samples.Add((0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B))
            }
        }

        if ($samples.Count -eq 0) {
            return $null
        }

        return $samples.ToArray()
    }
    finally {
        $bitmap.Dispose()
    }
}

function Get-ImageRegionLuminanceSamples([string]$path, [string]$boundsText, [string]$captureRectText) {
    if (!(Test-Path $path)) {
        return $null
    }

    $bounds = ConvertFrom-BoundingRectangleString $boundsText
    $captureRect = ConvertFrom-BoundingRectangleString $captureRectText
    if ($null -eq $bounds -or $null -eq $captureRect) {
        return $null
    }

    $bitmap = [System.Drawing.Bitmap]::FromFile((Resolve-Path $path).Path)
    try {
        $region = ConvertTo-FrameRectangle $bounds $captureRect $bitmap.Width $bitmap.Height 20
        if ($null -eq $region) {
            return $null
        }
    }
    finally {
        $bitmap.Dispose()
    }

    return Get-ImageLuminanceSamples $path $region 120
}

function Compare-LuminanceSamples($firstSamples, $secondSamples) {
    if ($null -eq $firstSamples -or $null -eq $secondSamples) {
        return $null
    }

    $count = [Math]::Min(@($firstSamples).Count, @($secondSamples).Count)
    if ($count -eq 0) {
        return $null
    }

    $sum = 0.0
    for ($i = 0; $i -lt $count; $i++) {
        $sum += [Math]::Abs([double]$firstSamples[$i] - [double]$secondSamples[$i])
    }

    return [Math]::Round($sum / $count, 3)
}

function Test-ControlRequiresLiveVisualClose([string]$control) {
    return $control -eq "TeachingTip" -or
        $control -eq "ComboBox" -or
        $control -eq "DatePicker" -or
        $control -eq "MessageBox" -or
        $control -eq "ContentDialog" -or
        $control -eq "DropDownButton" -or
        $control -eq "SplitButton" -or
        $control -eq "ToggleSplitButton" -or
        $control -eq "MenuBar" -or
        $control -eq "Menu" -or
        $control -eq "Flyout" -or
        $control -eq "Popup" -or
        $control -eq "MenuFlyout" -or
        $control -eq "CommandBar" -or
        $control -eq "CommandBarFlyout"
}

function Format-CaptureRectangle($captureRect) {
    if ($null -eq $captureRect) {
        return ""
    }

    return "{0},{1},{2},{3}" -f $captureRect.Left, $captureRect.Top, $captureRect.Width, $captureRect.Height
}

function Get-LatestLiveRecordingFramePath {
    if ([string]::IsNullOrWhiteSpace($script:GalleryLiveFrameDirectory) -or
        !(Test-Path $script:GalleryLiveFrameDirectory)) {
        return ""
    }

    $frames = @(Get-ChildItem -LiteralPath $script:GalleryLiveFrameDirectory -Filter "frame-*.png" -File -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending)
    foreach ($frame in $frames) {
        if ($frame.Length -le 0) {
            continue
        }

        $bitmap = $null
        try {
            $bitmap = [System.Drawing.Bitmap]::FromFile($frame.FullName)
            if ($bitmap.Width -gt 1 -and $bitmap.Height -gt 1) {
                return $frame.FullName
            }
        }
        catch {
        }
        finally {
            if ($null -ne $bitmap) {
                $bitmap.Dispose()
            }
        }
    }

    return ""
}

function Save-WindowVisualSnapshot($window, [string]$name, $captureRect = $null) {
    if ($null -eq $window) {
        return $null
    }

    if ($null -eq $captureRect) {
        $captureRect = Get-ExpandedCaptureRect ([IntPtr]$window.Current.NativeWindowHandle)
    }

    if ($null -eq $captureRect -or $captureRect.Width -le 1 -or $captureRect.Height -le 1) {
        return $null
    }

    $snapshotDir = if ($script:GalleryVisualSnapshotDirectory) {
        $script:GalleryVisualSnapshotDirectory
    }
    else {
        Join-Path ([System.IO.Path]::GetTempPath()) "modernwpf-gallery-visual-checks"
    }
    New-Item -ItemType Directory -Force -Path $snapshotDir | Out-Null

    $safeName = $name -replace "[^A-Za-z0-9_.-]", "_"
    $path = Join-Path $snapshotDir ("{0}.png" -f $safeName)
    $liveFramePath = Get-LatestLiveRecordingFramePath
    if (![string]::IsNullOrWhiteSpace($liveFramePath)) {
        try {
            Copy-Item -LiteralPath $liveFramePath -Destination $path -Force
            return [ordered]@{
                Path = $path
                Rect = Format-CaptureRectangle $captureRect
                CaptureRect = $captureRect
            }
        }
        catch {
        }
    }

    $bitmap = New-Object System.Drawing.Bitmap $captureRect.Width, $captureRect.Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($captureRect.Left, $captureRect.Top, 0, 0, $bitmap.Size)
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    catch {
        return $null
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }

    return [ordered]@{
        Path = $path
        Rect = Format-CaptureRectangle $captureRect
        CaptureRect = $captureRect
    }
}

function New-OpenRepeatVisualCloseContext($window, [string]$control) {
    if (!(Test-ControlRequiresLiveVisualClose $control)) {
        return $null
    }

    $baseline = Save-WindowVisualSnapshot $window "open-repeat-baseline"
    if ($null -eq $baseline) {
        return [ordered]@{
            Generated = $false
            Reason = "Baseline live snapshot could not be captured."
        }
    }

    return [ordered]@{
        Generated = $true
        BaselinePath = $baseline.Path
        CaptureRect = $baseline.Rect
        CaptureRectValue = $baseline.CaptureRect
        Bounds = ""
        LastCloseVisualChecked = $false
        LastCloseVisualClosed = $false
        LastCloseVisualDelta = $null
        LastCloseVisualSnapshot = ""
    }
}

function Test-OpenRepeatVisualClosed($window, $visualCloseContext) {
    if ($null -eq $visualCloseContext) {
        return $null
    }

    if (!$visualCloseContext.Contains("Generated") -or !$visualCloseContext.Generated) {
        return [ordered]@{
            Checked = $false
            Closed = $true
            Reason = if ($visualCloseContext.Contains("Reason")) { $visualCloseContext.Reason } else { "Visual close context was not generated." }
        }
    }

    if (!$visualCloseContext.Contains("Bounds") -or [string]::IsNullOrWhiteSpace($visualCloseContext.Bounds)) {
        return [ordered]@{
            Checked = $false
            Closed = $true
            Reason = "Open-repeat bounds are not available."
        }
    }

    $snapshot = Save-WindowVisualSnapshot $window ("open-repeat-close-{0:HHmmssfff}" -f (Get-Date)) $visualCloseContext.CaptureRectValue
    if ($null -eq $snapshot) {
        return [ordered]@{
            Checked = $false
            Closed = $true
            Reason = "Live close snapshot could not be captured."
        }
    }

    $baselineSamples = Get-ImageRegionLuminanceSamples $visualCloseContext.BaselinePath $visualCloseContext.Bounds $visualCloseContext.CaptureRect
    $currentSamples = Get-ImageRegionLuminanceSamples $snapshot.Path $visualCloseContext.Bounds $visualCloseContext.CaptureRect
    $delta = Compare-LuminanceSamples $baselineSamples $currentSamples
    $closed = $null -ne $delta -and [double]$delta -le 1.0

    $visualCloseContext["LastCloseVisualChecked"] = $true
    $visualCloseContext["LastCloseVisualClosed"] = $closed
    $visualCloseContext["LastCloseVisualDelta"] = $delta
    $visualCloseContext["LastCloseVisualSnapshot"] = $snapshot.Path

    return [ordered]@{
        Checked = $true
        Closed = $closed
        Delta = $delta
        Snapshot = $snapshot.Path
    }
}

function Get-PosterFrameIntervalSeconds([string]$control, [string]$interactionKind) {
    return 0.5
}

function Get-ControlRecordingFrameRate([string]$control, [string]$interactionKind) {
    if ($control -eq "CommandBarFlyout" -and $interactionKind -eq "OpenRepeat") {
        return [Math]::Max(30, $FrameRate)
    }

    if ($control -eq "ThemeShadow" -and $interactionKind -eq "Value") {
        return [Math]::Max(30, $FrameRate)
    }

    return $FrameRate
}

function Export-PosterFrames([string]$videoPath, [string]$caseDir, [double]$frameIntervalSeconds) {
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
    $sampleTime = $frameIntervalSeconds
    $index = 0
    while ($sampleTime -lt ($effectiveDuration - 0.2)) {
        $frameSpecs.Add(@{
            Name = ("t{0:0000}" -f [int][Math]::Round($sampleTime * 1000.0))
            Seconds = $sampleTime
        })
        $sampleTime += $frameIntervalSeconds
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

function Export-DenseTransitionReviewSheet([string]$videoPath, [string]$caseDir, [int]$durationSeconds) {
    if ($SkipFrameExtraction) {
        return $null
    }

    $ffmpeg = Get-FfmpegPath
    if ([string]::IsNullOrWhiteSpace($ffmpeg)) {
        return $null
    }

    $analysisDir = Join-Path $caseDir "analysis"
    New-Item -ItemType Directory -Force -Path $analysisDir | Out-Null

    $sheetPath = Join-Path $analysisDir "dense-transition-review.jpg"
    $reviewFps = [Math]::Min([Math]::Max(8, $FrameRate), 15)
    $tileColumns = 8
    $actualDuration = Get-VideoDurationSeconds $videoPath
    $effectiveDuration = if ($null -ne $actualDuration -and $actualDuration -gt 0.5) { $actualDuration } else { [double]$durationSeconds }
    $tileRows = [Math]::Max(4, [int][Math]::Ceiling(($effectiveDuration * $reviewFps) / [double]$tileColumns))
    $filter = "fps=$reviewFps,scale=360:-1,tile=${tileColumns}x$tileRows"
    $output = & $ffmpeg -hide_banner -loglevel error -y -i $videoPath -vf $filter -frames:v 1 $sheetPath 2>&1
    if ($LASTEXITCODE -ne 0 -or !(Test-Path $sheetPath)) {
        return [ordered]@{
            Path = $sheetPath
            Generated = $false
            Error = ($output | Select-Object -Last 3) -join " "
        }
    }

    return [ordered]@{
        Path = $sheetPath
        Generated = $true
        Fps = $reviewFps
        Tile = "${tileColumns}x$tileRows"
        Stats = Get-ImageStats $sheetPath
    }
}

function Export-DenseAnalysisFrames([string]$videoPath, [string]$caseDir, [string]$name, [int]$fps) {
    if ($SkipFrameExtraction) {
        return @()
    }

    $ffmpeg = Get-FfmpegPath
    if ([string]::IsNullOrWhiteSpace($ffmpeg) -or !(Test-Path $videoPath)) {
        return @()
    }

    $analysisDir = Join-Path $caseDir "analysis"
    $frameDir = Join-Path $analysisDir $name
    New-Item -ItemType Directory -Force -Path $frameDir | Out-Null
    $pattern = Join-Path $frameDir "frame-%04d.png"
    $effectiveFps = [Math]::Max(1, $fps)
    $filter = "fps=$effectiveFps"
    $output = & $ffmpeg -hide_banner -loglevel error -y -i $videoPath -vf $filter $pattern 2>&1
    if ($LASTEXITCODE -ne 0) {
        return @()
    }

    return @(Get-ChildItem -LiteralPath $frameDir -Filter "frame-*.png" | Sort-Object Name | ForEach-Object { $_.FullName })
}

function Get-PixelLuma($pixel) {
    return (0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B)
}

function Compare-ImageRegionMeanDeltaSampled($baseline, $current, $region, [int]$sampleStep) {
    if ($null -eq $baseline -or $null -eq $current -or $null -eq $region) {
        return $null
    }

    $left = [int]$region.X
    $top = [int]$region.Y
    $width = [int]$region.Width
    $height = [int]$region.Height
    if ($width -le 1 -or $height -le 1) {
        return $null
    }

    $sum = 0.0
    $count = 0
    for ($y = 0; $y -lt $height; $y += $sampleStep) {
        $pixelY = $top + $y
        if ($pixelY -lt 0 -or $pixelY -ge $baseline.Height -or $pixelY -ge $current.Height) {
            continue
        }

        for ($x = 0; $x -lt $width; $x += $sampleStep) {
            $pixelX = $left + $x
            if ($pixelX -lt 0 -or $pixelX -ge $baseline.Width -or $pixelX -ge $current.Width) {
                continue
            }

            $baselinePixel = $baseline.GetPixel($pixelX, $pixelY)
            $currentPixel = $current.GetPixel($pixelX, $pixelY)
            $sum += [Math]::Abs((Get-PixelLuma $baselinePixel) - (Get-PixelLuma $currentPixel))
            $count++
        }
    }

    if ($count -eq 0) {
        return $null
    }

    return [ordered]@{
        MeanDelta = [Math]::Round($sum / [double]$count, 3)
        SampleCount = $count
    }
}

function Format-FrameRectangle($region) {
    if ($null -eq $region) {
        return ""
    }

    return ("{0},{1},{2},{3}" -f $region.X, $region.Y, $region.Width, $region.Height)
}

function Get-FrameRectangleMaxDelta($first, $second) {
    if ($null -eq $first -or $null -eq $second) {
        return $null
    }

    return [Math]::Max(
        [Math]::Max([Math]::Abs([int]$first.X - [int]$second.X), [Math]::Abs([int]$first.Y - [int]$second.Y)),
        [Math]::Max([Math]::Abs([int]$first.Width - [int]$second.Width), [Math]::Abs([int]$first.Height - [int]$second.Height)))
}

function Get-LumaMedian([double[]]$values) {
    if ($null -eq $values -or $values.Count -eq 0) {
        return $null
    }

    $ordered = @($values | Sort-Object)
    $middle = [int][Math]::Floor($ordered.Count / 2)
    if (($ordered.Count % 2) -eq 0) {
        return (($ordered[$middle - 1] + $ordered[$middle]) / 2.0)
    }

    return $ordered[$middle]
}

function Get-ThemeShadowCardEdgeBounds($bitmap, $region) {
    if ($null -eq $bitmap -or $null -eq $region) {
        return $null
    }

    $margin = 32
    $left = [Math]::Max(0, [int]$region.X - $margin)
    $top = [Math]::Max(0, [int]$region.Y - $margin)
    $right = [Math]::Min($bitmap.Width, [int]$region.X + [int]$region.Width + $margin)
    $bottom = [Math]::Min($bitmap.Height, [int]$region.Y + [int]$region.Height + $margin)
    if (($right - $left) -le 8 -or ($bottom - $top) -le 8) {
        return $null
    }

    $centerX = [Math]::Max(0, [Math]::Min($bitmap.Width - 1, [int]([double]$region.X + ([double]$region.Width / 2.0))))
    $centerY = [Math]::Max(0, [Math]::Min($bitmap.Height - 1, [int]([double]$region.Y + ([double]$region.Height / 2.0))))
    $cardLuma = Get-PixelLuma ($bitmap.GetPixel($centerX, $centerY))

    $backgroundSamples = New-Object System.Collections.Generic.List[double]
    $samplePoints = @(
        @([int]$left, $centerY),
        @([int]($right - 1), $centerY),
        @($centerX, [int]$top),
        @($centerX, [int]($bottom - 1)),
        @([int]$left, [int]$top),
        @([int]($right - 1), [int]$top),
        @([int]$left, [int]($bottom - 1)),
        @([int]($right - 1), [int]($bottom - 1))
    )
    foreach ($point in $samplePoints) {
        $x = [Math]::Max(0, [Math]::Min($bitmap.Width - 1, [int]$point[0]))
        $y = [Math]::Max(0, [Math]::Min($bitmap.Height - 1, [int]$point[1]))
        $backgroundSamples.Add((Get-PixelLuma ($bitmap.GetPixel($x, $y))))
    }

    $backgroundLuma = Get-LumaMedian $backgroundSamples.ToArray()
    if ($null -eq $backgroundLuma -or [Math]::Abs($cardLuma - [double]$backgroundLuma) -lt 2.0) {
        return $null
    }

    $threshold = [double]$backgroundLuma + (($cardLuma - [double]$backgroundLuma) * 0.6)
    $cardIsBrighter = $cardLuma -gt [double]$backgroundLuma
    $columnCounts = @{}
    $rowCounts = @{}

    for ($x = $left; $x -lt $right; $x++) {
        $columnCounts[$x] = 0
    }
    for ($y = $top; $y -lt $bottom; $y++) {
        $rowCounts[$y] = 0
    }

    for ($y = $top; $y -lt $bottom; $y++) {
        for ($x = $left; $x -lt $right; $x++) {
            $luma = Get-PixelLuma ($bitmap.GetPixel($x, $y))
            $isCardPixel = if ($cardIsBrighter) { $luma -ge $threshold } else { $luma -le $threshold }
            if ($isCardPixel) {
                $columnCounts[$x] = [int]$columnCounts[$x] + 1
                $rowCounts[$y] = [int]$rowCounts[$y] + 1
            }
        }
    }

    $minColumnHits = [Math]::Max(20, [int]([double]$region.Height * 0.45))
    $minRowHits = [Math]::Max(20, [int]([double]$region.Width * 0.45))

    $xStart = $null
    $xEnd = $null
    for ($x = $left; $x -lt $right; $x++) {
        if ([int]$columnCounts[$x] -ge $minColumnHits) {
            if ($null -eq $xStart) {
                $xStart = $x
            }
            $xEnd = $x
        }
    }

    $yStart = $null
    $yEnd = $null
    for ($y = $top; $y -lt $bottom; $y++) {
        if ([int]$rowCounts[$y] -ge $minRowHits) {
            if ($null -eq $yStart) {
                $yStart = $y
            }
            $yEnd = $y
        }
    }

    if ($null -eq $xStart -or $null -eq $xEnd -or $null -eq $yStart -or $null -eq $yEnd) {
        return $null
    }

    return [pscustomobject]@{
        X = [int]$xStart
        Y = [int]$yStart
        Width = [int]($xEnd - $xStart + 1)
        Height = [int]($yEnd - $yStart + 1)
    }
}

function Get-ThemeShadowShadowEnvelopeBounds($bitmap, $cardRegion, [int]$sampleStep = 3) {
    if ($null -eq $bitmap -or $null -eq $cardRegion) {
        return $null
    }

    $sampleStep = [Math]::Max(1, $sampleStep)
    $margin = 96
    $left = [Math]::Max(0, [int]$cardRegion.X - $margin)
    $top = [Math]::Max(0, [int]$cardRegion.Y - $margin)
    $right = [Math]::Min($bitmap.Width, [int]$cardRegion.X + [int]$cardRegion.Width + $margin)
    $bottom = [Math]::Min($bitmap.Height, [int]$cardRegion.Y + [int]$cardRegion.Height + $margin)
    if (($right - $left) -le 8 -or ($bottom - $top) -le 8) {
        return $null
    }

    $backgroundSamples = New-Object System.Collections.Generic.List[double]
    $samplePoints = @(
        @([int]$left, [int]$top),
        @([int]($right - 1), [int]$top),
        @([int]$left, [int]($bottom - 1)),
        @([int]($right - 1), [int]($bottom - 1)),
        @([int](($left + $right) / 2), [int]$top),
        @([int](($left + $right) / 2), [int]($bottom - 1))
    )
    foreach ($point in $samplePoints) {
        $x = [Math]::Max(0, [Math]::Min($bitmap.Width - 1, [int]$point[0]))
        $y = [Math]::Max(0, [Math]::Min($bitmap.Height - 1, [int]$point[1]))
        $backgroundSamples.Add((Get-PixelLuma ($bitmap.GetPixel($x, $y))))
    }

    $backgroundLuma = Get-LumaMedian $backgroundSamples.ToArray()
    if ($null -eq $backgroundLuma) {
        return $null
    }

    $ignoredLeft = [int]$cardRegion.X - 1
    $ignoredTop = [int]$cardRegion.Y - 1
    $ignoredRight = [int]$cardRegion.X + [int]$cardRegion.Width + 1
    $ignoredBottom = [int]$cardRegion.Y + [int]$cardRegion.Height + 1
    $threshold = 1.5
    $xStart = $null
    $xEnd = $null
    $yStart = $null
    $yEnd = $null
    $pixelCount = 0

    for ($y = $top; $y -lt $bottom; $y += $sampleStep) {
        for ($x = $left; $x -lt $right; $x += $sampleStep) {
            if ($x -ge $ignoredLeft -and $x -lt $ignoredRight -and
                $y -ge $ignoredTop -and $y -lt $ignoredBottom) {
                continue
            }

            $luma = Get-PixelLuma ($bitmap.GetPixel($x, $y))
            if ([Math]::Abs([double]$luma - [double]$backgroundLuma) -lt $threshold) {
                continue
            }

            if ($null -eq $xStart -or $x -lt $xStart) {
                $xStart = $x
            }
            if ($null -eq $xEnd -or $x -gt $xEnd) {
                $xEnd = $x
            }
            if ($null -eq $yStart -or $y -lt $yStart) {
                $yStart = $y
            }
            if ($null -eq $yEnd -or $y -gt $yEnd) {
                $yEnd = $y
            }
            $pixelCount++
        }
    }

    if ($pixelCount -lt 25 -or $null -eq $xStart -or $null -eq $xEnd -or $null -eq $yStart -or $null -eq $yEnd) {
        return $null
    }

    return [pscustomobject]@{
        X = [int]$xStart
        Y = [int]$yStart
        Width = [int]($xEnd - $xStart + $sampleStep)
        Height = [int]($yEnd - $yStart + $sampleStep)
    }
}

function Get-ThemeShadowDenseFrameStability($videoPath, [string]$caseDir, $recordingResult, $interactionResult) {
    $cardDeltaThreshold = 2.0
    $cardEdgeShiftThreshold = 1.0
    $sampleStep = 6
    $maxAnalyzedFrames = 72

    if ($SkipFrameExtraction) {
        return [ordered]@{
            Generated = $false
            Stable = $false
            Reason = "Frame extraction was skipped."
            CardDeltaThreshold = $cardDeltaThreshold
            CardEdgeShiftThreshold = $cardEdgeShiftThreshold
        }
    }

    if ($null -eq $recordingResult -or [string]::IsNullOrWhiteSpace($recordingResult.Rect)) {
        return [ordered]@{
            Generated = $false
            Stable = $false
            Reason = "Recorder capture rectangle was not reported."
            CardDeltaThreshold = $cardDeltaThreshold
            CardEdgeShiftThreshold = $cardEdgeShiftThreshold
        }
    }

    if ($null -eq $interactionResult -or !$interactionResult.Contains("ThemeShadowCasterBeforeBounds")) {
        return [ordered]@{
            Generated = $false
            Stable = $false
            Reason = "ThemeShadow caster bounds were not reported."
            CardDeltaThreshold = $cardDeltaThreshold
            CardEdgeShiftThreshold = $cardEdgeShiftThreshold
        }
    }

    $bounds = ConvertFrom-BoundingRectangleString $interactionResult.ThemeShadowCasterBeforeBounds
    $captureRect = ConvertFrom-BoundingRectangleString $recordingResult.Rect
    if ($null -eq $bounds -or $null -eq $captureRect) {
        return [ordered]@{
            Generated = $false
            Stable = $false
            Reason = "ThemeShadow caster or capture bounds could not be parsed."
            CardDeltaThreshold = $cardDeltaThreshold
            CardEdgeShiftThreshold = $cardEdgeShiftThreshold
        }
    }

    $targetFps = if ($recordingResult.PSObject.Properties["FrameRate"] -and $null -ne $recordingResult.FrameRate) {
        [int]$recordingResult.FrameRate
    }
    else {
        [Math]::Max(1, $FrameRate)
    }
    $framePaths = @(Export-DenseAnalysisFrames $videoPath $caseDir "theme-shadow-dense-frames" $targetFps)
    if ($framePaths.Count -lt 2) {
        return [ordered]@{
            Generated = $false
            Stable = $false
            Reason = "Fewer than two dense ThemeShadow frames were decoded."
            FrameCount = $framePaths.Count
            CardDeltaThreshold = $cardDeltaThreshold
            CardEdgeShiftThreshold = $cardEdgeShiftThreshold
        }
    }
    if ($framePaths.Count -gt $maxAnalyzedFrames) {
        $stride = [int][Math]::Ceiling([double]$framePaths.Count / [double]$maxAnalyzedFrames)
        $sampledFramePaths = New-Object System.Collections.Generic.List[string]
        for ($i = 0; $i -lt $framePaths.Count; $i += $stride) {
            $sampledFramePaths.Add($framePaths[$i])
        }
        if ($sampledFramePaths[$sampledFramePaths.Count - 1] -ne $framePaths[$framePaths.Count - 1]) {
            $sampledFramePaths.Add($framePaths[$framePaths.Count - 1])
        }
        $framePaths = @($sampledFramePaths)
    }

    $baseline = [System.Drawing.Bitmap]::FromFile($framePaths[0])
    try {
        $region = ConvertTo-FrameRectangle $bounds $captureRect $baseline.Width $baseline.Height 0
        if ($null -eq $region) {
            return [ordered]@{
                Generated = $false
                Stable = $false
                Reason = "ThemeShadow caster bounds could not be converted into frame coordinates."
                FrameCount = $framePaths.Count
                CardDeltaThreshold = $cardDeltaThreshold
                CardEdgeShiftThreshold = $cardEdgeShiftThreshold
            }
        }

        $baselineCardBounds = Get-ThemeShadowCardEdgeBounds $baseline $region
        if ($null -eq $baselineCardBounds) {
            return [ordered]@{
                Generated = $true
                Stable = $false
                Reason = "ThemeShadow card edges could not be detected in the baseline dense frame."
                FrameCount = $framePaths.Count
                FrameRate = $targetFps
                Bounds = $interactionResult.ThemeShadowCasterBeforeBounds
                FrameRegion = (Format-FrameRectangle $region)
                BaselineFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[0])
                CardDeltaThreshold = $cardDeltaThreshold
                CardEdgeShiftThreshold = $cardEdgeShiftThreshold
            }
        }

        $maxCardMeanDelta = 0.0
        $maxCardEdgeShift = 0.0
        $worstFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[0])
        $worstEdgeFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[0])
        $cardEdgesDetected = $true

        for ($i = 1; $i -lt $framePaths.Count; $i++) {
            $current = [System.Drawing.Bitmap]::FromFile($framePaths[$i])
            try {
                $match = Compare-ImageRegionMeanDeltaSampled $baseline $current $region $sampleStep
                if ($null -eq $match) {
                    continue
                }

                $meanDelta = [double]$match.MeanDelta
                if ($meanDelta -gt $maxCardMeanDelta) {
                    $maxCardMeanDelta = $meanDelta
                    $worstFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[$i])
                }

                $currentCardBounds = Get-ThemeShadowCardEdgeBounds $current $region
                $edgeShift = Get-FrameRectangleMaxDelta $baselineCardBounds $currentCardBounds
                if ($null -eq $edgeShift) {
                    $cardEdgesDetected = $false
                    $worstEdgeFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[$i])
                    continue
                }

                if ([double]$edgeShift -gt $maxCardEdgeShift) {
                    $maxCardEdgeShift = [double]$edgeShift
                    $worstEdgeFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[$i])
                }
            }
            finally {
                $current.Dispose()
            }
        }

        $cardEdgeStable = $cardEdgesDetected -and ([double]$maxCardEdgeShift -le $cardEdgeShiftThreshold)

        return [ordered]@{
            Generated = $true
            Stable = (([double]$maxCardMeanDelta -le $cardDeltaThreshold) -and $cardEdgeStable)
            FrameCount = $framePaths.Count
            FrameRate = $targetFps
            Bounds = $interactionResult.ThemeShadowCasterBeforeBounds
            FrameRegion = (Format-FrameRectangle $region)
            BaselineCardEdgeBounds = (Format-FrameRectangle $baselineCardBounds)
            BaselineFrame = [IO.Path]::GetFileNameWithoutExtension($framePaths[0])
            WorstFrame = $worstFrame
            WorstEdgeFrame = $worstEdgeFrame
            MaxCardMeanDelta = [Math]::Round($maxCardMeanDelta, 3)
            CardDeltaThreshold = $cardDeltaThreshold
            MaxCardEdgeShift = [Math]::Round($maxCardEdgeShift, 3)
            CardEdgeShiftThreshold = $cardEdgeShiftThreshold
            CardEdgeStable = $cardEdgeStable
            CardEdgesDetected = $cardEdgesDetected
            SampleStep = $sampleStep
        }
    }
    finally {
        $baseline.Dispose()
    }
}

function Get-MaxFrameDelta($frames) {
    $paths = @($frames | Where-Object { Test-FrameExtracted $_ } | ForEach-Object { $_.Path })
    if ($paths.Count -lt 2) {
        return $null
    }

    $samplesByPath = @{}
    foreach ($path in $paths) {
        $samples = Get-ImageLuminanceSamples $path $null 220
        if ($null -ne $samples) {
            $samplesByPath[$path] = $samples
        }
    }

    if ($samplesByPath.Count -lt 2) {
        return $null
    }

    $sampledPaths = @($paths | Where-Object { $samplesByPath.ContainsKey($_) })
    if ($sampledPaths.Count -lt 2) {
        return $null
    }

    $deltas = New-Object System.Collections.Generic.List[double]
    $baselinePath = $sampledPaths[0]
    for ($i = 1; $i -lt $sampledPaths.Count; $i++) {
        $path = $sampledPaths[$i]

        $baselineDelta = Compare-LuminanceSamples $samplesByPath[$baselinePath] $samplesByPath[$path]
        if ($null -ne $baselineDelta) {
            $deltas.Add([double]$baselineDelta)
        }

        $previousPath = $sampledPaths[$i - 1]
        $previousDelta = Compare-LuminanceSamples $samplesByPath[$previousPath] $samplesByPath[$path]
        if ($null -ne $previousDelta) {
            $deltas.Add([double]$previousDelta)
        }
    }

    if ($deltas.Count -eq 0) {
        return $null
    }

    return [Math]::Round(($deltas | Measure-Object -Maximum).Maximum, 3)
}

function Add-InteractionBoundsEntries($value, [string]$path, $entries) {
    if ($null -eq $value) {
        return
    }

    if ($value -is [System.Collections.IDictionary]) {
        foreach ($key in $value.Keys) {
            $keyText = [string]$key
            $childPath = if ([string]::IsNullOrWhiteSpace($path)) { $keyText } else { "$path.$keyText" }
            $childValue = $value[$key]
            if (($keyText -eq "Bounds" -or $keyText.EndsWith("Bounds", [StringComparison]::Ordinal)) -and
                $childValue -is [string] -and
                $null -ne (ConvertFrom-BoundingRectangleString $childValue)) {
                $entries.Add([ordered]@{
                    Name = $childPath
                    Bounds = $childValue
                })
            }

            Add-InteractionBoundsEntries $childValue $childPath $entries
        }

        return
    }

    if ($value -is [pscustomobject]) {
        foreach ($property in $value.PSObject.Properties) {
            $childPath = if ([string]::IsNullOrWhiteSpace($path)) { $property.Name } else { "$path.$($property.Name)" }
            if (($property.Name -eq "Bounds" -or $property.Name.EndsWith("Bounds", [StringComparison]::Ordinal)) -and
                $property.Value -is [string] -and
                $null -ne (ConvertFrom-BoundingRectangleString $property.Value)) {
                $entries.Add([ordered]@{
                    Name = $childPath
                    Bounds = $property.Value
                })
            }

            Add-InteractionBoundsEntries $property.Value $childPath $entries
        }

        return
    }

    if ($value -is [System.Collections.IEnumerable] -and !($value -is [string])) {
        $index = 0
        foreach ($item in $value) {
            $childPath = if ([string]::IsNullOrWhiteSpace($path)) { "[{0}]" -f $index } else { "{0}[{1}]" -f $path, $index }
            Add-InteractionBoundsEntries $item $childPath $entries
            $index++
        }
    }
}

function Get-InteractionBoundsEntries($interactionResult) {
    $entries = New-Object System.Collections.Generic.List[object]
    Add-InteractionBoundsEntries $interactionResult "" $entries

    $seen = @{}
    $unique = New-Object System.Collections.Generic.List[object]
    foreach ($entry in $entries) {
        $key = "{0}|{1}" -f $entry.Name, $entry.Bounds
        if (!$seen.ContainsKey($key)) {
            $seen[$key] = $true
            $unique.Add($entry)
        }
    }

    return $unique.ToArray()
}

function Test-FrameExtracted($frame) {
    if ($null -eq $frame) {
        return $false
    }

    if ($frame -is [System.Collections.IDictionary]) {
        return $frame.Contains("Extracted") -and [bool]$frame.Extracted
    }

    return $frame.PSObject.Properties.Match("Extracted").Count -gt 0 -and [bool]$frame.Extracted
}

function Get-LocalFrameDeltas($frames, $recordingResult, $interactionResult) {
    $paths = @($frames | Where-Object { Test-FrameExtracted $_ } | Sort-Object Name | ForEach-Object { $_.Path })
    if ($paths.Count -lt 2 -or $null -eq $recordingResult -or [string]::IsNullOrWhiteSpace($recordingResult.Rect)) {
        return @()
    }

    $boundsEntries = @(Get-InteractionBoundsEntries $interactionResult)
    if ($boundsEntries.Count -eq 0) {
        return @()
    }

    $localDeltas = New-Object System.Collections.Generic.List[object]
    foreach ($entry in $boundsEntries) {
        $samplesByPath = @{}
        foreach ($path in $paths) {
            $samples = Get-ImageRegionLuminanceSamples $path $entry.Bounds $recordingResult.Rect
            if ($null -ne $samples) {
                $samplesByPath[$path] = $samples
            }
        }

        if ($samplesByPath.Count -lt 2) {
            continue
        }

        $sampledPaths = @($paths | Where-Object { $samplesByPath.ContainsKey($_) })
        if ($sampledPaths.Count -lt 2) {
            continue
        }

        $deltas = New-Object System.Collections.Generic.List[double]
        $baselinePath = $sampledPaths[0]
        for ($i = 1; $i -lt $sampledPaths.Count; $i++) {
            $path = $sampledPaths[$i]

            $baselineDelta = Compare-LuminanceSamples $samplesByPath[$baselinePath] $samplesByPath[$path]
            if ($null -ne $baselineDelta) {
                $deltas.Add([double]$baselineDelta)
            }

            $previousPath = $sampledPaths[$i - 1]
            $previousDelta = Compare-LuminanceSamples $samplesByPath[$previousPath] $samplesByPath[$path]
            if ($null -ne $previousDelta) {
                $deltas.Add([double]$previousDelta)
            }
        }

        if ($deltas.Count -gt 0) {
            $localDeltas.Add([ordered]@{
                Name = $entry.Name
                Bounds = $entry.Bounds
                MaxDelta = [Math]::Round(($deltas | Measure-Object -Maximum).Maximum, 3)
            })
        }
    }

    return $localDeltas.ToArray()
}

function Get-MaxLocalFrameDelta($localFrameDeltas) {
    $deltas = New-Object System.Collections.Generic.List[double]
    foreach ($entry in @($localFrameDeltas)) {
        if ($null -eq $entry) {
            continue
        }

        $maxDelta = $null
        if ($entry -is [System.Collections.IDictionary] -and $entry.Contains("MaxDelta")) {
            $maxDelta = $entry["MaxDelta"]
        }
        else {
            $maxDeltaProperty = $entry.PSObject.Properties["MaxDelta"]
            if ($null -ne $maxDeltaProperty) {
                $maxDelta = $maxDeltaProperty.Value
            }
        }

        if ($null -eq $maxDelta) {
            continue
        }

        $deltas.Add([double]$maxDelta)
    }

    if ($deltas.Count -eq 0) {
        return $null
    }

    return [Math]::Round(($deltas | Measure-Object -Maximum).Maximum, 3)
}

function Get-TextVisualClosedEvidence($frames, $recordingResult, $interactionResult) {
    if ($null -eq $interactionResult -or
        !$interactionResult.Contains("InitialSuggestionBounds") -or
        [string]::IsNullOrWhiteSpace($interactionResult.InitialSuggestionBounds)) {
        return [ordered]@{
            Generated = $false
            Reason = "Initial suggestion bounds were not recorded."
        }
    }

    $extractedFrames = @($frames | Where-Object { Test-FrameExtracted $_ } | Sort-Object Name)
    if ($extractedFrames.Count -lt 2 -or $null -eq $recordingResult -or [string]::IsNullOrWhiteSpace($recordingResult.Rect)) {
        return [ordered]@{
            Generated = $false
            Reason = "Not enough extracted frames were available."
        }
    }

    $baselineFrame = $extractedFrames[0]
    $finalFrame = $extractedFrames[$extractedFrames.Count - 1]
    $baselineSamples = Get-ImageRegionLuminanceSamples $baselineFrame.Path $interactionResult.InitialSuggestionBounds $recordingResult.Rect
    $finalSamples = Get-ImageRegionLuminanceSamples $finalFrame.Path $interactionResult.InitialSuggestionBounds $recordingResult.Rect
    $delta = Compare-LuminanceSamples $baselineSamples $finalSamples
    if ($null -eq $delta) {
        return [ordered]@{
            Generated = $false
            Reason = "Could not sample the initial suggestion bounds."
        }
    }

    $roundedDelta = [Math]::Round([double]$delta, 3)
    return [ordered]@{
        Generated = $true
        Bounds = $interactionResult.InitialSuggestionBounds
        BaselineFrame = $baselineFrame.Name
        FinalFrame = $finalFrame.Name
        FinalDelta = $roundedDelta
        Closed = $roundedDelta -le 4.0
    }
}

function Get-FrameSeconds($frame) {
    if ($null -eq $frame -or [string]::IsNullOrWhiteSpace($frame.Name)) {
        return $null
    }

    if ($frame.Name -match '^t(\d+)$') {
        return [double]::Parse($Matches[1], [Globalization.CultureInfo]::InvariantCulture) / 1000.0
    }

    return $null
}

function Get-ClosestExtractedFrame($frames, $seconds) {
    if ($null -eq $seconds) {
        return $null
    }

    $bestFrame = $null
    $bestDistance = [double]::PositiveInfinity
    foreach ($frame in @($frames | Where-Object { Test-FrameExtracted $_ })) {
        $frameSeconds = Get-FrameSeconds $frame
        if ($null -eq $frameSeconds) {
            continue
        }

        $distance = [Math]::Abs([double]$frameSeconds - [double]$seconds)
        if ($distance -lt $bestDistance) {
            $bestFrame = $frame
            $bestDistance = $distance
        }
    }

    return $bestFrame
}

function Get-ExtractedFramesInRange($frames, $startSeconds, $endSeconds) {
    $selected = New-Object System.Collections.Generic.List[object]
    foreach ($frame in @($frames | Where-Object { Test-FrameExtracted $_ } | Sort-Object Name)) {
        $frameSeconds = Get-FrameSeconds $frame
        if ($null -eq $frameSeconds) {
            continue
        }

        if ($null -ne $startSeconds -and [double]$frameSeconds -lt [double]$startSeconds) {
            continue
        }

        if ($null -ne $endSeconds -and [double]$frameSeconds -gt [double]$endSeconds) {
            continue
        }

        $selected.Add($frame)
    }

    return $selected.ToArray()
}

function Get-CachedOpenRepeatRegionSamples([string]$path, [string]$bounds, [string]$captureRect, $samplesByPath) {
    $key = "{0}|{1}|{2}" -f $path, $bounds, $captureRect
    if ($samplesByPath.ContainsKey($key)) {
        return $samplesByPath[$key]
    }

    $samples = Get-ImageRegionLuminanceSamples $path $bounds $captureRect
    $samplesByPath[$key] = $samples
    return $samples
}

function Get-OpenRepeatBaselineDeltaEntries($frames, $baselineFrame, $recordingResult, [string]$bounds, $startSeconds, $endSeconds, $samplesByPath) {
    if ($null -eq $baselineFrame) {
        return @()
    }

    $baselineSamples = Get-CachedOpenRepeatRegionSamples $baselineFrame.Path $bounds $recordingResult.Rect $samplesByPath
    if ($null -eq $baselineSamples) {
        return @()
    }

    $entries = New-Object System.Collections.Generic.List[object]
    foreach ($frame in @(Get-ExtractedFramesInRange $frames $startSeconds $endSeconds)) {
        $samples = Get-CachedOpenRepeatRegionSamples $frame.Path $bounds $recordingResult.Rect $samplesByPath
        if ($null -eq $samples) {
            continue
        }

        $delta = Compare-LuminanceSamples $baselineSamples $samples
        if ($null -eq $delta) {
            continue
        }

        $entries.Add([pscustomobject]@{
            Frame = $frame
            Seconds = Get-FrameSeconds $frame
            Delta = [double]$delta
        })
    }

    return $entries.ToArray()
}

function Select-OpenRepeatDeltaEntry($entries, [string]$mode) {
    $bestEntry = $null
    foreach ($entry in @($entries)) {
        if ($null -eq $entry) {
            continue
        }

        if ($null -eq $bestEntry) {
            $bestEntry = $entry
            continue
        }

        if (($mode -eq "Max" -and [double]$entry.Delta -gt [double]$bestEntry.Delta) -or
            ($mode -eq "Min" -and [double]$entry.Delta -lt [double]$bestEntry.Delta)) {
            $bestEntry = $entry
        }
    }

    return $bestEntry
}

function Get-OpenRepeatDirectFrameEvidence(
    $frames,
    $recordingResult,
    $interactionResult,
    [string]$bounds,
    [double]$openThreshold,
    [double]$closedThreshold,
    $samplesByPath) {
    $initialFrame = Get-ClosestExtractedFrame $frames $interactionResult.InitialVisualSeconds
    $firstFrame = Get-ClosestExtractedFrame $frames $interactionResult.FirstOpenVisualSeconds
    $closedFrame = Get-ClosestExtractedFrame $frames $interactionResult.ClosedVisualSeconds
    $secondFrame = Get-ClosestExtractedFrame $frames $interactionResult.SecondOpenVisualSeconds

    if ($null -eq $initialFrame -or
        $null -eq $firstFrame -or
        $null -eq $closedFrame -or
        $null -eq $secondFrame) {
        return $null
    }

    if ($firstFrame.Path -eq $closedFrame.Path -or
        $secondFrame.Path -eq $closedFrame.Path -or
        $firstFrame.Path -eq $secondFrame.Path) {
        return $null
    }

    $initialSamples = Get-CachedOpenRepeatRegionSamples $initialFrame.Path $bounds $recordingResult.Rect $samplesByPath
    $firstSamples = Get-CachedOpenRepeatRegionSamples $firstFrame.Path $bounds $recordingResult.Rect $samplesByPath
    $closedSamples = Get-CachedOpenRepeatRegionSamples $closedFrame.Path $bounds $recordingResult.Rect $samplesByPath
    $secondSamples = Get-CachedOpenRepeatRegionSamples $secondFrame.Path $bounds $recordingResult.Rect $samplesByPath
    $initialClosedDelta = Compare-LuminanceSamples $initialSamples $closedSamples
    $firstOpenDelta = Compare-LuminanceSamples $closedSamples $firstSamples
    $secondOpenDelta = Compare-LuminanceSamples $closedSamples $secondSamples

    if ($null -eq $initialClosedDelta -or
        $null -eq $firstOpenDelta -or
        $null -eq $secondOpenDelta) {
        return $null
    }

    $firstOpenEvidence = [double]$firstOpenDelta -ge $openThreshold
    $closeVisualClosed = $interactionResult.Contains("CloseVisualClosed") -and [bool]$interactionResult.CloseVisualClosed
    $closedEvidence = ([double]$initialClosedDelta -le $closedThreshold) -or $closeVisualClosed
    $secondOpenEvidence = [double]$secondOpenDelta -ge $openThreshold
    if (!$firstOpenEvidence -or !$closedEvidence -or !$secondOpenEvidence) {
        return $null
    }

    return [ordered]@{
        Generated = $true
        Bounds = $bounds
        InitialFrame = $initialFrame.Name
        FirstFrame = $firstFrame.Name
        ClosedFrame = $closedFrame.Name
        SecondFrame = $secondFrame.Name
        InitialVisualSeconds = $interactionResult.InitialVisualSeconds
        FirstOpenVisualSeconds = $interactionResult.FirstOpenVisualSeconds
        ClosedVisualSeconds = $interactionResult.ClosedVisualSeconds
        SecondOpenVisualSeconds = $interactionResult.SecondOpenVisualSeconds
        FirstOpenDelta = [Math]::Round([double]$firstOpenDelta, 3)
        ClosedDelta = [Math]::Round([double]$initialClosedDelta, 3)
        SecondOpenDelta = [Math]::Round([double]$secondOpenDelta, 3)
        OpenThreshold = $openThreshold
        ClosedThreshold = $closedThreshold
        FirstOpenEvidence = $true
        ClosedEvidence = $true
        SecondOpenEvidence = $true
        FirstOpenClosedDelta = [Math]::Round([double]$firstOpenDelta, 3)
        InitialClosedDelta = [Math]::Round([double]$initialClosedDelta, 3)
        SecondOpenClosedDelta = [Math]::Round([double]$secondOpenDelta, 3)
        Detection = "DirectEventFrameClosedBaselineScan"
    }
}

function Get-OpenRepeatOpenThreshold([string]$control) {
    if ($control -eq "CommandBar") {
        return 3.0
    }

    if ($control -eq "CommandBarFlyout") {
        return 2.0
    }

    return 5.0
}

function Get-OpenRepeatClosedThreshold([string]$control) {
    if ($control -eq "DatePicker") {
        return 1.2
    }

    return 1.0
}

function Get-OpenRepeatVisualEvidence($frames, $recordingResult, $interactionResult, [string]$control = "") {
    if ($null -eq $interactionResult -or
        $null -eq $recordingResult -or
        [string]::IsNullOrWhiteSpace($recordingResult.Rect) -or
        !$interactionResult.Contains("InitialVisualSeconds") -or
        !$interactionResult.Contains("FirstOpenVisualSeconds") -or
        !$interactionResult.Contains("ClosedVisualSeconds") -or
        !$interactionResult.Contains("SecondOpenVisualSeconds") -or
        !$interactionResult.Contains("FirstOpenStartSeconds") -or
        !$interactionResult.Contains("SecondOpenStartSeconds") -or
        !$interactionResult.Contains("FirstOpenElementBounds") -or
        [string]::IsNullOrWhiteSpace($interactionResult.FirstOpenElementBounds)) {
        return [ordered]@{ Generated = $false }
    }

    $bounds = $interactionResult.FirstOpenElementBounds
    $samplesByPath = @{}
    $openThreshold = Get-OpenRepeatOpenThreshold $control
    $closedThreshold = Get-OpenRepeatClosedThreshold $control
    $initialFrame = Get-ClosestExtractedFrame $frames $interactionResult.InitialVisualSeconds
    if ($null -eq $initialFrame) {
        $initialFrame = @($frames | Where-Object { Test-FrameExtracted $_ } | Sort-Object Name | Select-Object -First 1)[0]
    }

    $baselineSeconds = Get-FrameSeconds $initialFrame
    $firstOpenEntry = $null
    $closedEntry = $null
    $secondOpenEntry = $null

    $firstOpenEndSeconds = if ($null -ne $interactionResult.ClosedVisualSeconds) { $interactionResult.ClosedVisualSeconds } else { $null }
    $closedEndSeconds = if ($null -ne $interactionResult.SecondOpenStartSeconds) { $interactionResult.SecondOpenStartSeconds } else { $null }

    $firstOpenEntries = Get-OpenRepeatBaselineDeltaEntries `
        $frames `
        $initialFrame `
        $recordingResult `
        $bounds `
        $interactionResult.FirstOpenStartSeconds `
        $firstOpenEndSeconds `
        $samplesByPath
    foreach ($entry in @($firstOpenEntries | Sort-Object Seconds)) {
        if ($null -eq $baselineSeconds -or [double]$entry.Seconds -le ([double]$baselineSeconds + 0.5)) {
            continue
        }

        if ([double]$entry.Delta -ge $openThreshold) {
            $firstOpenEntry = $entry
            break
        }
    }

    if ($null -ne $firstOpenEntry) {
        $closedEntries = Get-OpenRepeatBaselineDeltaEntries `
            $frames `
            $initialFrame `
            $recordingResult `
            $bounds `
            ([double]$firstOpenEntry.Seconds + 0.5) `
            $closedEndSeconds `
            $samplesByPath
        foreach ($entry in @($closedEntries | Sort-Object Seconds)) {
            if ([double]$entry.Seconds -le ([double]$firstOpenEntry.Seconds + 0.5)) {
                continue
            }

            if ([double]$entry.Delta -le $closedThreshold) {
                $closedEntry = $entry
                break
            }
        }
    }

    if ($null -ne $closedEntry) {
        $secondOpenEntries = Get-OpenRepeatBaselineDeltaEntries `
            $frames `
            $initialFrame `
            $recordingResult `
            $bounds `
            $interactionResult.SecondOpenStartSeconds `
            $null `
            $samplesByPath
        foreach ($entry in @($secondOpenEntries | Sort-Object Seconds)) {
            if ([double]$entry.Seconds -le ([double]$closedEntry.Seconds + 0.5)) {
                continue
            }

            if ([double]$entry.Delta -ge $openThreshold) {
                $secondOpenEntry = $entry
                break
            }
        }
    }

    if ($null -eq $initialFrame -or $null -eq $firstOpenEntry -or $null -eq $closedEntry -or $null -eq $secondOpenEntry) {
        $directEvidence = Get-OpenRepeatDirectFrameEvidence `
            $frames `
            $recordingResult `
            $interactionResult `
            $bounds `
            $openThreshold `
            $closedThreshold `
            $samplesByPath
        if ($null -ne $directEvidence) {
            return $directEvidence
        }

        return [ordered]@{
            Generated = $false
            Bounds = $bounds
            InitialFrame = if ($null -ne $initialFrame) { $initialFrame.Name } else { "" }
            Reason = "Open-repeat transition scan did not find first-open, closed, and second-open frames."
        }
    }

    if ($firstOpenEntry.Frame.Path -eq $closedEntry.Frame.Path -or
        $secondOpenEntry.Frame.Path -eq $closedEntry.Frame.Path -or
        $firstOpenEntry.Frame.Path -eq $secondOpenEntry.Frame.Path) {
        return [ordered]@{
            Generated = $false
            Bounds = $bounds
            InitialFrame = $initialFrame.Name
            FirstFrame = $firstOpenEntry.Frame.Name
            ClosedFrame = $closedEntry.Frame.Name
            SecondFrame = $secondOpenEntry.Frame.Name
            Reason = "Open and closed visual samples resolved to the same frame."
        }
    }

    return [ordered]@{
        Generated = $true
        Bounds = $bounds
        InitialFrame = $initialFrame.Name
        FirstFrame = $firstOpenEntry.Frame.Name
        ClosedFrame = $closedEntry.Frame.Name
        SecondFrame = $secondOpenEntry.Frame.Name
        InitialVisualSeconds = $interactionResult.InitialVisualSeconds
        FirstOpenVisualSeconds = $interactionResult.FirstOpenVisualSeconds
        ClosedVisualSeconds = $interactionResult.ClosedVisualSeconds
        SecondOpenVisualSeconds = $interactionResult.SecondOpenVisualSeconds
        FirstOpenDelta = [Math]::Round([double]$firstOpenEntry.Delta, 3)
        ClosedDelta = [Math]::Round([double]$closedEntry.Delta, 3)
        SecondOpenDelta = [Math]::Round([double]$secondOpenEntry.Delta, 3)
        OpenThreshold = $openThreshold
        ClosedThreshold = $closedThreshold
        FirstOpenEvidence = [double]$firstOpenEntry.Delta -ge $openThreshold
        ClosedEvidence = [double]$closedEntry.Delta -le $closedThreshold
        SecondOpenEvidence = [double]$secondOpenEntry.Delta -ge $openThreshold
        Detection = "BaselineDeltaEventWindowScan"
    }
}

function Test-OpenRepeatVisualEvidence($visualEvidence) {
    if ($null -eq $visualEvidence -or !$visualEvidence.Contains("Generated") -or !$visualEvidence.Generated) {
        return $false
    }

    return [bool]$visualEvidence.FirstOpenEvidence -and
        [bool]$visualEvidence.ClosedEvidence -and
        [bool]$visualEvidence.SecondOpenEvidence
}

function Get-EarlyFrameDelta($frames) {
    $paths = @(
        $frames |
            Where-Object { Test-FrameExtracted $_ } |
            Sort-Object Name |
            Select-Object -First 4 |
            ForEach-Object { $_.Path }
    )
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

function Test-AnimationEvidence([string]$control, $earlyFrameDelta) {
    if (!(Test-ControlRequiresAnimatedVisualProof $control) -or $null -eq $earlyFrameDelta) {
        return $false
    }

    return [double]$earlyFrameDelta -ge 0.02
}

function Get-NonBlankFrameCount($frames) {
    $count = 0
    foreach ($frame in $frames) {
        if ((Test-FrameExtracted $frame) -and $null -ne $frame.Stats -and $frame.Stats.NonBlank) {
            $count++
        }
    }

    return $count
}

function Get-ExtractedFrameCount($frames) {
    $count = 0
    foreach ($frame in $frames) {
        if (Test-FrameExtracted $frame) {
            $count++
        }
    }

    return $count
}

function Get-MinimumNonBlankFrameCount([int]$extractedFrameCount) {
    if ($extractedFrameCount -le 0) {
        return 0
    }

    if ($extractedFrameCount -le 3) {
        return $extractedFrameCount
    }

    return [Math]::Max(2, [int][Math]::Ceiling($extractedFrameCount * 0.75))
}

function Get-RenderedPageArtifactAnchor([string]$artifactDir) {
    $candidates = @(
        @{ FileName = "ContentPagePane.png"; Source = "ContentPagePaneRenderedArtifact" },
        @{ FileName = "GalleryItemPageRoot.png"; Source = "GalleryItemPageRootRenderedArtifact" }
    )

    foreach ($candidate in $candidates) {
        $path = Join-Path $artifactDir $candidate.FileName
        if (!(Test-Path -LiteralPath $path)) {
            continue
        }

        $stats = Get-ImageStats $path
        if ($null -ne $stats -and $stats.NonBlank) {
            return [ordered]@{
                FileName = $candidate.FileName
                Source = $candidate.Source
                Path = (Resolve-Path -LiteralPath $path).Path
                Stats = $stats
            }
        }
    }

    return $null
}

function Get-FirstNonBlankExtractedFrame($frames) {
    foreach ($frame in @($frames | Where-Object { Test-FrameExtracted $_ } | Sort-Object Name)) {
        if ($null -ne $frame.Stats -and $frame.Stats.NonBlank) {
            return $frame
        }
    }

    return $null
}

function Get-ScreenRecordingGalleryAnchor($frames, $recordingResult, [string]$windowBounds, [string]$artifactDir) {
    $threshold = 25.0
    if ($null -eq $recordingResult -or [string]::IsNullOrWhiteSpace($recordingResult.Rect)) {
        return [ordered]@{
            Generated = $false
            Reason = "Recorder did not report a capture rectangle."
            Threshold = $threshold
        }
    }

    if ([string]::IsNullOrWhiteSpace($windowBounds)) {
        return [ordered]@{
            Generated = $false
            Reason = "Gallery window bounds were not recorded."
            Threshold = $threshold
        }
    }

    $anchorPath = Join-Path $artifactDir "ModernWpfGalleryMainWindow.png"
    if (!(Test-Path -LiteralPath $anchorPath)) {
        return [ordered]@{
            Generated = $false
            Reason = "ModernWpfGalleryMainWindow rendered artifact was not produced."
            Threshold = $threshold
        }
    }

    $frame = Get-FirstNonBlankExtractedFrame $frames
    if ($null -eq $frame) {
        return [ordered]@{
            Generated = $false
            Reason = "No nonblank extracted screen frame was available."
            Anchor = (Resolve-Path -LiteralPath $anchorPath).Path
            Threshold = $threshold
        }
    }

    $delta = Compare-FrameWindowRegionToAnchorMeanDelta $frame.Path $anchorPath $windowBounds $recordingResult.Rect
    if ($null -eq $delta) {
        return [ordered]@{
            Generated = $false
            Reason = "Could not compare the screen frame window region with the rendered Gallery anchor."
            Frame = $frame.Name
            FramePath = $frame.Path
            Anchor = (Resolve-Path -LiteralPath $anchorPath).Path
            WindowBounds = $windowBounds
            CaptureRect = $recordingResult.Rect
            Threshold = $threshold
        }
    }

    return [ordered]@{
        Generated = $true
        Frame = $frame.Name
        FramePath = $frame.Path
        Anchor = (Resolve-Path -LiteralPath $anchorPath).Path
        WindowBounds = $windowBounds
        CaptureRect = $recordingResult.Rect
        AnchorDelta = $delta
        Threshold = $threshold
        Matched = ([double]$delta -le $threshold)
    }
}

function Test-OpenRepeatEvidence($interactionResult) {
    if ($null -eq $interactionResult) {
        return $false
    }

    if (!$interactionResult.Contains("FirstOpenElementFound") -or
        !$interactionResult.Contains("SecondOpenElementFound") -or
        !$interactionResult.Contains("ClosedElementGone")) {
        return $false
    }

    $anchored = $true
    if ($interactionResult.Contains("FirstOpenElementAnchored") -and
        $interactionResult.Contains("SecondOpenElementAnchored")) {
        $anchored = $interactionResult.FirstOpenElementAnchored -and $interactionResult.SecondOpenElementAnchored
    }

    return $interactionResult.FirstOpenElementFound -and $interactionResult.SecondOpenElementFound -and $interactionResult.ClosedElementGone -and $anchored
}

function Test-StateEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("StateChanged")) {
        return $false
    }

    return [bool]$interactionResult.StateChanged
}

function Test-ExpansionEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("ExpansionChanged")) {
        return $false
    }

    return [bool]$interactionResult.ExpansionChanged
}

function Test-ValueEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("TargetReached")) {
        return $false
    }

    return [bool]$interactionResult.TargetReached
}

function Test-LayoutStabilityEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("LayoutStable")) {
        return $false
    }

    return [bool]$interactionResult.LayoutStable
}

function Test-SelectionEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("SelectionChanged")) {
        return $false
    }

    return [bool]$interactionResult.SelectionChanged
}

function Test-VisualSelectionEvidence([string]$control, [string]$interactionKind, $maxLocalFrameDelta) {
    if ($interactionKind -ne "Selection" -or $null -eq $maxLocalFrameDelta) {
        return $false
    }

    switch ($control) {
        "DataGrid" { return [double]$maxLocalFrameDelta -ge 10.0 }
        "SelectorBar" { return [double]$maxLocalFrameDelta -ge 0.05 }
        default { return $false }
    }
}

function Test-LocalVisualEvidence($maxLocalFrameDelta) {
    if ($null -eq $maxLocalFrameDelta) {
        return $false
    }

    return [double]$maxLocalFrameDelta -ge 0.05
}

function Get-LocalFrameDeltaByName($localFrameDeltas, [string]$name) {
    foreach ($entry in @($localFrameDeltas)) {
        if ($null -eq $entry) {
            continue
        }

        $entryName = $null
        $maxDelta = $null
        if ($entry -is [System.Collections.IDictionary]) {
            if ($entry.Contains("Name")) {
                $entryName = $entry["Name"]
            }
            if ($entry.Contains("MaxDelta")) {
                $maxDelta = $entry["MaxDelta"]
            }
        }
        else {
            $nameProperty = $entry.PSObject.Properties["Name"]
            if ($null -ne $nameProperty) {
                $entryName = $nameProperty.Value
            }

            $deltaProperty = $entry.PSObject.Properties["MaxDelta"]
            if ($null -ne $deltaProperty) {
                $maxDelta = $deltaProperty.Value
            }
        }

        if ($entryName -eq $name -and $null -ne $maxDelta) {
            return [double]$maxDelta
        }
    }

    return $null
}

function Test-ThemeShadowVisualEvidence($localFrameDeltas) {
    $delta = Get-LocalFrameDeltaByName $localFrameDeltas "ThemeShadowVisualBounds"
    return $null -ne $delta -and [double]$delta -ge 0.05
}

function Get-ThemeShadowArtifactEvidenceFromInteraction($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("ThemeShadowArtifactEvidence")) {
        return $null
    }

    return $interactionResult.ThemeShadowArtifactEvidence
}

function Test-ThemeShadowArtifactVisualEvidence($interactionResult) {
    $evidence = Get-ThemeShadowArtifactEvidenceFromInteraction $interactionResult
    if ($null -eq $evidence -or !$evidence.Contains("Generated") -or !$evidence.Contains("VisualChanged")) {
        return $false
    }

    if ($evidence.Contains("ShadowEnvelopeChanged")) {
        return [bool]$evidence.Generated -and [bool]$evidence.VisualChanged -and [bool]$evidence.ShadowEnvelopeChanged
    }

    return [bool]$evidence.Generated -and [bool]$evidence.VisualChanged
}

function Test-ThemeShadowArtifactCardStabilityEvidence($interactionResult) {
    $evidence = Get-ThemeShadowArtifactEvidenceFromInteraction $interactionResult
    if ($null -eq $evidence -or !$evidence.Contains("Generated") -or !$evidence.Contains("CardEdgesStable")) {
        return $false
    }

    return [bool]$evidence.Generated -and [bool]$evidence.CardEdgesStable
}

function Test-ThemeShadowCasterStabilityEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("ThemeShadowCasterStable")) {
        return $false
    }

    return [bool]$interactionResult.ThemeShadowCasterStable
}

function Test-ThemeShadowDenseFrameStabilityEvidence($themeShadowDenseFrameStability) {
    if ($null -eq $themeShadowDenseFrameStability) {
        return $false
    }

    if (!$themeShadowDenseFrameStability.Contains("Generated") -or !$themeShadowDenseFrameStability.Contains("Stable")) {
        return $false
    }

    return [bool]$themeShadowDenseFrameStability.Generated -and [bool]$themeShadowDenseFrameStability.Stable
}

function Test-InteractionRequiresLocalVisualEvidence([string]$interactionKind) {
    switch ($interactionKind) {
        "State" { return $true }
        "Expansion" { return $true }
        "Value" { return $true }
        "Selection" { return $true }
        "Option" { return $true }
        "Text" { return $true }
        "Output" { return $true }
        "Scroll" { return $true }
        "ShellNavigation" { return $true }
        "Breadcrumb" { return $true }
        "RouteNavigation" { return $true }
        default { return $false }
    }
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

    if ($interactionResult.Contains("ExpectedOutput") -and
        ![string]::IsNullOrWhiteSpace($interactionResult.ExpectedOutput) -and
        $interactionResult.Contains("OutputMatched") -and
        !$interactionResult.OutputMatched) {
        return $false
    }

    return [bool]$interactionResult.OutputChanged
}

function Test-TextEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("OutputMatched")) {
        return $false
    }

    if ($interactionResult.Contains("SuggestionClosed") -and !$interactionResult.SuggestionClosed) {
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

function Test-ShellNavigationEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("ShellNavigationChanged")) {
        return $false
    }

    return [bool]$interactionResult.ShellNavigationChanged
}

function Test-BreadcrumbEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("BreadcrumbChanged")) {
        return $false
    }

    return [bool]$interactionResult.BreadcrumbChanged
}

function Test-RouteNavigationEvidence($interactionResult) {
    if ($null -eq $interactionResult -or !$interactionResult.Contains("RouteNavigationChanged")) {
        return $false
    }

    return [bool]$interactionResult.RouteNavigationChanged
}

function Test-PreparedOpenEvidence($interactionResult) {
    if ($null -eq $interactionResult -or
        !$interactionResult.Contains("OpenElementFound") -or
        !$interactionResult.Contains("OpenElementAnchored")) {
        return $false
    }

    return [bool]$interactionResult.OpenElementFound -and [bool]$interactionResult.OpenElementAnchored
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
    $encoders = @($results | ForEach-Object {
            if ($null -ne $_.RecorderResult -and ![string]::IsNullOrWhiteSpace($_.RecorderResult.VideoEncoder)) {
                $_.RecorderResult.VideoEncoder
            }
        } | Sort-Object -Unique)
    if ($encoders.Count -gt 0) {
        $lines.Add(("Video encoder: ``{0}``" -f ($encoders -join ", ")))
    }
    $lines.Add(("Duration: ``{0}s`` default; ShellNavigation, AutoSuggestBox text, ToolTip, MenuBar, MessageBox, and fast popup open-repeat controls use larger maximum windows, but recording stops early after interaction evidence plus a short tail at ``{1}fps`` by default. Individual controls may raise the capture frame rate for dense visual stability checks." -f $DurationSeconds, $FrameRate))
    $lines.Add("")
    $lines.Add("| Control | Status | Interaction | Duration | Recording | Dense review | Max frame delta | Max local delta | Notes |")
    $lines.Add("| --- | --- | --- | ---: | --- | --- | ---: | ---: | --- |")
    foreach ($result in $results) {
        $recording = Format-RelativePath $result.Recording
        $duration = if ($null -ne $result.RecorderResult -and $null -ne $result.RecorderResult.DurationSeconds) {
            "{0:0.###}s/{1}s" -f [double]$result.RecorderResult.DurationSeconds, [int]$result.RecordingDurationSeconds
        }
        else {
            "/{0}s" -f [int]$result.RecordingDurationSeconds
        }
        $denseReview = if ($null -ne $result.DenseTransitionReview -and $result.DenseTransitionReview.Generated) {
            Format-RelativePath $result.DenseTransitionReview.Path
        }
        else {
            ""
        }
        $delta = if ($null -eq $result.MaxFrameDelta) { "" } else { $result.MaxFrameDelta.ToString([Globalization.CultureInfo]::InvariantCulture) }
        $localDelta = if ($null -eq $result.MaxLocalFrameDelta) { "" } else { $result.MaxLocalFrameDelta.ToString([Globalization.CultureInfo]::InvariantCulture) }
        $notes = ($result.Notes -replace "\|", "\|")
        $lines.Add(("| {0} | {1} | {2} | {3} | ``{4}`` | ``{5}`` | {6} | {7} | {8} |" -f $result.Control, $result.Status, $result.InteractionKind, $duration, $recording, $denseReview, $delta, $localDelta, $notes))
    }

    $reportPath = Join-Path $runDir "report.md"
    Set-Content -Path $reportPath -Value $lines -Encoding UTF8
    return $reportPath
}

function Write-RunCheckpoint([string]$runDir, $results) {
    $manifestPath = Join-Path $runDir "recording-manifest.json"
    $manifestTempPath = Join-Path $runDir "recording-manifest.json.tmp"
    $results | ConvertTo-Json -Depth 8 | Set-Content -Path $manifestTempPath -Encoding UTF8
    Move-Item -Path $manifestTempPath -Destination $manifestPath -Force

    $reportPath = Write-Report $runDir $results
    return [ordered]@{
        Manifest = $manifestPath
        Report = $reportPath
    }
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

    $route = Get-ControlRoute $control
    $process = $null
    $recordingJob = $null
    $notes = New-Object System.Collections.Generic.List[string]
    $interactionResult = $null
    $recordingResult = $null
    $frames = @()
    $denseTransitionReview = $null
    $localFrameDeltas = @()
    $maxLocalFrameDelta = $null
    $themeShadowDenseFrameStability = $null
    $screenRecordingGalleryAnchor = $null
    $localVisualEvidence = $false
    $visualOpenRepeatEvidence = $null
    $visualOpenRepeatEvidenceAccepted = $false
    $textVisualClosedEvidence = $null
    $status = "Passed"
    $renderedPageArtifactAnchor = $null
    $windowBounds = ""
    $recordingPath = Join-Path $caseDir ("{0}-{1}{2}" -f $Theme.ToLowerInvariant(), $control.ToLowerInvariant(), $extension)
    $interactionKind = Get-ControlInteractionKind $control
    $recordingDurationSeconds = Get-ControlRecordingDurationSeconds $control $interactionKind

    Write-Host ("Recording {0} ({1})..." -f $control, $interactionKind)

    try {
        $args = @("--visual-test", "--route", $route, "--theme", $Theme, "--visual-artifact-dir", $artifactDir)
        if (Test-ControlRequiresAnimatedVisualProof $control) {
            $args += "--preserve-animated-visuals"
        }
        if (Test-ControlRequiresDiagnosticPreparation $control) {
            $args += "--open-interactions"
        }

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
        $windowBounds = Format-NativeWindowRectangle ([GalleryRecordingNative]::GetRect([IntPtr]$window.Current.NativeWindowHandle))
        Wait-ModernWpfReady $window $route $artifactDir | Out-Null
        Start-Sleep -Milliseconds 700

        $sampleId = Get-RequiredSampleAutomationId $control
        $sampleElement = Find-DescendantByAutomationId $window $sampleId
        if ($null -eq $sampleElement) {
            if (Test-ControlSupportsRenderedPageArtifactAnchor $control) {
                $renderedPageArtifactAnchor = Get-RenderedPageArtifactAnchor $artifactDir
                if ($null -ne $renderedPageArtifactAnchor) {
                    $notes.Add(("Sample '{0}' not found; accepted nonblank {1}." -f $sampleId, $renderedPageArtifactAnchor.Source))
                }
                else {
                    $notes.Add("Sample '$sampleId' not found and no nonblank rendered page artifact was produced.")
                    $status = "Failed"
                }
            }
            else {
                $notes.Add("Sample '$sampleId' not found; recording still captured route.")
                $status = "Failed"
            }
        }

        $script:GalleryVisualSnapshotDirectory = Join-Path $caseDir "live-snapshots"
        $script:GalleryLiveFrameDirectory = Join-Path (Split-Path -Parent $recordingPath) (([IO.Path]::GetFileNameWithoutExtension($recordingPath)) + ".frames")
        $recordingFrameRate = Get-ControlRecordingFrameRate $control $interactionKind
        $recordingJob = Start-RecordingJob $window.Current.ProcessId ([IntPtr]$window.Current.NativeWindowHandle) $recordingPath $CaptureMode $recordingDurationSeconds $VideoEncoder ([bool]$BenchmarkEncoders) $recordingFrameRate
        $script:GalleryRecordingStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        [void](Wait-LiveRecordingWarmupFrames $recordingFrameRate 0.4 15)
        $interactionResult = Invoke-RecordedInteraction $window $control $sampleElement $artifactDir
        $process.Refresh()
        if ($process.HasExited) {
            throw "ModernWpf Gallery exited during $control interaction."
        }

        Start-Sleep -Milliseconds 350
        $recordingStopWallClockSeconds = Get-RecordingElapsedSeconds
        Request-RecordingStop $recordingJob
        Start-Sleep -Milliseconds 250
        Close-GalleryRecordingProcess $process
        $recordingResult = Wait-RecordingJob $recordingJob $recordingDurationSeconds
        $recordingJob = $null
        if ($null -eq $recordingResult -or !(Test-Path $recordingPath)) {
            throw "Recorder did not produce '$recordingPath'."
        }
        Normalize-InteractionResultVideoTimestamps $interactionResult $recordingResult $recordingStopWallClockSeconds

        $posterFrameIntervalSeconds = Get-PosterFrameIntervalSeconds $control $interactionKind
        $frames = Export-PosterFrames $recordingPath $caseDir $posterFrameIntervalSeconds
        if (Test-ControlRequiresDenseTransitionReview $control $interactionKind) {
            $denseTransitionReview = Export-DenseTransitionReviewSheet $recordingPath $caseDir $recordingDurationSeconds
            if ($null -eq $denseTransitionReview -or !$denseTransitionReview.Generated) {
                $status = "NeedsReview"
                $notes.Add("Dense transition review sheet was not generated.")
            }
            else {
                $notes.Add(("Dense transition review sheet generated at {0}." -f (Format-RelativePath $denseTransitionReview.Path)))
            }
        }

        $nonBlankFrameCount = Get-NonBlankFrameCount $frames
        $extractedFrameCount = Get-ExtractedFrameCount $frames
        $minimumNonBlankFrameCount = Get-MinimumNonBlankFrameCount $extractedFrameCount
        if ($nonBlankFrameCount -lt $minimumNonBlankFrameCount -and !$SkipFrameExtraction) {
            $status = "Failed"
            $notes.Add(("Only {0} of {1} extracted poster frames were nonblank; at least {2} are required." -f $nonBlankFrameCount, $extractedFrameCount, $minimumNonBlankFrameCount))
        }
        if ($CaptureMode -eq "Screen" -and !$SkipFrameExtraction) {
            $screenRecordingGalleryAnchor = Get-ScreenRecordingGalleryAnchor $frames $recordingResult $windowBounds $artifactDir
            if ($null -eq $screenRecordingGalleryAnchor -or
                !$screenRecordingGalleryAnchor.Contains("Generated") -or
                !$screenRecordingGalleryAnchor.Generated) {
                $status = "Failed"
                $reason = if ($null -ne $screenRecordingGalleryAnchor -and $screenRecordingGalleryAnchor.Contains("Reason")) { $screenRecordingGalleryAnchor.Reason } else { "Screen Gallery anchor evidence was not generated." }
                $notes.Add(("Screen capture did not prove the Gallery window was recorded. {0}" -f $reason))
            }
            elseif (!$screenRecordingGalleryAnchor.Matched) {
                $status = "Failed"
                $notes.Add(("Screen capture did not match the rendered Gallery window anchor. Frame={0}; delta={1}; threshold={2}; window={3}; capture={4}. This usually means screen mode captured a different desktop or monitor." -f $screenRecordingGalleryAnchor.Frame, $screenRecordingGalleryAnchor.AnchorDelta, $screenRecordingGalleryAnchor.Threshold, $screenRecordingGalleryAnchor.WindowBounds, $screenRecordingGalleryAnchor.CaptureRect))
            }
        }

        $localFrameDeltas = Get-LocalFrameDeltas $frames $recordingResult $interactionResult
        $maxLocalFrameDelta = Get-MaxLocalFrameDelta $localFrameDeltas
        $localVisualEvidence = Test-LocalVisualEvidence $maxLocalFrameDelta
        $themeShadowDenseFrameStability = if ($control -eq "ThemeShadow" -and $interactionKind -eq "Value") {
            Get-ThemeShadowDenseFrameStability $recordingPath $caseDir $recordingResult $interactionResult
        }
        else {
            $null
        }
        $visualOpenRepeatEvidence = if ($interactionKind -eq "OpenRepeat") {
            Get-OpenRepeatVisualEvidence $frames $recordingResult $interactionResult $control
        }
        else {
            $null
        }
        $visualOpenRepeatEvidenceAccepted = Test-OpenRepeatVisualEvidence $visualOpenRepeatEvidence
        $textVisualClosedEvidence = if ($interactionKind -eq "Text") {
            Get-TextVisualClosedEvidence $frames $recordingResult $interactionResult
        }
        else {
            $null
        }

        $openRepeatVisualEvidenceFailed =
            $interactionKind -eq "OpenRepeat" -and
            !$visualOpenRepeatEvidenceAccepted
        $openRepeatClosedFailed =
            $interactionKind -eq "OpenRepeat" -and
            $null -ne $interactionResult -and
            $interactionResult.Contains("ClosedElementGone") -and
            !$interactionResult.ClosedElementGone
        if ($openRepeatVisualEvidenceFailed) {
            $status = "Failed"
            if ($null -ne $visualOpenRepeatEvidence -and
                $visualOpenRepeatEvidence.Contains("Generated") -and
                $visualOpenRepeatEvidence.Generated) {
                $notes.Add(("Open-repeat frames did not prove first-open, closed, and second-open states. Frames={0}/{1}/{2}/{3}; deltas={4}/{5}/{6}." -f $visualOpenRepeatEvidence.InitialFrame, $visualOpenRepeatEvidence.FirstFrame, $visualOpenRepeatEvidence.ClosedFrame, $visualOpenRepeatEvidence.SecondFrame, $visualOpenRepeatEvidence.FirstOpenDelta, $visualOpenRepeatEvidence.ClosedDelta, $visualOpenRepeatEvidence.SecondOpenDelta))
            }
            else {
                $reason = if ($null -ne $visualOpenRepeatEvidence -and $visualOpenRepeatEvidence.Contains("Reason")) { $visualOpenRepeatEvidence.Reason } else { "Visual evidence was not generated." }
                $notes.Add(("Open-repeat frames did not prove both opens against the closed state. {0}" -f $reason))
            }
        }
        $openRepeatGeometryFailed =
            $interactionKind -eq "OpenRepeat" -and
            $null -ne $interactionResult -and
            $interactionResult.Contains("FirstOpenElementAnchored") -and
            $interactionResult.Contains("SecondOpenElementAnchored") -and
            (!$interactionResult.FirstOpenElementAnchored -or !$interactionResult.SecondOpenElementAnchored)
        if ($openRepeatClosedFailed) {
            $status = "Failed"
            $notes.Add(("Opened element did not disappear between first and second open. CloseMethod={0}; trigger={1}; first={2}; second={3}." -f $interactionResult.CloseMethod, $interactionResult.TriggerBounds, $interactionResult.FirstOpenElementBounds, $interactionResult.SecondOpenElementBounds))
        }
        if ($openRepeatGeometryFailed) {
            if ($control -eq "CommandBar" -and
                $interactionResult.FirstOpenElementAnchored -and
                !$interactionResult.SecondOpenElementFound -and
                $visualOpenRepeatEvidenceAccepted -and
                $null -ne $denseTransitionReview -and
                $denseTransitionReview.Generated) {
                $openRepeatGeometryFailed = $false
                $notes.Add(("CommandBar repeat-open visual evidence accepted from frames {0}/{1}/{2}/{3}: first delta {4}, closed delta {5}, second delta {6}." -f $visualOpenRepeatEvidence.InitialFrame, $visualOpenRepeatEvidence.FirstFrame, $visualOpenRepeatEvidence.ClosedFrame, $visualOpenRepeatEvidence.SecondFrame, $visualOpenRepeatEvidence.FirstOpenDelta, $visualOpenRepeatEvidence.ClosedDelta, $visualOpenRepeatEvidence.SecondOpenDelta))
            }
            else {
                $status = "Failed"
                $notes.Add(("Opened element was detached from trigger. Trigger={0}; first={1}; second={2}." -f $interactionResult.TriggerBounds, $interactionResult.FirstOpenElementBounds, $interactionResult.SecondOpenElementBounds))
            }
        }

        if ($null -ne $interactionResult -and $interactionResult.Contains("Invoked") -and !$interactionResult.Invoked -and !$openRepeatClosedFailed -and !$openRepeatGeometryFailed -and !$visualOpenRepeatEvidenceAccepted) {
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
            Request-RecordingStop $recordingJob
            $jobToStop = if ($recordingJob -is [System.Collections.IDictionary] -and $recordingJob.Contains("Job")) { $recordingJob.Job } else { $recordingJob }
            Stop-Job -Job $jobToStop -ErrorAction SilentlyContinue
            Remove-Job -Job $jobToStop -Force -ErrorAction SilentlyContinue
            $recordingJob = $null
        }
    }
    finally {
        Close-GalleryRecordingProcess $process
    }

    $maxFrameDelta = Get-MaxFrameDelta $frames
    if ($null -eq $maxLocalFrameDelta) {
        $localFrameDeltas = Get-LocalFrameDeltas $frames $recordingResult $interactionResult
        $maxLocalFrameDelta = Get-MaxLocalFrameDelta $localFrameDeltas
        $localVisualEvidence = Test-LocalVisualEvidence $maxLocalFrameDelta
    }
    if ($null -eq $visualOpenRepeatEvidence -and $interactionKind -eq "OpenRepeat") {
        $visualOpenRepeatEvidence = Get-OpenRepeatVisualEvidence $frames $recordingResult $interactionResult $control
        $visualOpenRepeatEvidenceAccepted = Test-OpenRepeatVisualEvidence $visualOpenRepeatEvidence
    }
    if ($null -eq $textVisualClosedEvidence -and $interactionKind -eq "Text") {
        $textVisualClosedEvidence = Get-TextVisualClosedEvidence $frames $recordingResult $interactionResult
    }
    if ($null -eq $themeShadowDenseFrameStability -and $control -eq "ThemeShadow" -and $interactionKind -eq "Value") {
        $themeShadowDenseFrameStability = Get-ThemeShadowDenseFrameStability $recordingPath $caseDir $recordingResult $interactionResult
    }
    $animationFrameDelta = if (Test-ControlRequiresAnimatedVisualProof $control) { Get-EarlyFrameDelta $frames } else { $null }
    $animationEvidence = Test-AnimationEvidence $control $animationFrameDelta
    $openRepeatEvidence = (Test-OpenRepeatEvidence $interactionResult) -and (
        $interactionKind -ne "OpenRepeat" -or
        $visualOpenRepeatEvidenceAccepted)
    $stateEvidence = Test-StateEvidence $interactionResult
    $expansionEvidence = Test-ExpansionEvidence $interactionResult
    $valueEvidence = Test-ValueEvidence $interactionResult
    $layoutStabilityEvidence = Test-LayoutStabilityEvidence $interactionResult
    $themeShadowVideoVisualEvidence = Test-ThemeShadowVisualEvidence $localFrameDeltas
    $themeShadowArtifactVisualEvidence = Test-ThemeShadowArtifactVisualEvidence $interactionResult
    $themeShadowVisualEvidence = $themeShadowVideoVisualEvidence
    $themeShadowCasterStabilityEvidence = Test-ThemeShadowCasterStabilityEvidence $interactionResult
    $themeShadowDenseFrameStabilityEvidence = Test-ThemeShadowDenseFrameStabilityEvidence $themeShadowDenseFrameStability
    $themeShadowArtifactCardStabilityEvidence = Test-ThemeShadowArtifactCardStabilityEvidence $interactionResult
    $layoutStabilityEvidenceAccepted =
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $valueEvidence -and
        $layoutStabilityEvidence -and
        $themeShadowCasterStabilityEvidence -and
        $themeShadowArtifactCardStabilityEvidence -and
        $themeShadowDenseFrameStabilityEvidence -and
        $themeShadowVisualEvidence
    $selectionEvidence = Test-SelectionEvidence $interactionResult
    $visualSelectionEvidence = Test-VisualSelectionEvidence $control $interactionKind $maxLocalFrameDelta
    $optionEvidence = Test-OptionEvidence $interactionResult
    $outputEvidence = Test-OutputEvidence $interactionResult
    $textEvidence = if ($interactionKind -eq "Text") { Test-TextEvidence $interactionResult } else { $false }
    $scrollEvidence = Test-ScrollEvidence $interactionResult
    $shellNavigationEvidence = Test-ShellNavigationEvidence $interactionResult
    $breadcrumbEvidence = Test-BreadcrumbEvidence $interactionResult
    $routeNavigationEvidence = Test-RouteNavigationEvidence $interactionResult
    $preparedOpenEvidence = Test-PreparedOpenEvidence $interactionResult
    $interactionEvidenceForKind = $false
    switch ($interactionKind) {
        "State" { $interactionEvidenceForKind = $stateEvidence }
        "Expansion" { $interactionEvidenceForKind = $expansionEvidence }
        "Value" { $interactionEvidenceForKind = $valueEvidence }
        "Selection" { $interactionEvidenceForKind = $selectionEvidence -or $visualSelectionEvidence }
        "Option" { $interactionEvidenceForKind = $optionEvidence }
        "Text" { $interactionEvidenceForKind = $textEvidence }
        "Output" { $interactionEvidenceForKind = $outputEvidence }
        "Scroll" { $interactionEvidenceForKind = $scrollEvidence }
        "ShellNavigation" { $interactionEvidenceForKind = $shellNavigationEvidence }
        "Breadcrumb" { $interactionEvidenceForKind = $breadcrumbEvidence }
        "RouteNavigation" { $interactionEvidenceForKind = $routeNavigationEvidence }
    }
    if ($status -eq "Passed" -and $interactionKind -eq "Selection" -and !$selectionEvidence -and !$visualSelectionEvidence) {
        $status = "Failed"
        $notes.Add("Selection interaction did not change machine-readable selection/output and no visual selection evidence was accepted.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Selection" -and $control -eq "SelectorBar" -and !$visualSelectionEvidence) {
        $status = "Failed"
        $notes.Add("SelectorBar selection changed through automation but no rendered frame change was detected.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Selection" -and !$selectionEvidence -and $visualSelectionEvidence) {
        $notes.Add(("Visual selection evidence was accepted from local frame delta {0}." -f $maxLocalFrameDelta.ToString([Globalization.CultureInfo]::InvariantCulture)))
    }

    if ($status -eq "Passed" -and $interactionKind -eq "State" -and !$stateEvidence) {
        $status = "Failed"
        $notes.Add("State interaction did not change the target toggle state.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Expansion" -and !$expansionEvidence) {
        $status = "Failed"
        $notes.Add("Expansion interaction did not expose the expected expanded child content.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Value" -and !$valueEvidence) {
        $status = "Failed"
        $notes.Add("Value interaction did not reach the expected target value.")
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value") {
        $valueInputMethod = if ($null -ne $interactionResult -and $interactionResult.Contains("ValueInputMethod")) { $interactionResult.ValueInputMethod } else { "" }
        if ($valueInputMethod -ne "RangeValuePatternAnimated") {
            $status = "Failed"
            $notes.Add(("ThemeShadow depth interaction used {0}; expected animated RangeValuePattern transition inside the recorded clip." -f $valueInputMethod))
        }
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $valueEvidence -and
        !$layoutStabilityEvidence) {
        $status = "Failed"
        $beforeBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("BeforeLayoutBounds")) { $interactionResult.BeforeLayoutBounds } else { "" }
        $afterBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("AfterLayoutBounds")) { $interactionResult.AfterLayoutBounds } else { "" }
        $notes.Add(("ThemeShadow depth changed but one or more sample bounds moved or could not be proven stable. Before={0}; after={1}." -f $beforeBounds, $afterBounds))
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $valueEvidence -and
        $layoutStabilityEvidence -and
        !$themeShadowCasterStabilityEvidence) {
        $status = "Failed"
        $beforeCasterBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("ThemeShadowCasterBeforeBounds")) { $interactionResult.ThemeShadowCasterBeforeBounds } else { "" }
        $afterCasterBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("ThemeShadowCasterAfterBounds")) { $interactionResult.ThemeShadowCasterAfterBounds } else { "" }
        $notes.Add(("ThemeShadow depth changed but the card/caster bounds moved or were not measured. Before={0}; after={1}." -f $beforeCasterBounds, $afterCasterBounds))
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $valueEvidence -and
        $layoutStabilityEvidence -and
        $themeShadowCasterStabilityEvidence -and
        !$themeShadowArtifactCardStabilityEvidence) {
        $status = "Failed"
        $artifactEvidence = Get-ThemeShadowArtifactEvidenceFromInteraction $interactionResult
        if ($null -ne $artifactEvidence -and $artifactEvidence.Contains("Generated") -and $artifactEvidence.Generated) {
            $notes.Add(("ThemeShadow before/after rendered artifacts show the card edge moved or could not be proven stable. BeforeEdge={0}; afterEdge={1}; edgeShift={2}; threshold={3}." -f $artifactEvidence.BeforeCardEdgeBounds, $artifactEvidence.AfterCardEdgeBounds, $artifactEvidence.CardEdgeShift, $artifactEvidence.CardEdgeShiftThreshold))
        }
        else {
            $reason = if ($null -ne $artifactEvidence -and $artifactEvidence.Contains("Reason")) { $artifactEvidence.Reason } else { "Before/after rendered artifact evidence was not generated." }
            $notes.Add(("ThemeShadow depth changed but before/after rendered artifacts did not prove the card stayed fixed. {0}" -f $reason))
        }
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $valueEvidence -and
        $layoutStabilityEvidence -and
        !$themeShadowVisualEvidence) {
        $status = "Failed"
        $visualDelta = Get-LocalFrameDeltaByName $localFrameDeltas "ThemeShadowVisualBounds"
        $visualBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("ThemeShadowVisualBounds")) { $interactionResult.ThemeShadowVisualBounds } else { "" }
        $artifactEvidence = Get-ThemeShadowArtifactEvidenceFromInteraction $interactionResult
        $artifactDelta = if ($null -ne $artifactEvidence -and $artifactEvidence.Contains("RootMeanDelta")) { $artifactEvidence.RootMeanDelta } else { "" }
        $notes.Add(("ThemeShadow depth changed but no rendered sample-root visual delta was proven. Bounds={0}; videoDelta={1}; artifactRootDelta={2}." -f $visualBounds, $visualDelta, $artifactDelta))
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $valueEvidence -and
        $layoutStabilityEvidence -and
        $themeShadowCasterStabilityEvidence -and
        !$themeShadowDenseFrameStabilityEvidence) {
        $status = "Failed"
        if ($null -ne $themeShadowDenseFrameStability -and $themeShadowDenseFrameStability.Contains("Generated") -and $themeShadowDenseFrameStability.Generated) {
            $notes.Add(("ThemeShadow rendered card moved or changed too much in dense video frames, indicating a visible layout shift. WorstFrame={0}; cardDelta={1}; threshold={2}; worstEdgeFrame={3}; edgeShift={4}; edgeThreshold={5}; baselineEdge={6}." -f $themeShadowDenseFrameStability.WorstFrame, $themeShadowDenseFrameStability.MaxCardMeanDelta, $themeShadowDenseFrameStability.CardDeltaThreshold, $themeShadowDenseFrameStability.WorstEdgeFrame, $themeShadowDenseFrameStability.MaxCardEdgeShift, $themeShadowDenseFrameStability.CardEdgeShiftThreshold, $themeShadowDenseFrameStability.BaselineCardEdgeBounds))
        }
        else {
            $reason = if ($null -ne $themeShadowDenseFrameStability -and $themeShadowDenseFrameStability.Contains("Reason")) { $themeShadowDenseFrameStability.Reason } else { "Dense rendered-card stability evidence was not generated." }
            $notes.Add(("ThemeShadow depth changed but dense video frames did not prove the rendered card stayed fixed. {0}" -f $reason))
        }
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Option" -and !$optionEvidence) {
        $status = "Failed"
        $notes.Add("Option interaction did not change the option or sample state.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Text" -and !$textEvidence) {
        $status = "Failed"
        if ($null -ne $interactionResult -and
            $interactionResult.Contains("SuggestionClosed") -and
            !$interactionResult.SuggestionClosed) {
            $notes.Add("Text interaction left the suggestion popup visible after the claimed suggestion choice.")
        }
        else {
            $notes.Add("Text interaction did not expose the expected output.")
        }
    }

    if ($status -eq "Passed" -and
        $interactionKind -eq "Text" -and
        $control -eq "AutoSuggestBox") {
        if ($null -eq $textVisualClosedEvidence -or
            !$textVisualClosedEvidence.Generated -or
            !$textVisualClosedEvidence.Closed) {
            $status = "Failed"
            if ($null -ne $textVisualClosedEvidence -and $textVisualClosedEvidence.Generated) {
                $notes.Add(("Rendered final frame still differs inside the initial suggestion popup bounds. Bounds={0}; frames={1}->{2}; delta={3}." -f $textVisualClosedEvidence.Bounds, $textVisualClosedEvidence.BaselineFrame, $textVisualClosedEvidence.FinalFrame, $textVisualClosedEvidence.FinalDelta))
            }
            else {
                $reason = if ($null -ne $textVisualClosedEvidence -and $textVisualClosedEvidence.Contains("Reason")) { $textVisualClosedEvidence.Reason } else { "Visual close evidence was not generated." }
                $notes.Add(("Rendered final frame did not prove the suggestion popup closed. {0}" -f $reason))
            }
        }
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Output" -and !$outputEvidence) {
        $status = "NeedsReview"
        $notes.Add("Machine-readable output text did not change; manual frame review is required.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Scroll" -and !$scrollEvidence) {
        $status = "Failed"
        $notes.Add("Scroll interaction did not change the target scroll percent.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "ShellNavigation" -and !$shellNavigationEvidence) {
        $status = "Failed"
        $failures = if ($null -ne $interactionResult -and $interactionResult.Contains("Failures")) {
            @($interactionResult.Failures) -join " "
        }
        else {
            "Shell navigation interaction did not expose the expected expanded/collapsed child states."
        }
        $notes.Add($failures)
    }

    if ($status -eq "Passed" -and $interactionKind -eq "Breadcrumb" -and !$breadcrumbEvidence) {
        $status = "Failed"
        $notes.Add("Breadcrumb interaction did not remove the trailing folders after clicking Folder1.")
    }

    if ($status -eq "Passed" -and $interactionKind -eq "RouteNavigation" -and !$routeNavigationEvidence) {
        $status = "Failed"
        $afterRoute = if ($null -ne $interactionResult -and $interactionResult.Contains("AfterRoute")) { $interactionResult.AfterRoute } else { "" }
        $expectedRoute = if ($null -ne $interactionResult -and $interactionResult.Contains("ExpectedRoute")) { $interactionResult.ExpectedRoute } else { "" }
        $targetSampleVisible = if ($null -ne $interactionResult -and $interactionResult.Contains("TargetSampleVisible")) { $interactionResult.TargetSampleVisible } else { $false }
        $notes.Add(("Route navigation did not reach the expected page. Expected={0}; actual={1}; target sample visible={2}." -f $expectedRoute, $afterRoute, $targetSampleVisible))
    }

    if ($status -eq "Passed" -and $interactionKind -eq "PreparedOpen" -and !$preparedOpenEvidence) {
        $status = "Failed"
        $notes.Add("Prepared opened content was not visible and anchored during recording.")
    }

    if ($status -eq "Passed" -and (Test-ControlRequiresAnimatedVisualProof $control) -and !$animationEvidence) {
        $status = "NeedsReview"
        $notes.Add("Animated visual proof was not detected in early poster frames.")
    }

    if ($status -eq "Passed" -and $animationEvidence) {
        $notes.Add(("Animated visual proof delta {0} was detected in early poster frames." -f $animationFrameDelta.ToString([Globalization.CultureInfo]::InvariantCulture)))
    }

    if ($status -eq "Passed" -and
        $control -eq "ThemeShadow" -and
        $interactionKind -eq "Value" -and
        $layoutStabilityEvidenceAccepted) {
        $beforeCasterBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("ThemeShadowCasterBeforeBounds")) { $interactionResult.ThemeShadowCasterBeforeBounds } else { "" }
        $afterCasterBounds = if ($null -ne $interactionResult -and $interactionResult.Contains("ThemeShadowCasterAfterBounds")) { $interactionResult.ThemeShadowCasterAfterBounds } else { "" }
        $valueInputMethod = if ($null -ne $interactionResult -and $interactionResult.Contains("ValueInputMethod")) { $interactionResult.ValueInputMethod } else { "" }
        $dragStartPoint = if ($null -ne $interactionResult -and $interactionResult.Contains("DragStartPoint")) { $interactionResult.DragStartPoint } else { "" }
        $dragEndPoint = if ($null -ne $interactionResult -and $interactionResult.Contains("DragEndPoint")) { $interactionResult.DragEndPoint } else { "" }
        $notes.Add(("ThemeShadow card/caster bounds stayed fixed while depth changed through {0} ({1}->{2}). Caster before={3}; after={4}. The visible shadow envelope may expand with depth; live WinUI source-geometry captures show the same behavior." -f $valueInputMethod, $dragStartPoint, $dragEndPoint, $beforeCasterBounds, $afterCasterBounds))
        $artifactEvidence = Get-ThemeShadowArtifactEvidenceFromInteraction $interactionResult
        if ($null -ne $artifactEvidence -and $artifactEvidence.Contains("Generated") -and $artifactEvidence.Generated) {
            $notes.Add(("Before/after ThemeShadow artifacts changed visually and kept the rendered card edge fixed. rootDelta={0}; edgeShift={1}; beforeEdge={2}; afterEdge={3}; shadowEnvelope={4}->{5}; envelopeDelta={6}." -f $artifactEvidence.RootMeanDelta, $artifactEvidence.CardEdgeShift, $artifactEvidence.BeforeCardEdgeBounds, $artifactEvidence.AfterCardEdgeBounds, $artifactEvidence.BeforeShadowEnvelopeBounds, $artifactEvidence.AfterShadowEnvelopeBounds, $artifactEvidence.ShadowEnvelopeDelta))
        }
        if ($null -ne $themeShadowDenseFrameStability -and $themeShadowDenseFrameStability.Contains("Generated") -and $themeShadowDenseFrameStability.Generated) {
            $notes.Add(("Dense ThemeShadow video frames kept the rendered card fixed. Frames={0}; cardDelta={1}; threshold={2}; edgeShift={3}; edgeThreshold={4}; baselineEdge={5}." -f $themeShadowDenseFrameStability.FrameCount, $themeShadowDenseFrameStability.MaxCardMeanDelta, $themeShadowDenseFrameStability.CardDeltaThreshold, $themeShadowDenseFrameStability.MaxCardEdgeShift, $themeShadowDenseFrameStability.CardEdgeShiftThreshold, $themeShadowDenseFrameStability.BaselineCardEdgeBounds))
        }
    }

    if ($status -eq "Passed" -and
        $interactionKind -ne "Static" -and
        $null -ne $maxFrameDelta -and
        $maxFrameDelta -lt 0.35) {
        if ((Test-InteractionRequiresLocalVisualEvidence $interactionKind) -and $interactionEvidenceForKind -and !$localVisualEvidence -and !$layoutStabilityEvidenceAccepted) {
            $status = "NeedsReview"
            $notes.Add("Interactive recording produced low poster-frame delta and no local rendered change inside recorded interaction bounds.")
        }
        elseif ($layoutStabilityEvidenceAccepted -and !$localVisualEvidence) {
            $notes.Add("ThemeShadow target value was reached, but local video delta was unexpectedly low.")
        }
        elseif ($localVisualEvidence) {
            $notes.Add(("Local visual delta {0} was detected inside recorded interaction bounds." -f $maxLocalFrameDelta.ToString([Globalization.CultureInfo]::InvariantCulture)))
        }

        if ($status -eq "Passed") {
            if ($interactionKind -eq "OpenRepeat" -and $openRepeatEvidence) {
                $notes.Add("Expected open elements were detected on both opens despite low full-frame delta.")
            }
            elseif ($interactionKind -eq "State" -and $stateEvidence) {
                $notes.Add("Before/after toggle state changed despite low full-frame delta.")
            }
            elseif ($interactionKind -eq "Expansion" -and $expansionEvidence) {
                $notes.Add("Expanded child content was detected despite low full-frame delta.")
            }
            elseif ($layoutStabilityEvidenceAccepted) {
                $notes.Add("ThemeShadow target value was reached, but full-frame video delta was unexpectedly low.")
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
            elseif ($interactionKind -eq "ShellNavigation" -and $shellNavigationEvidence) {
                $notes.Add("Shell navigation expand/collapse evidence changed despite low full-frame delta.")
            }
            elseif ($interactionKind -eq "Breadcrumb" -and $breadcrumbEvidence) {
                $notes.Add("Breadcrumb item collection changed despite low full-frame delta.")
            }
            elseif ($interactionKind -eq "RouteNavigation" -and $routeNavigationEvidence) {
                $notes.Add("Route changed to the expected page despite low full-frame delta.")
            }
            elseif ($interactionKind -eq "PreparedOpen" -and $preparedOpenEvidence) {
                $notes.Add("Prepared opened content was visible and anchored despite low full-frame delta.")
            }
            else {
                $status = "NeedsReview"
                $notes.Add("Interactive recording produced low poster-frame delta.")
            }
        }
    }

    $result = [ordered]@{
        Control = $control
        Theme = $Theme
        Route = $route
        Status = $status
        InteractionKind = $interactionKind
        RecordingDurationSeconds = $recordingDurationSeconds
        ActualRecordingDurationSeconds = if ($null -ne $recordingResult -and $null -ne $recordingResult.DurationSeconds) { $recordingResult.DurationSeconds } else { $null }
        Recording = if (Test-Path $recordingPath) { (Resolve-Path $recordingPath).Path } else { $recordingPath }
        RecorderResult = $recordingResult
        Frames = $frames
        DenseTransitionReview = $denseTransitionReview
        MaxFrameDelta = $maxFrameDelta
        LocalFrameDeltas = $localFrameDeltas
        MaxLocalFrameDelta = $maxLocalFrameDelta
        ScreenRecordingGalleryAnchor = $screenRecordingGalleryAnchor
        LocalVisualEvidence = $localVisualEvidence
        AnimationFrameDelta = $animationFrameDelta
        AnimationEvidence = $animationEvidence
        OpenRepeatEvidence = $openRepeatEvidence
        VisualOpenRepeatEvidence = $visualOpenRepeatEvidence
        StateEvidence = $stateEvidence
        ExpansionEvidence = $expansionEvidence
        ValueEvidence = $valueEvidence
        LayoutStabilityEvidence = $layoutStabilityEvidence
        ThemeShadowCasterStabilityEvidence = $themeShadowCasterStabilityEvidence
        ThemeShadowVideoVisualEvidence = $themeShadowVideoVisualEvidence
        ThemeShadowArtifactVisualEvidence = $themeShadowArtifactVisualEvidence
        ThemeShadowArtifactCardStabilityEvidence = $themeShadowArtifactCardStabilityEvidence
        ThemeShadowDenseFrameStabilityEvidence = $themeShadowDenseFrameStabilityEvidence
        ThemeShadowDenseFrameStability = $themeShadowDenseFrameStability
        ThemeShadowVisualEvidence = $themeShadowVisualEvidence
        SelectionEvidence = $selectionEvidence
        VisualSelectionEvidence = $visualSelectionEvidence
        OptionEvidence = $optionEvidence
        OutputEvidence = $outputEvidence
        TextEvidence = $textEvidence
        TextVisualClosedEvidence = $textVisualClosedEvidence
        ScrollEvidence = $scrollEvidence
        ShellNavigationEvidence = $shellNavigationEvidence
        BreadcrumbEvidence = $breadcrumbEvidence
        RouteNavigationEvidence = $routeNavigationEvidence
        PreparedOpenEvidence = $preparedOpenEvidence
        RenderedPageArtifactAnchor = $renderedPageArtifactAnchor
        InteractionResult = $interactionResult
        Notes = ($notes.ToArray() -join " ")
    }
    $results.Add($result)
    [void](Write-RunCheckpoint $runDir $results)
}

$checkpoint = Write-RunCheckpoint $runDir $results

[pscustomobject]@{
    RunDirectory = $runDir
    Manifest = $checkpoint.Manifest
    Report = $checkpoint.Report
    Total = $results.Count
    Passed = @($results | Where-Object { $_.Status -eq "Passed" }).Count
    NeedsReview = @($results | Where-Object { $_.Status -eq "NeedsReview" }).Count
    Failed = @($results | Where-Object { $_.Status -eq "Failed" }).Count
}
