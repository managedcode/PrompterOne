using TUnit.Core.Interfaces;

namespace PrompterOne.Testing;

/// <summary>
/// Provides environment-aware parallel test limits using standard CI markers.
/// </summary>
public abstract class EnvironmentAwareParallelLimitBase : IParallelLimit
{
    private const string AzurePipelinesEnv = "TF_BUILD";
    private const string CiEnv = "CI";
    private const string GitHubActionsEnv = "GITHUB_ACTIONS";
    private const string NumericTrueValue = "1";

    protected virtual int CiLimit { get; } = 4;
    protected virtual int LocalLimit { get; } = 6;

    public int Limit => ResolveLimit();

    protected int ResolveLimit() =>
        IsCiEnvironment()
            ? CiLimit
            : LocalLimit;

    private static bool IsCiEnvironment() =>
        IsEnabled(Environment.GetEnvironmentVariable(CiEnv))
        || IsEnabled(Environment.GetEnvironmentVariable(AzurePipelinesEnv))
        || IsEnabled(Environment.GetEnvironmentVariable(GitHubActionsEnv));

    private static bool IsEnabled(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, NumericTrueValue, StringComparison.OrdinalIgnoreCase);
    }
}
