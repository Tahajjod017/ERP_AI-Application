(() => {
    const initialized = new WeakSet();

    function initLazy(el) {
        const key = el.dataset.init;
        if (!key || initialized.has(el)) return;

        const initFunc = window[key];
        if (typeof initFunc === "function") {
            initFunc(el);
            initialized.add(el);
        }
    }

    // Initialize main page elements
    document.querySelectorAll('[data-init]').forEach(initLazy);

    // Watch for new elements in the body (optional)
    const observer = new MutationObserver(() => {
        document.querySelectorAll('[data-init]').forEach(initLazy);
    });
    observer.observe(document.body, { childList: true, subtree: true });
})();
