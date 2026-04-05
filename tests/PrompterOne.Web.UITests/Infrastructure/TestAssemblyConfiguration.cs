[assembly: CollectionBehavior(MaxParallelThreads = PrompterOne.Web.UITests.UiTestExecution.MaxParallelThreads)]

namespace PrompterOne.Web.UITests;

internal static class UiTestExecution
{
    public const int MaxParallelThreads = 2;
}
