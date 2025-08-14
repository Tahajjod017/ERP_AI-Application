//#region  Example Section


//<select class="form-select choiceDD" asp-for="LicenceTypeID" asp-items="@ViewBag.LicenseTypeDD as SelectList">
//    <option value="">@Html.SmartLocalize("Select License Type")</option>
//</select>


//choiceManager.clearChoice('LicenceTypeID');
//choiceManager.clearChoice('LicenceTypeID', 'LicenceTypeID');
//choiceManager.setChoiceValue('LicenceTypeID', '5');
//choiceManager.setChoiceValue('LicenceTypeID', '');
//choiceManager.getChoiceValue('EmployeePersonalId')


//choiceManager.resetChoice('dropdown1', 'dropdown2'); // Method 1: Complete reset (destroys and recreates)



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


const deb = false


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
                if (deb) console.warn('Choices dropdown must have an ID:', select);
                return;
            }

            const config = { ...this.defaultConfig };
            try {
                this.instances[id] = new Choices(select, config);
                if (deb) console.debug(`Choices initialized for ID: ${id}`);
            } catch (error) {
                console.error(`Failed to initialize Choices for ID: ${id}`, error);
            }
        });
    }


    //choiceManager.clearChoice('dropdown1');
    //choiceManager.clearChoice('dropdown1', 'dropdown2');

    clearChoice(...ids) {
        ids.forEach(id => {
            const instance = this.instances[id];
            if (instance) {
                instance.removeActiveItems();
                instance.setChoiceByValue('');
                $(`#${id}`).val('').trigger('change');
                if (deb) console.debug(`Cleared choice for ID: ${id}`);
            } else {
                $(`#${id}`).val('').trigger('change');
                if (deb) console.warn(`No Choices instance found for ID: ${id} during clearChoice`);
            }
        });
    }


    //choiceManager.setChoiceValue('dropdown1', '5');
    //choiceManager.setChoiceValue('dropdown1', '');


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
                if (deb) console.debug(`Set value ${strValue} for ID: ${id}`);
            });

            // Update the underlying <select> and trigger change
            $(`#${id}`).val(valueArray).trigger('change');
            if (deb) console.debug(`Set values ${valueArray.join(', ')} for ID: ${id}`);
        } else {
            // Fallback to jQuery if no Choices instance
            $(`#${id}`).val(valueArray).trigger('change');
            if (deb) console.warn(`No Choices instance found for ID: ${id} during setChoiceValue`);
        }
    }


    //#region Reset Choice


    //choiceManager.resetChoice('dropdown1');
    //choiceManager.resetChoice('dropdown1', 'dropdown2');
    
    resetChoice(...ids) {
        ids.forEach(id => {
            const selectElement = document.getElementById(id);
            if (!selectElement) {
                if (deb) console.warn(`No select element found for ID: ${id}`);
                return;
            }

            const instance = this.instances[id];
            if (instance) {
                // Destroy the current instance
                instance.destroy();
                if (deb) console.debug(`Destroyed Choices instance for ID: ${id}`);
            }

            // Reset the select element to its original state
            selectElement.selectedIndex = 0;
            $(selectElement).val('').trigger('change');

            // Recreate the Choices instance
            try {
                const config = { ...this.defaultConfig };
                this.instances[id] = new Choices(selectElement, config);
                if (deb) console.debug(`Recreated Choices instance for ID: ${id}`);
            } catch (error) {
                console.error(`Failed to recreate Choices for ID: ${id}`, error);
            }
        });
    }

    


    //#endregion


    //choiceManager.getChoiceValue('dropdown1')

    getChoiceValue(id) {
        const instance = this.instances[id];
        if (instance) {
            const selected = instance.getValue(true);
            if (deb) console.debug(`Got value ${selected} for ID: ${id}`);
            return selected;
        } else {
            const value = $(`#${id}`).val();
            if (deb) console.warn(`No Choices instance found for ID: ${id} during getChoiceValue, using jQuery value: ${value}`);
            return value;
        }
    }

    // Get placeholder from HTML select element
    getPlaceholderFromHtml(id) {
        const select = document.getElementById(id);
        if (!select) {
            if (deb) console.warn(`No select element found for ID: ${id}`);
            return null;
        }

        // Check for data-placeholder attribute
        const dataPlaceholder = select.getAttribute('data-placeholder');
        if (dataPlaceholder) {
            if (deb) console.debug(`Found data-placeholder for ID: ${id}: ${dataPlaceholder}`);
            return dataPlaceholder;
        }

        // Check for an option with empty value
        const placeholderOption = select.querySelector('option[value=""]');
        if (placeholderOption) {
            const text = placeholderOption.textContent.trim();
            if (deb) console.debug(`Found placeholder option for ID: ${id}: ${text}`);
            return text;
        }

        if (deb) console.debug(`No placeholder found in HTML for ID: ${id}`);
        return null;
    }


    

 
    //const departmentDataDummy = [
    //    { departmentID: 101, departmentName: "Engineering" },
    //    { departmentID: 102, departmentName: "Marketing" },
    //    { departmentID: 103, departmentName: "Sales" }
    //];


    //choiceManager.populateDropdown('dropdown1', departmentDataDummy);
    
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
                if (deb) console.debug(`Auto-detected keys for data: valueKey=${valueKey}, labelKey=${labelKey}`);
            } else if (keys.length === 1) {
                valueKey = valueKey || keys[0];
                labelKey = labelKey || keys[0];
                if (deb) console.debug(`Single key detected for data: valueKey=${valueKey}, labelKey=${labelKey}`);
            } else {
                if (deb) console.warn('No keys found in data for auto-detection');
                return;
            }
        }

        // Handle single ID or array of IDs
        const ids = Array.isArray(dropdownIds) ? dropdownIds : [dropdownIds];

        ids.forEach(id => {
            const instance = this.instances[id];
            if (!instance) {
                if (deb) console.warn(`No Choices instance found for ID: ${id} during populateDropdown`);
                return;
            }

            // Get placeholder from HTML or fall back to config
            const placeholder = this.getPlaceholderFromHtml(id) || mergedConfig.placeholder;
            if (deb) console.debug(`Using placeholder for ID: ${id}: ${placeholder}`);

            // Clear existing options and selections
            instance.clearStore();
            instance.removeActiveItems();
            if (deb) console.debug(`Cleared store and active items for ID: ${id}`);

            // Format data
            let formattedData;
            try {
                formattedData = data.map(item => ({
                    value: String(item[valueKey]),
                    label: String(item[labelKey]).trim(),
                    selected: item.selected || false,
                    disabled: item.disabled || false
                }));
                if (deb) console.debug(`Formatted data for ID: ${id}`, formattedData);
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
                if (deb) console.debug(`Populated dropdown for ID: ${id} with ${formattedData.length} options`);
            } catch (error) {
                console.error(`Error populating dropdown for ID: ${id}`, error);
            }
        });
    }

    disableChoice(id) {
        const instance = this.instances[id];
        if (instance) {
            instance.disable();
            // Manually add 'is-disabled' class if you need extra customization
            const container = document.querySelector(`#${id}`);
            if (container) {
                container.classList.add('is-disabled');
            }
            if (deb) console.debug(`Disabled choice for ID: ${id}`);
        } else {
            if (deb) console.warn(`No Choices instance found for ID: ${id} during disableChoice`);
        }
    }

    enableChoice(id) {
        const instance = this.instances[id];
        if (instance) {
            instance.enable();
            // Remove 'is-disabled' class if it was added
            const container = document.querySelector(`#${id}`);
            if (container) {
                container.classList.remove('is-disabled');
            }
            if (deb) console.debug(`Enabled choice for ID: ${id}`);
        } else {
            if (deb) console.warn(`No Choices instance found for ID: ${id} during enableChoice`);
        }
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
            if (deb) console.warn(`No Choices instance found for ID: ${id} in populateStatic`);
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
            if (deb) console.debug(`Set placeholder for ID: ${id}: ${placeholder}`);

            // Add options
            instance.setChoices(formattedOptions, 'value', 'label', true);
            if (deb) console.debug(`Set ${formattedOptions.length} options for ID: ${id}`);
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
            if (deb) console.warn(`No Choices instance found for ID: ${id} in populateFromApi`);
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
            if (deb) console.debug(`Populated ${formattedOptions.length} options from API for ID: ${id}`);
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
            if (deb) console.debug(`Cleared options for ID: ${id}`);
        } else {
            if (deb) console.warn(`No Choices instance found for ID: ${id} in clearOptions`);
        }
    }

    addOption(id, config = {}) {
        const instance = this.choiceManager.instances[id];
        if (!instance) {
            if (deb) console.warn(`No Choices instance found for ID: ${id} in addOption`);
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
            if (deb) console.debug(`Added option ${label} for ID: ${id}`);
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
//                if (deb) console.warn('Choices dropdown must have an ID:', select);
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
//            if (!id) return if (deb) console.warn('Choices dropdown must have an ID:', select);

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

