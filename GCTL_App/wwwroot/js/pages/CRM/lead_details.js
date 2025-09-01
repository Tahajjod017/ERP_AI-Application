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
		let buttonID = $(".option-btn.active").data('id');
		let date = $("#lDetailsDate").val();
		let text = $("#lDetailsText").val();
		let id = $("#leadID").val();
		let convertedDate = convertToISODateTime(date);

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
				updateActivate();
			},
			error: function (error) {
				toastr.error(error.message);
			}
		});
	});

	const options = {
		day: '2-digit',
		month: 'short',
		year: 'numeric',
		hour: '2-digit',
		minute: '2-digit',
		hour12: true
	};
	let currentPage = 1;
	function updateActivate() {
		let search = $("#searchActivity").val();
		let id = $("#leadID").val();
		$.ajax({
			url: '/LeadDetails/getActivityList',
			method: 'GET',
			contentType: 'application/json',
			data: { id: id, query: search, page: 1 },
			success: function (response) {
				debugger
				$('#allActivity').empty();
				response.forEach((value, index) => {
					const activityDate = new Date(value.activityDateTime).toLocaleString('en-GB', options);
					$('#allActivity').append(`<div class="border-bottom border-translucent py-4">
				<div class="d-flex">
					<div class="d-flex bg-primary-subtle rounded-circle flex-center me-3 bg-primary-subtle" style="width:25px; height:25px"><span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon} text-primary-dark"></span></div>
					<div class="flex-1">
						<div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
							<div class="flex-1 me-2">
								<h5 class="text-body-highlight lh-sm">Assigned as a director for Project The Chewing Gum Attack</h5>
								<p class="fs-9 mb-0">by<a class="ms-1" href="#!">Jackson Pollock</a></p>
							</div>
						<div class="fs-9"><span class="fa-regular fa-calendar-days text-primary me-2"></span><span class="fw-semibold">${activityDate}</span></div>
					</div>
					<p class="fs-9 mb-0">${value.activityNote}</p>
				</div>
			</div>
		</div>`)
				})
			},
			error: function (jqXHR, textStatus, errorThrown) {
				console.log(jqXHR, textStatus, errorThrown);
				toastr.error("An error occurred: " + textStatus);
			}
		});
	}
	$("#activity-tab").on('click', function () {
		console.log("clicked");
		debugger
		updateActivate();
	});
	$("#activity-tab").on('click', function () {
		console.log("clicked");
		debugger
		updateActivate();
	});
	updateActivate();

	$('#searchActivity').on("input", function () {  
		updateActivate();

	});

	$('#activity-tab').on('scroll', function () {
		const container = $(this);
		if (!loading && container.scrollTop() + container.innerHeight() >= container[0].scrollHeight - 10) {
			currentPage++;
			loadActivities(currentPage);
		}
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
		if (!Number.isInteger(day) || !Number.isInteger(month) || !Number.isInteger(year)) {
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

		// Format to ISO 8601 with local timezone offset
		const pad = (num) => String(num).padStart(2, '0');
		const yearStr = date.getFullYear();
		const monthStr = pad(date.getMonth() + 1);
		const dayStr = pad(date.getDate());
		const hoursStr = pad(date.getHours());
		const minutesStr = pad(date.getMinutes());
		const secondsStr = pad(date.getSeconds());
		const offsetMinutes = date.getTimezoneOffset();
		const offsetSign = offsetMinutes > 0 ? '-' : '+';
		const offsetHours = pad(Math.floor(Math.abs(offsetMinutes) / 60));
		const offsetMins = pad(Math.abs(offsetMinutes) % 60);
		const offset = `${offsetSign}${offsetHours}:${offsetMins}`;

		return `${yearStr}-${monthStr}-${dayStr}T${hoursStr}:${minutesStr}:${secondsStr}${offset}`;
	}
});