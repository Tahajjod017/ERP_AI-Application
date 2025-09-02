$(function () {

	// active item buttons
	$(".option-btn").on('click', function () {
		$(".option-btn").removeClass('active');
		$(this).addClass('active');
		let btnText = $(this).text().trim();
		if (btnText === "Attachment") {
			console.log("attachment");
			$('#file-field').css('display', 'block');
		} else {
			$('#file-field').css('display', 'none');
		}
			console.log($(this).data('id'));

		console.log($(this).text().trim());
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
				$('#allActivity').empty();
				toastr.success(response.message);
				currentPage = 1;
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
	//let currentPage = 1;
	//let loading = true;
	//function updateActivate() {
	//	debugger;
	//	let search = $("#searchActivity").val();
	//	let id = $("#leadID").val();
	//	showDev(search);

	//	loading = true;

	//	$.ajax({
	//		url: '/LeadDetails/getActivityList',
	//		method: 'GET',
	//		contentType: 'application/json',
	//		data: { id: id, query: search, page: currentPage },
	//		success: function (response) {
	//			debugger
	//			$('#activity').empty();
	//			response.forEach((value, index) => {
	//				const activityDate = new Date(value.activityDateTime).toLocaleString('en-GB', options);
	//				$('#activity').append(`<div class="border-bottom border-translucent py-4">
	//			<div class="d-flex">
	//				<div class="d-flex bg-primary-subtle rounded-circle flex-center me-3 bg-primary-subtle" style="width:25px; height:25px"><span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon} text-primary-dark"></span></div>
	//				<div class="flex-1">
	//					<div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
	//						<div class="flex-1 me-2">
	//							<h5 class="text-body-highlight lh-sm">${value.leadActivityName}</h5>
	//							<p class="fs-9 mb-0">by<a class="ms-1" href="#!">${value.createdByName}</a></p>
	//						</div>
	//					<div class="fs-9"><span class="fa-regular fa-calendar-days text-primary me-2"></span><span class="fw-semibold">${activityDate}</span></div>
	//				</div>
	//				<p class="fs-9 mb-0">${value.activityNote}</p>
	//			</div>
	//		</div>
	//	</div>`)
	//			})
	//		},
	//		complete: function () { loading =  false },
	//		error: function (jqXHR, textStatus, errorThrown) {
	//			console.log(jqXHR, textStatus, errorThrown);
	//			toastr.error("An error occurred: " + textStatus);
	//		}
	//	});
	//}
	//let currentPage = 1;
	//let loading = false;
	//let lastSearch = "";

	//$('#activity').on('scroll', function () {
	//	const container = $(this);

	//	// Scroll Down (near bottom)
	//	if (!loading && container.scrollTop() + container.innerHeight() >= container[0].scrollHeight - 10) {
	//		currentPage++;
	//		updateActivate(currentPage, "down");
	//	}

	//	// Scroll Up (near top)
	//	if (!loading && container.scrollTop() <= 10 && currentPage > 1) {
	//		currentPage--;
	//		updateActivate(currentPage, "up");
	//	}
	//});

	//// Search handler
	//$("#searchActivity").on("input", function () {
	//	let search = $(this).val().trim();
	//	if (search !== lastSearch) {
	//		lastSearch = search;
	//		currentPage = 1;
	//		$('#activity').empty();
	//		updateActivate(currentPage, "reset");
	//	}
	//});

	//function updateActivate(page, direction = "down") {
	//	let search = $("#searchActivity").val();
	//	let id = $("#leadID").val();

	//	loading = true;

	//	$.ajax({
	//		url: '/LeadDetails/getActivityList',
	//		method: 'GET',
	//		contentType: 'application/json',
	//		data: { id: id, query: search, page: page },
	//		success: function (response) {
	//			if (direction === "reset") {
	//				// ?? Fresh search ? clear
	//				$('#activity').empty();
	//			}

	//			if (direction === "up") {
	//				// ?? Prepend (older records)
	//				response.reverse().forEach((value) => {
	//					const activityDate = new Date(value.activityDateTime).toLocaleString('en-GB', options);
	//					$('#activity').prepend(renderActivity(value, activityDate));
	//				});

	//				// ?? Keep scroll position after prepend
	//				const container = $('#activity');
	//				container.scrollTop(container[0].scrollHeight / 2);

	//			} else {
	//				// ?? Append (newer records, or reset)
	//				response.forEach((value) => {
	//					const activityDate = new Date(value.activityDateTime).toLocaleString('en-GB', options);
	//					$('#activity').append(renderActivity(value, activityDate));
	//				});
	//			}
	//		},
	//		complete: function () { loading = false; },
	//		error: function (jqXHR, textStatus, errorThrown) {
	//			console.log(jqXHR, textStatus, errorThrown);
	//			toastr.error("An error occurred: " + textStatus);
	//		}
	//	});
	//}


	let currentPage = 1;
	let loading = false;
	let lastSearch = "";
	let noMoreDataDown = false;
	let noMoreDataUp = false;

	$('#activity').on('scroll', function () {
		const container = $(this);

		// Scroll Down (near bottom)
		if (!loading && !noMoreDataDown && container.scrollTop() + container.innerHeight() >= container[0].scrollHeight - 10) {
			currentPage++;
			updateActivate(currentPage, "down");
		}

		// Scroll Up (near top)
		if (!loading && !noMoreDataUp && container.scrollTop() <= 10 && currentPage > 1) {
			currentPage--;
			updateActivate(currentPage, "up");
		}
	});

	// Search handler
	$("#searchActivity").on("input", function () {
		let search = $(this).val().trim();
		if (search !== lastSearch) {
			lastSearch = search;
			currentPage = 1;
			noMoreDataDown = false;
			noMoreDataUp = false;
			$('#activity').empty();
			updateActivate(currentPage, "reset");
		}
	});

	function updateActivate(page, direction = "down") {
		let search = $("#searchActivity").val();
		let id = $("#leadID").val();

		loading = true;

		$.ajax({
			url: '/LeadDetails/getActivityList',
			method: 'GET',
			contentType: 'application/json',
			data: { id: id, query: search, page: page },
			success: function (response) {
				if (!response || response.length === 0) {
					if (direction === "down") noMoreDataDown = true;
					if (direction === "up") noMoreDataUp = true;
					return; // ? stop duplicates
				}

				if (direction === "reset") {
					$('#activity').empty();
					noMoreDataDown = false;
					noMoreDataUp = false;
				}

				if (direction === "up") {
					response.reverse().forEach((value) => {
						const activityDate = new Date(value.activityDateTime).toLocaleString('en-GB', options);
						$('#activity').prepend(renderActivity(value, activityDate));
					});

					const container = $('#activity');
					container.scrollTop(50); // keep view stable after prepend

				} else {
					response.forEach((value) => {
						const activityDate = new Date(value.activityDateTime).toLocaleString('en-GB', options);
						$('#activity').append(renderActivity(value, activityDate));
					});
				}
			},
			complete: function () { loading = false; },
			error: function (jqXHR, textStatus, errorThrown) {
				console.log(jqXHR, textStatus, errorThrown);
				toastr.error("An error occurred: " + textStatus);
			}
		});
	}

	function renderActivity(value, activityDate) {
		return `
<div class="border-bottom border-translucent py-4">
    <div class="d-flex">
        <div class="d-flex bg-primary-subtle rounded-circle flex-center me-3" style="width:25px; height:25px">
            <span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon}"></span>
        </div>
        <div class="flex-1">
            <div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
                <div class="flex-1 me-2">
                    <h5 class="text-body-highlight lh-sm">${value.leadActivityName}</h5>
                    <p class="fs-9 mb-0">by<a class="ms-1" href="#!">${value.createdByName}</a></p>
                </div>
                <div class="fs-9">
                    <span class="fa-regular fa-calendar-days text-primary me-2"></span>
                    <span class="fw-semibold">${activityDate}</span>
                </div>
            </div>
            <p class="fs-9 mb-0">${value.activityNote}</p>
        </div>
    </div>
</div>`;
	}


	//// Helper: render activity card
	//function renderActivity(value, activityDate) {
	//	return `
 //   <div class="border-bottom border-translucent py-4">
 //       <div class="d-flex">
 //           <div class="d-flex bg-primary-subtle rounded-circle flex-center me-3" style="width:25px; height:25px">
 //               <span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon}"></span>
 //           </div>
 //           <div class="flex-1">
 //               <div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
 //                   <div class="flex-1 me-2">
 //                       <h5 class="text-body-highlight lh-sm">${value.leadActivityName}</h5>
 //                       <p class="fs-9 mb-0">by<a class="ms-1" href="#!">${value.createdByName}</a></p>
 //                   </div>
 //                   <div class="fs-9">
 //                       <span class="fa-regular fa-calendar-days text-primary me-2"></span>
 //                       <span class="fw-semibold">${activityDate}</span>
 //                   </div>
 //               </div>
 //               <p class="fs-9 mb-0">${value.activityNote}</p>
 //           </div>
 //       </div>
 //   </div>`;
	//}


	//$("#activity-tab").on('click', function () {
	//	console.log("clicked");
	//	debugger
	//	updateActivate();
	//});
	$("#activity-tab").on('click', function () {
		console.log("clicked");
		debugger
		updateActivate(1);
	});


	updateActivate(1);
	let typingTimer;
	let delay = 300;

	//$('#searchActivity').on("input", function () {
	//	currentPage = 1;
	//	clearTimeout(typingTimer);
	//	typingTimer = setTimeout(function () {
	//		updateActivate();
	//	}, delay)

	//});

	
	//$('#activity').on('scroll', function () {
	//	const container = $(this);
	//	if (!loading && container.scrollTop() + container.innerHeight() >= container[0].scrollHeight - 10) {
	//		currentPage++;
	//		updateActivate(currentPage, function () {
	//			loading = false;
	//		});
	//	}
	//	if (!loading && container.scrollTop() <= 10 && currentPage > 1) {
	//		if (currentPage > 1) {   
	//			currentPage--;
	//			console.log(currentPage);
	//			updateActivate(currentPage, true);
	//		}
	//		updateActivate(currentPage, function () {
	//			loading = false;
	//		});
	//	}
	//});


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