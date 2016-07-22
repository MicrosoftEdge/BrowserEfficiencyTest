//--------------------------------------------------------------
//
// Microsoft Edge Power Test
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files(the ""Software""),
// to deal in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//--------------------------------------------------------------

using Elevator;
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
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace TestingPower
{
    internal class Program
    {
        private static string s_profilePath = "";
        private static int s_loops = 1;
        private static List<Scenario> s_scenarios = new List<Scenario>();
        private static Dictionary<string, Scenario> s_possibleScenarios = new Dictionary<string, Scenario>();

        private static readonly List<string> s_SupportedBrowsers = new List<string> { "chrome", "edge", "firefox", "opera" };
        private static bool s_doWarmup = false;
        private static List<string> s_browsers = new List<string>();
        private static int s_iterations = 1;
        private static string s_scenarioName = "";
        private static bool s_useTraceController;
        private static string s_etlPath = "";

        private static void Main(string[] args)
        {
            // A "Scenario" is opening up a tab and doing something (watch a youtube video, browse the facebook feed)
            CreatePossibleScenarios();

            ProcessArgs(args);

            List<UserInfo> logins = GetLoginsFromFile();

            // A warmup pass is one run thru the selected scenarios and browsers.
            // It allows the browsers to cache some content which helps reduce power variability from run to run.
            if (s_doWarmup)
            {
                Console.WriteLine("[{0}] - Starting warmup pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                foreach (string browser in s_browsers)
                {
                    using (var driver = CreateDriverAndMaximize(browser))
                    {
                        foreach (var scenario in s_scenarios)
                        {
                            Console.WriteLine("[{0}] - Warmup - Browser: {1}  Scenario: {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), browser, scenario.Name);

                            scenario.Run(driver, browser, logins);

                            Thread.Sleep(1 * 1000);
                        }
                        driver.Quit();
                    }
                }
                Console.WriteLine("[{0}] - Completed warmup pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (s_useTraceController)
            {
                Console.WriteLine("[{0}] - Waiting briefly to let E3 system clear out data from before running the test.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                // that the browser energy data reported by E3 going forward is from this test run and not from warmup or before running the test pass.
                Thread.Sleep(12 * 1000);
            }

            using (var elevatorClient = ElevatorClient.Create(s_useTraceController))
            {
                elevatorClient.ConnectAsync().Wait();
                elevatorClient.SendControllerMessageAsync(Elevator.Commands.START_PASS).Wait();

                Console.WriteLine("[{0}] - Starting Test Pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // Core Execution Loop
                for (int iteration = 0; iteration < s_iterations; iteration++)
                {
                    foreach (string browser in s_browsers)
                    {
                        elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_BROWSER} {browser} ITERATION {iteration} SCENARIO_NAME {s_scenarioName}").Wait();

                        Console.WriteLine("[{0}] - Launching Browser Driver {1} -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), browser);

                        using (var driver = CreateDriverAndMaximize(browser))
                        {
                            Stopwatch watch = Stopwatch.StartNew();
                            bool isFirstScenario = true;

                            // Allow multiple loops of all the scenarios if the user desires. Great for compounding small
                            // differences to make them easier to measure.
                            for (int loop = 0; loop < s_loops; loop++)
                            {
                                foreach (var scenario in s_scenarios)
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
                                        CreateNewTab(driver, browser);
                                    }
                                    else
                                    {
                                        isFirstScenario = false;
                                    }

                                    Console.WriteLine("[{0}] - Executing - Iteration: {1}  Browser: {2}  Scenario: {3}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), iteration, browser, scenario.Name);

                                    // Here, control is handed to the scenario to navigate, and do whatever it wants
                                    scenario.Run(driver, browser, logins);

                                    // When we get control back, we sleep for the remaining time for the scenario. This ensures
                                    // the total time for a scenario is always the same
                                    var runTime = watch.Elapsed.Subtract(startTime);
                                    var timeLeft = TimeSpan.FromSeconds(scenario.Duration).Subtract(runTime);
                                    if (timeLeft < TimeSpan.FromSeconds(0))
                                    {
                                        // Of course it's possible we don't get control back until after we were supposed to
                                        // continue to the next scenario. In that case, invalidate the run by throwing.
                                        throw new Exception(string.Format("Scenario ran longer than expected! The browser ran for {0}s. The timeout for this scenario is {1}s.", runTime.TotalSeconds, scenario.Duration));
                                    }

                                    Console.WriteLine("[{0}] - Completed - Iteration: {1}  Browser: {2}  Scenario: {3}. Scenario ran for {4} seconds.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), iteration, browser, scenario.Name, runTime.TotalSeconds);

                                    Thread.Sleep(timeLeft);
                                }
                            }

                            Console.WriteLine("[{0}] - Completed Browser: {1}  Iteration: {2} ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), browser, iteration);

                            driver.Quit();
                        }

                        if (s_useTraceController)
                        {
                            Console.WriteLine("[{0}] - Pausing for E3 System Events to clear -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                            // that the browser energy data reported by E3 for this run is only for this run and does not bleed into any other runs.
                            Thread.Sleep(12 * 1000);
                        }

                        elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.END_BROWSER} {browser}").Wait();
                    }
                }
                Console.WriteLine("[{0}] - Ending Test Pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                elevatorClient.SendControllerMessageAsync(Elevator.Commands.END_PASS).Wait();
            }

            // process the E3 Energy data from test traces if tracing controller was used
            if (s_useTraceController)
            {
                ProcessEnergyData(s_etlPath);
            }
        }

        /// <summary>
        /// Extracts the E3 Energy data from ETL files created during the test, aggregates the data and saves it to csv files.
        /// </summary>
        private static void ProcessEnergyData(string etlPath)
        {
            IEnumerable<string> etlFiles = null;
            AutomateXPerf xPerf = new AutomateXPerf();
            EnergyDataProcessor energyProcessor = new EnergyDataProcessor();

            Console.WriteLine("[{0}] - Starting processing of energy data. -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            etlFiles = Directory.EnumerateFiles(etlPath, "*.etl");

            if (etlFiles.Count() == 0)
            {
                Console.WriteLine("No ETL files were found! Unable to process E3 Energy data.");
                return;
            }

            foreach (var etl in etlFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(etl);

                xPerf.DumpEtlEventsToFile(etl, Path.ChangeExtension(fileName, ".csv"));

                energyProcessor.ProcessEnergyData(Path.ChangeExtension(fileName, ".csv"));
            }

            energyProcessor.SaveCompononentEnergyToFile(Path.ChangeExtension("componentEnergy_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"), ".csv"));
            energyProcessor.SaveProcessEnergyToFile(Path.ChangeExtension("processEnergy_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"), ".csv"));

            Console.WriteLine("[{0}] - Completed processing of energy data. -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
            s_possibleScenarios.Add(scenario.Name, scenario);
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

            Console.WriteLine("Usage: TestingPower.exe -browser|-b [chrome|edge|firefox|opera|operabeta] -scenario|-s all|<scenario1> <scenario2> [-loops <loopcount>] [-iterations|-i <iterationcount>] [-tracecontrolled|-tc <etlpath>] [-warmup|-w] [-profile|-p <chrome profile path>]");
            for (int argNum = 0; argNum < args.Length; argNum++)
            {
                var arg = args[argNum].ToLowerInvariant();
                switch (arg)
                {
                    case "-browser":
                    case "-b":
                        argNum++;

                        if (args[argNum].ToLowerInvariant() == "all")
                        {
                            foreach (string browser in s_SupportedBrowsers)
                            {
                                s_browsers.Add(browser);
                            }

                            break;
                        }

                        while (argNum < args.Length)
                        {
                            string browser = args[argNum].ToLowerInvariant();
                            if (!s_SupportedBrowsers.Contains(browser))
                            {
                                throw new Exception($"Unsupported browser '{browser}'");
                            }

                            s_browsers.Add(browser);
                            int nextArgNum = argNum + 1;
                            if (nextArgNum < args.Length && args[argNum + 1].StartsWith("-"))
                            {
                                break;
                            }

                            argNum++;
                        }

                        break;
                    case "-scenario":
                    case "-s":
                        argNum++;

                        if (args[argNum] == "all")
                        {
                            // Specify the "official" runs, including order
                            s_scenarios.Add(s_possibleScenarios["youtube"]);
                            s_scenarios.Add(s_possibleScenarios["cnn"]);
                            s_scenarios.Add(s_possibleScenarios["techRadar"]);
                            s_scenarios.Add(s_possibleScenarios["amazon"]);
                            s_scenarios.Add(s_possibleScenarios["facebook"]);
                            s_scenarios.Add(s_possibleScenarios["google"]);
                            s_scenarios.Add(s_possibleScenarios["gmail"]);
                            s_scenarios.Add(s_possibleScenarios["wikipedia"]);
                            s_scenarioName = "all";
                            break;
                        }

                        while (argNum < args.Length)
                        {
                            var scenario = args[argNum];
                            if (!s_possibleScenarios.ContainsKey(scenario))
                            {
                                throw new Exception($"Unexpected scenario '{scenario}'");
                            }

                            s_scenarios.Add(s_possibleScenarios[scenario]);

                            if (string.IsNullOrEmpty(s_scenarioName))
                            {
                                s_scenarioName = scenario;
                            }
                            else
                            {
                                s_scenarioName = s_scenarioName + "_" + scenario;
                            }

                            int nextArgNum = argNum + 1;
                            if (nextArgNum < args.Length && args[argNum + 1].StartsWith("-"))
                            {
                                break;
                            }

                            argNum++;
                        }

                        break;
                    case "-loops":
                        argNum++;
                        s_loops = int.Parse(args[argNum]);
                        break;
                    case "-tracecontrolled":
                    case "-tc":
                        s_useTraceController = true;
                        argNum++;
                        s_etlPath = args[argNum];
                        break;
                    case "-warmup":
                    case "-w":
                        s_doWarmup = true;
                        break;
                    case "-iterations":
                    case "-i":
                        argNum++;
                        s_iterations = int.Parse(args[argNum]);
                        break;
                    case "-profile":
                    case "-p":
                        argNum++;
                        s_profilePath = args[argNum];
                        if(!Directory.Exists(s_profilePath))
                        {
                            throw new DirectoryNotFoundException("The profile path does not exist!");
                        }
                        break;
                    default:
                        throw new Exception($"Unexpected argument encountered '{args[argNum]}'");
                }
            }
        }

        private static void CloseTabs(RemoteWebDriver driver)
        {
            // Simply go through and close every tab one by one.
            //foreach (var window in s_driver.WindowHandles)
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
        public static void scrollPage(RemoteWebDriver driver, int timesToScroll)
        {
            // Webdriver examples had scrolling by executing Javascript. That approach seemed troublesome because the
            // browser is scrolling in a way very different from how it would with a real user, so we don't do it.
            // Page down seemed to be the best compromise in terms of it behaving like a real user scrolling, and it
            // working reliably across browsers.
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
        /// <param name="browser">
        /// The browser that the new tab is being created in.
        /// </param>
        private static void CreateNewTab(RemoteWebDriver driver, string browser)
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
                driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);
            }

            // Give the browser more than enough time to open the tab and get to it so the next commands from the
            // scenario don't get lost
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Instantiates a RemoteWebDriver instance based on the browser passed to this method. Opens the browser and maximizes its window.
        /// </summary>
        /// <param name="browser">The browser to get instantiate the Web Driver for.</param>
        /// <returns>The RemoteWebDriver of the browser passed in to the method.</returns>
        private static RemoteWebDriver CreateDriverAndMaximize(string browser)
        {
            // Create a webdriver for the respective browser, depending on what we're testing.
            RemoteWebDriver driver = null;
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

                    if (!string.IsNullOrEmpty(s_profilePath))
                    {
                        option.AddArgument("--user-data-dir=" + s_profilePath);
                    }

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

        /// <summary>
        /// Here we do the work to finish the test, like quitting the driver.
        /// </summary>
        private static void Teardown(RemoteWebDriver driver)
        {
            // TODO: Capture power usage / results automatically
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
        }

        private static bool IsAlertPresent(RemoteWebDriver driver)
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
