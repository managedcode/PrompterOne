(function () {
    const teleprompterReaderNamespace = "TeleprompterReaderInterop";

    window[teleprompterReaderNamespace] = {
        measureClusterOffset(stageId, textId, targetWordId, focalPointPercent) {
            const stage = document.getElementById(stageId);
            const text = document.getElementById(textId);
            const targetWord = document.getElementById(targetWordId);

            if (!(stage instanceof HTMLElement) || !(text instanceof HTMLElement) || !(targetWord instanceof HTMLElement)) {
                return null;
            }

            const previousTransition = text.style.transition;
            const previousTransform = text.style.transform;

            text.style.transition = "none";
            text.style.transform = "none";
            void text.offsetHeight;

            const stageRect = stage.getBoundingClientRect();
            const wordRect = targetWord.getBoundingClientRect();
            const focalPoint = stageRect.top + (stageRect.height * (Number(focalPointPercent) / 100));
            const offset = focalPoint - (wordRect.top + (wordRect.height / 2));

            text.style.transform = previousTransform;
            text.style.transition = previousTransition;

            return Number.isFinite(offset) ? offset : null;
        }
    };
})();
