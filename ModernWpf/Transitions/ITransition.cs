// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Controls the behavior of transitions.
    /// </summary>
    [Obsolete]
    public interface ITransition
    {
        /// <summary>
        /// Occurs when the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// has completed playing.
        /// </summary>
        event EventHandler Completed;

        /// <summary>
        /// Gets the
        /// <see cref="T:System.Windows.Media.Animation.ClockState"/>
        /// of the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>.
        /// </summary>
        /// <returns></returns>
        ClockState GetCurrentState();

        /// <summary>
        /// Gets the current time of the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>.
        /// </summary>
        /// <returns>The current time.</returns>
        TimeSpan? GetCurrentTime();

        /// <summary>
        /// Pauses the animation clock associated with the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the animation clock, or run-time state, associated with the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>.
        /// </summary>
        void Resume();

        /// <summary>
        /// Moves the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// to the specified animation position. The
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// performs the requested seek when the next clock tick occurs.
        /// </summary>
        /// <param name="offset">The specified animation position.</param>
        void Seek(TimeSpan offset);

        /// <summary>
        /// Moves the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>
        /// to the specified animation position immediately (synchronously).
        /// </summary>
        /// <param name="offset">The specified animation position</param>
        void SeekAlignedToLastTick(TimeSpan offset);

        /// <summary>
        /// Advances the current time of the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>'s
        /// clock to the end of its active period.
        /// </summary>
        void SkipToFill();

        /// <summary>
        /// Initiates the set of animations associated with the
        /// <see cref="T:ModernWpf.Controls.ITransition"/>.
        /// </summary>
        void Begin();

        /// <summary>
        /// Stops the <see cref="T:ModernWpf.Controls.ITransition"/>.
        /// </summary>
        void Stop();
    }
}