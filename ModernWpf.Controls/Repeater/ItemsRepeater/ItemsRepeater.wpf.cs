using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    partial class ItemsRepeater
    {
        // WPF-specific workaround to avoid freezing and improve performance
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            var virtInfo = TryGetVirtualizationInfo(child);
            if (virtInfo != null && virtInfo.IsRealized)
            {
                var oldDesiredSize = virtInfo.DesiredSize;
                if (!oldDesiredSize.IsEmpty)
                {
                    var newDesiredSize = child.DesiredSize;
                    var renderSize = child.RenderSize;

                    if (newDesiredSize.Height != oldDesiredSize.Height && renderSize.Height == oldDesiredSize.Height ||
                        newDesiredSize.Width != oldDesiredSize.Width && renderSize.Width == oldDesiredSize.Width)
                    {
                        base.OnChildDesiredSizeChanged(child);
                    }
                }
            }
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            return new RepeaterUIElementCollection(this, logicalParent);
        }
    }
}
