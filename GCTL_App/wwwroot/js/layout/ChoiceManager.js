//<select class="form-select choiceDD" asp-for="LicenceTypeID" asp-items="@ViewBag.LicenseTypeDD as SelectList">
//    <option value="">@Html.SmartLocalize("Select License Type")</option>
//</select>


//choiceManager.clearChoice('LicenceTypeID');
//choiceManager.setChoiceValue('LicenceTypeID', '5');


//#region Universal Choices Dropdown Manager

class UniversalChoices {
    constructor(className = 'choiceDD') {

        this.className = className;
        this.instances = {};
        this.defaultConfig = {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select an option'
        };
    }

    initAll() {
        document.querySelectorAll(`select.${this.className}`).forEach(select => {
            const id = select.id;
            if (!id) return console.warn('Choices dropdown must have an ID:', select);

            const config = { ...this.defaultConfig };
            this.instances[id] = new Choices(`#${id}`, config);
        });
    }

    //clearChoice(id) {
    //    const instance = this.instances[id];
    //    if (instance) {
    //        instance.removeActiveItems();
    //        instance.setChoiceByValue('');
    //        $(`#${id}`).val('').trigger('change');
    //    } else {
    //        $(`#${id}`).val('').trigger('change');
    //    }
    //}

    clearChoice(...ids) {
        ids.forEach(id => {
            const instance = this.instances[id];
            if (instance) {
                instance.removeActiveItems();
                instance.setChoiceByValue('');
                $(`#${id}`).val('').trigger('change');
            } else {
                $(`#${id}`).val('').trigger('change');
            }
        });
    }


   


    setChoiceValue(id, value) {
        value = String(value); // Ensure value is converted to string
        const instance = this.instances[id];

        if (instance) {
            instance.setChoiceByValue(value);
            $(`#${id}`).val(value).trigger('change');
        } else {
            $(`#${id}`).val(value).trigger('change');
        }
    }


}

// Initialize on page load
const choiceManager = new UniversalChoices();
window.addEventListener('DOMContentLoaded', () => {
    choiceManager.initAll();
});

// Optional global access if needed later
window.choiceManager = choiceManager;

//#endregion
