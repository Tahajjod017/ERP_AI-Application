$(function () {

	// active item buttons
	$(".option-btn").on('click', function () {
		$(".option-btn").removeClass('active');
		$(this).addClass('active');

		console.log($(this).data('id'));
	});

	// addLDetails button work
	$('#addLDetails').on('click', function (e) {
		e.preventDefault();
		debugger;
		let buttonID = $(".option-btn.active").data('id');
		let date = $("#lDetailsDate").val();
		let text = $("#lDetailsText").val();
		let id = $("#leadID").val();

        //let dateTime = parseDateToDBFormat(date);
		console.log(date);
		let convertedDate = convertToISODateTime(date);
		console.log();

		debugger;
		let data = {
			LeadID: id,
			LeadActivityTypeID: buttonID,
			ActivityDateTime: convertedDate,
			ActivityNote: text,
		};
		console.log(JSON.stringify(data));

		$.ajax({
			url: '/LeadDetails/CeateLeadDetail',
			method: 'POST',
			contentType: 'application/json',
			data: JSON.stringify(data),
			success: function (response) {
				debugger;
				toastr.success(response.message);
			},
			error: function (error) {
				toastr.error(error.message);
			}
		});
	});

	function convertToISODateTime(dateTimeString) {
		// Split input into date and time parts
		const [datePart, timePart] = dateTimeString.split(' ');

		// Validate input format
		if (!datePart || !timePart) {
			throw new Error('Invalid format. Expected dd/mm/yy hh:mm');
		}

		// Split date (dd/mm/yy) and validate
		const [day, month, year] = datePart.split('/').map(Number);
		if (!day || !month || !year) {
			throw new Error('Invalid date format. Expected dd/mm/yy');
		}

		// Handle two-digit year (assume 21st century)
		const fullYear = year < 100 ? 2000 + year : year;

		// Split time (hh:mm) and validate
		const [hours, minutes] = timePart.split(':').map(Number);
		if (!Number.isInteger(hours) || !Number.isInteger(minutes)) {
			throw new Error('Invalid time format. Expected hh:mm');
		}

		// Create Date object (month is 0-based in JS, so subtract 1)
		const date = new Date(fullYear, month - 1, day, hours, minutes);

		// Validate Date object
		if (isNaN(date.getTime())) {
			throw new Error('Invalid date or time');
		}
		console.log(date);
		console.log(date.toISOString());
		// Return ISO 8601 format with UTC (Z)
		return date.toISOString();
	}
});