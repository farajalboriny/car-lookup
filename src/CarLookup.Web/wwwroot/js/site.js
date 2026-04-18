(function () {
    "use strict";

    const form = document.getElementById("searchForm");
    if (!form) {
        return;
    }

    const makeSelectEl = document.getElementById("makeSelect");
    const yearSelect = document.getElementById("yearSelect");
    const typeSelect = document.getElementById("typeSelect");
    const makeNameInput = document.getElementById("makeNameInput");
    const searchButton = document.getElementById("searchButton");

    const makeTs = new TomSelect(makeSelectEl, {
        maxOptions: null,
        placeholder: "Start typing a make\u2026",
        allowEmptyOption: false
    });

    function updateSearchButtonState() {
        const ready = makeTs.getValue() && yearSelect.value && typeSelect.value;
        searchButton.disabled = !ready;
    }

    async function loadVehicleTypes(makeId) {
        typeSelect.innerHTML = '<option value="">Loading\u2026</option>';
        typeSelect.disabled = true;
        try {
            const response = await fetch(`/Index?handler=VehicleTypes&makeId=${encodeURIComponent(makeId)}`, {
                headers: { "Accept": "application/json" }
            });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const types = await response.json();
            typeSelect.innerHTML = '<option value="">Choose a vehicle type\u2026</option>';
            for (const t of types) {
                const opt = document.createElement("option");
                opt.value = t.name;
                opt.textContent = t.name;
                typeSelect.appendChild(opt);
            }
            typeSelect.disabled = types.length === 0;
            if (types.length === 0) {
                typeSelect.innerHTML = '<option value="">No types available for this make</option>';
            }
        } catch (err) {
            typeSelect.innerHTML = '<option value="">Couldn\u2019t load types \u2014 try another make</option>';
            typeSelect.disabled = true;
        } finally {
            updateSearchButtonState();
        }
    }

    makeTs.on("change", function (value) {
        let selectedOption = null;
        for (const option of makeSelectEl.options) {
            if (option.value === value) {
                selectedOption = option;
                break;
            }
        }
        makeNameInput.value = selectedOption ? selectedOption.dataset.name : "";
        if (value) {
            loadVehicleTypes(value);
        } else {
            typeSelect.innerHTML = '<option value="">Pick a make first\u2026</option>';
            typeSelect.disabled = true;
            updateSearchButtonState();
        }
    });

    yearSelect.addEventListener("change", updateSearchButtonState);
    typeSelect.addEventListener("change", updateSearchButtonState);
})();
