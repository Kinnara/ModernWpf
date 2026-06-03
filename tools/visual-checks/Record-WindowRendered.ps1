param(
    [string]$Output,
    [int]$DurationSeconds = 5,
    [int]$FrameRate = 10,
    [int]$ProcessId = 0,
    [IntPtr]$WindowHandle = [IntPtr]::Zero,
    [int]$Left = 0,
    [int]$Top = 0,
    [int]$Width = 0,
    [int]$Height = 0,
    [ValidateSet("Rendered", "Screen")]
    [string]$CaptureMode = "Rendered",
    [switch]$KeepFrames
)

$ErrorActionPreference = "Stop"

if ($DurationSeconds -le 0) {
    throw "DurationSeconds must be greater than zero."
}
if ($FrameRate -le 0) {
    throw "FrameRate must be greater than zero."
}
if ($ProcessId -le 0 -and $WindowHandle -eq [IntPtr]::Zero) {
    throw "Pass either -ProcessId or -WindowHandle."
}
if ($Width -le 0 -or $Height -le 0) {
    throw "Capture rectangle must have positive width and height."
}

function Get-FfmpegPath {
    $command = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        return ""
    }

    return $command.Source
}

$ffmpeg = Get-FfmpegPath
if ([string]::IsNullOrWhiteSpace($ffmpeg)) {
    throw "ffmpeg was not found on PATH. Rendered recording requires ffmpeg."
}

try {
    Add-Type -AssemblyName System.Drawing.Common
}
catch {
    Add-Type -AssemblyName System.Drawing
}

if ([string]::IsNullOrWhiteSpace($Output)) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss-fff"
    $Output = Join-Path (Join-Path (Get-Location) "artifacts\window-recordings") ("rendered-$stamp.mp4")
}

$fullOutput = [IO.Path]::GetFullPath($Output)
if ([IO.Path]::GetExtension($fullOutput) -ine ".mp4") {
    throw "Rendered recording writes .mp4 files."
}

$outputDirectory = Split-Path -Parent $fullOutput
if (![string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

if (-not ("ModernWpfRenderedRecorder.Capture" -as [type])) {
    $drawingAssemblies = [AppDomain]::CurrentDomain.GetAssemblies() |
        Where-Object {
            $_.GetName().Name -in @(
                "System.Drawing.Common",
                "System.Drawing.Primitives",
                "System.Private.Windows.Core",
                "System.Private.Windows.GdiPlus")
        } |
        ForEach-Object { $_.Location } |
        Where-Object { ![string]::IsNullOrWhiteSpace($_) } |
        Select-Object -Unique
    foreach ($assemblyName in @("System.Drawing.Common", "System.Drawing.Primitives", "System.Private.Windows.Core", "System.Private.Windows.GdiPlus")) {
        $candidate = Join-Path $PSHOME ($assemblyName + ".dll")
        if (Test-Path $candidate) {
            $drawingAssemblies = @($drawingAssemblies) + $candidate
        }
    }
    $drawingAssemblies = $drawingAssemblies | Select-Object -Unique
    Add-Type -ReferencedAssemblies $drawingAssemblies -TypeDefinition @'
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ModernWpfRenderedRecorder
{
    public static class Capture
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        public static RECT GetRect(IntPtr hWnd)
        {
            RECT rect;
            if (!GetWindowRect(hWnd, out rect))
            {
                rect = new RECT();
            }

            return rect;
        }

        public static int GetProcessId(IntPtr hWnd)
        {
            int processId;
            GetWindowThreadProcessId(hWnd, out processId);
            return processId;
        }

        public static void CaptureScreenRect(int left, int top, int width, int height, string outputPath)
        {
            using (Bitmap canvas = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(canvas))
            {
                bool copied = false;
                try
                {
                    graphics.CopyFromScreen(left, top, 0, 0, new Size(width, height));
                    copied = true;
                }
                catch
                {
                    graphics.Clear(Color.Transparent);
                    IntPtr hdcDest = graphics.GetHdc();
                    try
                    {
                        IntPtr hdcSource = GetDC(IntPtr.Zero);
                        if (hdcSource != IntPtr.Zero)
                        {
                            try
                            {
                                copied = BitBlt(hdcDest, 0, 0, width, height, hdcSource, left, top, 0x00CC0020);
                            }
                            finally
                            {
                                ReleaseDC(IntPtr.Zero, hdcSource);
                            }
                        }
                    }
                    finally
                    {
                        graphics.ReleaseHdc(hdcDest);
                    }
                }

                if (!copied)
                {
                    graphics.Clear(Color.Transparent);
                }

                canvas.Save(outputPath, ImageFormat.Png);
            }
        }

        public static void CaptureProcessWindows(int processId, IntPtr mainHandle, int left, int top, int width, int height, string outputPath)
        {
            using (Bitmap canvas = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(canvas))
            {
                graphics.Clear(Color.Transparent);

                if (mainHandle != IntPtr.Zero)
                {
                    DrawWindow(graphics, mainHandle, left, top, false);
                }

                ArrayList windows = GetProcessWindows(processId, mainHandle);
                windows.Reverse();
                foreach (object item in windows)
                {
                    IntPtr window = (IntPtr)item;
                    DrawWindow(graphics, window, left, top, true);
                }

                canvas.Save(outputPath, ImageFormat.Png);
            }
        }

        private static ArrayList GetProcessWindows(int processId, IntPtr excludedHandle)
        {
            ArrayList windows = new ArrayList();
            EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
            {
                if (hWnd == excludedHandle || !IsWindowVisible(hWnd))
                {
                    return true;
                }

                int candidateProcessId;
                GetWindowThreadProcessId(hWnd, out candidateProcessId);
                if (candidateProcessId != processId)
                {
                    return true;
                }

                RECT rect;
                if (!GetWindowRect(hWnd, out rect) || rect.Right <= rect.Left || rect.Bottom <= rect.Top)
                {
                    return true;
                }

                windows.Add(hWnd);
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private static void DrawWindow(Graphics canvas, IntPtr hWnd, int captureLeft, int captureTop, bool clearEdgeTransparentBlack)
        {
            RECT rect;
            if (!GetWindowRect(hWnd, out rect))
            {
                return;
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            using (Bitmap windowBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (Graphics windowGraphics = Graphics.FromImage(windowBitmap))
            {
                bool printed = false;
                foreach (uint flags in new uint[] { 2, 0 })
                {
                    windowGraphics.Clear(Color.Transparent);
                    IntPtr hdc = windowGraphics.GetHdc();
                    try
                    {
                        printed = PrintWindow(hWnd, hdc, flags);
                    }
                    finally
                    {
                        windowGraphics.ReleaseHdc(hdc);
                    }

                    if (printed && BitmapHasVisiblePixels(windowBitmap))
                    {
                        break;
                    }
                }

                if (!printed || !BitmapHasVisiblePixels(windowBitmap))
                {
                    return;
                }

                if (clearEdgeTransparentBlack)
                {
                    ClearEdgeTransparentBlack(windowBitmap);
                }

                canvas.DrawImageUnscaled(windowBitmap, rect.Left - captureLeft, rect.Top - captureTop);
            }
        }

        private static void ClearEdgeTransparentBlack(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            bool[] visited = new bool[width * height];
            int[] pending = new int[width * height];
            int pendingCount = 0;

            for (int x = 0; x < width; x++)
            {
                AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, x, 0);
                AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, x, height - 1);
            }

            for (int y = 1; y < height - 1; y++)
            {
                AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, 0, y);
                AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, width - 1, y);
            }

            while (pendingCount > 0)
            {
                int index = pending[--pendingCount];
                int x = index % width;
                int y = index / width;

                Color pixel = bitmap.GetPixel(x, y);
                if (!IsTransparentBlackPixel(pixel))
                {
                    continue;
                }

                bitmap.SetPixel(x, y, Color.FromArgb(0, pixel.R, pixel.G, pixel.B));

                if (x > 0)
                {
                    AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, x - 1, y);
                }

                if (x + 1 < width)
                {
                    AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, x + 1, y);
                }

                if (y > 0)
                {
                    AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, x, y - 1);
                }

                if (y + 1 < height)
                {
                    AddTransparentBlackPixel(bitmap, visited, pending, ref pendingCount, x, y + 1);
                }
            }
        }

        private static void AddTransparentBlackPixel(Bitmap bitmap, bool[] visited, int[] pending, ref int pendingCount, int x, int y)
        {
            int width = bitmap.Width;
            int index = y * width + x;
            if (!visited[index] &&
                pendingCount < pending.Length &&
                IsTransparentBlackPixel(bitmap.GetPixel(x, y)))
            {
                visited[index] = true;
                pending[pendingCount++] = index;
            }
        }

        private static bool IsTransparentBlackPixel(Color pixel)
        {
            return pixel.A > 16 && pixel.R <= 4 && pixel.G <= 4 && pixel.B <= 4;
        }

        private static bool BitmapHasVisiblePixels(Bitmap bitmap)
        {
            int step = Math.Max(1, Math.Max(bitmap.Width, bitmap.Height) / 80);
            for (int y = 0; y < bitmap.Height; y += step)
            {
                for (int x = 0; x < bitmap.Width; x += step)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if (pixel.A > 16 && (pixel.R > 3 || pixel.G > 3 || pixel.B > 3))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
'@
}

if ($WindowHandle -ne [IntPtr]::Zero -and $ProcessId -le 0) {
    $ProcessId = [ModernWpfRenderedRecorder.Capture]::GetProcessId($WindowHandle)
}

$frameRoot = Join-Path $outputDirectory ([IO.Path]::GetFileNameWithoutExtension($fullOutput) + ".frames")
if (Test-Path $frameRoot) {
    Remove-Item -LiteralPath $frameRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $frameRoot | Out-Null

$frameCount = $DurationSeconds * $FrameRate
$intervalMs = 1000.0 / $FrameRate
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
for ($index = 0; $index -lt $frameCount; $index++) {
    $framePath = Join-Path $frameRoot ("frame-{0:00000}.png" -f $index)
    if ($CaptureMode -eq "Screen") {
        [ModernWpfRenderedRecorder.Capture]::CaptureScreenRect($Left, $Top, $Width, $Height, $framePath)
    }
    else {
        [ModernWpfRenderedRecorder.Capture]::CaptureProcessWindows($ProcessId, $WindowHandle, $Left, $Top, $Width, $Height, $framePath)
    }

    $nextDue = ($index + 1) * $intervalMs
    $sleepMs = [int][Math]::Floor($nextDue - $stopwatch.Elapsed.TotalMilliseconds)
    if ($sleepMs -gt 0) {
        Start-Sleep -Milliseconds $sleepMs
    }
}

$inputPattern = Join-Path $frameRoot "frame-%05d.png"
$arguments = @(
    "-hide_banner",
    "-loglevel", "error",
    "-nostdin",
    "-y",
    "-framerate", $FrameRate.ToString([Globalization.CultureInfo]::InvariantCulture),
    "-i", $inputPattern,
    "-an",
    "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2",
    "-c:v", "libx264",
    "-preset", "veryfast",
    "-crf", "23",
    "-pix_fmt", "yuv420p",
    "-movflags", "+faststart",
    $fullOutput
)

$ffmpegOutput = & $ffmpeg @arguments 2>&1
if ($LASTEXITCODE -ne 0) {
    $message = ($ffmpegOutput | Select-Object -Last 20) -join [Environment]::NewLine
    throw "ffmpeg rendered encoding failed with exit code $LASTEXITCODE.$([Environment]::NewLine)$message"
}

if (!$KeepFrames) {
    Remove-Item -LiteralPath $frameRoot -Recurse -Force
}

[pscustomobject]@{
    Output = $fullOutput
    Frames = $frameCount
    FrameRate = $FrameRate
    Rect = "{0},{1},{2},{3}" -f $Left, $Top, $Width, $Height
    Bytes = (Get-Item -LiteralPath $fullOutput).Length
    Recorder = if ($CaptureMode -eq "Screen") { "ScreenFfmpeg" } else { "RenderedFfmpeg" }
    FrameDirectory = if ($KeepFrames) { $frameRoot } else { "" }
}
