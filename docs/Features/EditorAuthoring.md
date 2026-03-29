# Editor Authoring

## Intent

The `/editor` screen is a TPS-native authoring surface. The editable source remains the system of record, while the structure sidebar, metadata rail, status bar, and highlighted overlay stay synchronized with that source.

## Main Flow

```mermaid
flowchart LR
    Source["Raw TPS source textarea"]
    History["Undo/redo history"]
    FrontMatter["Front-matter metadata"]
    Structure["Segment/block structure editor"]
    Outline["Outline + status"]
    Highlight["Highlighted overlay"]
    Save["Autosave to script repository"]

    Source --> History
    Source --> FrontMatter
    Source --> Outline
    Source --> Highlight
    FrontMatter --> Source
    Structure --> Source
    Source --> Save
```

## Structure Editing Contract

```mermaid
sequenceDiagram
    participant User
    participant Sidebar as "Structure Inspector"
    participant Page as "EditorPage"
    participant Core as "TpsStructureEditor"
    participant Session as "ScriptSessionService"

    User->>Sidebar: Edit segment/block fields
    Sidebar->>Page: Updated view model
    Page->>Core: Rewrite TPS header safely
    Core-->>Page: Updated source + selection
    Page->>Session: Persist draft
    Session-->>Page: Recompiled workspace state
    Page-->>Sidebar: Refreshed structure editor state
```

## Current Behavior

- floating selection toolbar supports formatting actions and stays anchored to the selection
- active segment and block can be edited through the left sidebar inspector
- speed-offset metadata fields persist into front matter
- source edits refresh metadata, outline, and status
- metadata and structure edits rewrite the source rather than bypassing it

## Verification

- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`
