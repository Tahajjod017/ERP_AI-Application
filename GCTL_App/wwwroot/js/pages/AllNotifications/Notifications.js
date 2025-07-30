

$(document).ready(function () {
    loadNotifications();
});

// Load notifications once and render both views
function loadNotifications() {
    $.ajax({
        url: '/Notifications/GetAllNotificationsAsync',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {

                renderNotificationCards(response.data);
                renderNotificationDetails(response.data);

                const container = $('#notificationContainer');
                container.empty(); // Clear any existing notifications
                response.data.forEach(notification => {
                    const cardHtml = `
                        <div class="px-2 px-sm-3 py-3 notification-card position-relative ${notification.isChecked ? 'read' : 'unread'} border-bottom">
                            <div class="d-flex align-items-center justify-content-between position-relative">
                                <div class="d-flex">
                                    <div class="avatar avatar-m status-online me-3">
                                        ${notification.employeeImage
                            ? `<img class="rounded-circle" src="${notification.employeeImage}" alt="" />`
                            : `<div class="avatar-name rounded-circle"><span>${notification.employeeName?.charAt(0) || '?'}</span></div>`}
                                    </div>
                                    <div class="flex-1 me-sm-3">
                                        <h4 class="fs-9 text-body-emphasis">${notification.employeeName || 'Unknown'}</h4>
                                        <p class="fs-9 text-body-highlight mb-2 mb-sm-3 fw-normal">
                                            <span class='me-1 fs-10'>🔔</span>New notification
                                            <span class="ms-2 text-body-quaternary text-opacity-75 fw-bold fs-10">
                                                ${notification.createdAt ? moment(notification.createdAt).fromNow() : ''}
                                            </span>
                                        </p>
                                        <p class="text-body-secondary fs-9 mb-0">
                                            <span class="me-1 fas fa-clock"></span>
                                            <span class="fw-bold">${notification.createdAt ? moment(notification.createdAt).format('hh:mm A') : ''} </span>
                                            ${notification.createdAt ? moment(notification.createdAt).format('MMMM D, YYYY') : ''}
                                        </p>
                                    </div>
                                </div>
                                <div class="dropdown notification-dropdown">
                                    <button class="btn fs-10 btn-sm dropdown-toggle dropdown-caret-none transition-none" type="button" data-bs-toggle="dropdown" data-boundary="window" aria-haspopup="true" aria-expanded="false" data-bs-reference="parent">
                                        <span class="fas fa-ellipsis-h fs-10 text-body"></span>
                                    </button>
                                    <div class="dropdown-menu py-2">
                                        <a class="dropdown-item" href="#!">Mark as unread</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    container.append(cardHtml);
                });

            } else {
                console.error("Error fetching notifications:", response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX request failed:", error);
        }
    });
}

function getAvatarHtml(employee) {
    if (employee.employeeImage && employee.employeeImage !== '') {
        return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
    } else {
        const initial = employee.employeeName?.charAt(0).toUpperCase() || '?';
        return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
    }
}

// Render notification cards (e.g., in header dropdown)
function renderNotificationCards(notifications) {
    const container = $('#notificationContainer');
    if (!container.length) return;
    container.empty();

    notifications.forEach(notification => {
        const html = `
            <div class="px-2 px-sm-3 py-3 notification-card position-relative ${notification.isChecked ? 'read' : 'unread'} border-bottom">
                <div class="d-flex align-items-center justify-content-between">
                    <div class="d-flex">
                        <div class="avatar avatar-m me-3">${getAvatarHtml(notification)}</div>
                        <div class="flex-1 me-sm-3">
                            <h4 class="fs-9 text-body-emphasis">${notification.employeeName || 'Unknown'}</h4>
                            <p class="fs-9 text-body-highlight mb-2 mb-sm-3 fw-normal">
                               <span class='me-1 fs-10'><b>Title</b></span>${notification.alertTitle}<br/>
                                <span class='me-1 fs-10'><b>Note</b></span>${notification.alertNote}
                                <span class="ms-2 text-body-quaternary text-opacity-75 fw-bold fs-10">${moment(notification.createdAt).fromNow()}</span>
                            </p>
                            <p class="text-body-secondary fs-9 mb-0">
                                <i class="fas fa-clock me-1"></i>
                                <span class="fw-bold">${moment(notification.createdAt).format('hh:mm A')}</span>
                                ${moment(notification.createdAt).format('MMMM D, YYYY')}
                            </p>
                        </div>
                    </div>
                 <div class="dropdown notification-dropdown">
    <button class="btn fs-10 btn-sm dropdown-toggle dropdown-caret-none transition-none notification-dropdown-toggle"
            type="button" data-bs-toggle="dropdown" data-boundary="window" aria-haspopup="true"
            aria-expanded="false" data-bs-reference="parent">
        <i style="font-size:18px" class="fa">&#xf141;</i>
    </button>

    <div class="dropdown-menu dropdown-menu-end py-2">
        <!-- Only show if not already marked as checked -->
        ${notification.isChecked === false ? `
            <a data-id="${notification.alertForEmployeeID}" class="dropdown-item isCheckedBtn" href="#!">Mark as Read</a>
        ` : `
            <span class="dropdown-item text-muted">Already Read</span>
        `}
    </div>
          </div>

                </div>
            </div>`;
        container.append(html);
    });
}

// Render detailed grouped notifications (Today, Yesterday, Earlier)
function renderNotificationDetails(notifications) {
    const container = $('#notificationContainerDetailsPage');
    if (!container.length) return;
    container.empty();

    const grouped = {
        Today: [],
        Yesterday: [],
        Earlier: []
    };

    const today = moment().startOf('day');
    const yesterday = moment().subtract(1, 'days').startOf('day');

    notifications.forEach(n => {
        const date = moment(n.createdAt);
        if (date.isSame(today, 'day')) grouped.Today.push(n);
        else if (date.isSame(yesterday, 'day')) grouped.Yesterday.push(n);
        else grouped.Earlier.push(n);
    });

    const buildGroupHtml = (title, items) => {
        if (items.length === 0) return '';
        let html = `<h5 class="text-body-emphasis mb-3">${title}</h5><div class="mb-4 border-bottom">`;

        items.forEach(notification => {
            const avatar = getAvatarHtml(notification);
            const time = moment(notification.createdAt).format('hh:mm A');
            const date = moment(notification.createdAt).format('MMMM D, YYYY');
            const relativeTime = moment(notification.createdAt).fromNow(true);

            html += `
    <div class="d-flex align-items-center justify-content-between py-3 px-3 notification-card border-top ${notification.isChecked ? 'read' : 'unread'}">
    <div class="d-flex">
        <div class="avatar avatar-xl me-3">${avatar}</div>
        <div class="flex-1">
            <h4 class="fs-9 text-body-emphasis">${notification.employeeName}</h4>
            <p class="fs-9 text-body-highlight">
                <span class='me-1 fs-10'><b>Title</b></span>${notification.alertTitle}<br />
                <span class='me-1 fs-10'><b>Note</b></span>${notification.alertNote}
                <span class="ms-2 text-body-tertiary fw-bold fs-10">${relativeTime}</span>
            </p>
            <p class="text-body-secondary fs-9 mb-0">
                <i class="fas fa-clock me-1"></i>
                <span class="fw-bold">${time}</span> ${date}
            </p>
        </div>
    </div>
    <div class="dropdown notification-dropdown">
        <button class="btn fs-10 btn-sm dropdown-toggle dropdown-caret-none transition-none notification-dropdown-toggle"
                type="button" data-bs-toggle="dropdown" data-boundary="window" aria-haspopup="true"
                aria-expanded="false" data-bs-reference="parent">
            <i style="font-size:18px" class="fa">&#xf141;</i>
        </button>

        <div class="dropdown-menu dropdown-menu-end py-2">
            <!-- Only show if not already marked as checked -->
            ${notification.isChecked === false ? `
            <a data-id="${notification.alertForEmployeeID}" class="dropdown-item isCheckedBtn" href="#!">Mark as Read</a>
            ` : `
            <span class="dropdown-item text-muted">Already Read</span>
            `}
        </div>
    </div>

</div>
`;
        });

        html += `</div>`;
        return html;
    };

    let finalHtml = `<h2 class="mb-3">Notifications</h2>`;
    finalHtml += buildGroupHtml('Today', grouped.Today);
    finalHtml += buildGroupHtml('Yesterday', grouped.Yesterday);
    finalHtml += buildGroupHtml('Earlier', grouped.Earlier);

    container.html(finalHtml);
}


//

//#region Updated

$(document).off('click', '.isCheckedBtn, .isCheckedBtnCardsmall').on('click', '.isCheckedBtn, .isCheckedBtnCardsmall', function (e) {
    e.preventDefault();
    const alertId = $(this).data('id');
    const data = {
        AlertForEmployeeID: alertId,
        IsChecked: true
    };

    $.ajax({
        url: '/Notifications/IsCheckedUpdated',
        type: 'POST',
        data: data,
        success: function (response) {
            toastr[response.success ? 'success' : 'error'](response.message);
            loadNotifications();
        },
        error: function () {
            console.error('Failed to update checked status');
        }
    });
});
//#endregion

