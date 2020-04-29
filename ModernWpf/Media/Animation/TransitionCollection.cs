using System.Collections.ObjectModel;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents a collection of Transition objects. Each Transition object represents
    /// a different theme transition, part of the Windows Runtime animation library.
    /// </summary>
    public class TransitionCollection : Collection<Transition>
    {
        /// <summary>
        /// Initializes a new instance of the TransitionCollection class.
        /// </summary>
        public TransitionCollection()
        {
        }
    }
}
