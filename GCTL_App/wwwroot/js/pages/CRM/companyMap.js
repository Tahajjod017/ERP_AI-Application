let autocomplete;

function initAutocomplete() {
    console.log("Running");
    autocomplete = new google.maps.places.Autocomplete(
        document.getElementById("autocomplete"),
        {
            types: ["(cities)"], // or use ["geocode"] for full addresses
            fields: ["address_components", "geometry"],
        }
    );

    autocomplete.addListener("place_changed", () => {
        const place = autocomplete.getPlace();

        let city = "", state = "", country = "";

        for (const component of place.address_components) {
            const type = component.types[0];

            if (type === "locality") {
                city = component.long_name;
            }
            if (type === "administrative_area_level_1") {
                state = component.short_name;
            }
            if (type === "country") {
                country = component.long_name;
            }
        }

        //document.getElementById("city").value = city;
        //document.getElementById("state").value = state;
        //document.getElementById("country").value = country;
    });
}

window.initAutocomplete = initAutocomplete;