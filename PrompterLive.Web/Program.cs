using PrompterLive.Web.Components;
using PrompterLive.Shared.Services;
using PrompterLive.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the PrompterLive.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddPrompterLiveShared();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(PrompterLive.Shared._Imports).Assembly,
        typeof(PrompterLive.Web.Client._Imports).Assembly);

app.Run();

public partial class Program;
