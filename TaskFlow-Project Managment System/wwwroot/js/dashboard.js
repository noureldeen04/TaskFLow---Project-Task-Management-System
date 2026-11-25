(() => {
    if (!window.Chart) return;

    const emptyTooltip = {
        callbacks: {
            label: () => "No task data connected"
        }
    };

    const statusCanvas = document.getElementById("task-status-chart");
    if (statusCanvas) {
        new Chart(statusCanvas, {
            type: "doughnut",
            data: { labels: [], datasets: [{ data: [], backgroundColor: [] }] },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false }, tooltip: emptyTooltip }
            }
        });
    }

    const workloadCanvas = document.getElementById("team-workload-chart");
    if (workloadCanvas) {
        new Chart(workloadCanvas, {
            type: "bar",
            data: { labels: [], datasets: [] },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: { x: { display: false }, y: { display: false } },
                plugins: { legend: { display: false }, tooltip: emptyTooltip }
            }
        });
    }
})();