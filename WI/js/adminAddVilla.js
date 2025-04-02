async function checkLogin() {
    const IsLoggedIn = await AdminRequest.IsLoggedIn();
    if (IsLoggedIn.success === false) {
        window.location.href = 'AdminLogin.html';
    }
}
checkLogin();

const imageInput = document.getElementById("imageInput");
const imagePreviewContainer = document.getElementById("imagePreviewContainer");
const maxImages = 20;
const maxFileSize = 2 * 1024 * 1024; // 2MB
const allowedTypes = ["image/png", "image/jpeg", "image/webp"];
let selectedImages = [];
let mainImage = null;

imageInput.addEventListener("change", function () {
    if (selectedImages.length >= maxImages) {
        alert("You can only upload up to 20 images.");
        imageInput.value = "";
        return;
    }

    const files = Array.from(imageInput.files);
    files.forEach((file) => {
        if (!allowedTypes.includes(file.type)) {
            alert("Only .png, .jpeg, and .webp files are allowed.");
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

function previewImage(file) {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = function (event) {
        const imgElement = document.createElement("img");
        imgElement.src = event.target.result;
        imgElement.classList.add("thumbnail");

        const removeBtn = document.createElement("button");
        removeBtn.innerText = "X";
        removeBtn.classList.add("remove-btn");
        removeBtn.onclick = function () {
            selectedImages = selectedImages.filter((img) => img !== file);
            imagePreviewContainer.removeChild(wrapper);
            if (mainImage === file) {
                mainImage = selectedImages.length > 0 ? selectedImages[0] : null;
                updateMainImage();
            }
        };

        const selectMainBtn = document.createElement("img");
        selectMainBtn.src = "../Assets/icons/mainImageIcon.svg";
        selectMainBtn.alt = "Select as main image";
        selectMainBtn.classList.add("main-btn");
        selectMainBtn.onclick = function () {
            mainImage = file;
            updateMainImage();
        };

        const wrapper = document.createElement("div");
        wrapper.classList.add("image-wrapper");
        wrapper.appendChild(imgElement);
        wrapper.appendChild(selectMainBtn);
        wrapper.appendChild(removeBtn);
        imagePreviewContainer.appendChild(wrapper);

        selectedImages.push(file);
        if (!mainImage) {
            mainImage = file;
            updateMainImage();
        }
    };
}

function updateMainImage() {
    document.querySelectorAll(".image-wrapper").forEach((wrapper, index) => {
        const img = selectedImages[index];
        const selectMainBtn = wrapper.querySelector(".main-btn");
        if (img === mainImage) {
            wrapper.classList.add("main-image");
            selectMainBtn.style.visibility = "hidden";
        } else {
            wrapper.classList.remove("main-image");
            selectMainBtn.style.visibility = "visible";
        }
    });
}

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
    formData.append("VillaName", villaName);
    formData.append("Description", description);
    formData.append("Price", price);
    formData.append("Location", location);
    formData.append("Capacity", capacity);
    formData.append("Bedrooms", bedrooms);
    formData.append("Bathrooms", bathrooms);
    formData.append("PropertyTagsJson", JSON.stringify(propertyTags));
    formData.append("LocationTagsJson", JSON.stringify(locationTags));

    // Append all images
    selectedImages.forEach((file) => {
        if (file === mainImage) {
            formData.append("MainImage", file);
        }
        else {
            formData.append("Images", file);
        }
    });



    try {
        const response = await AdminRequest.request("POST", `${AdminRequest.address}/upload-villa`, formData, true);
        if (response.success) {
            alert("Villa added successfully!");
            // window.location.href = "VillaList.html";
        } else {
            alert("Error: " + response.data.Reason);
        }
    } catch (error) {
        alert("Error submitting form.");
        console.error(error);
    }
});

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