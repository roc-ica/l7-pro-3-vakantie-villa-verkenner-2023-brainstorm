
async function getVillas() {
    const ids = [1, 2];
    for (let i = 0; i < ids.length; i++) {
        document.getElementsByClassName('villaContainer')[0].innerHTML += `
            <div class="villaCard">
                <p style="margin-left:5px;">Loading...</p>
            </div>
        `;
    }
    const villas = await VillaRequests.getVillasByIDs(ids)
    if (villas.success === false) {
        document.getElementsByClassName('villaContainer')[0].innerHTML = `
            <div class="villaCard">
                <p style="margin-left:5px;">${villas.data.Reason}</p>
            </div>
        `;
        return;
    }
    else {
        document.getElementsByClassName('villaContainer')[0].innerHTML = '';
        console.log(villas);
        villaData = JSON.parse(villas.data.Villas);
        for (let i = 0; i < villaData.length; i++) {
            const villa = new SmallVilla(villaData[i]);
            document.getElementsByClassName('villaContainer')[0].innerHTML += villa.html;
        }
    }
}

getVillas();