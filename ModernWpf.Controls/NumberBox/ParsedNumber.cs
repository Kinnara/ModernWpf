namespace ModernWpf.Controls
{
    internal readonly struct ParsedNumber
    {
        private static readonly ParsedNumber s_empty = Create(double.NaN, 0);
        
        public static ref readonly ParsedNumber Empty => ref s_empty;

        public static ParsedNumber Create(double value, int charLength) =>
            new ParsedNumber(value, charLength);

        public static bool IsEmpty(ParsedNumber value) =>
            value.CharLength == 0;

        public double Value { get; }

        public int CharLength { get; }

        public ParsedNumber(double value, int charLength)
        {
            Value = value;
            CharLength = charLength;
        }
    }
}
