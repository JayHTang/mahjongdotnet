// Global scripts for Mahjong

window.parseBool = function (val) {
    return String(val).toLowerCase() === "true";
};

window.trackEvent = function (eventName, params) {
    if (typeof gtag === "function") {
        gtag("event", eventName, params || {});
    }
};

// Prevent double submission only when form submission was not prevented by validation
jQuery.fn.preventDoubleSubmission = function () {
    $(this).on("submit", function (e) {
        if (e.isDefaultPrevented()) {
            return;
        }
        var $form = $(this);
        if ($form.data("submitted") === true) {
            e.preventDefault();
        } else {
            $form.data("submitted", true);
        }
    });
    return this;
};

$(function () {
    // Theme toggle initialization (persists manual override, respects system preference)
    var rootEl = document.documentElement;
    var themeToggle = document.getElementById("themeToggle");
    var themeLabel = document.getElementById("themeLabel");

    function updateThemeLabel() {
        if (!themeLabel) return;
        var current = rootEl.getAttribute("data-theme") || "light";
        themeLabel.textContent = current === "dark" ? "浅色" : "深色";
    }

    function applyTheme(nextTheme, persist, notifyAnalytics) {
        rootEl.setAttribute("data-theme", nextTheme);
        updateThemeLabel();
        if (persist) {
            try {
                localStorage.setItem("theme", nextTheme);
            } catch (e) { }
        }
        if (notifyAnalytics) {
            window.trackEvent("theme_toggle", { theme: nextTheme });
        }
        window.dispatchEvent(new CustomEvent("mahjong:themechange", { detail: { theme: nextTheme } }));
    }

    updateThemeLabel();

    if (themeToggle) {
        themeToggle.addEventListener("click", function () {
            var current = rootEl.getAttribute("data-theme") || "light";
            var next = current === "dark" ? "light" : "dark";
            applyTheme(next, true, true);
        });
    }

    if (window.matchMedia) {
        var mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
        if (mediaQuery && typeof mediaQuery.addEventListener === "function") {
            mediaQuery.addEventListener("change", function (e) {
                try {
                    if (!localStorage.getItem("theme")) {
                        applyTheme(e.matches ? "dark" : "light", false, false);
                    }
                } catch (err) { }
            });
        }
    }

    // Automatically trim leading and trailing spaces for text inputs and textareas on blur
    $(document).on("blur", "input[type='text'], input[type='search'], input[type='email'], input[type='url'], input[type='tel'], input:not([type]), textarea", function () {
        var val = $(this).val();
        if (typeof val === "string") {
            var trimmed = val.trim();
            if (val !== trimmed) {
                $(this).val(trimmed);
            }
        }
    });

    // Also trim before any form submission
    $(document).on("submit", "form", function () {
        $(this).find("input[type='text'], input[type='search'], input[type='email'], input[type='url'], input[type='tel'], input:not([type]), textarea").each(function () {
            var val = $(this).val();
            if (typeof val === "string") {
                var trimmed = val.trim();
                if (val !== trimmed) {
                    $(this).val(trimmed);
                }
            }
        });
    });

    // Defer double-submission guard attachment to end of ready queue so page-specific validators bind first
    setTimeout(function () {
        $("form").preventDoubleSubmission();
    }, 0);
});
