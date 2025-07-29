

// #region 🟣 Get Employee Avatar HTML (Initial or Image)
function getAvatarHtml(employee) {
    if (employee.employeeImage && employee.employeeImage !== '') {
        return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
    } else {
        const initial = employee.employeeName.charAt(0).toUpperCase();
        return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
    }
}
// #endregion


$(document).ready(function () {
    $.ajax({
        url: '/Notifications/GetAllNotificationsAsync',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
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
});
