# AGENTS.md

## Purpose

`PrompterLive.Shared` is the shared Razor Class Library for pages, components, styles, browser interop wrappers, and app DI.

## Entry Points

- `Routes.razor`
- `Layout/MainLayout.razor`
- `Pages/*`
- `Components/*`
- `Services/PrompterLiveServiceCollectionExtensions.cs`
- `Services/Browser*`
- `wwwroot/app.css`
- `wwwroot/prompterlive.js`

## Boundaries

- Keep page state thin and delegate reusable logic to `PrompterLive.Core`.
- Do not reference MAUI-only APIs from this project.
- JS interop must stay inside service wrappers, not scattered through components.
- Preserve `data-testid` selectors on core actions and surfaces.

## Commands

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Shared/PrompterLive.Shared.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Shared.Tests/PrompterLive.Shared.Tests.csproj`

## Risks

- Shared pages are used by both WebAssembly and MAUI BlazorWebView. Avoid assumptions that only hold in one host.
- The visual language should continue to track the `new-design/index.html` reference.
