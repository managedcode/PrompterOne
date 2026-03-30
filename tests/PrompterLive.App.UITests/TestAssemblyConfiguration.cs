[assembly: CollectionBehavior(MaxParallelThreads = PrompterLive.App.UITests.UiTestExecution.MaxParallelThreads)]

namespace PrompterLive.App.UITests;

internal static class UiTestExecution
{
    public const int MaxParallelThreads = 4;
}
