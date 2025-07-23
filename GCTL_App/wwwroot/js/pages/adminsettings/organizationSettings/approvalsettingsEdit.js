//edit function
$(document).ready(function () {
    getOrgName();
    getApproverByOrgName();
    getApproverTypeName();



    $(document).on('click', '#edit_approval_settingBtn', function () {
        var approvalSettingID = $(this).data('id');

        // Start by fetching the approval setting data
        $.ajax({
            url: '/ApprovalSettings/EditApprovalSetting',
            type: 'GET',
            data: { id: approvalSettingID },
            success: function (data) {
                if (data) {
                    // First, set ApprovalTypeEditID immediately after data is fetched
                    
                    choiceManager.setChoiceValue('OrganizationeEditID', data.organizationID);

                    var orgId = data.organizationID;  // Get orgId after the organization is set
                    getApproverByOrgName(orgId, function () {
                        // After getApproverByOrgName is done, call getApproverTypeName
                        
                            // After all three functions complete, populate the fields
                            setApprovalSettings(data);
                       
                });
                   
                    choiceManager.setChoiceValue('ApprovalTypeEditID', data.approvalTypeID);
                } else {
                    alert('Error fetching approval setting data: ' + data.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error fetching approval setting data:", error);
            }
        });
    });

    // Handle the dynamic change of the organization selection
    $(document).on('change', '#OrganizationeEditID', function () {
        var orgId = $(this).val();  // Get the updated organization ID
        // Re-run the functions to populate the dependent fields
        getApproverByOrgName(orgId, function () {


            // Fetch the approval setting data again based on the new organization ID
            fetchApprovalSettingData(approvalSettingID);

        });
    });

    // Function to populate the rest of the approval settings after data is loaded
    function setApprovalSettings(data) {
        // Setting values for other fields
        $('#chkFirstEdit').prop('checked', true);
        choiceManager.enableChoice('selFirstEdit');
        choiceManager.setChoiceValue('selFirstEdit', data.firstApprovalID);

        if (data.isEnableSecondApproval === 'on') {
            $('#chkSecondEdit').prop('checked', true);
            choiceManager.enableChoice('selSecondEdit');
            choiceManager.setChoiceValue('selSecondEdit', data.secondApprovalID);
        } else {
            $('#chkSecondEdit').prop('checked', false);
        }

        if (data.isEnableThirdApproval === 'on') {
            $('#chkThirdEdit').prop('checked', true);
            choiceManager.enableChoice('selThirdEdit');
            choiceManager.setChoiceValue('selThirdEdit', data.thirdApprovalID);
        } else {
            $('#chkThirdEdit').prop('checked', false);
        }

        if (data.allowSelfApproval === 'on') {
            $('#chkSelfApprovalEdit').prop('checked', true);
            choiceManager.disableChoice('selSelfApprovalEdit');
        } else {
            $('#chkThirdEdit').prop('checked', false);
            choiceManager.setChoiceValue('selSelfApprovalEdit', data.selfExceptionApprovalID);
        }
    }

    // Function to get organization names and populate the dropdown
    function getOrgName(callback) {
        $.ajax({
            url: '/ApprovalSettings/GetChoiceOrgnization', // Adjust the URL to your update endpoint
            type: 'GET',
            success: function (data) {
                // Assuming the response has 'value' and 'text' properties
                const simplified = data.map(role => ({
                    value: role.value,
                    label: role.text
                }));

                // Populating the dropdown with simplified data
                choiceManager.populateDropdown('OrganizationeEditID', simplified);

                // Call the callback function once getOrgName completes
                if (callback) callback();  // Proceed with the next function (getApproverByOrgName)
            },
            error: function (xhr, status, error) {
                console.error("Error fetching organization data:", error);
            }
        });
    }

    // Function to get approvers by organization name
    function getApproverByOrgName(orgId, callback) {
        $.ajax({
            url: '/ApprovalSettings/GetDesignation2',  // Endpoint to fetch designations
            type: 'GET',
            data: { organizationId: orgId }, // Pass the selected organization ID
            success: function (data) {
                // Assuming the response has 'value' and 'text' properties
                const simplified = data.map(role => ({
                    value: role.value,  // Assuming 'value' is the key for the id
                    label: role.text    // Assuming 'text' is the key for the display text
                }));

                // Populating the dropdown with simplified data
                choiceManager.populateDropdown('selFirstEdit', simplified);
                choiceManager.populateDropdown('selSecondEdit', simplified);
                choiceManager.populateDropdown('selThirdEdit', simplified);
                choiceManager.populateDropdown('selSelfApprovalEdit', simplified);

                // Call the callback function once getApproverByOrgName completes
                if (callback) callback();  // Proceed with the next function (getApproverTypeName)
            },
            error: function (xhr, status, error) {
                console.error("Error fetching approver data:", error);
            }
        });
    }

    // Function to get approver type names
    function getApproverTypeName(callback) {
        $.ajax({
            url: '/ApprovalSettings/GetappvalTypes', // Adjust the URL to your update endpoint
            type: 'GET',
            success: function (data) {
                // Assuming the response has 'value' and 'text' properties
                const simplified = data.map(role => ({
                    value: role.value,  // Assuming 'value' is the key for the id
                    label: role.text    // Assuming 'text' is the key for the display text
                }));

                // Populating the dropdown with simplified data
                choiceManager.populateDropdown('ApprovalTypeEditID', simplified);

                // Call the callback function once getApproverTypeName completes
                if (callback) callback();  // Proceed with setting the values
            },
            error: function (xhr, status, error) {
                console.error("Error fetching approver type data:", error);
            }
        });
    }



});



