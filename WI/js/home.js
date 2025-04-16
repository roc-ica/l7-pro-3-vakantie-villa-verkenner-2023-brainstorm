async function getVillas() {
    let count = 2
    for (let i = 0; i < count; i++) {
        document.getElementsByClassName('villaContainer')[0].innerHTML += `
            <div class="villaCard">
                <p style="margin-left:5px;">Loading...</p>
            </div>`;
    }

    const villas = await VillaRequests.getFirstVillas(count);

    if (villas.success === false) {
        document.getElementsByClassName('villaContainer')[0].innerHTML = `
            <div class="villaCard">
                <p style="margin-left:5px;">${villas.data.Reason}</p>
            </div>
        `;

        return;
    } else {
        document.getElementsByClassName('villaContainer')[0].innerHTML = '';
        console.log(villas); // nog nodig?
        villaData = JSON.parse(villas.data.Villas);

        for (let i = 0; i < villaData.length; i++) {
            const villa = new SmallVilla(villaData[i]);
            document.getElementsByClassName('villaContainer')[0].innerHTML += villa.html;
        }
    }
}

getVillas();