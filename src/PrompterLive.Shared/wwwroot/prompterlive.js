(function () {
    const documentStorageKey = "prompterlive.library.v1";
    const documentSeedVersionKey = "prompterlive.library.seed-version";
    const folderStorageKey = "prompterlive.folders.v1";
    const folderSeedVersionKey = "prompterlive.folders.seed-version";
    const settingsPrefix = "prompterlive.settings.";
    const cultureSettingKey = settingsPrefix + "culture";
    const defaultCultureName = "en";
    const blockedCulturePrefix = "ru";
    const cultureSeparator = "-";
    const alternateCultureSeparator = "_";
    const supportedCultures = new Set(["en", "uk", "fr", "es", "pt", "it"]);
    const streamMap = new Map();
    const audioMonitorMap = new Map();
    const readerAnimations = new Map();
    const microphoneMonitorFillSelector = "[data-mic-role='fill']";
    const microphoneMonitorValueSelector = "[data-mic-role='value']";
    const microphoneMonitorActiveState = "active";
    const microphoneMonitorIdleState = "idle";
    const microphoneMonitorLevelMultiplier = 2800;
    const shellAutoHideDelayMs = 2400;
    const shellStateOffline = "offline";
    const shellStateOnline = "online";
    const shellErrorUiId = "blazor-error-ui";
    const shellErrorEyebrowId = "app-shell-error-eyebrow";
    const shellErrorTitleId = "app-shell-error-title";
    const shellErrorMessageId = "app-shell-error-message";
    const shellErrorDetailId = "app-shell-error-detail";
    const shellConnectivityUiId = "app-connectivity-ui";
    const shellConnectivityEyebrowId = "app-connectivity-eyebrow";
    const shellConnectivityTitleId = "app-connectivity-title";
    const shellConnectivityMessageId = "app-connectivity-message";
    const shellConnectivityRetryId = "app-connectivity-retry";
    const shellConnectivityDismissId = "app-connectivity-dismiss";
    const shellBootstrapReloadSelector = "[data-testid='diagnostics-bootstrap-reload']";
    const shellBootstrapDismissSelector = "[data-testid='diagnostics-bootstrap-dismiss']";
    const shellText = {
        en: {
            errorEyebrow: "Diagnostics",
            errorTitle: "Prompter.live hit a shell error",
            errorMessage: "The app shell could not recover automatically. Reload the app to restore editing, reading, and live tools.",
            reload: "Reload App",
            dismiss: "Dismiss",
            connectivityEyebrow: "Connection",
            offlineTitle: "Connection lost",
            offlineMessage: "Prompter.live is offline. Live routing, cloud sync, and remote publishing will resume when the browser reconnects.",
            onlineTitle: "Connection restored",
            onlineMessage: "The browser connection is back. Continue working or reload if anything still looks stale.",
            retry: "Retry Now"
        },
        uk: {
            errorEyebrow: "Діагностика",
            errorTitle: "У Prompter.live сталася помилка оболонки",
            errorMessage: "Оболонка застосунку не змогла відновитися автоматично. Перезавантажте застосунок, щоб повернути редагування, читання і live-інструменти.",
            reload: "Перезавантажити",
            dismiss: "Закрити",
            connectivityEyebrow: "Зʼєднання",
            offlineTitle: "Зʼєднання втрачено",
            offlineMessage: "Prompter.live офлайн. Live routing, хмарна синхронізація та віддалена публікація відновляться, коли браузер перепідключиться.",
            onlineTitle: "Зʼєднання відновлено",
            onlineMessage: "Браузер знову онлайн. Можна продовжувати роботу або перезавантажити застосунок, якщо щось усе ще виглядає застарілим.",
            retry: "Спробувати знову"
        },
        fr: {
            errorEyebrow: "Diagnostic",
            errorTitle: "Prompter.live a rencontré une erreur de shell",
            errorMessage: "Le shell de l’application n’a pas pu se rétablir automatiquement. Rechargez l’application pour retrouver l’édition, la lecture et le live.",
            reload: "Recharger",
            dismiss: "Fermer",
            connectivityEyebrow: "Connexion",
            offlineTitle: "Connexion perdue",
            offlineMessage: "Prompter.live est hors ligne. Le routage live, la synchronisation cloud et la publication distante reprendront quand le navigateur se reconnectera.",
            onlineTitle: "Connexion rétablie",
            onlineMessage: "La connexion du navigateur est de retour. Continuez à travailler ou rechargez si quelque chose semble encore obsolète.",
            retry: "Réessayer"
        },
        es: {
            errorEyebrow: "Diagnóstico",
            errorTitle: "Prompter.live encontró un error del shell",
            errorMessage: "El shell de la aplicación no pudo recuperarse automáticamente. Recarga la aplicación para restaurar edición, lectura y herramientas en vivo.",
            reload: "Recargar",
            dismiss: "Cerrar",
            connectivityEyebrow: "Conexión",
            offlineTitle: "Conexión perdida",
            offlineMessage: "Prompter.live está sin conexión. El enrutado en vivo, la sincronización en la nube y la publicación remota se reanudarán cuando el navegador vuelva a conectarse.",
            onlineTitle: "Conexión restaurada",
            onlineMessage: "La conexión del navegador volvió. Sigue trabajando o recarga si algo aún se ve desactualizado.",
            retry: "Reintentar"
        },
        pt: {
            errorEyebrow: "Diagnóstico",
            errorTitle: "O Prompter.live encontrou um erro de shell",
            errorMessage: "O shell do app não conseguiu se recuperar automaticamente. Recarregue o app para restaurar edição, leitura e ferramentas ao vivo.",
            reload: "Recarregar",
            dismiss: "Fechar",
            connectivityEyebrow: "Conexão",
            offlineTitle: "Conexão perdida",
            offlineMessage: "O Prompter.live está offline. O roteamento ao vivo, a sincronização em nuvem e a publicação remota voltarão quando o navegador reconectar.",
            onlineTitle: "Conexão restaurada",
            onlineMessage: "A conexão do navegador voltou. Continue trabalhando ou recarregue se algo ainda parecer desatualizado.",
            retry: "Tentar novamente"
        },
        it: {
            errorEyebrow: "Diagnostica",
            errorTitle: "Prompter.live ha rilevato un errore della shell",
            errorMessage: "La shell dell’app non è riuscita a riprendersi automaticamente. Ricarica l’app per ripristinare modifica, lettura e strumenti live.",
            reload: "Ricarica",
            dismiss: "Chiudi",
            connectivityEyebrow: "Connessione",
            offlineTitle: "Connessione persa",
            offlineMessage: "Prompter.live è offline. Il routing live, la sincronizzazione cloud e la pubblicazione remota riprenderanno quando il browser si riconnetterà.",
            onlineTitle: "Connessione ripristinata",
            onlineMessage: "La connessione del browser è tornata. Continua a lavorare oppure ricarica se qualcosa sembra ancora non aggiornato.",
            retry: "Riprova"
        }
    };
    let connectivityHideTimer = 0;

    function normalizeCultureName(cultureName) {
        if (!cultureName || typeof cultureName !== "string") {
            return "";
        }

        return cultureName
            .trim()
            .replaceAll(alternateCultureSeparator, cultureSeparator)
            .toLowerCase();
    }

    function resolveSupportedCulture(cultureName) {
        const normalizedCulture = normalizeCultureName(cultureName);
        if (!normalizedCulture) {
            return "";
        }

        const languageName = normalizedCulture.split(cultureSeparator)[0];
        if (languageName === blockedCulturePrefix) {
            return defaultCultureName;
        }

        return supportedCultures.has(languageName)
            ? languageName
            : "";
    }

    function getBrowserCultures() {
        if (Array.isArray(window.navigator.languages) && window.navigator.languages.length > 0) {
            return window.navigator.languages;
        }

        return [window.navigator.language || defaultCultureName];
    }

    function getPreferredCulture() {
        const storedCulture = resolveSupportedCulture(window.localStorage.getItem(cultureSettingKey));
        if (storedCulture) {
            return storedCulture;
        }

        for (const browserCulture of getBrowserCultures()) {
            const supportedCulture = resolveSupportedCulture(browserCulture);
            if (supportedCulture) {
                return supportedCulture;
            }
        }

        return defaultCultureName;
    }

    function applyDocumentCulture(cultureName) {
        const normalizedCulture = resolveSupportedCulture(cultureName) || defaultCultureName;
        if (document && document.documentElement) {
            document.documentElement.lang = normalizedCulture;
        }

        return normalizedCulture;
    }

    function getShellStrings() {
        const cultureName = resolveSupportedCulture(document?.documentElement?.lang) || defaultCultureName;
        return shellText[cultureName] || shellText[defaultCultureName];
    }

    function setShellText(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    }

    function showShellDetail(detail) {
        const detailElement = document.getElementById(shellErrorDetailId);
        if (!detailElement) {
            return;
        }

        if (!detail) {
            detailElement.hidden = true;
            detailElement.textContent = "";
            return;
        }

        detailElement.hidden = false;
        detailElement.textContent = detail;
    }

    function hideBootstrapError() {
        const errorUi = document.getElementById(shellErrorUiId);
        if (errorUi) {
            errorUi.style.display = "none";
        }
    }

    function updateShellCopy() {
        const strings = getShellStrings();
        setShellText(shellErrorEyebrowId, strings.errorEyebrow);
        setShellText(shellErrorTitleId, strings.errorTitle);
        setShellText(shellErrorMessageId, strings.errorMessage);
        setShellText(shellConnectivityEyebrowId, strings.connectivityEyebrow);

        const reloadButton = document.querySelector(shellBootstrapReloadSelector);
        if (reloadButton) {
            reloadButton.textContent = strings.reload;
        }

        const bootstrapDismissButton = document.querySelector(shellBootstrapDismissSelector);
        if (bootstrapDismissButton) {
            bootstrapDismissButton.textContent = strings.dismiss;
        }

        const retryButton = document.getElementById(shellConnectivityRetryId);
        if (retryButton) {
            retryButton.textContent = strings.retry;
        }

        const connectivityDismissButton = document.getElementById(shellConnectivityDismissId);
        if (connectivityDismissButton) {
            connectivityDismissButton.textContent = strings.dismiss;
        }
    }

    function hideConnectivityStatus() {
        const connectivityUi = document.getElementById(shellConnectivityUiId);
        if (!connectivityUi) {
            return;
        }

        window.clearTimeout(connectivityHideTimer);
        connectivityHideTimer = 0;
        connectivityUi.hidden = true;
        delete connectivityUi.dataset.state;
    }

    function showConnectivityStatus(state) {
        const connectivityUi = document.getElementById(shellConnectivityUiId);
        if (!connectivityUi) {
            return;
        }

        const strings = getShellStrings();
        const isOnline = state === shellStateOnline;
        setShellText(
            shellConnectivityTitleId,
            isOnline ? strings.onlineTitle : strings.offlineTitle);
        setShellText(
            shellConnectivityMessageId,
            isOnline ? strings.onlineMessage : strings.offlineMessage);

        connectivityUi.hidden = false;
        connectivityUi.dataset.state = state;

        window.clearTimeout(connectivityHideTimer);
        connectivityHideTimer = 0;

        if (isOnline) {
            connectivityHideTimer = window.setTimeout(hideConnectivityStatus, shellAutoHideDelayMs);
        }
    }

    function showBootstrapError(detail) {
        const errorUi = document.getElementById(shellErrorUiId);
        if (!errorUi) {
            return;
        }

        updateShellCopy();
        showShellDetail(detail);
        errorUi.style.display = "grid";
    }

    function initializeAppShell() {
        updateShellCopy();

        const errorUi = document.getElementById(shellErrorUiId);
        if (errorUi) {
            const errorUiObserver = new MutationObserver(() => {
                if (errorUi.style.display && errorUi.style.display !== "none") {
                    errorUi.style.display = "grid";
                }
            });

            errorUiObserver.observe(errorUi, {
                attributes: true,
                attributeFilter: ["style"]
            });
        }

        const bootstrapDismissButton = document.querySelector(shellBootstrapDismissSelector);
        if (bootstrapDismissButton) {
            bootstrapDismissButton.addEventListener("click", hideBootstrapError);
        }

        const connectivityRetryButton = document.getElementById(shellConnectivityRetryId);
        if (connectivityRetryButton) {
            connectivityRetryButton.addEventListener("click", () => window.location.reload());
        }

        const connectivityDismissButton = document.getElementById(shellConnectivityDismissId);
        if (connectivityDismissButton) {
            connectivityDismissButton.addEventListener("click", hideConnectivityStatus);
        }

        window.addEventListener("offline", () => showConnectivityStatus(shellStateOffline));
        window.addEventListener("online", () => showConnectivityStatus(shellStateOnline));
        window.addEventListener("error", event => {
            if (event?.message) {
                showBootstrapError(event.message);
            }
        });
        window.addEventListener("unhandledrejection", event => {
            const reason = event?.reason;
            const detail = typeof reason === "string"
                ? reason
                : reason?.message || "";

            showBootstrapError(detail);
        });

        if (window.navigator && window.navigator.onLine === false) {
            showConnectivityStatus(shellStateOffline);
        }
    }

    function normalizeDocument(document) {
        if (!document) {
            return null;
        }

        if (Array.isArray(document)) {
            const [id, title, text, documentName, updatedAt] = document;
            return {
                id: id || "",
                title: title || "Untitled Script",
                text: text || "",
                documentName: documentName || "untitled-script.tps",
                updatedAt: updatedAt || new Date().toISOString(),
                folderId: null
            };
        }

        if (typeof document !== "object") {
            return null;
        }

        return {
            id: document.id ?? document.Id ?? "",
            title: document.title ?? document.Title ?? "Untitled Script",
            text: document.text ?? document.Text ?? "",
            documentName: document.documentName ?? document.DocumentName ?? "untitled-script.tps",
            updatedAt: document.updatedAt ?? document.UpdatedAt ?? new Date().toISOString(),
            folderId: document.folderId ?? document.FolderId ?? null
        };
    }

    function normalizeFolder(folder) {
        if (!folder || typeof folder !== "object") {
            return null;
        }

        return {
            id: folder.id ?? folder.Id ?? "",
            name: folder.name ?? folder.Name ?? "Untitled Folder",
            parentId: folder.parentId ?? folder.ParentId ?? null,
            displayOrder: Number.isFinite(folder.displayOrder ?? folder.DisplayOrder)
                ? folder.displayOrder ?? folder.DisplayOrder
                : 0,
            updatedAt: folder.updatedAt ?? folder.UpdatedAt ?? new Date().toISOString()
        };
    }

    function normalizeFolders(folders) {
        if (!Array.isArray(folders)) {
            return [];
        }

        return folders
            .map(normalizeFolder)
            .filter(Boolean);
    }

    function normalizeDocuments(documents) {
        if (!documents) {
            return [];
        }

        if (Array.isArray(documents)) {
            if (documents.length === 0) {
                return [];
            }

            if (typeof documents[0] !== "object" || documents[0] === null) {
                return normalizeDocument(documents) ? [normalizeDocument(documents)] : [];
            }

            return documents.map(normalizeDocument).filter(Boolean);
        }

        const normalized = normalizeDocument(documents);
        return normalized ? [normalized] : [];
    }

    function readDocuments() {
        try {
            const raw = window.localStorage.getItem(documentStorageKey);
            if (!raw) {
                return [];
            }

            const parsed = JSON.parse(raw);
            if (Array.isArray(parsed)) {
                return normalizeDocuments(parsed);
            }

            return normalizeDocuments(parsed);
        } catch {
            return [];
        }
    }

    function writeDocuments(documents) {
        window.localStorage.setItem(documentStorageKey, JSON.stringify(documents));
    }

    function readFolders() {
        try {
            const raw = window.localStorage.getItem(folderStorageKey);
            if (!raw) {
                return [];
            }

            return normalizeFolders(JSON.parse(raw));
        } catch {
            return [];
        }
    }

    function writeFolders(folders) {
        window.localStorage.setItem(folderStorageKey, JSON.stringify(folders));
    }

    async function stopStream(stream) {
        if (!stream) {
            return;
        }

        stream.getTracks().forEach(track => track.stop());
    }

    function getMicrophoneMonitorElements(rootElementId) {
        const root = document.getElementById(rootElementId);
        if (!root) {
            return null;
        }

        return {
            root,
            fill: root.querySelector(microphoneMonitorFillSelector),
            value: root.querySelector(microphoneMonitorValueSelector)
        };
    }

    function updateMicrophoneMonitorUi(rootElementId, levelPercent) {
        const elements = getMicrophoneMonitorElements(rootElementId);
        if (!elements) {
            return;
        }

        const normalizedLevel = Number.isFinite(levelPercent)
            ? Math.max(0, Math.min(100, Math.round(levelPercent)))
            : 0;

        elements.root.dataset.liveLevel = normalizedLevel.toString();
        elements.root.dataset.liveState = normalizedLevel > 0
            ? microphoneMonitorActiveState
            : microphoneMonitorIdleState;

        if (elements.fill) {
            elements.fill.style.width = `${normalizedLevel}%`;
        }

        if (elements.value) {
            elements.value.textContent = `${normalizedLevel}%`;
        }
    }

    async function stopMicrophoneLevelMonitor(rootElementId) {
        const monitor = audioMonitorMap.get(rootElementId);
        if (!monitor) {
            updateMicrophoneMonitorUi(rootElementId, 0);
            return;
        }

        audioMonitorMap.delete(rootElementId);

        if (monitor.frameHandle) {
            window.cancelAnimationFrame(monitor.frameHandle);
        }

        monitor.sourceNode?.disconnect();
        monitor.analyser?.disconnect();
        await stopStream(monitor.stream);
        if (monitor.audioContext) {
            await monitor.audioContext.close().catch(() => {});
        }
        updateMicrophoneMonitorUi(rootElementId, 0);
    }

    async function startMicrophoneLevelMonitor(rootElementId, deviceId) {
        const root = document.getElementById(rootElementId);
        if (!root || !navigator.mediaDevices?.getUserMedia) {
            return;
        }

        await stopMicrophoneLevelMonitor(rootElementId);
        updateMicrophoneMonitorUi(rootElementId, 0);

        let stream = null;
        let audioContext = null;
        let analyser = null;
        let sourceNode = null;

        try {
            stream = await navigator.mediaDevices.getUserMedia({
                audio: deviceId ? { deviceId: { exact: deviceId } } : true,
                video: false
            });

            audioContext = new AudioContext();
            analyser = audioContext.createAnalyser();
            analyser.fftSize = 1024;
            analyser.smoothingTimeConstant = 0.82;

            sourceNode = audioContext.createMediaStreamSource(stream);
            sourceNode.connect(analyser);

            const samples = new Uint8Array(analyser.fftSize);
            const monitor = {
                stream,
                audioContext,
                analyser,
                sourceNode,
                frameHandle: 0
            };

            const step = () => {
                if (!audioMonitorMap.has(rootElementId)) {
                    return;
                }

                analyser.getByteTimeDomainData(samples);

                let sumSquares = 0;
                for (let index = 0; index < samples.length; index += 1) {
                    const normalizedSample = (samples[index] - 128) / 128;
                    sumSquares += normalizedSample * normalizedSample;
                }

                const rms = Math.sqrt(sumSquares / samples.length);
                updateMicrophoneMonitorUi(rootElementId, rms * microphoneMonitorLevelMultiplier);
                monitor.frameHandle = window.requestAnimationFrame(step);
            };

            await audioContext.resume().catch(() => {});
            audioMonitorMap.set(rootElementId, monitor);
            step();
        } catch (error) {
            sourceNode?.disconnect();
            analyser?.disconnect();
            await stopStream(stream);
            if (audioContext) {
                await audioContext.close().catch(() => {});
            }
            updateMicrophoneMonitorUi(rootElementId, 0);
            throw error;
        }
    }

    applyDocumentCulture(getPreferredCulture());
    initializeAppShell();

    window.PrompterLive = {
        localization: {
            getPreferredCulture() {
                return applyDocumentCulture(getPreferredCulture());
            },
            setPreferredCulture(cultureName) {
                const normalizedCulture = applyDocumentCulture(cultureName);
                window.localStorage.setItem(cultureSettingKey, normalizedCulture);
                updateShellCopy();
                return normalizedCulture;
            }
        },
        shell: {
            hideBootstrapError,
            hideConnectivityStatus,
            showBootstrapError,
            showConnectivityOffline() {
                showConnectivityStatus(shellStateOffline);
            },
            showConnectivityOnline() {
                showConnectivityStatus(shellStateOnline);
            }
        },
        storage: {
            ensureSeedData(seedDocuments) {
                const current = readDocuments();
                const normalizedSeedDocuments = normalizeDocuments(seedDocuments);

                if (current.length === 0) {
                    writeDocuments(normalizedSeedDocuments);
                    return;
                }

                const existingIds = new Set(current.map(document => document.id));
                const merged = current.concat(
                    normalizedSeedDocuments.filter(document => !existingIds.has(document.id))
                );

                writeDocuments(merged);
            },
            getSeedVersion() {
                return window.localStorage.getItem(documentSeedVersionKey);
            },
            setSeedVersion(version) {
                if (!version) {
                    window.localStorage.removeItem(documentSeedVersionKey);
                    return;
                }

                window.localStorage.setItem(documentSeedVersionKey, version);
            },
            getFolderSeedVersion() {
                return window.localStorage.getItem(folderSeedVersionKey);
            },
            setFolderSeedVersion(version) {
                if (!version) {
                    window.localStorage.removeItem(folderSeedVersionKey);
                    return;
                }

                window.localStorage.setItem(folderSeedVersionKey, version);
            },
            listDocuments() {
                return readDocuments();
            },
            listDocumentsJson() {
                return JSON.stringify(readDocuments());
            },
            getDocument(id) {
                return readDocuments().find(document => document.id === id) ?? null;
            },
            getDocumentJson(id) {
                return JSON.stringify(window.PrompterLive.storage.getDocument(id));
            },
            saveDocument(document) {
                const normalizedDocument = normalizeDocument(document);
                if (!normalizedDocument) {
                    return null;
                }

                const documents = readDocuments();
                const index = documents.findIndex(item => item.id === normalizedDocument.id);
                if (index >= 0) {
                    documents[index] = normalizedDocument;
                } else {
                    documents.push(normalizedDocument);
                }

                writeDocuments(documents);
                return normalizedDocument;
            },
            saveDocumentJson(document) {
                return JSON.stringify(window.PrompterLive.storage.saveDocument(document));
            },
            deleteDocument(id) {
                const documents = readDocuments().filter(document => document.id !== id);
                writeDocuments(documents);
            },
            listFolders() {
                return readFolders();
            },
            listFoldersJson() {
                return JSON.stringify(readFolders());
            },
            saveFolder(folder) {
                const normalizedFolder = normalizeFolder(folder);
                if (!normalizedFolder) {
                    return null;
                }

                const folders = readFolders();
                const index = folders.findIndex(item => item.id === normalizedFolder.id);
                if (index >= 0) {
                    folders[index] = normalizedFolder;
                } else {
                    folders.push(normalizedFolder);
                }

                writeFolders(folders);
                return normalizedFolder;
            },
            saveFolderJson(folder) {
                return JSON.stringify(window.PrompterLive.storage.saveFolder(folder));
            }
        },

        settings: {
            loadJson(key) {
                return window.localStorage.getItem(settingsPrefix + key);
            },
            load(key) {
                try {
                    const raw = window.localStorage.getItem(settingsPrefix + key);
                    return raw ? JSON.parse(raw) : null;
                } catch {
                    return null;
                }
            },
            saveJson(key, json) {
                if (typeof json !== "string") {
                    window.localStorage.removeItem(settingsPrefix + key);
                    return;
                }

                window.localStorage.setItem(settingsPrefix + key, json);
            },
            save(key, value) {
                window.localStorage.setItem(settingsPrefix + key, JSON.stringify(value));
            }
        },

        downloads: {
            saveText(fileName, content) {
                const blob = new Blob([content], { type: "text/plain;charset=utf-8" });
                const url = URL.createObjectURL(blob);
                const anchor = document.createElement("a");
                anchor.href = url;
                anchor.download = fileName || "script.tps";
                anchor.click();
                URL.revokeObjectURL(url);
            }
        },

        media: {
            async queryPermissions() {
                const state = { cameraGranted: false, microphoneGranted: false };

                if (!navigator.permissions?.query) {
                    return state;
                }

                try {
                    const camera = await navigator.permissions.query({ name: "camera" });
                    state.cameraGranted = camera.state === "granted";
                } catch {
                }

                try {
                    const microphone = await navigator.permissions.query({ name: "microphone" });
                    state.microphoneGranted = microphone.state === "granted";
                } catch {
                }

                return state;
            },

            async requestPermissions() {
                try {
                    const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
                    await stopStream(stream);
                } catch {
                }

                return await window.PrompterLive.media.queryPermissions();
            },

            async listDevices() {
                if (!navigator.mediaDevices?.enumerateDevices) {
                    return [];
                }

                const devices = await navigator.mediaDevices.enumerateDevices();
                return devices.map((device, index) => ({
                    deviceId: device.deviceId,
                    label: device.label || `${device.kind} ${index + 1}`,
                    kind: device.kind,
                    isDefault: device.deviceId === "default"
                }));
            },

            async attachCamera(elementId, deviceId, muted) {
                const element = document.getElementById(elementId);
                if (!element || !navigator.mediaDevices?.getUserMedia) {
                    return;
                }

                if (streamMap.has(elementId)) {
                    await stopStream(streamMap.get(elementId));
                }

                const stream = await navigator.mediaDevices.getUserMedia({
                    video: deviceId ? { deviceId: { exact: deviceId } } : true,
                    audio: false
                });

                element.srcObject = stream;
                element.muted = muted !== false;
                element.playsInline = true;
                await element.play().catch(() => {});
                streamMap.set(elementId, stream);
            },

            async detachCamera(elementId) {
                const stream = streamMap.get(elementId);
                if (stream) {
                    await stopStream(stream);
                    streamMap.delete(elementId);
                }

                const element = document.getElementById(elementId);
                if (element) {
                    element.srcObject = null;
                }
            },

            async startMicrophoneLevelMonitor(elementId, deviceId) {
                await startMicrophoneLevelMonitor(elementId, deviceId);
            },

            async stopMicrophoneLevelMonitor(elementId) {
                await stopMicrophoneLevelMonitor(elementId);
            }
        },

        reader: {
            startAutoScroll(elementId, pixelsPerSecond) {
                const element = document.getElementById(elementId);
                if (!element) {
                    return;
                }

                window.PrompterLive.reader.stopAutoScroll(elementId);

                let lastTimestamp = 0;
                const step = timestamp => {
                    if (!readerAnimations.has(elementId)) {
                        return;
                    }

                    if (!lastTimestamp) {
                        lastTimestamp = timestamp;
                    }

                    const delta = (timestamp - lastTimestamp) / 1000;
                    lastTimestamp = timestamp;
                    element.scrollTop += pixelsPerSecond * delta;

                    if (element.scrollTop + element.clientHeight >= element.scrollHeight) {
                        readerAnimations.delete(elementId);
                        return;
                    }

                    readerAnimations.set(elementId, requestAnimationFrame(step));
                };

                readerAnimations.set(elementId, requestAnimationFrame(step));
            },

            stopAutoScroll(elementId) {
                const frame = readerAnimations.get(elementId);
                if (frame) {
                    cancelAnimationFrame(frame);
                    readerAnimations.delete(elementId);
                }
            }
        }
    };

})();
