const clarityFunctionName = "clarity";
const clarityProviderName = "clarity";
const googleAnalyticsFunctionName = "gtag";
const googleAnalyticsProviderName = "google-analytics";
const googleDataLayerName = "dataLayer";

const telemetryContract = {
    eventsCollection: "",
    harnessGlobalName: "",
    initializationsCollection: "",
    pageViewsCollection: "",
    runtimeGlobalName: "",
    runtimeHarnessEnabledProperty: "",
    runtimeWasmDebugEnabledProperty: "",
    vendorLoadsCollection: ""
};

const telemetryState = {
    clarityConfigured: false,
    clarityProjectId: "",
    googleAnalyticsConfigured: false,
    googleAnalyticsMeasurementId: "",
    initialized: false,
    runtimeEnabled: false
};

function applyTelemetryContract(contract) {
    telemetryContract.eventsCollection = contract?.eventsCollection ?? "";
    telemetryContract.harnessGlobalName = contract?.harnessGlobalName ?? "";
    telemetryContract.initializationsCollection = contract?.initializationsCollection ?? "";
    telemetryContract.pageViewsCollection = contract?.pageViewsCollection ?? "";
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

function recordBlockedVendorLoad(provider) {
    recordHarnessEntry(telemetryContract.vendorLoadsCollection, {
        blocked: true,
        provider,
        url: ""
    });
}

async function ensureGoogleAnalyticsConfigured() {
    if (!telemetryState.runtimeEnabled || !telemetryState.googleAnalyticsMeasurementId || telemetryState.googleAnalyticsConfigured) {
        return;
    }

    installGoogleAnalyticsStub();
    recordBlockedVendorLoad(googleAnalyticsProviderName);

    window[googleAnalyticsFunctionName]("js", new Date());
    window[googleAnalyticsFunctionName]("config", telemetryState.googleAnalyticsMeasurementId, { send_page_view: false });
    telemetryState.googleAnalyticsConfigured = true;
}

async function ensureClarityConfigured() {
    if (!telemetryState.runtimeEnabled || !telemetryState.clarityProjectId || telemetryState.clarityConfigured) {
        return;
    }

    installClarityStub();
    recordBlockedVendorLoad(clarityProviderName);

    telemetryState.clarityConfigured = true;
}

function buildInitializationSnapshot(config) {
    const runtime = getRuntime();
    const sentryConfigured = config?.sentryConfigured === true;

    return {
        clarityConfigured: Boolean(config?.clarityProjectId),
        debugEnabled: runtime[telemetryContract.runtimeWasmDebugEnabledProperty] === true,
        googleAnalyticsConfigured: Boolean(config?.googleAnalyticsMeasurementId),
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

    recordHarnessEntry(telemetryContract.initializationsCollection, buildInitializationSnapshot(config));

    if (!telemetryState.runtimeEnabled) {
        return false;
    }

    await ensureGoogleAnalyticsConfigured();
    await ensureClarityConfigured();
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
    window[googleAnalyticsFunctionName]("event", eventName, payload ?? {});
    return true;
}
