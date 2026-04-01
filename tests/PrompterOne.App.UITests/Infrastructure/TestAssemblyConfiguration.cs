[assembly: CollectionBehavior(MaxParallelThreads = PrompterOne.App.UITests.UiTestExecution.MaxParallelThreads)]

namespace PrompterOne.App.UITests;

internal static class UiTestExecution
{
    public const int MaxParallelThreads = 2;
}
