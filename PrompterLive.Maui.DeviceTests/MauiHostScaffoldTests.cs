namespace PrompterLive.Maui.DeviceTests;

public sealed class MauiHostScaffoldTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));

    [Fact]
    public void MauiProject_TargetsTheRequestedHybridPlatforms()
    {
        var project = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Maui", "PrompterLive.Maui.csproj"));

        Assert.Contains("net10.0-android", project, StringComparison.Ordinal);
        Assert.Contains("net10.0-ios", project, StringComparison.Ordinal);
        Assert.Contains("net10.0-maccatalyst", project, StringComparison.Ordinal);
        Assert.Contains("net10.0-windows10.0.19041.0", project, StringComparison.Ordinal);
    }

    [Fact]
    public void MauiProgram_WiresTheSharedBlazorApplicationLayer()
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Maui", "MauiProgram.cs"));

        Assert.Contains("AddPrompterLiveShared()", source, StringComparison.Ordinal);
        Assert.Contains("AddMauiBlazorWebView()", source, StringComparison.Ordinal);
        Assert.Contains("IFormFactor", source, StringComparison.Ordinal);
    }
}
