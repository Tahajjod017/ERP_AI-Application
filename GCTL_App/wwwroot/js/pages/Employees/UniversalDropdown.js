class UniversalDropdown {
    constructor(elementId, apiRoute, config = {}) {
        this.elementId = elementId;
        this.apiRoute = apiRoute;
        this.debounceTimer = null;
        this.loading = false;
        this.currentPage = 1;
        this.lastSearch = '';
        this.hasMore = true;
        this.choicesInstance = null;
        this.initialLoadDone = false;

        // Default config with overrides
        this.config = {
            searchEnabled: true,
            placeholder: true,
            placeholderValue: 'Select an option...',
            searchPlaceholderValue: 'Type to search...',
            noChoicesText: 'Type 3 or more characters...',
            searchResultLimit: -1, // Disable local limiting
            shouldSort: false,
            duplicateItemsAllowed: false,
            itemSelectText: '',
            removeItemButton: true,
            searchChoices: false, // Disable client-side filtering
            fuseOptions: false,
            searchFn: () => true,
            pageSize: 50,
            debounceDelay: 500,
            ...config
        };

        this.init();
    }

    // Initialize the dropdown
    init() {
        const selectEl = document.getElementById(this.elementId);
        if (!selectEl) {
            console.error(`Element with ID ${this.elementId} not found`);
            return;
        }

        this.choicesInstance = new Choices(selectEl, this.config);

        // Attach event listeners
        selectEl.addEventListener('search', this.handleSearch.bind(this));
        selectEl.addEventListener('showDropdown', this.handleShowDropdown.bind(this));
    }

    // Fetch data from server
    async fetchOptions(search, page = 1) {
        this.loading = true;
        try {
            const res = await fetch(`${this.apiRoute}?search=${encodeURIComponent(search)}&page=${page}&pageSize=${this.config.pageSize}`);
            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`);
            }
            const data = await res.json();
            this.hasMore = data.hasMore;
            return data;
        } catch (error) {
            console.error(`Error fetching data for ${this.elementId}:`, error);
            return { items: [], hasMore: false };
        } finally {
            this.loading = false;
        }
    }

    // Handle search event with debounce
    async handleSearch(e) {
        const searchTerm = e.detail.value;
        clearTimeout(this.debounceTimer);

        if (searchTerm.length < 3) {
            this.choicesInstance.clearChoices();
            if (searchTerm.length === 0 && this.initialLoadDone) {
                this.currentPage = 1;
                this.lastSearch = '';
                const data = await this.fetchOptions('', 1);
                if (data.items && data.items.length > 0) {
                    this.choicesInstance.setChoices(data.items, 'value', 'label', true);
                }
            } else {
                this.choicesInstance.setChoices([{
                    value: '',
                    label: 'Type 3 or more characters...',
                    disabled: true
                }], 'value', 'label', true);
            }
            return;
        }

        this.debounceTimer = setTimeout(async () => {
            this.currentPage = 1;
            this.lastSearch = searchTerm;
            this.choicesInstance.clearChoices();
            this.choicesInstance.setChoices([{
                value: '',
                label: 'Searching...',
                disabled: true
            }], 'value', 'label', true);

            const data = await this.fetchOptions(searchTerm, this.currentPage);
            this.choicesInstance.clearChoices();
            if (data.items && data.items.length > 0) {
                this.choicesInstance.setChoices(data.items, 'value', 'label', true);
            } else {
                this.choicesInstance.setChoices([{
                    value: '',
                    label: 'No results found',
                    disabled: true
                }], 'value', 'label', true);
            }
        }, this.config.debounceDelay);
    }

    // Handle scroll for infinite loading
    async handleScroll(e) {
        const dropdownList = e.target;
        if (!this.loading && this.hasMore && dropdownList.scrollTop + dropdownList.clientHeight >= dropdownList.scrollHeight - 10) {
            this.currentPage++;
            const data = await this.fetchOptions(this.lastSearch, this.currentPage);
            if (data.items && data.items.length > 0) {
                this.choicesInstance.setChoices(data.items, 'value', 'label', false);
            }
        }
    }

    // Handle dropdown open
    async handleShowDropdown() {
        const dropdownList = document.querySelector(`#${this.elementId} + .choices__list--dropdown .choices__list[role="listbox"]`);
        if (dropdownList) {
            dropdownList.removeEventListener('scroll', this.handleScroll.bind(this));
            dropdownList.addEventListener('scroll', this.handleScroll.bind(this));
        }

        if (!this.initialLoadDone) {
            this.currentPage = 1;
            this.lastSearch = '';
            const data = await this.fetchOptions('', 1);
            if (data.items && data.items.length > 0) {
                this.choicesInstance.setChoices(data.items, 'value', 'label', true);
            }
            this.initialLoadDone = true;
        }
    }

    // Load single item by ID
    async loadItemById(itemId) {
        try {
            const res = await fetch(`${this.apiRoute}/GetById?id=${itemId}`);
            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`);
            }
            const item = await res.json();
            if (item && this.choicesInstance) {
                this.choicesInstance.setChoices([{
                    value: item.id,
                    label: item.name,
                    selected: true
                }], 'value', 'label', false);
                this.choicesInstance.setChoiceByValue(itemId.toString());
            }
        } catch (error) {
            console.error(`Error loading item by ID for ${this.elementId}:`, error);
        }
    }

    // Get last integer from URL
    static getLastIntFromUrl() {
        const parts = window.location.pathname.split('/').filter(Boolean).reverse();
        return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
    }
}

// Usage example:
// Initialize a dropdown
// const employeeDropdown = new UniversalDropdown('EmployeePersonalId', '/EmployeePersonal/SearchEmployeeDD');
// const departmentDropdown = new UniversalDropdown('DepartmentId', '/Department/SearchDepartmentDD', { placeholderValue: 'Select Department...' });

// Load item from URL if present
// const lastInt = UniversalDropdown.getLastIntFromUrl();
// if (lastInt) {
//     employeeDropdown.loadItemById(lastInt).then(() => {
//         // Additional logic after loading
//     });
// }