const emptyValue = "";
const titleHeaderMarker = "#";
const segmentHeaderMarker = "##";
const blockHeaderMarker = "###";
const headerTimingPattern = /^\d+:\d{2}-\d+:\d{2}$/;
const speakerPrefix = "Speaker:";
const numericWpmPattern = /^\d+\s*WPM$/i;
const inlineTagPattern = /\[[^[\]]+\]/g;
const escapedSequencePattern = /\\[\/\[\]]/;
const emotionTagNames = ["warm", "concerned", "focused", "motivational", "neutral", "urgent", "happy", "excited", "sad", "calm", "energetic", "professional"];
const volumeTagNames = ["loud", "soft", "whisper"];
const deliveryTagNames = ["aside", "rhetorical", "sarcasm", "building"];
const speedTagNames = ["xslow", "slow", "normal", "fast", "xfast"];
const speedDocumentationByName = Object.freeze({
    xslow: "Slow the wrapped text down significantly.",
    slow: "Open the wrapped text up a little and slow it down.",
    normal: "Restore the inherited or default pace inside a nested speed scope.",
    fast: "Tighten the wrapped text and deliver it faster.",
    xfast: "Push the wrapped text to the fastest delivery preset."
});
const simpleWrapperSpecs = [
    createWrapperCompletion("emphasis", "[emphasis]${1:text}[/emphasis]", "Emphasis wrapper", "Wrap words that should land harder."),
    createWrapperCompletion("highlight", "[highlight]${1:text}[/highlight]", "Highlight wrapper", "Mark words that should read as visually highlighted."),
    createWrapperCompletion("stress", "[stress]${1:me}[/stress]", "Stress wrapper", "Wrap the stressed syllable or letters inside a word."),
    createWrapperCompletion("strong", "[strong]${1:text}[/strong]", "Strong wrapper", "Alias for stronger emphasis on a phrase."),
    createWrapperCompletion("bold", "[bold]${1:text}[/bold]", "Bold wrapper", "Alias for stronger emphasis on a phrase."),
    ...emotionTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Emotion wrapper", `Apply the ${name} delivery color and feel to the wrapped text.`)),
    ...volumeTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Volume wrapper", `Wrap text that should be delivered as ${name}.`)),
    ...deliveryTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Delivery wrapper", `Wrap text that should be delivered as ${name}.`)),
    ...speedTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Speed wrapper", speedDocumentationByName[name]))
];
const parameterizedWrapperSpecs = [
    createParameterizedCompletion("phonetic", "[phonetic:${1:ˈkæməl}]${2:camel}[/phonetic]", "IPA pronunciation guide", "Attach an IPA pronunciation guide to the wrapped word."),
    createParameterizedCompletion("pronunciation", "[pronunciation:${1:KAM-uhl}]${2:camel}[/pronunciation]", "Simple pronunciation guide", "Attach a readable pronunciation guide to the wrapped word."),
    createParameterizedCompletion("stress", "[stress:${1:de-VE-lop-ment}]${2:development}[/stress]", "Syllable guide", "Attach a full syllable guide; renderers should show it as a tooltip or overlay.")
];
const standaloneCompletionSpecs = [
    createCompletionSpec("[pause:2s]", "[pause:${1:2s}]", "Timed pause", "Pause for an explicit number of seconds or milliseconds.", "pause"),
    createCompletionSpec("[breath]", "[breath]", "Breath mark", "Mark a natural breath point without adding time.", "tag"),
    createCompletionSpec("[edit_point]", "[edit_point]", "Edit point", "Mark a standard edit point.", "tag"),
    createCompletionSpec("[edit_point:high]", "[edit_point:${1:high}]", "Priority edit point", "Mark an edit point with a priority such as high, medium, or low.", "tag"),
    createCompletionSpec("[140WPM]", "[${1:140}WPM]", "Inline WPM override", "Override speaking pace inline for the wrapped scope.", "tag")
];
const headerCompletionSpecs = [
    createCompletionSpec("# Title", "# ${1:Document Title}", "Document title", "Top-level TPS document title.", "header"),
    createCompletionSpec("## [Segment Name|Speaker:Host|140WPM|neutral|0:00-0:30]", "## [${1:Segment Name}|Speaker:${2:Host}|${3:140}WPM|${4:neutral}|${5:0:00-0:30}]", "Segment header", "Create a structured TPS segment header.", "header"),
    createCompletionSpec("### [Block Name|Speaker:Host|140WPM|focused]", "### [${1:Block Name}|Speaker:${2:Host}|${3:140}WPM|${4:focused}]", "Block header", "Create a structured TPS block header.", "header"),
    createCompletionSpec("## Segment Title", "## ${1:Segment Title}", "Simple segment header", "Plain markdown segment header with inherited metadata.", "header"),
    createCompletionSpec("### Block Title", "### ${1:Block Title}", "Simple block header", "Plain markdown block header with inherited metadata.", "header")
];
const pauseCompletionSpecs = [
    createCompletionSpec("/", " / ", "Short pause", "Insert a short 300ms pause.", "pause"),
    createCompletionSpec("//", " //", "Medium pause", "Insert a medium 600ms pause.", "pause")
];
const plainWrapperLookup = new Map(simpleWrapperSpecs.map(spec => [spec.name, spec]));
const parameterizedWrapperLookup = new Map(parameterizedWrapperSpecs.map(spec => [spec.name, spec]));
const supportByLanguageId = new Map();
const headerDocumentationByMarker = Object.freeze({
    [titleHeaderMarker]: "Top-level TPS title. Use it for the document name above the front matter and body.",
    [segmentHeaderMarker]: "TPS segment header. Segments break a large script into major sections and can carry WPM, emotion, timing, and speaker metadata.",
    [blockHeaderMarker]: "TPS block header. Blocks live inside a segment and carry narrower structure and metadata."
});

export function ensureTpsLanguage(monaco, options) {
    if (supportByLanguageId.has(options.languageId)) {
        return;
    }

    if (!monaco.languages.getLanguages().some(language => language.id === options.languageId)) {
        monaco.languages.register({ id: options.languageId });
    }

    monaco.languages.setLanguageConfiguration(options.languageId, {
        autoClosingPairs: [{ open: "[", close: "]" }],
        brackets: [["[", "]"]],
        surroundingPairs: [{ open: "[", close: "]" }]
    });
    monaco.languages.setMonarchTokensProvider(options.languageId, createTokenizer());

    const completionProvider = createCompletionProvider(monaco);
    const hoverProvider = createHoverProvider(monaco);
    monaco.languages.registerCompletionItemProvider(options.languageId, completionProvider);
    monaco.languages.registerHoverProvider(options.languageId, hoverProvider);
    supportByLanguageId.set(options.languageId, { completionProvider, hoverProvider });
}

export function getTpsLanguageSupport(languageId) {
    return supportByLanguageId.get(languageId) ?? null;
}

function createTokenizer() {
    const simpleWrapperPattern = createAlternationPattern(simpleWrapperSpecs.map(spec => spec.name));
    const parameterizedPattern = createAlternationPattern(parameterizedWrapperSpecs.map(spec => spec.name));
    const closingPattern = createAlternationPattern([...simpleWrapperSpecs, ...parameterizedWrapperSpecs].map(spec => spec.name));
    return {
        tokenizer: {
            root: [
                [/^---$/, "frontmatter.delimiter"],
                [/^([A-Za-z0-9_]+)(:)(\s*)(.+)$/, ["frontmatter.key", "delimiter", "white", "frontmatter.value"]],
                [/^(###)(\s+)(\[.*\]|.+)$/, ["header.block.hash", "white", "header.block.body"]],
                [/^(##)(\s+)(\[.*\]|.+)$/, ["header.segment.hash", "white", "header.segment.body"]],
                [/^(#)(\s+)(.+)$/, ["header.title.hash", "white", "header.title.body"]],
                [escapedSequencePattern, "escape.sequence"],
                [/\[pause:[^\]]+\]/i, "pause.timed"],
                [/\[(?:edit_point|editpoint)(?::[^\]]+)?\]/i, "cue.editpoint"],
                [/\[breath\]/i, "cue.breath"],
                [new RegExp(`\\[(?:${parameterizedPattern}):[^\\]]+\\]`, "i"), "cue.pronunciation"],
                [new RegExp(`\\[\\/(?:${closingPattern})\\]`, "i"), "cue.close"],
                [new RegExp(`\\[(?:${simpleWrapperPattern})\\]`, "i"), "cue.open"],
                [/\[\d+\s*WPM\]/i, "wpm.badge"],
                [/(?<!\\)(?<!\S)\/\/(?=\s|$)/, "pause.long"],
                [/(?<!\\)(?<!\S)\/(?=\s|$)/, "pause.short"],
                [/\[[^\]]+\]/, "meta.tag"]
            ]
        }
    };
}

function createCompletionProvider(monaco) {
    return {
        triggerCharacters: ["#", "[", "/"],
        provideCompletionItems(model, position) {
            const range = resolveCompletionRange(monaco, model, position);
            return {
                suggestions: resolveCompletionSpecs(model, position).map(spec => ({
                    detail: spec.detail,
                    documentation: spec.documentation,
                    insertText: spec.insertText,
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    kind: resolveCompletionKind(monaco, spec.group),
                    label: spec.label,
                    range
                }))
            };
        }
    };
}

function createHoverProvider(monaco) {
    return {
        provideHover(model, position) {
            const line = model.getLineContent(position.lineNumber);
            return findWrappedGuideHover(monaco, line, position) ??
                findTagHover(monaco, line, position) ??
                findPauseHover(monaco, line, position) ??
                findHeaderHover(monaco, line, position);
        }
    };
}

function resolveCompletionRange(monaco, model, position) {
    const line = model.getLineContent(position.lineNumber);
    const prefix = line.slice(0, position.column - 1);
    if (/^\s*#{1,3}$/.test(prefix)) {
        const markerStart = prefix.indexOf("#") + 1;
        return new monaco.Range(position.lineNumber, markerStart, position.lineNumber, position.column);
    }

    const lastOpenBracket = prefix.lastIndexOf("[");
    if (lastOpenBracket > prefix.lastIndexOf("]")) {
        return new monaco.Range(position.lineNumber, lastOpenBracket + 1, position.lineNumber, position.column);
    }

    const pauseMatch = /\/{1,2}$/.exec(prefix);
    if (pauseMatch) {
        return new monaco.Range(position.lineNumber, position.column - pauseMatch[0].length, position.lineNumber, position.column);
    }

    const word = model.getWordUntilPosition(position);
    return new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn);
}

function resolveCompletionSpecs(model, position) {
    const prefix = model.getLineContent(position.lineNumber).slice(0, position.column - 1);
    const insideTag = prefix.lastIndexOf("[") > prefix.lastIndexOf("]");
    if (/^\s*#{1,3}$/.test(prefix)) {
        return [...headerCompletionSpecs, ...standaloneCompletionSpecs, ...simpleWrapperSpecs, ...parameterizedWrapperSpecs];
    }

    if (prefix.endsWith("//") || prefix.endsWith("/")) {
        return [...pauseCompletionSpecs, ...standaloneCompletionSpecs, ...simpleWrapperSpecs];
    }

    if (insideTag) {
        return [...standaloneCompletionSpecs, ...simpleWrapperSpecs, ...parameterizedWrapperSpecs, ...headerCompletionSpecs, ...pauseCompletionSpecs];
    }

    return [...headerCompletionSpecs, ...standaloneCompletionSpecs, ...simpleWrapperSpecs, ...parameterizedWrapperSpecs, ...pauseCompletionSpecs];
}

function resolveCompletionKind(monaco, group) {
    if (group === "header") {
        return monaco.languages.CompletionItemKind.Snippet;
    }

    if (group === "pause") {
        return monaco.languages.CompletionItemKind.Operator;
    }

    return monaco.languages.CompletionItemKind.Keyword;
}

function findWrappedGuideHover(monaco, line, position) {
    const wrappedGuidePattern = /\[(phonetic|pronunciation|stress):([^\]]+)\]([^[]+?)\[\/\1\]/ig;
    for (const match of line.matchAll(wrappedGuidePattern)) {
        const openTag = match[0].slice(0, match[0].indexOf(match[3], 0));
        const startColumn = (match.index ?? 0) + openTag.length + 1;
        const endColumn = startColumn + match[3].length;
        if (position.column < startColumn || position.column > endColumn) {
            continue;
        }

        const normalizedName = match[1].toLowerCase();
        const spec = parameterizedWrapperLookup.get(normalizedName);
        return createHover(
            monaco,
            position.lineNumber,
            startColumn,
            endColumn,
            resolveParameterizedTitle(normalizedName),
            `${spec?.documentation ?? emptyValue}\n\nGuide: \`${match[2]}\``.trim());
    }

    return null;
}

function findTagHover(monaco, line, position) {
    for (const match of line.matchAll(inlineTagPattern)) {
        const startColumn = (match.index ?? 0) + 1;
        const endColumn = startColumn + match[0].length;
        if (position.column < startColumn || position.column > endColumn) {
            continue;
        }

        const tagHelp = resolveTagHelp(match[0]);
        return tagHelp
            ? createHover(monaco, position.lineNumber, startColumn, endColumn, tagHelp.title, tagHelp.description)
            : null;
    }

    return null;
}

function findPauseHover(monaco, line, position) {
    for (let index = 0; index < line.length; index += 1) {
        const pauseLength = getPauseLength(line, index);
        if (pauseLength === 0) {
            continue;
        }

        const startColumn = index + 1;
        const endColumn = startColumn + pauseLength;
        if (position.column >= startColumn && position.column <= endColumn) {
            return createHover(monaco, position.lineNumber, startColumn, endColumn, pauseLength === 2 ? "Medium pause" : "Short pause", pauseLength === 2 ? "Insert a 600ms beat pause between phrases." : "Insert a 300ms short pause between clauses.");
        }

        index += pauseLength - 1;
    }

    return null;
}

function findHeaderHover(monaco, line, position) {
    const headerMatch = /^(#{1,3})(\s+)(.+)$/.exec(line);
    if (!headerMatch) {
        return null;
    }

    const marker = headerMatch[1];
    const bodyStartColumn = marker.length + headerMatch[2].length + 1;
    if (position.column <= marker.length) {
        return createHover(monaco, position.lineNumber, 1, marker.length + 1, resolveHeaderTitle(marker), headerDocumentationByMarker[marker]);
    }

    const body = headerMatch[3];
    if (body.startsWith("[") && body.endsWith("]")) {
        const contentStartColumn = bodyStartColumn + 1;
        let partStartColumn = contentStartColumn;
        for (const [index, part] of body.slice(1, -1).split("|").entries()) {
            const partEndColumn = partStartColumn + part.length;
            if (position.column >= partStartColumn && position.column <= partEndColumn) {
                return createHover(monaco, position.lineNumber, partStartColumn, partEndColumn, resolveHeaderPartTitle(marker, index), resolveHeaderPartDescription(marker, index, part.trim()));
            }

            partStartColumn = partEndColumn + 1;
        }
    }

    return createHover(monaco, position.lineNumber, bodyStartColumn, line.length + 1, resolveHeaderTitle(marker), headerDocumentationByMarker[marker]);
}

function resolveTagHelp(rawTag) {
    const inner = rawTag.slice(1, -1).trim();
    const isClosing = inner.startsWith("/");
    const content = isClosing ? inner.slice(1).trim() : inner;
    const separatorIndex = content.indexOf(":");
    const name = (separatorIndex >= 0 ? content.slice(0, separatorIndex) : content).trim().toLowerCase();
    const argument = separatorIndex >= 0 ? content.slice(separatorIndex + 1).trim() : emptyValue;
    if (numericWpmPattern.test(content)) {
        return { title: "Inline WPM override", description: "Override speaking pace inline with a numeric WPM badge such as `[180WPM]`." };
    }

    if (name === "pause") {
        return { title: "Timed pause", description: "Pause for an explicit duration using seconds or milliseconds, for example `[pause:2s]` or `[pause:1000ms]`." };
    }

    if (name === "breath") {
        return { title: "Breath mark", description: "Mark a natural breath point without adding extra timing." };
    }

    if (name === "edit_point" || name === "editpoint") {
        return { title: "Edit point", description: argument ? `Mark an edit point with priority \`${argument}\`.` : "Mark a standard edit point in the script." };
    }

    if (argument && parameterizedWrapperLookup.has(name)) {
        return { title: resolveParameterizedTitle(name), description: `${parameterizedWrapperLookup.get(name).documentation}\n\nGuide: \`${argument}\`` };
    }

    if (plainWrapperLookup.has(name)) {
        return { title: plainWrapperLookup.get(name).detail, description: plainWrapperLookup.get(name).documentation };
    }

    return null;
}

function resolveHeaderTitle(marker) {
    return marker === titleHeaderMarker ? "Document title" : marker === segmentHeaderMarker ? "Segment header" : "Block header";
}

function resolveHeaderPartTitle(marker, index) {
    if (index === 0) {
        return marker === segmentHeaderMarker ? "Segment name" : "Block name";
    }

    return "Header metadata";
}

function resolveHeaderPartDescription(marker, index, value) {
    if (index === 0) {
        return marker === segmentHeaderMarker ? "Name of the major TPS segment." : "Name of the TPS block inside the current segment.";
    }

    if (value.startsWith(speakerPrefix)) {
        return "Talent assignment for multi-speaker scripts. Prefix the speaker name with `Speaker:`.";
    }

    if (numericWpmPattern.test(value)) {
        return "WPM override for this header scope. Omit it when the inherited pace is already correct.";
    }

    if (emotionTagNames.includes(value.toLowerCase())) {
        return `Emotion override for this scope. \`${value}\` affects the delivery styling and inherited mood.`;
    }

    if (headerTimingPattern.test(value)) {
        return "Optional timing window for the header scope.";
    }

    return "Additional header metadata for the current scope.";
}

function resolveParameterizedTitle(name) {
    return name === "stress" ? "Syllable guide" : name === "phonetic" ? "IPA pronunciation guide" : "Pronunciation guide";
}

function createHover(monaco, lineNumber, startColumn, endColumn, title, description) {
    return {
        contents: [{ value: `**${title}**\n\n${description}` }],
        range: new monaco.Range(lineNumber, startColumn, lineNumber, endColumn)
    };
}

function getPauseLength(text, index) {
    if (text[index] !== "/" || (index > 0 && text[index - 1] === "\\")) {
        return 0;
    }

    const length = index + 1 < text.length && text[index + 1] === "/" ? 2 : 1;
    const previous = index === 0 ? "\0" : text[index - 1];
    const next = index + length >= text.length ? "\0" : text[index + length];
    return (previous === "\0" || /\s/.test(previous)) && (next === "\0" || /\s/.test(next)) ? length : 0;
}

function createWrapperCompletion(name, insertText, detail, documentation) {
    return createCompletionSpec(`[${name}]text[/${name}]`, insertText, detail, documentation, "tag", name);
}

function createParameterizedCompletion(name, insertText, detail, documentation) {
    return createCompletionSpec(`[${name}:guide]text[/${name}]`, insertText, detail, documentation, "tag", name);
}

function createCompletionSpec(label, insertText, detail, documentation, group, name) {
    return { detail, documentation, group, insertText, label, name: name ?? emptyValue };
}

function createAlternationPattern(names) {
    return names.map(name => name.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")).join("|");
}
