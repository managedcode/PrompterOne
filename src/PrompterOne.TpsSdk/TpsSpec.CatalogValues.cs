namespace ManagedCode.Tps;

public static partial class TpsSpec
{
    public static class SpeedOffsetValues
    {
        public const int Xslow = -40;
        public const int Slow = -20;
        public const int Fast = 25;
        public const int Xfast = 50;
    }

    public static class EmotionPaletteValues
    {
        public static readonly EmotionPalette Neutral = new(PaletteHex.AccentBlue, PaletteHex.TextSlate900, PaletteHex.BackgroundBlue400);
        public static readonly EmotionPalette Warm = new(PaletteHex.AccentOrange600, PaletteHex.TextStone900, PaletteHex.BackgroundOrange300);
        public static readonly EmotionPalette Professional = new(PaletteHex.AccentBlue700, PaletteHex.TextSlate900, PaletteHex.BackgroundBlue300);
        public static readonly EmotionPalette Focused = new(PaletteHex.AccentGreen700, PaletteHex.TextGreen950, PaletteHex.BackgroundGreen300);
        public static readonly EmotionPalette Concerned = new(PaletteHex.AccentRed700, PaletteHex.TextRose950, PaletteHex.BackgroundRed300);
        public static readonly EmotionPalette Urgent = new(PaletteHex.AccentRed600, PaletteHex.TextWhiteRose, PaletteHex.BackgroundRed300);
        public static readonly EmotionPalette Motivational = new(PaletteHex.AccentViolet600, PaletteHex.TextWhite, PaletteHex.BackgroundViolet300);
        public static readonly EmotionPalette Excited = new(PaletteHex.AccentPink600, PaletteHex.TextWhitePink, PaletteHex.BackgroundPink300);
        public static readonly EmotionPalette Happy = new(PaletteHex.AccentAmber600, PaletteHex.TextStone900, PaletteHex.BackgroundAmber300);
        public static readonly EmotionPalette Sad = new(PaletteHex.AccentIndigo600, PaletteHex.TextIndigo50, PaletteHex.BackgroundIndigo300);
        public static readonly EmotionPalette Calm = new(PaletteHex.AccentTeal700, PaletteHex.TextTeal50, PaletteHex.BackgroundTeal200);
        public static readonly EmotionPalette Energetic = new(PaletteHex.AccentOrange700, PaletteHex.TextOrange50, PaletteHex.BackgroundOrange300);
    }
}
