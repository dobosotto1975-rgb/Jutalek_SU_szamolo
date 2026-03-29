console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initCustomSelects();
    initCalculator();
});

function initCustomSelects() {
    const selects = document.querySelectorAll(".custom-select");

    selects.forEach(initCustomSelect);

    document.addEventListener("click", function (e) {
        const insideSelect = e.target.closest(".custom-select");
        if (!insideSelect) {
            closeAllCustomSelects();
        }
    });

    function initCustomSelect(wrapper) {
        const targetId = wrapper.dataset.target;
        const realSelect = document.getElementById(targetId);
        const trigger = wrapper.querySelector(".custom-select-trigger");
        const text = wrapper.querySelector(".custom-select-text");
        const search = wrapper.querySelector(".custom-select-search");
        const options = Array.from(wrapper.querySelectorAll(".custom-select-option"));
        const placeholder = wrapper.dataset.placeholder || "-- válassz --";

        if (!realSelect || !trigger || !text || !search || options.length === 0) {
            return;
        }

        syncFromRealSelect();

        trigger.addEventListener("click", function (e) {
            e.preventDefault();
            const isOpen = wrapper.classList.contains("open");
            closeAllCustomSelects();

            if (!isOpen) {
                wrapper.classList.add("open");
                search.value = "";
                filterOptions("");
                setTimeout(() => search.focus(), 10);
            }
        });

        search.addEventListener("input", function () {
            filterOptions(search.value);
        });

        options.forEach(option => {
            option.addEventListener("click", function () {
                const value = option.dataset.value || "";
                const label = option.dataset.text || placeholder;

                realSelect.value = value;
                text.textContent = label;

                options.forEach(o => o.classList.remove("selected"));
                option.classList.add("selected");

                wrapper.classList.remove("open");
                search.value = "";
                filterOptions("");

                realSelect.dispatchEvent(new Event("change", { bubbles: true }));
            });
        });

        function syncFromRealSelect() {
            const selectedOption = realSelect.options[realSelect.selectedIndex];
            if (selectedOption && selectedOption.value !== "") {
                text.textContent = selectedOption.text;
                options.forEach(o => {
                    o.classList.toggle("selected", o.dataset.value === selectedOption.value);
                });
            } else {
                text.textContent = placeholder;
                options.forEach(o => o.classList.remove("selected"));
            }
        }

        function filterOptions(term) {
            const normalized = (term || "").trim().toLowerCase();
            let visibleCount = 0;

            options.forEach(option => {
                const content = (option.dataset.text || "").toLowerCase();
                const visible = content.includes(normalized);
                option.classList.toggle("hidden", !visible);

                if (visible) {
                    visibleCount++;
                }
            });

            let empty = wrapper.querySelector(".custom-select-empty");
            if (visibleCount === 0) {
                if (!empty) {
                    empty = document.createElement("div");
                    empty.className = "custom-select-empty";
                    empty.textContent = "Nincs találat.";
                    wrapper.querySelector(".custom-select-options").appendChild(empty);
                }
            } else if (empty) {
                empty.remove();
            }
        }
    }

    function closeAllCustomSelects() {
        document.querySelectorAll(".custom-select.open").forEach(x => x.classList.remove("open"));
    }
}

function initCalculator() {
    const productInput = document.getElementById("Product");
    const amountInput = document.getElementById("Amount");
    const commissionInput = document.getElementById("Commission");
    const suInput = document.getElementById("Su");

    if (!productInput || !amountInput || !commissionInput || !suInput || !window.productRules) {
        return;
    }

    const recalculate = () => {
        const product = productInput.value || "";
        const amount = parseHungarianNumber(amountInput.value);

        if (!product || amount <= 0) {
            commissionInput.value = "0";
            suInput.value = "0";
            return;
        }

        const rule = window.productRules[product];
        if (!rule) {
            commissionInput.value = "0";
            suInput.value = "0";
            return;
        }

        let commission = 0;
        let su = 0;

        if (rule.divisor && Number(rule.divisor) > 0) {
            su = amount / Number(rule.divisor);
        }

        if (rule.mode === "percent" && rule.percent !== null && rule.percent !== undefined) {
            commission = amount * Number(rule.percent);
        } else if (rule.mode === "none") {
            commission = 0;
        } else if (rule.mode === "db") {
            commission = 0;
        }

        commissionInput.value = roundNumber(commission, 0);
        suInput.value = roundNumber(su, 4);
    };

    productInput.addEventListener("change", recalculate);
    amountInput.addEventListener("input", recalculate);

    recalculate();
}

function parseHungarianNumber(value) {
    if (!value) return 0;

    const cleaned = value
        .toString()
        .replace(/\s/g, "")
        .replace(/\./g, "")
        .replace(",", ".");

    const parsed = parseFloat(cleaned);
    return isNaN(parsed) ? 0 : parsed;
}

function roundNumber(value, decimals) {
    const factor = Math.pow(10, decimals);
    const rounded = Math.round((value + Number.EPSILON) * factor) / factor;
    return rounded.toFixed(decimals).replace(".", ",");
}