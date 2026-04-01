console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initCustomSelects();
    initUkQuestion();
    initYesProductFields();
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

        realSelect.addEventListener("change", syncFromRealSelect);

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

function getYesProducts() {
    return [
        "Vienna Yes alapdíj, ha a teljes díj  120e Ft-ig /állománydíjas/",
        "Vienna Yes alapdíj, ha a teljes díj 120-145e Ft között /állománydíjas/",
        "Vienna Yes alapdíj, ha a teljes díj 145e Ft-tól /állománydíjas/"
    ];
}

function isYesProduct(product) {
    return getYesProducts().includes((product || "").trim());
}

function initUkQuestion() {
    const productInput = document.getElementById("Product");
    const ukQuestionWrap = document.getElementById("ukQuestionWrap");
    const ukSelect = document.getElementById("ukSelect");

    if (!productInput || !ukQuestionWrap || !ukSelect || !window.productRules) {
        return;
    }

    const toggleUkQuestion = () => {
        const product = (productInput.value || "").trim();
        const rule = window.productRules[product];
        const requiresUkQuestion = !!(rule && rule.requiresUkQuestion);

        if (requiresUkQuestion) {
            ukQuestionWrap.style.display = "block";
        } else {
            ukQuestionWrap.style.display = "none";
            ukSelect.value = "false";
        }
    };

    toggleUkQuestion();
    productInput.addEventListener("change", toggleUkQuestion);
}

function initYesProductFields() {
    const productInput = document.getElementById("Product");
    const yesWrap = document.getElementById("yesFieldsWrap");
    const fullBaseInput = document.getElementById("YesFullBaseAmount");
    const fullTotalInput = document.getElementById("YesFullTotalAmount");
    const discountInput = document.getElementById("YesDiscountPercent");
    const fullSupplementInput = document.getElementById("YesFullSupplementAmount");
    const discountedBaseInput = document.getElementById("YesDiscountedBaseAmount");
    const discountedSupplementInput = document.getElementById("YesDiscountedSupplementAmount");
    const discountedTotalInput = document.getElementById("YesDiscountedTotalAmount");

    if (!productInput || !yesWrap) {
        return;
    }

    const recalculateYesFields = () => {
        const product = (productInput.value || "").trim();
        const visible = isYesProduct(product);

        yesWrap.style.display = visible ? "block" : "none";

        if (!visible) {
            clearOutput(fullSupplementInput);
            clearOutput(discountedBaseInput);
            clearOutput(discountedSupplementInput);
            clearOutput(discountedTotalInput);

            if (fullBaseInput) fullBaseInput.value = "";
            if (fullTotalInput) fullTotalInput.value = "";
            if (discountInput) discountInput.value = "";
            return;
        }

        const fullBase = parseHungarianNumber(fullBaseInput ? fullBaseInput.value : "");
        const fullTotal = parseHungarianNumber(fullTotalInput ? fullTotalInput.value : "");
        const discountPercent = parseHungarianNumber(discountInput ? discountInput.value : "");

        if (fullTotal < fullBase) {
            clearOutput(fullSupplementInput);
            clearOutput(discountedBaseInput);
            clearOutput(discountedSupplementInput);
            clearOutput(discountedTotalInput);
            return;
        }

        const discountRate = discountPercent / 100;
        const supplement = fullTotal - fullBase;
        const discountedBase = fullBase * (1 - discountRate);
        const discountedSupplement = supplement * (1 - discountRate);
        const discountedTotal = discountedBase + discountedSupplement;

        setOutput(fullSupplementInput, supplement, 2);
        setOutput(discountedBaseInput, discountedBase, 2);
        setOutput(discountedSupplementInput, discountedSupplement, 2);
        setOutput(discountedTotalInput, discountedTotal, 2);
    };

    productInput.addEventListener("change", recalculateYesFields);

    if (fullBaseInput) {
        fullBaseInput.addEventListener("input", recalculateYesFields);
        fullBaseInput.addEventListener("change", recalculateYesFields);
    }

    if (fullTotalInput) {
        fullTotalInput.addEventListener("input", recalculateYesFields);
        fullTotalInput.addEventListener("change", recalculateYesFields);
    }

    if (discountInput) {
        discountInput.addEventListener("input", recalculateYesFields);
        discountInput.addEventListener("change", recalculateYesFields);
    }

    recalculateYesFields();
}

function initCalculator() {
    const productInput = document.getElementById("Product");
    const amountInput = document.getElementById("Amount");
    const ukSelect = document.getElementById("ukSelect");
    const commissionInput = document.getElementById("Commission");
    const suInput = document.getElementById("Su");
    const commissionPercentInput = document.getElementById("CommissionPercent");
    const dividerInput = document.getElementById("Divider");

    if (!productInput || !amountInput || !window.productRules) {
        return;
    }

    const recalculate = () => {
        const product = (productInput.value || "").trim();
        const amount = parseHungarianNumber(amountInput.value);
        const isUkContract = ukSelect ? (ukSelect.value === "true") : false;

        clearOutput(commissionInput);
        clearOutput(suInput);
        clearOutput(commissionPercentInput);
        clearOutput(dividerInput);

        if (!product || amount <= 0) {
            return;
        }

        const rule = window.productRules[product];
        if (!rule) {
            return;
        }

        let commission = 0;
        let su = 0;
        const divisor = Number(rule.divisor || 0);
        const percentDecimal = Number(rule.percent || 0);
        const percentDisplay = percentDecimal * 100;

        if (divisor > 0) {
            su = amount / divisor;
        }

        if (rule.mode === "percent") {
            commission = amount * percentDecimal;
        }

        if (rule.requiresUkQuestion && isUkContract) {
            commission = 0;
        }

        setOutput(commissionInput, commission, 0);
        setOutput(suInput, su, 4);
        setOutput(commissionPercentInput, percentDisplay, 2);
        setOutput(dividerInput, divisor, 2);
    };

    productInput.addEventListener("change", recalculate);
    amountInput.addEventListener("input", recalculate);
    amountInput.addEventListener("change", recalculate);

    if (ukSelect) {
        ukSelect.addEventListener("change", recalculate);
    }

    recalculate();
}

function parseHungarianNumber(value) {
    if (!value) {
        return 0;
    }

    let text = value.toString().trim().replace(/\s/g, "");
    if (!text) {
        return 0;
    }

    const hasComma = text.includes(",");
    const hasDot = text.includes(".");

    if (hasComma && hasDot) {
        const lastComma = text.lastIndexOf(",");
        const lastDot = text.lastIndexOf(".");

        if (lastComma > lastDot) {
            text = text.replace(/\./g, "").replace(",", ".");
        } else {
            text = text.replace(/,/g, "");
        }
    } else if (hasComma) {
        text = text.replace(/\./g, "").replace(",", ".");
    } else {
        const parts = text.split(".");
        if (parts.length > 2) {
            const decimalPart = parts.pop();
            text = parts.join("") + "." + decimalPart;
        }
    }

    const parsed = parseFloat(text);
    return Number.isNaN(parsed) ? 0 : parsed;
}

function roundNumber(value, decimals) {
    const factor = Math.pow(10, decimals);
    const rounded = Math.round((value + Number.EPSILON) * factor) / factor;
    return rounded.toFixed(decimals).replace(".", ",");
}

function setOutput(element, value, decimals) {
    if (!element) {
        return;
    }

    element.value = roundNumber(value, decimals);
}

function clearOutput(element) {
    if (!element) {
        return;
    }

    element.value = "0";
}