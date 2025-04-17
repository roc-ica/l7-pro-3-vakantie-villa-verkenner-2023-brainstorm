let selectedImages = [];
let removedImages = [];
let mainImage = null;
const maxImages = 20;
const maxFileSize = 2 * 1024 * 1024; // 2MB
const allowedTypes = ["image/png", "image/jpeg", "image/webp", "image/jpg", "image/avif"];

function previewImage(file) {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = function (event) {
        const imgSrc = event.target.result;

        addImagePreview({ src: imgSrc, file, isNew: true });
    };
}

function previewImageFromURL(url) {
    addImagePreview({ src: url, url, isNew: false });
}

function addImagePreview({ src, file, url, isNew }) {
    const wrapper = document.createElement("div");
    wrapper.classList.add("image-wrapper");

    const imgElement = document.createElement("img");
    imgElement.src = src;
    imgElement.classList.add("thumbnail");

    const removeBtn = document.createElement("button");
    removeBtn.innerText = "X";
    removeBtn.classList.add("remove-btn");
    removeBtn.onclick = function () {
        if (!isNew && url) {
            removedImages.push(url);
        }

        selectedImages = selectedImages.filter(
            img => !(isNew ? img.file === file : img.url === url)
        );

        imagePreviewContainer.removeChild(wrapper);

        if (mainImage && ((mainImage.file && mainImage.file === file) || (mainImage.url && mainImage.url === url))) {
            mainImage = selectedImages.length > 0 ? selectedImages[0] : null;
            updateMainImage();
        }
    };

    const selectMainBtn = document.createElement("img");
    selectMainBtn.src = "../Assets/icons/mainImageIcon.svg";
    selectMainBtn.alt = "Select as main image";
    selectMainBtn.classList.add("main-btn");
    selectMainBtn.onclick = function () {
        mainImage = isNew ? { file } : { url };
        updateMainImage();
    };

    wrapper.appendChild(imgElement);
    wrapper.appendChild(selectMainBtn);
    wrapper.appendChild(removeBtn);
    imagePreviewContainer.appendChild(wrapper);

    if (isNew) {
        selectedImages.push({ file, isNew: true });
    } else {
        selectedImages.push({ url, isNew: false });
    }

    if (!mainImage) {
        mainImage = isNew ? { file } : { url };
        updateMainImage();
    }
}

function updateMainImage() {
    const wrappers = document.querySelectorAll(".image-wrapper");

    wrappers.forEach((wrapper, index) => {
        const imageObj = selectedImages[index];
        const selectMainBtn = wrapper.querySelector(".main-btn");
        let isCurrentMain =
            (mainImage.file && imageObj.file === mainImage.file) ||
            (mainImage.url && imageObj.url === mainImage.url);

        if (isCurrentMain) {
            wrapper.classList.add("main-image");
            selectMainBtn.style.visibility = "hidden";
        } else {
            wrapper.classList.remove("main-image");
            selectMainBtn.style.visibility = "visible";
        }
    });
}

imageInput.addEventListener("change", function () {
    if (selectedImages.length >= maxImages) {
        alert("You can only upload up to 20 images.");
        imageInput.value = "";

        return;
    }

    const files = Array.from(imageInput.files);
    files.forEach((file) => {
        if (!allowedTypes.includes(file.type)) {
            alert("Only .png, .jpeg, .avif and .webp files are allowed.");

            return;
        }

        if (file.size > maxFileSize) {
            alert("File size must be less than 2MB.");
            
            return;
        }

        if (selectedImages.length < maxImages) {
            previewImage(file);
        }
    });

    imageInput.value = "";
});

document.getElementById('addVillaButton').addEventListener('click', async function (event) {
    event.preventDefault();

    if (selectedImages.length === 0) {
        alert("Please upload at least one image.");

        return;
    }

    let villaName = document.getElementById("name").value;
    let description = document.getElementById("description").value;
    let price = document.getElementById("price").value;
    let location = document.getElementById("location").value;
    let capacity = document.getElementById("capacity").value;
    let bedrooms = document.getElementById("bedrooms").value;
    let bathrooms = document.getElementById("bathrooms").value;
    let propertyTags = Array.from(document.querySelectorAll('.tagCheckBox:checked')).map(tag => tag.value);
    let locationTags = Array.from(document.querySelectorAll('.locationTagCheckBox:checked')).map(tag => tag.value);

    const formData = new FormData();
    formData.append("VillaID", id);
    formData.append("VillaName", villaName);
    formData.append("Description", description);
    formData.append("Price", price);
    formData.append("Location", location);
    formData.append("Capacity", capacity);
    formData.append("Bedrooms", bedrooms);
    formData.append("Bathrooms", bathrooms);
    formData.append("PropertyTagsJson", JSON.stringify(propertyTags));
    formData.append("LocationTagsJson", JSON.stringify(locationTags));

    // New images
    selectedImages.forEach((img) => {
        if (img.isNew) {
            if (mainImage.file && mainImage.file === img.file) {
                formData.append("MainImage", img.file);
            } else {
                formData.append("Images", img.file);
            }
        }
    });

    // Existing main image (only one)
    if (mainImage.url) {
        formData.append("MainImageUrl", mainImage.url);
    }

    // Deleted existing image URLs
    formData.append("RemovedImagesJson", JSON.stringify(removedImages));

    try {
        document.getElementById('addVillaButton').disabled = true;
        const response = await AdminRequest.editVilla(formData);
        console.log(response);
        console.log(JSON.parse(response.data.data)); // zijn deze logs nog nodig?
        document.getElementById('addVillaButton').disabled = false;

        if (response.success) {
            alert("Villa updated successfully!");
            window.location.href = "VillaList.html";
        } else {
            alert("Error: " + response.data.Reason);
        }
    } catch (error) {
        alert("Error submitting form.");
        console.error(error);
    }
});

async function getVilla(id) {
    let data = await VillaRequests.getVillaByIDEdit(id);
    if (data.success == false) { // kan het niet (!data.success) ?
        console.error("Failed to get villa");
        // window.location.href = "index.html"; gaan we dit ooit weer terugzetten of kan het weg?
        return;
    }

    let villa = JSON.parse(data.data.Villa);
    console.log(villa); // nodig?

    document.getElementById("name").value = villa.Name;
    document.getElementById("description").value = villa.Description;
    document.getElementById("price").value = villa.Price;
    document.getElementById("location").value = villa.Location;
    document.getElementById("capacity").value = villa.Capacity;
    document.getElementById("bedrooms").value = villa.Bedrooms;
    document.getElementById("bathrooms").value = villa.Bathrooms;

    const propertyTagsContainer = document.querySelectorAll('#propertyTagsContainer .tagCheckBox');
    const locationTagsContainer = document.querySelectorAll('#locationTagsContainer .locationTagCheckBox');
    let propertyTags = [];
    let locationTags = [];

    villa.PropertyTags.forEach(tag => {
        propertyTags.push(`${tag}`);
    });

    villa.LocationTags.forEach(tag => {
        locationTags.push(`${tag}`);
    });

    propertyTagsContainer.forEach(tag => {
        if (propertyTags.includes(tag.value)) {
            tag.checked = true;
        }
    });

    locationTagsContainer.forEach(tag => {
        if (locationTags.includes(tag.value)) {
            tag.checked = true;
        }
    });

    villa.VillaImagePaths.forEach((url) => {
        previewImageFromURL(url);
    });

    mainImage = { url: villa.VillaMainImagePath };
    previewImageFromURL(villa.VillaMainImagePath);
    updateMainImage();
}

async function getTags() {
    let Tags = await VillaRequests.getTags();
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
            <input class="form-check-input tagCheckBox" type="checkbox" value="${PropertyTags[i].PropertyTagId}" id="propertyTag${PropertyTags[i].PropertyTagId}">
            <label class="form-check-label" for="propertyTag${PropertyTags[i].PropertyTagID}">${PropertyTags[i].PropertyTag1}</label>
        </div>`;
    }

    for (let i = 0; i < locationTags.length; i++) {
        locationTagsContainer.innerHTML += `<div class="form-check">
            <input class="form-check-input locationTagCheckBox" type="checkbox" value="${locationTags[i].LocationTagId}" id="locationTag${locationTags[i].LocationTagId}">
            <label class="form-check-label" for="locationTag${locationTags[i].LocationTagId}">${locationTags[i].LocationTag1}</label>
        </div>`;
    }
}

getTags();

url = new URL(window.location.href);
let id = url.searchParams.get("villaID");
getVilla(Number(id));