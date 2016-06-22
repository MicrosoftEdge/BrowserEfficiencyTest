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
        private static string browser = string.Empty;
        private static int loops = 1;
        private static List<Scenario> scenarios = new List<Scenario>();
        private static Dictionary<string, Scenario> possibleScenarios = new Dictionary<string, Scenario>();

        private static void Main(string[] args)
        {
            // A "Scenario" is opening up a tab and doing something (watch a youtube video, browse the facebook feed)
            CreatePossibleScenarios();

            ProcessArgs(args);

            List<UserInfo> logins = GetLoginsFromFile();
            
            // Core Execution Loop
            using (var driver = CreateDriverAndMazimize(browser))
            {
                Stopwatch watch = Stopwatch.StartNew();
                bool isFirstScenario = true;

                // Allow multiple loops of all the scenarios if the user desires. Great for compounding small
                // differences to make them easier to measure.
                for (int loop = 0; loop < loops; loop++)
                {
                    foreach (var scenario in scenarios)
                    {
                        // We want every scenario to take the same amount of time total, even if there are changes in
                        // how long pages take to load. The biggest reason for this is so that you can measure energy
                        // or power and their ratios will be the same either way.
                        // So start by getting the current time.
                        var startTime = watch.Elapsed;

                        // The first scenario naviagates in the browser's new tab / welcome page.
                        // After that, scenarios open in their own tabs
                        if (!isFirstScenario)
                        {
                            CreateNewTab();
                        }
                        else
                        {
                            isFirstScenario = false;
                        }

                        // Here, control is handed to the scenario to navigate, and do whatever it wants
                        scenario.Run(driver, browser, logins);

                        // When we get control back, we sleep for the remaining time for the scenario. This ensures
                        // the total time for a scenario is always the same
                        var runTime = watch.Elapsed.Subtract(startTime);
                        
                        // We even allow the exception to fall through and break the run if we pass a negative number
                        // in here (meaning the scenario took longer to return than the total time it expected)
                        var timeLeft = TimeSpan.FromSeconds(scenario.Duration).Subtract(runTime);
                        
                        if (timeLeft < TimeSpan.FromSeconds(0))
                        {                            
                            throw new Exception("Scenario ran longer than expected! The browser ran slower than expected or the duration of the scenario is too short.");
                        }
                        Thread.Sleep(timeLeft);
                    }
                }

                // CloseTabs crashes Opera
                // CloseTabs();
            }
        }

        /// <summary>
        /// All scenarios must be instantiated and added to the list of possible scenarios in this method.
        /// The order doensn't matter.
        /// </summary>
        private static void CreatePossibleScenarios()
        {
            // All scenarios are added to the list even if they're not final / not great. Order doesn't matter here.
            AddScenario(new FacebookNewsfeedScroll());
            AddScenario(new GmailGoThroughEmails());
            AddScenario(new Msn());
            AddScenario(new Msnbc());
            AddScenario(new OutlookViewEmails());
            AddScenario(new RedditSearchSubreddit());
            AddScenario(new WikipediaUnitedStates());
            AddScenario(new YoutubeWatchVideo());
            AddScenario(new AmazonSearch());
            AddScenario(new GoogleSearch());
            AddScenario(new CnnTopStory());
            AddScenario(new TechRadarSurfacePro4Review());
        }

        private static void AddScenario(Scenario scenario)
        {
            possibleScenarios.Add(scenario.Name, scenario);
        }

        /// <summary>
        /// Based on user input in the args, here we break them apart and determine:
        ///  - Which browser to run on
        ///  - Which scenario(s) to run
        ///  - How many loops to execute
        /// </summary>
        /// <param name="args">The input arguments. E.g. "-browser edge -scenario all"</param>
        private static void ProcessArgs(string[] args)
        {
            // Processes the arguments. Here we'll decide which browser, scenarios, and number of loops to run

            Console.WriteLine("Usage: TestingPower.exe -browser [chrome|edge|firefox|opera|operabeta] -scenario all|<scenario1> <scenario2> [-loops <loopcount>]");
            for (int argNum = 0; argNum < args.Length;  argNum++)
            {
                var arg = args[argNum].ToLowerInvariant();
                switch (arg)
                {
                    case "-browser":
                        argNum++;
                        browser = args[argNum].ToLowerInvariant();
                        switch (browser)
                        {
                            case "operabeta":
                            case "opera":
                            case "chrome":
                            case "firefox":
                            case "edge":
                                break;
                            default:
                                throw new Exception($"Unexpected browser '{browser}'");
                        }

                        break;
                    case "-scenario":
                        argNum++;

                        if (args[argNum] == "all")
                        {
                            // Specify the "official" runs, including order
                            scenarios.Add(possibleScenarios["youtube"]);
                            scenarios.Add(possibleScenarios["cnn"]);
                            scenarios.Add(possibleScenarios["techRadar"]);
                            scenarios.Add(possibleScenarios["amazon"]);
                            // Reddit and amazon combined hang Opera.
                            // Re-ordering them causes the other to crash.
                            // Choosing amazon per higher Alexa rating.
                            // scenarios.Add(possibleScenarios["reddit"]);
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
                        loops = int.Parse(args[argNum]);
                        break;
                    default:
                        throw new Exception($"Unexpected argument encountered '{args[argNum]}'");
                }
            }
        }

        private static void CloseTabs()
        {
            // Simply go through and close every tab one by one.
            foreach (var window in driver.WindowHandles)
            {
                driver.SwitchTo().Window(window).Close();

            }
        }

        private static List<UserInfo> GetLoginsFromFile()
        {
            // Get the usernames and passwords from an external file for any scenarios that need them.
            // Json makes this easy =)
            string jsonText = File.ReadAllText("config.json");
            return JsonConvert.DeserializeObject<List<UserInfo>>(jsonText);
        }

        /// <summary>
        /// Utility function for scenarios to call into to scroll the page
        /// </summary>
        /// <param name="timesToScroll">An abstract quantification of how much to scroll</param>
        public static void scrollPage(int timesToScroll)
        {
            // Webdriver examples had scrolling by executing Javascript. This seemed troublesome because the browser is
            // scrolling in a way very different from how it would with a real user. This seemed to be the best
            // compromise in terms of it behaving like a real user scrolling, and it working reliably across browsers.
            // Use the page down key.
            for (int i = 0; i < timesToScroll; i++)
            {
                driver.Keyboard.SendKeys(Keys.PageDown);
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// Creates a new tab and puts focus in it so the next navigation will be in the new tab
        /// </summary>
        private static void CreateNewTab()
        {
            // Sadly, we had to special case this a bit by browser because no mechanism behaved correctly for everyone
            if (browser == "firefox")
            {
                // Use ctrl+t for Firefox. Send them to the body or else there can be focus problems.
                IWebElement body = driver.FindElementByTagName("body");
                body.SendKeys(Keys.Control + 't');                
            }
            else
            {
                // For other browsers, use some JS. Note that this means you have to disable popup blocking in Edge
                // You actually have to in Opera too, but that's provided in a flag below
                driver.ExecuteScript("window.open();");
                // Go to that tab
                driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count-1]);
            }

            // Give the browser more than enough time to open the tab and get to it so the next commands from the
            // scenario don't get lost
            Thread.Sleep(2000);
        }
        
        private static RemoteWebDriver CreateDriverAndMazimize(string browser)
        {
            // Create a webdriver for the respective browser, depending on what we're testing.
            switch (browser)
            {
                case "opera":
                case "operabeta":
                    OperaOptions oOption = new OperaOptions();
                    oOption.AddArgument("--disable-popup-blocking");
                    oOption.AddArgument("--power-save-mode=on");
                    if (browser == "operabeta")
                    {
                        // TODO: Ideally, this code would look inside the Opera beta folder for opera.exe
                        // rather than depending on flaky hard-coded version in directory
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

            driver.Manage().Window.Maximize();

            Thread.Sleep(1000);

            return driver;
        }
        private static void Teardown()
        {
            // 9)	STOP: Capture remaining battery - We’ll capture this through windows APIs
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