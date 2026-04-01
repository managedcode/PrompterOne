(function () {
    const disposeMethodName = "dispose";
    const initializeMethodName = "initialize";
    const invokeFailureLogPrefix = "[PrompterOneCrossTabInterop] Failed to deliver message to .NET listener.";
    const namespace = "PrompterOneCrossTabInterop";
    const publishMethodName = "publish";
    const receiveMethodName = "ReceiveAsync";
    const registry = new Map();

    function resolveEntry(channelName) {
        if (typeof BroadcastChannel === "undefined") {
            return null;
        }

        let entry = registry.get(channelName);
        if (entry) {
            return entry;
        }

        const channel = new BroadcastChannel(channelName);
        entry = {
            channel,
            listeners: new Set()
        };

        channel.onmessage = async function (event) {
            const payload = event?.data ?? null;
            if (!payload) {
                return;
            }

            const listeners = Array.from(entry.listeners);
            for (const listener of listeners) {
                try {
                    await listener.invokeMethodAsync(receiveMethodName, payload);
                }
                catch (error) {
                    console.warn(invokeFailureLogPrefix, error);
                }
            }
        };

        registry.set(channelName, entry);
        return entry;
    }

    window[namespace] = {
        [disposeMethodName]: function (channelName, dotNetHelper) {
            const entry = registry.get(channelName);
            if (!entry) {
                return;
            }

            entry.listeners.delete(dotNetHelper);
            if (entry.listeners.size !== 0) {
                return;
            }

            entry.channel.close();
            registry.delete(channelName);
        },

        [initializeMethodName]: function (channelName, dotNetHelper) {
            const entry = resolveEntry(channelName);
            if (!entry) {
                return false;
            }

            entry.listeners.add(dotNetHelper);
            return true;
        },

        [publishMethodName]: function (channelName, message) {
            const entry = resolveEntry(channelName);
            if (!entry) {
                return false;
            }

            entry.channel.postMessage(message);
            return true;
        }
    };
})();
