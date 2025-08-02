(function ($) {
    $.createSpiralPattern = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        $(() => {


            $('#SpiralPatternTypeID').val(1);
            $('#Weekly').removeClass('d-none');

            $('#SpiralPatternTypeID').on('change', function (e) {
                e.preventDefault();

                var value = $(this).val();

                if (value === '1') {
                    $('#Weekly').removeClass('d-none');
                    $('#Fortnightly').addClass('d-none');
                    $('#Monthly').addClass('d-none');
                } else if (value === '2') {
                    $('#Fortnightly').removeClass('d-none');
                    $('#Weekly').addClass('d-none');
                    $('#Monthly').addClass('d-none');
                } else if (value === '3') {
                    $('#Monthly').removeClass('d-none');
                    $('#Weekly').addClass('d-none');
                    $('#Fortnightly').addClass('d-none');
                } 
            });



            // #region loadShiftByOrg
            // $('#addShiftModal').on('show.bs.modal', function () {
            $('#OrganizationID').on('change', function () {

                var organizationId = $(this).val();

                $.ajax({
                    url: '/CreateSpiralPattern/GetShiftByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (shifts) {
                        $('.AddShiftModalShiftID').each(function () {
                            const $dropdown = $(this);
                            $dropdown.empty();

                            $dropdown.append(`<option value="">Select Shift...</option>`);

                            shifts.forEach(shift => {
                                $dropdown.append(
                                    `<option value="${shift.id}">
                                    ${shift.name}
                                </option>`
                                );
                            });
                        });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading shifts:', error);
                    }
                });
            });
            // #endregion


            // #region Dropdowns
            function initOrganizationDD() {
                organizationDD = new Choices('#OrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Organization...'
                });
            }
            document.addEventListener('DOMContentLoaded', initOrganizationDD);
            initOrganizationDD();


            function initSpiralPatternTypeDD() {
                spiralPatternTypeDD = new Choices('#SpiralPatternTypeID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Pattern Type...'
                });
            }
            document.addEventListener('DOMContentLoaded', initSpiralPatternTypeDD);
            initSpiralPatternTypeDD();
            // #endregion




            let $activeShiftButton = null;

            $(document).on('click', '.add-shift-btn', function () {
                $activeShiftButton = $(this); // Save reference to clicked button
            });

            $('#saveShift').on('click', function () {
                const $select = $('#AddShiftModalShiftID');
                const selectedOption = $select.find('option:selected');
                const shiftName = selectedOption.text();
                const shiftId = selectedOption.val();

                if (!shiftId) {
                    alert('Please select a shift.');
                    return;
                }

                // ⬇ Get dynamic time range from selected option
                const startTime = selectedOption.data('start');
                const endTime = selectedOption.data('end');
                const timeRange = `${startTime} - ${endTime}`;

                const badgeHtml = `
        <div class="badge badge-phoenix-primary p-2">
            <p class="my-2 fs-10">${timeRange}</p>
            <div class="add-shift-btn2">
                <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal">
                    <i class="fas fa-edit text-success"></i>
                </a>
                <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                    <i class="fas fa-trash text-danger"></i>
                </a>
            </div>
        </div>
    `;

                if ($activeShiftButton) {
                    const $td = $activeShiftButton.closest('td');
                    $td.html(badgeHtml);
                    $activeShiftButton = null;
                }

                // Close the modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('addShiftModal'));
                modal.hide();
            });


            
        });

    }
}(jQuery));