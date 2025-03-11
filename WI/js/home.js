
async function getVillas() {
    const villas = await VillaRequests.getVillasByIDs([2, 5])
    villas.forEach(villa => {
        let smallVilla = new SmallVilla(villa);
        document.getElementsByClassName('VillaContainer')[0].innerHTML += smallVilla.html;
    });

}

getVillas();