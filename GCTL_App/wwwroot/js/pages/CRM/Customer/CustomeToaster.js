// customToaster.js
(() => {
    // ===== Inject CSS =====
    const style = document.createElement("style");
    style.textContent = `
    /* ====== Toaster ====== */
    #customToaster {
        position: fixed;
        top: 20px;
        left: 50%;
        transform: translateX(-50%) translateY(-50px);
        min-width: 280px;
        max-width: 350px;
        background: #fff;
        color: #333;
        font-weight: 600;
        font-size: 18px;
        padding: 20px;
        border-radius: 12px;
        box-shadow: 0 8px 25px rgba(0,0,0,0.15);
        text-align: center;
        opacity: 0;
        pointer-events: none;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 12px;
        transition: opacity 0.4s ease, transform 0.4s ease;
        z-index: 9999;
    }
    #customToaster.show-toast {
        opacity: 1;
        transform: translateX(-50%) translateY(0);
    }
    #customToaster.hide-toast {
        opacity: 0;
        transform: translateX(-50%) translateY(-50px);
    }
    #customToaster img.toast-icon {
        width: 60px;
        height: 60px;
    }
    #customToaster .spinner {
        border: 6px solid #f3f3f3;
        border-top: 6px solid #2196f3;
        border-radius: 50%;
        width: 50px;
        height: 50px;
        animation: spin 1s linear infinite;
    }
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
    /* ====== Confirmation Modal ====== */
    #customConfirmModal {
        position: fixed;
        top:0;
        left:0;
        width:100%;
        height:100%;
        background: rgba(0,0,0,0.6);
        display: none;
        justify-content: center;
        align-items: center;
        z-index:10000;
    }
    #customConfirmModal.active { display:flex; }
    #customConfirmModal .modal-content {
        background:#fff;
        padding:30px 25px;
        border-radius:16px;
        box-shadow:0 10px 30px rgba(0,0,0,0.25);
        text-align:center;
        max-width:400px;
        width:90%;
        display:flex;
        align-items:center;
        flex-direction: column;
    }
    #customConfirmModal .confirm-icon { width:60px; height:60px; display:inline-block; margin-bottom:15px; }
    #customConfirmModal .confirm-icon.spin { animation: spinIcon 1.5s linear infinite; }
    @keyframes spinIcon { 0% {transform: rotate(0deg);} 100% {transform: rotate(360deg);} }
    #customConfirmModal .confirm-title { font-size:22px; font-weight:700; margin-bottom:10px; color:#333; }
    #customConfirmModal .confirm-message { font-size:16px; color:#555; }
    #customConfirmModal .confirm-actions { margin-top:20px; display:flex; justify-content:center; gap:20px; }
    #customConfirmModal .confirm-btn { padding:12px 25px; border:none; border-radius:8px; cursor:pointer; font-size:16px; color:#fff; transition: transform 0.2s ease, opacity 0.2s ease; }
    #customConfirmModal .confirm-btn:hover { transform: translateY(-2px); opacity:0.9; }
    #customConfirmModal .confirm-btn-yes { background-color:#4caf50; }
    #customConfirmModal .confirm-btn-no { background-color:#f44336; }
    `;
    document.head.appendChild(style);

    // ===== Inject HTML =====
    const html = `
    <div id="customToaster" class="hide-toast"></div>
    <div id="customConfirmModal">
        <div class="modal-content">
            <img id="modalIcon" class="confirm-icon" alt="Icon">
            <h2 class="confirm-title">Confirmation</h2>
            <p id="confirmMessage" class="confirm-message">Message</p>
            <div class="confirm-actions">
                <button id="confirmYes" class="confirm-btn confirm-btn-yes">Yes</button>
                <button id="confirmNo" class="confirm-btn confirm-btn-no">No</button>
            </div>
        </div>
    </div>`;
    document.body.insertAdjacentHTML("beforeend", html);

    // ===== JS Logic =====
    const customToaster = (() => {
        debugger;
        const durationDefault = 3000;
        const toaster = document.getElementById("customToaster");
        const modal = document.getElementById("customConfirmModal");
        const modalIcon = document.getElementById("modalIcon");
        const confirmMessage = document.getElementById("confirmMessage");
        const btnYes = document.getElementById("confirmYes");
        const btnNo = document.getElementById("confirmNo");

        function show(message, type = "success", duration = durationDefault) {
            if (!toaster) return;
            let iconHTML = "";
            switch (type) {
                case "success": iconHTML = `<img src="/media/costomTosterImg/tickmark.png" class="toast-icon" alt="success">`; break;
                case "error": iconHTML = `<img src="/media/costomTosterImg/cross.png" class="toast-icon" alt="error">`; break;
                case "loading": iconHTML = `<div class="spinner"></div>`; break;
                case "info": iconHTML = `<img src="/media/costomTosterImg/star.png" class="toast-icon" alt="info">`; break;
                case "warning": iconHTML = `<img src="/media/costomTosterImg/circle.png" class="toast-icon spin" alt="warning">`; break;
                case "alert": iconHTML = `<img src="/media/costomTosterImg/alarm.png" class="toast-icon" alt="alert">`; break;
            }
            toaster.innerHTML = `${iconHTML}<span>${message}</span>`;
            toaster.classList.remove("hide-toast");
            toaster.classList.add("show-toast");
            setTimeout(() => { toaster.classList.remove("show-toast"); toaster.classList.add("hide-toast"); }, duration);
        }

        async function confirm(message) {
            return new Promise(resolve => {
                confirmMessage.textContent = message;
                modalIcon.classList.remove("spin");
                modalIcon.src = "/media/costomTosterImg/circle.png";
                modalIcon.classList.add("spin");
                modal.classList.add("active");

                const cleanup = () => {
                    modal.classList.remove("active");
                    btnYes.removeEventListener("click", onYes);
                    btnNo.removeEventListener("click", onNo);
                }

                const onYes = () => { cleanup(); resolve(true); }
                const onNo = () => { cleanup(); resolve(false); }

                btnYes.addEventListener("click", onYes);
                btnNo.addEventListener("click", onNo);
            });
        }

        return {
            success: (msg) => show(msg, "success"),
            error: (msg) => show(msg, "error"),
            loading: (msg, dur) => show(msg, "loading", dur),
            info: (msg, dur) => show(msg, "info", dur),
            warning: (msg, dur) => show(msg, "warning", dur),
            alert: (msg, dur) => show(msg, "alert", dur),
            confirm
        }
    })();

    // Expose globally
    window.customToaster = customToaster;
})();
