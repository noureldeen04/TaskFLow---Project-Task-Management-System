(() => {
    const app = document.getElementById("tf-app");
    const sidebar = document.getElementById("tf-sidebar");
    const openButton = document.getElementById("tf-sidebar-open");
    const closeButton = document.getElementById("tf-sidebar-close");
    const collapseButton = document.getElementById("tf-sidebar-collapse");
    const backdrop = document.getElementById("tf-sidebar-backdrop");
    const desktopQuery = window.matchMedia("(min-width: 992px)");

    if (!app || !sidebar || !backdrop) return;

    const focusableSelector = 'a[href], button:not([disabled]), input:not([disabled]), [tabindex]:not([tabindex="-1"])';

    function openMobileSidebar() {
        sidebar.classList.add("is-open");
        backdrop.hidden = false;
        openButton?.setAttribute("aria-expanded", "true");
        document.body.classList.add("tf-no-scroll");
        closeButton?.focus();
    }

    function closeMobileSidebar(returnFocus = true) {
        sidebar.classList.remove("is-open");
        backdrop.hidden = true;
        openButton?.setAttribute("aria-expanded", "false");
        document.body.classList.remove("tf-no-scroll");
        if (returnFocus) openButton?.focus();
    }

    openButton?.addEventListener("click", openMobileSidebar);
    closeButton?.addEventListener("click", () => closeMobileSidebar());
    backdrop.addEventListener("click", () => closeMobileSidebar());

    collapseButton?.addEventListener("click", () => {
        if (!desktopQuery.matches) return;
        const collapsed = app.classList.toggle("is-sidebar-collapsed");
        collapseButton.setAttribute("aria-expanded", String(!collapsed));
        collapseButton.querySelector("span")?.replaceChildren(
            document.createTextNode(collapsed ? "Expand sidebar" : "Collapse sidebar")
        );
        collapseButton.title = collapsed ? "Expand sidebar" : "Collapse sidebar";
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && sidebar.classList.contains("is-open")) {
            closeMobileSidebar();
        }

        if (event.key === "Tab" && sidebar.classList.contains("is-open")) {
            const focusable = [...sidebar.querySelectorAll(focusableSelector)]
                .filter(element => element.offsetParent !== null);
            if (!focusable.length) return;

            const first = focusable[0];
            const last = focusable[focusable.length - 1];

            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault();
                last.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault();
                first.focus();
            }
        }
    });

    desktopQuery.addEventListener("change", event => {
        if (event.matches) closeMobileSidebar(false);
    });
})();
