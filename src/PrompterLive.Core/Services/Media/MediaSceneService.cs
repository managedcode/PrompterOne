using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Media;

namespace PrompterLive.Core.Services.Media;

public sealed class MediaSceneService : IMediaSceneService
{
    public MediaSceneState State { get; private set; } = MediaSceneState.Empty;

    public event EventHandler? Changed;

    public SceneCameraSource AddCamera(string deviceId, string label)
    {
        var source = new SceneCameraSource(
            SourceId: $"cam-{Guid.NewGuid():N}",
            DeviceId: deviceId,
            Label: label,
            Transform: new MediaSourceTransform(
                X: State.Cameras.Count switch
                {
                    0 => 0.82,
                    1 => 0.18,
                    _ => 0.5
                },
                Y: State.Cameras.Count > 1 ? 0.18 : 0.82,
                Width: 0.28,
                Height: 0.28,
                ZIndex: State.Cameras.Count + 1));

        State = State with
        {
            Cameras = State.Cameras.Concat([source]).ToList()
        };

        NotifyChanged();
        return source;
    }

    public void RemoveCamera(string sourceId)
    {
        State = State with
        {
            Cameras = State.Cameras.Where(camera => !string.Equals(camera.SourceId, sourceId, StringComparison.Ordinal)).ToList()
        };

        NotifyChanged();
    }

    public void UpdateTransform(string sourceId, MediaSourceTransform transform)
    {
        State = State with
        {
            Cameras = State.Cameras
                .Select(camera => string.Equals(camera.SourceId, sourceId, StringComparison.Ordinal)
                    ? camera with { Transform = transform }
                    : camera)
                .ToList()
        };

        NotifyChanged();
    }

    public void SetIncludeInOutput(string sourceId, bool includeInOutput)
    {
        var camera = State.Cameras.FirstOrDefault(item => string.Equals(item.SourceId, sourceId, StringComparison.Ordinal));
        if (camera is null)
        {
            return;
        }

        UpdateTransform(sourceId, camera.Transform with { IncludeInOutput = includeInOutput });
    }

    public void SetPrimaryMicrophone(string? deviceId, string? label = null)
    {
        State = State with
        {
            PrimaryMicrophoneId = deviceId,
            PrimaryMicrophoneLabel = label
        };

        NotifyChanged();
    }

    public void UpsertAudioInput(AudioInputState inputState)
    {
        var inputs = State.AudioBus.Inputs.ToList();
        var existingIndex = inputs.FindIndex(item => string.Equals(item.DeviceId, inputState.DeviceId, StringComparison.Ordinal));
        if (existingIndex >= 0)
        {
            inputs[existingIndex] = inputState;
        }
        else
        {
            inputs.Add(inputState);
        }

        State = State with
        {
            AudioBus = State.AudioBus with { Inputs = inputs }
        };

        NotifyChanged();
    }

    public void ApplyState(MediaSceneState state)
    {
        State = state;
        NotifyChanged();
    }

    public void Reset()
    {
        State = MediaSceneState.Empty;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
