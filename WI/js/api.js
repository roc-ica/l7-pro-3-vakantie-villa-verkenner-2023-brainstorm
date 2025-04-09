class Requests {
    static get address() {
        return 'http://localhost:3010/api';
    }

    // internal function to send a request
    static async Sendrequest(method, endpoint, body, requiresAuth) {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open(method, endpoint);

            // Remove the Content-Type header for FormData
            if (!(body instanceof FormData)) {
                xhr.setRequestHeader("Content-Type", "application/json");
            }

            if (requiresAuth) {
                const sessionKey = sessionStorage.getItem("SessionKey");
                if (sessionKey) {
                    xhr.setRequestHeader("Authorization", `Bearer ${sessionKey}`);
                } else {
                    reject("No session key found. Please log in.");
                    return;
                }
            }

            xhr.onload = () => resolve(JSON.parse(xhr.responseText));
            xhr.onerror = () => reject(xhr.statusText);

            xhr.send(body instanceof FormData ? body : JSON.stringify(body));
        });
    }


    // public function to send a request handling errors
    static async request(method, endpoint, body, requiresAuth = false) {
        try {
            return await this.Sendrequest(method, endpoint, body, requiresAuth);
        }
        catch (error) {
            console.error(error);
            return { success: false, data: { Reason: error } };
        }
    }
}

class VillaRequests extends Requests {

    static get address() {
        return super.address + '/villa';
    }

    static async getVillas() {
        return await this.request('GET', `${this.address}/get-all`);
    }

    static async getVillasByIDs(ids) {
        return await this.request('POST', `${this.address}/get-by-ids`, ids);
    }

    static async getVillaByID(id) {
        return await this.request('POST', `${this.address}/get-by-id`, id);
    }

    static async getTags() {
        return await this.request('GET', `${this.address}/get-tags`);
    }

    static async getVillasByFilters(filters) {
        return await this.request('POST', `${this.address}/get-by-filters`, filters);
    }
}


class AdminRequest extends Requests {

    static get address() {
        return super.address + '/admin';
    }

    static async login(email, password) {
        return await this.request('POST', `${this.address}/login`, { email, password });
    }

    static async IsLoggedIn() {
        return await this.request('GET', `${this.address}/is-allowed`, {}, true);
    }

    static async addVilla(data) {
        return await this.request('POST', `${this.address}/upload-villa`, data, true);
    }
}

class MoreInfoRequest extends Requests {
    static get address() {
        return super.address + '/moreInfoRequest';
    }

    static async requestMoreInfo(villaId, email, message) {
        return await this.request('POST', `${this.address}/moreInfoRequest`, { villaId, email, message });
    }
}

class PDFRequest extends Requests {
    static get address() {
        return super.address + '/pdf';
    }

    static async generatePDF(VillaID) {
        return await this.request('POST', `${this.address}/get`, VillaID);
    }
}

// models

class SmallVilla {
    constructor(villa) {
        this.id = villa.VillaID;
        this.name = villa.Name;
        this.price = villa.Price;
        this.image = villa.VillaImagePath;
        this.location = villa.Location;
        this.capacity = villa.Capacity;
        this.bedrooms = villa.Bedrooms;
        this.bathrooms = villa.Bathrooms;
    }

    get html() {
        return `
        <div class="villaCard">
            <div class="imageContainer">
                <img src="${this.image}" alt="Villa">
            </div>
            <div class="info">
                <div class="title">
                    <h2>${this.name}</h2>
                </div>
                <div class="details">
                    <p>${this.location}</p>
                    <p><img src="Assets/icons/personIcon.svg" alt="Person icon">
${this.capacity} personen</p>
                    <p><img src="Assets/icons/bedIcon.svg" alt="Bed icon">${this.bedrooms} Slaapkamers</p>
                    <p><img src="Assets/icons/bathIcon.svg" alt="Bath icon">${this.bathrooms} Badkamers</p>
                
                </div>
                <div class="actions">
                    <h3 id="price">€${this.price},-</h3>
                    <a href="villa.html?villaID=${this.id}" class="buttonLink">Bekijk</a>
                </div>
            </div>
        </div>
        `;
    }
}
