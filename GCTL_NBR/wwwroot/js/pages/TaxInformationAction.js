$(document).ready(function () {
   
    //alert();
    //    const taxpayerID = getQueryParam("taxpayerID");
    //    if (taxpayerID) {
    //        $('#TaxpayerID').val(taxpayerID);
    //    }


    //    const urlParams = new URLSearchParams(window.location.search);
    //    const taxpayerID = urlParams.get('taxpayerID');

    //    // If taxpayerID exists, append it to the href of all links
    //    if (taxpayerID) {
    //        // Loop through each <a> tag inside the nav items
    //        $('ul#myTab li a').each(function () {
    //            let currentHref = $(this).attr('href');


    //            if (!currentHref.includes('taxpayerID')) {
    //                const newHref = currentHref + (currentHref.includes('?') ? '&' : '?') + 'taxpayerID=' + taxpayerID;
    //                $(this).attr('href', newHref);
    //            }
    //        });
    //    }



    const taxpayerIDStr = getQueryParam("taxpayerID");
    if (taxpayerIDStr) {
        $('#taxPayerId').val(taxpayerIDStr);

        
        $('ul#myTab li a').each(function () {
            let currentHref = $(this).attr('href');
            if (!currentHref.includes('taxpayerID')) {
                const newHref = currentHref + (currentHref.includes('?') ? '&' : '?') + 'taxpayerID=' + taxpayerIDStr;
                $(this).attr('href', newHref);
            }
        });
    }

    const taxpayerID = parseInt(taxpayerIDStr, 10) || 0; // Ensure taxpayerID is a number

    
   


    GetTaxPayerInfo(taxpayerID);


    function getQueryParam(name) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    }


    function populateDropdown(dropdownId, apiUrl, defaultText = "-- নির্বাচন করুন --") {
        $.ajax({
            url: apiUrl,
            type: "GET",
            dataType: "json",
            success: function (response) {
                var $dropdown = $("#" + dropdownId);
                $dropdown.empty(); // Clear previous options
                $dropdown.append('<option value="">' + defaultText + '</option>');

                $.each(response, function (index, item) {
                    $dropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            },
            error: function () {
                toastr.success(dropdownId + " ডাটা লোড করতে সমস্যা হয়েছে!");
            }
        });
    }

    // Populate dropdowns
    populateDropdown("actionTackenIttiu", "/action/get");
    populateDropdown("acctionTackenCircle", "/action/get");

    // ITIIU Form Submission
    $("#ittiuForm").submit(function (e) {
        e.preventDefault();

        //var formData = {
        //    fileReceiveDate: $("#fileReceiveDate").val() ? new Date($("#fileReceiveDate").val()).toISOString() : null,
        //    teamFileReceiveDate: $("#teamFileReceiveDate").val() ? new Date($("#teamFileReceiveDate").val()).toISOString() : null,
        //    actionTakenIttiu: parseInt($("#actionTackenIttiu").val(), 10) || 0,
        //    reportFileReturnDate: $("#reportFileReturnDate").val() ? new Date($("#reportFileReturnDate").val()).toISOString() : null,
        //    demand: parseFloat($("#demand").val()) || 0,
        //    collection: parseFloat($("#collection").val()) || 0,
        //    possibleTaxEvasion: parseFloat($("#possibleTaxEvasion").val()) || 0,
        //    taxPayerId: parseInt($("#taxPayerId").val()) || 0,


        //};

        var formData = {
            fileReceiveDate: $("#fileReceiveDate").val() ? new Date($("#fileReceiveDate").val()).toISOString().split("T")[0] : null,
            teamFileReceiveDate: $("#teamFileReceiveDate").val() ? new Date($("#teamFileReceiveDate").val()).toISOString().split("T")[0] : null,
            reportFileReturnDate: $("#reportFileReturnDate").val() ? new Date($("#reportFileReturnDate").val()).toISOString().split("T")[0] : null,
            actionDateIttiu: $("#actionDateIttiu").val() ? new Date($("#actionDateIttiu").val()).toISOString().split("T")[0] : null,
            actionTakenIttiu: parseInt($("#actionTackenIttiu").val()) || 0,
            demand: parseFloat($("#demand").val()) || 0,
            collection: parseFloat($("#collection").val()) || 0,
            possibleTaxEvasion: parseFloat($("#possibleTaxEvasion").val()) || 0,
            taxPayerId: parseInt($("#taxPayerId").val()) || 0
        };



        //console.log('iitti', formData)

        $.ajax({
            url: "/action/team/create",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            dataType: "json",
            success: function () {
                toastr.success("তথ্য সফলভাবে সংরক্ষণ করা হয়েছে!");
                $("#ittiuForm")[0].reset();
                //  $(".datetimepicker").flatpickr().clear();
                GetTaxPayerInfo(taxpayerID)
                loadTimelineIttu(taxpayerID);
            },
            error: function () {
                toastr.success("ডাটা সংরক্ষণ করতে সমস্যা হয়েছে!");
            }
        });
    });

    // Reset ITIIU Form
    $("#resetForm").click(function () {
        $("#ittiuForm")[0].reset();
        $(".datetimepicker").flatpickr().clear();
    });

    // Circle Form Submission
    $("#btn2").click(function (e) {
        e.preventDefault();

        //var formData = {
        //    fileReceiveDate: $("#floatingInputUITDate").val() ? new Date($("#floatingInputUITDate").val()).toISOString() : null,
        //    actionTakenCircle: parseInt($("#acctionTackenCircle").val()) || 0,
        //    actionTaken: parseInt($("#floatingSelectAcctionTacken").val()) || 0,
        //    taxPayerId: parseInt($("#taxPayerId").val()) || 0,
        //    taxDeterminationDate: $("#floatingInputReportDate").val() ? new Date($("#floatingInputReportDate").val()).toISOString() : null,
        //    demand: parseFloat($("#floatingInputDemand").val()) || 0,
        //    collection: parseFloat($("#floatingInputCollectionByITTIU").val()) || 0,

        //};


        var formData = {
            fileReceiveDate: $("#floatingInputUITDate").val() ? new Date($("#floatingInputUITDate").val()).toISOString().split("T")[0] : null,
            taxDeterminationDate: $("#floatingInputReportDate").val() ? new Date($("#floatingInputReportDate").val()).toISOString().split("T")[0] : null,
            actionDateCircle: $("#actionDateCircle").val() ? new Date($("#actionDateCircle").val()).toISOString().split("T")[0] : null,
            actionTakenCircle: parseInt($("#acctionTackenCircle").val()) || 0,
            actionTaken: parseInt($("#floatingSelectAcctionTacken").val()) || 0,
            taxPayerId: parseInt($("#taxPayerId").val(), 10) || 0,
            demand: parseFloat($("#floatingInputDemand").val()) || 0,
            collection: parseFloat($("#floatingInputCollectionByITTIU").val()) || 0
        };




        //console.log('circle', formData)

        $.ajax({
            url: "/action/circle/create",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            dataType: "json",
            success: function () {
                toastr.success("তথ্য সফলভাবে সংরক্ষণ করা হয়েছে!");
                $(".form-control, .form-select").val('');
                //  $(".datetimepicker").flatpickr().clear();
                GetTaxPayerInfo(taxpayerID)
                GetTaxActionByIttuInfo(taxpayerID)
                GetTaxActionByCircleInfo(taxpayerID)
                loadTimelineCircle(taxpayerID)
            },
            error: function () {
                toastr.success("ডাটা সংরক্ষণ করতে সমস্যা হয়েছে!");
            }
        });
    });

    // Reset Circle Form
    $(".btn-phoenix-primary").click(function () {
        $(".form-control, .form-select").val('');
        $(".datetimepicker").flatpickr().clear();
    });


    function GetTaxPayerInfo(taxpayerid) {

       
        $.ajax({
            url: "/taxAction/getTaxInfo", // Change this to your actual API endpoint
            type: "GET",
            data: { taxpayerid: taxpayerid }, // Sending taxpayeraaaid as a parameter
            dataType: "json",
            success: function (response) {

                if (response) {

                    console.log('taxpayer dd', response)

                    // Assuming response contains the data as numbers
                    let itiiuCollection = response.itiiuCollection || 0; // Default to 0 if undefined
                    let circleCollection = response.circleCollection || 0; // Default to 0 if undefined

                    // Calculate the total
                    let totalCollected = itiiuCollection + circleCollection;

                    // Function to format date as "17th Nov, 2020"
                    const formatDate = (date) => {
                        const day = date.getDate();
                        const suffix = (day % 10 === 1 && day !== 11) ? 'st' :
                            (day % 10 === 2 && day !== 12) ? 'nd' :
                                (day % 10 === 3 && day !== 13) ? 'rd' : 'th';
                        const month = date.toLocaleString('en-US', { month: 'short' });
                        const year = date.getFullYear();
                        return `${day}${suffix} ${month}, ${year}`;
                    };

                    // Assuming response contains the dates in "YYYY-MM-DD" format
                    const fileReceivedByITIIU = new Date(response.fileReceivedByITIIU);
                    const fileReceivedByTeamAt = new Date(response.fileReceivedByTeamAt);
                    const fileSentToCircle = new Date(response.fileSentToCircle);

                    // Update the elements with the formatted dates
                    $("#itiiu-file-receive").text(formatDate(fileReceivedByITIIU));
                    $("#team-file-receive").text(formatDate(fileReceivedByTeamAt));
                    $("#file-send-date").text(formatDate(fileSentToCircle));



                    console.log('taxpaye dd', response)
                    //$("#itiiu-file-receive").text(response.fileReceivedByITIIU);
                    //$("#team-file-receive").text(response.fileReceivedByTeamAt);
                    //$("#file-send-date").text(response.fileSentToCircle);
                    // $("#itiiu-action").text(response.itiiuAction);
                    // $("#circle-action").text(response.circleAction);
                    // $("#management-action").text(response.managementAction);
                    //  $("#case-status").text(response.caseStatus);
                    $("#tax-amount").text(response.itiiuCreatedDemand);
                    $("#possible-tax-evasion").text(response.itiiuAmountOfPotentialTaxEvasion);
                    $("#itiiu-collected").text(response.itiiuCollection);
                    $("#circle-collected").text(response.circleCollection);
                    $("#total-collected").text(totalCollected);


                    $("#fileReceiveDate").val(response.fileReceivedByITIIU);
                    $("#teamFileReceiveDate").val(response.fileReceivedByTeamAt);
                    $("#reportFileReturnDate").val(response.fileReceivedByCircle);
                    $("#demand").val(response.itiiuCreatedDemand);
                    $("#collection").val(response.itiiuCollection);
                    $("#possibleTaxEvasion").val(response.itiiuAmountOfPotentialTaxEvasion);



                    $("#floatingInputUITDate").val(response.circleTaxAssessmentDate);
                    $("#floatingInputReportDate").val(response.fileReceivedByCircle);
                    $("#floatingInputDemand").val(response.circleCreatedDemand);
                    $("#floatingInputCollectionByITTIU").val(response.circleCollection);

                    
                  
                }

                
            },
            error: function (error) {
                console.error("Error fetching tax information:", error);
            }
        });
    }

    GetTaxActionByIttuInfo(taxpayerID)
    function GetTaxActionByIttuInfo(taxpayerid) {
        $.ajax({
            url: "/taxAction/getActionByItiiuInfo", // Change this to your actual API endpoint
            type: "GET",
            data: { taxpayerid: taxpayerid }, // Sending taxpayeraaaid as a parameter
            dataType: "json",
            success: function (response) {
                if (response) {
                     $("#itiiu-action").text(response.actionName);

                    //console.log('taxpAct tiiu dd iff', response)
                } else {
                    //console.log('taxpAct tiiu dd else', response)

                }
               
                // $("#circle-action").text(response.actionName);
                // $("#management-action").text(response.managementAction);
                //  $("#case-status").text(response.caseStatus);
              
            },
            error: function (error) {
                console.error("Error fetching tax information:", error);
            }
        });
    }

    GetTaxActionByCircleInfo(taxpayerID)
    function GetTaxActionByCircleInfo(taxpayerid) {
        $.ajax({
            url: "/taxAction/getActionByCircleInfo", // Change this to your actual API endpoint
            type: "GET",
            data: { taxpayerid: taxpayerid }, // Sending taxpayeraaaid as a parameter
            dataType: "json",
            success: function (response) {

                if (response) {
                     $("#circle-action").text(response.actionName);

                    //console.log('taxpAct circle dd iff', response)
                } else {
                    //console.log('taxpAct circle dd else', response)

                }

                // $("#itiiu-action").text(response.actionName);
                // $("#management-action").text(response.managementAction);
                //  $("#case-status").text(response.caseStatus);

            },
            error: function (error) {
                console.error("Error fetching tax information:", error);
            }
        });
    }

    loadTimelineIttu(taxpayerID);
    loadTimelineCircle(taxpayerID);

    function loadTimelineIttu(taxpayerid) {
        $.ajax({
            url: "/Timeline/GetTimelineDataIttu",
            type: "GET",
            data: { taxpayerid: taxpayerid }, // Sending taxpayeraaaid as a parameter
        
            dataType: "json",
            success: function (data) {
                //console.log('ddddddd', data)
                var timelineHtml = '';
                $.each(data, function (index, item) {
                    timelineHtml += `
                        <div class="timeline-item position-relative">
                            <div class="row g-md-3">
                                <div class="col-12 col-md-auto d-flex">
                                    <div class="timeline-item-date order-1 order-md-0 me-md-4">
                                        <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                            ${formatDate(item.date)}<br class="d-none d-md-block" />
                                            ${formatTime(item.date)}
                                        </p>
                                    </div>
                                    <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                        <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                            <span class="fa-solid ${item.icon} text-primary-dark fs-10"></span>
                                        </div>
                                        <span class="timeline-bar border-end border-dashed"></span>
                                    </div>
                                </div>
                                <div class="col">
                                    <div class="timeline-item-content ps-6 ps-md-3">
                                        <h5 class="fs-9 lh-sm">${item.title}</h5>
                                        <p class="fs-9">by <a class="fw-semibold" href="${item.authorLink}">${item.author}</a></p>
                                        <p class="fs-9 text-body-secondary mb-5">
                                            ${item.description}
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                });

                $("#timelineContainer").html(timelineHtml);
            },
            error: function () {
                //console.log("Error loading timeline data.");
            }
        });
    }


    function loadTimelineCircle(taxpayerid) {
        $.ajax({
            url: "/Timeline/GetTimelineDataCircle",
            type: "GET",
            data: { taxpayerid: taxpayerid }, // Sending taxpayeraaaid as a parameter

            dataType: "json",
            success: function (data) {
                //console.log('ddddddd', data)
                var timelineHtml = '';
                $.each(data, function (index, item) {
                    timelineHtml += `
                        <div class="timeline-item position-relative">
                            <div class="row g-md-3">
                                <div class="col-12 col-md-auto d-flex">
                                    <div class="timeline-item-date order-1 order-md-0 me-md-4">
                                        <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                            ${formatDate(item.date)}<br class="d-none d-md-block" />
                                          
                                        </p>
                                    </div>
                                    <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                        <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                            <span class="fa-solid ${item.icon} text-primary-dark fs-10"></span>
                                        </div>
                                        <span class="timeline-bar border-end border-dashed"></span>
                                    </div>
                                </div>
                                <div class="col">
                                    <div class="timeline-item-content ps-6 ps-md-3">
                                        <h5 class="fs-9 lh-sm">${item.title}</h5>
                                        <p class="fs-9">by <a class="fw-semibold" href="${item.authorLink}">${item.author}</a></p>
                                        <p class="fs-9 text-body-secondary mb-5">
                                            ${item.description}
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                });

                $("#timelineContainerCircle").html(timelineHtml);
            },
            error: function () {
                //console.log("Error loading timeline data.");
            }
        });
    }



    function formatDate(dateString) {
        var date = new Date(dateString);
        var options = { day: '2-digit', month: 'short', year: 'numeric' };
        return date.toLocaleDateString('en-US', options).toUpperCase();
    }

     // ${ formatTime(item.date) }
    function formatTime(dateString) {
        var date = new Date(dateString);
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12; // Handle midnight
        minutes = minutes < 10 ? '0' + minutes : minutes;
        return hours + ':' + minutes + ' ' + ampm;
    }



});
