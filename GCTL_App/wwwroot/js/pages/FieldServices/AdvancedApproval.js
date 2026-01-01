$(document).ready(function () {

    //#region LoadTableData
    function loadTableData(sortColumn, sortOrder) {
        var searchTerm = $("#empAdvanced-searchInput").val();

        $.ajax({
            url: '/AdvancedApproval/GetAll',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                console.log(response);
                var tableBody = $("#advancedApproval-tbody");
                tableBody.empty();

                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                            <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                                <td>
                                    <input type="checkbox" class="form-check-input addEmpCheck-selectedItem" data-id="${item.employeeAdvanceID}" />
                                </td>
                                <td class="empId align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">${item.customerID2}</td>
                                <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">${item.customerName}</td>
                                <td class="empProjectName align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTitle}</td>
                                <td class="empProjectType align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTypeName || 'N/A'}</td>
                                <td class="empSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.amountRequested || 0}</td>
                                <td class="empGroupName align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.groupEmployeeName}</td>
                                <td class="empStatus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.statusName}</td>
                                <td class="empapprovedName align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.requestedByUser}</td>
                                <td class="empDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.startDate}</td>
                                <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                                    <div class="d-flex btn-reveal-trigger position-static">
                                        <a href="#!"
                                           class="btn btn-phoenix-danger btn-icon me-1 fs-10 text-body px-0 employeeAdvance-editBtn"
                                           data-id="${item.employeeAdvanceID}"
                                           title="Edit">
                                            <i class="fas fa-edit text-black"></i>
                                        </a>
                                        <a href="#!" 
                                           class="btn btn-phoenix-danger btn-icon me-1 fs-10 text-body px-0 advance-delete" 
                                           data-id="${item.employeeAdvanceID}" 
                                           title="Delete">
                                            <i class="fa-regular fa-trash-can text-black"></i>
                                        </a>
                                    </div>
                                </td>
                            </tr>
                        `);
                    });
                }
            }
        });
    }
    //#endregion

});