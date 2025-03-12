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

        //TODO: remove override
        this.image = 'Assets/villas/LuckyDuck/Exterior.avif'
    }

    get html() {
        return `
        <div class="villaCard">
            <div class="image">
                <img src="${this.image}" alt="Villa">
            </div>
            <div class="info">
                <div class="title">
                    <h2>${this.name}</h2>
                </div>
                <div class="details">
                    <p>${this.location}</p>
                    <p><svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
<path d="M8.00008 8.00002C6.99175 8.00002 6.12855 7.64099 5.4105 6.92294C4.69244 6.20488 4.33341 5.34169 4.33341 4.33335C4.33341 3.32502 4.69244 2.46183 5.4105 1.74377C6.12855 1.02571 6.99175 0.666687 8.00008 0.666687C9.00842 0.666687 9.87161 1.02571 10.5897 1.74377C11.3077 2.46183 11.6667 3.32502 11.6667 4.33335C11.6667 5.34169 11.3077 6.20488 10.5897 6.92294C9.87161 7.64099 9.00842 8.00002 8.00008 8.00002ZM0.666748 15.3334V12.7667C0.666748 12.2472 0.800429 11.7698 1.06779 11.3344C1.33515 10.899 1.69036 10.5667 2.13341 10.3375C3.08064 9.86391 4.04314 9.5087 5.02092 9.2719C5.99869 9.03509 6.99175 8.91669 8.00008 8.91669C9.00842 8.91669 10.0015 9.03509 10.9792 9.2719C11.957 9.5087 12.9195 9.86391 13.8667 10.3375C14.3098 10.5667 14.665 10.899 14.9324 11.3344C15.1997 11.7698 15.3334 12.2472 15.3334 12.7667V15.3334H0.666748ZM2.50008 13.5H13.5001V12.7667C13.5001 12.5986 13.4581 12.4459 13.374 12.3084C13.29 12.1709 13.1792 12.0639 13.0417 11.9875C12.2167 11.575 11.3841 11.2656 10.5438 11.0594C9.70355 10.8531 8.85564 10.75 8.00008 10.75C7.14453 10.75 6.29661 10.8531 5.45633 11.0594C4.61605 11.2656 3.78341 11.575 2.95841 11.9875C2.82091 12.0639 2.71015 12.1709 2.62612 12.3084C2.5421 12.4459 2.50008 12.5986 2.50008 12.7667V13.5ZM8.00008 6.16669C8.50425 6.16669 8.93585 5.98717 9.29487 5.62815C9.6539 5.26912 9.83342 4.83752 9.83342 4.33335C9.83342 3.82919 9.6539 3.39759 9.29487 3.03856C8.93585 2.67953 8.50425 2.50002 8.00008 2.50002C7.49591 2.50002 7.06432 2.67953 6.70529 3.03856C6.34626 3.39759 6.16675 3.82919 6.16675 4.33335C6.16675 4.83752 6.34626 5.26912 6.70529 5.62815C7.06432 5.98717 7.49591 6.16669 8.00008 6.16669Z" fill="#E9ECEF"/>
</svg>
${this.capacity} personen</p>
                </div>
                <div class="actions">
                    <h3 id="price">€${this.price},-</h3>
                    <a href="villa.html?villaID=${this.id}" class="ButtonLink">Bekijk</a>
                </div>
            </div>
        </div>
        `;
    }
}