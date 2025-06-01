let assignPageSize = 5;
let assignCurrentPage = 1;
let roleListPageSize = 5;
let roleListCurrentPage = 1;

function loadPaginatedData(config) {
    const {
        url,
        searchInputSelector,
        pageSize,
        currentPage,
        tableBodySelector,
        paginationInfoSelector,
        paginationLinksSelector,
        buildRowHtml,
        onPageChange,
        extraParams = {}
    } = config;

    const searchTerm = $(searchInputSelector).val();


    $.get(url, {
        pageNumber: currentPage,
        pageSize: pageSize,
        searchTerm: searchTerm,
        ...extraParams
    }, function (response) {
        const tbody = $(tableBodySelector);
        tbody.empty();

        if (Array.isArray(response.data)) {
            response.data.forEach(item => tbody.append(buildRowHtml(item)));
        } else {
            for (const [role, users] of Object.entries(response.data)) {
                users.forEach(user => tbody.append(buildRowHtml(role, user)));
            }
        }

        updatePagination(response.totalCount, pageSize, currentPage, paginationLinksSelector, onPageChange);
        $(paginationInfoSelector).text(`Showing page ${currentPage} of ${Math.ceil(response.totalCount / pageSize)}`);
    });
}

function updatePagination(totalCount, pageSize, currentPage, containerSelector, onPageClick) {
    const totalPages = Math.ceil(totalCount / pageSize);
    const container = $(containerSelector);
    container.empty();

    for (let i = 1; i <= totalPages; i++) {
        const li = $(`<li class="page-item ${i === currentPage ? 'active' : ''}"><a class="page-link" href="#">${i}</a></li>`);
        li.on('click', function (e) {
            e.preventDefault();
            onPageClick(i);
        });
        container.append(li);
    }
}
$(document).on('change', '#SelectedCompanyId, #SelectedTenantId', function () {
    assignCurrentPage = 1;
    roleListCurrentPage = 1;
    loadAssignRolesTable();
    loadRoleUsersTable();
});



function loadAssignRolesTable() {
    const selectedCompanyId = $('#SelectedCompanyId').val();
    const selectedTenantId = $('#SelectedTenantId').val();
    loadPaginatedData({
        url: '/AccessPermission/GetAvailableRoles',
        searchInputSelector: '#roleAssign-searchInput',
        pageSize: assignPageSize,
        currentPage: assignCurrentPage,
        tableBodySelector: '#assignUserTableBody',
        paginationInfoSelector: '#assignPaginationInfo',
        paginationLinksSelector: '#assignPaginationLinks',
        buildRowHtml: (role) => {
            const isAdmin = role.roleName === "Admin";
            const deleteBtn = isAdmin
                ? `<button class="btn btn-danger btn-sm ms-1" disabled title="This role is protected">Delete</button>`
                : `<button class="btn btn-danger btn-sm ms-1 open-confirm-delete-modal" data-role-name="${role.roleName}">Delete</button>`;

            return `
                <tr data-role-name="${role.roleName}">
                    <td style="width: 20%; min-width: 150px;">${role.roleName}</td>
                    <td style="width: 30%; min-width: 250px;">
                        <input class="form-control user-search" data-role="${role.roleName}" placeholder="Search Users" />
                        <div class="user-results-scroll mt-2">
                            <ul id="userSearchResults-${role.roleName}" class="list-unstyled mb-0 user-results-list"></ul>
                        </div>
                    </td>
                    <td style="width: 35%; min-width: 250px;">
                        <ul id="selectedUsers-${role.roleName}" class="list-unstyled mt-2 ps-2 small text-body scrollable-user-list"></ul>
                    </td>
                    <td style="width: 15%; min-width: 100px;" class="text-end">
                        <div class="btn-group" role="group">
                            <button class="btn btn-success btn-sm assign-role-btnR" data-role="${role.roleName}" disabled>Assign</button>
                            ${deleteBtn}
                        </div>
                    </td>
                </tr>
            `;
        },
        onPageChange: (page) => {
            assignCurrentPage = page;
            loadAssignRolesTable();
        },
        extraParams: {
            companyId: selectedCompanyId,
            tenantId: selectedTenantId
        }
    });
}
$(document).on('input', '#roleAssign-searchInput', function () {
    assignCurrentPage = 1;
    loadAssignRolesTable();
});


function loadRoleUsersTable() {
    const selectedCompanyId = $('#SelectedCompanyId').val();
    const selectedTenantId = $('#SelectedTenantId').val();
    loadPaginatedData({
        url: '/AccessPermission/GetRoleUserAssignments',
        searchInputSelector: '#roleList-searchInput',
        pageSize: roleListPageSize,
        currentPage: roleListCurrentPage,
        tableBodySelector: '#roleList-tBody',
        paginationInfoSelector: '#roleList-paginationInfo',
        paginationLinksSelector: '#rolePaginationLinks',
        buildRowHtml: (roleName, user) => `
            <tr>
                <td>${roleName}</td>
                <td>${user.userName}</td>
                <td class="text-end">
                    <form class="remove-user-form d-inline" method="post" action="/AccessPermission/RemoveUserFromRole">
                        <input type="hidden" name="roleName" value="${roleName}" />
                        <input type="hidden" name="userName" value="${user.userName}" />
                        <input type="hidden" name="__RequestVerificationToken" value="${$('input[name="__RequestVerificationToken"]').val()}" />
                        <button type="submit" class="btn btn-phoenix-danger btn-sm"><i class="fas fa-trash-alt"></i></button>
                    </form>
                </td>
            </tr>
        `,
        onPageChange: (page) => {
            roleListCurrentPage = page;
            loadRoleUsersTable();
        },
        extraParams: {
            companyId: selectedCompanyId,
            tenantId: selectedTenantId,
        }
    });
}

$(document).ready(function () {
    loadAssignRolesTable();
    loadRoleUsersTable();
    $('#roleList-searchInput').on('click', function () {
        
        roleListCurrentPage = 1;
       
        loadRoleUsersTable();
    });
});
$(document).on('click', '#roleList-searchInput', function () {
        roleListCurrentPage = 1;

        loadRoleUsersTable();

});
$(document).on('input', '#roleList-searchInput', function () {
    roleListCurrentPage = 1;
    loadRoleUsersTable();
});
$(document).on('click', '#roleAssign-searchInput', function () {
    assignCurrentPage = 1;
    loadAssignRolesTable();
});
$(document).on('click', '#roleAssign-pageSizeDropdown .dropdown-item, .dropdown-item', function (e) {
    e.preventDefault(); // Prevent page reload due to href=""
});
$(document).on('click', '#roleList-pageSizeDropdown .dropdown-item', function (e) {
    e.preventDefault(); 
    // your existing code here
});

// New event for page size change in Assign Roles Table
$(document).on('click', '#roleAssign-pageSizeDropdown .dropdown-item', function () {
    const newPageSize = $(this).data('size');
    assignPageSize = newPageSize;
    $('#selectedPageSize').text(newPageSize); // Update the "Showing" label
    assignCurrentPage = 1; // Reset the current page to 1
    loadAssignRolesTable(); // Reload the table with the new page size
});

// New event for page size change in Role Users List
$(document).on('click', '#roleList-pageSizeDropdown .dropdown-item', function () {
    const newPageSize = $(this).data('size');
    roleListPageSize = newPageSize;
    $('#selectedPageSize2').text(newPageSize); // Update the "Showing" label
    roleListCurrentPage = 1; // Reset the current page to 1
    loadRoleUsersTable(); // Reload the table with the new page size
});

$(document).ready(function () {
    // When roles are loaded, apply scroll class
    $(document).on('DOMNodeInserted', 'ul[id^="selectedUsers-"]', function () {
        $(this).addClass('scrollable-user-list');
    });
});
