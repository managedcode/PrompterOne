using Microsoft.JSInterop;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Services;

public sealed class MediaRuntimeContractService(IJSRuntime jsRuntime) : IDisposable, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private bool _initializationAttempted;
    private Task<IJSObjectReference?>? _moduleTask;

    public async Task InitializeAsync()
    {
        if (_initializationAttempted)
        {
            return;
        }

        _initializationAttempted = true;

        var module = await GetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            MediaRuntimeContractInteropMethodNames.Configure,
            new
            {
                browserMediaInteropNamespace = AppMediaRuntime.BrowserMedia.InteropNamespace,
                captureCapabilitiesOverrideGlobalName = AppMediaRuntime.BrowserMedia.CaptureCapabilitiesOverrideGlobalName,
                concealDeviceIdentitySessionFlag = AppMediaRuntime.BrowserMedia.ConcealDeviceIdentitySessionFlag,
                contractProperty = AppMediaRuntime.Runtime.ContractProperty,
                defaultVdoNinjaBaseUrl = AppMediaRuntime.GoLive.DefaultVdoNinjaBaseUrl,
                defaultVdoNinjaStreamLabel = AppMediaRuntime.GoLive.DefaultVdoNinjaStreamLabel,
                goLiveMediaComposerNamespace = AppMediaRuntime.GoLive.MediaComposerNamespace,
                goLiveOutputNamespace = AppMediaRuntime.GoLive.OutputNamespace,
                goLiveOutputSupportNamespace = AppMediaRuntime.GoLive.OutputSupportNamespace,
                goLiveOutputVdoNinjaNamespace = AppMediaRuntime.GoLive.OutputVdoNinjaNamespace,
                goLiveRemoteSourcesNamespace = AppMediaRuntime.GoLive.RemoteSourcesNamespace,
                liveKitClientGlobalName = AppMediaRuntime.Vendor.LiveKitClientGlobalName,
                mediaHarnessEnabledProperty = AppMediaRuntime.Runtime.HarnessEnabledProperty,
                recordingFileHarnessGlobalName = AppMediaRuntime.BrowserMedia.RecordingFileHarnessGlobalName,
                remoteSourceSeedGlobalName = AppMediaRuntime.BrowserMedia.RemoteSourceSeedGlobalName,
                runtimeGlobalName = AppMediaRuntime.Runtime.GlobalName,
                syntheticHarnessGlobalName = AppMediaRuntime.BrowserMedia.SyntheticHarnessGlobalName,
                syntheticMetadataProperty = AppMediaRuntime.BrowserMedia.SyntheticMetadataProperty,
                vdoNinjaLegacyGlobalName = AppMediaRuntime.Vendor.VdoNinjaLegacyGlobalName,
                vdoNinjaSdkGlobalName = AppMediaRuntime.Vendor.VdoNinjaSdkGlobalName
            });
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is null)
        {
            return;
        }

        var module = await _moduleTask;
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }

    public void Dispose()
    {
    }

    private Task<IJSObjectReference?> GetModuleAsync() =>
        _moduleTask ??= ImportModuleAsync();

    private async Task<IJSObjectReference?> ImportModuleAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<IJSObjectReference>(
                MediaRuntimeContractInteropMethodNames.JSImportMethodName,
                MediaRuntimeContractInteropMethodNames.ModulePath);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }
}
