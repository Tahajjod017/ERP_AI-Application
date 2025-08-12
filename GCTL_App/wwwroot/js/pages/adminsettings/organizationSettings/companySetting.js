$(document).ready(function () {

    $('#logoUpload').on('change', function (event) {
        var reader = new FileReader();
        reader.onload = function (e) {
            // Display the selected image in the logo preview
            $('#logoPreview').attr('src', e.target.result);
        };
        // Read the file as a data URL (base64 encoded)
        if (event.target.files[0]) {
            reader.readAsDataURL(event.target.files[0]);
        }
    });


    $('#faviconUpload').on('change', function (event) {
        var reader = new FileReader();
        reader.onload = function (e) {
            // Display the selected image in the favicon preview
            $('#faviconPreview').attr('src', e.target.result);
        };
        // Read the file as a data URL (base64 encoded)
        if (event.target.files[0]) {
            reader.readAsDataURL(event.target.files[0]);
        }
    });
});

//$(document).ready(function () {
//    // When the logo input changes (image is selected)
//    $('#logoUpload').on('change', function (event) {
//        var file = event.target.files[0];

//        // Check if a file is selected
//        if (file) {
//            var reader = new FileReader();
//            reader.onload = function (e) {
//                var img = new Image();
//                img.onload = function () {
//                    // Validate image size (160px x 50px)
//                    if (img.width === 160 && img.height === 50) {
//                        // If valid size, show the image preview
//                        $('#logoPreview').attr('src', e.target.result);
//                        $('#logoPreview').show(); // Show the preview image
//                        $('#logoWarning').hide(); // Hide the warning message
//                        $('#clearLogo').show(); // Show the clear logo button
//                    } else {
//                        // If invalid size, hide preview and show warning
//                        $('#logoPreview').hide();
//                        $('#logoWarning').text('Invalid image size. Recommended size is 160px x 50px.').show();
//                        $('#clearLogo').hide();
//                    }
//                };
//                img.src = e.target.result; // Set the image source to the selected file
//            };
//            reader.readAsDataURL(file); // Read the file as a data URL
//        }
//    });

//    // When the favicon input changes (image is selected)
//    $('#faviconUpload').on('change', function (event) {
//        var file = event.target.files[0];

//        // Check if a file is selected
//        if (file) {
//            var reader = new FileReader();
//            reader.onload = function (e) {
//                var img = new Image();
//                img.onload = function () {
//                    // Validate image size (128px x 128px)
//                    if (img.width === 128 && img.height === 128) {
//                        // If valid size, show the image preview
//                        $('#faviconPreview').attr('src', e.target.result);
//                        $('#faviconPreview').show(); // Show the preview image
//                        $('#faviconWarning').hide(); // Hide the warning message
//                    } else {
//                        // If invalid size, hide preview and show warning
//                        $('#faviconPreview').hide();
//                        $('#faviconWarning').text('Invalid image size. Recommended size is 128px x 128px.').show();
//                    }
//                };
//                img.src = e.target.result; // Set the image source to the selected file
//            };
//            reader.readAsDataURL(file); // Read the file as a data URL
//        }
//    });
//});

// submit 
$(document).ready(function () {
    $('#companySettingsForm').on('submit', function (e) {
        e.preventDefault();  // Prevent the default form submission

        var form = $(this);
        var formData = new FormData(form[0]);  // Create a FormData object from the form

        
        for (var pair of formData) {
            console.log(pair[0] + ': ' + pair[1]);  
        }

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

$(document).on('click', '#addCompanySettings-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/CompanySettings/SoftDelete',
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
//edit
$(document).on('click', '#edit_organization_settingBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#edit_organization_setting').modal('show'); // Show the delete confirmation modal
    /* $('#confirmDeleteBtn').data('id', approvalSettingID); /*/// Store the approvalSettingID on the "Yes, Delete" button
});


// Function to load table data
var currentPage = 1;
var pageSize = 5;

$('#addCompanySettings-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addCompanySettings-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addCompanySettings-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addCompanySettings-nextPageBtn").on('click', function () {
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
    var searchTerm = $("#addCompanySettings-searchInput").val();

    $.ajax({
        url: '/CompanySettings/GetAllData',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#addCompanySettings-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.organizationID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-8">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ps-4">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.emailAddress}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.countryName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.emailAddress}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.phone}</td>
                           
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit_organization_settingBtn"
                               data-id="${item.organizationID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.organizationID}"
                              class="btn btn-outline-light btn-icon"  
                              id="addCompanySettings-singleDelBtn" >
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

            $("#addCompanySettings-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#addCompanySettings-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addCompanySettings-paginationLinks");
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
    $("#addCompanySettings-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addCompanySettings-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});


$('.column-toggle').on('change', function () {
    var column = $(this).data('column');
    var isChecked = $(this).prop('checked');

    if (isChecked) {
        showColumn(column);
    } else {
        hideColumn(column);
    }
});

// Show the column
function showColumn(column) {
    $("th[data-sort='" + column + "']").removeClass('d-none');
    $("td[data-column='" + column + "']").removeClass('d-none');
}

// Hide the column
function hideColumn(column) {
    $("th[data-sort='" + column + "']").addClass('d-none');
    $("td[data-column='" + column + "']").addClass('d-none');
}

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

