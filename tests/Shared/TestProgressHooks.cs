namespace PrompterOne.Testing;

internal static class TestProgressHooks
{
    [BeforeEvery(HookType.Test)]
    public static void LogTestStart(TestContext context)
    {
        Console.WriteLine($"[RUN] {BuildLabel(context)}");
    }

    private static string BuildLabel(TestContext context)
    {
        var metadata = context.Metadata;
        var className = metadata.TestDetails.ClassType.Name;
        var displayName = string.IsNullOrWhiteSpace(metadata.DisplayName)
            ? metadata.TestName
            : metadata.DisplayName;

        return $"{className}.{displayName}";
    }
}
