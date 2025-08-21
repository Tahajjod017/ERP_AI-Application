(function ($) {
    $.menutabs = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            SaveBtn: '#menuTab-saveBtn',
            EditBtn: '#menuTab-edit',
            ResetBtn: '#menuTab-resetBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        $(() => {

            $(settings.SaveBtn).on('click', function (e) {
                var id = $('#menuTab-form #MenuTabId').val();
                var url = '';

                if (id > 0) {
                    url = '/MenuTabs/Update';
                } else {
                    url = '/MenuTabs/Create';
                }

                $('#menuTab-form').attr('action', url);

                $('#menuTab-form').submit();
            });



            $(document).on('click', settings.EditBtn, function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/MenuTabs/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#menuTab-form #MenuTabId').val(data.menuTabId);
                            $('#menuTab-form #Title').val(data.title);
                            if (typeChoices) {
                                typeChoices.setChoiceByValue(data.type);
                            }
                            $('#menuTab-form #ParentId').val(data.parentId);
                            $('#menuTab-form #OrderBy').val(data.orderBy);
                            $('#menuTab-form #ControllerName').val(data.controllerName);
                            $('#menuTab-form #ViewName').val(data.viewName);
                            $('#menuTab-form #Icon').val(data.icon);
                            $('#menuTab-form #IsActive').prop('checked', data.isActive);

                            $('#menuTab-form #menuTab-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });




            $(settings.ResetBtn).on('click', function () {
                clear();
            })

            function clear() {
                $('#menuTab-form')[0].reset();
                $('#MenuTabId').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#menuTab-form #menuTab-saveBtn').text('Save');
                $("#menuTab-check-all").prop('checked', false);
                $('.menuTab-selectItem').prop('checked', false);
                $('#menuTab-check-all').prop('checked', false).prop('indeterminate', false);

                if (typeChoices) {
                    typeChoices.setChoiceByValue('');
                }

                loadTableData();
            }


            $('#ActionTakenName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#ActionTakenName').val().trim();

                if (name === '') {
                    $('#ActionTakenName').css('border', '1px solid red');
                } else {
                    $('#ActionTakenName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#ActionTakenName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/enuTabs/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="ActionTakenName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="ActionTakenName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }


                        
            $(document).ready(function () {
                $('input[name="ControllerName"]').on('input', function () {
                    const controllerName = $(this).val();
                    $('input[name="ViewName"]').val(controllerName);
                });
            });


            let typeChoices;
            $(document).ready(function () {
                const typeSelect = document.getElementById('Type');
                if (typeSelect) {
                    typeChoices = new Choices(typeSelect, {
                        removeItemButton: false,
                        placeholder: true,
                        placeholderValue: 'Select Type',
                        shouldSort: false,
                        allowHTML: false,
                        searchEnabled: false
                    });
                }
            });



        });





        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                  Pagination Starts
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        var currentPage = 1;
        var pageSize = 5;

        $('#menuTab-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                loadTableData();
                currentPage = 1;
            }
        });



        $(document).ready(function () {
            loadTableData();

            $("#menuTab-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#menuTab-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#menuTab-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = '';
        let currentSortOrder = '';

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
            var searchTerm = $("#menuTab-searchInput").val();

            $.ajax({
                url: '/MenuTabs/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#menuTab-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-start text-middle align-middle text-nowrap ps-2" style="width: 5%;"> ${item.menuTabId}</td>
                                    <td class="sort text-start white-space-nowrap align-middle ps-2">${item.title}</td>
                                    <td class="text-start white-space-nowrap align-middle ps-2">${item.type}</td>
                                    <td class="text-center white-space-nowrap align-middle ps-2">
                                        ${item.parentId != null ? item.parentId.toString() : '-'}
                                    </td>
                                    <td class="text-center white-space-nowrap align-middle ps-2">${item.orderBy}</td>
                                    <td class="text-start white-space-nowrap align-middle ps-2">${item.controllerName}</td>
                                    <td class="text-start white-space-nowrap align-middle ps-2">${item.viewName}</td>
                                    <td class="text-start white-space-nowrap align-middle ps-2">${item.icon}</td>
                                    <td class="text-start white-space-nowrap align-middle ps-2">${item.isActive}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-3">
                                        <div class="row g-3">
                                            <a class="btn btn-phoenix-secondary btn-icon me-1 fs-10 text-body px-0 actionTaken-bulkEdit" href="#!" id="menuTab-edit" data-id="${item.menuTabId}""><i class="fas fa-edit"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#menuTab-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#menuTab-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#menuTab-paginationLinks");
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
            $("#menuTab-prevPageBtn").prop('disabled', currentPage === 1);
            $("#menuTab-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));