class Requests {
    static get address() {
        return 'https://localhost:7290/api';
    }

    // internal function to send a request
    static async Sendrequest(method, endpoint, body) {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open(method, endpoint);
            xhr.setRequestHeader('Content-Type', 'application/json');

            xhr.onload = () => {
                resolve(JSON.parse(xhr.responseText));
            };

            xhr.onerror = () => reject(xhr.statusText);
            xhr.send(JSON.stringify(body));
        });
    }

    // public function to send a request handling errors
    static async request(method, endpoint, body) {
        try {
            return await this.Sendrequest(method, endpoint, body);
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

    static async getTags() {
        return await this.request('GET', `${this.address}/get-tags`);
    }
    static async getVillasByFilters(filters) {
        return await this.request('POST', `${this.address}/get-by-filters`, filters);
    }
}


class LoginRequest extends Requests {

    static get address() {
        return super.address + '/login';
    }

    static async login(email, password) {
        return await this.request('POST', `${this.address}/login`, { email, password });
    }
}


// models

class SmallVilla {
    constructor(villa) {
        this.id = villa.VillaID;
        this.name = villa.Name;
        this.price = villa.Price;
        this.image = villa.VillaImage;
        this.location = villa.Location;
        this.capacity = villa.Capacity;
        this.bedrooms = villa.Bedrooms;
        this.bathrooms = villa.Bathrooms;

        //TODO: remove override
        this.image = 'Assets/villas/LuckyDuck/Exterior.avif'
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
