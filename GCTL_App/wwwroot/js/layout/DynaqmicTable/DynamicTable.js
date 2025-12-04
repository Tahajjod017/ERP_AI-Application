

//#region Dynamic table version 2


const DynamicTable = {
    init() {
        document.querySelectorAll('.dyn-table').forEach(table => {
            const tableId = table.id || `dyn-table-${Math.random().toString(36).substr(2, 9)}`;
            if (!table.id) table.id = tableId;
            this.initializeTable(table, tableId);
        });
    },

    initializeTable(table, tableId) {

        const headers = Array.from(table.querySelectorAll('thead th')).map((th, index) => {
            th.dataset.column = index; // assign data-column automatically
            return th;
        });


        table.querySelectorAll('tbody tr').forEach(row => {
            Array.from(row.children).forEach((td, index) => {
                td.dataset.column = index;
            });
        });

        const columnVisibility = this.loadColumnVisibility(tableId);
        const columnToggleContainer = this.createColumnToggleDropdown(headers, tableId);

        const cardHeader = table.closest('.card')?.querySelector('.card-header');
        if (cardHeader) {
            const rightContent = cardHeader.querySelector('.right-content') || document.createElement('div');
            if (!rightContent.classList.contains('right-content')) {
                rightContent.classList.add('right-content', 'd-flex', 'align-items-center', 'flex-wrap', 'row-gap-3');
                cardHeader.appendChild(rightContent);
            }
            rightContent.prepend(columnToggleContainer);
        }

        headers.forEach(header => {
            const columnIndex = header.dataset.column;
            if (columnVisibility.hasOwnProperty(columnIndex)) {
                this.toggleColumn(table, columnIndex, columnVisibility[columnIndex]);
                columnToggleContainer.querySelector(`#col-${tableId}-${columnIndex}`).checked = columnVisibility[columnIndex];
            }
        });

        columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
            toggle.addEventListener('change', () => {
                this.toggleColumn(table, toggle.dataset.column, toggle.checked);
                this.saveColumnVisibility(tableId, toggle.dataset.column, toggle.checked);
            });
        });

        columnToggleContainer.querySelector('#selectAllColumns')?.addEventListener('click', () => {
            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                toggle.checked = true;
                this.toggleColumn(table, toggle.dataset.column, true);
                this.saveColumnVisibility(tableId, toggle.dataset.column, true);
            });
        });

        columnToggleContainer.querySelector('#resetColumns')?.addEventListener('click', () => {
            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                toggle.checked = true;
                this.toggleColumn(table, toggle.dataset.column, true);
            });
            localStorage.removeItem(`${tableId}_columnVisibility`);
        });

        columnToggleContainer.querySelector('.column-toggle-dropdown').addEventListener('click', e => {
            e.stopPropagation();
        });
    },

    createColumnToggleDropdown(headers, tableId) {
        const dropdown = document.createElement('div');
        dropdown.classList.add('dropdown', 'me-3');
        dropdown.innerHTML = `
            <button class="btn  dropdown-toggle colmDrag  text-muted" type="button" data-bs-toggle="dropdown" aria-expanded="false" >
                <i class="fas fa-columns me-2"></i>Columns
            </button>
            <div class="dropdown-menu column-toggle-dropdown p-2">
                <div class="fw-bold mb-2 text-muted">Show/Hide Columns</div>
                ${Array.from(headers).map((header, index) => `
                    <div class="column-toggle-item">
                        <div class="form-check">
                            <input class="form-check-input column-toggle" type="checkbox" id="col-${tableId}-${index}" data-column="${index}" checked>
                            <label class="form-check-label" for="col-${tableId}-${index}">${header.textContent.trim()}</label>
                        </div>
                    </div>
                `).join('')}
                <hr class="my-2">
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" id="selectAllColumns">Select All</button>
                    <button class="btn btn-sm btn-outline-secondary" id="resetColumns">Reset</button>
                </div>
            </div>
        `;
        return dropdown;
    },

    toggleColumn(table, columnIndex, isVisible) {
        const headers = table.querySelectorAll(`th[data-column="${columnIndex}"]`);
        const cells = table.querySelectorAll(`td[data-column="${columnIndex}"]`);
        if (isVisible) {
            headers.forEach(header => header.style.display = '');
            cells.forEach(cell => cell.style.display = '');
        } else {
            headers.forEach(header => header.style.display = 'none');
            cells.forEach(cell => cell.style.display = 'none');
        }
    },

    loadColumnVisibility(tableId) {
        try {
            const saved = localStorage.getItem(`${tableId}_columnVisibility`);
            return saved ? JSON.parse(saved) : {};
        } catch (e) {
            console.error(`Failed to load column visibility for table ${tableId}:`, e);
            return {};
        }
    },

    saveColumnVisibility(tableId, columnIndex, isVisible) {
        const columnVisibility = this.loadColumnVisibility(tableId);
        columnVisibility[columnIndex] = isVisible;
        localStorage.setItem(`${tableId}_columnVisibility`, JSON.stringify(columnVisibility));
    }
};

$(document).ready(function () {
    DynamicTable.init();
});


//#endregion


//#region dynamic DD

const DynamicTableDD = {
    init() {
        console.log('Initializing DynamicTableDD...');
        document.querySelectorAll('.dyn-tableDD').forEach(table => {
            const tableId = table.id || `dyn-table-${Math.random().toString(36).substr(2, 9)}`;
            if (!table.id) {
                table.id = tableId;
                console.log(`Assigned table ID: ${tableId}`);
            }
            // Find .columnDD with matching class name
            const tableClasses = table.classList;
            let columnDDInput = null;
            for (const cls of tableClasses) {
                if (cls !== 'dyn-table') {
                    columnDDInput = document.querySelector(`.columnDD.${cls}`);
                    if (columnDDInput) {
                        console.log(`Found columnDD input with class ${cls} for table ${tableId}`);
                        break;
                    }
                }
            }
            this.initializeTable(table, tableId, columnDDInput);
        });
    },

    initializeTable(table, tableId, columnDDInput) {
        console.log(`Initializing table ${tableId}`);
        const headers = Array.from(table.querySelectorAll('thead th')).map((th, index) => {
            th.dataset.column = index;
            return th;
        });

        table.querySelectorAll('tbody tr').forEach(row => {
            Array.from(row.children).forEach((td, index) => {
                td.dataset.column = index;
            });
        });

        const columnVisibility = this.loadColumnVisibility(tableId);
        console.log(`Loaded column visibility for ${tableId}:`, columnVisibility);

        let columnToggleContainer = null;
        if (columnDDInput) {
            columnToggleContainer = this.createColumnToggleDropdown(headers, tableId);
            columnDDInput.parentElement.insertBefore(columnToggleContainer, columnDDInput.nextSibling);
            columnToggleContainer.style.position = 'relative';

            const updateDropdownPosition = () => {
                const inputRect = columnDDInput.getBoundingClientRect();
                const dropdownMenu = columnToggleContainer.querySelector('.dropdown-menu');
                dropdownMenu.style.position = 'absolute';
                dropdownMenu.style.top = `${inputRect.height + 5}px`;
                dropdownMenu.style.left = '0';
                console.log(`Updated dropdown position for ${tableId}: top=${inputRect.height + 5}px, left=0`);
            };

            window.addEventListener('resize', updateDropdownPosition);
            window.addEventListener('scroll', updateDropdownPosition);
            updateDropdownPosition();
        } else {
            console.warn(`No matching columnDD input found for table ${tableId}, skipping dropdown creation`);
        }

        headers.forEach(header => {
            const columnIndex = header.dataset.column;
            const isVisible = columnVisibility.hasOwnProperty(columnIndex) ? columnVisibility[columnIndex] : true;
            console.log(`Applying visibility for column ${columnIndex} in table ${tableId}: ${isVisible}`);
            this.toggleColumn(table, columnIndex, isVisible);
            if (columnToggleContainer) {
                const checkbox = columnToggleContainer.querySelector(`#col-${tableId}-${columnIndex}`);
                if (checkbox) {
                    checkbox.checked = isVisible;
                }
            }
        });

        if (columnToggleContainer) {
            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                toggle.addEventListener('change', () => {
                    console.log(`Toggling column ${toggle.dataset.column} in table ${tableId} to ${toggle.checked}`);
                    this.toggleColumn(table, toggle.dataset.column, toggle.checked);
                    this.saveColumnVisibility(tableId, toggle.dataset.column, toggle.checked);
                });
            });

            columnToggleContainer.querySelector('#selectAllColumns')?.addEventListener('click', () => {
                console.log(`Selecting all columns for table ${tableId}`);
                columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                    toggle.checked = true;
                    this.toggleColumn(table, toggle.dataset.column, true);
                    this.saveColumnVisibility(tableId, toggle.dataset.column, true);
                });
            });

            columnToggleContainer.querySelector('#resetColumns')?.addEventListener('click', () => {
                console.log(`Resetting columns for table ${tableId}`);
                columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                    toggle.checked = true;
                    this.toggleColumn(table, toggle.dataset.column, true);
                });
                localStorage.removeItem(`${tableId}_columnVisibility`);
                console.log(`Cleared localStorage for ${tableId}_columnVisibility`);
            });

            columnToggleContainer.querySelector('.column-toggle-dropdown').addEventListener('click', e => {
                e.stopPropagation();
            });
        }
    },

    createColumnToggleDropdown(headers, tableId) {
        const dropdown = document.createElement('div');
        dropdown.classList.add('dropdown');
        dropdown.innerHTML = `
            <button class="btn  dropdown-toggle colmDrag  text-muted" type="button" data-bs-toggle="dropdown" aria-expanded="false" >
                <i class="fas fa-columns me-2"></i>Columns
            </button>
            <div class="dropdown-menu column-toggle-dropdown p-2" style="min-width: 200px;">
                <div class="fw-bold mb-2 text-muted">Show/Hide Columns</div>
                ${Array.from(headers)
                .map(
                    (header, index) => `
                    <div class="column-toggle-item">
                        <div class="form-check">
                            <input class="form-check-input column-toggle" type="checkbox" id="col-${tableId}-${index}" data-column="${index}" checked>
                            <label class="form-check-label" for="col-${tableId}-${index}">${header.textContent.trim()}</label>
                        </div>
                    </div>
                `
                )
                .join('')}
                <hr class="my-2">
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" id="selectAllColumns">Select All</button>
                    <button class="btn btn-sm btn-outline-secondary" id="resetColumns">Reset</button>
                </div>
            </div>
        `;
        return dropdown;
    },

    toggleColumn(table, columnIndex, isVisible) {
        const headers = table.querySelectorAll(`th[data-column="${columnIndex}"]`);
        const cells = table.querySelectorAll(`td[data-column="${columnIndex}"]`);
        console.log(`Toggling column ${columnIndex} in table ${table.id}: headers=${headers.length}, cells=${cells.length}`);
        if (isVisible) {
            headers.forEach(header => (header.style.display = ''));
            cells.forEach(cell => (cell.style.display = ''));
        } else {
            headers.forEach(header => (header.style.display = 'none'));
            cells.forEach(cell => (cell.style.display = 'none'));
        }
    },

    loadColumnVisibility(tableId) {
        try {
            const saved = localStorage.getItem(`${tableId}_columnVisibility`);
            if (!saved) {
                console.log(`No saved visibility data found for ${tableId}`);
                return {};
            }

            const data = JSON.parse(saved);
            const { visibility, expiry } = data;

            if (expiry && Date.now() > expiry) {
                console.log(`Visibility data for ${tableId} has expired, clearing...`);
                localStorage.removeItem(`${tableId}_columnVisibility`);
                return {};
            }

            console.log(`Loaded valid visibility data for ${tableId}:`, visibility);
            return visibility || {};
        } catch (e) {
            console.error(`Failed to load column visibility for table ${tableId}:`, e);
            return {};
        }
    },

    saveColumnVisibility(tableId, columnIndex, isVisible) {
        try {
            const columnVisibility = this.loadColumnVisibility(tableId);
            columnVisibility[columnIndex] = isVisible;

            const oneYearFromNow = Date.now() + 365 * 24 * 60 * 60 * 1000;
            const data = {
                visibility: columnVisibility,
                expiry: oneYearFromNow
            };

            localStorage.setItem(`${tableId}_columnVisibility`, JSON.stringify(data));
            console.log(`Saved visibility for ${tableId}, column ${columnIndex}: ${isVisible}, expires at ${new Date(oneYearFromNow).toISOString()}`);
        } catch (e) {
            console.error(`Failed to save column visibility for table ${tableId}:`, e);
        }
    },

    applyColumnVisibilityToNewRows(table, tableId) {
        const columnVisibility = this.loadColumnVisibility(tableId);
        console.log(`Applying visibility to new rows for ${tableId}:`, columnVisibility);
        Object.keys(columnVisibility).forEach(columnIndex => {
            if (!columnVisibility[columnIndex]) {
                table.querySelectorAll(`td[data-column="${columnIndex}"]`).forEach(cell => {
                    cell.style.display = 'none';
                });
            }
        });
    }
};

$(document).ready(function () {
    console.log('Document ready, initializing DynamicTableDD');
    DynamicTableDD.init();
});

//#endregion

//#region dynamic table version 1


//const DynamicTable = {


//    // Initialize all tables with class 'dyn-table'
//    init() {
//        document.querySelectorAll('.dyn-table').forEach(table => {
//            const tableId = table.id || `dyn-table-${Math.random().toString(36).substr(2, 9)}`; // Unique ID for each table
//            if (!table.id) table.id = tableId; // Assign ID if none exists
//            this.initializeTable(table, tableId);
//        });
//    },

//    // Initialize column visibility for a single table
//    initializeTable(table, tableId) {
//        const headers = table.querySelectorAll('thead th[data-column]');
//        const columnVisibility = this.loadColumnVisibility(tableId);
//        const columnToggleContainer = this.createColumnToggleDropdown(table, headers, tableId);

//        // Insert dropdown into the DOM (e.g., in the card header)
//        const cardHeader = table.closest('.card')?.querySelector('.card-header');
//        if (cardHeader) {
//            const rightContent = cardHeader.querySelector('.right-content') || document.createElement('div');
//            if (!rightContent.classList.contains('right-content')) {
//                rightContent.classList.add('right-content', 'd-flex', 'align-items-center', 'flex-wrap', 'row-gap-3');
//                cardHeader.appendChild(rightContent);
//            }
//            rightContent.prepend(columnToggleContainer);
//        }

//        // Apply initial column visibility
//        headers.forEach(header => {
//            const columnIndex = header.dataset.column;
//            if (columnVisibility.hasOwnProperty(columnIndex)) {
//                this.toggleColumn(table, columnIndex, columnVisibility[columnIndex]);
//            }
//        });

//        // Event listeners for column toggles
//        columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
//            toggle.addEventListener('change', () => {
//                this.toggleColumn(table, toggle.dataset.column, toggle.checked);
//                this.saveColumnVisibility(tableId, toggle.dataset.column, toggle.checked);
//            });
//        });

//        // Select All and Reset buttons
//        columnToggleContainer.querySelector('#selectAllColumns')?.addEventListener('click', () => {
//            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
//                toggle.checked = true;
//                this.toggleColumn(table, toggle.dataset.column, true);
//                this.saveColumnVisibility(tableId, toggle.dataset.column, true);
//            });
//        });

//        columnToggleContainer.querySelector('#resetColumns')?.addEventListener('click', () => {
//            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
//                toggle.checked = true;
//                this.toggleColumn(table, toggle.dataset.column, true);
//            });
//            localStorage.removeItem(`${tableId}_columnVisibility`);
//        });

//        // Prevent dropdown from closing when clicking inside
//        columnToggleContainer.querySelector('.column-toggle-dropdown').addEventListener('click', e => {
//            e.stopPropagation();
//        });
//    },

//    // Create column toggle dropdown UI
//    createColumnToggleDropdown(table, headers, tableId) {
//        const dropdown = document.createElement('div');
//        dropdown.classList.add('dropdown', 'me-3');
//        dropdown.innerHTML = `
//            <button class="btn btn-light dropdown-toggle bg-white text-muted" type="button" data-bs-toggle="dropdown" aria-expanded="false" style="border: 1px solid #cbd0dd;color: #979aa3 !important;">
//                <i class="fas fa-columns me-2"></i>Columns
//            </button>
//            <div class="dropdown-menu column-toggle-dropdown p-2">
//                <div class="fw-bold mb-2 text-muted">Show/Hide Columns</div>
//                ${Array.from(headers).map(header => `
//                    <div class="column-toggle-item">
//                        <div class="form-check">
//                            <input class="form-check-input column-toggle" type="checkbox" id="col-${tableId}-${header.dataset.column}" data-column="${header.dataset.column}" checked>
//                            <label class="form-check-label" for="col-${tableId}-${header.dataset.column}">${header.textContent.trim()}</label>
//                        </div>
//                    </div>
//                `).join('')}
//                <hr class="my-2">
//                <div class="d-flex gap-2">
//                    <button class="btn btn-sm btn-outline-primary" id="selectAllColumns">Select All</button>
//                    <button class="btn btn-sm btn-outline-secondary" id="resetColumns">Reset</button>
//                </div>
//            </div>
//        `;
//        return dropdown;
//    },

//    // Toggle column visibility
//    toggleColumn(table, columnIndex, isVisible) {
//        const headers = table.querySelectorAll(`th[data-column="${columnIndex}"]`);
//        const cells = table.querySelectorAll(`td[data-column="${columnIndex}"]`);
//        console.log(`Toggling column ${columnIndex} in table ${table.id}: headers=${headers.length}, cells=${cells.length}`);
//        if (isVisible) {
//            headers.forEach(header => header.style.display = '');
//            cells.forEach(cell => cell.style.display = '');
//        } else {
//            headers.forEach(header => header.style.display = 'none');
//            cells.forEach(cell => cell.style.display = 'none');
//        }
//    },

//    // Load column visibility from localStorage
//    loadColumnVisibility(tableId) {
//        try {
//            const saved = localStorage.getItem(`${tableId}_columnVisibility`);
//            return saved ? JSON.parse(saved) : {};
//        } catch (e) {
//            console.error(`Failed to load column visibility for table ${tableId}:`, e);
//            return {};
//        }
//    },

//    // Save column visibility to localStorage
//    saveColumnVisibility(tableId, columnIndex, isVisible) {
//        const columnVisibility = this.loadColumnVisibility(tableId);
//        columnVisibility[columnIndex] = isVisible;
//        localStorage.setItem(`${tableId}_columnVisibility`, JSON.stringify(columnVisibility));
//    },


//   // DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('id'), 'id');


//    applyColumnVisibilityToNewRows(table, tableId) {
//        const columnVisibility = this.loadColumnVisibility(tableId);
//        Object.keys(columnVisibility).forEach(columnIndex => {
//            if (!columnVisibility[columnIndex]) {
//                table.querySelectorAll(`td[data-column="${columnIndex}"]`).forEach(cell => {
//                    cell.style.display = 'none';
//                });
//            }
//        });
//    }
//};

//// Initialize on document ready
//$(document).ready(function () {
//    DynamicTable.init();
//});


//#endregion



//#region Drag version

//const DynamicTableDrag = {
//    init() {
//        console.log('Initializing DynamicTableDrag at ' + new Date().toISOString());
//        if (typeof Sortable === 'undefined') {
//            const script = document.createElement('script');
//            script.src = 'https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js';
//            script.onload = () => {
//                console.log('SortableJS loaded');
//                this.initializeTables();
//            };
//            document.head.appendChild(script);
//        } else {
//            this.initializeTables();
//        }
//    },

//    initializeTables() {
//        document.querySelectorAll('.dyn-tableDrag').forEach(table => {
//            const tableId = table.id || `dyn-tableDrag-${Math.random().toString(36).substr(2, 9)}`;
//            if (!table.id) {
//                table.id = tableId;
//                console.log(`Assigned table ID: ${tableId}`);
//            }
//            const tableClasses = table.classList;
//            let columnDragInput = null;
//            for (const cls of tableClasses) {
//                if (cls !== 'dyn-tableDrag') {
//                    columnDragInput = document.querySelector(`.columnDrag.${cls}`);
//                    if (columnDragInput) {
//                        console.log(`Found columnDrag input with class ${cls} for table ${tableId}`);
//                        break;
//                    }
//                }
//            }
//            this.initializeTable(table, tableId, columnDragInput);
//        });
//    },

//    initializeTable(table, tableId, columnDragInput) {
//        console.log(`Initializing table ${tableId}`);
//        const headers = Array.from(table.querySelectorAll('thead th')).map((th, index) => {
//            th.dataset.column = index;
//            return th;
//        });

//        table.querySelectorAll('tbody tr').forEach(row => {
//            Array.from(row.children).forEach((td, index) => {
//                td.dataset.column = index;
//            });
//        });

//        const { visibility, order } = this.loadColumnSettings(tableId);
//        console.log(`Loaded settings for ${tableId}: visibility=${JSON.stringify(visibility)}, order=${JSON.stringify(order)}`);

//        // Apply saved column order
//        let currentOrder = order && order.length === headers.length ? order : headers.map((_, index) => index);
//        console.log(`Applying column order for ${tableId}: ${currentOrder}`);
//        this.reorderColumns(table, currentOrder);

//        let columnToggleContainer = null;
//        if (columnDragInput) {
//            columnToggleContainer = this.createColumnToggleDropdown(headers, tableId, currentOrder, visibility);
//            columnDragInput.parentElement.insertBefore(columnToggleContainer, columnDragInput.nextSibling);
//            columnToggleContainer.style.position = 'relative';

//            const updateDropdownPosition = () => {
//                const inputRect = columnDragInput.getBoundingClientRect();
//                const dropdownMenu = columnToggleContainer.querySelector('.dropdown-menu');
//                dropdownMenu.style.position = 'absolute';
//                dropdownMenu.style.top = `${inputRect.height + 5}px`;
//                dropdownMenu.style.left = '0';
//                console.log(`Updated dropdown position for ${tableId}: top=${inputRect.height + 5}px, left=0`);
//            };

//            window.addEventListener('resize', updateDropdownPosition);
//            window.addEventListener('scroll', updateDropdownPosition);
//            updateDropdownPosition();

//            // Make dropdown items draggable
//            const dropdownList = columnToggleContainer.querySelector('.column-toggle-list');
//            Sortable.create(dropdownList, {
//                animation: 150,
//                handle: '.sortable-handle',
//                onEnd: (evt) => {
//                    const newOrder = Array.from(dropdownList.querySelectorAll('.column-toggle-item'))
//                        .map(item => parseInt(item.dataset.column));
//                    console.log(`New dropdown order for ${tableId}:`, newOrder);
//                    this.reorderColumns(table, newOrder);
//                    this.saveColumnSettings(tableId, visibility, newOrder);
//                }
//            });
//        } else {
//            console.warn(`No matching columnDrag input found for table ${tableId}, skipping dropdown creation`);
//        }

//        // Make table headers draggable
//        const headerRow = table.querySelector('thead tr');
//        Sortable.create(headerRow, {
//            animation: 150,
//            handle: 'th',
//            onEnd: (evt) => {
//                const newOrder = Array.from(headerRow.querySelectorAll('th'))
//                    .map(th => parseInt(th.dataset.column));
//                console.log(`New table column order for ${tableId}:`, newOrder);
//                this.reorderColumns(table, newOrder);
//                this.saveColumnSettings(tableId, visibility, newOrder);
//                if (columnToggleContainer) {
//                    this.updateDropdownOrder(columnToggleContainer, newOrder, visibility);
//                }
//            }
//        });

//        // Apply visibility after reordering
//        Object.entries(visibility).forEach(([columnIndex, isVisible]) => {
//            console.log(`Applying visibility for column ${columnIndex} in table ${tableId}: ${isVisible}`);
//            this.toggleColumn(table, columnIndex, isVisible);
//        });

//        // Sync dropdown checkboxes
//        if (columnToggleContainer) {
//            Object.entries(visibility).forEach(([columnIndex, isVisible]) => {
//                const checkbox = columnToggleContainer.querySelector(`#col-${tableId}-${columnIndex}`);
//                if (checkbox) {
//                    checkbox.checked = isVisible;
//                    console.log(`Set checkbox for column ${columnIndex} in ${tableId} to ${isVisible}`);
//                }
//            });

//            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
//                toggle.addEventListener('change', () => {
//                    console.log(`Toggling column ${toggle.dataset.column} in table ${tableId} to ${toggle.checked}`);
//                    this.toggleColumn(table, toggle.dataset.column, toggle.checked);
//                    const newVisibility = { ...visibility, [toggle.dataset.column]: toggle.checked };
//                    this.saveColumnSettings(tableId, newVisibility, currentOrder);
//                });
//            });

//            columnToggleContainer.querySelector('#selectAllColumns')?.addEventListener('click', () => {
//                console.log(`Selecting all columns for table ${tableId}`);
//                const newVisibility = {};
//                headers.forEach(header => {
//                    newVisibility[header.dataset.column] = true;
//                    this.toggleColumn(table, header.dataset.column, true);
//                });
//                columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
//                    toggle.checked = true;
//                });
//                this.saveColumnSettings(tableId, newVisibility, currentOrder);
//            });

//            columnToggleContainer.querySelector('#resetColumns')?.addEventListener('click', () => {
//                console.log(`Resetting columns for table ${tableId}`);
//                const newVisibility = {};
//                const newOrder = headers.map((_, index) => index);
//                headers.forEach(header => {
//                    newVisibility[header.dataset.column] = true;
//                    this.toggleColumn(table, header.dataset.column, true);
//                });
//                columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
//                    toggle.checked = true;
//                });
//                this.reorderColumns(table, newOrder);
//                this.updateDropdownOrder(columnToggleContainer, newOrder, newVisibility);
//                localStorage.removeItem(`${tableId}_columnSettings`);
//                console.log(`Cleared localStorage for ${tableId}_columnSettings`);
//            });

//            columnToggleContainer.querySelector('.column-toggle-dropdown').addEventListener('click', e => {
//                e.stopPropagation();
//            });
//        }
//    },

//    createColumnToggleDropdown(headers, tableId, order, visibility) {
//        const dropdown = document.createElement('div');
//        dropdown.classList.add('dropdown');
//        const orderedHeaders = order && order.length === headers.length
//            ? order.map(index => headers.find(h => parseInt(h.dataset.column) === index))
//            : headers;
//        dropdown.innerHTML = `
//            <button class="btn btn-light dropdown-toggle bg-white text-muted" type="button" data-bs-toggle="dropdown" aria-expanded="false" style="border: 1px solid #cbd0dd;color: #979aa3 !important;">
//                <i class="fas fa-columns me-2"></i>Columns
//            </button>
//            <div class="dropdown-menu column-toggle-dropdown p-2" style="min-width: 200px;">
//                <div class="fw-bold mb-2 text-muted">Show/Hide & Reorder Columns</div>
//                <div class="column-toggle-list">
//                    ${orderedHeaders
//                .map(
//                    header => `
//                        <div class="column-toggle-item" data-column="${header.dataset.column}">
//                            <div class="form-check d-flex align-items-center">
//                                <i class="fas fa-grip-vertical me-1 sortable-handle" style="cursor: move;"></i>
//                                <input class="form-check-input column-toggle ms-1 me-2" type="checkbox" id="col-${tableId}-${header.dataset.column}" data-column="${header.dataset.column}" ${visibility[header.dataset.column] === false ? '' : 'checked'}>
//                                <label class="form-check-label" for="col-${tableId}-${header.dataset.column}">${header.textContent.trim()}</label>
//                            </div>
//                        </div>
//                    `
//                )
//                .join('')}
//                </div>
//                <hr class="my-2">
//                <div class="d-flex gap-2">
//                    <button class="btn btn-sm btn-outline-primary" id="selectAllColumns">Select All</button>
//                    <button class="btn btn-sm btn-outline-secondary" id="resetColumns">Reset</button>
//                </div>
//            </div>
//        `;
//        console.log(`Created dropdown for ${tableId} with visibility: ${JSON.stringify(visibility)}`);
//        return dropdown;
//    },

//    toggleColumn(table, columnIndex, isVisible) {
//        const headers = table.querySelectorAll(`th[data-column="${columnIndex}"]`);
//        const cells = table.querySelectorAll(`td[data-column="${columnIndex}"]`);
//        console.log(`Toggling column ${columnIndex} in table ${table.id}: headers=${headers.length}, cells=${cells.length}, isVisible=${isVisible}`);
//        if (isVisible) {
//            headers.forEach(header => {
//                header.style.display = '';
//                console.log(`Showing header ${columnIndex} in table ${table.id}`);
//            });
//            cells.forEach(cell => {
//                cell.style.display = '';
//                console.log(`Showing cell ${columnIndex} in table ${table.id}`);
//            });
//        } else {
//            headers.forEach(header => {
//                header.style.display = 'none';
//                console.log(`Hiding header ${columnIndex} in table ${table.id}`);
//            });
//            cells.forEach(cell => {
//                cell.style.display = 'none';
//                console.log(`Hiding cell ${columnIndex} in table ${table.id}`);
//            });
//        }
//    },

//    reorderColumns(table, order) {
//        const headerRow = table.querySelector('thead tr');
//        const headers = Array.from(headerRow.querySelectorAll('th'));
//        const rows = table.querySelectorAll('tbody tr');

//        const sortedHeaders = order.map(index => headers.find(h => parseInt(h.dataset.column) === index));
//        headerRow.innerHTML = '';
//        sortedHeaders.forEach(header => headerRow.appendChild(header));

//        rows.forEach(row => {
//            const cells = Array.from(row.querySelectorAll('td'));
//            row.innerHTML = '';
//            order.forEach(index => {
//                const cell = cells.find(c => parseInt(c.dataset.column) === index);
//                if (cell) row.appendChild(cell);
//            });
//        });
//        console.log(`Reordered columns for table ${table.id}:`, order);
//    },

//    updateDropdownOrder(container, order, visibility) {
//        const dropdownList = container.querySelector('.column-toggle-list');
//        const items = Array.from(dropdownList.querySelectorAll('.column-toggle-item'));
//        dropdownList.innerHTML = '';
//        order.forEach(index => {
//            const item = items.find(i => parseInt(i.dataset.column) === index);
//            if (item) {
//                const checkbox = item.querySelector('.column-toggle');
//                checkbox.checked = visibility.hasOwnProperty(index) ? visibility[index] : true;
//                dropdownList.appendChild(item);
//                console.log(`Updated dropdown item for column ${index}: checked=${checkbox.checked}`);
//            }
//        });
//        console.log(`Updated dropdown order for table:`, order);
//    },

//    loadColumnSettings(tableId) {
//        try {
//            const saved = localStorage.getItem(`${tableId}_columnSettings`);
//            if (!saved) {
//                console.log(`No saved settings data found for ${tableId}`);
//                return { visibility: {}, order: null };
//            }

//            const data = JSON.parse(saved);
//            const { visibility, order, expiry } = data;

//            if (expiry && Date.now() > expiry) {
//                console.log(`Settings data for ${tableId} has expired, clearing...`);
//                localStorage.removeItem(`${tableId}_columnSettings`);
//                return { visibility: {}, order: null };
//            }

//            console.log(`Loaded valid settings data for ${tableId}:`, { visibility, order });
//            return { visibility: visibility || {}, order: order || null };
//        } catch (e) {
//            console.error(`Failed to load column settings for table ${tableId}:`, e);
//            return { visibility: {}, order: null };
//        }
//    },

//    saveColumnSettings(tableId, visibility, order) {
//        try {
//            const oneYearFromNow = Date.now() + 365 * 24 * 60 * 60 * 1000;
//            const data = {
//                visibility,
//                order,
//                expiry: oneYearFromNow
//            };

//            localStorage.setItem(`${tableId}_columnSettings`, JSON.stringify(data));
//            console.log(`Saved settings for ${tableId}: visibility=${JSON.stringify(visibility)}, order=${JSON.stringify(order)}, expires at ${new Date(oneYearFromNow).toISOString()}`);
//        } catch (e) {
//            console.error(`Failed to save column settings for table ${tableId}:`, e);
//        }
//    },

//    applyColumnSettingsToNewRows(table, tableId) {
//        const { visibility, order } = this.loadColumnSettings(tableId);
//        console.log(`Applying settings to new rows for ${tableId}: visibility=${JSON.stringify(visibility)}, order=${JSON.stringify(order)}`);
//        table.querySelectorAll('tbody tr').forEach(row => {
//            Array.from(row.children).forEach((td, index) => {
//                td.dataset.column = index;
//            });
//            if (order) {
//                const cells = Array.from(row.querySelectorAll('td'));
//                row.innerHTML = '';
//                order.forEach(index => {
//                    const cell = cells.find(c => parseInt(c.dataset.column) === index);
//                    if (cell) row.appendChild(cell);
//                });
//            }
//            Object.entries(visibility).forEach(([columnIndex, isVisible]) => {
//                if (!isVisible) {
//                    table.querySelectorAll(`td[data-column="${columnIndex}"]`).forEach(cell => {
//                        cell.style.display = 'none';
//                        console.log(`Hiding new cell ${columnIndex} in table ${tableId}`);
//                    });
//                }
//            });
//        });
//    }
//};

//$(document).ready(function () {
//    console.log('Document ready, initializing DynamicTableDrag at ' + new Date().toISOString());
//    DynamicTableDrag.init();
//});

//#endregion


//#region Drag V2

const DynamicTableDrag = {
    init() {
        console.log('Initializing DynamicTableDrag at ' + new Date().toISOString());
        if (typeof Sortable === 'undefined') {
            const script = document.createElement('script');
            //script.src = 'https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js';
            script.src ='/js/layout/DynaqmicTable/cdn.js' ;
            script.onload = () => {
                console.log('SortableJS loaded');
                this.initializeTables();
            };
            script.onerror = () => console.error('Failed to load SortableJS');
            document.head.appendChild(script);
        } else {
            this.initializeTables();
        }
    },

    initializeTables() {
        document.querySelectorAll('.dyn-tableDrag').forEach(table => {
            if (!table.id) {
                console.error('Table missing ID; settings won\'t be saved persistently.');
                return; // Skip tables without IDs
            }
            const tableId = table.id;
            const tableClasses = table.classList;
            let columnDragInput = null;
            for (const cls of tableClasses) {
                if (cls !== 'dyn-tableDrag') {
                    columnDragInput = document.querySelector(`.columnDrag.${cls}`);
                    if (columnDragInput) {
                        console.log(`Found columnDrag input with class ${cls} for table ${tableId}`);
                        break;
                    }
                }
            }
            this.initializeTable(table, tableId, columnDragInput);
        });
    },

    initializeTable(table, tableId, columnDragInput) {
        console.log(`Initializing table ${tableId}`);
        const headers = Array.from(table.querySelectorAll('thead th')).map((th, index) => {
            th.dataset.column = index;
            return th;
        });

        // Apply data-column to existing rows
        this.applyDataColumnToRows(table);

        const { visibility, order } = this.loadColumnSettings(tableId);
        console.log(`Loaded settings for ${tableId}: visibility=${JSON.stringify(visibility)}, order=${JSON.stringify(order)}`);

        // Apply saved column order or default to header indices
        let currentOrder = order && order.length === headers.length ? order : headers.map((_, index) => index);
        console.log(`Applying column order for ${tableId}: ${currentOrder}`);
        this.reorderColumns(table, currentOrder);

        let columnToggleContainer = null;
        if (columnDragInput) {
            columnToggleContainer = this.createColumnToggleDropdown(headers, tableId, currentOrder, visibility);
            columnDragInput.parentElement.insertBefore(columnToggleContainer, columnDragInput.nextSibling);
            columnToggleContainer.style.position = 'relative';

            const updateDropdownPosition = () => {
                const inputRect = columnDragInput.getBoundingClientRect();
                const dropdownMenu = columnToggleContainer.querySelector('.dropdown-menu');
                dropdownMenu.style.position = 'absolute';
                dropdownMenu.style.top = `${inputRect.height + 5}px`;
                dropdownMenu.style.left = '0';
                console.log(`Updated dropdown position for ${tableId}: top=${inputRect.height + 5}px, left=0`);
            };

            window.addEventListener('resize', updateDropdownPosition);
            window.addEventListener('scroll', updateDropdownPosition);
            updateDropdownPosition();

            // Make dropdown items draggable
            const dropdownList = columnToggleContainer.querySelector('.column-toggle-list');
            Sortable.create(dropdownList, {
                animation: 150,
                handle: '.sortable-handle',
                onEnd: (evt) => {
                    const newOrder = Array.from(dropdownList.querySelectorAll('.column-toggle-item'))
                        .map(item => parseInt(item.dataset.column));
                    console.log(`New dropdown order for ${tableId}:`, newOrder);
                    this.reorderColumns(table, newOrder);
                    currentOrder = newOrder; // Update current order
                    this.saveColumnSettings(tableId, visibility, newOrder);
                }
            });
        } else {
            console.warn(`No matching columnDrag input found for table ${tableId}, skipping dropdown creation`);
        }

        // Make table headers draggable
        const headerRow = table.querySelector('thead tr');
        Sortable.create(headerRow, {
            animation: 150,
            handle: 'th',
            onEnd: (evt) => {
                const newOrder = Array.from(headerRow.querySelectorAll('th'))
                    .map(th => parseInt(th.dataset.column));
                console.log(`New table column order for ${tableId}:`, newOrder);
                this.reorderColumns(table, newOrder);
                currentOrder = newOrder; // Update current order
                this.saveColumnSettings(tableId, visibility, newOrder);
                if (columnToggleContainer) {
                    this.updateDropdownOrder(columnToggleContainer, newOrder, visibility);
                }
            }
        });

        // Apply visibility after reordering
        Object.entries(visibility).forEach(([columnIndex, isVisible]) => {
            console.log(`Applying visibility for column ${columnIndex} in table ${tableId}: ${isVisible}`);
            this.toggleColumn(table, columnIndex, isVisible);
        });

        // Sync dropdown checkboxes and add event listeners
        if (columnToggleContainer) {
            Object.entries(visibility).forEach(([columnIndex, isVisible]) => {
                const checkbox = columnToggleContainer.querySelector(`#col-${tableId}-${columnIndex}`);
                if (checkbox) {
                    checkbox.checked = isVisible !== false; // Default to true if not explicitly false
                    console.log(`Set checkbox for column ${columnIndex} in ${tableId} to ${checkbox.checked}`);
                }
            });

            columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                toggle.addEventListener('change', () => {
                    console.log(`Toggling column ${toggle.dataset.column} in table ${tableId} to ${toggle.checked}`);
                    this.toggleColumn(table, toggle.dataset.column, toggle.checked);
                    visibility[toggle.dataset.column] = toggle.checked;
                    this.saveColumnSettings(tableId, visibility, currentOrder);
                });
            });

            columnToggleContainer.querySelector('#selectAllColumns')?.addEventListener('click', () => {
                console.log(`Selecting all columns for table ${tableId}`);
                const newVisibility = {};
                headers.forEach(header => {
                    newVisibility[header.dataset.column] = true;
                    this.toggleColumn(table, header.dataset.column, true);
                });
                columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                    toggle.checked = true;
                });
                Object.assign(visibility, newVisibility);
                this.saveColumnSettings(tableId, visibility, currentOrder);
            });

            columnToggleContainer.querySelector('#resetColumns')?.addEventListener('click', () => {
                console.log(`Resetting columns for table ${tableId}`);
                const newVisibility = {};
                const newOrder = headers.map((_, index) => index);
                headers.forEach(header => {
                    newVisibility[header.dataset.column] = true;
                    this.toggleColumn(table, header.dataset.column, true);
                });
                columnToggleContainer.querySelectorAll('.column-toggle').forEach(toggle => {
                    toggle.checked = true;
                });
                this.reorderColumns(table, newOrder);
                this.updateDropdownOrder(columnToggleContainer, newOrder, newVisibility);
                localStorage.removeItem(`${tableId}_columnSettings`);
                console.log(`Cleared localStorage for ${tableId}_columnSettings`);
                // Reset the variables
                Object.assign(visibility, newVisibility);
                currentOrder = newOrder;
            });

            columnToggleContainer.querySelector('.column-toggle-dropdown').addEventListener('click', e => {
                e.stopPropagation();
            });
        }

        // Store reference for later use
        table.dtdInstance = {
            currentOrder: currentOrder,
            visibility: visibility,
            columnToggleContainer: columnToggleContainer
        };
    },

    // New method to apply data-column attributes to existing rows
    applyDataColumnToRows(table) {
        table.querySelectorAll('tbody tr').forEach(row => {
            Array.from(row.children).forEach((td, index) => {
                td.dataset.column = index;
            });
        });
    },

    createColumnToggleDropdown(headers, tableId, order, visibility) {
        const dropdown = document.createElement('div');
        dropdown.classList.add('dropdown');
        const orderedHeaders = order && order.length === headers.length
            ? order.map(index => headers.find(h => parseInt(h.dataset.column) === index))
            : headers;
        dropdown.innerHTML = `
            <button class="btn  dropdown-toggle colmDrag  text-muted" type="button" data-bs-toggle="dropdown" aria-expanded="false" >
                <i class="fas fa-columns me-2"></i>Columns
            </button>
            <div class="dropdown-menu column-toggle-dropdown p-2" style="min-width: 200px;">
                <div class="fw-bold mb-2 text-muted">Show/Hide & Reorder Columns</div>
                <div class="column-toggle-list">
                    ${orderedHeaders
                .map(header => `
                            <div class="column-toggle-item" data-column="${header.dataset.column}">
                                <div class="form-check d-flex align-items-center">
                                    <i class="fas fa-grip-vertical me-1 sortable-handle" style="cursor: move;"></i>
                                    <input class="form-check-input column-toggle ms-1 me-2" type="checkbox" id="col-${tableId}-${header.dataset.column}" data-column="${header.dataset.column}" ${visibility[header.dataset.column] === false ? '' : 'checked'}>
                                    <label class="form-check-label" for="col-${tableId}-${header.dataset.column}">${header.textContent.trim()}</label>
                                </div>
                            </div>
                        `)
                .join('')}
                </div>
                <hr class="my-2">
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" id="selectAllColumns">Select All</button>
                    <button class="btn btn-sm btn-outline-secondary" id="resetColumns">Reset</button>
                </div>
            </div>
        `;
        console.log(`Created dropdown for ${tableId} with visibility: ${JSON.stringify(visibility)}`);
        return dropdown;

        //<button class="btn btn-light dropdown-toggle bg-white text-muted" type="button" data-bs-toggle="dropdown" aria-expanded="false" style="border: 1px solid #cbd0dd;color: #979aa3 !important;">

    },

    toggleColumn(table, columnIndex, isVisible) {
        const headers = table.querySelectorAll(`th[data-column="${columnIndex}"]`);
        const cells = table.querySelectorAll(`td[data-column="${columnIndex}"]`);
        console.log(`Toggling column ${columnIndex} in table ${table.id}: headers=${headers.length}, cells=${cells.length}, isVisible=${isVisible}`);
        if (isVisible) {
            headers.forEach(header => {
                header.style.display = '';
                console.log(`Showing header ${columnIndex} in table ${table.id}`);
            });
            cells.forEach(cell => {
                cell.style.display = '';
                console.log(`Showing cell ${columnIndex} in table ${table.id}`);
            });
        } else {
            headers.forEach(header => {
                header.style.display = 'none';
                console.log(`Hiding header ${columnIndex} in table ${table.id}`);
            });
            cells.forEach(cell => {
                cell.style.display = 'none';
                console.log(`Hiding cell ${columnIndex} in table ${table.id}`);
            });
        }
    },

    reorderColumns(table, order) {
        const headerRow = table.querySelector('thead tr');
        const headers = Array.from(headerRow.querySelectorAll('th'));
        const rows = table.querySelectorAll('tbody tr');

        const sortedHeaders = order.map(index => headers.find(h => parseInt(h.dataset.column) === index));
        headerRow.innerHTML = '';
        sortedHeaders.forEach(header => header && headerRow.appendChild(header));

        rows.forEach(row => {
            const cells = Array.from(row.querySelectorAll('td'));
            row.innerHTML = '';
            order.forEach(index => {
                const cell = cells.find(c => parseInt(c.dataset.column) === index);
                if (cell) row.appendChild(cell);
            });
        });
        console.log(`Reordered columns for table ${table.id}:`, order);
    },

    updateDropdownOrder(container, order, visibility) {
        const dropdownList = container.querySelector('.column-toggle-list');
        const items = Array.from(dropdownList.querySelectorAll('.column-toggle-item'));
        dropdownList.innerHTML = '';
        order.forEach(index => {
            const item = items.find(i => parseInt(i.dataset.column) === index);
            if (item) {
                const checkbox = item.querySelector('.column-toggle');
                checkbox.checked = visibility.hasOwnProperty(index) ? visibility[index] : true;
                dropdownList.appendChild(item);
                console.log(`Updated dropdown item for column ${index}: checked=${checkbox.checked}`);
            }
        });
        console.log(`Updated dropdown order for table:`, order);
    },

    loadColumnSettings(tableId) {
        try {
            const saved = localStorage.getItem(`${tableId}_columnSettings`);
            if (!saved) {
                console.log(`No settings found for ${tableId}`);
                return { visibility: {}, order: null };
            }
            const data = JSON.parse(saved);
            if (!data.visibility || !data.order || !data.expiry) {
                console.warn(`Invalid settings format for ${tableId}`);
                localStorage.removeItem(`${tableId}_columnSettings`);
                return { visibility: {}, order: null };
            }
            if (Date.now() > data.expiry) {
                console.log(`Settings for ${tableId} expired`);
                localStorage.removeItem(`${tableId}_columnSettings`);
                return { visibility: {}, order: null };
            }
            console.log(`Loaded valid settings for ${tableId}:`, data);
            return { visibility: data.visibility, order: data.order };
        } catch (e) {
            console.error(`Failed to load settings for ${tableId}:`, e);
            return { visibility: {}, order: null };
        }
    },

    saveColumnSettings(tableId, visibility, order) {
        try {
            if (!order || !Array.isArray(order) || order.length === 0) {
                console.warn(`Invalid order array for ${tableId}, resetting to default`);
                order = Array.from(document.getElementById(tableId).querySelectorAll('thead th')).map((_, i) => i);
            }
            const cleanVisibility = {};
            Object.entries(visibility).forEach(([key, value]) => {
                if (!isNaN(key) && typeof value === 'boolean') {
                    cleanVisibility[key] = value;
                }
            });
            const oneYearFromNow = Date.now() + 365 * 24 * 60 * 60 * 1000;
            const data = { visibility: cleanVisibility, order, expiry: oneYearFromNow };
            localStorage.setItem(`${tableId}_columnSettings`, JSON.stringify(data));
            console.log(`Saved settings for ${tableId}:`, data);
        } catch (e) {
            console.error(`Failed to save settings for ${tableId}:`, e);
            alert('Failed to save table settings. Check browser storage settings.');
        }
    },

   
    applyColumnSettingsToNewRows(table, tableId) {
        if (!table.dtdInstance) {
            console.warn(`Table ${tableId} not initialized with DynamicTableDrag`);
            return;
        }

        const { currentOrder, visibility } = table.dtdInstance;
        console.log(`Applying settings to new rows for ${tableId}: visibility=${JSON.stringify(visibility)}, order=${JSON.stringify(currentOrder)}`);

        // Apply data-column attributes to all rows
        this.applyDataColumnToRows(table);

        // Reorder columns if order is defined
        if (currentOrder && currentOrder.length > 0) {
            this.reorderColumns(table, currentOrder);
        }

        // Apply visibility settings
        Object.entries(visibility).forEach(([columnIndex, isVisible]) => {
            if (!isVisible) {
                this.toggleColumn(table, columnIndex, false);
            }
        });
    },

    // Public method to refresh table settings after data changes
    refreshTableSettings(tableId) {
        const table = document.getElementById(tableId);
        if (table && table.dtdInstance) {
            this.applyColumnSettingsToNewRows(table, tableId);
            console.log(`Refreshed settings for table ${tableId}`);
        } else {
            console.warn(`Table ${tableId} not found or not initialized`);
        }
    }
};


$(document).ready(function () {
    console.log('Document ready, initializing DynamicTableDrag at ' + new Date().toISOString());
    DynamicTableDrag.init();
});


//#endregion








