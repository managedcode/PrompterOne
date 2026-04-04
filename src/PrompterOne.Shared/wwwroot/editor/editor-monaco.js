const cssClassPrefix = "po";
const largeDraftDecorationCharacterThreshold = 16000;
const largeDraftDecorationViewportLinePadding = 24;
const frontMatterDelimiter = "---";
const emptyValue = "";
const hostStates = new WeakMap();
const numericWpmRegex = /^(?<wpm>\d+)\s*WPM$/i;
const tagPattern = /\[[^[\]]+\]/g;
const scopeKindRoot = "root";
const scopeKindNeutral = "neutral";
const scopeKindStyle = "style";
const scopeKindWpm = "wpm";
const scopeKindPronunciation = "pronunciation";
const emotionClasses = new Map([
    ["warm", `${cssClassPrefix}-inline-emotion-warm`],
    ["concerned", `${cssClassPrefix}-inline-emotion-concerned`],
    ["focused", `${cssClassPrefix}-inline-emotion-focused`],
    ["motivational", `${cssClassPrefix}-inline-emotion-motivational`],
    ["neutral", `${cssClassPrefix}-inline-emotion-neutral`],
    ["urgent", `${cssClassPrefix}-inline-emotion-urgent`],
    ["happy", `${cssClassPrefix}-inline-emotion-happy`],
    ["excited", `${cssClassPrefix}-inline-emotion-excited`],
    ["sad", `${cssClassPrefix}-inline-emotion-sad`],
    ["calm", `${cssClassPrefix}-inline-emotion-calm`],
    ["energetic", `${cssClassPrefix}-inline-emotion-energetic`],
    ["professional", `${cssClassPrefix}-inline-emotion-professional`]
]);
const volumeClasses = new Map([
    ["loud", `${cssClassPrefix}-inline-loud`],
    ["soft", `${cssClassPrefix}-inline-soft`],
    ["whisper", `${cssClassPrefix}-inline-whisper`]
]);
const deliveryClasses = new Map([
    ["aside", `${cssClassPrefix}-inline-delivery-aside`],
    ["rhetorical", `${cssClassPrefix}-inline-delivery-rhetorical`],
    ["sarcasm", `${cssClassPrefix}-inline-delivery-sarcasm`],
    ["building", `${cssClassPrefix}-inline-delivery-building`]
]);
const speedClasses = new Map([
    ["xslow", `${cssClassPrefix}-inline-speed-xslow`],
    ["slow", `${cssClassPrefix}-inline-speed-slow`],
    ["normal", null],
    ["fast", `${cssClassPrefix}-inline-speed-fast`],
    ["xfast", `${cssClassPrefix}-inline-speed-xfast`]
]);
const headerEmotionTags = new Set(emotionClasses.keys());
const monacoDefaults = Object.freeze({
    automaticLayout: true,
    contextmenu: false,
    cursorBlinking: "smooth",
    editContext: false,
    fixedOverflowWidgets: true,
    folding: false,
    fontFamily: "var(--mono)",
    fontLigatures: false,
    fontSize: 16,
    glyphMargin: false,
    letterSpacing: 0,
    lineDecorationsWidth: 0,
    lineNumbers: "off",
    lineNumbersMinChars: 0,
    lineHeight: 32,
    minimap: { enabled: false },
    overviewRulerBorder: false,
    overviewRulerLanes: 0,
    padding: { top: 40, bottom: 40 },
    quickSuggestions: true,
    renderLineHighlight: "none",
    roundedSelection: true,
    scrollBeyondLastLine: false,
    scrollbar: {
        alwaysConsumeMouseWheel: false,
        horizontal: "hidden",
        vertical: "auto",
        useShadows: false
    },
    suggestOnTriggerCharacters: true,
    tabSize: 4,
    trimAutoWhitespace: false,
    wordWrap: "on",
    wrappingIndent: "same"
});

let stylesheetPromise;
let loaderPromise;
let monacoPromise;
let themeObserverRegistered = false;

export async function initializeEditor(host, proxy, semanticSnapshot, dotNetRef, options) {
    if (!host || !proxy || !dotNetRef || !options) {
        return false;
    }

    const monaco = await loadMonacoAsync(options);
    const existingState = hostStates.get(host);
    if (existingState) {
        existingState.proxy = proxy;
        existingState.semanticSnapshot = semanticSnapshot;
        existingState.dotNetRef = dotNetRef;
        existingState.options = options;
        await syncEditorState(host, proxy.value ?? emptyValue, proxy.selectionStart ?? 0, proxy.selectionEnd ?? 0);
        return true;
    }

    const editor = monaco.editor.create(host, {
        ...monacoDefaults,
        language: options.languageId,
        placeholder: options.placeholder,
        theme: resolveThemeName(options),
        value: proxy.value ?? emptyValue
    });

    const state = {
        decorationCollection: editor.createDecorationsCollection(),
        decorationFrameId: 0,
        dotNetRef,
        editor,
        host,
        monaco,
        options,
        proxy,
        semanticSnapshot,
        suppressProxySelection: false,
        suppressSelectionNotification: false,
        suppressTextNotification: false,
        subscriptions: []
    };

    state.subscriptions.push(
        editor.onDidChangeModelContent(() => onEditorContentChanged(state)),
        editor.onDidChangeCursorSelection(() => notifySelectionChanged(state, false)),
        editor.onDidScrollChange(() => {
            if (shouldUseVisibleRangeDecorations(state)) {
                scheduleDecorations(state);
            }

            if (!editor.getSelection()?.isEmpty()) {
                notifySelectionChanged(state, false);
            }
        }),
        editor.onMouseUp(() => notifySelectionChanged(state, true))
    );

    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyZ, () => notifyHistoryRequested(state, "undo"));
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyY, () => notifyHistoryRequested(state, "redo"));
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.KeyZ, () => notifyHistoryRequested(state, "redo"));

    const proxySelectionHandler = () => {
        if (state.suppressProxySelection) {
            return;
        }

        applySelection(state, proxy.selectionStart ?? 0, proxy.selectionEnd ?? 0, false);
    };

    proxy.addEventListener("select", proxySelectionHandler);
    proxy.addEventListener("keyup", proxySelectionHandler);
    state.proxySelectionHandler = proxySelectionHandler;

    hostStates.set(host, state);
    host.setAttribute(options.editorEngineAttributeName, options.editorEngineAttributeValue);
    host.setAttribute(options.editorReadyAttributeName, "true");

    syncProxyFromEditor(state);
    scheduleDecorations(state);
    ensureHarness(options);
    ensureThemeObserver(monaco, options);
    return true;
}

export async function syncEditorState(host, text, selectionStart, selectionEnd) {
    const state = hostStates.get(host);
    if (!state) {
        return;
    }

    const nextText = text ?? emptyValue;
    state.suppressTextNotification = true;
    state.suppressSelectionNotification = true;

    const model = state.editor.getModel();
    if (model && model.getValue() !== nextText) {
        model.setValue(nextText);
    }

    applySelection(state, selectionStart ?? 0, selectionEnd ?? 0, false);
    state.suppressTextNotification = false;
    state.suppressSelectionNotification = false;
    syncProxyFromEditor(state);
    renderSemanticSnapshot(state, nextText);
    scheduleDecorations(state);
}

export function getSelectionState(host) {
    const state = hostStates.get(host);
    return state ? createSelectionState(state) : createEmptySelectionState();
}

export function setSelection(host, start, end) {
    const state = hostStates.get(host);
    if (!state) {
        return createEmptySelectionState();
    }

    applySelection(state, start ?? 0, end ?? 0, true);
    notifySelectionChanged(state, false);
    return createSelectionState(state);
}

export function disposeEditor(host) {
    const state = hostStates.get(host);
    if (!state) {
        return;
    }

    if (state.decorationFrameId) {
        cancelAnimationFrame(state.decorationFrameId);
    }

    state.proxy.removeEventListener("select", state.proxySelectionHandler);
    state.proxy.removeEventListener("keyup", state.proxySelectionHandler);
    state.decorationCollection.clear();
    for (const subscription of state.subscriptions) {
        subscription.dispose();
    }

    state.editor.dispose();
    host.removeAttribute(state.options.editorReadyAttributeName);
    hostStates.delete(host);
}

async function loadMonacoAsync(options) {
    if (monacoPromise) {
        return monacoPromise;
    }

    await ensureStylesheetAsync(options.monacoStylesheetPath);
    await ensureLoaderAsync(options.monacoLoaderPath);
    window.require.config({ paths: { vs: options.monacoVsPath } });
    window.MonacoEnvironment = { baseUrl: options.monacoVsPath };

    monacoPromise = new Promise((resolve, reject) => {
        window.require(["vs/editor/editor.main"], monaco => {
            ensureTpsLanguage(monaco, options);
            resolve(monaco);
        }, reject);
    });

    return monacoPromise;
}

function ensureStylesheetAsync(stylesheetPath) {
    if (!stylesheetPromise) {
        stylesheetPromise = new Promise((resolve, reject) => {
            const existing = document.querySelector(`link[data-monaco-stylesheet="${stylesheetPath}"]`);
            if (existing) {
                resolve();
                return;
            }

            const link = document.createElement("link");
            link.rel = "stylesheet";
            link.href = stylesheetPath;
            link.dataset.monacoStylesheet = stylesheetPath;
            link.onload = () => resolve();
            link.onerror = () => reject(new Error(`Unable to load Monaco stylesheet: ${stylesheetPath}`));
            document.head.appendChild(link);
        });
    }

    return stylesheetPromise;
}

function ensureLoaderAsync(loaderPath) {
    if (!loaderPromise) {
        loaderPromise = new Promise((resolve, reject) => {
            if (window.require) {
                resolve();
                return;
            }

            const script = document.createElement("script");
            script.src = loaderPath;
            script.async = true;
            script.dataset.monacoLoader = loaderPath;
            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Unable to load Monaco loader: ${loaderPath}`));
            document.head.appendChild(script);
        });
    }

    return loaderPromise;
}

function ensureTpsLanguage(monaco, options) {
    if (monaco.languages.getLanguages().some(language => language.id === options.languageId)) {
        applyResolvedTheme(monaco, options);
        return;
    }

    monaco.languages.register({ id: options.languageId });
    monaco.languages.setLanguageConfiguration(options.languageId, {
        autoClosingPairs: [{ open: "[", close: "]" }],
        brackets: [["[", "]"]],
        surroundingPairs: [{ open: "[", close: "]" }]
    });
    monaco.languages.setMonarchTokensProvider(options.languageId, {
        tokenizer: {
            root: [
                [/^---$/, "frontmatter.delimiter"],
                [/^([A-Za-z0-9_]+)(:)(\s*)(.+)$/, ["frontmatter.key", "delimiter", "white", "frontmatter.value"]],
                [/^(##)(\s+)(\[.*\])$/, ["header.segment.hash", "white", "header.segment.body"]],
                [/^(###)(\s+)(\[.*\])$/, ["header.block.hash", "white", "header.block.body"]],
                [/\[pause:[^\]]+\]/i, "pause.timed"],
                [/\/\//, "pause.long"],
                [/\//, "pause.short"],
                [/\[(emphasis|highlight|strong|bold|loud|soft|whisper|warm|concerned|focused|motivational|neutral|urgent|happy|excited|sad|calm|energetic|professional|xslow|slow|fast|xfast|aside|rhetorical|sarcasm|building|stress)\]/i, "cue.open"],
                [/\[\/(emphasis|highlight|strong|bold|loud|soft|whisper|warm|concerned|focused|motivational|neutral|urgent|happy|excited|sad|calm|energetic|professional|xslow|slow|fast|xfast|aside|rhetorical|sarcasm|building|stress)\]/i, "cue.close"],
                [/\[\d+\s*WPM\]/i, "wpm.badge"],
                [/\[[^\]]+\]/, "meta.tag"]
            ]
        }
    });
    monaco.languages.registerCompletionItemProvider(options.languageId, createCompletionProvider(monaco));
    applyResolvedTheme(monaco, options);
}

function createCompletionProvider(monaco) {
    return {
        triggerCharacters: ["/", "["],
        provideCompletionItems(model, position) {
            const word = model.getWordUntilPosition(position);
            const range = new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn);
            return {
                suggestions: [
                    createSuggestion(monaco, "/", " / ", "Short pause", range),
                    createSuggestion(monaco, "//", " //", "Beat pause", range),
                    createSuggestion(monaco, "[pause:2s]", "[pause:2s]", "Timed pause", range),
                    createSuggestion(monaco, "[emphasis]text[/emphasis]", "[emphasis]${1:text}[/emphasis]", "Emphasis wrapper", range, true),
                    createSuggestion(monaco, "[highlight]text[/highlight]", "[highlight]${1:text}[/highlight]", "Highlight wrapper", range, true),
                    createSuggestion(monaco, "## [Segment|140WPM|focused]", "## [${1:Segment Name}|140WPM|focused]", "Segment header", range, true),
                    createSuggestion(monaco, "### [Block|140WPM|professional]", "### [${1:Block Name}|140WPM|professional]", "Block header", range, true)
                ]
            };
        }
    };
}

function createSuggestion(monaco, label, insertText, detail, range, snippet = false) {
    return {
        detail,
        insertText,
        insertTextRules: snippet ? monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet : undefined,
        kind: monaco.languages.CompletionItemKind.Snippet,
        label,
        range
    };
}

function ensureHarness(options) {
    if (window[options.browserHarnessGlobalName]) {
        return;
    }

    window[options.browserHarnessGlobalName] = {
        focus: testId => {
            const state = getRequiredHarnessState(testId);
            state.editor.focus();
            return createHarnessState(state, options);
        },
        getState: testId => {
            const state = getOptionalHarnessState(testId);
            return state ? createHarnessState(state, options) : null;
        },
        setSelection: (testId, start, end, revealSelection = true) => {
            const state = getRequiredHarnessState(testId);
            applySelection(state, start ?? 0, end ?? 0, revealSelection !== false);
            notifySelectionChanged(state, false);
            return createSelectionState(state);
        },
        setText: (testId, text) => {
            const state = getRequiredHarnessState(testId);
            const model = state.editor.getModel();
            const nextText = text ?? emptyValue;
            if (model && model.getValue() !== nextText) {
                model.setValue(nextText);
            }

            return createHarnessState(state, options);
        }
    };
}

function createHarnessState(state, options) {
    const model = state.editor.getModel();
    return {
        decorationClasses: model?.getAllDecorations().map(decoration =>
            decoration.options.inlineClassName || decoration.options.lineClassName || "").filter(Boolean) ?? [],
        engine: state.host.getAttribute(options.editorEngineAttributeName) ?? emptyValue,
        languageId: model?.getLanguageId() ?? emptyValue,
        lineCount: model?.getLineCount() ?? 0,
        ready: state.host.getAttribute(options.editorReadyAttributeName) === "true",
        scrollTop: state.editor.getScrollTop(),
        selection: createSelectionState(state),
        text: state.editor.getValue()
    };
}

function getOptionalHarnessState(testId) {
    const host = document.querySelector(`[data-testid="${testId}"]`);
    return host ? hostStates.get(host) : null;
}

function getRequiredHarnessState(testId) {
    const state = getOptionalHarnessState(testId);
    if (!state) {
        throw new Error(`Unable to resolve Monaco editor state for test id "${testId}".`);
    }

    return state;
}

function ensureThemeObserver(monaco, options) {
    if (themeObserverRegistered) {
        return;
    }

    themeObserverRegistered = true;
    const observer = new MutationObserver(() => applyResolvedTheme(monaco, options));
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ["class", "data-theme"] });
    if (document.body) {
        observer.observe(document.body, { attributes: true, attributeFilter: ["class"] });
    }
}

function applyResolvedTheme(monaco, options) {
    monaco.editor.defineTheme(options.darkThemeName, createThemeData(false));
    monaco.editor.defineTheme(options.lightThemeName, createThemeData(true));
    monaco.editor.setTheme(resolveThemeName(options));
}

function resolveThemeName(options) {
    return isLightThemeActive() ? options.lightThemeName : options.darkThemeName;
}

function isLightThemeActive() {
    return document.documentElement.dataset.theme === "light" ||
        document.documentElement.classList.contains("theme-light") ||
        document.body?.classList.contains("theme-light") === true;
}

function createThemeData(isLight) {
    return {
        base: isLight ? "vs" : "vs-dark",
        inherit: true,
        rules: [
            { token: "frontmatter.key", foreground: isLight ? "5F57D6" : "A0AAFF" },
            { token: "frontmatter.value", foreground: isLight ? "23784B" : "6FE89A" },
            { token: "frontmatter.delimiter", foreground: isLight ? "7F8A8A" : "8A9A94" },
            { token: "header.segment.hash", foreground: isLight ? "8B6A33" : "F2E1AA" },
            { token: "header.segment.body", foreground: isLight ? "8B6A33" : "F2E1AA" },
            { token: "header.block.hash", foreground: isLight ? "3B6E9A" : "8ECFFF" },
            { token: "header.block.body", foreground: isLight ? "3B6E9A" : "8ECFFF" },
            { token: "pause.timed", foreground: isLight ? "8E6A00" : "E0C070" },
            { token: "pause.long", foreground: isLight ? "9A7A19" : "EEDB96" },
            { token: "pause.short", foreground: isLight ? "8E6A00" : "E0C070" },
            { token: "cue.open", foreground: isLight ? "8A7B6B" : "8A9E98" },
            { token: "cue.close", foreground: isLight ? "8A7B6B" : "8A9E98" },
            { token: "wpm.badge", foreground: isLight ? "936F00" : "FFE066", fontStyle: "bold" },
            { token: "meta.tag", foreground: isLight ? "6E7781" : "B8C0C8" }
        ],
        colors: {
            "editor.background": isLight ? "#00000000" : "#00000000",
            "editor.foreground": isLight ? "#32281E" : "#ECF0EE",
            "editor.selectionBackground": isLight ? "#FFE06633" : "#FFE0662E",
            "editorCursor.foreground": isLight ? "#5C4D3D" : "#ECF0EE"
        }
    };
}

function onEditorContentChanged(state) {
    syncProxyFromEditor(state);
    dispatchProxyChangedEvent(state);
    renderSemanticSnapshot(state, state.editor.getValue());
    scheduleDecorations(state);
    if (!state.suppressTextNotification) {
        void state.dotNetRef.invokeMethodAsync(state.options.textChangedCallbackName, state.editor.getValue());
    }
}

function notifySelectionChanged(state, dismissMenus) {
    syncProxyFromEditor(state);
    if (!state.suppressSelectionNotification) {
        const selectionState = createSelectionState(state);
        void state.dotNetRef.invokeMethodAsync(
            state.options.selectionChangedCallbackName,
            selectionState.start,
            selectionState.end,
            selectionState.line,
            selectionState.column,
            selectionState.toolbarTop,
            selectionState.toolbarLeft,
            dismissMenus);
    }
}

function notifyHistoryRequested(state, command) {
    void state.dotNetRef.invokeMethodAsync(state.options.historyRequestedCallbackName, command);
}

function renderSemanticSnapshot(state, text) {
    const renderer = window.EditorSurfaceInterop;
    if (renderer?.renderOverlay && state.semanticSnapshot) {
        renderer.renderOverlay(state.semanticSnapshot, text ?? emptyValue, state.options.cueContracts);
    }
}

function scheduleDecorations(state) {
    if (state.decorationFrameId) {
        cancelAnimationFrame(state.decorationFrameId);
    }

    state.decorationFrameId = requestAnimationFrame(() => {
        state.decorationCollection.set(buildDecorations(state));
        state.decorationFrameId = 0;
    });
}

function buildDecorations(state) {
    const { editor, monaco } = state;
    const model = editor.getModel();
    if (!monaco || !model) {
        return [];
    }

    const decorations = [];
    const frontMatterEndLine = resolveFrontMatterEndLine(model);
    const lineRanges = getDecorationLineRanges(state, model);

    for (const lineRange of lineRanges) {
        for (let lineNumber = lineRange.startLineNumber; lineNumber <= lineRange.endLineNumber; lineNumber++) {
            const line = model.getLineContent(lineNumber);
            if (frontMatterEndLine > 0 && lineNumber <= frontMatterEndLine) {
                decorateFrontMatterLine(monaco, decorations, lineNumber, line);
                continue;
            }

            if (decorateHeaderLine(monaco, decorations, lineNumber, line, "##", `${cssClassPrefix}-line-segment`)) {
                continue;
            }

            if (decorateHeaderLine(monaco, decorations, lineNumber, line, "###", `${cssClassPrefix}-line-block`)) {
                continue;
            }

            decorateBodyLine(monaco, decorations, lineNumber, line);
        }
    }

    return decorations;
}

function getDecorationLineRanges(state, model) {
    const lineCount = model.getLineCount();
    if (!shouldUseVisibleRangeDecorations(state)) {
        return [{
            endLineNumber: lineCount,
            startLineNumber: 1
        }];
    }

    const visibleRanges = state.editor.getVisibleRanges();
    if (!visibleRanges.length) {
        return [{
            endLineNumber: Math.min(lineCount, largeDraftDecorationViewportLinePadding * 2),
            startLineNumber: 1
        }];
    }

    const mergedRanges = [];
    for (const visibleRange of visibleRanges) {
        const nextRange = {
            endLineNumber: Math.min(lineCount, visibleRange.endLineNumber + largeDraftDecorationViewportLinePadding),
            startLineNumber: Math.max(1, visibleRange.startLineNumber - largeDraftDecorationViewportLinePadding)
        };
        const previousRange = mergedRanges[mergedRanges.length - 1];
        if (previousRange && nextRange.startLineNumber <= previousRange.endLineNumber + 1) {
            previousRange.endLineNumber = Math.max(previousRange.endLineNumber, nextRange.endLineNumber);
            continue;
        }

        mergedRanges.push(nextRange);
    }

    return mergedRanges;
}

function resolveFrontMatterEndLine(model) {
    if (model.getLineCount() < 2 || model.getLineContent(1) !== frontMatterDelimiter) {
        return 0;
    }

    for (let lineNumber = 2; lineNumber <= model.getLineCount(); lineNumber++) {
        if (model.getLineContent(lineNumber) === frontMatterDelimiter) {
            return lineNumber;
        }
    }

    return 1;
}

function shouldUseVisibleRangeDecorations(state) {
    const model = state.editor.getModel();
    return (model?.getValueLength() ?? 0) >= largeDraftDecorationCharacterThreshold;
}

function decorateFrontMatterLine(monaco, decorations, lineNumber, line) {
    if (line === frontMatterDelimiter) {
        decorations.push(createInlineDecoration(monaco, lineNumber, 1, line.length + 1, `${cssClassPrefix}-frontmatter-delimiter`, `${cssClassPrefix}-line-frontmatter`));
        return;
    }

    const match = /^(\s*)([A-Za-z0-9_]+):\s*(.*)$/.exec(line);
    if (!match) {
        decorations.push(createLineDecoration(monaco, lineNumber, `${cssClassPrefix}-line-frontmatter`));
        return;
    }

    decorations.push(createLineDecoration(monaco, lineNumber, `${cssClassPrefix}-line-frontmatter`));
    const keyStart = match[1].length + 1;
    const keyEnd = keyStart + match[2].length;
    const valueStart = keyEnd + 2;
    decorations.push(createInlineDecoration(monaco, lineNumber, keyStart, keyEnd + 1, `${cssClassPrefix}-frontmatter-key`));
    decorations.push(createInlineDecoration(monaco, lineNumber, valueStart, line.length + 1, `${cssClassPrefix}-frontmatter-value`));
}

function decorateHeaderLine(monaco, decorations, lineNumber, line, prefix, lineClassName) {
    if (!line.startsWith(prefix + " [") || !line.endsWith("]")) {
        return false;
    }

    decorations.push(createLineDecoration(monaco, lineNumber, lineClassName));
    decorations.push(createInlineDecoration(monaco, lineNumber, 1, prefix.length + 1, `${cssClassPrefix}-header-mark`));
    decorations.push(createInlineDecoration(monaco, lineNumber, prefix.length + 2, prefix.length + 3, `${cssClassPrefix}-header-bracket`));
    decorations.push(createInlineDecoration(monaco, lineNumber, line.length, line.length + 1, `${cssClassPrefix}-header-bracket`));

    const contentStart = prefix.length + 3;
    const content = line.slice(contentStart - 1, -1);
    let segmentStart = contentStart;
    content.split("|").forEach((part, index) => {
        const className = index === 0
            ? `${cssClassPrefix}-header-name`
            : part.startsWith("Speaker:")
                ? `${cssClassPrefix}-header-speaker`
                : /\d+\s*WPM$/i.test(part)
                    ? `${cssClassPrefix}-header-wpm`
                    : headerEmotionTags.has(part.toLowerCase())
                        ? `${cssClassPrefix}-header-emotion`
                        : `${cssClassPrefix}-header-meta`;
        decorations.push(createInlineDecoration(monaco, lineNumber, segmentStart, segmentStart + part.length, className));
        segmentStart += part.length + 1;
    });

    return true;
}

function decorateBodyLine(monaco, decorations, lineNumber, line) {
    const scopes = [createInlineScopeFrame("root", scopeKindRoot, createInlineRenderState())];
    let index = 0;

    for (const match of line.matchAll(new RegExp(tagPattern.source, "g"))) {
        const tagIndex = match.index ?? 0;
        if (tagIndex > index) {
            decorateInlineTextSegment(monaco, decorations, lineNumber, line.slice(index, tagIndex), index, scopes);
        }

        decorateInlineTag(monaco, decorations, lineNumber, match[0], tagIndex, scopes);
        index = tagIndex + match[0].length;
    }

    if (index < line.length) {
        decorateInlineTextSegment(monaco, decorations, lineNumber, line.slice(index), index, scopes);
    }
}

function decorateMatch(monaco, decorations, lineNumber, match, className) {
    const startColumn = (match.index ?? 0) + 1;
    decorateRawRange(monaco, decorations, lineNumber, startColumn, startColumn + match[0].length, className);
}

function decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, className) {
    decorations.push(createInlineDecoration(monaco, lineNumber, startColumn, endColumn, className));
}

function decorateInlineTextSegment(monaco, decorations, lineNumber, text, lineOffset, scopes) {
    if (!text) {
        return;
    }

    const currentScope = scopes[scopes.length - 1];
    if (currentScope.kind === scopeKindPronunciation) {
        currentScope.buffer.push({
            endColumn: lineOffset + text.length + 1,
            startColumn: lineOffset + 1
        });
        return;
    }

    let chunkStart = 0;
    for (let index = 0; index < text.length; index += 1) {
        const pauseLength = getPauseLength(text, index);
        if (pauseLength === 0) {
            continue;
        }

        decorateStyledTextSegment(
            monaco,
            decorations,
            lineNumber,
            lineOffset + chunkStart + 1,
            lineOffset + index + 1,
            currentScope.state);

        const pauseClassName = pauseLength === 2
            ? `${cssClassPrefix}-pause-long`
            : `${cssClassPrefix}-pause-short`;
        decorateRawRange(
            monaco,
            decorations,
            lineNumber,
            lineOffset + index + 1,
            lineOffset + index + pauseLength + 1,
            pauseClassName);

        index += pauseLength - 1;
        chunkStart = index + 1;
    }

    decorateStyledTextSegment(
        monaco,
        decorations,
        lineNumber,
        lineOffset + chunkStart + 1,
        lineOffset + text.length + 1,
        currentScope.state);
}

function decorateStyledTextSegment(monaco, decorations, lineNumber, startColumn, endColumn, state, extraClasses) {
    if (startColumn >= endColumn) {
        return;
    }

    const className = buildInlineStateClassName(state, extraClasses);
    if (!className) {
        return;
    }

    decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, className);
}

function decorateInlineTag(monaco, decorations, lineNumber, rawTag, tagIndex, scopes) {
    const inner = rawTag.slice(1, -1).trim();
    if (!inner) {
        return;
    }

    const startColumn = tagIndex + 1;
    const endColumn = startColumn + rawTag.length;
    if (inner.startsWith("/")) {
        const matchedScope = popInlineScope(scopes, inner.slice(1).trim());
        if (matchedScope?.kind === scopeKindPronunciation) {
            decoratePronunciationBuffer(monaco, decorations, lineNumber, matchedScope, scopes[scopes.length - 1].state);
        }

        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        return;
    }

    const separatorIndex = inner.indexOf(":");
    const name = (separatorIndex >= 0 ? inner.slice(0, separatorIndex) : inner).trim();
    const argument = separatorIndex >= 0 ? inner.slice(separatorIndex + 1).trim() : null;
    const normalizedName = name.toLowerCase();
    const currentState = scopes[scopes.length - 1].state;

    if (numericWpmRegex.test(name)) {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-inline-wpm`);
        scopes.push(createInlineScopeFrame(name, scopeKindWpm, currentState));
        return;
    }

    if (normalizedName === "pause") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-pause-timed`);
        return;
    }

    if (normalizedName === "breath") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-pause-long`);
        return;
    }

    if (normalizedName === "edit_point" || normalizedName === "editpoint") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        return;
    }

    if (normalizedName === "phonetic" || normalizedName === "pronunciation") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-inline-pronunciation`);
        scopes.push(createInlineScopeFrame(name, scopeKindPronunciation, currentState, argument));
        return;
    }

    if (normalizedName === "highlight") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            isHighlighted: true
        }));
        return;
    }

    if (normalizedName === "emphasis" || normalizedName === "strong" || normalizedName === "bold") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            isEmphasis: true
        }));
        return;
    }

    if (normalizedName === "stress") {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            isStress: true
        }));
        return;
    }

    if (speedClasses.has(normalizedName)) {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            speedClass: speedClasses.get(normalizedName),
            speedValue: speedClasses.get(normalizedName) ? normalizedName : null
        }));
        return;
    }

    if (volumeClasses.has(normalizedName)) {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            volumeClass: volumeClasses.get(normalizedName),
            volumeValue: normalizedName
        }));
        return;
    }

    if (deliveryClasses.has(normalizedName)) {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            deliveryClass: deliveryClasses.get(normalizedName),
            deliveryValue: normalizedName
        }));
        return;
    }

    if (emotionClasses.has(normalizedName)) {
        decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
        scopes.push(createInlineScopeFrame(name, scopeKindStyle, {
            ...currentState,
            emotionClass: emotionClasses.get(normalizedName)
        }));
        return;
    }

    decorateRawRange(monaco, decorations, lineNumber, startColumn, endColumn, `${cssClassPrefix}-tag`);
    scopes.push(createInlineScopeFrame(name, scopeKindNeutral, currentState));
}

function decoratePronunciationBuffer(monaco, decorations, lineNumber, scope, parentState) {
    for (const chunk of scope.buffer) {
        decorateStyledTextSegment(
            monaco,
            decorations,
            lineNumber,
            chunk.startColumn,
            chunk.endColumn,
            parentState,
            [`${cssClassPrefix}-inline-pronunciation-word`]);
    }
}

function createInlineScopeFrame(name, kind, state, argument) {
    return {
        argument: argument ?? null,
        buffer: [],
        kind,
        name,
        state
    };
}

function createInlineRenderState() {
    return {
        deliveryClass: null,
        deliveryValue: null,
        emotionClass: null,
        isEmphasis: false,
        isHighlighted: false,
        isStress: false,
        speedClass: null,
        speedValue: null,
        volumeClass: null,
        volumeValue: null
    };
}

function popInlineScope(scopes, closingName) {
    if (scopes.length <= 1) {
        return null;
    }

    const poppedScopes = [];
    let matchedScope = null;
    while (scopes.length > 1) {
        const currentScope = scopes.pop();
        poppedScopes.push(currentScope);
        if (currentScope?.name.localeCompare(closingName, undefined, { sensitivity: "accent" }) === 0) {
            matchedScope = currentScope;
            break;
        }
    }

    if (!matchedScope) {
        for (let index = poppedScopes.length - 1; index >= 0; index -= 1) {
            scopes.push(poppedScopes[index]);
        }
    }

    return matchedScope;
}

function buildInlineStateClassName(state, extraClasses) {
    const classes = [];
    if (state.isEmphasis) {
        classes.push(`${cssClassPrefix}-inline-emphasis`);
    }
    if (state.isHighlighted) {
        classes.push(`${cssClassPrefix}-inline-highlight`);
    }
    if (state.isStress) {
        classes.push(`${cssClassPrefix}-inline-stress`);
    }
    if (state.emotionClass) {
        classes.push(state.emotionClass);
    }
    if (state.volumeClass) {
        classes.push(state.volumeClass);
    }
    if (state.deliveryClass) {
        classes.push(state.deliveryClass);
    }
    if (state.speedClass) {
        classes.push(state.speedClass);
    }
    for (const extraClass of extraClasses ?? []) {
        if (extraClass) {
            classes.push(extraClass);
        }
    }

    return classes.join(" ");
}

function getPauseLength(text, index) {
    if (text[index] !== "/") {
        return 0;
    }

    const length = index + 1 < text.length && text[index + 1] === "/" ? 2 : 1;
    const previous = index === 0 ? "\0" : text[index - 1];
    const nextIndex = index + length;
    const next = nextIndex >= text.length ? "\0" : text[nextIndex];

    if ((previous !== "\0" && !/\s/.test(previous)) || (next !== "\0" && !/\s/.test(next))) {
        return 0;
    }

    return length;
}

function createInlineDecoration(monaco, lineNumber, startColumn, endColumn, inlineClassName, lineClassName) {
    return {
        options: {
            inlineClassName,
            lineClassName
        },
        range: new monaco.Range(lineNumber, startColumn, lineNumber, endColumn)
    };
}

function createLineDecoration(monaco, lineNumber, lineClassName) {
    return {
        options: { isWholeLine: true, lineClassName },
        range: new monaco.Range(lineNumber, 1, lineNumber, 1)
    };
}

function applySelection(state, start, end, revealSelection) {
    const model = state.editor.getModel();
    if (!model) {
        return;
    }

    const safeStart = Math.max(0, Math.min(start, model.getValueLength()));
    const safeEnd = Math.max(safeStart, Math.min(end, model.getValueLength()));
    const startPosition = model.getPositionAt(safeStart);
    const endPosition = model.getPositionAt(safeEnd);
    const range = new state.monaco.Range(
        startPosition.lineNumber,
        startPosition.column,
        endPosition.lineNumber,
        endPosition.column);
    const preservedScrollTop = state.editor.getScrollTop();
    const preservedScrollLeft = state.editor.getScrollLeft();

    state.editor.setSelection({
        endColumn: endPosition.column,
        endLineNumber: endPosition.lineNumber,
        startColumn: startPosition.column,
        startLineNumber: startPosition.lineNumber
    });

    if (revealSelection) {
        const scrollType = state.monaco.editor.ScrollType.Immediate;
        const lineHeight = state.editor.getOption(state.monaco.editor.EditorOption.lineHeight);
        const targetScrollTop = Math.max(
            0,
            state.editor.getTopForLineNumber(startPosition.lineNumber) - (lineHeight * 2));
        state.editor.focus();
        state.editor.setScrollTop(targetScrollTop, scrollType);
        state.editor.render();
    }
    else {
        state.editor.setScrollPosition({
            scrollLeft: preservedScrollLeft,
            scrollTop: preservedScrollTop
        }, state.monaco.editor.ScrollType.Immediate);
    }

    syncProxyFromEditor(state);
}

function syncProxyFromEditor(state) {
    const selection = createSelectionState(state);
    state.proxy.value = state.editor.getValue();
    state.suppressProxySelection = true;
    state.proxy.selectionStart = selection.start;
    state.proxy.selectionEnd = selection.end;
    state.suppressProxySelection = false;
}

function dispatchProxyChangedEvent(state) {
    state.proxy.dispatchEvent(new CustomEvent(state.options.proxyChangedEventName, {
        detail: {
            selectionEnd: state.proxy.selectionEnd ?? 0,
            selectionStart: state.proxy.selectionStart ?? 0,
            textLength: state.proxy.value.length
        }
    }));
}

function createSelectionState(state) {
    const selection = state.editor.getSelection();
    const model = state.editor.getModel();
    if (!selection || !model) {
        return createEmptySelectionState();
    }

    const start = model.getOffsetAt(selection.getStartPosition());
    const end = model.getOffsetAt(selection.getEndPosition());
    const visiblePosition = state.editor.getScrolledVisiblePosition(selection.getStartPosition());
    const rawToolbarTop = visiblePosition ? Math.max(44, (visiblePosition.top ?? 0) + 10) : 44;
    const rawToolbarLeft = visiblePosition ? (visiblePosition.left ?? 0) + ((visiblePosition.width ?? 0) / 2) : 0;
    const toolbarTop = Number.isFinite(rawToolbarTop) ? rawToolbarTop : 44;
    const toolbarLeft = Number.isFinite(rawToolbarLeft) ? rawToolbarLeft : 0;

    return {
        column: selection.getPosition().column,
        end,
        line: selection.getPosition().lineNumber,
        start,
        toolbarLeft,
        toolbarTop
    };
}

function createEmptySelectionState() {
    return {
        column: 1,
        end: 0,
        line: 1,
        start: 0,
        toolbarLeft: 0,
        toolbarTop: 44
    };
}
