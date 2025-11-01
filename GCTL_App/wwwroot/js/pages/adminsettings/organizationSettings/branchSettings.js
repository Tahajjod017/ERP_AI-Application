// submit 
$(document).ready(function () {
    $('#branchSettingsForm').on('submit', function (e) {
        e.preventDefault();  // Prevent the default form submission
        // === ONLY OrganizationID Validation ===
        if (!$('#OrganizationID').val()) {
            $('#OrganizationID').closest('.choices').addClass('is-invalid');
            toastr.error('Please select an organization');
            $('span[data-valmsg-for="OrganizationID"]').html('Organization is required');
            return;
        }
        var form = $(this);
        var formData = new FormData(form[0]);  // Create a FormData object from the form


        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success(response.message, '');
                    form.trigger("reset");
                    loadTableData();
                    choiceManager.resetChoice('OrganizationID');
                } else {
                    toastr.error(response.message, 'Error');
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Unexpected error: " + error, 'Server Error');
            }
        });
    });
});
// Clear validation when user selects organization
$('#OrganizationID').on('change', function () {
    $(this).closest('.choices').removeClass('is-invalid');
    $('span[data-valmsg-for="OrganizationID"]').html('');
});
$(document).on('click', '#addBranchSettings-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/BranchSettings/SoftDelete',
            method: 'POST',
            data: { ids: [id] },
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success(response.message);
                    // Optionally, reload the table data or remove the deleted row from the table
                    loadTableData(); // Reload data after delete
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("Error occurred while deleting.");
            },
            complete: function () {
                // Hide the modal after the action
                $('#confirmDeleteModal').modal('hide');
            }
        });
    } else {
        toastr.error("Invalid action.");
    }
});

$(document).on('click', '#edit_branch_settingBtn', function () {
    var holidaySettingID = $(this).data('id');
   // $('#edit_branch_setting').modal('show'); // Show the delete confirmation modal
    var myModal = new bootstrap.Modal(document.getElementById('edit_branch_setting'));
    myModal.show();
    // Store the ID in the hidden input field
    $('#OrganizationBranchIDEdit').val(holidaySettingID);

    // Load the existing data for the selected holiday setting
    $.ajax({
        url: '/BranchSettings/GetById',
        method: 'GET',
        data: { id: holidaySettingID },
        success: function (response) {
            if (response.isSuccess) {
                // Populate the form fields with the existing data  
                choiceManager.setChoiceValue('OrganizationIDEdit', response.data.organizationID);
                $('#OrganizationBranchNameEdit').val(response.data.organizationBranchName);
                $('#EmailAddressEdit').val(response.data.emailAddress);
                $('#PhoneEdit').val(response.data.phone);
                $('#FaxEdit').val(response.data.fax);
                $('#WebAddressEdit').val(response.data.webAddress);
               // $('#WebAddressEdit').val(response.data.webAddress);
                $('#AddressEdit').val(response.data.address);

                choiceManager.setChoiceValue('CountryIDEdit', response.data.CountryID);

                $('#StreetEdit').val(response.data.street);

                $('#CityEdit').val(response.data.city);
                $('#PostCodeEdit').val(response.data.postCode);
                $('#LatitudeEdit').val(response.data.latitude);
                $('#LongitudeEdit').val(response.data.longitude);

                
                // Initialize the datepicker for the edit form

               
            } else {
                toastr.error(response.message, 'Error');
            }
        },
        error: function (xhr, status, error) {
            // Handle Access Denied error (403)
            if (xhr.status === 403 && xhr.responseJSON && xhr.responseJSON.message === "Access denied.") {
                // Redirect to AccessDenied page
                window.location.href = '/Home/AccessDenied'; // Change URL to your actual AccessDenied page
            } else {
                toastr.error("Unexpected error: " + error, 'Server Error');
            }
        }
    });

    /* $('#confirmDeleteBtn').data('id', approvalSettingID); /*/// Store the approvalSettingID on the "Yes, Delete" button
});

$('#branchSettingsFormEdit').submit(function (event) {
    event.preventDefault(); // Prevent default form submission

    var formData = $(this).serialize(); // Serialize the form data

    // Append the approvalSettingID to the form data
    // formData += '&approvalSettingID=' + weekendSettingID;

    // Send the data via AJAX
    $.ajax({
        url: '/BranchSettings/Updates', // Adjust URL if necessary
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                // Handle success
                toastr.success('Branch setting updated successfully!');
                $('#edit_branch_setting').modal('hide'); // Hide the modal
                loadTableData();
            } else {
                // Handle failure
                toastr.error('Failed to update Branch setting: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            // Handle AJAX errors
            toastr.error('Error: ' + error);
        }
    });
});


// Function to load table data
var currentPage = 1;
var pageSize = 5;

$('#addBranchSettings-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addBranchSettings-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addBranchSettings-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addBranchSettings-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});


let currentSortColumn = 'BloodGroupName';
let currentSortOrder = 'asc';

$('th.sort').on('click', function () {
    const column = $(this).data('sort');

    if (currentSortColumn === column) {
        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortOrder = 'asc';
    }

    loadTableData(currentSortColumn, currentSortOrder);
    updateSortingIndicator(column, currentSortOrder);
});


function updateSortingIndicator() {
    $('th.sort').each(function () {
        const $th = $(this);
        const column = $th.data('sort');
        $th.find('.sort-icon').remove();

        if (column === currentSortColumn) {
            const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
        } else {
            $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
        }
    });
}

function loadTableData(sortColumn, sortOrder) {
    var searchTerm = $("#addBranchSettings-searchInput").val();

    $.ajax({
        url: '/BranchSettings/GetAllData',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#addBranchSettings-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.organizationBranchID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-8">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ps-4">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.organizationBranchName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.address}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.countryName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.emailAddress}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.phone}</td>
                           
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit_branch_settingBtn"
                               data-id="${item.organizationBranchID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                             
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.organizationBranchID}"
                              class="btn btn-outline-light btn-icon"  
                              id="addBranchSettings-singleDelBtn" >
                              <i class="far fa-trash-alt text-black"></i>
                            </a>
                          </div>
                    </td>
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#addBranchSettings-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#addBranchSettings-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addBranchSettings-paginationLinks");
    paginationLinks.empty();
    // Window size (number of pages before/after the current page)
    const windowSize = 1;
    const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;
    // Helper function for ellipsis
    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
    // Add "First Page" and ellipsis if needed
    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addEllipsis());
    }
    // Add page number buttons within the window range
    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);
    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }
    // Add ellipsis and "Last Page" button if needed
    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    }
    // Disable or enable previous/next buttons
    $("#addBranchSettings-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addBranchSettings-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});


let map;
let marker;
let autocomplete;
let namCompany = '';


// Initialize the map with a default location
function initMap() {
    const defaultLat = 37.7749;
    const defaultLng = -122.4194;

    // Create map options
    const mapOptions = {
        center: { lat: defaultLat, lng: defaultLng },
        zoom: 12,
    };

    map = new google.maps.Map(document.getElementById("map"), mapOptions);

    // Add a marker at the center (default location)
    marker = new google.maps.Marker({
        position: map.getCenter(),
        map: map,
        draggable: true,
    });

    // Set up the address autocomplete
    const input = document.getElementById('FullAddress');
    autocomplete = new google.maps.places.Autocomplete(input);

    // Bind the autocomplete to the map and fields
    autocomplete.bindTo('bounds', map);

    // Listen for place selection and update fields
    autocomplete.addListener('place_changed', function () {
        debugger
        const place = autocomplete.getPlace();

        if (!place.geometry) {
            alert("Place details not found");
            return;
        }

        // Set map center to the selected place's location
        map.setCenter(place.geometry.location);
        map.setZoom(15);

        // Place a marker at the selected location
        marker.setPosition(place.geometry.location);

        // Fill in the address fields based on selected place
        updateAddressFields(place);

        namCompany = place.name;
    });

    // Listen for changes in Latitude and Longitude input fields
    const latInput = document.getElementById("Latitude");
    const lngInput = document.getElementById("Longitude");

    latInput.addEventListener('input', function () {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        if (!isNaN(lat) && !isNaN(lng)) {
            const newLocation = new google.maps.LatLng(lat, lng);
            marker.setPosition(newLocation);
            map.setCenter(newLocation);
        }
    });

    lngInput.addEventListener('input', function () {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        if (!isNaN(lat) && !isNaN(lng)) {
            const newLocation = new google.maps.LatLng(lat, lng);
            marker.setPosition(newLocation);
            map.setCenter(newLocation);
        }
    });
}

// Update the fields when an address is selected
function updateAddressFields(place) {
    const addressField = document.getElementById('Address');
    const cityField = document.getElementById('City');
    const streetField = document.getElementById('Street');
    const postcodeField = document.getElementById('PostCode');
    const latitudeField = document.getElementById('Latitude');
    const longitudeField = document.getElementById('Longitude');

    let address = '';
    let city = '';
    let street = '';
    let postalCode = '';

    // Loop through the address components
    for (let i = 0; i < place.address_components.length; i++) {

        const component = place.address_components[i];
        console.log(component)
        if (component.types.includes("street_number")) {
            street = component.long_name;
        }
        if (component.types.includes("route")) {
            street += ' ' + component.long_name;
        }
        if (component.types.includes("locality")) {
            city = component.long_name;
        }
        if (component.types.includes("postal_code")) {
            postalCode = component.long_name;
        }
    }

    // Set the fields with the address details
    addressField.value = street;
    cityField.value = city;
    streetField.value = street;
    postcodeField.value = postalCode;
    latitudeField.value = place.geometry.location.lat();
    longitudeField.value = place.geometry.location.lng();
}

// Call initMap when the script is loaded
window.initMap = initMap;


// --- KEEP YOUR ORIGINAL VARIABLES & initMap / updateAddressFields AS-IS ---
// ------------------ EDIT-ONLY STATE ------------------
let mapEdit;
let markerEdit;
let autocompleteEdit;
let geocoderEdit;

// Clone of your address updater for EDIT fields
function updateAddressFieldsEdit(place) {
    const addressField = document.getElementById('AddressEdit');
    const cityField = document.getElementById('CityEdit');
    const streetField = document.getElementById('StreetEdit');
    const postcodeField = document.getElementById('PostCodeEdit');
    const latitudeField = document.getElementById('LatitudeEdit');
    const longitudeField = document.getElementById('LongitudeEdit');

    let street = '', city = '', postalCode = '';

    if (place.address_components) {
        for (const component of place.address_components) {
            if (component.types.includes('street_number')) street = component.long_name + (street ? ' ' + street : '');
            if (component.types.includes('route')) street = (street ? street + ' ' : '') + component.long_name;
            if (component.types.includes('locality')) city = component.long_name;
            if (component.types.includes('postal_code')) postalCode = component.long_name;
        }
    }

    if (addressField) addressField.value = street;
    if (cityField) cityField.value = city;
    if (streetField) streetField.value = street;
    if (postcodeField) postcodeField.value = postalCode;

    if (place.geometry?.location) {
        if (latitudeField) latitudeField.value = place.geometry.location.lat();
        if (longitudeField) longitudeField.value = place.geometry.location.lng();
    }
}

// Build the EDIT map once
function initMapEdit() {
    const mapEl = document.getElementById('mapEdit');
    if (!mapEl) return;

    if (!geocoderEdit) geocoderEdit = new google.maps.Geocoder();

    const defaultCenter = { lat: 23.8103, lng: 90.4125 }; // Dhaka default
    mapEdit = new google.maps.Map(mapEl, { center: defaultCenter, zoom: 12 });

    markerEdit = new google.maps.Marker({
        position: defaultCenter,
        map: mapEdit,
        draggable: true
    });

    // Drag marker -> update Lat/Lng
    markerEdit.addListener('dragend', () => {
        const p = markerEdit.getPosition();
        document.getElementById('LatitudeEdit').value = p.lat();
        document.getElementById('LongitudeEdit').value = p.lng();
    });

    // Autocomplete for EDIT
    const input = document.getElementById('FullAddressEdit');
    if (input) {
        autocompleteEdit = new google.maps.places.Autocomplete(input);
        autocompleteEdit.bindTo('bounds', mapEdit);

        autocompleteEdit.addListener('place_changed', function () {
            const place = autocompleteEdit.getPlace();
            if (!place.geometry) {
                alert('Place details not found');
                return;
            }
            mapEdit.setCenter(place.geometry.location);
            mapEdit.setZoom(15);
            markerEdit.setPosition(place.geometry.location);
            updateAddressFieldsEdit(place);
        });
    }

    // Manual Lat/Lng typing -> move marker/map
    const latInput = document.getElementById('LatitudeEdit');
    const lngInput = document.getElementById('LongitudeEdit');
    const syncFromInputs = () => {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        if (!isNaN(lat) && !isNaN(lng)) {
            const pos = new google.maps.LatLng(lat, lng);
            markerEdit.setPosition(pos);
            mapEdit.setCenter(pos);
        }
    };
    latInput?.addEventListener('input', syncFromInputs);
    lngInput?.addEventListener('input', syncFromInputs);
}

// Recenter EDIT map from current fields (works after AJAX set)
function centerEditMapFromCurrentFields() {
    if (!mapEdit) return;

    const lat = parseFloat(document.getElementById('LatitudeEdit')?.value);
    const lng = parseFloat(document.getElementById('LongitudeEdit')?.value);
    const fullAddr = (document.getElementById('FullAddressEdit')?.value || '').trim();

    if (!isNaN(lat) && !isNaN(lng)) {
        const pos = new google.maps.LatLng(lat, lng);
        mapEdit.setCenter(pos);
        mapEdit.setZoom(15);
        markerEdit.setPosition(pos);
        return;
    }

    if (fullAddr) {
        if (!geocoderEdit) geocoderEdit = new google.maps.Geocoder();
        geocoderEdit.geocode({ address: fullAddr }, (results, status) => {
            if (status === 'OK' && results && results[0]) {
                const loc = results[0].geometry.location;
                mapEdit.setCenter(loc);
                mapEdit.setZoom(15);
                markerEdit.setPosition(loc);
                updateAddressFieldsEdit(results[0]); // backfill street/city/postcode/lat/lng
            }
        });
    }
}

// Wire up modal lifecycle
document.addEventListener('DOMContentLoaded', function () {
    const modalEl = document.getElementById('edit_branch_setting');
    if (!modalEl) return;

    modalEl.addEventListener('shown.bs.modal', function () {
        // initialize once
        if (!mapEdit) initMapEdit();
        // fix tiles
        google.maps.event.trigger(mapEdit, 'resize');
        // center to current values (in case fields were prefilled)
        centerEditMapFromCurrentFields();
    });

    // optional: clear on close
    modalEl.addEventListener('hidden.bs.modal', function () {
        // document.getElementById('companySettingsFormEdit')?.reset();
        // If you use Choices, clear those here
    });
});
