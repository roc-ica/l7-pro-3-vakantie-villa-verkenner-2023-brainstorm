const filterValues = {
    "PropertyTags": [],
    "LocationTags": [],
    "Search": "",
    "Location": "",
};

const filterConfig = {
    "Prijs": { min: 10, max: 500 },
    "Capaciteit": { min: 1, max: 60 },
    "Slaapkamers": { min: 1, max: 10 },
    "Badkamers": { min: 1, max: 10 }
};

const FormattedFilters = {}
let Tags = [];
let Villas = [];

async function getVillas() {
    [Villas, Tags] = await Promise.all([
        VillaRequests.getVillas(),
        VillaRequests.getTags()
    ]);

    if (Villas.success === false) {
        console.log(Villas.error);

        return;
    }

    updateVillaDisplay();
    if (Tags.success === false) {
        console.log(Tags.error);

        return;
    }

    let PropertyTags = JSON.parse(Tags.data.PropertyTags);
    let locationTags = JSON.parse(Tags.data.LocationTags);

    const PropertyTagsContainer = document.getElementById('propertyTagsContainer');
    const locationTagsContainer = document.getElementById('locationTagsContainer');

    PropertyTagsContainer.innerHTML = '';
    locationTagsContainer.innerHTML = '';

    for (let i = 0; i < PropertyTags.length; i++) {
        PropertyTagsContainer.innerHTML += `<div class="form-check">
            <input class="form-check-input tagCheckBox" type="checkbox" value="${PropertyTags[i].PropertyTagId}" id="propertyTag${PropertyTags[i].PropertyTagId}" onchange="UpdateAllFilterValues()">
            <label class="form-check-label" for="propertyTag${PropertyTags[i].PropertyTagID}">${PropertyTags[i].PropertyTag1}</label>
        </div>`;
    }

    for (let i = 0; i < locationTags.length; i++) {
        locationTagsContainer.innerHTML += `<div class="form-check">
            <input class="form-check-input" type="checkbox" value="${locationTags[i].LocationTagId}" id="locationTag${locationTags[i].LocationTagId}" onchange="UpdateAllFilterValues()">
            <label class="form-check-label" for="locationTag${locationTags[i].LocationTagId}">${locationTags[i].LocationTag1}</label>
        </div>`;
    }
}

function updateVillaDisplay() {
    let villaData = JSON.parse(Villas.data.Villas);
    document.getElementsByClassName('villaList')[0].innerHTML = '';

    if (villaData.length === 0) {
        document.getElementsByClassName('villaList')[0].innerHTML = 
        `<div class="villaCard">
                <p style="margin-left:5px;">No villas found</p>
            </div>`;

        return;
    }

    for (let i = 0; i < villaData.length; i++) {
        const villa = new SmallVilla(villaData[i]);
        document.getElementsByClassName('villaList')[0].innerHTML += villa.html;
    }
}

function initializeRangeSlider(container) {
    const range = container.querySelector(".range-selected");
    const rangeInputs = container.querySelectorAll(".range-input input");
    const rangePrices = container.querySelectorAll(".range-price input");
    const filterTitle = container.closest(".filterGroup").querySelector("h2").innerText.trim();

    const initialMin = filterConfig[filterTitle]?.min || 0;
    const initialMax = filterConfig[filterTitle]?.max || 1000;
    const settings = { minRangeGap: 1, stepSize: 1 };


    //kan dit misschien in een for loopje? of is het ingewikkelder dan het lijkt?
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
    const propertyTags = Array.from(document.querySelectorAll(".tagCheckBox:checked")).map(input => input.value);
    console.log(propertyTags); //is dit nog nodig?
    const locationTags = Array.from(document.querySelectorAll("#locationTagsContainer input:checked")).map(input => input.value);
    const search = document.getElementById("searchField").value;
    const location = document.getElementById("locationSearchField").value;
    filterValues["PropertyTags"] = propertyTags;
    filterValues["LocationTags"] = locationTags;
    filterValues["Search"] = search;
    filterValues["Location"] = location;

    FormattedFilters["PropertyTagsIDs"] = propertyTags
    FormattedFilters["LocationTagsIDs"] = locationTags
    FormattedFilters["Search"] = search
    FormattedFilters["Location"] = location
    FormattedFilters["MinPrice"] = filterValues["Prijs"]?.min
    FormattedFilters["MaxPrice"] = filterValues["Prijs"]?.max
    FormattedFilters["MinCapacity"] = filterValues["Capaciteit"]?.min
    FormattedFilters["MaxCapacity"] = filterValues["Capaciteit"]?.max
    FormattedFilters["MinBedrooms"] = filterValues["Slaapkamers"]?.min
    FormattedFilters["MaxBedrooms"] = filterValues["Slaapkamers"]?.max
    FormattedFilters["MinBathrooms"] = filterValues["Badkamers"]?.min
    FormattedFilters["MaxBathrooms"] = filterValues["Badkamers"]?.max

    if (timeout) {
        clearTimeout(timeout);
    }
    
    timeout = setTimeout(async () => {
        Villas = await VillaRequests.getVillasByFilters(FormattedFilters);
        if (Villas.success === false) {
            console.log(Villas.error);

            return;
        }
        
        updateVillaDisplay();
    }, 500);
}

getVillas();
document.querySelectorAll(".range").forEach(slider => initializeRangeSlider(slider));
UpdateAllFilterValues();