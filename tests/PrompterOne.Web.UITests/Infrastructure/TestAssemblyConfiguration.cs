[assembly: ParallelLimiter<PrompterOne.Web.UITests.UiTestParallelLimit>]

namespace PrompterOne.Web.UITests;

public sealed class UiTestParallelLimit : PrompterOne.Testing.EnvironmentAwareParallelLimitBase
{
    protected override int CiLimit { get; } = 2;
    protected override int LocalLimit { get; } = UiTestParallelization.DefaultWorkerLimit;
}

internal static class UiTestParallelization
{
    public const int DefaultWorkerLimit = 4;
    public const string EditorAuthoringConstraintKey = nameof(EditorAuthoringConstraintKey);
    public const string EditorPerformanceConstraintKey = nameof(EditorPerformanceConstraintKey);
}
