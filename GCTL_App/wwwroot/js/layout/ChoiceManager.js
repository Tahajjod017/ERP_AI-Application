//#region  Example Section


//<select class="form-select choiceDD" asp-for="LicenceTypeID" asp-items="@ViewBag.LicenseTypeDD as SelectList">
//    <option value="">@Html.SmartLocalize("Select License Type")</option>
//</select>


//choiceManager.clearChoice('LicenceTypeID');
//choiceManager.clearChoice('LicenceTypeID', 'LicenceTypeID');
//choiceManager.setChoiceValue('LicenceTypeID', '5');
//choiceManager.setChoiceValue('LicenceTypeID', '');
//choiceManager.getChoiceValue('EmployeePersonalId')

// Example: Populate multiple dropdowns with auto-detected keys
const branchDataDummy = [
    { organizationBranchID: 1, organizationBranchName: "Googleplex" },
    { organizationBranchID: 2, organizationBranchName: "New York, USA" },
    { organizationBranchID: 3, organizationBranchName: "London, UK" },
    { organizationBranchID: 4, organizationBranchName: "Singapore" }
];

// Placeholder will be taken from HTML
//choiceManager.populateDropdown(['table1BranchDropdown', 'table2BranchDropdown'], branchDataDummy);

// Example: Populate department dropdown with auto-detected keys
const departmentDataDummy = [
    { departmentID: 101, departmentName: "Engineering" },
    { departmentID: 102, departmentName: "Marketing" },
    { departmentID: 103, departmentName: "Sales" }
];

//// Placeholder will be taken from HTML
//choiceManager.populateDropdown('table3DepartmentDropdown', departmentDataDummy);

//// Example: Override placeholder if needed
//choiceManager.populateDropdown('table4BranchDropdown', branchDataDummy, {
//    placeholder: 'Custom Select a Branch'
//});

//#endregion





//#region Unversal Choice Version 3

class UniversalChoices {
    constructor(className = 'choiceDD') {
        this.className = className;
        this.instances = {};
        this.defaultConfig = {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select one', // Default placeholder if none in HTML
            allowHTML: true
        };
        this.populator = new ChoicePopulator(this);
    }

    initAll() {
        document.querySelectorAll(`select.${this.className}`).forEach(select => {
            const id = select.id;
            if (!id) {
                console.warn('Choices dropdown must have an ID:', select);
                return;
            }

            const config = { ...this.defaultConfig };
            try {
                this.instances[id] = new Choices(select, config);
                console.debug(`Choices initialized for ID: ${id}`);
            } catch (error) {
                console.error(`Failed to initialize Choices for ID: ${id}`, error);
            }
        });
    }

    clearChoice(...ids) {
        ids.forEach(id => {
            const instance = this.instances[id];
            if (instance) {
                instance.removeActiveItems();
                instance.setChoiceByValue('');
                $(`#${id}`).val('').trigger('change');
                console.debug(`Cleared choice for ID: ${id}`);
            } else {
                $(`#${id}`).val('').trigger('change');
                console.warn(`No Choices instance found for ID: ${id} during clearChoice`);
            }
        });
    }

    //setChoiceValue(id, value) {
    //    value = String(value);
    //    const instance = this.instances[id];

    //    if (instance) {
    //        instance.setChoiceByValue(value);
    //        $(`#${id}`).val(value).trigger('change');
    //        console.debug(`Set value ${value} for ID: ${id}`);
    //    } else {
    //        $(`#${id}`).val(value).trigger('change');
    //        console.warn(`No Choices instance found for ID: ${id} during setChoiceValue`);
    //    }
    //}


    setChoiceValue(id, values) {
        // Ensure values is an array, even for single value
        const valueArray = Array.isArray(values) ? values : [values];
        const instance = this.instances[id];

        if (instance) {
            // Clear existing selections
            instance.removeActiveItems();

            // Set multiple values for multi-select
            valueArray.forEach(value => {
                const strValue = String(value);
                instance.setChoiceByValue(strValue);
                console.debug(`Set value ${strValue} for ID: ${id}`);
            });

            // Update the underlying <select> and trigger change
            $(`#${id}`).val(valueArray).trigger('change');
            console.debug(`Set values ${valueArray.join(', ')} for ID: ${id}`);
        } else {
            // Fallback to jQuery if no Choices instance
            $(`#${id}`).val(valueArray).trigger('change');
            console.warn(`No Choices instance found for ID: ${id} during setChoiceValue`);
        }
    }

    getChoiceValue(id) {
        const instance = this.instances[id];
        if (instance) {
            const selected = instance.getValue(true);
            console.debug(`Got value ${selected} for ID: ${id}`);
            return selected;
        } else {
            const value = $(`#${id}`).val();
            console.warn(`No Choices instance found for ID: ${id} during getChoiceValue, using jQuery value: ${value}`);
            return value;
        }
    }

    // Get placeholder from HTML select element
    getPlaceholderFromHtml(id) {
        const select = document.getElementById(id);
        if (!select) {
            console.warn(`No select element found for ID: ${id}`);
            return null;
        }

        // Check for data-placeholder attribute
        const dataPlaceholder = select.getAttribute('data-placeholder');
        if (dataPlaceholder) {
            console.debug(`Found data-placeholder for ID: ${id}: ${dataPlaceholder}`);
            return dataPlaceholder;
        }

        // Check for an option with empty value
        const placeholderOption = select.querySelector('option[value=""]');
        if (placeholderOption) {
            const text = placeholderOption.textContent.trim();
            console.debug(`Found placeholder option for ID: ${id}: ${text}`);
            return text;
        }

        console.debug(`No placeholder found in HTML for ID: ${id}`);
        return null;
    }

    // Populate one or multiple dropdowns with auto-detected or specified keys
    populateDropdown(dropdownIds, data = [], config = {}) {
        const defaultConfig = {
            placeholder: this.defaultConfig.placeholderValue, // Use class default
            labelKey: null,
            valueKey: null
        };
        const mergedConfig = { ...defaultConfig, ...config };

        // Auto-detect keys if not provided
        let { labelKey, valueKey } = mergedConfig;
        if (data.length > 0 && (!labelKey || !valueKey)) {
            const keys = Object.keys(data[0]);
            if (keys.length >= 2) {
                valueKey = valueKey || keys[0];
                labelKey = labelKey || keys[1];
                console.debug(`Auto-detected keys for data: valueKey=${valueKey}, labelKey=${labelKey}`);
            } else if (keys.length === 1) {
                valueKey = valueKey || keys[0];
                labelKey = labelKey || keys[0];
                console.debug(`Single key detected for data: valueKey=${valueKey}, labelKey=${labelKey}`);
            } else {
                console.warn('No keys found in data for auto-detection');
                return;
            }
        }

        // Handle single ID or array of IDs
        const ids = Array.isArray(dropdownIds) ? dropdownIds : [dropdownIds];

        ids.forEach(id => {
            const instance = this.instances[id];
            if (!instance) {
                console.warn(`No Choices instance found for ID: ${id} during populateDropdown`);
                return;
            }

            // Get placeholder from HTML or fall back to config
            const placeholder = this.getPlaceholderFromHtml(id) || mergedConfig.placeholder;
            console.debug(`Using placeholder for ID: ${id}: ${placeholder}`);

            // Clear existing options and selections
            instance.clearStore();
            instance.removeActiveItems();
            console.debug(`Cleared store and active items for ID: ${id}`);

            // Format data
            let formattedData;
            try {
                formattedData = data.map(item => ({
                    value: String(item[valueKey]),
                    label: String(item[labelKey]).trim(),
                    selected: item.selected || false,
                    disabled: item.disabled || false
                }));
                console.debug(`Formatted data for ID: ${id}`, formattedData);
            } catch (error) {
                console.error(`Error formatting data for ID: ${id}`, error);
                return;
            }

            // Populate dropdown
            try {
                this.populator.populateStatic(id, formattedData, {
                    labelKey: 'label',
                    valueKey: 'value',
                    placeholder
                });
                console.debug(`Populated dropdown for ID: ${id} with ${formattedData.length} options`);
            } catch (error) {
                console.error(`Error populating dropdown for ID: ${id}`, error);
            }
        });
    }
}

class ChoicePopulator {
    constructor(choiceManager) {
        this.choiceManager = choiceManager;
        this.defaultFetchOptions = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        };
    }

    populateStatic(id, options, config = {}) {
        const {
            labelKey = 'label',
            valueKey = 'value',
            placeholder = 'Select one' // Fallback placeholder
        } = config;

        const instance = this.choiceManager.instances[id];
        if (!instance) {
            console.warn(`No Choices instance found for ID: ${id} in populateStatic`);
            return;
        }

        const formattedOptions = options.map(option => ({
            value: String(option[valueKey]),
            label: option[labelKey],
            selected: option.selected || false,
            disabled: option.disabled || false
        }));

        try {
            // Clear store explicitly
            instance.clearStore();

            // Always add placeholder as first option
            instance.setChoices([{
                value: '',
                label: placeholder,
                placeholder: true,
                selected: true // Ensure placeholder is selected by default
            }], 'value', 'label', false);
            console.debug(`Set placeholder for ID: ${id}: ${placeholder}`);

            // Add options
            instance.setChoices(formattedOptions, 'value', 'label', true);
            console.debug(`Set ${formattedOptions.length} options for ID: ${id}`);
        } catch (error) {
            console.error(`Error setting choices for ID: ${id}`, error);
        }
    }

    async populateFromApi(id, url, config = {}) {
        const {
            labelKey = 'label',
            valueKey = 'value',
            placeholder = 'Select one',
            fetchOptions = {}
        } = config;

        const instance = this.choiceManager.instances[id];
        if (!instance) {
            console.warn(`No Choices instance found for ID: ${id} in populateFromApi`);
            return;
        }

        try {
            const response = await fetch(url, {
                ...this.defaultFetchOptions,
                ...fetchOptions
            });

            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            const data = await response.json();

            const formattedOptions = data.map(item => ({
                value: String(item[valueKey]),
                label: item[labelKey],
                selected: item.selected || false,
                disabled: item.disabled || false
            }));

            instance.clearStore();

            // Always add placeholder
            instance.setChoices([{
                value: '',
                label: placeholder,
                placeholder: true,
                selected: true
            }], 'value', 'label', false);

            instance.setChoices(formattedOptions, 'value', 'label', true);
            console.debug(`Populated ${formattedOptions.length} options from API for ID: ${id}`);
            return true;
        } catch (error) {
            console.error(`Error populating choices for ID ${id} from ${url}:`, error);
            return false;
        }
    }

    clearOptions(id) {
        const instance = this.choiceManager.instances[id];
        if (instance) {
            instance.clearStore();
            console.debug(`Cleared options for ID: ${id}`);
        } else {
            console.warn(`No Choices instance found for ID: ${id} in clearOptions`);
        }
    }

    addOption(id, config = {}) {
        const instance = this.choiceManager.instances[id];
        if (!instance) {
            console.warn(`No Choices instance found for ID: ${id} in addOption`);
            return;
        }

        const {
            value,
            label,
            selected = false,
            disabled = false
        } = config;

        try {
            instance.setChoices([{
                value: String(value),
                label,
                selected,
                disabled
            }], 'value', true);
            console.debug(`Added option ${label} for ID: ${id}`);
        } catch (error) {
            console.error(`Error adding option for ID: ${id}`, error);
        }
    }
}

// Initialize on page load
const choiceManager = new UniversalChoices();
window.addEventListener('DOMContentLoaded', () => {
    choiceManager.initAll();


});

// Optional global access
window.choiceManager = choiceManager;

//#endregion



//#region Unversal Choice Version 2

//class UniversalChoices {
//    constructor(className = 'choiceDD') {
//        this.className = className;
//        this.instances = {};
//        this.defaultConfig = {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select an option',
//            allowHTML: true // ✅ Suppress deprecation warning
//        };
//    }

//    initAll() {
//        document.querySelectorAll(`select.${this.className}`).forEach(select => {
//            const id = select.id;
//            if (!id) {
//                console.warn('Choices dropdown must have an ID:', select);
//                return;
//            }

//            const config = { ...this.defaultConfig };
//            this.instances[id] = new Choices(select, config); // ✅ Pass actual element
//        });
//    }

//    clearChoice(...ids) {
//        ids.forEach(id => {
//            const instance = this.instances[id];
//            if (instance) {
//                instance.removeActiveItems();
//                instance.setChoiceByValue('');
//                $(`#${id}`).val('').trigger('change');
//            } else {
//                $(`#${id}`).val('').trigger('change');
//            }
//        });
//    }

//    setChoiceValue(id, value) {
//        value = String(value); // Ensure value is a string
//        const instance = this.instances[id];

//        if (instance) {
//            instance.setChoiceByValue(value);
//            $(`#${id}`).val(value).trigger('change');
//        } else {
//            $(`#${id}`).val(value).trigger('change');
//        }
//    }

//    getChoiceValue(id) {
//        const instance = this.instances[id];
//        if (instance) {
//            const selected = instance.getValue(true); // `true` returns raw value(s)
//            return selected;
//        } else {
           
//            return $(`#${id}`).val(); // or document.getElementById(id).value
//        }
//    }



//}

//// ✅ Initialize on page load
//const choiceManager = new UniversalChoices();
//window.addEventListener('DOMContentLoaded', () => {
//    choiceManager.initAll();
//});

//// ✅ Optional global access
//window.choiceManager = choiceManager;

//#endregion

//#region Universal Choices Dropdown Manager

//class UniversalChoices {
//    constructor(className = 'choiceDD') {

//        this.className = className;
//        this.instances = {};
//        this.defaultConfig = {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select an option'
//        };
//    }

//    initAll() {
//        document.querySelectorAll(`select.${this.className}`).forEach(select => {
//            const id = select.id;
//            if (!id) return console.warn('Choices dropdown must have an ID:', select);

//            const config = { ...this.defaultConfig };
//            this.instances[id] = new Choices(`#${id}`, config);
//        });
//    }

//    //clearChoice(id) {
//    //    const instance = this.instances[id];
//    //    if (instance) {
//    //        instance.removeActiveItems();
//    //        instance.setChoiceByValue('');
//    //        $(`#${id}`).val('').trigger('change');
//    //    } else {
//    //        $(`#${id}`).val('').trigger('change');
//    //    }
//    //}

//    clearChoice(...ids) {
//        ids.forEach(id => {
//            const instance = this.instances[id];
//            if (instance) {
//                instance.removeActiveItems();
//                instance.setChoiceByValue('');
//                $(`#${id}`).val('').trigger('change');
//            } else {
//                $(`#${id}`).val('').trigger('change');
//            }
//        });
//    }


   


//    setChoiceValue(id, value) {
//        value = String(value); // Ensure value is converted to string
//        const instance = this.instances[id];

//        if (instance) {
//            instance.setChoiceByValue(value);
//            $(`#${id}`).val(value).trigger('change');
//        } else {
//            $(`#${id}`).val(value).trigger('change');
//        }
//    }


//}


//const choiceManager = new UniversalChoices();
//window.addEventListener('DOMContentLoaded', () => {
//    choiceManager.initAll();
//});


//window.choiceManager = choiceManager;

//#endregion

