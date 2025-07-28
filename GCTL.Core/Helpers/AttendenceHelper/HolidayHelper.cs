using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.AttendenceHelper
{
    public class HolidayHelper
    {
        private readonly string _connectionString;

        public HolidayHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connection"); // Assuming this is stored in appsettings.json
        }

        // Method to get active holidays based on OrganizationID and a date range
        public List<Holidays> GetActiveHolidays(int organizationId, DateTime startDate, DateTime endDate)
        {
            List<Holidays> holidays = new List<Holidays>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query to get active holiday information
                string holidayQuery = @"
                SELECT h.HolidayID, h.HolidayTitle, h.HolidayDescription, h.StartDate, h.EndDate, h.TotalDays, h.StatusID 
                FROM Holidays h
                INNER JOIN Statuses s ON h.StatusID = s.StatusID
                WHERE h.OrganizationID = @OrganizationID
                AND h.StartDate >= @StartDate
                AND h.EndDate <= @EndDate
                AND s.StatusName = 'Active'";  // Only Active holidays

                SqlCommand command = new SqlCommand(holidayQuery, connection);
                command.Parameters.AddWithValue("@OrganizationID", organizationId);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        holidays.Add(new Holidays
                        {
                            HolidayID = reader.GetInt32(0),  // Assuming this field is not NULL
                            HolidayTitle = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),  // Handle NULLs
                            HolidayDescription = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),  // Handle NULLs
                            StartDate = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),  // Handle NULLs
                            EndDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),  // Handle NULLs
                            TotalDays = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),  // Handle NULLs
                            StatusID = reader.GetInt32(6)  // Assuming this field is not NULL
                        });
                    }
                }
            }

            return holidays;
        }
    }
}
