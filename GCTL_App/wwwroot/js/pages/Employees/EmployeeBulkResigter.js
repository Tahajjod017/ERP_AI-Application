
let connection;
let connectionId;

// Initialize SignalR connection
async function initSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/employeeUploadHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveProgress", function (data) {
        updateProgress(data.current, data.total, data.percentage, data.message);
    });

    try {
        await connection.start();
        connectionId = connection.connectionId;
        console.log("SignalR Connected:", connectionId);
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        setTimeout(initSignalR, 5000);
    }
}

// Initialize on page load
$(document).ready(function () {
    initSignalR();

    // Search functionality with debounce
    let searchTimeout;
    $('#searchInput').on('input', function () {
        clearTimeout(searchTimeout);
        const searchTerm = $(this).val();
        const pageSize = $('#pageSizeSelect').val();

        searchTimeout = setTimeout(function () {
            window.location.href = `/EmployeeBulkResigter/Preview?page=1&pageSize=${pageSize}&search=${encodeURIComponent(searchTerm)}`;
        }, 500);
    });

    // Page size change
    $('#pageSizeSelect').on('change', function () {
        const pageSize = $(this).val();
        const searchTerm = $('#searchInput').val();
        window.location.href = `/EmployeeBulkResigter/Preview?page=1&pageSize=${pageSize}&search=${encodeURIComponent(searchTerm)}`;
    });

    // Confirm Save Button - Show Confirmation Modal
    $('#confirmSaveBtn').on('click', function () {
       // $('#confirmModal').modal('show');
    });

    // Proceed with save
    $('#proceedSaveBtn').on('click', async function () {
        if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
            alert('Connection not ready. Please wait and try again.');
            return;
        }

      //  $('#confirmModal').modal('hide');
        hideModal('confirmModal')

        const btn = $('#confirmSaveBtn');
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');

        showProgressModal();

        try {
            const response = await fetch('/EmployeeBulkResigter/ConfirmSave', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-SignalR-ConnectionId': connectionId,
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            const result = await response.json();

            if (result.success) {
                setTimeout(() => {
                    hideProgressModal();
                    showSuccessModal(result.message);
                }, 1500);
            } else {
                hideProgressModal();
                alert(result.message || 'An error occurred while saving.');
                btn.prop('disabled', false).html('<i class="fas fa-check me-2"></i>Confirm & Save');
            }
        } catch (error) {
            hideProgressModal();
            console.error('Error:', error);
            alert('An error occurred while saving employee data.');
            btn.prop('disabled', false).html('<i class="fas fa-check me-2"></i>Confirm & Save');
        }
    });

    // Set current search term and page size
    const urlParams = new URLSearchParams(window.location.search);
    const searchTerm = urlParams.get('search') || '';
    const pageSize = urlParams.get('pageSize') || '10';

    $('#searchInput').val(searchTerm);
    $('#pageSizeSelect').val(pageSize);
});

// Edit Employee
function editEmployee(index) {
    $.get(`/EmployeeBulkResigter/GetEmployee?index=${index}`, function (data) {
        if (data.success) {
            const emp = data.employee;
            $('#editIndex').val(index);
            $('#editEmployeeCode').val(emp.employeeCode);
            $('#editFirstName').val(emp.firstName);
            $('#editLastName').val(emp.lastName);
            $('#editGender').val(emp.gender);
            $('#editEmail').val(emp.email);
            $('#editOfficialEmail').val(emp.officialEmail);
            $('#editPhoneNumber').val(emp.phoneNumber);
            $('#editBranch').val(emp.branch);
            $('#editJoiningDate').val(emp.joiningDate ? emp.joiningDate.split('T')[0] : '');
            $('#editComment').val(emp.comment);
            $('#editDesignation').val(emp.designation);
            $('#editDepartmentName').val(emp.departmentName);
            $('#editImmediateSupervisorName').val(emp.immediateSupervisorName);
            $('#editDepartmentHeadName').val(emp.departmentHeadName);

           // $('#editModal').modal('show');
        }
    });
}

// Save Edit
$('#saveEditBtn').on('click', function () {
    const formData = {
        index: $('#editIndex').val(),
        employeeCode: $('#editEmployeeCode').val(),
        firstName: $('#editFirstName').val(),
        lastName: $('#editLastName').val(),
        gender: $('#editGender').val(),
        email: $('#editEmail').val(),
        officialEmail: $('#editOfficialEmail').val(),
        phoneNumber: $('#editPhoneNumber').val(),
        branch: $('#editBranch').val(),
        joiningDate: $('#editJoiningDate').val(),
        comment: $('#editComment').val(),
        designation: $('#editDesignation').val(),
        departmentName: $('#editDepartmentName').val(),
        immediateSupervisorName: $('#editImmediateSupervisorName').val(),
        departmentHeadName: $('#editDepartmentHeadName').val()
    };

    $.ajax({
        url: '/EmployeeBulkResigter/UpdateEmployee',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            if (result.success) {

                hideModal('editModal');
               // $('#editModal').modal('hide');
                location.reload();
            } else {
                alert(result.message);
            }
        }
    });
});

// Delete Employee
function deleteEmployee(index) {
    if (confirm('Are you sure you want to remove this employee from the list?')) {
        $.post('/EmployeeBulkResigter/DeleteEmployee',
            {
                index: index,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            function (result) {
                if (result.success) {
                    location.reload();
                } else {
                    alert(result.message);
                }
            }
        );
    }
}

function showConfirmModal() {
    // $('#confirmModal').modal('show');
}

function showProgressModal() {
    const modal = `
    <div class="modal fade show" id="progressModal" data-bs-backdrop="static" data-bs-keyboard="false"
        style="display: block; background: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="fas fa-spinner fa-spin me-2"></i>Processing Employee Registration</h5>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <div class="progress" style="height: 25px;">
                            <div id="progressBar" class="progress-bar progress-bar-striped progress-bar-animated"
                                role="progressbar" style="width: 0%">0%</div>
                        </div>
                    </div>
                    <div id="progressMessage" class="text-center fw-bold">Initializing...</div>
                    <div id="progressCount" class="text-center text-muted mt-2">0 / 0</div>
                </div>
            </div>
        </div>
    </div>`;
    $('body').append(modal);
}

function hideProgressModal() {
    // $('#progressModal').remove();
    hideModal('progressModal')
}

function updateProgress(current, total, percentage, message) {
    $('#progressBar').css('width', percentage + '%').text(percentage + '%');
    $('#progressMessage').text(message);
    $('#progressCount').text(`${current} / ${total}`);

    if (percentage >= 100) {
        $('#progressBar').removeClass('progress-bar-animated')
            .addClass('bg-success');
    }
}

function showSuccessModal(message) {
    const modal = `
    <div class="modal fade show" id="successModal" data-bs-backdrop="static"
        style="display: block; background: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title"><i class="fas fa-check-circle me-2"></i>Success</h5>
                </div>
                <div class="modal-body text-center py-4">
                    <i class="fas fa-check-circle text-success" style="font-size: 48px;"></i>
                    <p class="mt-3 mb-0">${message}</p>
                </div>
                <div class="modal-footer justify-content-center">
                    <button type="button" class="btn btn-phoenix-primary" onclick="uploadMore()">
                        <i class="fas fa-upload me-2"></i>Upload More
                    </button>
                    <button type="button" class="btn btn-phoenix-success" onclick="goToEmployeePage()">
                        <i class="fas fa-users me-2"></i>Go to Employee Page
                    </button>
                </div>
            </div>
        </div>
    </div>`;
    $('body').append(modal);
}

function uploadMore() {
    window.location.href = '/EmployeeBulkResigter/Index';
}

function goToEmployeePage() {
    window.location.href = '/Employeelist/index';
}
