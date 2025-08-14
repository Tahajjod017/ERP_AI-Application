//let autocomplete;

//function initAutocomplete() {
//    console.log("initAuto Running");
//    const input = document.getElementById("autocomplete");
//    if (!input) return;

//    autocomplete = new google.maps.places.Autocomplete(input, {
//        types: ["(cities)"],
//        fields: ["address_components", "geometry"],
//    });

//    autocomplete.addListener("place_changed", () => {
//        const place = autocomplete.getPlace();
//        let city = "", state = "", country = "";

//        for (const component of place.address_components) {
//            const type = component.types[0];
//            if (type === "locality") city = component.long_name;
//            if (type === "administrative_area_level_1") state = component.short_name;
//            if (type === "country") country = component.long_name;
//        }

//        document.getElementById("city").value = city;
//        document.getElementById("state").value = state;
//        document.getElementById("country").value = country;
//    });
//}

//// Init when modal is shown
//document.addEventListener('shown.bs.modal', function (event) {
//    console.log("running");
//    if (event.target.id === 'locationModal') {
//        setTimeout(() => {
//            initAutocomplete();
//        }, 300);
//    }
//});