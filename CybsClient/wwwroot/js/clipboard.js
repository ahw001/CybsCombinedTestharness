// Global clipboard helper — loaded once from Components/App.razor.
//
// Lives here rather than in a collocated <Component>.razor.js because its consumer
// (Components/Pages/PageUtils/CopyButton.razor) renders many times per page — the API Log
// sidebar alone mounts dozens of them — and a collocated <script> tag would then be emitted
// once per instance. One global load is correct and is guaranteed to execute regardless of
// the consuming component's render mode.
//
// navigator.clipboard requires a secure context — https://localhost qualifies, as does every
// deployed origin — and the execCommand path covers anything that does not. Same proven
// implementation as Components/Pages/StorePages/StoreCheckout.razor.js.
//
// Returns true/false so the caller can render real feedback instead of assuming success.
window.cybsClipboard = window.cybsClipboard || {};

window.cybsClipboard.copyText = async function (text) {
    if (!text) {
        return false;
    }

    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        }
    } catch (e) {
        console.warn("[cybsClipboard] navigator.clipboard.writeText failed, falling back:", e);
    }

    try {
        var textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.setAttribute('readonly', '');
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';
        document.body.appendChild(textArea);
        textArea.select();
        var copied = document.execCommand('copy');
        document.body.removeChild(textArea);
        return copied;
    } catch (e) {
        console.error("[cybsClipboard] clipboard fallback failed:", e);
        return false;
    }
};
