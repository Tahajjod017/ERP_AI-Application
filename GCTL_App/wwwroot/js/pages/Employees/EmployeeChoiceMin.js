//#region Universal choice

let globalChoices = {};

function InitChoiceDD(elementId, config = {}) {
    globalChoices[elementId] = new Choices(`#${elementId}`, {
        removeItemButton: true,
        shouldSort: false,
        placeholderValue: 'Select an option',
        ...config
    });
    return globalChoices[elementId];
}


//InitChoiceDD('ConveyanceAllowancePercentage', { placeholderValue: 'Select %' });
//InitChoiceDD('MedicalAllowancePercentage', { placeholderValue: 'Select %' });
//InitChoiceDD('HouseRentAllowancePercentage', { placeholderValue: 'Select %' });



//#endregion

//#region Clear Coice

//clearChoiceDD('ConveyanceAllowancePercentage');
function clearChoiceDD(elementId) {
    const choicesInstance = globalChoices[elementId];

    if (choicesInstance) {
        choicesInstance.removeActiveItems();
        choicesInstance.setChoiceByValue('');
        $(`#${elementId}`).val('').trigger('change');
    } else {
        $(`#${elementId}`).val('').trigger('change');
    }
}





//#endregion