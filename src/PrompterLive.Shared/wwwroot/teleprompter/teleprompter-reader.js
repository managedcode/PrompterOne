(function () {
    const teleprompterReaderNamespace = "TeleprompterReaderInterop";

    window[teleprompterReaderNamespace] = {
        measureClusterOffset(stageId, textId, targetWordId, focalPointPercent, neutralizeCard) {
            const stage = document.getElementById(stageId);
            const text = document.getElementById(textId);
            const targetWord = document.getElementById(targetWordId);
            const card = Boolean(neutralizeCard)
                ? targetWord instanceof HTMLElement
                    ? targetWord.closest(".rd-card")
                    : null
                : null;

            if (!(stage instanceof HTMLElement) || !(text instanceof HTMLElement) || !(targetWord instanceof HTMLElement)) {
                return null;
            }

            const previousTransition = text.style.transition;
            const previousTransform = text.style.transform;
            const previousCardOpacity = card instanceof HTMLElement ? card.style.opacity : "";
            const previousCardTransition = card instanceof HTMLElement ? card.style.transition : "";
            const previousCardTransform = card instanceof HTMLElement ? card.style.transform : "";

            if (card instanceof HTMLElement) {
                card.style.opacity = "0";
                card.style.transition = "none";
                card.style.transform = "translateY(0)";
            }

            text.style.transition = "none";
            text.style.transform = "none";
            void text.offsetHeight;

            const stageRect = stage.getBoundingClientRect();
            const wordRect = targetWord.getBoundingClientRect();
            const focalPoint = stageRect.top + (stageRect.height * (Number(focalPointPercent) / 100));
            const offset = focalPoint - (wordRect.top + (wordRect.height / 2));

            text.style.transform = previousTransform;
            text.style.transition = previousTransition;
            if (card instanceof HTMLElement) {
                card.style.opacity = previousCardOpacity;
                card.style.transition = previousCardTransition;
                card.style.transform = previousCardTransform;
            }

            return Number.isFinite(offset) ? offset : null;
        }
    };
})();
