using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.AttendenceHelper
{
    public class WeekendHelper
    {
        private readonly string _connectionString;

        public WeekendHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connection"); // Assuming this is stored in appsettings.json
        }

        // Method to get weekend/holiday dates based on OrganizationID, OrganizationBranchID and a date range
        // Method to get weekend weekday numbers based on OrganizationID and OrganizationBranchID
        public List<int> GetWeekendWeekdayNumbers(int organizationId, int? organizationBranchId)
        {
            List<int> weekendWeekdays = new List<int>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query to get the weekend settings
                string weekendSettingsQuery = @"
                SELECT ws.WeekendSettingID, wd.WeekendDayID, wd.WeekdayNumber
                FROM WeekendSettings ws
                INNER JOIN WeekendDays wd ON ws.WeekendSettingID = wd.WeekendSettingID
                WHERE ws.OrganizationID = @OrganizationID
                AND (@OrganizationBranchID IS NULL OR ws.OrganizationBranchID = @OrganizationBranchID)";  // Handle NULL for OrganizationBranchID

                SqlCommand command = new SqlCommand(weekendSettingsQuery, connection);
                command.Parameters.AddWithValue("@OrganizationID", organizationId);
                command.Parameters.AddWithValue("@OrganizationBranchID", (object)organizationBranchId ?? DBNull.Value);  // Pass NULL if branch ID is nullable

                using (var reader = command.ExecuteReader())
                {
                    // Store the weekend days in a list (weekday numbers)
                    while (reader.Read())
                    {
                        int weekdayNumber = reader.GetInt32(2);  // Get WeekdayNumber (Saturday=6, Sunday=7, etc.)
                        weekendWeekdays.Add(weekdayNumber);
                    }
                }
            }

            return weekendWeekdays;
        }
    }
}
