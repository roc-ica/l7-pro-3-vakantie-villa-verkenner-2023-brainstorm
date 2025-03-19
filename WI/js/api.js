class Requests {
    static get address() {
        return 'https://localhost:7290/api';
    }
}

class VillaRequests extends Requests {

    static get address() {
        return super.address + '/villa';
    }

    static async getVillas() {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open('GET', this.address);
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(JSON.parse(xhr.responseText));
                } else {
                    reject(xhr.statusText);
                }
            };
            xhr.onerror = () => reject(xhr.statusText);
            xhr.send();
        });
    }

    static async getVillasByIDs(ids) {
        let idString = ids.join(',');
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open('GET', `${this.address}/${idString}`);
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(JSON.parse(xhr.responseText));
                } else {
                    reject(xhr.statusText);
                }
            };
            xhr.onerror = () => reject(xhr.statusText);
            xhr.send();
        });
    }
}

class SmallVilla {
    constructor(villa) {
        this.id = villa.villaID;
        this.name = villa.name;
        this.price = villa.price;
        this.image = villa.villaImage;
        this.location = villa.location;
        this.capacity = villa.capacity;

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

class LoginRequest extends Requests{

    static get address(){
        return super.address + '/login';
    }
    static async login(email, password){
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open('GET', `${this.address}/${email}/${password}`);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(JSON.parse(xhr.responseText));
                } else {
                    reject(xhr.statusText);
                }
            };
            xhr.onerror = () => reject(xhr.statusText);
            xhr.send();
        });
    }
}