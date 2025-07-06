using GCTL.Core.ViewModels.Language;
using GCTL.Data.Models;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.UserProfile;

namespace GCTL_App.Controllers.Language
{
    public class LanguageController : BaseController
    {
        private readonly ILanguageTableService _languageTableService;
        private readonly ITranslateService _translationService;
        private readonly AppDbContext _dbContext;
        private readonly IHubContext<ProgressHub> _hubContext;

        public LanguageController(ITranslateService translateService, IUserProfileService userProfileService, ILanguageTableService languageTableService, AppDbContext dbContext, IHubContext<ProgressHub> hubContext) : base(translateService, userProfileService)
        {
            _translationService = translateService;
            _languageTableService = languageTableService;
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var s = _languageTableService.GetLangInd();
            ViewBag.AvLangInd = s.Result;
            return View();
        }


        public IActionResult Index2()
        {
            var s = _languageTableService.GetLangInd();
            ViewBag.AvLangInd = s.Result;
            return View();
        }

        public IActionResult GetLanguageOnSession()
        {
            var languageCode = HttpContext.Items["Language"] as string ?? "en";
            return Ok(languageCode);
        }

        public IActionResult TranslateToMultipleInd()
        {



            //var distinctTranslationCodes = _dbContext.Translation
            //    .Select(trans => trans.LanguageCode)
            //    .Distinct();

            var result = _dbContext.LanguageLists.ToList();
            var s = _languageTableService.GetLangInd();
            ViewBag.AvLangInd = s.Result;


            return View(result);
        }


        public IActionResult GetTranslationsTable(string languageCode)
        {
            // var translations = _languageTableService.GetTranslations(languageCode);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> TranslateToMultipleInd(string languageCode, string translationQuality)
        {
            int sleepTime;

            switch (translationQuality)
            {
                case "Good":
                    sleepTime = 2000; // 1 second
                    break;
                case "Better":
                    sleepTime = 3000; // 2 seconds
                    break;
                case "Best":
                    sleepTime = 4000; // 3 seconds
                    break;
                default:
                    sleepTime = 2000; // Default to 2 seconds if no quality is selected
                    break;
            }

            int o = 1;

            await _languageTableService.AddTableWithInd(languageCode);



            var getData = await _languageTableService.GetDataWithIndAsync(languageCode);
            var englishTexts = await _dbContext.LanguageMainTables.ToListAsync();

            var engTextList = getData
                .Where(x => !string.IsNullOrWhiteSpace(x.EngText))
                .Select(x => x.EngText.ToLower())
                .ToList();

            englishTexts = englishTexts
                .Where(et => !string.IsNullOrWhiteSpace(et.EnglishText) &&
                             !engTextList.Contains(et.EnglishText.ToLower()))
                .ToList();


            int total = englishTexts.Count;
            ViewBag.total = total;

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            using (var driver = new ChromeDriver(options))
            {
                try
                {
                    List<TableDataIndDto> listDto = new List<TableDataIndDto>();

                    string url = $"https://translate.google.com/?hl=en&sl=en&tl={languageCode}&op=translate";
                    driver.Navigate().GoToUrl(url);

                    Thread.Sleep(sleepTime);

                    var sourceTextBox = driver.FindElement(By.XPath("//textarea[@aria-label='Source text']"));

                    foreach (var item in englishTexts)
                    {
                        sourceTextBox.Clear();
                        sourceTextBox.SendKeys(item.EnglishText);
                        Thread.Sleep(sleepTime + 1000);

                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        var translationContainer = wait.Until(d => d.FindElement(By.ClassName("usGWQd")));
                        var result = translationContainer.Text;

                        var tableDataIndDto = new TableDataIndDto
                        {
                            EngText = item.EnglishText,
                            LangCode = languageCode,
                            TranslateText = result,

                        };

                        listDto.Add(tableDataIndDto);
                        // await _languageTableService.SaveDataWithInd(tableDataIndDto);


                        int progress = (o++ * 100) / total;
                        await _hubContext.Clients.All.SendAsync("UpdateProgress", progress, total);

                    }

                    await _languageTableService.SaveBulkDataWithInd(listDto);


                    await _hubContext.Clients.All.SendAsync("OperationCompleted");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error during translation: {ex.Message}");
                }
                finally
                {
                    driver.Quit();
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult ChangeLanguage(string languageCode)
        {
            if (languageCode == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Response.Cookies.Append("Language", languageCode, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult GetTranslationsTable11(string languageCode)
        {
            var translations = _languageTableService.GetTranslationsTableData(languageCode);
            return Ok(translations);
        }

        public async Task<IActionResult> UpdateTranslationData(LanguageUpdateVM updateVM)
        {
            var translations = await _languageTableService.UpdateTranslateAsync(updateVM);
            return Ok(translations);
        }




    }
}
