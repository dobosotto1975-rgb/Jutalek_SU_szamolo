console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initUkQuestion();
    initYesProductFields();
    initCalculator();
});

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
        text = text.replace(",", ".");
    }

    const parsed = Number(text);
    return Number.isFinite(parsed) ? parsed : 0;
}

function setOutput(element, value, decimals) {
    if (!element) return;
    element.value = Number(value || 0).toLocaleString("hu-HU", {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });
}

function clearOutput(element) {
    if (!element) return;
    element.value = "";
}