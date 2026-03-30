# Go Live Runtime

## Scope

`Go Live` is the dedicated browser-only routing surface for arming live destinations.

It is separate from:

- `Settings`, which owns device setup such as camera selection, resolution, FPS, microphones, and audio sync
- `Teleprompter`, which owns the read experience and can run alongside the armed live configuration

## Main Flow

```mermaid
sequenceDiagram
    participant User
    participant Settings as "SettingsPage"
    participant GoLive as "GoLivePage"
    participant Preview as "GoLiveCameraPreviewCard"
    participant Routing as "GoLiveDestinationRouting"
    participant Studio as "StudioSettingsStore"
    participant Scene as "IMediaSceneService"
    participant Providers as "Streaming providers"
    participant Reader as "TeleprompterPage"

    User->>Settings: Configure camera, FPS, mic, sync
    Settings->>Studio: Persist device preferences
    Settings->>Scene: Persist scene cameras and audio bus
    User->>GoLive: Open Go Live
    GoLive->>Studio: Load live routing settings
    GoLive->>Scene: Load current scene sources
    GoLive->>Routing: Normalize destination source selections
    GoLive->>Preview: Mount the first included scene camera
    GoLive->>Providers: Describe LiveKit / VDO.Ninja / RTMP readiness
    User->>GoLive: Arm one or more destinations
    User->>GoLive: Select scene cameras per destination
    GoLive->>Studio: Persist output targets
    User->>Reader: Open teleprompter
    Reader->>Scene: Reuse same scene cameras under text
```

## Contracts

```mermaid
flowchart LR
    Page["GoLivePage"]
    Hero["GoLiveHero"]
    Preview["GoLiveCameraPreviewCard"]
    Program["GoLiveProgramFeedCard"]
    Sources["GoLiveSourcesCard"]
    TargetSources["GoLiveDestinationSourcePicker"]
    Studio["StreamStudioSettings"]
    Routing["GoLiveDestinationRouting"]
    Scene["MediaSceneState"]
    CameraInterop["CameraPreviewInterop"]
    LiveKit["LiveKitOutputProvider"]
    Vdo["VdoNinjaOutputProvider"]
    Rtmp["RtmpStreamingOutputProvider"]

    Page --> Hero
    Page --> Preview
    Page --> Program
    Page --> Sources
    Page --> TargetSources
    Page --> Studio
    Page --> Routing
    Page --> Scene
    Preview --> CameraInterop
    Page --> LiveKit
    Page --> Vdo
    Page --> Rtmp
```

## Rules

- `Settings` must not own live destination routing anymore.
- `Settings` must expose a visible CTA into `Go Live` so device setup and live routing stay discoverable as separate flows.
- `Go Live` may arm multiple destinations at the same time.
- `Go Live` must reuse the browser-composed scene and not invent a separate media graph.
- `Go Live` must show a live camera preview inside the program feed area, using the first included scene camera and falling back to the first visible scene camera.
- `Go Live` must show a stable empty preview state instead of mounting camera interop when the current scene has no cameras.
- each live destination must persist its own selected scene cameras, independent of the shared program feed source list
- legacy streaming settings must normalize to the current included program cameras so existing browser storage keeps working
- Camera source inclusion is persisted through `MediaSceneState`.
- Destination credentials and endpoints are persisted only in browser storage for this standalone runtime.
- Browser acceptance verifies `Go Live` preview and source switching against deterministic synthetic cameras, not only against static DOM state.
