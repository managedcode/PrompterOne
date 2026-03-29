using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PrompterLive.Shared.Services;
using PrompterLive.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the PrompterLive.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddPrompterLiveShared();

await builder.Build().RunAsync();
