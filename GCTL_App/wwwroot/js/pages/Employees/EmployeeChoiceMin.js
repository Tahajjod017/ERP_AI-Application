




////#region Universal Choices Dropdown Manager

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

//    clearChoice(id) {
//        const instance = this.instances[id];
//        if (instance) {
//            instance.removeActiveItems();
//            instance.setChoiceByValue('');
//            $(`#${id}`).val('').trigger('change');
//        } else {
//            $(`#${id}`).val('').trigger('change');
//        }
//    }


//    // ✅ Add this method to support edit value setting
//    setChoiceValue(id, value) {
//        const instance = this.instances[id];
//        if (instance) {
//            instance.setChoiceByValue(value);
//            $(`#${id}`).val(value).trigger('change');
//        } else {
//            $(`#${id}`).val(value).trigger('change');
//        }
//    }


//}

//// Initialize on page load
//const choiceManager = new UniversalChoices();
//window.addEventListener('DOMContentLoaded', () => {
//    choiceManager.initAll();
//});

//// Optional global access if needed later
//window.choiceManager = choiceManager;

////#endregion









////#region Universal choice

//let globalChoices = {};

//function InitChoiceDD(elementId, config = {}) {
//    globalChoices[elementId] = new Choices(`#${elementId}`, {
//        removeItemButton: true,
//        shouldSort: false,
//        placeholderValue: 'Select an option',
//        ...config
//    });
//    return globalChoices[elementId];
//}


////InitChoiceDD('ConveyanceAllowancePercentage', { placeholderValue: 'Select %' });
////InitChoiceDD('MedicalAllowancePercentage', { placeholderValue: 'Select %' });
////InitChoiceDD('HouseRentAllowancePercentage', { placeholderValue: 'Select %' });



////#endregion

////#region Clear Coice

////clearChoiceDD('ConveyanceAllowancePercentage');
//function clearChoiceDD(elementId) {
//    const choicesInstance = globalChoices[elementId];

//    if (choicesInstance) {
//        choicesInstance.removeActiveItems();
//        choicesInstance.setChoiceByValue('');
//        $(`#${elementId}`).val('').trigger('change');
//    } else {
//        $(`#${elementId}`).val('').trigger('change');
//    }
//}





////#endregion