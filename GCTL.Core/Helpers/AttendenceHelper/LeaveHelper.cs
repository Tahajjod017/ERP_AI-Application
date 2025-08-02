using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.AttendenceHelper
{
    public class LeaveHelper
    {
        private readonly string _connectionString;

        public LeaveHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connection"); // Assuming this is stored in appsettings.json
        }

        // Method to get leave dates and their corresponding leave type names for a date range
        public List<(DateTime leaveDate, string leaveTypeName)> GetLeaveDatesAndTypes(int employeeId, DateTime startDate, DateTime endDate)
        {
            var leaveDetails = new List<(DateTime leaveDate, string leaveTypeName)>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query to get leave details (leave date and leave type) for leave applications within the date range
                string leaveQuery = @"
            SELECT la.FromDate, la.ToDate, lt.LeaveTypeName
            FROM LeaveApplications la
            INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
            WHERE la.EmployeeID = @EmployeeID
            AND la.IsFinalApproved = 1
            AND ((la.FromDate BETWEEN @StartDate AND @EndDate) 
            OR (la.ToDate BETWEEN @StartDate AND @EndDate) 
            OR (@StartDate BETWEEN la.FromDate AND la.ToDate)
            OR (@EndDate BETWEEN la.FromDate AND la.ToDate))";  // Handling overlapping date ranges

                SqlCommand command = new SqlCommand(leaveQuery, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime fromDate = reader.GetDateTime(0);  // Get FromDate
                        DateTime toDate = reader.GetDateTime(1);    // Get ToDate
                        string leaveTypeName = reader.GetString(2); // Get LeaveTypeName

                        // Add each leave day within the range
                        if (fromDate.Date == toDate.Date)
                        {
                            leaveDetails.Add((fromDate.Date, leaveTypeName));  // Single day leave
                        }
                        else
                        {
                            for (DateTime date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
                            {
                                leaveDetails.Add((date, leaveTypeName));  // Multiple days of leave
                            }
                        }
                    }
                }
            }

            return leaveDetails;
        }
    }

}
