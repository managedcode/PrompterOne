using PrompterLive.Core.Models.Streaming;

namespace PrompterLive.Core.Abstractions;

public interface IStreamingOutputProvider
{
    string Id { get; }

    StreamingProviderKind Kind { get; }

    string DisplayName { get; }

    StreamingPublishDescriptor Describe(StreamingProfile profile);
}
