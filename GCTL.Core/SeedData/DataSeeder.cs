//using Bogus;
//using GCTL.Data.Models;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GCTL.Core.SeedData
//{
//    public class DataSeeder
//    {
//        private readonly AppDbContext _context;

//        public DataSeeder(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task SeedEmployeesAsync(AppDbContext context)
//        {
//            var employeeFaker = new Faker<Employees>()
//                .RuleFor(e => e.FirstName, f => f.Name.FirstName())
//                .RuleFor(e => e.LastName, f => f.Name.LastName())
//                .RuleFor(e => e.FatherName, f => f.Name.FullName())
//                .RuleFor(e => e.MotherName, f => f.Name.FullName())
//                .RuleFor(e => e.MobileNumber, f => f.Phone.PhoneNumber("017########"))
//                .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName))
//                .RuleFor(e => e.BirthCertificateNo, f => f.Random.AlphaNumeric(10))
//                .RuleFor(e => e.TIN, f => f.Random.Replace("##########"))
//                .RuleFor(e => e.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Between(new DateTime(1985, 1, 1), new DateTime(2005, 12, 31))))
//                .RuleFor(e => e.AboutEmployee, f => f.Lorem.Sentence())
//                .RuleFor(e => e.NID, f => f.Random.Replace("###########"))
//                .RuleFor(e => e.State, f => f.Address.State())
//                .RuleFor(e => e.City, f => f.Address.City())
//                .RuleFor(e => e.HouseNo, f => f.Random.Number(1, 300).ToString())
//                .RuleFor(e => e.RoadNo, f => f.Random.Number(1, 50).ToString())
//                .RuleFor(e => e.PostalCode, f => f.Address.ZipCode())
//                .RuleFor(e => e.LIP, f => f.Internet.Ip())
//                .RuleFor(e => e.LMAC, f => f.Internet.Mac())
//                .RuleFor(e => e.HasUser, f => f.Random.Bool(0.3f)) // 30% have user
//                .RuleFor(e => e.EmployeeCode, f => $"EMP{f.Random.Number(100000, 999999)}")
//                .RuleFor(e => e.IsActive, f => f.Random.Bool(0.9f)); // 90% active

//            var employees = employeeFaker.Generate(10000);

//            await context.Employees.AddRangeAsync(employees);
//            await context.SaveChangesAsync();
//        }


//        public async Task SeedEmployeeOfficeInfoAsync(AppDbContext context)
//        {
//            // Make sure Employees are seeded first
//            if (!context.Employees.Any())
//                return;

//            var employeeIds = await context.Employees
//                .Select(e => e.EmployeeID)
//                .ToListAsync();

//            var faker = new Faker();

//            var officeInfoList = employeeIds.Select(empId => new EmployeeOfficeInfo
//            {
//                EmployeeID = empId,
//                OrganizationID = faker.Random.Number(1, 1), // You can change range based on your real org data
//                DepartmentID = faker.Random.Number(1, 57),   // Adjust to valid department IDs
//                OfficePhone = faker.Phone.PhoneNumber("02########"),
//                OfficeEmail = faker.Internet.Email(),
//                AppointmentLetterNo = faker.Random.Replace("APPT-#####"),
//                AppointmentLetterIssueDate = DateOnly.FromDateTime(faker.Date.Past(3)),
//                JoiningDate = DateOnly.FromDateTime(faker.Date.Past(2)),
//                ProvisionPeriodStartDate = DateOnly.FromDateTime(faker.Date.Past(2)),
//                ProvisionPeriod = faker.Random.Number(3, 6), // In months
//                ConfirmationDate = DateOnly.FromDateTime(faker.Date.Recent(100)),
//                ConfirmationLetterNo = faker.Random.Replace("CONF-#####"),
//                LIP = faker.Internet.Ip(),
//                LMAC = faker.Internet.Mac(),
//                EmploymentStatusId = 1 // Assuming "1" means "Active" or "Employed"
//            }).ToList();

//            await context.EmployeeOfficeInfo.AddRangeAsync(officeInfoList);
//            await context.SaveChangesAsync();
//        }


//        public async Task SeedShiftsAsync(AppDbContext context)
//        {
//            var faker = new Faker();

//            // Step 1: Get all existing organizations
//            var organizationIds = await context.Organization
//                .Select(o => o.OrganizationID)
//                .ToListAsync();

//            var shiftNames = new[] { "Morning", "Evening", "Night", "Weekend", "Half Day", "Flexible" };
//            var shifts = new List<Shifts>();

//            // Step 2: For each organization, add all shift types
//            foreach (var orgId in organizationIds)
//            {
//                foreach (var name in shiftNames)
//                {
//                    var startTime = GetRandomTime(faker, new TimeOnly(6, 0), new TimeOnly(10, 0));
//                    var endTime = GetRandomTime(faker, new TimeOnly(14, 0), new TimeOnly(20, 0));

//                    shifts.Add(new Shifts
//                    {
//                        ShiftName = name,
//                        OrganizationID = orgId,

//                        StartTime = startTime,
//                        EndTime = endTime,

//                        IsLateCount = faker.Random.Bool(),
//                        IsAutomaticORManualBreakTime = faker.Random.Bool(),
//                        IsMealBreakCompulsaryOrComplementaryDeductWithShift = faker.Random.Bool(),
//                        IsAllowStartAndEndTime = faker.Random.Bool(),

//                        MealBreakStartTime = GetRandomTime(faker, new TimeOnly(12, 0), new TimeOnly(13, 0)),
//                        MealBreakEndTime = GetRandomTime(faker, new TimeOnly(13, 0), new TimeOnly(14, 0)),

//                        IsAllowOvertime = faker.Random.Bool(),

//                        LIP = faker.Internet.Ip(),
//                        LMAC = faker.Internet.Mac(),

//                        GraceTime = GetRandomTime(faker, new TimeOnly(0, 5), new TimeOnly(0, 15)),
//                        MinimumWorkingTime = new TimeOnly(6, 0),
//                        MinimumRequiredOvertime = new TimeOnly(1, 0),
//                        MaximumAllowedOvertime = new TimeOnly(4, 0),
//                        MealBreakTime = new TimeOnly(1, 0)
//                    });
//                }
//            }

//            await context.Shifts.AddRangeAsync(shifts);
//            await context.SaveChangesAsync();
//        }


//        private static TimeOnly GetRandomTime(Faker faker, TimeOnly start, TimeOnly end)
//        {
//            var startDateTime = DateTime.Today.Add(start.ToTimeSpan());
//            var endDateTime = DateTime.Today.Add(end.ToTimeSpan());
//            var randomDateTime = faker.Date.Between(startDateTime, endDateTime);
//            return TimeOnly.FromDateTime(randomDateTime);
//        }
//    }
//}