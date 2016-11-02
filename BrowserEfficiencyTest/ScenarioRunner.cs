//--------------------------------------------------------------
//
// Browser Efficiency Test
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Executes automated scenarios on selected web browsers using WebDriver
    /// </summary>
    internal class ScenarioRunner
    {
        private bool _doWarmup;
        private int _iterations;
        private int _maxAttempts;
        private string _browserProfilePath;
        private bool _usingTraceController;
        private string _etlPath;

        private List<Scenario> _scenarios = new List<Scenario>();
        private List<string> _browsers = new List<string>();
        private CredentialManager _logins;

        private string _scenarioName;
        private int _e3RefreshDelaySeconds;

        // _measureSets format: Dictionary< "measure set name", Tuple < "WPR profile name", "tracing mode" >>
        private Dictionary<string, Tuple<string, string>> _measureSets;

        // For results from timing metrics that won't be in traces:
        private List<List<string>> _htmlTimerResults;

        /// <summary>
        /// Instantiates a ScenarioRunner with the passed in arguments
        /// </summary>
        /// <param name="args"></param>
        public ScenarioRunner(Arguments args)
        {
            _e3RefreshDelaySeconds = 12;

            _doWarmup = args.DoWarmup;
            _iterations = args.Iterations;
            _browserProfilePath = args.BrowserProfilePath;
            _usingTraceController = args.UsingTraceController;
            _etlPath = args.EtlPath;
            _maxAttempts = args.MaxAttempts;

            _scenarios = args.Scenarios.ToList();
            _browsers = args.Browsers.ToList();

            _scenarioName = args.ScenarioName;
            _measureSets = GetMeasureSetInfo(args.SelectedMeasureSets.ToList());
            _logins = new CredentialManager();

            _htmlTimerResults = new List<List<string>>();
        }

        // Creates a data structure of measure sets name, wprp file and tracing mode and creates an empty one
        // if there are no measure sets selected (user isn't doing any tracing). This helps to simplify the 
        // logic needed to allow both the ability to cycle through measure sets if the user selected any as
        // well as not use any measure sets if none were selected. The alternative would be to use the full
        // measureset objects, which would require either creating a dummy/empty measure set objects or adding
        // multiple checks throughout the main pass loop checking to see if measure sets were enabled or not.
        private Dictionary<string, Tuple<string, string>> GetMeasureSetInfo(List<MeasureSet> measureSets)
        {
            // _measureSets format: Dictionary< "measure set name", Tuple < "WPR profile name", "tracing mode" >>
            Dictionary<string, Tuple<string, string>> measureSetInfo = new Dictionary<string, Tuple<string, string>>();

            if (measureSets == null || measureSets.Count == 0)
            {
                // No measure sets selected so create a single empty value to use as a dummy measure set.
                measureSetInfo.Add("None", new Tuple<string, string>("", ""));
            }
            else
            {
                // Create a data structure containing the name, WPR profile name and tracing mode of all selected measure sets.
                var msInfo = from m in measureSets
                             select new KeyValuePair<string, Tuple<string, string>>(m.Name, new Tuple<string, string>(m.WprProfile, m.TracingMode.ToString()));

                // Format msInfo to a Dictionary of <string, Tuple<string, string>>
                measureSetInfo = msInfo.ToDictionary(k => k.Key, v => v.Value);
            }

            return measureSetInfo;
        }

        /// <summary>
        /// Runs the test passes specified by the arguments passed in when the ScenarioRunner object was instantiated.
        /// </summary>
        public void Run()
        {
            if (_doWarmup)
            {
                RunWarmupPass();
            }

            RunMainLoop();
        }

        private void RunWarmupPass()
        {
            // A warmup pass is one run thru the selected scenarios and browsers.
            // It allows the browsers to cache some content which helps reduce variability from run to run.
            Console.WriteLine("[{0}] - Starting warmup pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            foreach (string browser in _browsers)
            {
                using (var driver = RemoteWebDriverExtension.CreateDriverAndMaximize(browser))
                {
                    foreach (var scenario in _scenarios)
                    {
                        HtmlTimer timer = new HtmlTimer(driver, browser, scenario.Name, 0);

                        Console.WriteLine("[{0}] - Warmup - Browser: {1}  Scenario: {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), browser, scenario.Name);
                        
                        scenario.Run(driver, browser, _logins, timer);

                        Thread.Sleep(1 * 1000);
                    }
                    driver.Quit();
                }
            }
            Console.WriteLine("[{0}] - Completed warmup pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// The main loop of the class. This method will run through the specified number of iterations on all the
        /// specified browsers across all the specified scenarios.
        /// </summary>
        private void RunMainLoop()
        {
            if (_usingTraceController)
            {
                Console.WriteLine("[{0}] - Pausing before first tracing session to reduce interference.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                // that the browser energy data reported by E3 going forward is from this test run and not from warmup or before running the test pass.
                Thread.Sleep(_e3RefreshDelaySeconds * 1000);
            }

            using (var elevatorClient = ElevatorClient.Create(_usingTraceController))
            {
                elevatorClient.ConnectAsync().Wait();
                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_PASS} {_etlPath}").Wait();

                Console.WriteLine("[{0}] - Starting Test Pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // Core Execution Loop
                // TODO: Consider breaking up this large loop into smaller methods to ease readability.
                for (int iteration = 0; iteration < _iterations; iteration++)
                {
                    foreach (var currentMeasureSet in _measureSets)
                    {
                        // Randomize the order the browsers each iteration to reduce systematic bias in the test
                        Random rand = new Random();
                        _browsers = _browsers.OrderBy(a => rand.Next()).ToList<String>();

                        foreach (string browser in _browsers)
                        {
                            bool passSucceeded = false;
                            for (int attemptNumber = 0; attemptNumber < _maxAttempts && !passSucceeded; attemptNumber++)
                            {
                                if (attemptNumber > 0)
                                {
                                    Console.WriteLine("[{0}] - Attempting again...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                }

                                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_BROWSER} {browser} ITERATION {iteration} SCENARIO_NAME {_scenarioName} WPRPROFILE {currentMeasureSet.Value.Item1} MODE {currentMeasureSet.Value.Item2}").Wait();

                                Console.WriteLine("[{0}] - Launching Browser Driver {1} -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), browser);
                                using (var driver = RemoteWebDriverExtension.CreateDriverAndMaximize(browser))
                                {
                                    try
                                    {
                                        Stopwatch watch = Stopwatch.StartNew();
                                        bool isFirstScenario = true;

                                        foreach (var scenario in _scenarios)
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
                                                driver.CreateNewTab(browser);
                                            }
                                            else
                                            {
                                                isFirstScenario = false;
                                            }

                                            // Create a new timer for measuring durations if the scenario desires
                                            HtmlTimer timer = new HtmlTimer(driver, browser, scenario.Name, iteration);

                                            Console.WriteLine("[{0}] - Executing - Iteration: {1}  Browser: {2}  Scenario: {3}  MeasureSet: {4}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), iteration, browser, scenario.Name, currentMeasureSet.Key);

                                            // Here, control is handed to the scenario to navigate, and do whatever it wants
                                            scenario.Run(driver, browser, _logins, timer);

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

                                            Console.WriteLine("[{0}] - Completed - Iteration: {1}  Browser: {2}  Scenario: {3}  MeasureSet: {4}. Scenario ran for {5} seconds.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), iteration, browser, scenario.Name, currentMeasureSet.Key, runTime.TotalSeconds);

                                            Thread.Sleep(timeLeft);

                                            // Add the responsiveness results from this scenario to all our responsiveness results
                                            _htmlTimerResults.AddRange(timer.GetAllResults());
                                        }

                                        Console.WriteLine("[{0}] - Completed Browser: {1}  Iteration: {2}  MeasureSet: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), browser, iteration, currentMeasureSet.Key);

                                        driver.CloseAllTabs(browser);
                                        passSucceeded = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        // If something goes wrong and we get an exception halfway through the scenario, we clean up
                                        // and put everything back into a state where we can start the next iteration.
                                        elevatorClient.SendControllerMessageAsync(Elevator.Commands.CANCEL_PASS);
                                        driver.CloseAllTabs(browser);
                                        Console.WriteLine("/-EXCEPTION---------------------------------------------\\");
                                        Console.WriteLine("[{0}] - Caught exception while trying to run scenario. Exception:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                        Console.WriteLine(ex);
                                        if (_usingTraceController)
                                        {
                                            Console.WriteLine("Trace has been discarded");
                                        }
                                        Console.WriteLine("\\-------------------------------------------------------/");
                                    }
                                    finally
                                    {
                                        if (_usingTraceController)
                                        {
                                            Console.WriteLine("[{0}] - Pausing between tracing sessions to reduce interference.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                            // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                                            // that the browser energy data reported by E3 for this run is only for this run and does not bleed into any other runs.
                                            Thread.Sleep(_e3RefreshDelaySeconds * 1000);
                                        }
                                    }
                                }
                            }

                            if (passSucceeded)
                            {
                                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.END_BROWSER} {browser}").Wait();
                            }
                        }
                    }
                }
                Console.WriteLine("[{0}] - Ending Test Pass -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                elevatorClient.SendControllerMessageAsync(Elevator.Commands.END_PASS).Wait();
            }
        }

        public List<List<string>> GetResponsivenessResults()
        {
            return _htmlTimerResults;
        }
    }
}