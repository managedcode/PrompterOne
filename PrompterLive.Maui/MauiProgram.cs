using Microsoft.Extensions.Logging;
using PrompterLive.Shared.Services;
using PrompterLive.Services;

namespace PrompterLive;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the PrompterLive.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddPrompterLiveShared();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
