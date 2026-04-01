(function () {
    const interopNamespace = "LearnRsvpLayoutInterop";
    const focusSelector = ".rsvp-focus";
    const orpSelector = ".rsvp-focus-orp";
    const focusLeftExtentPropertyName = "--rsvp-focus-left-extent";
    const focusRightExtentPropertyName = "--rsvp-focus-right-extent";
    const fontSyncReadyAttributeName = "data-rsvp-layout-font-sync-ready";
    const pixelUnitSuffix = "px";
    const zeroPixels = "0px";

    function setPixelProperty(target, propertyName, value) {
        const roundedValue = Math.round(value * 100) / 100;
        target.style.setProperty(propertyName, `${roundedValue}${pixelUnitSuffix}`);
    }

    function applyDefaultLayout(rowElement) {
        rowElement.style.setProperty(focusLeftExtentPropertyName, zeroPixels);
        rowElement.style.setProperty(focusRightExtentPropertyName, zeroPixels);
    }

    function syncLayoutNow(rowElement) {
        const focusElement = rowElement.querySelector(focusSelector);
        const orpElement = focusElement?.querySelector(orpSelector);

        if (!(focusElement instanceof HTMLElement) || !(orpElement instanceof HTMLElement)) {
            applyDefaultLayout(rowElement);
            return;
        }

        applyDefaultLayout(rowElement);
        void rowElement.offsetWidth;

        const focusRect = focusElement.getBoundingClientRect();
        const orpRect = orpElement.getBoundingClientRect();
        const orpCenterPx = orpRect.left + (orpRect.width / 2);
        const focusLeftExtentPx = Math.max(orpCenterPx - focusRect.left, 0);
        const focusRightExtentPx = Math.max(focusRect.right - orpCenterPx, 0);

        setPixelProperty(rowElement, focusLeftExtentPropertyName, focusLeftExtentPx);
        setPixelProperty(rowElement, focusRightExtentPropertyName, focusRightExtentPx);
    }

    function scheduleFontReadySync(rowElement) {
        if (rowElement.getAttribute(fontSyncReadyAttributeName) === "true") {
            return;
        }

        rowElement.setAttribute(fontSyncReadyAttributeName, "true");

        if (!document.fonts?.ready) {
            return;
        }

        void document.fonts.ready.then(() => {
            if (rowElement.isConnected) {
                syncLayoutNow(rowElement);
            }
        });
    }

    window[interopNamespace] = {
        syncLayout(rowElement) {
            if (!(rowElement instanceof HTMLElement)) {
                return;
            }

            syncLayoutNow(rowElement);
            scheduleFontReadySync(rowElement);
        }
    };
})();
