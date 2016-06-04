using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TestingPower
{
    class Program
    {
        private static RemoteWebDriver driver;
        private static string _browser = string.Empty;
        private static int _loops = 1;
        private static List<Scenario> scenarios = new List<Scenario>();
        private static Dictionary<string, Scenario> possibleScenarios = new Dictionary<string, Scenario>();

        private static void Main(string[] args)
        {
            CreatePossibleScenarios();

            ProcessArgs(args);

            List<UserInfo> logins = GetLoginsFromFile();
            
            // Core Execution Loop
            using (var driver = CreateDriverAndMazimize(_browser))
            {
                Stopwatch watch = Stopwatch.StartNew();
                bool isFirstScenario = true;
                for (int loop = 0; loop < _loops; loop++)
                {
                    foreach (var scenario in scenarios)
                    {
                        var startTime = watch.Elapsed;
                        if (!isFirstScenario)
                        {
                            CreateNewTab();
                        }
                        else
                        {
                            isFirstScenario = false;
                        }

                        scenario.Run(driver, _browser, logins);

                        var runTime = watch.Elapsed.Subtract(startTime);
                        Thread.Sleep(TimeSpan.FromSeconds(scenario.Duration).Subtract(runTime));
                    }
                }

                // CloseTabs crashes Opera
                // CloseTabs();
            }
        }

        private static void CreatePossibleScenarios()
        {
            AddScenario(new OpenFacebookAndScroll());
            AddScenario(new OpenGmail());
            AddScenario(new OpenMsn());
            AddScenario(new OpenMsnbcAndScroll());
            AddScenario(new OpenOutlookAndArrow10Emails());
            AddScenario(new OpenRedditAndSearch());
            AddScenario(new OpenWikipediaAndScroll());
            AddScenario(new PlayYoutubeVideo());
            AddScenario(new SearchAmazon());
            AddScenario(new SearchGoogle());
        }

        private static void AddScenario(Scenario scenario)
        {
            possibleScenarios.Add(scenario.Name, scenario);
        }

        private static void ProcessArgs(string[] args)
        {
            Console.WriteLine("Usage: TestingPower.exe -browser [chrome|edge|firefox|opera|operabeta] -scenario all|<scenario1> <scenario2> [-loops <loopcount>]");
            for (int argNum = 0; argNum < args.Length;  argNum++)
            {
                var arg = args[argNum].ToLowerInvariant();
                switch (arg)
                {
                    case "-browser":
                        argNum++;
                        _browser = args[argNum].ToLowerInvariant();
                        switch (_browser)
                        {
                            case "operabeta":
                            case "opera":
                            case "chrome":
                            case "firefox":
                            case "edge":
                                break;
                            default:
                                throw new Exception($"Unexpected browser '{_browser}'");
                        }

                        break;
                    case "-scenario":
                        argNum++;

                        if (args[argNum] == "all")
                        {
                            // Specify the "official" runs, including order
                            scenarios.Add(possibleScenarios["youtube"]);
                            scenarios.Add(possibleScenarios["amazon"]);
                            // Reddit and amazon combined hang Opera.
                            // Re-ordering them causes the other to crash.
                            // Choosing amazon per higher Alexa rating.
                            //scenarios.Add(possibleScenarios["reddit"]);
                            scenarios.Add(possibleScenarios["facebook"]);
                            scenarios.Add(possibleScenarios["google"]);
                            scenarios.Add(possibleScenarios["gmail"]);
                            scenarios.Add(possibleScenarios["wikipedia"]);
                            break;
                        }

                        while (argNum < args.Length)
                        {
                            var scenario = args[argNum];
                            if (!possibleScenarios.ContainsKey(scenario))
                            {
                                throw new Exception($"Unexpected scenario '{scenario}'");
                            }

                            scenarios.Add(possibleScenarios[scenario]);
                            int nextArgNum = argNum + 1;
                            if (nextArgNum < args.Length && args[argNum+1].StartsWith("-"))
                            {
                                break;
                            }

                            argNum++;
                        }

                        break;
                    case "-loops":
                        argNum++;
                        _loops = int.Parse(args[argNum]);
                        break;
                    default:
                        throw new Exception($"Unexpected argument encountered '{args[argNum]}'");
                }
            }
        }

        private static void CloseTabs()
        {
            foreach (var window in driver.WindowHandles)
            {
                driver.SwitchTo().Window(window).Close();

            }
        }

        private static List<UserInfo> GetLoginsFromFile()
        {
            string jsonText = File.ReadAllText("config.json");
            return JsonConvert.DeserializeObject<List<UserInfo>>(jsonText);
        }

        public static void scrollPage(int timesToScroll)
        {
            for (int i = 0; i < timesToScroll; i++)
            {
                driver.Keyboard.SendKeys(Keys.PageDown);
                Thread.Sleep(1000);
            }
        }

        private static void CreateNewTab()
        {
            if (_browser == "firefox")
            {
                IWebElement body = driver.FindElementByTagName("body");
                body.SendKeys(Keys.Control + 't');                
            }
            else
            {
                driver.ExecuteScript("window.open();");
                // Go to that tab
                driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count-1]);
            }

            Thread.Sleep(2000);
        }
        
        private static RemoteWebDriver CreateDriverAndMazimize(string browser)
        {
            switch (browser)
            {
                case "opera":
                case "operabeta":
                    OperaOptions oOption = new OperaOptions();
                    oOption.AddArgument("--disable-popup-blocking");
                    if (browser == "operabeta")
                    {
                        oOption.BinaryLocation = @"C:\Program Files (x86)\Opera beta\38.0.2220.25\opera.exe";
                    }
                    driver = new OperaDriver(oOption);
                    break;
                case "firefox":
                    driver = new FirefoxDriver();
                    break;
                case "chrome":
                    ChromeOptions option = new ChromeOptions();
                    option.AddUserProfilePreference("profile.default_content_setting_values.notifications", 1);
                    driver = new ChromeDriver(option);
                    break;
                default:
                    EdgeOptions options = new EdgeOptions();
                    options.PageLoadStrategy = EdgePageLoadStrategy.Normal;                    
                    driver = new EdgeDriver(options);
                    break;
            }

            // This sleep exists to give enough time to click the Power Save button in Opera 38
            // Ideally, this would have a flag and this sleep woudl be deleted but we haven't found one yet.
            Thread.Sleep(3000);
            driver.Manage().Window.Maximize();
            Thread.Sleep(1000);

            return driver;
        }
        private static void Teardown()
        {
            // 9)	STOP: Capture remaining battery - We’ll capture this thru windows APIs
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
        }
        private static bool IsAlertPresent()
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }
    }
}