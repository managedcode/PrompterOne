using PrompterOne.Core.Models.Streaming;

namespace PrompterOne.Core.Abstractions;

public interface IStreamingOutputProvider
{
    string Id { get; }

    StreamingProviderKind Kind { get; }

    string DisplayName { get; }

    StreamingPublishDescriptor Describe(StreamingProfile profile);
}
