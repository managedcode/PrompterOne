let isInitialized = false;
let isOnline = true;

function readNavigatorOnlineStatus() {
    if (typeof navigator !== "object" || navigator === null) {
        return true;
    }

    return navigator.onLine !== false;
}

function handleOnline() {
    isOnline = true;
}

function handleOffline() {
    isOnline = false;
}

function initialize() {
    if (isInitialized) {
        return;
    }

    isOnline = readNavigatorOnlineStatus();

    if (typeof window === "object" && window !== null) {
        window.addEventListener("online", handleOnline);
        window.addEventListener("offline", handleOffline);
    }

    isInitialized = true;
}

export function dispose() {
    if (!isInitialized) {
        return;
    }

    if (typeof window === "object" && window !== null) {
        window.removeEventListener("online", handleOnline);
        window.removeEventListener("offline", handleOffline);
    }

    isInitialized = false;
}

export function getOnlineStatus() {
    initialize();
    return isOnline;
}
