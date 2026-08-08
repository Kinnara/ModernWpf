using System;

namespace ModernWpf.Controls
{
    public sealed class TimePickerValueChangedEventArgs : EventArgs
    {
        internal TimePickerValueChangedEventArgs(TimeSpan oldTime, TimeSpan newTime)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }

        public TimeSpan OldTime { get; }

        public TimeSpan NewTime { get; }
    }

    public sealed class TimePickerSelectedValueChangedEventArgs : EventArgs
    {
        internal TimePickerSelectedValueChangedEventArgs(TimeSpan? oldTime, TimeSpan? newTime)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }

        public TimeSpan? OldTime { get; }

        public TimeSpan? NewTime { get; }
    }
}
