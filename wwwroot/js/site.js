console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initUkQuestion();
    initYesProductFields();
    initCalculator();
});

function initUkQuestion() {
    const product = document.getElementById("Product");
    const wrap = document.getElementById("ukQuestionWrap");
    const ukSelect = document.getElementById("ukSelect");

    if (!product || !wrap) return;

    const toggle = () => {
        const value = (product.value || "").toLowerCase();
        const needsUk =
            value.includes("vienna yes") ||
            value.includes("ük") ||
            value.includes("uk");

        wrap.style.display = needsUk ? "block" : "none";

        if (!needsUk && ukSelect) {
            ukSelect.value = "false";
            ukSelect.dispatchEvent(new Event("change", { bubbles: true }));
        }
    };

    product.addEventListener("change", toggle);
    toggle();
}

function initYesProductFields() {
    const product = document.getElementById("Product");
    const section = document.getElementById("yesFieldsWrap");

    const base = document.getElementById("YesFullBaseAmount");
    const total = document.getElementById("YesFullTotalAmount");
    const discount = document.getElementById("YesDiscountPercent");

    const out1 = document.getElementById("YesFullSupplementAmount");
    const out2 = document.getElementById("YesDiscountedBaseAmount");
    const out3 = document.getElementById("YesDiscountedSupplementAmount");
    const out4 = document.getElementById("YesDiscountedTotalAmount");

    if (!product || !section) return;

    const isYesProduct = () => {
        return (product.value || "").toLowerCase().includes("vienna yes");
    };

    const recalc = () => {
        const show = isYesProduct();
        section.style.display = show ? "block" : "none";

        if (!show) {
            if (out1) out1.value = "0";
            if (out2) out2.value = "0";
            if (out3) out3.value = "0";
            if (out4) out4.value = "0";
            return;
        }

        const b = parseHuNumber(base?.value);
        const t = parseHuNumber(total?.value);
        const d = parseHuNumber(discount?.value) / 100;

        const supplement = Math.max(0, t - b);
        const discountedBase = b * (1 - d);
        const discountedSupplement = supplement * (1 - d);
        const discountedTotal = discountedBase + discountedSupplement;

        if (out1) out1.value = formatHu(supplement, 2);
        if (out2) out2.value = formatHu(discountedBase, 2);
        if (out3) out3.value = formatHu(discountedSupplement, 2);
        if (out4) out4.value = formatHu(discountedTotal, 2);
    };

    [base, total, discount].forEach(i => {
        if (i) {
            i.addEventListener("input", recalc);
            i.addEventListener("change", recalc);
        }
    });

    product.addEventListener("change", recalc);
    recalc();
}

function initCalculator() {
    const amount = document.getElementById("Amount");
    const product = document.getElementById("Product");
    const ukSelect = document.getElementById("ukSelect");

    const commission = document.getElementById("Commission");
    const su = document.getElementById("Su");
    const commissionPercent = document.getElementById("CommissionPercent");
    const divider = document.getElementById("Divider");

    if (!amount || !product || !commission || !su || !commissionPercent || !divider) return;

    const calc = () => {
        const val = parseHuNumber(amount.value);
        const name = (product.value || "").toLowerCase();
        const isUk = ukSelect ? ukSelect.value === "true" : false;

        let currentDivider = 27500;
        let currentPercent = 10;

        if (name.includes("lakás")) {
            currentDivider = 27500;
            currentPercent = 10;
        } else if (name.includes("kgfb")) {
            currentDivider = 20000;
            currentPercent = 12;
        } else if (name.includes("casco")) {
            currentDivider = 30000;
            currentPercent = 15;
        } else if (name.includes("vienna")) {
            currentDivider = 25000;
            currentPercent = 10;
        } else if (name.includes("utas")) {
            currentDivider = 15000;
            currentPercent = 20;
        }

        let comm = val * (currentPercent / 100);
        const suVal = currentDivider > 0 ? val / currentDivider : 0;

        if (isUk && name.includes("vienna yes")) {
            comm = 0;
        }

        commission.value = formatHu(comm, 0);
        su.value = formatHu(suVal, 4);
        commissionPercent.value = formatHu(currentPercent, 2);
        divider.value = formatHu(currentDivider, 0);
    };

    amount.addEventListener("input", calc);
    amount.addEventListener("change", calc);
    product.addEventListener("change", calc);

    if (ukSelect) {
        ukSelect.addEventListener("change", calc);
    }

    calc();
}

function parseHuNumber(value) {
    if (value === null || value === undefined) return 0;

    let text = value.toString().trim();
    if (!text) return 0;

    text = text.replace(/\s/g, "").replace(",", ".");
    const number = Number(text);

    return Number.isFinite(number) ? number : 0;
}

function formatHu(value, decimals) {
    return Number(value || 0).toLocaleString("hu-HU", {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });
}