
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
    document.getElementsByClassName('villaContainer')[0].innerHTML = '';
    villas.forEach(villa => {
        let smallVilla = new SmallVilla(villa);
        document.getElementsByClassName('villaContainer')[0].innerHTML += smallVilla.html;
    });

}

getVillas();