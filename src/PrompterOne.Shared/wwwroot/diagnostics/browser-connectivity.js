export function getOnlineStatus() {
    if (typeof navigator !== "object" || navigator === null) {
        return true;
    }

    return navigator.onLine === true;
}
