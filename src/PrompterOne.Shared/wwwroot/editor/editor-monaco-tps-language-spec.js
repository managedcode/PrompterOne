const emptyValue = "";
const titleHeaderMarker = "#";
const segmentHeaderMarker = "##";
const blockHeaderMarker = "###";
const headerTimingPattern = /^\d+:\d{2}-\d+:\d{2}$/;
const speakerPrefix = "Speaker:";
const numericWpmPattern = /^\d+\s*WPM$/i;
const inlineTagPattern = /\[[^[\]]+\]/g;
const escapedSequencePattern = /\\(?:[\\/\[\]\|\*])/;
const markdownBoldPattern = /\*\*([^*\n]+)\*\*/g;
const markdownItalicPattern = /\*(?!\*)([^*\n]+)\*(?!\*)/g;
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
    ...emotionTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Emotion wrapper", `Apply the ${name} delivery color and feel to the wrapped text.`)),
    ...volumeTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Volume wrapper", `Wrap text that should be delivered as ${name}.`)),
    ...deliveryTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Delivery wrapper", `Wrap text that should be delivered as ${name}.`)),
    ...speedTagNames.map(name => createWrapperCompletion(name, "[" + name + "]${1:text}[/" + name + "]", "Speed wrapper", speedDocumentationByName[name]))
];
const markdownWrapperSpecs = [
    createCompletionSpec("**text**", "**${1:text}**", "Markdown bold", "Strong emphasis level 2 using markdown bold syntax.", "tag"),
    createCompletionSpec("*text*", "*${1:text}*", "Markdown italic", "Emphasis level 1 using markdown italic syntax.", "tag")
];
const parameterizedWrapperSpecs = [
    createParameterizedCompletion("phonetic", "[phonetic:${1:ˈkæməl}]${2:camel}[/phonetic]", "IPA pronunciation guide", "Attach an IPA pronunciation guide to the wrapped word."),
    createParameterizedCompletion("pronunciation", "[pronunciation:${1:KAM-uhl}]${2:camel}[/pronunciation]", "Simple pronunciation guide", "Attach a readable pronunciation guide to the wrapped word."),
    createParameterizedCompletion("stress", "[stress:${1:de-VE-lop-ment}]${2:development}[/stress]", "Syllable guide", "Attach a full syllable guide; renderers should show it as a tooltip or overlay.")
];
const standaloneCompletionSpecs = [
    createCompletionSpec("[pause:2s]", "[pause:${1:2s}]", "Timed pause", "Pause for an explicit number of seconds or milliseconds.", "pause"),
    createCompletionSpec("[pause:1000ms]", "[pause:${1:1000ms}]", "Timed pause (ms)", "Pause for an explicit number of milliseconds.", "pause"),
    createCompletionSpec("[breath]", "[breath]", "Breath mark", "Mark a natural breath point without adding time.", "tag"),
    createCompletionSpec("[edit_point]", "[edit_point]", "Edit point", "Mark a standard edit point.", "tag"),
    createCompletionSpec("[edit_point:high]", "[edit_point:${1:high}]", "Priority edit point", "Mark an edit point with a priority such as high, medium, or low.", "tag"),
    createCompletionSpec("[edit_point:medium]", "[edit_point:${1:medium}]", "Priority edit point", "Mark an edit point with a medium priority.", "tag"),
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
const headerDocumentationByMarker = Object.freeze({
    [titleHeaderMarker]: "Top-level TPS title. Use it for the document name above the front matter and body.",
    [segmentHeaderMarker]: "TPS segment header. Segments break a large script into major sections and can carry WPM, emotion, timing, and speaker metadata.",
    [blockHeaderMarker]: "TPS block header. Blocks live inside a segment and carry narrower structure and metadata."
});

export {
    blockHeaderMarker,
    deliveryTagNames,
    emotionTagNames,
    emptyValue,
    escapedSequencePattern,
    headerCompletionSpecs,
    headerDocumentationByMarker,
    headerTimingPattern,
    inlineTagPattern,
    markdownBoldPattern,
    markdownItalicPattern,
    markdownWrapperSpecs,
    numericWpmPattern,
    parameterizedWrapperLookup,
    parameterizedWrapperSpecs,
    pauseCompletionSpecs,
    plainWrapperLookup,
    segmentHeaderMarker,
    simpleWrapperSpecs,
    speakerPrefix,
    speedTagNames,
    standaloneCompletionSpecs,
    titleHeaderMarker
};

function createWrapperCompletion(name, insertText, detail, documentation) {
    return createCompletionSpec(`[${name}]text[/${name}]`, insertText, detail, documentation, "tag", name);
}

function createParameterizedCompletion(name, insertText, detail, documentation) {
    return createCompletionSpec(`[${name}:guide]text[/${name}]`, insertText, detail, documentation, "tag", name);
}

function createCompletionSpec(label, insertText, detail, documentation, group, name) {
    return { detail, documentation, group, insertText, label, name: name ?? emptyValue };
}
