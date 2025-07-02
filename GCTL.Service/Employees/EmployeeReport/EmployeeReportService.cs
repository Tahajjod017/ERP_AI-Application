using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Employees.EmployeeTraining;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Core.ViewModels.Employee.EmployeeFamily;
using GCTL.Core.ViewModels.Employee.EmployeeTraining;

namespace GCTL.Service.Employees.EmployeeReport
{
    public class EmployeeReportService : IEmployeeReportService
    {
        private readonly IEmployeeAdditionalService _employeeAdditionalService;
        private readonly IEmployeeAllowanceService _employeeAllowanceService;
        private readonly IEmployeeBenifitService employeeBenifitService;
        private readonly IEmployeeContactService _employeeContactService;
        private readonly IEmployeeEducationalService _employeeEducationalService;
        private readonly IEmployeeFamilyService _employeeFamilyService;
        private readonly IEmployeeOfficialService _employeeOfficialService;
        private readonly IEmployeePersonalService _employeePersonalService;
        private readonly IEmployeeSalaryService _employeeSalaryService;
        private readonly IEmployeeTrainingService _employeeTrainingService;

        public EmployeeReportService(IEmployeeAdditionalService employeeAdditionalService, IEmployeeAllowanceService employeeAllowanceService, IEmployeeBenifitService employeeBenifitService, IEmployeeContactService employeeContactService, IEmployeeEducationalService employeeEducationalService, IEmployeeFamilyService employeeFamilyService, IEmployeeOfficialService employeeOfficialService, IEmployeePersonalService employeePersonalService, IEmployeeSalaryService employeeSalaryService, IEmployeeTrainingService employeeTrainingService)
        {
            _employeeAdditionalService = employeeAdditionalService;
            _employeeAllowanceService = employeeAllowanceService;
            this.employeeBenifitService = employeeBenifitService;
            _employeeContactService = employeeContactService;
            _employeeEducationalService = employeeEducationalService;
            _employeeFamilyService = employeeFamilyService;
            _employeeOfficialService = employeeOfficialService;
            _employeePersonalService = employeePersonalService;
            _employeeSalaryService = employeeSalaryService;
            _employeeTrainingService = employeeTrainingService;
        }

        public async Task<byte[]> GenaratePDF(int id)
        {
            // Define default values for fields not directly available
            string companyName = "Your Company Name"; // Default company name
            string companyAddress = "Your Company Address"; // Default company address
            QuestPDF.Settings.License = LicenseType.Community;
            try
            {
                var personal = await _employeePersonalService.GetEmployeePersonalById(id);
                var official = await _employeeOfficialService.GetEmployeeOfficalDetails(id);
                var contact = await _employeeContactService.GetEmployeeContactByIdAsync(id);
                var family = await _employeeFamilyService.GetEmployeeFamilyByIdAsync(id);
                var educational = await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(id);
                var additional = await _employeeAdditionalService.GetEmployeeAdditionalByIdAsync(id);
                var salary = await _employeeSalaryService.GetEmployeeSalaryByEmployeeIdAsync(id);
                var allowance = await _employeeAllowanceService.GetEmployeeAllowance(id);
                var training = await _employeeTrainingService.GetEmployeeTrainingByIdAsync(id);
                var benifit = await employeeBenifitService.GetEmployeeBenefitsAsync(id.ToString());

                if (personal == null)
                {
                    throw new Exception("No employee data found.");
                }

                using (var stream = new MemoryStream())
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4); // Portrait orientation
                            page.Margin(35);
                            page.DefaultTextStyle(x => x.FontFamily(Fonts.TimesNewRoman).FontSize(10));

                            // Header with Logo and Company Info
                            page.Header().Element(container =>
                            {
                                container.Row(row =>
                                {
                                    row.RelativeItem().AlignRight().Column(column =>
                                    {
                                        column.Item().Height(100).Width(100).Image("wwwroot/img/No-Image-Placeholder.svg.png");
                                        column.Item().Text(companyName).FontSize(18).Bold().AlignRight();
                                        column.Item().Width(200).Text(companyAddress).FontSize(12).Bold().LineHeight(1.2f).AlignRight();
                                    });
                                });
                            });

                            // Content
                            page.Content().Column(column =>
                            {
                                // Employee Information Section
                                column.Item().Element(container =>
                                {
                                    container.Row(row =>
                                    {
                                        row.RelativeItem(55).Column(col =>
                                        {
                                            col.Item().Text("EMPLOYEE INFORMATION").FontSize(12).Bold();
                                            col.Item().Table(table =>
                                            {
                                                table.ColumnsDefinition(columns =>
                                                {
                                                    columns.RelativeColumn(71.5f);
                                                    columns.RelativeColumn(5);
                                                    columns.RelativeColumn(200);
                                                });

                                                void AddInfoRow(string label, string value)
                                                {
                                                    table.Cell().Text(label).Bold().AlignLeft();
                                                    table.Cell().Text(":").Bold().AlignCenter();
                                                    table.Cell().Text(value ?? " ").AlignLeft();
                                                }

                                                AddInfoRow("Department", official.DepartmentID.ToString());
                                                AddInfoRow("Employee’s ID", personal.EmployeeCode);
                                                AddInfoRow("Designation", official.DesignationID.ToString());
                                                AddInfoRow("Date of Hire", official.JoiningDate?.ToString("dd/MM/yyyy"));
                                            });
                                        });

                                        row.RelativeItem(45).AlignRight().Height(90).Image(
                                             !string.IsNullOrEmpty(personal?.EmployeeImageFileName)
                                                 ? System.IO.File.ReadAllBytes($"wwwroot/uploads/employee/images/{personal.EmployeeImageFileName}")
                                                 : System.IO.File.ReadAllBytes("wwwroot/uploads/employee/images/MdShefain.jpg")
                                         ).FitArea();
                                    });
                                });

                                // Personal Information Section
                                column.Item().PaddingTop(10).Text("Personal Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(24);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(60);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        void AddEmptyRow()
                                        {
                                            leftTable.Cell().Text(" ").Bold().AlignLeft();
                                            leftTable.Cell().Text("").Bold().AlignCenter();
                                            leftTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddInfoRow("Employee Name", $"{personal?.FirstName} {personal?.LastName}");
                                        AddInfoRow("Father's Name", personal?.FatherName);
                                        AddInfoRow("Mother's Name", personal?.MotherName);
                                        AddInfoRow("Present Address", $"{personal?.HouseNo}, {personal?.RoadNo}, {personal?.City}, {personal?.State}, {personal?.PostalCode}");
                                        AddInfoRow("Permanent Address", $"{personal?.HouseNo}, {personal?.RoadNo}, {personal?.City}, {personal?.State}, {personal?.PostalCode}");
                                        AddInfoRow("Date of Birth", personal?.DateOfBirth?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Gender", personal?.GenderID.ToString());
                                        AddInfoRow("Blood Group", personal?.BloodGroupID.ToString());
                                        AddInfoRow("Nationality", personal?.Nationality);
                                        AddInfoRow("Religion", personal?.ReligionID.ToString());
                                        AddInfoRow("Marital Status", personal?.MaritalStatusID.ToString());
                                        AddInfoRow("Personal Mobile No", personal?.MobileNumber);
                                        AddInfoRow("TIN No", personal?.TIN);
                                        AddInfoRow("Birth Certificate No", personal?.BirthCertificateNo);
                                        AddInfoRow("About Employee", personal?.AboutEmployee);
                                        AddEmptyRow();
                                        AddInfoRow("Passport No.", additional?.PasportNo);
                                        AddInfoRow("Driving License No.", additional?.DrivingLicenceNo);
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(21.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(60);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            rightTable.Cell().Text(label).Bold().AlignLeft();
                                            rightTable.Cell().Text(":").Bold().AlignCenter();
                                            rightTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        void AddEmptyRow()
                                        {
                                            rightTable.Cell().Text(" ").Bold().AlignLeft();
                                            rightTable.Cell().Text("").Bold().AlignCenter();
                                            rightTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Place Of Birth", additional?.PasportPlaceOfIssue);
                                        AddInfoRow("Nationality ID", personal?.NID);
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Personal Mail ID", personal?.Email);
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Passport Expiry Date", additional?.PasportExpireDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Driving License Expiry Date", additional?.DrivingLicenceExpireDate?.ToString("dd/MM/yyyy"));
                                    });
                                });

                                // Additional Information (Empty as in original)
                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(33.3f);
                                        columns.RelativeColumn(33.3f);
                                        columns.RelativeColumn(33.3f);
                                    });

                                    table.Cell().Table(t => t.ColumnsDefinition(c => { c.RelativeColumn(33); c.RelativeColumn(5); c.RelativeColumn(62); }));
                                    table.Cell().Table(t => t.ColumnsDefinition(c => { c.RelativeColumn(33); c.RelativeColumn(5); c.RelativeColumn(62); }));
                                    table.Cell().Table(t => t.ColumnsDefinition(c => { c.RelativeColumn(33); c.RelativeColumn(5); c.RelativeColumn(62); }));
                                });

                                // Salary Account Info
                                column.Item().PaddingTop(10).Text("Salary Account Info").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(30);
                                        columns.RelativeColumn(25);
                                        columns.RelativeColumn(25);
                                        columns.RelativeColumn(20);
                                    });

                                    table.Header(header =>
                                    {
                                       

                                        header.Cell().Border(0.5f).Padding(2).Text("Bank Name").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Branch").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("A/C Name").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("A/C No.").Bold().FontSize(10).AlignCenter();

                                    });

                                    if (salary != null)
                                    {
                                        table.Cell().Text(salary?.BankName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(salary?.BranchName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(salary?.AccountName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(salary?.AccountNo ?? " ").FontSize(9).AlignLeft();
                                    }
                                });

                                // Work Permit Info
                                column.Item().PaddingTop(10).Text("Work Permit Info").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(24);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(58);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Work Permit No.", additional?.WorkPermaitNumber);
                                        AddInfoRow("Work Permit Type", additional?.WorkPermitType);
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(23);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(58);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            rightTable.Cell().Text(label).Bold().AlignLeft();
                                            rightTable.Cell().Text(":").Bold().AlignCenter();
                                            rightTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("WP Effective Date", additional?.WorkPermitEffectiveDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("WP Expire Date", additional?.WorkPermitExpireDate?.ToString("dd/MM/yyyy"));
                                    });
                                });

                                // Emergency Contact-1
                                var contact1 = contact?.FirstOrDefault();
                                column.Item().PaddingTop(10).Text("Emergency Contact-1").FontSize(11).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(26.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(65);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Name", contact1?.ContactName);
                                        AddInfoRow("Present Address", $"{personal?.HouseNo}, {personal?.RoadNo}, {personal?.City}, {personal?.State}, {personal?.PostalCode}");
                                        AddInfoRow("Relationship", contact1?.Relationship);
                                        AddInfoRow("Phone", contact1?.ContactNumber);
                                        AddInfoRow("E-mail", contact1?.ContactEmail);
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(26);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(65);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            rightTable.Cell().Text(label).Bold().AlignLeft();
                                            rightTable.Cell().Text(":").Bold().AlignCenter();
                                            rightTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        void AddEmptyRow()
                                        {
                                            rightTable.Cell().Text(" ").Bold().AlignLeft();
                                            rightTable.Cell().Text("").Bold().AlignCenter();
                                            rightTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Personal Phone", contact1?.PersonalPhone);
                                    });
                                });

                                // Emergency Contact-2
                                var contact2 = contact?.Skip(1).FirstOrDefault();
                                column.Item().PaddingTop(10).Text("Emergency Contact-2").FontSize(11).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(26.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(65);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Name", contact2?.ContactName);
                                        AddInfoRow("Present Address", $"{personal?.HouseNo}, {personal?.RoadNo}, {personal?.City}, {personal?.State}, {personal?.PostalCode}");
                                        AddInfoRow("Relationship", contact2?.Relationship);
                                        AddInfoRow("Phone", contact2?.ContactNumber);
                                        AddInfoRow("E-mail", contact2?.ContactEmail);
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(26);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(65);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            rightTable.Cell().Text(label).Bold().AlignLeft();
                                            rightTable.Cell().Text(":").Bold().AlignCenter();
                                            rightTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        void AddEmptyRow()
                                        {
                                            rightTable.Cell().Text(" ").Bold().AlignLeft();
                                            rightTable.Cell().Text("").Bold().AlignCenter();
                                            rightTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Personal Phone", contact2?.PersonalPhone);
                                    });
                                });

                                // Official Information
                                column.Item().PaddingTop(10).Text("Official Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(83);
                                            columns.ConstantColumn(6);
                                            columns.ConstantColumn(150);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Organization", official?.OrganizationID.ToString());
                                        AddInfoRow("Branch", official?.OrganizationBranchID.ToString());
                                        AddInfoRow("Department", official?.DepartmentID.ToString());
                                        AddInfoRow("Designation", official?.DesignationID.ToString());
                                        AddInfoRow("Employee Type", official?.EmployeeTypeID.ToString());
                                        AddInfoRow("Employment Nature", official?.EmploymentNatureID.ToString());
                                        AddInfoRow("Grade No", salary?.GradeID.ToString());
                                        AddInfoRow("Gross Salary", salary?.Salary?.ToString("N0"));
                                        AddInfoRow("Immediate Supervisor", official?.ImmediateSupervisorId.ToString());
                                        AddInfoRow("Official Phone", official?.OfficePhone);
                                        AddInfoRow("Appointment Date", official?.AppointmentLetterIssueDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Probation Period", official?.ProvisionPeriod.ToString());
                                        AddInfoRow("Probation End Date", official?.ProvisionPeriodStartDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Contract End Date", official?.ContractEndDate?.ToString("dd/MM/yyyy"));
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(78);
                                            columns.ConstantColumn(6);
                                            columns.ConstantColumn(150);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            rightTable.Cell().Text(label).Bold().AlignLeft();
                                            rightTable.Cell().Text(":").Bold().AlignCenter();
                                            rightTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        void AddEmptyRow()
                                        {
                                            rightTable.Cell().Text(" ").Bold().AlignLeft();
                                            rightTable.Cell().Text("").Bold().AlignCenter();
                                            rightTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Mode of Payment", salary?.PrimaryPaymentModeId.ToString());
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Head of Department", official?.HeadOfDepartmentId.ToString());
                                        AddInfoRow("Official Email", official?.OfficeEmail);
                                        AddInfoRow("Joining Date", official?.JoiningDate?.ToString("dd/MM/yyyy"));
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Confirmation Date", official?.ConfirmationDate?.ToString("dd/MM/yyyy"));
                                    });
                                });

                                // Family Information
                                column.Item().PaddingTop(10).Text("Family Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(25);
                                        columns.RelativeColumn(25);
                                        columns.RelativeColumn(25);
                                        columns.RelativeColumn(25);
                                    });

                                    table.Header(header =>
                                    {
                                        
                                        header.Cell().Border(0.5f).Padding(2).Text("Name").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Relationship").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Occupation").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Contact").Bold().FontSize(10).AlignCenter();

                                    });

                                    foreach (var member in family ?? Enumerable.Empty<EmployeeFamilyGetViewModel>())
                                    {
                                        table.Cell().Text(member?.FullName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(member?.RelationToEmployee ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(member?.Occupation ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Text(member?.ContactNumber ?? " ").FontSize(9).AlignCenter();
                                    }
                                });

                                // Educational Information
                                column.Item().PaddingTop(10).Text("Educational Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(17);
                                        columns.RelativeColumn(15);
                                        columns.RelativeColumn(20);
                                        columns.RelativeColumn(18);
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(7);
                                    });

                                    table.Header(header =>
                                    {
                                        

                                        header.Cell().Border(0.5f).Padding(2).Text("Exam Title").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Major").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Institution").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Board/University").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Result").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Passing Year").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Duration").Bold().FontSize(10).AlignCenter();

                                    });

                                    foreach (var edu in educational ?? Enumerable.Empty<EmployeeEducationGetViewModel>())
                                    {
                                        table.Cell().Text(edu?.DegreeID ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(edu?.MajorSubject ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(edu?.InstitutionName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(edu?.EducationBoardID ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(edu?.ResultTypeID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Text(edu?.PassingYearID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Text(edu?.YearDuration?.ToString() ?? " ").FontSize(9).AlignCenter();
                                    }
                                });

                                // Training Information
                                column.Item().PaddingTop(10).Text("Training Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(12);
                                        columns.RelativeColumn(18);
                                        columns.RelativeColumn(20);
                                        columns.RelativeColumn(15);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(10);
                                    });

                                    table.Header(header =>
                                    {
                                       

                                        header.Cell().Border(0.5f).Padding(2).Text("Course Type").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Course Title").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Institute").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Location").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Duration").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Country").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Year").Bold().FontSize(10).AlignCenter();

                                    });

                                    foreach (var qual in training ?? Enumerable.Empty<EmployeeTrainingGetViewModel>())
                                    {
                                        table.Cell().Text(qual?.TopicCovered ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(qual?.TranningTitle ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(qual?.InstituteName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(qual?.LocationName ?? " ").FontSize(9).AlignLeft();
                                        table.Cell().Text(qual?.YearDuration?.ToString() ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Text(qual?.CountryID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Text(qual?.TrainingYearID ?? " ").FontSize(9).AlignCenter();
                                    }
                                });

                                // Allowance Information
                                column.Item().PaddingTop(10).Text("Allowance Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(33);
                                        columns.RelativeColumn(33);
                                        columns.RelativeColumn(33);
                                    });

                                    table.Header(header =>
                                    {
                                       

                                        header.Cell().Border(0.5f).Padding(2).Text("Allowance Type").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Amount").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Effective Date").Bold().FontSize(10).AlignCenter();

                                    });

                                    if (allowance != null)
                                    {
                                        if (allowance.IsMobileAllowanceEnabled)
                                        {
                                            table.Cell().Text("Mobile Allowance").FontSize(9).AlignLeft();
                                            table.Cell().Text(allowance?.MobileAllowance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Text(allowance?.MobileAllowanceEffectiveFromStr ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsInternetAllowanceEnabled)
                                        {
                                            table.Cell().Text("Internet Allowance").FontSize(9).AlignLeft();
                                            table.Cell().Text(allowance?.InternetAllowance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Text(allowance?.InternetAllowanceEffectiveFromStr ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsShiftAllowanceEnabled)
                                        {
                                            table.Cell().Text("Shift Allowance").FontSize(9).AlignLeft();
                                            table.Cell().Text(allowance?.ShiftAllowance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Text(" ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsHouseRentAllowancePercentageEnabled)
                                        {
                                            table.Cell().Text("House Rent Allowance").FontSize(9).AlignLeft();
                                            table.Cell().Text(allowance?.HouseRentAllowancePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Text(" ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsMedicalAllowancePercentageEnabled)
                                        {
                                            table.Cell().Text("Medical Allowance").FontSize(9).AlignLeft();
                                            table.Cell().Text(allowance?.MedicalAllowancePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Text(" ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsConveyanceAllowancePercentageEnabled)
                                        {
                                            table.Cell().Text("Conveyance Allowance").FontSize(9).AlignLeft();
                                            table.Cell().Text(allowance?.ConveyanceAllowancePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Text(" ").FontSize(9).AlignCenter();
                                        }
                                    }
                                });

                                // Benefits Information
                                column.Item().PaddingTop(10).Text("Benefits Information").FontSize(12).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Header(header =>
                                    {
                                       

                                        header.Cell().Border(0.5f).Padding(2).Text("Benefit Type").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Border(0.5f).Padding(2).Text("Amount").Bold().FontSize(10).AlignCenter();

                                    });

                                    if (benifit != null)
                                    {
                                        if (benifit.IsHealthInsuranceEnabled)
                                        {
                                            table.Cell().Text("Health Insurance").FontSize(9).AlignLeft();
                                            table.Cell().Text(benifit?.HealthInsurance?.ToString("N0") ?? " ").FontSize(9).AlignLeft();
                                        }
                                        if (benifit.IsPerformanceBonusEnabled)
                                        {
                                            table.Cell().Text("Performance Bonus").FontSize(9).AlignLeft();
                                            table.Cell().Text(benifit?.PerformanceBonus?.ToString("N0") ?? " ").FontSize(9).AlignLeft();
                                        }
                                        if (benifit.IsFastivalBonusPercentageEnabled)
                                        {
                                            table.Cell().Text("Festival Bonus").FontSize(9).AlignLeft();
                                            table.Cell().Text(benifit?.FastivalBonusPercentage?.ToString("N0") ?? " ").FontSize(9).AlignLeft();
                                        }
                                        if (benifit.IsProvidantFundEnabled)
                                        {
                                            table.Cell().Text("Provident Fund (Employee)").FontSize(9).AlignLeft();
                                            table.Cell().Text(benifit?.ProvidantFundEmployeePercentage?.ToString("N0") ?? " ").FontSize(9).AlignLeft();
                                            table.Cell().Text("Provident Fund (Organization)").FontSize(9).AlignLeft();
                                            table.Cell().Text(benifit?.ProvidantFundOrganizationPercentage?.ToString("N0") ?? " ").FontSize(9).AlignLeft();
                                        }
                                    }
                                });

                                // Signature Section
                                column.Item().PaddingTop(15).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(50);
                                        columns.RelativeColumn(50);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(25);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(58);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        void AddEmptyRow()
                                        {
                                            leftTable.Cell().Text(" ").AlignLeft();
                                            leftTable.Cell().Text(" ").AlignCenter();
                                            leftTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddInfoRow("Access Card No", official?.AttendanceId);
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddInfoRow("Personal Mail ID", personal?.Email);
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(25);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(58);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            rightTable.Cell().Text(label).Bold().AlignLeft();
                                            rightTable.Cell().Text(":").Bold().AlignCenter();
                                            rightTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Employee's Signature", !string.IsNullOrEmpty(personal?.EmployeeSignatureFileName) ? "[Signature]" : "_______________________________");
                                        AddInfoRow("Date", DateTime.Now.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Verified By", "HR Department");
                                        AddInfoRow("Signature", "_______________________________");
                                        AddInfoRow("Date", DateTime.Now.ToString("dd/MM/yyyy"));
                                    });
                                });
                            });

                            // Footer with Page Numbers and Print Date
                            page.Footer().AlignBottom().Row(row =>
                            {
                                row.RelativeItem().PaddingLeft(30).Text(text =>
                                {
                                    text.Span("Print Date: ").FontSize(8);
                                    text.Span(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")).FontSize(8);
                                });

                                row.RelativeItem().AlignRight().PaddingRight(30).Text(text =>
                                {
                                    text.Span("Page ").FontSize(8);
                                    text.CurrentPageNumber().FontSize(8);
                                    text.Span(" of ").FontSize(8);
                                    text.TotalPages().FontSize(8);
                                });
                            });
                        });
                    }).GeneratePdf(stream);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("An error occurred while generating the PDF.");
            }
        }


    }
}
