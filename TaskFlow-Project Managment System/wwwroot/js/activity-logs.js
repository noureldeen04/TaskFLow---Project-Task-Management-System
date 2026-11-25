(() => {
    const rowsRoot = document.getElementById("activity-rows");
    if (!rowsRoot) return;

    const controls = {
        search: document.getElementById("activity-search"),
        user: document.getElementById("activity-user"),
        entity: document.getElementById("activity-entity"),
        action: document.getElementById("activity-action"),
        from: document.getElementById("activity-from"),
        to: document.getElementById("activity-to")
    };
    const result = document.getElementById("activity-result");
    const count = document.getElementById("activity-count");
    const emptyRow = rowsRoot.querySelector(".tf-activity-empty");
    const emptyTitle = emptyRow?.querySelector("strong");
    const emptyDescription = emptyRow?.querySelector(".tf-empty-state span");

    const logRows = () => [...rowsRoot.querySelectorAll("tr[data-activity-row]")];
    const normalize = value => (value || "").trim().toLocaleLowerCase();

    function applyFilters() {
        const fromDate = controls.from.value;
        const toDate = controls.to.value;
        if (fromDate && toDate && fromDate > toDate) {
            result.textContent = "The start date must be before the end date.";
            controls.from.focus();
            return;
        }

        const search = normalize(controls.search.value);
        let visibleCount = 0;
        for (const row of logRows()) {
            const rowDate = row.dataset.date || "";
            const matches =
                (!search || normalize(row.textContent).includes(search)) &&
                (!controls.user.value || row.dataset.user === controls.user.value) &&
                (!controls.entity.value || normalize(row.dataset.entity) === normalize(controls.entity.value)) &&
                (!controls.action.value || normalize(row.dataset.action) === normalize(controls.action.value)) &&
                (!fromDate || !rowDate || rowDate >= fromDate) &&
                (!toDate || !rowDate || rowDate <= toDate);
            row.hidden = !matches;
            if (matches) visibleCount++;
        }

        if (emptyRow) {
            emptyRow.hidden = visibleCount > 0;
            if (emptyTitle) emptyTitle.textContent = logRows().length ? "No matching activity" : "No activity has been recorded";
            if (emptyDescription) emptyDescription.textContent = logRows().length
                ? "Change or reset the filters to see more activity."
                : "Activity will appear here after audit logging is implemented.";
        }
        result.textContent = `Showing ${visibleCount} ${visibleCount === 1 ? "record" : "records"}`;
        count.textContent = `${visibleCount} ${visibleCount === 1 ? "record" : "records"}`;
    }

    function resetFilters() {
        for (const control of Object.values(controls)) control.value = "";
        applyFilters();
    }

    function exportCsv() {
        const headers = ["User", "Action", "Entity", "Description", "Date & time"];
        const visibleRows = logRows().filter(row => !row.hidden);
        const csvValue = value => {
            const safeValue = String(value ?? "").replace(/^[\u0000-\u0020]*[=+\-@]/, match => `'${match}`);
            return `"${safeValue.replaceAll('"', '""')}"`;
        };
        const records = [headers, ...visibleRows.map(row => {
            const cells = [...row.querySelectorAll("td")];
            return cells.slice(0, 5).map(cell => cell.innerText.trim());
        })];
        const csv = records.map(record => record.map(csvValue).join(",")).join("\r\n");
        const url = URL.createObjectURL(new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8" }));
        const link = document.createElement("a");
        link.href = url;
        link.download = "activity-logs.csv";
        document.body.append(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
        result.textContent = visibleRows.length
            ? `Exported ${visibleRows.length} visible ${visibleRows.length === 1 ? "record" : "records"}.`
            : "Exported an empty CSV: no audit records are connected.";
    }

    document.getElementById("apply-activity-filters")?.addEventListener("click", applyFilters);
    document.getElementById("reset-activity-filters")?.addEventListener("click", resetFilters);
    document.getElementById("export-activity-csv")?.addEventListener("click", exportCsv);
    controls.search.addEventListener("keydown", event => {
        if (event.key === "Enter") applyFilters();
    });
})();
