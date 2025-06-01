$(function () {
    const form = $('#assignRoleForm')[0];

    // Enable Bootstrap validation styles on submit
    $('#assignRoleForm').on('submit', function (e) {
        e.preventDefault();
        e.stopPropagation();

        if (!form.checkValidity()) {
            $(form).addClass('was-validated');
            return;
        }

        // Collect form data
        const data = $(this).serialize();

        $.post('/RoleCreateAssign/AssignRole', data)
            .done(function () {
                alert('Role assigned successfully.');
                form.reset();
                $(form).removeClass('was-validated');
                $('#roleSelect').prop('disabled', true).html('<option value="" disabled selected>Select role</option>');
                $('#assignRoleBtn').prop('disabled', true);
                $('#userSearchResults').hide().empty();
            })
            .fail(function () {
                alert('Failed to assign role. Please try again.');
            });
    });

    // Load roles when company changes
    $('#companySelect').on('change', function () {
        const companyId = $(this).val();
        const roleSelect = $('#roleSelect');
        roleSelect.prop('disabled', true).html('<option>Loading roles...</option>');
        $('#assignRoleBtn').prop('disabled', true);

        if (!companyId) {
            roleSelect.prop('disabled', true).html('<option value="" disabled selected>Select role</option>');
            return;
        }

        $.get('/RoleCreateAssign/GetRolesByCompany', { companyId })
            .done(function (roles) {
                roleSelect.empty().append('<option value="" disabled selected>Select role</option>');
                roles.forEach(r => roleSelect.append(`<option value="${r.id}">${r.name}</option>`));
                roleSelect.prop('disabled', false);
            })
            .fail(function () {
                roleSelect.html('<option value="" disabled>Error loading roles</option>');
            });
    });

    // User search autocomplete
    let debounceTimer;
    $('#userSearch').on('input', function () {
        clearTimeout(debounceTimer);
        const query = $(this).val().trim();

        if (query.length < 2) {
            $('#userSearchResults').hide().empty();
            $('#selectedUserId').val('');
            $('#assignRoleBtn').prop('disabled', true);
            return;
        }

        debounceTimer = setTimeout(() => {
            $.get('/RoleCreateAssign/SearchUsers', { searchTerm: query })
                .done(users => {
                    const results = $('#userSearchResults').empty();
                    if (!users.length) {
                        results.hide();
                        $('#selectedUserId').val('');
                        $('#assignRoleBtn').prop('disabled', true);
                        return;
                    }

                    users.forEach(u => results.append(`
                        <li class="list-group-item list-group-item-action" data-user-id="${u.id}" role="option" tabindex="0">
                            ${u.userName} <small class="text-muted">(${u.email})</small>
                        </li>
                    `));
                    results.show();
                })
                .fail(() => {
                    $('#userSearchResults').hide().empty();
                    $('#selectedUserId').val('');
                    $('#assignRoleBtn').prop('disabled', true);
                });
        }, 300);
    });

    // Select user from list
    $('#userSearchResults').on('click keydown', 'li', function (e) {
        if (e.type === 'click' || e.key === 'Enter') {
            const userName = $(this).text().trim();
            const userId = $(this).data('user-id');

            $('#userSearch').val(userName);
            $('#selectedUserId').val(userId);
            $('#userSearchResults').hide().empty();

            // Enable submit if all fields valid
            validateForm();
        }
    });

    // Hide user search results on outside click
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#userSearch, #userSearchResults').length) {
            $('#userSearchResults').hide().empty();
        }
    });

    // Enable submit button only if all required fields have values
    $('#companySelect, #roleSelect, #selectedUserId').on('change input', validateForm);
    function validateForm() {
        const companyVal = $('#companySelect').val();
        const roleVal = $('#roleSelect').val();
        const userVal = $('#selectedUserId').val();

        $('#assignRoleBtn').prop('disabled', !(companyVal && roleVal && userVal));
    }
});








/////////TableData//////////////////

let currentPage = 1;
let pageSize = 5;

$('#role-pageSizeSelect').on('change', function () {
    const selectedSize = $(this).val();
    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadRoleTableData();
    }
});

$('#role-searchInput').on('input', function () {
    currentPage = 1;
    loadRoleTableData();
});

$('#role-prevPageBtn').on('click', function () {
    if (currentPage > 1) {
        currentPage--;
        loadRoleTableData();
    }
});

$('#role-nextPageBtn').on('click', function () {
    currentPage++;
    loadRoleTableData();
});

$(document).on('click', '#role-paginationLinks .page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadRoleTableData();
});

function loadRoleTableData() {
    const searchTerm = $('#role-searchInput').val();

    $.ajax({
        url: '/RoleCreateAssign/GetRoles', // replace with your real API URL
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm
        },
        success: function (response) {
            const tbody = $('#role-tBody');
            tbody.empty();

            if (response.data.length > 0) {
                response.data.forEach((item, index) => {
                    const rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tbody.append(`
                        <tr>
                            <td class="text-center"><input type="checkbox" class="role-selectItem" data-id="${item.id}" /></td>
                            <td>${rowIndex}</td>
                            <td>${item.companyName}</td>
                            <td>${item.roleName}</td>
                            <td>${item.createdBy}</td>
                            <td class="text-center">
                                
                            </td>
                        </tr>
                    `);
                });
            } else {
                tbody.append('<tr><td colspan="6" class="text-center">No data available</td></tr>');
            }

            updateRolePagination(response.paginationInfo);
        },
        error: function () {
            console.log("Error loading roles data.");
        }
    });
}

function updateRolePagination(paginationInfo) {
    $('#role-paginationInfo').text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} of ${paginationInfo.totalItems} entries`);

    const paginationLinks = $('#role-paginationLinks');
    paginationLinks.empty();

    const windowSize = 1;
    const currentPage = paginationInfo.currentPage;
    const totalPages = paginationInfo.totalPages;

    const createPageButton = (page) => `
        <li class="page-item ${page === currentPage ? 'active' : ''}">
            <button class="page-link page-btn" data-page="${page}">${page}</button>
        </li>
    `;

    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addEllipsis());
    }

    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);

    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }

    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    }

    $('#role-prevPageBtn').prop('disabled', currentPage === 1);
    $('#role-nextPageBtn').prop('disabled', currentPage === totalPages);
}

// Initial load
$(document).ready(function () {
    loadRoleTableData();
});
