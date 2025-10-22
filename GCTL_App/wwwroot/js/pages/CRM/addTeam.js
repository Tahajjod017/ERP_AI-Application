$(function () {

    let ids = {
        teamID: '#TeamID',
        generatedID: '#GeneratedID',
        submitBtn: '#CreateTeamBtn',
        resetBtn: '#resetTeamBtn',
        employeeDrpdn: '#EmployeeIds',
        teamName: '#TeamName',
        employeeIDs: '#EmployeeIds',
        resultShowDiv: '#teamsDiv',
    }

    // ============================
    // load employee
    // ============================

    let page = 1;
    let term = '';
    const pageSize3 = 20;

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



    // =========================
    // create team
    //==========================
    $(ids.submitBtn).on('click', function () {
        let formData = {
            TeamID: $(ids.teamID).val(),
            TeamName: $(ids.teamName).val(),
            EmployeeIds: $(ids.employeeIDs).val()
        };

        //showDev(formData);
        console.log(formData);

        $.ajax({
            url: '/AddTeams/CreateTeam',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    fetchTeamList();
                    resetForm();
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr) {
                console.error('Error:', xhr);
                alert('Something went wrong.');
            }
        });
    });

    // =====================================
    // reset button
    // =====================================
    $(ids.resetBtn).on("click", function () {
        resetForm();
    })

    // ===============================
    // get Team List
    //==============================
    let pageNumber = 1;
    let pageSize = 25;
    let searchTerm = '';
    let sortColumn = 'CreatedAt';
    let sortOrder = 'asc';

    async function fetchTeamList() {
        try {
            const query = new URLSearchParams({
                pageNumber,
                pageSize,
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
            //showDev(data);
            renderTeamCards(data);
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

        // Loop through all teams
        teams.forEach(function (team) {
            let membersHtml = '';

            if (team.teamDetails && team.teamDetails.length > 0) {
                team.teamDetails.forEach(function (member) {
                    const headLabel = member.isTeamHead
                        ? '<span class="badge bg-success ms-2">Team Head</span>'
                        : '';

                    membersHtml += `
                    <div class="d-flex align-items-center mb-2">
                        <i class="fas fa-user me-2 pb-2"></i>
                        <span class="fw-semibold">${member.teamMemberName}</span>
                        ${headLabel}
                    </div>
                `;
                });
            }

            const teamHtml = `
            <div class="col-auto">
                <div style="height: 280px; min-width: 70px;">
                    <div class="card h-100" style="width: 20rem; height: 100%;">
                        <div class="card-body d-flex flex-column p-3" style="height: 268px;">
                            <a href="#" class="text-warning fw-bold text-center h5 text-decoration-none mb-2 addTeam-edit"
                               data-teamid="${team.teamID}"
                               title="Click for Edit Team">
                                <i class="fas fa-edit"></i> &nbsp;<span>${team.teamName}</span> &nbsp;<span>(${team.teamGID})</span> 
                            </a>
                            <hr class="my-2" />
                            <div class="overflow-auto flex-grow-1">
                                ${membersHtml}
                            </div>
                            <a href="/TeamDetails/index/${team.teamID}" class="btn btn-outline-primary rounded-pill btn-sm w-100 viewDetailsBtn">View Details</a>
                        </div>
                    </div>
                </div>
            </div>
        `;

            container.innerHTML += teamHtml;
        });
    }

    $(document).on("click", ".addTeam-edit", async function (e) {
        e.preventDefault();
        page = 1; 
        hasMore = true;
        term = ''; 
        let id = $(this).data("teamid");

        const response = await fetch(`/AddTeams/GetIndivudialTeamDetails?id=${id}`);
        if (!response.ok) throw new Error('Network response was not ok');
        const result = await response.json();
        $(ids.submitBtn).text("Update Team");
        $(ids.teamID).val(result.teamID);
        $(ids.teamName).val(result.teamName);
        $(ids.employeeIDs).empty();
        result.teamMembersInfo.forEach(member => {
            const opt = new Option(member.teamMemberName, member.teamMemberID, true, true);
            $(ids.employeeIDs).append(opt);
        });
        const selectedIds = result.teamMembersInfo.map(m => m.teamMemberID.toString());
        $(ids.employeeIDs).val(selectedIds);
        ms.update();

        fetchPage({ append: false, keepSelected: true });

        
    });


    // ============================
    // reset function
    // ============================
    function resetForm() {
        $(ids.teamID).val('');
        $(ids.teamName).val('');
        $(ids.employeeIDs).val([]);
        coreui.MultiSelect.getInstance('#EmployeeIds')?.update();
        $(ids.submitBtn).text("Create Team");
    }

    fetchTeamList();
});