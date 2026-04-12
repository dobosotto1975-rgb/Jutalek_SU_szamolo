console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initUkQuestion();
    initCalculator();
});

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
            ukQuestionWrap.style.display = "flex";
        } else {
            ukQuestionWrap.style.display = "none";
            ukSelect.value = "false";
        }
    };

    productInput.addEventListener("change", toggleUkQuestion);
    toggleUkQuestion();
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
        const percentValue = Number(rule.percent || 0);

        if (divisor > 0) {
            su = amount / divisor;
        }

        if (rule.mode === "percent") {
            commission = amount * (percentValue / 100);
        }

        if (rule.requiresUkQuestion && isUkContract) {
            commission = 0;
        }

        setOutput(commissionInput, commission, 0);
        setOutput(suInput, su, 4);
        setOutput(commissionPercentInput, percentValue, 2);
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