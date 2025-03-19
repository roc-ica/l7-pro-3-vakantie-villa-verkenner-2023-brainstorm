// Global dictionary to store filter values
const filterValues = {
    "PropertyTags": [],
    "LocationTags": [],
    "Search": "",
    "Location": "",
};

// Configuration object for dynamic min/max values
const filterConfig = {
    "Prijs": { min: 10, max: 500 },
    "Capaciteit": { min: 1, max: 60 },
    "Slaapkamers": { min: 1, max: 10 },
    "Badkamers": { min: 1, max: 10 }
};

async function getVillas() {
    const [villas, Tags] = await Promise.all([
        VillaRequests.getVillas(),
        VillaRequests.getTags()
    ]);
    console.log(villas);
    console.log(Tags);
    if (villas.success === false) {
        console.log(villas.error);
        return;
    }
    let villaData = JSON.parse(villas.data.Villas);
    document.getElementsByClassName('villaList')[0].innerHTML = '';
    for (let i = 0; i < villaData.length; i++) {
        const villa = new SmallVilla(villaData[i]);
        document.getElementsByClassName('villaList')[0].innerHTML += villa.html;
    }

    if (Tags.success === false) {
        console.log(Tags.error);
        return;
    }
    let PropertyTags = JSON.parse(Tags.data.PropertyTags);
    let locationTags = JSON.parse(Tags.data.LocationTags);
    console.log(PropertyTags);
    console.log(locationTags);
    const PropertyTagsContainer = document.getElementById('propertyTagsContainer');
    const locationTagsContainer = document.getElementById('locationTagsContainer');

    PropertyTagsContainer.innerHTML = '';
    locationTagsContainer.innerHTML = '';

    for (let i = 0; i < PropertyTags.length; i++) {
        PropertyTagsContainer.innerHTML += `<div class="form-check">
            <input class="form-check-input" type="checkbox" value="${PropertyTags[i].PropertyTagID}" id="propertyTag${PropertyTags[i].PropertyTagID}" onchange="UpdateAllFilterValues()">
            <label class="form-check-label" for="propertyTag${PropertyTags[i].PropertyTagID}">${PropertyTags[i].PropertyTag1}</label>
        </div>`;
    }

    for (let i = 0; i < locationTags.length; i++) {
        locationTagsContainer.innerHTML += `<div class="form-check">
            <input class="form-check-input" type="checkbox" value="${locationTags[i].LocationTagID}" id="locationTag${locationTags[i].LocationTagID}" onchange="UpdateAllFilterValues()">
            <label class="form-check-label" for="locationTag${locationTags[i].LocationTagID}">${locationTags[i].LocationTag1}</label>
        </div>`;
    }
}

function initializeRangeSlider(container) {
    const range = container.querySelector(".range-selected");
    const rangeInputs = container.querySelectorAll(".range-input input");
    const rangePrices = container.querySelectorAll(".range-price input");
    const filterTitle = container.closest(".filterGroup").querySelector("h2").innerText.trim();

    // Get dynamic min/max values from config or default to generic values
    const initialMin = filterConfig[filterTitle]?.min || 0;
    const initialMax = filterConfig[filterTitle]?.max || 1000;
    const settings = { minRangeGap: 1, stepSize: 1 };


    rangeInputs[0].min = initialMin;
    rangeInputs[0].max = initialMax;
    rangeInputs[1].min = initialMin;
    rangeInputs[1].max = initialMax;
    rangeInputs[0].value = initialMin;
    rangeInputs[1].value = initialMax;
    rangeInputs[0].step = settings.stepSize;
    rangeInputs[1].step = settings.stepSize;

    rangePrices[0].min = initialMin;
    rangePrices[0].max = initialMax;
    rangePrices[1].min = initialMin;
    rangePrices[1].max = initialMax;
    rangePrices[0].value = initialMin;
    rangePrices[1].value = initialMax;
    rangePrices[0].step = settings.stepSize;
    rangePrices[1].step = settings.stepSize;
    handlePriceInput();
    handleRangeInput();
    updateSlider();

    // Store initial values in filterValues
    filterValues[filterTitle] = {
        min: initialMin,
        max: initialMax
    };


    function updateSlider() {
        const minValue = parseInt(rangeInputs[0].value);
        const maxValue = parseInt(rangeInputs[1].value);

        const rangeMin = parseInt(rangeInputs[0].min);
        const rangeMax = parseInt(rangeInputs[1].max);

        // Normalize values based on the actual range
        const leftPercent = ((minValue - rangeMin) / (rangeMax - rangeMin)) * 100;
        const rightPercent = 100 - ((maxValue - rangeMin) / (rangeMax - rangeMin)) * 100;

        range.style.left = leftPercent + "%";
        range.style.right = rightPercent + "%";
    }


    function updateFilterValues() {
        filterValues[filterTitle] = {
            min: parseInt(rangeInputs[0].value),
            max: parseInt(rangeInputs[1].value)
        };
        UpdateAllFilterValues();
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
}

let timeout = null;

function UpdateAllFilterValues() {
    const propertyTags = Array.from(document.querySelectorAll("#propertyTagsContainer input:checked")).map(input => input.value);
    const locationTags = Array.from(document.querySelectorAll("#locationTagsContainer input:checked")).map(input => input.value);
    const search = document.getElementById("searchField").value;
    const location = document.getElementById("locationSearchField").value;
    filterValues["PropertyTags"] = propertyTags;
    filterValues["LocationTags"] = locationTags;
    filterValues["Search"] = search;
    filterValues["Location"] = location;
    console.log(filterValues);
    if (timeout) {
        clearTimeout(timeout);
    }
    timeout = setTimeout(async () => {
        let data = await VillaRequests.getVillasByFilters(filterValues);
        console.log(data);
    }, 500);
}

getVillas();
document.querySelectorAll(".range").forEach(slider => initializeRangeSlider(slider));