(() => {
    const root = document.documentElement;
    const toggle = document.getElementById("tf-theme-toggle");
    const icon = document.getElementById("tf-theme-icon");

    function applyTheme(theme) {
        const nextTheme = theme === "dark" ? "dark" : "light";
        root.dataset.theme = nextTheme;
        localStorage.setItem("taskflow-theme", nextTheme);

        if (toggle) {
            const nextLabel = nextTheme === "dark" ? "Switch to light mode" : "Switch to dark mode";
            toggle.setAttribute("aria-label", nextLabel);
            toggle.setAttribute("title", nextLabel);
        }
        if (icon) {
            icon.className = nextTheme === "dark" ? "bi bi-sun" : "bi bi-moon-stars";
        }
    }

    applyTheme(root.dataset.theme || "light");

    toggle?.addEventListener("click", () => {
        applyTheme(root.dataset.theme === "dark" ? "light" : "dark");
    });
})();
