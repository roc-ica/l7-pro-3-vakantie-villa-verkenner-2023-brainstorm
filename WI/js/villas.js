async function getVillas() {
    const villas = await VillaRequests.getVillas();
    if (villas.success === false) {
        console.log(villas.error);
        return;
    }
    let villaData = JSON.parse(villas.data.Villas);
    document.getElementsByClassName('villaList')[0].innerHTML = '';
    for (let x = 0; x < 5; x++) {
        for (let i = 0; i < villaData.length; i++) {
            const villa = new SmallVilla(villaData[i]);
            document.getElementsByClassName('villaList')[0].innerHTML += villa.html;
        }
    }
    console.log(villaData);
}

const filterValues = {};

document.addEventListener("DOMContentLoaded", () => {
    // Global dictionary to store filter values

    function initializeRangeSlider(container) {
        const range = container.querySelector(".range-selected");
        const rangeInputs = container.querySelectorAll(".range-input input");
        const rangePrices = container.querySelectorAll(".range-price input");
        const filterTitle = container.closest(".filterGroup").querySelector("h2").innerText.trim();

        // Store initial values in filterValues
        filterValues[filterTitle] = {
            min: parseInt(rangeInputs[0].value),
            max: parseInt(rangeInputs[1].value)
        };

        const settings = { minRangeGap: 100, stepSize: 10 };

        function updateSlider() {
            const minValue = parseInt(rangeInputs[0].value);
            const maxValue = parseInt(rangeInputs[1].value);
            range.style.left = (minValue / rangeInputs[0].max) * 100 + "%";
            range.style.right = 100 - (maxValue / rangeInputs[1].max) * 100 + "%";
        }

        function updateFilterValues() {
            filterValues[filterTitle] = {
                min: parseInt(rangeInputs[0].value),
                max: parseInt(rangeInputs[1].value)
            };
        }

        function handlePriceInput() {
            let minPrice = parseInt(rangePrices[0].value);
            let maxPrice = parseInt(rangePrices[1].value);
            if (maxPrice - minPrice >= settings.minRangeGap) {
                rangeInputs[0].value = minPrice;
                rangeInputs[1].value = maxPrice;
            } else {
                if (this === rangePrices[0]) {
                    rangePrices[1].value = minPrice + settings.minRangeGap;
                } else {
                    rangePrices[0].value = maxPrice - settings.minRangeGap;
                }
            }
            rangeInputs[0].value = rangePrices[0].value;
            rangeInputs[1].value = rangePrices[1].value;
            updateSlider();
            updateFilterValues();
        }

        function handleRangeInput() {
            let minRange = parseInt(rangeInputs[0].value);
            let maxRange = parseInt(rangeInputs[1].value);
            if (maxRange - minRange < settings.minRangeGap) {
                if (this === rangeInputs[0]) {
                    rangeInputs[0].value = maxRange - settings.minRangeGap;
                } else {
                    rangeInputs[1].value = minRange + settings.minRangeGap;
                }
            } else {
                rangePrices[0].value = minRange;
                rangePrices[1].value = maxRange;
            }
            updateSlider();
            updateFilterValues();
        }

        rangePrices.forEach(input => input.addEventListener("input", handlePriceInput));
        rangeInputs.forEach(input => input.addEventListener("input", handleRangeInput));
        updateSlider();
    }

    document.querySelectorAll(".range").forEach(slider => initializeRangeSlider(slider));
});


getVillas();

