const clarityFunctionName = "clarity";
const clarityProviderName = "clarity";
const clarityScriptElementId = "prompterone-runtime-clarity";
const clarityScriptUrlPrefix = "https://www.clarity.ms/tag/";
const googleAnalyticsFunctionName = "gtag";
const googleAnalyticsProviderName = "google-analytics";
const googleAnalyticsScriptElementId = "prompterone-runtime-google-analytics";
const googleAnalyticsScriptUrlPrefix = "https://www.googletagmanager.com/gtag/js?id=";
const googleDataLayerName = "dataLayer";

const telemetryContract = {
    eventsCollection: "",
    harnessGlobalName: "",
    initializationsCollection: "",
    pageViewsCollection: "",
    runtimeAllowVendorLoadsProperty: "",
    runtimeGlobalName: "",
    runtimeHarnessEnabledProperty: "",
    runtimeWasmDebugEnabledProperty: "",
    vendorLoadsCollection: ""
};

const telemetryState = {
    clarityConfigured: false,
    clarityProjectId: "",
    clarityScriptLoadPromise: null,
    googleAnalyticsConfigured: false,
    googleAnalyticsMeasurementId: "",
    googleAnalyticsScriptLoadPromise: null,
    initialized: false,
    runtimeEnabled: false
};

function applyTelemetryContract(contract) {
    telemetryContract.eventsCollection = contract?.eventsCollection ?? "";
    telemetryContract.harnessGlobalName = contract?.harnessGlobalName ?? "";
    telemetryContract.initializationsCollection = contract?.initializationsCollection ?? "";
    telemetryContract.pageViewsCollection = contract?.pageViewsCollection ?? "";
    telemetryContract.runtimeAllowVendorLoadsProperty = contract?.runtimeAllowVendorLoadsProperty ?? "";
    telemetryContract.runtimeGlobalName = contract?.runtimeGlobalName ?? "";
    telemetryContract.runtimeHarnessEnabledProperty = contract?.runtimeHarnessEnabledProperty ?? "";
    telemetryContract.runtimeWasmDebugEnabledProperty = contract?.runtimeWasmDebugEnabledProperty ?? "";
    telemetryContract.vendorLoadsCollection = contract?.vendorLoadsCollection ?? "";
}

function getHarness() {
    const runtime = getRuntime();
    if (runtime[telemetryContract.runtimeHarnessEnabledProperty] !== true) {
        return null;
    }

    return window[telemetryContract.harnessGlobalName] ?? null;
}

function getRuntime() {
    return window[telemetryContract.runtimeGlobalName] ?? {};
}

function areVendorLoadsAllowed() {
    return getRuntime()[telemetryContract.runtimeAllowVendorLoadsProperty] === true;
}

function shouldBlockVendorLoads() {
    return getHarness() !== null && !areVendorLoadsAllowed();
}

function ensureHarnessCollection(name) {
    const harness = getHarness();
    if (harness === null) {
        return null;
    }

    if (!Array.isArray(harness[name])) {
        harness[name] = [];
    }

    return harness[name];
}

function recordHarnessEntry(name, payload) {
    const collection = ensureHarnessCollection(name);
    if (collection !== null) {
        collection.push(payload);
    }
}

function installGoogleAnalyticsStub() {
    window[googleDataLayerName] = window[googleDataLayerName] ?? [];

    if (typeof window[googleAnalyticsFunctionName] !== "function") {
        window[googleAnalyticsFunctionName] = function () {
            window[googleDataLayerName].push(arguments);
        };
    }
}

function installClarityStub() {
    if (typeof window[clarityFunctionName] === "function") {
        return;
    }

    const clarityStub = function () {
        clarityStub.q = clarityStub.q ?? [];
        clarityStub.q.push(arguments);
    };

    window[clarityFunctionName] = clarityStub;
}

function recordVendorLoad(provider, blocked, url) {
    recordHarnessEntry(telemetryContract.vendorLoadsCollection, {
        blocked,
        provider,
        url
    });
}

function buildGoogleAnalyticsScriptUrl(measurementId) {
    return `${googleAnalyticsScriptUrlPrefix}${encodeURIComponent(measurementId)}`;
}

function buildClarityScriptUrl(projectId) {
    return `${clarityScriptUrlPrefix}${encodeURIComponent(projectId)}`;
}

function loadExternalScript(provider, url, elementId, promisePropertyName) {
    const existingPromise = telemetryState[promisePropertyName];
    if (existingPromise !== null) {
        return existingPromise;
    }

    const loadPromise = new Promise((resolve, reject) => {
        const handleError = () => reject(new Error(`${provider} failed to load.`));

        const existingElement = document.getElementById(elementId);
        if (existingElement instanceof HTMLScriptElement) {
            if (existingElement.dataset.prompterOneLoaded === "true") {
                resolve();
                return;
            }

            if (existingElement.dataset.prompterOneFailed === "true") {
                handleError();
                return;
            }

            existingElement.addEventListener("load", resolve, { once: true });
            existingElement.addEventListener("error", handleError, { once: true });
            return;
        }

        const scriptElement = document.createElement("script");
        scriptElement.async = true;
        scriptElement.id = elementId;
        scriptElement.src = url;
        scriptElement.addEventListener("load", () => {
            scriptElement.dataset.prompterOneLoaded = "true";
            resolve();
        }, { once: true });
        scriptElement.addEventListener("error", () => {
            scriptElement.dataset.prompterOneFailed = "true";
            handleError();
        }, { once: true });

        recordVendorLoad(provider, false, url);
        (document.head ?? document.documentElement).appendChild(scriptElement);
    }).catch(error => {
        telemetryState[promisePropertyName] = null;
        throw error;
    });

    telemetryState[promisePropertyName] = loadPromise;
    return loadPromise;
}

async function ensureGoogleAnalyticsConfigured() {
    if (!telemetryState.runtimeEnabled || !telemetryState.googleAnalyticsMeasurementId || telemetryState.googleAnalyticsConfigured) {
        return;
    }

    installGoogleAnalyticsStub();

    if (shouldBlockVendorLoads()) {
        recordVendorLoad(googleAnalyticsProviderName, true, "");
    }
    else {
        try {
            await loadExternalScript(
                googleAnalyticsProviderName,
                buildGoogleAnalyticsScriptUrl(telemetryState.googleAnalyticsMeasurementId),
                googleAnalyticsScriptElementId,
                "googleAnalyticsScriptLoadPromise");
        }
        catch {
            return;
        }
    }

    window[googleAnalyticsFunctionName]("js", new Date());
    window[googleAnalyticsFunctionName]("config", telemetryState.googleAnalyticsMeasurementId, { send_page_view: false });
    telemetryState.googleAnalyticsConfigured = true;
}

async function ensureClarityConfigured() {
    if (!telemetryState.runtimeEnabled || !telemetryState.clarityProjectId || telemetryState.clarityConfigured) {
        return;
    }

    installClarityStub();

    if (shouldBlockVendorLoads()) {
        recordVendorLoad(clarityProviderName, true, "");
        telemetryState.clarityConfigured = true;
        return;
    }

    try {
        await loadExternalScript(
            clarityProviderName,
            buildClarityScriptUrl(telemetryState.clarityProjectId),
            clarityScriptElementId,
            "clarityScriptLoadPromise");
    }
    catch {
        return;
    }

    telemetryState.clarityConfigured = true;
}

function buildInitializationSnapshot(config) {
    const runtime = getRuntime();
    const sentryConfigured = config?.sentryConfigured === true;

    return {
        clarityConfigured: telemetryState.clarityConfigured,
        debugEnabled: runtime[telemetryContract.runtimeWasmDebugEnabledProperty] === true,
        googleAnalyticsConfigured: telemetryState.googleAnalyticsConfigured,
        hostEnabled: config?.hostEnabled === true,
        runtimeEnabled: telemetryState.runtimeEnabled,
        sentryConfigured,
        sentryRuntimeEnabled: sentryConfigured && telemetryState.runtimeEnabled
    };
}

function normalizePayload(eventName, payload) {
    const safePayload = payload && typeof payload === "object" ? payload : {};
    return {
        eventName,
        ...safePayload
    };
}

export async function initializeRuntimeTelemetry(config) {
    applyTelemetryContract(config?.contract);
    telemetryState.initialized = true;
    telemetryState.googleAnalyticsMeasurementId = config?.googleAnalyticsMeasurementId ?? "";
    telemetryState.clarityProjectId = config?.clarityProjectId ?? "";
    telemetryState.runtimeEnabled =
        config?.hostEnabled === true
        && getRuntime()[telemetryContract.runtimeWasmDebugEnabledProperty] !== true;

    if (!telemetryState.runtimeEnabled) {
        recordHarnessEntry(telemetryContract.initializationsCollection, buildInitializationSnapshot(config));
        return false;
    }

    await ensureGoogleAnalyticsConfigured();
    await ensureClarityConfigured();
    recordHarnessEntry(telemetryContract.initializationsCollection, buildInitializationSnapshot(config));
    return true;
}

export async function trackRuntimeTelemetryPageView(eventName, payload) {
    return trackRuntimeTelemetryEventInternal(eventName, payload, telemetryContract.pageViewsCollection);
}

export async function trackRuntimeTelemetryEvent(eventName, payload) {
    return trackRuntimeTelemetryEventInternal(eventName, payload, telemetryContract.eventsCollection);
}

async function trackRuntimeTelemetryEventInternal(eventName, payload, harnessCollectionName) {
    if (!telemetryState.initialized || !telemetryState.runtimeEnabled || !telemetryState.googleAnalyticsMeasurementId) {
        return false;
    }

    await ensureGoogleAnalyticsConfigured();

    const normalizedPayload = normalizePayload(eventName, payload);
    recordHarnessEntry(harnessCollectionName, normalizedPayload);
    window[googleAnalyticsFunctionName]("event", eventName, normalizedPayload);
    return true;
}
