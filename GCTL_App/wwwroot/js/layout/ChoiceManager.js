//<select class="form-select choiceDD" asp-for="LicenceTypeID" asp-items="@ViewBag.LicenseTypeDD as SelectList">
//    <option value="">@Html.SmartLocalize("Select License Type")</option>
//</select>


//choiceManager.clearChoice('LicenceTypeID');
//choiceManager.setChoiceValue('LicenceTypeID', '5');

//#region Unversal Choice Version 2
class UniversalChoices {
    constructor(className = 'choiceDD') {
        this.className = className;
        this.instances = {};
        this.defaultConfig = {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select an option',
            allowHTML: true // ✅ Suppress deprecation warning
        };
    }

    initAll() {
        document.querySelectorAll(`select.${this.className}`).forEach(select => {
            const id = select.id;
            if (!id) {
                console.warn('Choices dropdown must have an ID:', select);
                return;
            }

            const config = { ...this.defaultConfig };
            this.instances[id] = new Choices(select, config); // ✅ Pass actual element
        });
    }

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
        value = String(value); // Ensure value is a string
        const instance = this.instances[id];

        if (instance) {
            instance.setChoiceByValue(value);
            $(`#${id}`).val(value).trigger('change');
        } else {
            $(`#${id}`).val(value).trigger('change');
        }
    }

    getChoiceValue(id) {
        const instance = this.instances[id];
        if (instance) {
            const selected = instance.getValue(true); // `true` returns raw value(s)
            return selected;
        } else {
           
            return $(`#${id}`).val(); // or document.getElementById(id).value
        }
    }



}

// ✅ Initialize on page load
const choiceManager = new UniversalChoices();
window.addEventListener('DOMContentLoaded', () => {
    choiceManager.initAll();
});

// ✅ Optional global access
window.choiceManager = choiceManager;

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

