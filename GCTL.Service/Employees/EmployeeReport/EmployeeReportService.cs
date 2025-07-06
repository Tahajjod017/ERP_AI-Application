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
using GCTL.Service.FileHandler;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;


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
        private readonly IPdfFileHandler _pdfFileHandlerService;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficialRepository;



        public EmployeeReportService(IEmployeeAdditionalService employeeAdditionalService, IEmployeeAllowanceService employeeAllowanceService, IEmployeeBenifitService employeeBenifitService, IEmployeeContactService employeeContactService, IEmployeeEducationalService employeeEducationalService, IEmployeeFamilyService employeeFamilyService, IEmployeeOfficialService employeeOfficialService, IEmployeePersonalService employeePersonalService, IEmployeeSalaryService employeeSalaryService, IEmployeeTrainingService employeeTrainingService, IPdfFileHandler pdfFileHandlerService, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository)
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
            _pdfFileHandlerService = pdfFileHandlerService;
            _employeeOfficialRepository = employeeOfficialRepository;
        }

        public async Task<byte[]> GenaratePDF(int id)
        {
           
            var company = await _employeeOfficialRepository.AllActive().Where(e => e.EmployeeID == id).Include(m => m.Organization).FirstOrDefaultAsync();

            QuestPDF.Settings.License = LicenseType.Community;
            try
            {
                var personal = await _employeePersonalService.GetEmployeePersonalById(id);
                var official = await _employeeOfficialService.GetFullEmployeeOfficalDetails(id);
                var contact = await _employeeContactService.GetEmployeeContactByIdAsync(id);
                var family = await _employeeFamilyService.GetEmployeeFamilyByIdAsync(id);
                var educational = await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(id);
                var additional = await _employeeAdditionalService.GetFullEmployeeAdditionalByIdAsync(id);
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


                            //page.Background().Element(background =>
                            //{
                            //    _pdfFileHandlerService.ComposeWatermark(background, (int)company.OrganizationID);
                            //});


                            page.Header().Element(header =>
                            {
                                _pdfFileHandlerService.ComposeHeader(header, (int)company.OrganizationID, true);
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

                                                AddInfoRow("Department", official.DepartmentName.ToString());
                                                AddInfoRow("Employee’s ID", personal.EmployeeCode);
                                                AddInfoRow("Designation", official.DesignationName.ToString());
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

                                #region Personal Information Section

                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Personal Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(55);
                                        columns.RelativeColumn(45);
                                    });

                                    table.Cell().Table(leftTable =>
                                    {
                                        leftTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(24);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(45);
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

                                        AddInfoRow("Employee Name", $"{personal?.FirstName ?? ""} {personal?.LastName ?? ""}".Trim());
                                        AddInfoRow("Father's Name", personal?.FatherName ?? "");
                                        AddInfoRow("Mother's Name", personal?.MotherName ?? "");
                                        AddInfoRow("Present Address", $"{personal?.HouseNo ?? ""}, {personal?.RoadNo ?? ""}, {personal?.City ?? ""}, {personal?.State ?? ""}, {personal?.PostalCode ?? ""}".Trim(' ', ','));
                                        AddInfoRow("Permanent Address", $"{personal?.HouseNo ?? ""}, {personal?.RoadNo ?? ""}, {personal?.City ?? ""}, {personal?.State ?? ""}, {personal?.PostalCode ?? ""}".Trim(' ', ','));
                                        AddInfoRow("Date of Birth", personal?.DateOfBirth?.ToString("dd/MM/yyyy") ?? "");
                                        AddInfoRow("Gender", personal?.GenderName?.ToString() ?? "");
                                        AddInfoRow("Blood Group", personal?.BloodGroupName?.ToString() ?? "");
                                        AddInfoRow("Nationality", personal?.Nationality ?? "");
                                        AddInfoRow("Religion", personal?.ReligionName?.ToString() ?? "");
                                        AddInfoRow("Marital Status", personal?.MaritalStatusName?.ToString() ?? "");
                                        AddInfoRow("Personal Mobile No", personal?.MobileNumber ?? "");
                                        AddInfoRow("TIN No", personal?.TIN ?? "");
                                        AddInfoRow("Birth Certificate No", personal?.BirthCertificateNo ?? "");
                                        
                                        AddInfoRow("Passport No.", additional?.PasportNo ?? "");
                                        AddInfoRow("Driving License No.", additional?.DrivingLicenceNo ?? "");

                                        
                                        AddInfoRow("About Employee", personal?.AboutEmployee ?? "");
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(21.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(40);
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
                                       
                                        AddInfoRow("Personal Mail ID", personal?.Email);
                                        AddEmptyRow();
                                        AddEmptyRow();
                                       
                                        AddInfoRow("Expiry Date", additional?.PasportExpireDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Expiry Date", additional?.DrivingLicenceExpireDate?.ToString("dd/MM/yyyy"));
                                    });

                                 

                                    


                                });

                                #endregion

                                #region Additional Information (Empty as in original)

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

                                #endregion

                                #region Salary


                                // Salary Account Info
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Salary Account Info").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                        table.Cell().Border(0.5f).Padding(2).Text(salary?.BankName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(salary?.BranchName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(salary?.AccountName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(salary?.AccountNo ?? " ").FontSize(9).AlignCenter();
                                    }
                                });

                                #endregion

                                #region Work Permit Info

                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Work Permit Info").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                            columns.RelativeColumn(45);
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
                                            columns.RelativeColumn(21.5f);
                                            columns.RelativeColumn(2f);
                                            columns.RelativeColumn(40);
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

                                #endregion

                                #region emergency Contact

                                // Emergency Contact-1
                                var contact1 = contact?.FirstOrDefault();
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Emergency Contact-1").FontSize(11).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                            columns.RelativeColumn(45);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Name", contact1?.ContactName);
                                        AddInfoRow("Phone", contact1?.ContactNumber);
                                        AddInfoRow("Present Address", $"{personal?.HouseNo}, {personal?.RoadNo}, {personal?.City}, {personal?.State}, {personal?.PostalCode}");
                                      
                                        
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(21.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(40);
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

                                        AddInfoRow("Relationship", contact1?.Relationship);
                                        AddInfoRow("Personal Phone", contact1?.PersonalPhone);
                                        AddInfoRow("E-mail", contact1?.ContactEmail);
                                        
                                    });
                                });

                                // Emergency Contact-2
                                var contact2 = contact?.Skip(1).FirstOrDefault();
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Emergency Contact-2").FontSize(11).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                            columns.RelativeColumn(45);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Name", contact2?.ContactName);
                                        AddInfoRow("Phone", contact2?.ContactNumber);
                                        AddInfoRow("Present Address", $"{personal?.HouseNo}, {personal?.RoadNo}, {personal?.City}, {personal?.State}, {personal?.PostalCode}");
                                      
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(21.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(40);
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

                                      
                                        AddInfoRow("Relationship", contact2?.Relationship);
                                        AddInfoRow("Personal Phone", contact2?.PersonalPhone);
                                        AddInfoRow("E-mail", contact2?.ContactEmail);
                                    });
                                });


                                #endregion

                                column.Item().PageBreak();

                                #region Official Information

                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Official Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                            columns.ConstantColumn(98);
                                            columns.ConstantColumn(6);
                                            columns.ConstantColumn(135);
                                        });

                                        void AddInfoRow(string label, string value)
                                        {
                                            leftTable.Cell().Text(label).Bold().AlignLeft();
                                            leftTable.Cell().Text(":").Bold().AlignCenter();
                                            leftTable.Cell().Text(value ?? " ").AlignLeft();
                                        }

                                        AddInfoRow("Organization", official?.OrganizationName.ToString());
                                        AddInfoRow("Branch", official?.OrganizationBranchName.ToString());
                                        AddInfoRow("Department", official?.DepartmentName.ToString());
                                        AddInfoRow("Designation", official?.DesignationName.ToString());
                                        AddInfoRow("Employee Type", official?.EmployeeTypeName.ToString());
                                        AddInfoRow("Employment Nature", official?.EmploymentNatureName.ToString());
                                        AddInfoRow("Grade No", salary?.GradeID.ToString());
                                        AddInfoRow("Gross Salary", salary?.Salary?.ToString("N0"));
                                        AddInfoRow("Immediate Supervisor", official?.ImmediateSupervisorName.ToString());
                                        AddInfoRow("Official Phone", official?.OfficePhone);
                                        AddInfoRow("Appointment Date", official?.AppointmentLetterIssueDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Probation Period", official?.ProvisionPeriod.ToString());
                                       
                                        AddInfoRow("Contract End Date", official?.ContractEndDate?.ToString("dd/MM/yyyy"));
                                    });

                                    table.Cell().Table(rightTable =>
                                    {
                                        rightTable.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(93);
                                            columns.ConstantColumn(6);
                                            columns.ConstantColumn(125);
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
                                      
                                       
                                        AddInfoRow("Mode of Payment", salary?.PrimaryPaymentModeId.ToString());
                                      
                                        AddInfoRow("Head of Department", official?.HeadOfDepartmentId.ToString());
                                        AddInfoRow("Official Email", official?.OfficeEmail);
                                        AddInfoRow("Joining Date", official?.JoiningDate?.ToString("dd/MM/yyyy"));
                                        AddInfoRow("Probation Start Date", official?.ProvisionPeriodStartDate?.ToString("dd/MM/yyyy"));
                                       
                                      
                                    
                                        AddInfoRow("Confirmation Date", official?.ConfirmationDate?.ToString("dd/MM/yyyy"));
                                    });
                                });


                                #endregion

                                #region Family

                                // Family Information
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Family Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                        table.Cell().Border(0.5f).Padding(2).Text(member?.FullName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(member?.RelationToEmployee ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(member?.Occupation ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(member?.ContactNumber ?? " ").FontSize(9).AlignCenter();
                                    }
                                });

                                #endregion

                                #region Education

                                // Educational Information
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Educational Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(17);
                                        columns.RelativeColumn(15);
                                        columns.RelativeColumn(21);
                                        columns.RelativeColumn(18);
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(8);
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
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.DegreeID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.MajorSubject ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.InstitutionName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.EducationBoardID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.ResultTypeID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.PassingYearID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(edu?.YearDuration?.ToString() ?? " ").FontSize(9).AlignCenter();
                                    }
                                });

                                #endregion


                                #region Training

                                // Training Information
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Training Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(12);
                                        columns.RelativeColumn(18);
                                        columns.RelativeColumn(22);
                                        columns.RelativeColumn(14);
                                        columns.RelativeColumn(9);
                                        columns.RelativeColumn(9);
                                        columns.RelativeColumn(6);
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
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.TopicCovered ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.TranningTitle ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.InstituteName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.LocationName ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.YearDuration?.ToString() ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.CountryID ?? " ").FontSize(9).AlignCenter();
                                        table.Cell().Border(0.5f).Padding(2).Text(qual?.TrainingYearID ?? " ").FontSize(9).AlignCenter();
                                    }
                                });


                                #endregion

                                column.Item().PageBreak();

                                #region ALlowance
                                // Allowance Information
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Allowance Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                            table.Cell().Border(0.5f).Padding(2).Text("Mobile Allowance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.MobileAllowance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.MobileAllowanceEffectiveFromStr ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsInternetAllowanceEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Internet Allowance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.InternetAllowance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.InternetAllowanceEffectiveFromStr ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsShiftAllowanceEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Shift Allowance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.ShiftAllowance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(" ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsHouseRentAllowancePercentageEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("House Rent Allowance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.HouseRentAllowancePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(" ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsMedicalAllowancePercentageEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Medical Allowance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.MedicalAllowancePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(" ").FontSize(9).AlignCenter();
                                        }
                                        if (allowance.IsConveyanceAllowancePercentageEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Conveyance Allowance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(allowance?.ConveyanceAllowancePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(" ").FontSize(9).AlignCenter();
                                        }
                                    }
                                });

                                #endregion

                                #region Benifit
                                // Benefits Information
                                column.Item().PaddingTop(10).PaddingBottom(5).Text("Benefits Information").FontSize(12).Bold().Underline().DecorationDotted().DecorationThickness(1);
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
                                            table.Cell().Border(0.5f).Padding(2).Text("Health Insurance").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(benifit?.HealthInsurance?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (benifit.IsPerformanceBonusEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Performance Bonus").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(benifit?.PerformanceBonus?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (benifit.IsFastivalBonusPercentageEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Festival Bonus").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(benifit?.FastivalBonusPercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                        }
                                        if (benifit.IsProvidantFundEnabled)
                                        {
                                            table.Cell().Border(0.5f).Padding(2).Text("Provident Fund (Employee)").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(benifit?.ProvidantFundEmployeePercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text("Provident Fund (Organization)").FontSize(9).AlignCenter();
                                            table.Cell().Border(0.5f).Padding(2).Text(benifit?.ProvidantFundOrganizationPercentage?.ToString("N0") ?? " ").FontSize(9).AlignCenter();
                                        }
                                    }
                                });

                                #endregion

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
                                        AddInfoRow("Employee's Signature", !string.IsNullOrEmpty(personal?.EmployeeSignatureFileName) ? "[Signature]" : "_______________________________");
                                        AddInfoRow("Date", DateTime.Now.ToString("dd/MM/yyyy"));
                                       
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

                                        void AddEmptyRow()
                                        {
                                            rightTable.Cell().Text(" ").AlignLeft();
                                            rightTable.Cell().Text(" ").AlignCenter();
                                            rightTable.Cell().Text(" ").AlignLeft();
                                        }

                                        AddInfoRow("Personal Mail ID", personal?.Email);
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
                                        AddEmptyRow();
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



        public async Task<byte[]> GenerateEmployeeExcelReportAsync()
        {
            
            ExcelPackage.License.SetNonCommercialOrganization("GCTL");



            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Employee Report");

                // Title
                worksheet.Cells[1, 1, 1, 9].Merge = true;
                worksheet.Cells[1, 1].Value = "ABC Corporation";
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 1, 2, 9].Merge = true;
                worksheet.Cells[2, 1].Value = "123 Business Street, City, Country";
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1, 3, 9].Merge = true;
                worksheet.Cells[3, 1].Value = $"Report Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                worksheet.Cells[3, 1].Style.Font.Size = 10;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Fetch data
                var officialData = await _employeeOfficialService.GetAllEmployeeOfficialDetailsAsync();
                var personalData = await _employeePersonalService.GetAllEmployeePersonalAsync();
                var salaryData = await _employeeSalaryService.GetAllEmployeeSalaryAsync();

                var employees = officialData.Select(o => new
                {
                    EmployeeID = o.EmployeePersonalId,
                    DepartmentName = o.DepartmentName ?? "Unknown",
                    DesignationName = o.DesignationName ?? "",
                    JoiningDate = o.JoiningDate,
                    FullName = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId) is var person && person != null ? $"{person.FirstName} {person.LastName}".Trim() : string.Empty,
                    Email = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId)?.Email ?? "",
                    GrossSalary = salaryData.FirstOrDefault(s => s.EmployeePersonalId == o.EmployeePersonalId)?.Salary ?? 0,
                    EmployeeCode = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId)?.EmployeeCode ?? "",
                    DateOfBirth = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId)?.DateOfBirth,
                    Gender = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId)?.GenderName ?? ""
                }).OrderBy(e => e.DepartmentName).ToList();

                var departmentGroups = employees.GroupBy(e => e.DepartmentName).ToList();

                if (!departmentGroups.Any())
                {
                    throw new Exception("No employee data found.");
                }

                int currentRow = 5;

                foreach (var department in departmentGroups)
                {
                    // Department Header
                    worksheet.Cells[currentRow, 1, currentRow, 9].Merge = true;
                    worksheet.Cells[currentRow, 1].Value = $"Department: {department.Key}";
                    worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    currentRow++;

                    // Table Header
                    worksheet.Cells[currentRow, 1].Value = "Employee ID";
                    worksheet.Cells[currentRow, 2].Value = "Employee Code";
                    worksheet.Cells[currentRow, 3].Value = "Name";
                    worksheet.Cells[currentRow, 4].Value = "Designation";
                    worksheet.Cells[currentRow, 5].Value = "Joining Date";
                    worksheet.Cells[currentRow, 6].Value = "Gross Salary";
                    worksheet.Cells[currentRow, 7].Value = "Email";
                    worksheet.Cells[currentRow, 8].Value = "Date of Birth";
                    worksheet.Cells[currentRow, 9].Value = "Gender";

                    using (var range = worksheet.Cells[currentRow, 1, currentRow, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    currentRow++;

                    // Employee Data
                    foreach (var employee in department)
                    {
                        worksheet.Cells[currentRow, 1].Value = employee.EmployeeID;
                        worksheet.Cells[currentRow, 2].Value = employee.EmployeeCode;
                        worksheet.Cells[currentRow, 3].Value = employee.FullName;
                        worksheet.Cells[currentRow, 4].Value = employee.DesignationName;
                        worksheet.Cells[currentRow, 5].Value = employee.JoiningDate?.ToString("dd/MM/yyyy") ?? "";
                        worksheet.Cells[currentRow, 6].Value = employee.GrossSalary;
                        worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0";
                        worksheet.Cells[currentRow, 7].Value = employee.Email;
                        worksheet.Cells[currentRow, 8].Value = employee.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                        worksheet.Cells[currentRow, 9].Value = employee.Gender;

                        using (var range = worksheet.Cells[currentRow, 1, currentRow, 9])
                        {
                            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        currentRow++;
                    }

                    // Total Employees
                    worksheet.Cells[currentRow, 1, currentRow, 9].Merge = true;
                    worksheet.Cells[currentRow, 1].Value = $"Total Employees: {department.Count()}";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    currentRow += 2; // Add spacing between departments
                }

                worksheet.Cells[1, 1, currentRow - 1, 9].AutoFitColumns();

                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return stream.ToArray();
                }
            }


            //using (var package = new ExcelPackage())
            //{
            //    var officialData = await _employeeOfficialService.GetAllEmployeeOfficialDetailsAsync();
            //    var personalData = await _employeePersonalService.GetAllEmployeePersonalAsync();
            //    var salaryData = await _employeeSalaryService.GetAllEmployeeSalaryAsync();

            //    var employees = officialData.Select(o => new
            //    {
            //        EmployeeID = o.EmployeePersonalId,
            //        DepartmentName = o.DepartmentName ?? "Unknown",
            //        DesignationName = o.DesignationName ?? "",
            //        JoiningDate = o.JoiningDate,
            //        FullName = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId)?.FirstName ?? "",
            //        Email = personalData.FirstOrDefault(p => p.EmployeeID == o.EmployeePersonalId)?.Email ?? "",
            //        GrossSalary = salaryData.FirstOrDefault(s => s.EmployeePersonalId == o.EmployeePersonalId)?.Salary ?? 0
            //    }).OrderBy(e => e.DepartmentName).ToList();

            //    var departmentGroups = employees.GroupBy(e => e.DepartmentName).ToList();

            //    if (!departmentGroups.Any())
            //    {
            //        throw new Exception("No employee data found.");
            //    }

            //    foreach (var department in departmentGroups)
            //    {
            //        var worksheet = package.Workbook.Worksheets.Add(department.Key.Length > 31 ? department.Key.Substring(0, 31) : department.Key);

            //        worksheet.Cells[1, 1, 1, 6].Merge = true;
            //        worksheet.Cells[1, 1].Value = "ABC Corporation";
            //        worksheet.Cells[1, 1].Style.Font.Size = 14;
            //        worksheet.Cells[1, 1].Style.Font.Bold = true;
            //        worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            //        worksheet.Cells[2, 1, 2, 6].Merge = true;
            //        worksheet.Cells[2, 1].Value = "123 Business Street, City, Country";
            //        worksheet.Cells[2, 1].Style.Font.Size = 12;
            //        worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            //        worksheet.Cells[3, 1, 3, 6].Merge = true;
            //        worksheet.Cells[3, 1].Value = $"Report Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            //        worksheet.Cells[3, 1].Style.Font.Size = 10;
            //        worksheet.Cells[3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            //        worksheet.Cells[5, 1].Value = "Employee ID";
            //        worksheet.Cells[5, 2].Value = "Name";
            //        worksheet.Cells[5, 3].Value = "Designation";
            //        worksheet.Cells[5, 4].Value = "Joining Date";
            //        worksheet.Cells[5, 5].Value = "Gross Salary";
            //        worksheet.Cells[5, 6].Value = "Email";

            //        using (var range = worksheet.Cells[5, 1, 5, 6])
            //        {
            //            range.Style.Font.Bold = true;
            //            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            //            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            //            range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //        }

            //        int row = 6;
            //        foreach (var employee in department)
            //        {
            //            worksheet.Cells[row, 1].Value = employee.EmployeeID;
            //            worksheet.Cells[row, 2].Value = employee.FullName;
            //            worksheet.Cells[row, 3].Value = employee.DesignationName;
            //            worksheet.Cells[row, 4].Value = employee.JoiningDate?.ToString("dd/MM/yyyy") ?? "";
            //            worksheet.Cells[row, 5].Value = employee.GrossSalary;
            //            worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";
            //            worksheet.Cells[row, 6].Value = employee.Email;

            //            using (var range = worksheet.Cells[row, 1, row, 6])
            //            {
            //                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //            }
            //            row++;
            //        }

            //        worksheet.Cells[1, 1, row - 1, 6].AutoFitColumns();
            //    }

            //    using (var stream = new MemoryStream())
            //    {
            //        package.SaveAs(stream);
            //        return stream.ToArray();
            //    }
            //}
        }

    }
}
