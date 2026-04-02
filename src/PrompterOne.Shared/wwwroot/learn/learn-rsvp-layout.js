(function () {
    const interopNamespace = "LearnRsvpLayoutInterop";
    const falseString = "false";
    const pixelUnitSuffix = "px";
    const syncStates = new WeakMap();
    const trueString = "true";
    const zeroPixels = "0px";

    function setPixelProperty(target, propertyName, value) {
        const roundedValue = Math.round(value * 100) / 100;
        target.style.setProperty(propertyName, `${roundedValue}${pixelUnitSuffix}`);
    }

    function applyDefaultLayout(rowElement, focusLeftExtentPropertyName, focusRightExtentPropertyName) {
        rowElement.style.setProperty(focusLeftExtentPropertyName, zeroPixels);
        rowElement.style.setProperty(focusRightExtentPropertyName, zeroPixels);
    }

    function setLayoutReady(displayElement, layoutReadyAttributeName, isReady) {
        if (displayElement instanceof HTMLElement) {
            displayElement.setAttribute(layoutReadyAttributeName, isReady ? trueString : falseString);
        }
    }

    function syncLayoutNow(rowElement, focusElement, orpElement, focusLeftExtentPropertyName, focusRightExtentPropertyName) {
        if (!(focusElement instanceof HTMLElement) || !(orpElement instanceof HTMLElement)) {
            applyDefaultLayout(rowElement, focusLeftExtentPropertyName, focusRightExtentPropertyName);
            return false;
        }

        applyDefaultLayout(rowElement, focusLeftExtentPropertyName, focusRightExtentPropertyName);
        void rowElement.offsetWidth;

        const focusRect = focusElement.getBoundingClientRect();
        const orpRect = orpElement.getBoundingClientRect();
        const orpCenterPx = orpRect.left + (orpRect.width / 2);
        const focusLeftExtentPx = Math.max(orpCenterPx - focusRect.left, 0);
        const focusRightExtentPx = Math.max(focusRect.right - orpCenterPx, 0);

        setPixelProperty(rowElement, focusLeftExtentPropertyName, focusLeftExtentPx);
        setPixelProperty(rowElement, focusRightExtentPropertyName, focusRightExtentPx);
        return true;
    }

    function flushSync(state) {
        state.scheduled = false;

        const didSync = syncLayoutNow(
            state.rowElement,
            state.focusElement,
            state.orpElement,
            state.focusLeftExtentPropertyName,
            state.focusRightExtentPropertyName);

        if (didSync) {
            setLayoutReady(state.displayElement, state.layoutReadyAttributeName, true);
        }
    }

    function scheduleSync(state) {
        setLayoutReady(state.displayElement, state.layoutReadyAttributeName, false);

        if (state.scheduled) {
            return;
        }

        state.scheduled = true;
        window.requestAnimationFrame(() => flushSync(state));
    }

    function scheduleFontReadySync(state) {
        const displayElement = state.displayElement;
        if (!(displayElement instanceof HTMLElement) ||
            displayElement.getAttribute(state.fontSyncReadyAttributeName) === trueString) {
            return;
        }

        displayElement.setAttribute(state.fontSyncReadyAttributeName, trueString);

        if (!document.fonts?.ready) {
            return;
        }

        void document.fonts.ready.then(() => {
            if (displayElement.isConnected && state.rowElement?.isConnected) {
                scheduleSync(state);
            }
        });
    }

    function createSyncState(
        displayElement,
        rowElement,
        focusElement,
        orpElement,
        focusLeftExtentPropertyName,
        focusRightExtentPropertyName,
        layoutReadyAttributeName,
        fontSyncReadyAttributeName) {
        const state = {
            displayElement,
            fontSyncReadyAttributeName,
            focusElement,
            focusLeftExtentPropertyName,
            focusRightExtentPropertyName,
            layoutReadyAttributeName,
            orpElement,
            rowElement,
            scheduled: false
        };

        state.mutationObserver = new MutationObserver(() => scheduleSync(state));
        state.mutationObserver.observe(rowElement, {
            characterData: true,
            childList: true,
            subtree: true
        });

        if (typeof ResizeObserver === "function") {
            state.resizeObserver = new ResizeObserver(() => scheduleSync(state));
            state.resizeObserver.observe(displayElement);
            state.resizeObserver.observe(rowElement);
        }

        syncStates.set(rowElement, state);
        return state;
    }

    function updateSyncState(
        state,
        displayElement,
        rowElement,
        focusElement,
        orpElement,
        focusLeftExtentPropertyName,
        focusRightExtentPropertyName,
        layoutReadyAttributeName,
        fontSyncReadyAttributeName) {
        state.displayElement = displayElement;
        state.fontSyncReadyAttributeName = fontSyncReadyAttributeName;
        state.focusElement = focusElement;
        state.focusLeftExtentPropertyName = focusLeftExtentPropertyName;
        state.focusRightExtentPropertyName = focusRightExtentPropertyName;
        state.layoutReadyAttributeName = layoutReadyAttributeName;
        state.orpElement = orpElement;
        state.rowElement = rowElement;
    }

    window[interopNamespace] = {
        syncLayout(
            displayElement,
            rowElement,
            focusElement,
            orpElement,
            focusLeftExtentPropertyName,
            focusRightExtentPropertyName,
            layoutReadyAttributeName,
            fontSyncReadyAttributeName) {
            if (!(displayElement instanceof HTMLElement) || !(rowElement instanceof HTMLElement)) {
                return;
            }

            const state = syncStates.get(rowElement) ?? createSyncState(
                displayElement,
                rowElement,
                focusElement,
                orpElement,
                focusLeftExtentPropertyName,
                focusRightExtentPropertyName,
                layoutReadyAttributeName,
                fontSyncReadyAttributeName);

            updateSyncState(
                state,
                displayElement,
                rowElement,
                focusElement,
                orpElement,
                focusLeftExtentPropertyName,
                focusRightExtentPropertyName,
                layoutReadyAttributeName,
                fontSyncReadyAttributeName);

            scheduleSync(state);
            scheduleFontReadySync(state);
        }
    };
})();
