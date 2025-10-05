$(function () {

    let ids = {
        generatedID: '#GeneratedID',
        submitBtn: '#CreatTeamBtn',
        employeeDrpdn: '#EmployeeIds',
        teamName: '#TeamName',
        employeeIDs: '#EmployeeIds',
        resultShowDiv: '#teamsDiv',
    }


    async function getNextIndex() {
        try {
            const response = await fetch('/AddTeams/GetLastIndexNumber');
            if (!response.ok) throw new Error('Network response was not ok');

            const nextIndex = await response.json();

            return nextIndex;
        } catch (error) {
            console.error("Error fetching next index: ", error);
        }
    }

    getNextIndex().then(index => {
        $(ids.generatedID).val(index);
    });


    // ============================
    // load employee
    // ============================

    let page = 1;
    let term = '';
    const pageSize = 20;

    let hasMore = true;
    let loading = false;
    let debounce;
    let scrollPosition = 0;

    const selectEl = document.getElementById('EmployeeIds');
    if (!selectEl) return;

    const apiUrl = '/AddTeams/GetEmployeeList';
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
            const res = await fetch(`${apiUrl}?search=${encodeURIComponent(term)}&page=${page}&pageSize=${pageSize}`);
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



    // =========================
    // create team
    //==========================
    $(ids.submitBtn).on('click', function () {
        let formData = {
            TeamName: $(ids.teamName).val(),
            EmployeeIds: $(ids.employeeIDs).val()
        };

        $.ajax({
            url: '/AddTeams/CreateTeam',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert('Team saved successfully!');
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function (xhr) {
                console.error('Error:', xhr);
                alert('Something went wrong.');
            }
        });
    });

    // ===============================
    // get Team List
    //==============================
    let pageNumber = 1;
    let pageSize2 = 10;
    let searchTerm = '';
    let sortColumn = 'CreatedAt';
    let sortOrder = 'asc';

    async function fetchTeamList() {
        try {
            const query = new URLSearchParams({
                pageNumber,
                pageSize2,
                searchTerm,
                sortColumn,
                sortOrder
            });

            const response = await fetch(`/AddTeams/GetTeamList?${query.toString()}`, {
                method: 'GET',
                headers: { 'Accept': 'application/json' }
            });

            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const data = await response.json();
            renderTeamCards(data); // <-- render cards on page
            return data;
        } catch (error) {
            console.error('Error fetching teams:', error);
            return [];
        }
    }

    function renderTeamCards(teams) {
        const container = document.getElementById('teamsDiv');
        if (!container) {
            console.error("Container with id 'teamsDiv' not found in DOM");
            return;
        }

        container.innerHTML = '';

        teams.forEach(team => {
            const card = document.createElement('div');
            card.className = 'col-xl-3 col-lg-4 col-md-6 mb-4';

            const memberBadges = team.teamMemberName.map(name =>
                `<span class="badge bg-primary me-1 mb-1">${name}</span>`).join(' ');

            card.innerHTML = `
            <div class="card shadow-sm border-0 h-100 hover-shadow">
                <div class="card-body d-flex flex-column">
                    <div class="mb-2 text-muted small">Team #${team.teamID}</div>
                    <h5 class="card-title fw-bold">${team.teamGID}</h5>
                    <div class="mt-3">${memberBadges}</div>
                </div>
            </div>
        `;
            container.appendChild(card);
        });
    }

    // Call the fetch function
    fetchTeamList();


});