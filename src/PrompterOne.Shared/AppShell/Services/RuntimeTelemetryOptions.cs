namespace PrompterOne.Shared.Services;

public sealed record RuntimeTelemetryOptions(
    string GoogleAnalyticsMeasurementId,
    string ClarityProjectId,
    string SentryDsn,
    bool HostEnabled)
{
    public const string ClarityProjectIdKey = "ClarityProjectId";
    public const string ConfigurationSectionName = "RuntimeTelemetry";
    public const string GoogleAnalyticsMeasurementIdKey = "GoogleAnalyticsMeasurementId";
    public const string HostEnabledKey = "HostEnabled";
    public const string SentryDsnKey = "SentryDsn";
    public const string SectionSeparator = ":";

    public static RuntimeTelemetryOptions Disabled { get; } =
        new(string.Empty, string.Empty, string.Empty, HostEnabled: false);

    public bool SentryConfigured => !string.IsNullOrWhiteSpace(SentryDsn);

    public static string ClarityProjectIdPath =>
        string.Concat(ConfigurationSectionName, SectionSeparator, ClarityProjectIdKey);

    public static string GoogleAnalyticsMeasurementIdPath =>
        string.Concat(ConfigurationSectionName, SectionSeparator, GoogleAnalyticsMeasurementIdKey);

    public static string HostEnabledPath =>
        string.Concat(ConfigurationSectionName, SectionSeparator, HostEnabledKey);

    public static string SentryDsnPath =>
        string.Concat(ConfigurationSectionName, SectionSeparator, SentryDsnKey);
}
