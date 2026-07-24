namespace ModernWpf.Controls
{
    public interface ICommandBarElement
    {
        bool IsCompact { get; set; }

        bool IsInOverflow { get; }

        int DynamicOverflowOrder { get; set; }
    }
}
