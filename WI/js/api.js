class Requests {
    static get Adress() {
        return 'https://localhost:7290/api';
    }
}

class VillaRequests extends Requests {

    static get Adress() {
        return super.Adress + '/villa';
    }

    static async getVillas() {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open('GET', this.Adress);
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
            xhr.open('GET', `${this.Adress}/${idString}`);
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
        this.name = villa.naam;
        this.price = villa.prijs;
        this.image = villa.villaImage;
        this.location = villa.locatie;
        this.capacity = villa.capaciteit;
    }

    get html() {
        return `
        <div class="villaCard">
            <div class="image">
                <img src="${this.image}" alt="Villa">
            </div>
            <div class="info">
                <div class="title">
                    <h3>${this.name}</h3>
                </div>
                <div class="details">
                    <p>€${this.price},-</p>
                    <p>${this.location}</p>
                    <p>${this.capacity} personen</p>
                </div>
                <div class="actions">
                    <a href="villa.html?villaID=${this.id}" class="ButtonLink">Meer info</a>
                </div>
            </div>
        </div>
        `;
    }
}