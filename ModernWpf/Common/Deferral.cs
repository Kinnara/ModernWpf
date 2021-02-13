using System;
using System.Runtime.InteropServices;

namespace ModernWpf
{
    /// <summary>
    /// Represents a method that handles the completed event of a deferred action.
    /// </summary>
    public delegate void DeferralCompletedHandler();

    /// <summary>
    /// Stores a <see cref="DeferralCompletedHandler"/> to be invoked upon completion of the deferral
    /// and manipulates the state of the deferral.
    /// </summary>
    public class Deferral : IDisposable
    {
        private readonly DeferralCompletedHandler _handler;

        /// <summary>
        /// Initializes a new <see cref="Deferral"/> object and specifies a
        /// <see cref="DeferralCompletedHandler"/> to be called upon completion of the deferral.
        /// </summary>
        /// <param name="handler">A <see cref="DeferralCompletedHandler"/> to be called upon completion of the deferral.</param>
        public Deferral([In] DeferralCompletedHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// If the <see cref="DeferralCompletedHandler"/> has not yet been invoked, this will call it
        /// and drop the reference to the delegate.
        /// </summary>
        public void Complete() => _handler?.Invoke();

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
