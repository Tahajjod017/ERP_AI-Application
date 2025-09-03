// submit 
$(document).ready(function () {
    $('#branchSettingsForm').on('submit', function (e) {
        e.preventDefault();  // Prevent the default form submission

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
    $('#edit_branch_setting').modal('show'); // Show the delete confirmation modal

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
                             <td class="align-middle white-space-nowrap ps-4">${item.emailAddress}</td>
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
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
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

// Initialize the map with the user's current location
function initMap() {
    // Default coordinates (San Francisco, will be overwritten by user's current location)
    const defaultLat = 37.7749;
    const defaultLng = -122.4194;

    // Try to get the user's current location
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            const userLat = position.coords.latitude;
            const userLng = position.coords.longitude;

            // Show an alert when GPS is successfully enabled and location is fetched
            alert("GPS is enabled. Current location: Latitude " + userLat + ", Longitude " + userLng);

            // Initialize the map with the user's location
            const mapOptions = {
                center: { lat: userLat, lng: userLng },
                zoom: 12,
            };

            map = new google.maps.Map(document.getElementById("map"), mapOptions);

            // Place a marker at the current location
            marker = new google.maps.Marker({
                position: map.getCenter(),
                map: map,
                draggable: true,  // Allow the user to drag the marker
            });

            // Update the Latitude and Longitude fields with the user's current position
            updateCoordinates(userLat, userLng);

            // Listen for marker drag and update latitude and longitude fields
            google.maps.event.addListener(marker, 'dragend', function (event) {
                const lat = event.latLng.lat();
                const lng = event.latLng.lng();
                updateCoordinates(lat, lng);
            });
        }, function () {
            alert("Geolocation service failed. Using default location.");
            // Fallback to default location if geolocation is not available
            const mapOptions = {
                center: { lat: defaultLat, lng: defaultLng },
                zoom: 12,
            };
            map = new google.maps.Map(document.getElementById("map"), mapOptions);

            // Place a marker at the default location
            marker = new google.maps.Marker({
                position: map.getCenter(),
                map: map,
                draggable: true,
            });

            updateCoordinates(defaultLat, defaultLng);
        });
    } else {
        alert("Geolocation is not supported by this browser.");
        // Fallback to default location if geolocation is not supported
        const mapOptions = {
            center: { lat: defaultLat, lng: defaultLng },
            zoom: 12,
        };
        map = new google.maps.Map(document.getElementById("map"), mapOptions);

        // Place a marker at the default location
        marker = new google.maps.Marker({
            position: map.getCenter(),
            map: map,
            draggable: true,
        });

        updateCoordinates(defaultLat, defaultLng);
    }

    // Listen for changes in the Latitude and Longitude input fields
    const latInput = document.getElementById("Latitude");
    const lngInput = document.getElementById("Longitude");

    latInput.addEventListener('input', function () {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        if (!isNaN(lat) && !isNaN(lng)) {
            const newLocation = new google.maps.LatLng(lat, lng);
            marker.setPosition(newLocation);  // Update the marker position
            map.setCenter(newLocation);  // Recenter the map
        }
    });

    lngInput.addEventListener('input', function () {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        if (!isNaN(lat) && !isNaN(lng)) {
            const newLocation = new google.maps.LatLng(lat, lng);
            marker.setPosition(newLocation);  // Update the marker position
            map.setCenter(newLocation);  // Recenter the map
        }
    });
}

// Update the Latitude and Longitude input fields
function updateCoordinates(lat, lng) {
    document.getElementById("Latitude").value = lat;
    document.getElementById("Longitude").value = lng;
}

// Call initMap when the script is loaded
window.initMap = initMap;