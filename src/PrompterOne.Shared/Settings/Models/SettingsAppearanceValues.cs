namespace PrompterOne.Shared.Settings.Models;

public static class SettingsAppearanceValues
{
    public static class AccentColors
    {
        public const string Blue = "#60A5FA";
        public const string Emerald = "#34D399";
        public const string Gold = DefaultAccentColor;
        public const string Orange = "#FB923C";
        public const string Pink = "#F472B6";
        public const string Purple = "#A78BFA";
    }

    public static class TeleprompterFonts
    {
        public const string Georgia = "Georgia";
        public const string InterDefault = "Inter (Default)";
        public const string JetBrainsMono = "JetBrains Mono";
        public const string PlayfairDisplay = "Playfair Display";
        public const string SystemSerif = "System Serif";
    }

    public static class TextColors
    {
        public const string Amber = "#FDE68A";
        public const string Mint = "#6EE7B7";
        public const string WarmWhite = "#F5E6D0";
        public const string White = "#FFFFFF";
    }

    public const string CompactDensity = "compact";
    public const string DarkColorScheme = "dark";
    public const string DefaultAccentColor = "#C4A060";
    public const string DefaultDensity = "default";
    public const int DefaultTeleprompterFontSize = 48;
    public const string LightColorScheme = "light";
    public const string SpaciousDensity = "spacious";
    public const string SystemColorScheme = "system";
}
