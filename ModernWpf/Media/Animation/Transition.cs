using System.Windows;

namespace ModernWpf.Media.Animation
{
    /// <summary>
    /// Represents a visual behavior that occurs for predefined actions or state changes.
    /// Specific theme transitions (various Transition derived classes) can be applied
    /// to individual elements using the UIElement.Transitions property, or applied for
    /// scenario-specific theme transition properties such as ContentControl.ContentTransitions.
    /// </summary>
    public class Transition : DependencyObject
    {
        private protected Transition()
        {
        }
    }
}
