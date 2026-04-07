(() => {
    const runtimeGlobalNameFallback = "__prompterOneRuntime";
    const defaultThemeRuntime = {
        contractProperty: "theme",
        darkTheme: "dark",
        defaultAccent: "#C4A060",
        defaultDensity: "default",
        densityRootAttribute: "data-density",
        lightTheme: "light",
        runtimeGlobalName: runtimeGlobalNameFallback,
        settingsPageStorageKey: "prompterone.settings.prompterone.settings-page",
        systemTheme: "system",
        themeDarkClass: "theme-dark",
        themeGlobalName: "prompterOneTheme",
        themeLightClass: "theme-light",
        themeRootAttribute: "data-theme",
        themeSourceAttribute: "data-theme-source"
    };
    const root = document.documentElement;
    const themeMedia = typeof window.matchMedia === "function"
        ? window.matchMedia("(prefers-color-scheme: light)")
        : null;
    let selectedTheme = defaultThemeRuntime.darkTheme;

    function getThemeRuntime() {
        const runtime = window[defaultThemeRuntime.runtimeGlobalName];
        const configuredThemeRuntime = runtime?.[defaultThemeRuntime.contractProperty];

        return {
            ...defaultThemeRuntime,
            ...(configuredThemeRuntime ?? {})
        };
    }

    function clampColor(value) {
        return Math.max(0, Math.min(255, Math.round(value)));
    }

    function setBodyClass(className, enabled) {
        if (document.body) {
            document.body.classList.toggle(className, enabled);
        }
    }

    function buildColor(red, green, blue) {
        return `rgb(${red}, ${green}, ${blue})`;
    }

    function buildRgba(red, green, blue, alpha) {
        return `rgba(${red}, ${green}, ${blue}, ${alpha})`;
    }

    function hexToRgb(hex) {
        const match = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex ?? "");
        if (!match) {
            return null;
        }

        return {
            red: Number.parseInt(match[1], 16),
            green: Number.parseInt(match[2], 16),
            blue: Number.parseInt(match[3], 16)
        };
    }

    function mixWithWhite(rgb, ratio) {
        return {
            red: clampColor(rgb.red + ((255 - rgb.red) * ratio)),
            green: clampColor(rgb.green + ((255 - rgb.green) * ratio)),
            blue: clampColor(rgb.blue + ((255 - rgb.blue) * ratio))
        };
    }

    function mixWithBlack(rgb, ratio) {
        return {
            red: clampColor(rgb.red * (1 - ratio)),
            green: clampColor(rgb.green * (1 - ratio)),
            blue: clampColor(rgb.blue * (1 - ratio))
        };
    }

    function resolveEffectiveTheme(theme) {
        const themeRuntime = getThemeRuntime();
        if (theme === themeRuntime.systemTheme) {
            return themeMedia?.matches ? themeRuntime.lightTheme : themeRuntime.darkTheme;
        }

        return theme === themeRuntime.lightTheme ? themeRuntime.lightTheme : themeRuntime.darkTheme;
    }

    function setAccentTokens(accentHex, effectiveTheme) {
        const themeRuntime = getThemeRuntime();
        const rgb = hexToRgb(accentHex) ?? hexToRgb(themeRuntime.defaultAccent);
        if (!rgb) {
            return;
        }

        const accentText = effectiveTheme === themeRuntime.lightTheme
            ? mixWithBlack(rgb, 0.35)
            : mixWithWhite(rgb, 0.45);
        const accentLight = effectiveTheme === themeRuntime.lightTheme
            ? mixWithBlack(rgb, 0.12)
            : mixWithWhite(rgb, 0.72);
        const accentMid = effectiveTheme === themeRuntime.lightTheme
            ? mixWithBlack(rgb, 0.22)
            : mixWithWhite(rgb, 0.28);

        root.style.setProperty("--accent-color", accentHex);
        root.style.setProperty("--accent-rgb", `${rgb.red}, ${rgb.green}, ${rgb.blue}`);
        root.style.setProperty("--gold", accentHex);
        root.style.setProperty("--accent", accentHex);
        root.style.setProperty("--gold-text", buildColor(accentText.red, accentText.green, accentText.blue));
        root.style.setProperty("--gold-light", buildColor(accentLight.red, accentLight.green, accentLight.blue));
        root.style.setProperty("--gold-mid", buildColor(accentMid.red, accentMid.green, accentMid.blue));
        root.style.setProperty(
            "--gold-gradient",
            `linear-gradient(135deg, ${buildColor(accentLight.red, accentLight.green, accentLight.blue)}, ${buildColor(accentMid.red, accentMid.green, accentMid.blue)})`);

        [
            ["--gold-03", 0.03],
            ["--gold-04", 0.04],
            ["--gold-05", 0.05],
            ["--gold-06", 0.06],
            ["--gold-07", effectiveTheme === themeRuntime.lightTheme ? 0.08 : 0.07],
            ["--gold-08", effectiveTheme === themeRuntime.lightTheme ? 0.10 : 0.08],
            ["--gold-09", effectiveTheme === themeRuntime.lightTheme ? 0.12 : 0.09],
            ["--gold-10", effectiveTheme === themeRuntime.lightTheme ? 0.14 : 0.10],
            ["--gold-12", effectiveTheme === themeRuntime.lightTheme ? 0.18 : 0.12],
            ["--gold-14", effectiveTheme === themeRuntime.lightTheme ? 0.22 : 0.14],
            ["--gold-15", effectiveTheme === themeRuntime.lightTheme ? 0.25 : 0.15],
            ["--gold-16", effectiveTheme === themeRuntime.lightTheme ? 0.28 : 0.16],
            ["--gold-20", effectiveTheme === themeRuntime.lightTheme ? 0.32 : 0.20],
            ["--gold-25", effectiveTheme === themeRuntime.lightTheme ? 0.38 : 0.25],
            ["--gold-30", effectiveTheme === themeRuntime.lightTheme ? 0.45 : 0.30],
            ["--gold-35", effectiveTheme === themeRuntime.lightTheme ? 0.52 : 0.35],
            ["--gold-45", effectiveTheme === themeRuntime.lightTheme ? 0.65 : 0.45]
        ].forEach(([token, alpha]) => root.style.setProperty(token, buildRgba(rgb.red, rgb.green, rgb.blue, alpha)));
    }

    function applySettingsTheme(theme, accentColor, density) {
        const themeRuntime = getThemeRuntime();
        selectedTheme = theme ?? themeRuntime.darkTheme;
        const effectiveTheme = resolveEffectiveTheme(selectedTheme);

        root.setAttribute(themeRuntime.themeRootAttribute, effectiveTheme);
        root.setAttribute(themeRuntime.themeSourceAttribute, selectedTheme);
        root.setAttribute(themeRuntime.densityRootAttribute, density ?? themeRuntime.defaultDensity);
        root.classList.toggle(themeRuntime.themeLightClass, effectiveTheme === themeRuntime.lightTheme);
        root.classList.toggle(themeRuntime.themeDarkClass, effectiveTheme === themeRuntime.darkTheme);
        setBodyClass(themeRuntime.themeLightClass, effectiveTheme === themeRuntime.lightTheme);
        setBodyClass(themeRuntime.themeDarkClass, effectiveTheme === themeRuntime.darkTheme);
        setAccentTokens(accentColor ?? themeRuntime.defaultAccent, effectiveTheme);
    }

    function loadStoredPreferences() {
        const themeRuntime = getThemeRuntime();
        try {
            const stored = window.localStorage.getItem(themeRuntime.settingsPageStorageKey);
            if (!stored) {
                applySettingsTheme(themeRuntime.darkTheme, themeRuntime.defaultAccent, themeRuntime.defaultDensity);
                return;
            }

            const preferences = JSON.parse(stored);
            applySettingsTheme(
                preferences?.ColorScheme ?? themeRuntime.darkTheme,
                preferences?.AccentColor ?? themeRuntime.defaultAccent,
                preferences?.UiDensity ?? themeRuntime.defaultDensity);
        } catch {
            applySettingsTheme(themeRuntime.darkTheme, themeRuntime.defaultAccent, themeRuntime.defaultDensity);
        }
    }

    if (themeMedia) {
        themeMedia.addEventListener("change", () => {
            if (selectedTheme === getThemeRuntime().systemTheme) {
                loadStoredPreferences();
            }
        });
    }

    window[getThemeRuntime().themeGlobalName] = {
        applySettingsTheme
    };

    loadStoredPreferences();
})();
