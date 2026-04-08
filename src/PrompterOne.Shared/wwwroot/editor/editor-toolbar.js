const defaultScrollDelta = 320;

export function scrollBy(host, delta = defaultScrollDelta) {
    if (!(host instanceof HTMLElement)) {
        return;
    }

    host.scrollLeft += Number.isFinite(delta) ? delta : defaultScrollDelta;
}
