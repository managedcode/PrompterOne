export function openFilePicker(inputId) {
    const input = typeof inputId === "string"
        ? document.getElementById(inputId)
        : null;

    if (!(input instanceof HTMLInputElement)) {
        return;
    }

    input.click();
}
