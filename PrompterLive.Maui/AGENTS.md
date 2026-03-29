# AGENTS.md

## Purpose

`PrompterLive.Maui` is the Hybrid host shell that packages the shared Blazor UI for Android, iOS, Mac Catalyst, and Windows.

## Boundaries

- Keep feature logic in `PrompterLive.Core` or `PrompterLive.Shared`.
- Use this project for host wiring, assets, and device-specific service registration only.
- Keep the BlazorWebView loading the shared RCL assets without duplicating UI.

## Commands

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Maui/PrompterLive.Maui.csproj -f net10.0-android`

## Environment Note

- iOS and Mac Catalyst builds on this machine require Xcode `26.2`, but the installed version is `26.0.1`.
