(function () {
    const floatingToolbarAnchorMinTopPx = 44;
    const offscreenMirrorLeftPx = -99999;
    const editorSurfaceNamespace = "editorSurface";
    const historyBindings = new WeakMap();
    const prompterLive = window.PrompterLive || (window.PrompterLive = {});

    prompterLive[editorSurfaceNamespace] = {
        focus(textarea) {
            if (!textarea) {
                return;
            }

            textarea.focus({ preventScroll: true });
        },

        bindHistoryShortcuts(textarea, dotNetRef) {
            if (!textarea || !dotNetRef) {
                return;
            }

            prompterLive[editorSurfaceNamespace].unbindHistoryShortcuts(textarea);

            const handler = event => {
                const key = (event.key || "").toLowerCase();
                const hasModifier = event.metaKey || event.ctrlKey;
                const isUndo = hasModifier && !event.shiftKey && key === "z";
                const isRedo = hasModifier && (key === "y" || (event.shiftKey && key === "z"));

                if (!isUndo && !isRedo) {
                    return;
                }

                event.preventDefault();
                dotNetRef.invokeMethodAsync("HandleHistoryShortcut", isRedo ? "redo" : "undo");
            };

            textarea.addEventListener("keydown", handler);
            historyBindings.set(textarea, handler);
        },

        syncScroll(textarea, overlay) {
            if (!textarea || !overlay) {
                return;
            }

            overlay.scrollTop = textarea.scrollTop;
            overlay.scrollLeft = textarea.scrollLeft;
        },

        getSelectionState(textarea) {
            return createEditorSelectionState(textarea);
        },

        setSelection(textarea, start, end) {
            if (!textarea) {
                return createEmptyEditorSelectionState();
            }

            textarea.focus();
            textarea.setSelectionRange(start, end);
            return createEditorSelectionState(textarea);
        },

        unbindHistoryShortcuts(textarea) {
            if (!textarea) {
                return;
            }

            const handler = historyBindings.get(textarea);
            if (!handler) {
                return;
            }

            textarea.removeEventListener("keydown", handler);
            historyBindings.delete(textarea);
        }
    };

    function createEmptyEditorSelectionState() {
        return {
            start: 0,
            end: 0,
            line: 1,
            column: 1,
            toolbarTop: 0,
            toolbarLeft: 0
        };
    }

    function createEditorSelectionState(textarea) {
        if (!textarea) {
            return createEmptyEditorSelectionState();
        }

        const start = textarea.selectionStart || 0;
        const end = textarea.selectionEnd || start;
        const prefix = textarea.value.slice(0, start);
        const prefixLines = prefix.split("\n");
        const line = prefixLines.length;
        const column = (prefixLines[prefixLines.length - 1] || "").length + 1;
        const coords = measureTextareaSelectionGeometry(textarea, start, end);

        return {
            start,
            end,
            line,
            column,
            toolbarTop: Math.max(floatingToolbarAnchorMinTopPx, coords.top),
            toolbarLeft: coords.left
        };
    }

    function measureTextareaSelectionGeometry(textarea, start, end) {
        if (start === end) {
            return measureTextareaCaretGeometry(textarea, end);
        }

        return measureTextareaRangeGeometry(textarea, start, end);
    }

    function measureTextareaCaretGeometry(textarea, index) {
        const style = window.getComputedStyle(textarea);
        const mirror = document.createElement("div");
        const span = document.createElement("span");
        const value = textarea.value;

        mirror.style.position = "absolute";
        mirror.style.visibility = "hidden";
        mirror.style.whiteSpace = style.whiteSpace;
        mirror.style.wordBreak = style.wordBreak;
        mirror.style.overflowWrap = style.overflowWrap;
        mirror.style.fontFamily = style.fontFamily;
        mirror.style.fontSize = style.fontSize;
        mirror.style.fontWeight = style.fontWeight;
        mirror.style.lineHeight = style.lineHeight;
        mirror.style.letterSpacing = style.letterSpacing;
        mirror.style.fontVariantLigatures = style.fontVariantLigatures;
        mirror.style.tabSize = style.tabSize;
        mirror.style.padding = style.padding;
        mirror.style.border = style.border;
        mirror.style.boxSizing = style.boxSizing;
        mirror.style.width = `${textarea.clientWidth}px`;
        mirror.style.left = `${offscreenMirrorLeftPx}px`;
        mirror.style.top = "0";

        mirror.textContent = value.slice(0, index);
        span.textContent = value.slice(index, index + 1) || " ";
        mirror.appendChild(span);
        document.body.appendChild(mirror);

        const top = textarea.offsetTop + span.offsetTop - textarea.scrollTop - getLineTopInsetPx(style);
        const left = textarea.offsetLeft + span.offsetLeft - textarea.scrollLeft;
        document.body.removeChild(mirror);

        return {
            top,
            left
        };
    }

    function measureTextareaRangeGeometry(textarea, start, end) {
        const style = window.getComputedStyle(textarea);
        const mirror = document.createElement("div");
        const selection = document.createElement("span");
        const value = textarea.value;

        mirror.style.position = "absolute";
        mirror.style.visibility = "hidden";
        mirror.style.whiteSpace = style.whiteSpace;
        mirror.style.wordBreak = style.wordBreak;
        mirror.style.overflowWrap = style.overflowWrap;
        mirror.style.fontFamily = style.fontFamily;
        mirror.style.fontSize = style.fontSize;
        mirror.style.fontWeight = style.fontWeight;
        mirror.style.lineHeight = style.lineHeight;
        mirror.style.letterSpacing = style.letterSpacing;
        mirror.style.fontVariantLigatures = style.fontVariantLigatures;
        mirror.style.tabSize = style.tabSize;
        mirror.style.padding = style.padding;
        mirror.style.border = style.border;
        mirror.style.boxSizing = style.boxSizing;
        mirror.style.width = `${textarea.clientWidth}px`;
        mirror.style.left = `${offscreenMirrorLeftPx}px`;
        mirror.style.top = "0";

        mirror.textContent = value.slice(0, start);
        selection.textContent = value.slice(start, end) || " ";
        mirror.appendChild(selection);
        document.body.appendChild(mirror);

        const mirrorRect = mirror.getBoundingClientRect();
        const selectionRect = selection.getBoundingClientRect();
        const selectionRects = selection.getClientRects();
        const firstRect = selectionRects[0] || selectionRect;
        const top = textarea.offsetTop + firstRect.top - mirrorRect.top - textarea.scrollTop - getLineTopInsetPx(style);
        const left = textarea.offsetLeft + selectionRect.left - mirrorRect.left - textarea.scrollLeft + (selectionRect.width / 2);

        document.body.removeChild(mirror);

        return {
            top,
            left
        };
    }

    function getLineTopInsetPx(style) {
        const lineHeight = Number.parseFloat(style.lineHeight);
        const fontSize = Number.parseFloat(style.fontSize);
        if (!Number.isFinite(lineHeight) || !Number.isFinite(fontSize)) {
            return 0;
        }

        return Math.max(0, lineHeight - fontSize);
    }
})();
