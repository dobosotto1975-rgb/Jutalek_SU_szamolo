console.log("AdvisorDashboardApp betöltve.");

document.addEventListener("DOMContentLoaded", function () {
    initCustomSelects();
    initUkQuestion();
    initYesProductFields();
    initCalculator();
});

/* =========================
   CUSTOM SELECT
========================= */
function initCustomSelects() {
    const wrappers = document.querySelectorAll(".custom-select");
    if (!wrappers.length) return;

    wrappers.forEach(initOneCustomSelect);

    document.addEventListener("click", function (e) {
        if (!e.target.closest(".custom-select")) {
            document.querySelectorAll(".custom-select.open")
                .forEach(x => x.classList.remove("open"));
        }
    });

    function initOneCustomSelect(wrapper) {
        const targetId = wrapper.dataset.target;
        const realSelect = document.getElementById(targetId);
        const trigger = wrapper.querySelector(".custom-select-trigger");
        const text = wrapper.querySelector(".custom-select-text");
        const options = wrapper.querySelectorAll(".custom-select-option");

        if (!realSelect || !trigger) return;

        trigger.addEventListener("click", () => {
            wrapper.classList.toggle("open");
        });

        options.forEach(opt => {
            opt.addEventListener("click", () => {
                realSelect.value = opt.dataset.value;
                text.textContent = opt.dataset.text;
                wrapper.classList.remove("open");

                realSelect.dispatchEvent(new Event("change"));
            });
        });
    }
}

/* =========================
   ÜK kérdés
========================= */
function initUkQuestion() {
    const product = document.getElementById("Product");
    const wrap = document.getElementById("ukQuestionWrap");

    if (!product || !wrap) return;

    const toggle = () => {
        if (product.value.toLowerCase().includes("yes")) {
            wrap.style.display = "block";
        } else {
            wrap.style.display = "none";
        }
    };

    product.addEventListener("change", toggle);
    toggle();
}

/* =========================
   YES CALC
========================= */
function initYesProductFields() {
    const product = document.getElementById("Product");
    const section = document.getElementById("yesProductSection");

    const base = document.getElementById("YesFullBaseAmount");
    const total = document.getElementById("YesFullTotalAmount");
    const discount = document.getElementById("YesDiscountPercent");

    const out1 = document.getElementById("YesFullSupplementAmount");
    const out2 = document.getElementById("YesDiscountedBaseAmount");
    const out3 = document.getElementById("YesDiscountedSupplementAmount");
    const out4 = document.getElementById("YesDiscountedTotalAmount");

    if (!product) return;

    const calc = () => {
        if (!product.value.toLowerCase().includes("yes")) {
            section.style.display = "none";
            return;
        }

        section.style.display = "block";

        const b = parseFloat(base?.value || 0);
        const t = parseFloat(total?.value || 0);
        const d = (parseFloat(discount?.value || 0)) / 100;

        const sup = t - b;
        const db = b * (1 - d);
        const ds = sup * (1 - d);
        const dt = db + ds;

        if (out1) out1.value = sup.toFixed(2);
        if (out2) out2.value = db.toFixed(2);
        if (out3) out3.value = ds.toFixed(2);
        if (out4) out4.value = dt.toFixed(2);
    };

    [base, total, discount].forEach(i => {
        if (i) i.addEventListener("input", calc);
    });

    product.addEventListener("change", calc);
}

/* =========================
   FŐ KALKULÁTOR
========================= */
function initCalculator() {
    const amount = document.getElementById("Amount");
    const product = document.getElementById("Product");

    const commission = document.getElementById("Commission");
    const su = document.getElementById("Su");

    if (!amount || !product) return;

    const calc = () => {
        const val = parseFloat(amount.value || 0);

        let divider = 27500;
        let percent = 0.1;

        const suVal = val / divider;
        const comm = val * percent;

        if (commission) commission.textContent = comm.toFixed(0);
        if (su) su.textContent = suVal.toFixed(2);
    };

    amount.addEventListener("input", calc);
    product.addEventListener("change", calc);
}