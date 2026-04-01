using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using PrompterOne.App;
using PrompterOne.App.Services;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Settings.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("PrompterOne", LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore.Components.WebAssembly.Rendering.WebAssemblyRenderer", LogLevel.Warning);

builder.Services.AddLocalization();
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IAppVersionProvider>(_ => AppVersionProviderFactory.CreateFromAssembly(typeof(Program).Assembly));
builder.Services.AddPrompterOneShared();

var host = builder.Build();
await BrowserCultureRuntime.ApplyPreferredCultureAsync(host.Services.GetRequiredService<IJSRuntime>());
await host.RunAsync();
