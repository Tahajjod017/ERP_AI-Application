using System;
using System.Collections.Generic;
using System.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Language;
using GCTL.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GCTL.Service.Language
{



    public class LanguageTableService : ILanguageTableService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly AppDbContext _dbContext;

        public LanguageTableService(AppDbContext context, IConfiguration configuration, AppDbContext dbContext)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("connection");
            _dbContext = dbContext;
        }

        public async Task<bool> TableExistsWithIndRealOld(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code cannot be empty.");

            var tableName = $"LanguageInd_{languageCode}";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync(); // open database

            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = @tableName
                ) THEN 1 ELSE 0 END";

            var param = command.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            command.Parameters.Add(param);

            var result = (int)(await command.ExecuteScalarAsync());
            await connection.CloseAsync();
            return result == 1;
        }

        public async Task<string> GetTableNameWithInd(string languageCode)
        {
            var tableName = $"LanguageInd_{languageCode}";
            return await Task.FromResult(tableName);
        }

        public async Task<string> GetTranslateWithInd(string tableName, string key)
        {
            //return  key;
            try
            {
                var selectSql = $@"
            SELECT TranslatedText FROM {tableName} WHERE EngText = @Key
        ";

                //  using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(selectSql, connection))
                    {
                        command.Parameters.AddWithValue("@Key", key);

                        var result = await command.ExecuteScalarAsync();
                        await connection.CloseAsync();
                        return result?.ToString();
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task AddTableWithInd(string languageCode)
        {
            //var languageName = _context.LanguageLists.Where(e => e.Equals(languageCode)).Select(r => r.LanguageName).FirstOrDefault();

            var languageName = languageCode;

            var tableName = $"LanguageInd_{languageName}";

            // Validate the language code
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code cannot be empty.");

            var res = await TableExistsWithIndReal(languageCode);



            if (!res)
            {

                var supportQuery = $@"
                            CREATE TABLE {tableName} (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                EngText NVARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS,
                                TranslatedText NVARCHAR(MAX) COLLATE Indic_General_90_CI_AS
                            )";



                var createTableSql = $@"
                    CREATE TABLE {tableName} (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        EngText NVARCHAR(MAX),
                        TranslatedText NVARCHAR(MAX)
                    )";
                await _context.Database.ExecuteSqlRawAsync(supportQuery);
            }
        }


        public async Task SaveDataWithInd(TableDataIndDto model)
        {
            try
            {
                //var languageName = _context.LanguageLists.Where(e => e.Equals(model.LangCode)).Select(r => r.LanguageName).FirstOrDefault();
                var languageName = model.LangCode;

                var tableName = $"LanguageInd_{languageName}";

                var insertSql = $@"
                    INSERT INTO {tableName} (EngText, TranslatedText)
                    VALUES (N'{model.EngText}', N'{model.TranslateText}')";

                await _context.Database.ExecuteSqlRawAsync(insertSql);
                // await _context.Database.ExecuteSqlRawAsync(insertSql, new SqlParameter("@translatedText", translatedText));
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<string>> GetLangInd()
        {

            var languageCodes = new List<string>();
            string query = @"
            SELECT 
              REPLACE(table_name, 'LanguageInd_', '') AS LanguageName
            FROM 
              information_schema.tables
            WHERE 
              table_name LIKE 'LanguageInd_%';";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            languageCodes.Add(reader["LanguageName"].ToString());
                        }
                    }
                }
            }

            if (!languageCodes.Contains("en"))
            {
                languageCodes.Insert(0, "en");
            }


            return languageCodes;
        }

        public async Task SaveBulkDataWithInd(List<TableDataIndDto> listDto)
        {
            try
            {
                foreach (var model in listDto)
                {
                    var languageName = model.LangCode;
                    var tableName = $"LanguageInd_{languageName}";

                    // Step 1: Check if EngText exists
                    var existingTranslatedText = await GetExistingTranslatedTextAsync(tableName, model.EngText);

                    if (existingTranslatedText == null)
                    {
                        // Insert new
                        var insertSql = $@"
                            INSERT INTO {tableName} (EngText, TranslatedText)
                            VALUES (N'{model.EngText.Replace("'", "''")}', N'{model.TranslateText.Replace("'", "''")}')";

                        await _context.Database.ExecuteSqlRawAsync(insertSql);
                    }
                    else if (existingTranslatedText != model.TranslateText)
                    {
                        // Update existing
                        var updateSql = $@"
                    UPDATE {tableName}
                    SET TranslatedText = N'{model.TranslateText.Replace("'", "''")}'
                    WHERE EngText = N'{model.EngText.Replace("'", "''")}'";

                        await _context.Database.ExecuteSqlRawAsync(updateSql);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<string> GetExistingTranslatedTextAsync(string tableName, string engText)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT TOP 1 TranslatedText 
                FROM {tableName} 
                WHERE EngText = N'{engText.Replace("'", "''")}'";
            command.CommandType = CommandType.Text;

            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            return result != DBNull.Value && result != null ? result.ToString() : null;
        }


        public async Task<bool> TableExistsWithIndReal(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code cannot be empty.");

            var tableName = $"LanguageInd_{languageCode}";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = @tableName
                ) THEN 1 ELSE 0 END";
            command.CommandType = CommandType.Text;

            var param = command.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            command.Parameters.Add(param);

            var result = (int)(await command.ExecuteScalarAsync());
            await connection.CloseAsync();
            return result == 1;
        }

        public bool TableExistsWithIndRealNan(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code cannot be empty.");

            var tableName = $"LanguageInd_{languageCode}";

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = @tableName
                ) THEN 1 ELSE 0 END";
            command.CommandType = CommandType.Text;

            var param = command.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            command.Parameters.Add(param);

            var result = (int)(command.ExecuteScalar());
            connection.Close();
            return result == 1;
        }

        public string GetTableNameWithIndNan(string languageCode)
        {
            var tableName = $"LanguageInd_{languageCode}";
            return tableName;
        }

        public string GetTranslateWithIndNan(string tableName, string key)
        {
            try
            {
                var selectSql = $@"
            SELECT TranslatedText FROM {tableName} WHERE EngText = @Key
        ";

                //  using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(selectSql, connection))
                    {
                        command.Parameters.AddWithValue("@Key", key);

                        var result = command.ExecuteScalar();
                        connection.Close();
                        return result?.ToString();
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<CommonLanguageVM>> GetDataWithIndAsync(string languageCode)
        {
            var tableName = $"LanguageInd_{languageCode}";
            var results = new List<CommonLanguageVM>();

            var query = $"SELECT [Id], [EngText], [TranslatedText] FROM [{tableName}]";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new CommonLanguageVM
                        {
                            Id = reader.GetInt32(0),
                            EngText = reader.IsDBNull(1) ? null : reader.GetString(1),
                            TranslatedText = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return results;
        }


        public List<CommonLanguageVM> GetDataWithInd(string languageCode)
        {
            var tableName = $"LanguageInd_{languageCode}";
            var results = new List<CommonLanguageVM>();

            var query = $"SELECT [Id], [EngText], [TranslatedText] FROM [{tableName}]";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new CommonLanguageVM
                        {
                            Id = reader.GetInt32(0),
                            EngText = reader.IsDBNull(1) ? null : reader.GetString(1),
                            TranslatedText = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
                connection.Close();
            }

            return results;
        }

        public List<CommonLanguageVM> GetTranslationsTableData(string languageCode)
        {
            try
            {
                if (languageCode == "en")
                {
                    var tes = _dbContext.LanguageMainTables.Select(e => new CommonLanguageVM
                    {
                        Id = e.ID,
                        EngText = e.EnglishText,
                        TranslatedText = e.EnglishText,
                        LangCode = languageCode

                    }).ToList();

                    return tes;
                }

                var getData = GetDataWithInd(languageCode); // Ensure this retrieves valid data

                var result = (from lm in _dbContext.LanguageMainTables.AsEnumerable() // Convert to in-memory collection
                              join td in getData.AsEnumerable() // Ensure getData isn't causing translation issues
                              on lm.EnglishText equals td.EngText into leftJoin
                              from td in leftJoin.DefaultIfEmpty()
                              select new CommonLanguageVM
                              {
                                  Id = lm.ID,
                                  LangId = td != null ? td.Id : 0,
                                  EngText = lm.EnglishText,
                                  TranslatedText = td != null ? td.TranslatedText : "",
                                  LangCode = languageCode
                              }).ToList();

                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<CommonReturnViewModel> UpdateTranslateAsync(LanguageUpdateVM updateVM)
        {
            if (updateVM == null)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = "Data Null",
                };
            }

            var tableName = $"LanguageInd_{updateVM.LangCode}";

            var tableExists = TableExistsWithIndRealNan(updateVM.LangCode);

            if (!tableExists)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = "Table Not exists",
                };
            }




            if (updateVM.LangId == 0)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = "Update Language Transaction first",
                };
            }

            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE EngText = @EngText";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@EngText";
            parameter.Value = updateVM.EngText;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            int count = Convert.ToInt32(result);
            await connection.CloseAsync();



            if (count <= 0)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = "Update Language Transaction first",
                };
            }



            string updateQuery = $"UPDATE {tableName} SET TranslatedText = N'{updateVM.TranslatedText}' WHERE Id = {updateVM.LangId}";

            // Execute the raw SQL command
            int rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(updateQuery);

            if (rowsAffected > 0)
            {
                return new CommonReturnViewModel()
                {
                    Success = true,
                    Message = "Translation updated successfully",
                };
            }
            else
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = "Failed to update translation",
                };
            }
        }
    }

}
