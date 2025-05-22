using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.Language
{




    public class TranslateService : ITranslateService
    {
        private readonly ILanguageTableService _languageTableService;
        private readonly AppDbContext _context;

        public TranslateService(ILanguageTableService languageTableService, AppDbContext context)
        {
            _languageTableService = languageTableService;
            _context = context;
        }

        public async Task<string> GetTranslationAsyncInd(string key, string code, string languageCode)
        {
            try
            {
                var mainTblChk = _context.LanguageMainTables
                    .FirstOrDefault(mt => mt.EnglishText == key && mt.TextCode == code);

                if (mainTblChk == null)
                {
                    var mainTblChk2 = _context.LanguageMainTables
                    .FirstOrDefault(mt => mt.EnglishText == key);

                    if (mainTblChk2 == null)
                    {
                        AddNewEngText(key, code);
                        // UpdateEngTextByCode(key, code);
                    }
                    else
                    {

                    }


                }




                if (languageCode == "en")
                {
                    return key;
                }





                var tableExists = await _languageTableService.TableExistsWithIndReal(languageCode);

                if (!tableExists)
                {
                    return key;
                }

                var tableName = await _languageTableService.GetTableNameWithInd(languageCode);

                var text = await _languageTableService.GetTranslateWithInd(tableName, key);
                if (text != null)
                {
                    return text;
                }

                return key;
                //var translation = mainEntry?.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);



                //return translation?.TranslatedText ?? key; // Fallback to English text
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void UpdateEngTextByCode(string key, string code)
        {
            var mainTable = _context.LanguageMainTables
                .FirstOrDefault(mt => mt.TextCode == code);

            if (mainTable != null)
            {
                mainTable.EnglishText = key;
                mainTable.IsModified = true;
                _context.LanguageMainTables.Update(mainTable);
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Main table entry not found.");
            }
        }

        private void AddNewEngText(string key, string code)
        {
            try
            {
                LanguageMainTables mainTable = new LanguageMainTables
                {
                    TextCode = code,
                    EnglishText = key,
                    IsModified = false
                };
                _context.LanguageMainTables.Add(mainTable);
                _context.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public string GetTranslationInd(string key, string code, string languageCode)
        {
            try
            {
                var mainTblChk = _context.LanguageMainTables
                    .FirstOrDefault(mt => mt.EnglishText == key && mt.TextCode == code);

                if (mainTblChk == null)
                {
                    var mainTblChk2 = _context.LanguageMainTables
                    .FirstOrDefault(mt => mt.EnglishText == key);

                    if (mainTblChk2 == null)
                    {
                        AddNewEngText(key, code);
                        //UpdateEngTextByCode(key, code);
                    }
                    else
                    {

                    }


                }




                if (languageCode == "en")
                {
                    return key;
                }





                var tableExists = _languageTableService.TableExistsWithIndRealNan(languageCode);

                if (!tableExists)
                {
                    return key;
                }

                var tableName = _languageTableService.GetTableNameWithIndNan(languageCode);

                var text = _languageTableService.GetTranslateWithIndNan(tableName, key);
                if (text != null)
                {
                    return text;
                }

                return key;
                //var translation = mainEntry?.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);



                //return translation?.TranslatedText ?? key; // Fallback to English text
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
