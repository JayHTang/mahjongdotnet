/**
 * Highcharts Helpers - Reusable helper functions, formatting, and Light/Dark theme sync for Mahjong charts.
 */

if (typeof Highcharts !== "undefined") {
    Highcharts.setOptions({
        lang: {
            thousandsSep: ","
        }
    });
}

function parseChartData(raw) {
    if (!raw) return [];
    return typeof raw === "string" ? JSON.parse(raw) : raw;
}

function getMahjongChartTheme() {
    var isDark = (document.documentElement.getAttribute("data-theme") || "light") === "dark";
    var textMain = isDark ? "#f8fafc" : "#0f172a";
    var textSecondary = isDark ? "#cbd5e1" : "#334155";
    var textMuted = isDark ? "#94a3b8" : "#64748b";
    var gridColor = isDark ? "#334155" : "#e2e8f0";
    var tooltipBg = isDark ? "rgba(15, 23, 42, 0.96)" : "rgba(255, 255, 255, 0.96)";
    var tooltipBorder = isDark ? "#475569" : "#cbd5e1";

    return {
        chart: {
            backgroundColor: "transparent",
            style: {
                fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", Roboto, sans-serif'
            }
        },
        title: {
            text: "",
            style: { color: textMain, fontWeight: "700" }
        },
        subtitle: {
            style: { color: textMuted }
        },
        xAxis: {
            gridLineColor: gridColor,
            lineColor: gridColor,
            tickColor: gridColor,
            labels: { style: { color: textSecondary } },
            title: { style: { color: textSecondary } }
        },
        yAxis: {
            gridLineColor: gridColor,
            lineColor: gridColor,
            tickColor: gridColor,
            labels: { style: { color: textSecondary } },
            title: { style: { color: textSecondary } }
        },
        legend: {
            itemStyle: { color: textSecondary, fontWeight: "500" },
            itemHoverStyle: { color: textMain },
            itemHiddenStyle: { color: textMuted }
        },
        tooltip: {
            backgroundColor: tooltipBg,
            borderColor: tooltipBorder,
            style: { color: textMain }
        }
    };
}

window.__mahjongThemedCharts = window.__mahjongThemedCharts || [];

function registerThemedChart(chartInstance) {
    if (!chartInstance) return chartInstance;
    window.__mahjongThemedCharts.push(chartInstance);
    return chartInstance;
}

window.addEventListener("mahjong:themechange", function () {
    if (!window.__mahjongThemedCharts || !window.__mahjongThemedCharts.length) return;
    var themeOpts = getMahjongChartTheme();
    window.__mahjongThemedCharts.forEach(function (chart) {
        if (chart && typeof chart.update === "function" && chart.renderer) {
            chart.update(themeOpts, true);
        }
    });
});
