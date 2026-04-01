using PrompterOne.Core.Models.Media;

namespace PrompterOne.Core.Abstractions;

public interface IMediaSceneService
{
    MediaSceneState State { get; }

    event EventHandler? Changed;

    SceneCameraSource AddCamera(string deviceId, string label);

    void RemoveCamera(string sourceId);

    void UpdateTransform(string sourceId, MediaSourceTransform transform);

    void SetIncludeInOutput(string sourceId, bool includeInOutput);

    void SetPrimaryMicrophone(string? deviceId, string? label = null);

    void UpsertAudioInput(AudioInputState inputState);

    void ApplyState(MediaSceneState state);

    void Reset();
}
