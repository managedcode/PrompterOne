const mediaContractPropertyFallback = "media";
const runtimeGlobalNameFallback = "__prompterOneRuntime";

export function configureMediaRuntime(contract) {
    const runtimeGlobalName = typeof contract?.runtimeGlobalName === "string" && contract.runtimeGlobalName.length > 0
        ? contract.runtimeGlobalName
        : runtimeGlobalNameFallback;
    const contractProperty = typeof contract?.contractProperty === "string" && contract.contractProperty.length > 0
        ? contract.contractProperty
        : mediaContractPropertyFallback;

    window[runtimeGlobalName] = window[runtimeGlobalName] || {};
    const runtime = window[runtimeGlobalName];
    runtime[contractProperty] = {
        ...(runtime[contractProperty] ?? {}),
        ...(contract ?? {})
    };
}

export function configureThemeRuntime(contract) {
    const runtimeGlobalName = typeof contract?.runtimeGlobalName === "string" && contract.runtimeGlobalName.length > 0
        ? contract.runtimeGlobalName
        : runtimeGlobalNameFallback;
    const contractProperty = typeof contract?.contractProperty === "string" && contract.contractProperty.length > 0
        ? contract.contractProperty
        : "theme";

    window[runtimeGlobalName] = window[runtimeGlobalName] || {};
    const runtime = window[runtimeGlobalName];
    runtime[contractProperty] = {
        ...(runtime[contractProperty] ?? {}),
        ...(contract ?? {})
    };
}
