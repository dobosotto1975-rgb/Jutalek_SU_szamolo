console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initCustomSelects();
    initUkQuestion();
    initYesProductFields();
    initCalculator();
});

function initCustomSelects() {
    const wrappers = document.querySelectorAll(".custom-select");
    if (!wrappers.length) return;

    wrappers.forEach(initOneCustomSelect);

    document.addEventListener("click", function (e) {
        const insideSelect = e.target.closest(".custom-select");
        if (!insideSelect) {
            closeAllCustomSelects();
        }
    });

    function initOneCustomSelect(wrapper) {
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
            filterOptions(search.value || "");
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
                text.textContent = selectedOption.textContent;
            } else {
                text.textContent = placeholder;
            }

            options.forEach(option => {
                const isSelected = option.dataset.value === realSelect.value;
                option.classList.toggle("selected", isSelected);
            });
        }

        function filterOptions(searchText) {
            const query = normalizeText(searchText);

            let visibleCount = 0;
            options.forEach(option => {
                const label = normalizeText(option.dataset.text || option.textContent || "");
                const visible = !query || label.includes(query);
                option.classList.toggle("hidden", !visible);
                if (visible) visibleCount++;
            });

            let empty = wrapper.querySelector(".custom-select-empty");
            if (!visibleCount) {
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
}

function closeAllCustomSelects() {
    document.querySelectorAll(".custom-select.open").forEach(x => x.classList.remove("open"));
}

function initUkQuestion() {
    const productInput = document.getElementById("Product");
    const ukWrap = document.getElementById("ukQuestionWrap");
    const ukSelect = document.getElementById("ukSelect");

    if (!productInput || !ukWrap) return;

    const toggleUk = () => {
        const product = (productInput.value || "").trim();
        const rules = window.productRules || {};
        const rule = rules[product];

        const needsUk = !!(rule && rule.requiresUkQuestion);
        ukWrap.style.display = needsUk ? "block" : "none";

        if (!needsUk && ukSelect) {
            ukSelect.value = "false";
            ukSelect.dispatchEvent(new Event("change", { bubbles: true }));
        }
    };

    productInput.addEventListener("change", toggleUk);
    toggleUk();
}

function initYesProductFields() {
    const productInput = document.getElementById("Product");
    const yesSection = document.getElementById("yesProductSection");

    const fullBaseInput = document.getElementById("YesFullBaseAmount");
    const fullTotalInput = document.getElementById("YesFullTotalAmount");
    const discountInput = document.getElementById("YesDiscountPercent");

    const fullSupplementInput = document.getElementById("YesFullSupplementAmount");
    const discountedBaseInput = document.getElementById("YesDiscountedBaseAmount");
    const discountedSupplementInput = document.getElementById("YesDiscountedSupplementAmount");
    const discountedTotalInput = document.getElementById("YesDiscountedTotalAmount");

    if (!productInput || !yesSection) return;

    const isViennaYes = (value) => {
        return normalizeText(value).includes("vienna yes");
    };

    const toggleYesSection = () => {
        yesSection.style.display = isViennaYes(productInput.value) ? "block" : "none";
        recalculateYesFields();
    };

    const recalculateYesFields = () => {
        if (!isViennaYes(productInput.value)) {
            setOutput(fullSupplementInput, 0, 2);
            setOutput(discountedBaseInput, 0, 2);
            setOutput(discountedSupplementInput, 0, 2);
            setOutput(discountedTotalInput, 0, 2);
            return;
        }

        const fullBase = parseHungarianNumber(fullBaseInput?.value);
        const fullTotal = parseHungarianNumber(fullTotalInput?.value);
        const discountPercent = parseHungarianNumber(discountInput?.value);

        if (fullTotal < fullBase) {
            setOutput(fullSupplementInput, 0, 2);
            setOutput(discountedBaseInput, 0, 2);
            setOutput(discountedSupplementInput, 0, 2);
            setOutput(discountedTotalInput, 0, 2);
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

    productInput.addEventListener("change", toggleYesSection);

    [fullBaseInput, fullTotalInput, discountInput].forEach(input => {
        if (!input) return;
        input.addEventListener("input", recalculateYesFields);
        input.addEventListener("change", recalculateYesFields);
    });

    toggleYesSection();
}

function initCalculator() {
    const productInput = document.getElementById("Product");
    const amountInput = document.getElementById("Amount");
    const ukSelect = document.getElementById("ukSelect");

    const commissionElement = document.getElementById("Commission");
    const suElement = document.getElementById("Su");
    const commissionPercentElement = document.getElementById("CommissionPercent");
    const dividerElement = document.getElementById("Divider");

    const suMeterFill = document.getElementById("SuMeterFill");
    const suStatus = document.getElementById("SuStatus");

    if (!productInput || !amountInput) return;

    const recalculate = () => {
        const rules = window.productRules || {};
        const product = (productInput.value || "").trim();
        const amount = parseHungarianNumber(amountInput.value);
        const isUkContract = ukSelect ? ukSelect.value === "true" : false;

        setText(commissionElement, "0");
        setText(suElement, "0");
        setText(commissionPercentElement, "0");
        setText(dividerElement, "0");

        if (suMeterFill) {
            suMeterFill.style.width = "0%";
            suMeterFill.className = "su-meter-fill";
        }

        if (suStatus) {
            suStatus.textContent = "Nincs bónusz sávban";
            suStatus.className = "su-status";
        }

        if (!product || amount <= 0) return;

        const rule = rules[product];
        if (!rule) return;

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

        setText(commissionElement, formatNumberHu(commission, 0));
        setText(suElement, formatNumberHu(su, 2));
        setText(commissionPercentElement, formatNumberHu(percentDisplay, 2));
        setText(dividerElement, formatNumberHu(divisor, 2));

        updateSuMeter(su, suMeterFill, suStatus);
    };

    productInput.addEventListener("change", recalculate);
    amountInput.addEventListener("input", recalculate);
    amountInput.addEventListener("change", recalculate);

    if (ukSelect) {
        ukSelect.addEventListener("change", recalculate);
    }

    recalculate();
}

function updateSuMeter(su, fill, status) {
    if (!fill || !status) return;

    const maxVisual = 9;
    const percent = Math.max(0, Math.min((su / maxVisual) * 100, 100));
    fill.style.width = percent + "%";

    fill.classList.remove("warning", "success");
    status.classList.remove("warning", "success");

    if (su >= 9) {
        fill.classList.add("success");
        status.classList.add("success");
        status.textContent = "9 SU felett – felső bónuszsáv";
    } else if (su >= 4.5) {
        fill.classList.add("warning");
        status.classList.add("warning");
        status.textContent = "4,5 SU felett – középső bónuszsáv";
    } else {
        status.textContent = "Nincs bónusz sávban";
    }
}

function parseHungarianNumber(value) {
    if (value === null || value === undefined) return 0;

    let text = value.toString().trim();
    if (!text) return 0;

    text = text.replace(/\s/g, "");

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
        text = text.replace(",", ".");
    }

    const parsed = Number(text);
    return Number.isFinite(parsed) ? parsed : 0;
}

function setOutput(element, value, decimals) {
    if (!element) return;
    element.value = formatNumberHu(value, decimals);
}

function setText(element, value) {
    if (!element) return;
    element.textContent = value;
}

function formatNumberHu(value, decimals) {
    const numeric = Number(value || 0);
    return numeric.toLocaleString("hu-HU", {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });
}

function normalizeText(value) {
    return (value || "").toString().toLowerCase().trim();
}