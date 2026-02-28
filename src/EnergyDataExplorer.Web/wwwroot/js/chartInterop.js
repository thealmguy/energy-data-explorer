// Chart.js interop helpers for Blazor WebAssembly
// Charts are keyed by canvas element ID to allow update and destroy operations.

const charts = {};

window.chartInterop = {
    createChart(elementId, config) {
        const canvas = document.getElementById(elementId);
        if (!canvas) return;

        if (charts[elementId]) {
            charts[elementId].destroy();
        }

        charts[elementId] = new Chart(canvas, config);
    },

    updateChart(elementId, labels, datasets) {
        const chart = charts[elementId];
        if (!chart) return;

        chart.data.labels = labels;
        chart.data.datasets = datasets;
        chart.update();
    },

    destroyChart(elementId) {
        if (charts[elementId]) {
            charts[elementId].destroy();
            delete charts[elementId];
        }
    }
};
