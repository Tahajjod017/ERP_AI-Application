//#region and ids
let customers = [];

var actions = {
    create: "/CreateJobs/Upsert",
    delete : "",
    getLists : "",
    getItemData: ""
}

var ids = {
    formID: "#CreateJob-form",
    submitBtn: "#submitBtn",
    resetBtn: "#resetBtn",
}
//#endregion


$(function () {
    $('#CustomerID').select2({
        placeholder: 'Select Customer',
        width: '100%',
        dropdownParent: $('#CustomerID').closest('.modal').length ? $('#CustomerID').closest('.modal') : null,
        ajax: {
            url: '/CreateJobs/GetCustomers',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    search: params.term || '',
                    page: params.page || 1
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    
                    results: data.results,
                    pagination: {
                        more: data.pagination.more
                    }
                };
            },
            cache: true
        },
        width: '100%'
    });



    function resetForm() {
        // Reset all form fields
        $("#CreateJob-form")[0].reset();

        // Clear CoreUI MultiSelect
        const teamSelect = document.querySelector('[asp-for="TeamMembers"]');
        if (teamSelect) {
            const ms = coreui.MultiSelect.getInstance(teamSelect);
            if (ms && typeof ms.clear === "function") {
                ms.clear();
            }
        }

        // Clear Dropzone files (if initialized)
        if (window.Dropzone && Dropzone.instances) {
            Dropzone.instances.forEach(dz => dz.removeAllFiles(true));
        }

        // Reset Select2 dropdowns
        $("select").each(function () {
            if ($(this).hasClass("select2-hidden-accessible")) {
                $(this).val(null).trigger("change");
            }
        });
    }

    // Bind reset button click
    $(document).on("click", "#resetBtn", resetForm);


    // Bind reset button
$("#resetBtn").on("click", resetForm);





    //#region submit function
    $(ids.submitBtn).on("click", function (e) {
        e.preventDefault();
        try {
            const formData = new FormData();
            debugger
            formData.append("CreateJobID", $("#CreateJobID").val());
            formData.append("CustomerID", $("#CustomerID").val());
            formData.append("JobTitle", $("#JobTitle").val());
            formData.append("JobID", $("#JobID").val());
            formData.append("StartDate", $("#StartDate").val());
            formData.append("EndDate", $("#EndDate").val());
            formData.append("StatusID", $("#StatusID").val());
            formData.append("JobLocation", $("#JobLocation").val());
            formData.append("Note", $("#Note").val());

            const teamMembers = $("#TeamMembers").val();
            if (teamMembers && teamMembers.length > 0) {
                teamMembers.forEach(id => formData.append("TeamMembers", id))
            }


            $.ajax({
                url: actions.create,
                type: "POST",
                data: formData,
                processData: false, // Important for FormData
                contentType: false, // Important for FormData
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (data) {
                    resetForm();
                    if (data.success) {
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message || "Something went wrong");
                    }
                },
                error: function (xhr) {
                    toastr.error("Error generating PDF");
                    console.error(xhr);
                }
            });
        } catch (ex) {
            toastr.error(ex);
        }
        
    });
    //#endregion

    //#region load team member
    let page = 1;
    let term = '';
    const pageSize3 = 20;

    let hasMore = true;
    let loading = false;
    let debounce;
    let scrollPosition = 0;

    const selectEl = document.getElementById('TeamMembers');
    if (!selectEl) return;

    const apiUrl = '/CreateJobs/GetTechnicianList';
    const ms = coreui.MultiSelect.getOrCreateInstance(selectEl);
    function ensureSearchHandler() {
        const wrapper = selectEl.nextElementSibling;
        if (!wrapper) return;
        const searchInput = wrapper.querySelector('.form-multi-select-search');
        const box = wrapper.querySelector('.form-multi-select-options');
        if (!searchInput) return;
        if (searchInput.dataset.listenerAttached) return;

        searchInput.dataset.listenerAttached = '1';

        searchInput.addEventListener('mousedown', (e) => e.stopPropagation());
        searchInput.addEventListener('input', (e) => {
            const val = e.target.value.trim();
            clearTimeout(debounce);

            if (val.length < 3) {
                addOptions([], { reset: true });
                term = val;
                page = 1;
                term = '';
                hasMore = false;
                if (box) box.scrollTop = 0;
                return;
            }

            debounce = setTimeout(() => {
                term = val;
                page = 1;
                hasMore = true;
                fetchPage({ append: false });
                if (box) box.scrollTop = 0;
            }, 1000);
        });
    }

    function addOptions(items, { reset = false } = {}) {
        const wrapper = selectEl.nextElementSibling;
        const box = wrapper?.querySelector('.form-multi-select-options');
        if (box) {
            scrollPosition = box.scrollTop;
        }

        if (reset) {
            const keep = new Set([...selectEl.options].filter(o => o.selected).map(o => o.value));
            [...selectEl.options].forEach(o => { if (!keep.has(o.value)) o.remove(); });
        }
        const existing = new Set([...selectEl.options].map(o => String(o.value)));
        for (const it of (items || [])) {
            const v = String(it.value);
            if (existing.has(v)) continue;
            const opt = document.createElement('option');
            opt.value = v;
            opt.textContent = it.label;
            selectEl.appendChild(opt);
        }
        const wrapperBefore = selectEl.nextElementSibling;
        const oldSearchInput = wrapperBefore?.querySelector('.form-multi-select-search');
        const oldSearchValue = oldSearchInput ? oldSearchInput.value : '';
        const oldSelStart = oldSearchInput?.selectionStart;
        const oldSelEnd = oldSearchInput?.selectionEnd;

        const wasOpen = !!ms._isShown;
        ms.update();

        if (wasOpen) {
            ms.show();
        }

        ensureSearchHandler();

        const wrapperAfter = selectEl.nextElementSibling;
        const newSearchInput = wrapperAfter?.querySelector('.form-multi-select-search');
        if (newSearchInput && oldSearchValue) {
            try {
                newSearchInput.value = oldSearchValue;
                if (typeof oldSelStart === 'number' && typeof oldSelEnd === 'number') {
                    newSearchInput.setSelectionRange(oldSelStart, oldSelEnd);
                }
            } catch (err) {
            }
        }
        setTimeout(() => {
            const wrapper = selectEl.nextElementSibling;
            const box = wrapper?.querySelector('.form-multi-select-options');
            if (box) {
                if (reset) {
                    box.scrollTop = 0;
                    scrollPosition = 0;
                } else {
                    box.scrollTop = scrollPosition;
                }
            }
        }, 10);

        rebindScroll();
    }

    async function fetchPage({ append }) {
        if (loading || (!hasMore && append)) return;
        loading = true;
        try {
            const res = await fetch(`${apiUrl}?search=${encodeURIComponent(term)}&page=${page}&pageSize=${pageSize3}`);
            const data = await res.json();

            addOptions(data.items, { reset: !append });
            hasMore = !!data.hasMore;

            if (append) {
                page += 1;
            } else {
                page = 2;
            }
        } catch (e) {
            console.error(e);
        } finally {
            loading = false;
        }
    }

    function rebindScroll() {
        const wrapper = selectEl.nextElementSibling;
        const box = wrapper?.querySelector('.form-multi-select-options');
        if (!box) return;

        if (box.dataset.infiniteAttached) return;
        box.dataset.infiniteAttached = '1';

        box.addEventListener('scroll', () => {
            if (box.scrollTop + box.clientHeight >= box.scrollHeight - 10) {
                if (hasMore && !loading) fetchPage({ append: true });
            }
        });
    }

    selectEl.addEventListener('shown.coreui.multi-select', () => {
        const wrapper = selectEl.nextElementSibling;
        const searchInput = wrapper?.querySelector('.form-multi-select-search');
        const box = wrapper?.querySelector('.form-multi-select-options');

        ensureSearchHandler();

        if (selectEl.options.length === 0) {
            page = 1; term = ''; hasMore = true;
            fetchPage({ append: false });
        }

        rebindScroll();
    });
    //#endregion

        
    //#region initialize file field
    // MUST be once globally
Dropzone.autoDiscover = false;

let jobDropzone = null;

function initJobDropzone() {

    const dzElement = document.getElementById('jobDropzone');
    if (!dzElement) return;

    // Destroy if already exists (modal reopened)
    if (jobDropzone) {
        jobDropzone.destroy();
        jobDropzone = null;
    }

    const previewTemplate =
        dzElement.querySelector('.dz-preview').innerHTML;

    // Clear template from DOM to avoid duplication
    dzElement.querySelector('.dz-preview').remove();

    jobDropzone = new Dropzone(dzElement, {
        url: '/CreateJobs/Upload', // 🔁 change to your API
        paramName: 'Files',
        maxFiles: 5,
        maxFilesize: 10, // MB
        acceptedFiles: '.jpg,.png,.pdf,.doc,.docx',
        addRemoveLinks: false,
        autoProcessQueue: false,
        previewTemplate: previewTemplate
    });
}

    //#endregion
    initJobDropzone()
});
