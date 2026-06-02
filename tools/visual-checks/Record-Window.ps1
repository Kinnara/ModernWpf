param(
    [string]$Output = "",
    [int]$DurationSeconds = 5,
    [int]$FrameRate = 10,
    [string]$ProcessName = "",
    [string]$WindowTitle = "",
    [IntPtr]$WindowHandle = [IntPtr]::Zero,
    [int]$Left = [int]::MinValue,
    [int]$Top = [int]::MinValue,
    [int]$Width = 0,
    [int]$Height = 0,
    [switch]$ListWindows
)

$ErrorActionPreference = "Stop"

if (-not ("ModernWpfGalleryRecorder.Recorder" -as [type])) {
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ModernWpfGalleryRecorder
{
    public sealed class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string Title { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public sealed class RecordingResult
    {
        public string Output { get; set; }
        public int Frames { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FrameRate { get; set; }
        public long Bytes { get; set; }
    }

    public static class Native
    {
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
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr ho);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines, byte[] lpvBits, ref BITMAPINFO lpbmi, uint usage);

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
        }

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint SRCCOPY = 0x00CC0020;
        private const uint CAPTUREBLT = 0x40000000;
        private const uint BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;

        public static void MakeDpiAware()
        {
            try { SetProcessDPIAware(); }
            catch { }
        }

        public static WindowInfo[] ListWindows()
        {
            MakeDpiAware();
            var windows = new List<WindowInfo>();
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                {
                    return true;
                }
                if (IsIconic(hWnd))
                {
                    return true;
                }

                int length = GetWindowTextLength(hWnd);
                if (length <= 0)
                {
                    return true;
                }

                var title = new StringBuilder(length + 1);
                GetWindowText(hWnd, title, title.Capacity);
                if (title.Length == 0)
                {
                    return true;
                }

                int processId;
                GetWindowThreadProcessId(hWnd, out processId);
                string processName = "";
                try
                {
                    processName = Process.GetProcessById(processId).ProcessName;
                }
                catch
                {
                }

                RECT rect;
                if (!GetWindowRect(hWnd, out rect))
                {
                    return true;
                }

                int width = Math.Max(0, rect.Right - rect.Left);
                int height = Math.Max(0, rect.Bottom - rect.Top);
                if (width == 0 || height == 0)
                {
                    return true;
                }

                windows.Add(new WindowInfo
                {
                    Handle = hWnd,
                    ProcessId = processId,
                    ProcessName = processName,
                    Title = title.ToString(),
                    Left = rect.Left,
                    Top = rect.Top,
                    Width = width,
                    Height = height
                });
                return true;
            }, IntPtr.Zero);

            return windows.ToArray();
        }

        public static WindowInfo FindByHandle(IntPtr handle)
        {
            MakeDpiAware();
            RECT rect;
            if (!GetWindowRect(handle, out rect))
            {
                throw new InvalidOperationException("Window handle was not found: " + handle);
            }
            if (IsIconic(handle))
            {
                throw new InvalidOperationException("Window is minimized and cannot be recorded: " + handle);
            }

            int processId;
            GetWindowThreadProcessId(handle, out processId);
            string processName = "";
            string titleText = "";
            try
            {
                processName = Process.GetProcessById(processId).ProcessName;
            }
            catch
            {
            }

            int length = GetWindowTextLength(handle);
            if (length > 0)
            {
                var title = new StringBuilder(length + 1);
                GetWindowText(handle, title, title.Capacity);
                titleText = title.ToString();
            }

            return new WindowInfo
            {
                Handle = handle,
                ProcessId = processId,
                ProcessName = processName,
                Title = titleText,
                Left = rect.Left,
                Top = rect.Top,
                Width = Math.Max(0, rect.Right - rect.Left),
                Height = Math.Max(0, rect.Bottom - rect.Top)
            };
        }

        public static WindowInfo GetPrimaryScreenBounds()
        {
            MakeDpiAware();
            return new WindowInfo
            {
                Handle = IntPtr.Zero,
                ProcessId = 0,
                ProcessName = "",
                Title = "Primary screen",
                Left = 0,
                Top = 0,
                Width = GetSystemMetrics(SM_CXSCREEN),
                Height = GetSystemMetrics(SM_CYSCREEN)
            };
        }

        public static byte[] CaptureTopDownBgrFrame(int left, int top, int width, int height)
        {
            int stride = ((width * 3) + 3) & ~3;
            byte[] frame = new byte[stride * height];
            IntPtr screenDc = IntPtr.Zero;
            IntPtr memoryDc = IntPtr.Zero;
            IntPtr bitmap = IntPtr.Zero;
            IntPtr oldObject = IntPtr.Zero;

            try
            {
                screenDc = GetDC(IntPtr.Zero);
                if (screenDc == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not get the screen device context.");
                }

                memoryDc = CreateCompatibleDC(screenDc);
                if (memoryDc == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not create a compatible device context.");
                }

                bitmap = CreateCompatibleBitmap(screenDc, width, height);
                if (bitmap == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not create a compatible bitmap.");
                }

                oldObject = SelectObject(memoryDc, bitmap);
                if (!BitBlt(memoryDc, 0, 0, width, height, screenDc, left, top, SRCCOPY | CAPTUREBLT))
                {
                    throw new InvalidOperationException("BitBlt failed while capturing the screen.");
                }

                var bitmapInfo = new BITMAPINFO();
                bitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bitmapInfo.bmiHeader.biWidth = width;
                bitmapInfo.bmiHeader.biHeight = -height;
                bitmapInfo.bmiHeader.biPlanes = 1;
                bitmapInfo.bmiHeader.biBitCount = 24;
                bitmapInfo.bmiHeader.biCompression = BI_RGB;
                bitmapInfo.bmiHeader.biSizeImage = (uint)frame.Length;

                int lines = GetDIBits(screenDc, bitmap, 0, (uint)height, frame, ref bitmapInfo, DIB_RGB_COLORS);
                if (lines != height)
                {
                    throw new InvalidOperationException("GetDIBits failed while reading the captured frame.");
                }

                return frame;
            }
            finally
            {
                if (oldObject != IntPtr.Zero && memoryDc != IntPtr.Zero)
                {
                    SelectObject(memoryDc, oldObject);
                }
                if (bitmap != IntPtr.Zero)
                {
                    DeleteObject(bitmap);
                }
                if (memoryDc != IntPtr.Zero)
                {
                    DeleteDC(memoryDc);
                }
                if (screenDc != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, screenDc);
                }
            }
        }
    }

    public sealed class AviWriter : IDisposable
    {
        private struct IndexEntry
        {
            public uint Offset;
            public uint Size;
        }

        private readonly FileStream _stream;
        private readonly BinaryWriter _writer;
        private readonly List<IndexEntry> _index = new List<IndexEntry>();
        private readonly int _width;
        private readonly int _height;
        private readonly int _frameRate;
        private readonly int _stride;
        private readonly int _frameSize;
        private long _riffSizePosition;
        private long _totalFramesPosition;
        private long _streamLengthPosition;
        private long _moviListStart;
        private long _moviListSizePosition;
        private long _moviDataStart;
        private bool _closed;

        public AviWriter(string path, int width, int height, int frameRate)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException("Capture dimensions must be positive.");
            }
            if (frameRate <= 0)
            {
                throw new ArgumentOutOfRangeException("Frame rate must be positive.");
            }

            _width = width;
            _height = height;
            _frameRate = frameRate;
            _stride = ((_width * 3) + 3) & ~3;
            _frameSize = _stride * _height;
            _stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new BinaryWriter(_stream, Encoding.ASCII);
            WriteHeader();
        }

        public void AddFrame(byte[] topDownBgrFrame)
        {
            if (topDownBgrFrame == null)
            {
                throw new ArgumentNullException("topDownBgrFrame");
            }
            if (topDownBgrFrame.Length != _frameSize)
            {
                throw new ArgumentException("Unexpected frame size.", "topDownBgrFrame");
            }

            long chunkStart = _stream.Position;
            WriteFourCc("00db");
            _writer.Write((uint)topDownBgrFrame.Length);
            _writer.Write(topDownBgrFrame);
            if ((topDownBgrFrame.Length & 1) != 0)
            {
                _writer.Write((byte)0);
            }

            _index.Add(new IndexEntry
            {
                Offset = checked((uint)(chunkStart - _moviDataStart)),
                Size = checked((uint)topDownBgrFrame.Length)
            });
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (_closed)
            {
                return;
            }

            long moviEnd = _stream.Position;
            WriteFourCc("idx1");
            _writer.Write((uint)(_index.Count * 16));
            foreach (IndexEntry entry in _index)
            {
                WriteFourCc("00db");
                _writer.Write((uint)0x10);
                _writer.Write(entry.Offset);
                _writer.Write(entry.Size);
            }

            long fileEnd = _stream.Position;
            PatchUInt32(_riffSizePosition, checked((uint)(fileEnd - 8)));
            PatchUInt32(_moviListSizePosition, checked((uint)(moviEnd - (_moviListStart + 8))));
            PatchUInt32(_totalFramesPosition, checked((uint)_index.Count));
            PatchUInt32(_streamLengthPosition, checked((uint)_index.Count));
            _stream.Position = fileEnd;
            _writer.Flush();
            _writer.Dispose();
            _stream.Dispose();
            _closed = true;
        }

        private void WriteHeader()
        {
            WriteFourCc("RIFF");
            _riffSizePosition = _stream.Position;
            _writer.Write((uint)0);
            WriteFourCc("AVI ");

            WriteList("hdrl", () =>
            {
                WriteChunk("avih", () =>
                {
                    _writer.Write((uint)(1000000 / _frameRate));
                    _writer.Write((uint)(_frameSize * _frameRate));
                    _writer.Write((uint)0);
                    _writer.Write((uint)0x10);
                    _totalFramesPosition = _stream.Position;
                    _writer.Write((uint)0);
                    _writer.Write((uint)0);
                    _writer.Write((uint)1);
                    _writer.Write((uint)_frameSize);
                    _writer.Write((uint)_width);
                    _writer.Write((uint)_height);
                    _writer.Write((uint)0);
                    _writer.Write((uint)0);
                    _writer.Write((uint)0);
                    _writer.Write((uint)0);
                });

                WriteList("strl", () =>
                {
                    WriteChunk("strh", () =>
                    {
                        WriteFourCc("vids");
                        WriteFourCc("DIB ");
                        _writer.Write((uint)0);
                        _writer.Write((ushort)0);
                        _writer.Write((ushort)0);
                        _writer.Write((uint)0);
                        _writer.Write((uint)1);
                        _writer.Write((uint)_frameRate);
                        _writer.Write((uint)0);
                        _streamLengthPosition = _stream.Position;
                        _writer.Write((uint)0);
                        _writer.Write((uint)_frameSize);
                        _writer.Write((uint)0xFFFFFFFF);
                        _writer.Write((uint)0);
                        _writer.Write((int)0);
                        _writer.Write((int)0);
                        _writer.Write((int)_width);
                        _writer.Write((int)_height);
                    });

                    WriteChunk("strf", () =>
                    {
                        _writer.Write((uint)40);
                        _writer.Write((int)_width);
                        _writer.Write((int)(-_height));
                        _writer.Write((ushort)1);
                        _writer.Write((ushort)24);
                        _writer.Write((uint)0);
                        _writer.Write((uint)_frameSize);
                        _writer.Write((int)0);
                        _writer.Write((int)0);
                        _writer.Write((uint)0);
                        _writer.Write((uint)0);
                    });
                });
            });

            _moviListStart = _stream.Position;
            WriteFourCc("LIST");
            _moviListSizePosition = _stream.Position;
            _writer.Write((uint)0);
            WriteFourCc("movi");
            _moviDataStart = _stream.Position;
        }

        private void WriteChunk(string fourCc, Action writeBody)
        {
            WriteFourCc(fourCc);
            long sizePosition = _stream.Position;
            _writer.Write((uint)0);
            long bodyStart = _stream.Position;
            writeBody();
            long bodyEnd = _stream.Position;
            if (((bodyEnd - bodyStart) & 1) != 0)
            {
                _writer.Write((byte)0);
            }

            long end = _stream.Position;
            PatchUInt32(sizePosition, checked((uint)(bodyEnd - bodyStart)));
            _stream.Position = end;
        }

        private void WriteList(string type, Action writeBody)
        {
            long listStart = _stream.Position;
            WriteFourCc("LIST");
            long sizePosition = _stream.Position;
            _writer.Write((uint)0);
            WriteFourCc(type);
            writeBody();
            long end = _stream.Position;
            PatchUInt32(sizePosition, checked((uint)(end - (listStart + 8))));
            _stream.Position = end;
        }

        private void PatchUInt32(long position, uint value)
        {
            long current = _stream.Position;
            _stream.Position = position;
            _writer.Write(value);
            _stream.Position = current;
        }

        private void WriteFourCc(string value)
        {
            if (value == null || value.Length != 4)
            {
                throw new ArgumentException("FOURCC values must be exactly four ASCII characters.", "value");
            }

            byte[] bytes = Encoding.ASCII.GetBytes(value);
            _writer.Write(bytes);
        }
    }

    public static class Recorder
    {
        public static RecordingResult RecordRect(int left, int top, int width, int height, string output, int durationSeconds, int frameRate)
        {
            Native.MakeDpiAware();
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            int totalFrames = Math.Max(1, checked(durationSeconds * frameRate));
            var stopwatch = Stopwatch.StartNew();

            using (var writer = new AviWriter(output, width, height, frameRate))
            {
                for (int frame = 0; frame < totalFrames; frame++)
                {
                    long targetMilliseconds = (long)Math.Round(frame * (1000.0 / frameRate));
                    int waitMilliseconds = (int)(targetMilliseconds - stopwatch.ElapsedMilliseconds);
                    if (waitMilliseconds > 0)
                    {
                        Thread.Sleep(waitMilliseconds);
                    }

                    writer.AddFrame(Native.CaptureTopDownBgrFrame(left, top, width, height));
                }
            }

            return new RecordingResult
            {
                Output = output,
                Frames = totalFrames,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                FrameRate = frameRate,
                Bytes = new FileInfo(output).Length
            };
        }
    }
}
'@
}

if ($DurationSeconds -le 0) {
    throw "DurationSeconds must be greater than zero."
}
if ($FrameRate -le 0) {
    throw "FrameRate must be greater than zero."
}

if ($ListWindows) {
    [ModernWpfGalleryRecorder.Native]::ListWindows() |
        Sort-Object ProcessName, Title |
        Select-Object ProcessName, ProcessId, Handle, Title, Left, Top, Width, Height
    return
}

$target = $null
if ($WindowHandle -ne [IntPtr]::Zero) {
    $target = [ModernWpfGalleryRecorder.Native]::FindByHandle($WindowHandle)
}
elseif (![string]::IsNullOrWhiteSpace($WindowTitle)) {
    $target = [ModernWpfGalleryRecorder.Native]::ListWindows() |
        Where-Object { $_.Title.IndexOf($WindowTitle, [StringComparison]::OrdinalIgnoreCase) -ge 0 } |
        Sort-Object ProcessName, Title |
        Select-Object -First 1
}
elseif (![string]::IsNullOrWhiteSpace($ProcessName)) {
    $normalizedProcessName = [IO.Path]::GetFileNameWithoutExtension($ProcessName)
    $target = [ModernWpfGalleryRecorder.Native]::ListWindows() |
        Where-Object { $_.ProcessName -ieq $normalizedProcessName } |
        Sort-Object Title |
        Select-Object -First 1
}

if ($null -ne $target) {
    $Left = $target.Left
    $Top = $target.Top
    $Width = $target.Width
    $Height = $target.Height
}
elseif ($Left -eq [int]::MinValue -or $Top -eq [int]::MinValue -or $Width -le 0 -or $Height -le 0) {
    $screen = [ModernWpfGalleryRecorder.Native]::GetPrimaryScreenBounds()
    $Left = $screen.Left
    $Top = $screen.Top
    $Width = $screen.Width
    $Height = $screen.Height
}

if ($Width -le 0 -or $Height -le 0) {
    throw "Capture rectangle must have positive width and height."
}

if ([string]::IsNullOrWhiteSpace($Output)) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss-fff"
    $Output = Join-Path (Join-Path (Get-Location) "artifacts\window-recordings") ("recording-$stamp.avi")
}

$result = [ModernWpfGalleryRecorder.Recorder]::RecordRect($Left, $Top, $Width, $Height, $Output, $DurationSeconds, $FrameRate)
[pscustomobject]@{
    Output = $result.Output
    Frames = $result.Frames
    FrameRate = $result.FrameRate
    Rect = "{0},{1},{2},{3}" -f $result.Left, $result.Top, $result.Width, $result.Height
    Bytes = $result.Bytes
}
