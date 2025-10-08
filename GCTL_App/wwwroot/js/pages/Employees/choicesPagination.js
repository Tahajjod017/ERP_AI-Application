function initPaginatedChoices(config) {
    const {
        selectId,
        endpoint,
        placeholderText,
        minSearchLength = 3,
        pageSize = 50,
        dependencies = [],
        customParams = {}
    } = config;

    const selectEl = document.getElementById(selectId);
    if (!selectEl) {
        console.error(`${selectId} select element not found`);
        return null;
    }

    let choicesInstance = window.choiceManager?.instances[selectId];
    if (!choicesInstance) {
        console.warn(`No Choices instance found for ${selectId} in choiceManager. Initializing new instance.`);
        try {
            choicesInstance = new Choices(selectEl, {
                removeItemButton: true,
                shouldSort: false,
                placeholderValue: placeholderText,
                allowHTML: true,
                searchEnabled: true,
                placeholder: true,
                searchPlaceholderValue: 'Type to search...',
                noChoicesText: `Type ${minSearchLength} or more characters...`,
                searchResultLimit: -1,
                duplicateItemsAllowed: false,
                itemSelectText: '',
                searchChoices: false
            });
            if (window.choiceManager) {
                window.choiceManager.instances[selectId] = choicesInstance;
                console.debug(`Registered new Choices instance for ${selectId} in choiceManager`);
            }
        } catch (error) {
            console.error(`Failed to initialize Choices for ${selectId}:`, error);
            return null;
        }
    }

    let debounceTimer;
    let loading = false;
    let currentPage = 1;
    let lastSearch = '';
    let hasMore = true;
    let initialLoadDone = false;
    let scrollListenerAttached = false;

    function areDependenciesSet() {
        return dependencies.every(dep => {
            const value = window.choiceManager?.getChoiceValue(dep);
            return value && value.trim() !== '';
        });
    }

    async function fetchOptions(search, page = 1) {
        if (loading) return { items: [], hasMore: false };

        if (dependencies.length > 0 && !areDependenciesSet()) {
            choicesInstance.clearStore();
            choicesInstance.setChoices([{
                value: '',
                label: `Please select ${dependencies.join(', ')} first`,
                disabled: true
            }], 'value', 'label', true);
            return { items: [], hasMore: false };
        }

        loading = true;
        try {
            let queryParams = {
                search: encodeURIComponent(search),
                page,
                pageSize,
                ...customParams
            };

            dependencies.forEach(dep => {
                const value = window.choiceManager?.getChoiceValue(dep) || '';
                queryParams[dep.toLowerCase()] = encodeURIComponent(value);
            });

            const queryString = Object.entries(queryParams)
                .map(([key, value]) => `${key}=${value}`)
                .join('&');
            const url = `${endpoint}?${queryString}`;

            console.debug(`Fetching options for ${selectId} from ${url} (page ${page})`);
            const res = await fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            });

            if (!res.ok) {
                const errorText = await res.text();
                console.error(`HTTP error for ${selectId}: Status ${res.status}, Response: ${errorText}`);
                throw new Error(`HTTP error! Status: ${res.status}`);
            }

            let data;
            try {
                data = await res.json();
            } catch (jsonError) {
                console.error(`Failed to parse JSON for ${selectId}:`, jsonError);
                throw new Error('Invalid JSON response');
            }

            console.debug(`Received response for ${selectId} (page ${page}):`, data);

            const items = data.items || (Array.isArray(data) ? data : []);
            if (!Array.isArray(items)) {
                console.error(`Expected an array of items for ${selectId}, got:`, data);
                throw new Error(`Expected an array of items`);
            }

            const formattedItems = items.map(item => ({
                value: String(item.value),
                label: item.label,
                selected: item.selected || false,
                disabled: item.disabled || false
            }));

            // For page 1, clear and set new choices
            if (page === 1) {
                choicesInstance.clearStore();
                choicesInstance.setChoices([{
                    value: '',
                    label: placeholderText,
                    disabled: true
                }], 'value', 'label', false);
            }

            // Add new items
            if (formattedItems.length > 0) {
                choicesInstance.setChoices(formattedItems, 'value', 'label', false);
            }

            hasMore = data.hasMore !== undefined ? data.hasMore : (items.length === pageSize);
            console.debug(`Populated ${items.length} options for ${selectId}, hasMore: ${hasMore}`);

            return { items, hasMore };
        } catch (error) {
            console.error(`Error fetching options for ${selectId}:`, error);
            choicesInstance.clearStore();
            choicesInstance.setChoices([{
                value: '',
                label: 'Error loading data',
                disabled: true
            }], 'value', 'label', true);
            return { items: [], hasMore: false };
        } finally {
            loading = false;
        }
    }

    selectEl.addEventListener('search', function (e) {
        const searchTerm = e.detail.value;
        clearTimeout(debounceTimer);

        if (searchTerm.length < minSearchLength) {
            choicesInstance.clearStore();
            if (searchTerm.length === 0) {
                currentPage = 1;
                lastSearch = '';
                hasMore = true;
                fetchOptions('', 1).then(data => {
                    if (!data.items || data.items.length === 0) {
                        choicesInstance.setChoices([{
                            value: '',
                            label: 'No options available',
                            disabled: true
                        }], 'value', 'label', true);
                    }
                });
                return;
            }
            choicesInstance.setChoices([{
                value: '',
                label: `Type ${minSearchLength} or more characters...`,
                disabled: true
            }], 'value', 'label', true);
            return;
        }

        debounceTimer = setTimeout(async () => {
            currentPage = 1;
            lastSearch = searchTerm;
            hasMore = true;
            choicesInstance.clearStore();
            choicesInstance.setChoices([{
                value: '',
                label: 'Searching...',
                disabled: true
            }], 'value', 'label', true);
            await fetchOptions(searchTerm, currentPage);
        }, 500);
    });

    async function handleScroll(e) {
        const dropdownList = e.target;
        console.debug(`Scroll event for ${selectId}: scrollTop=${dropdownList.scrollTop}, clientHeight=${dropdownList.clientHeight}, scrollHeight=${dropdownList.scrollHeight}, hasMore=${hasMore}, loading=${loading}`);

        if (!loading && hasMore && dropdownList.scrollTop + dropdownList.clientHeight >= dropdownList.scrollHeight - 50) {
            console.debug(`Loading next page (${currentPage + 1}) for ${selectId}`);
            currentPage++;
            await fetchOptions(lastSearch, currentPage);
        }
    }

    function attachScrollListener() {
        const dropdownList = document.querySelector(`#${selectId} + .choices .choices__list--dropdown .choices__list[role="listbox"]`) ||
            document.querySelector(`.choices[data-type*="select-one"] .choices__list--dropdown .choices__list[role="listbox"]`);

        if (dropdownList && !scrollListenerAttached) {
            console.debug(`Attaching scroll listener for ${selectId}`, dropdownList);
            dropdownList.removeEventListener('scroll', handleScroll);
            dropdownList.addEventListener('scroll', handleScroll);
            dropdownList.style.maxHeight = '200px';
            dropdownList.style.overflowY = 'auto';
            scrollListenerAttached = true;
        } else if (!dropdownList) {
            console.warn(`Dropdown list not found for ${selectId}`);
        }
    }

    selectEl.addEventListener('showDropdown', async () => {
        console.debug(`Dropdown opened for ${selectId}`);

        // Always try to attach scroll listener when dropdown opens
        attachScrollListener();

        if (!initialLoadDone) {
            currentPage = 1;
            lastSearch = '';
            hasMore = true;
            console.debug(`Initial load for ${selectId}`);
            await fetchOptions('', 1);
            initialLoadDone = true;
        }
    });

    // ✅ Return object with instance and refresh method
    return {
        instance: choicesInstance,
        refresh: async function () {
            console.debug(`Refreshing ${selectId}`);
            currentPage = 1;
            lastSearch = '';
            hasMore = true;
            initialLoadDone = false;
            scrollListenerAttached = false;

            if (dependencies.length > 0 && !areDependenciesSet()) {
                choicesInstance.clearStore();
                choicesInstance.setChoices([{
                    value: '',
                    label: `Please select ${dependencies.join(', ')} first`,
                    disabled: true
                }], 'value', 'label', true);
                return;
            }

            choicesInstance.clearStore();
            choicesInstance.setChoices([{
                value: '',
                label: 'Loading...',
                disabled: true
            }], 'value', 'label', true);

            await fetchOptions('', 1);
            initialLoadDone = true;
        },
        reset: function () {
            choicesInstance.clearStore();
            choicesInstance.setChoices([{
                value: '',
                label: placeholderText,
                disabled: true
            }], 'value', 'label', true);
            currentPage = 1;
            lastSearch = '';
            hasMore = true;
            initialLoadDone = false;
            scrollListenerAttached = false;
        }
    };
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { initPaginatedChoices };
} else {
    window.initPaginatedChoices = initPaginatedChoices;
}