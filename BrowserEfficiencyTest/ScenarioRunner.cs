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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.Win32;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Executes automated scenarios on selected web browsers using WebDriver
    /// </summary>
    internal class ScenarioRunner
    {
        // Calculated path for staging extension files under Edge's local appdata folder.
        public static readonly string ExtensionsStagingRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages\\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\\LocalState\\");
        public static Dictionary<string, string> _extensionsNameAndVersion = new Dictionary<string, string>();

        private ResponsivenessTimer _timer;
        private bool _useTimer;
        private int _iterations;
        private int _maxAttempts;
        private string _browserProfilePath;
        private bool _usingTraceController;
        private string _etlPath;
        private List<string> _extensionsPaths;
        private string _extensionsStagingPath;
        private bool _overrideTimeout;
        private List<WorkloadScenario> _scenarios = new List<WorkloadScenario>();
        private List<string> _browsers = new List<string>();
        private CredentialManager _logins;
        private string _workloadName;
        private int _e3RefreshDelaySeconds;
        private bool _captureBaseline;
        private int _baselineCaptureSeconds;
        private bool _clearBrowserCache;
        private bool _executeWarmupRun;
        private string _hostName;
        private int _port;
        private bool _enableVerboseLogging;
        private bool _enableScenarioTracing;
        private string _executeScriptFileName;

        // _measureSets format: Dictionary< "measure set name", Tuple < "WPR profile name", "tracing mode" >>
        private Dictionary<string, Tuple<string, string>> _measureSets;

        /// <summary>
        /// Instantiates a ScenarioRunner with the passed in arguments
        /// </summary>
        /// <param name="args"></param>
        public ScenarioRunner(Arguments args)
        {
            _e3RefreshDelaySeconds = 12;
            _iterations = args.Iterations;
            _browserProfilePath = args.BrowserProfilePath;
            _usingTraceController = args.UsingTraceController;
            _etlPath = args.EtlPath;
            _extensionsStagingPath = "";
            _maxAttempts = args.MaxAttempts;
            _overrideTimeout = args.OverrideTimeout;
            _scenarios = args.Scenarios.ToList();
            _browsers = args.Browsers.ToList();
            _workloadName = args.ScenarioName;
            _measureSets = GetMeasureSetInfo(args.SelectedMeasureSets.ToList());
            _logins = new CredentialManager(args.CredentialPath);
            _timer = new ResponsivenessTimer();
            _captureBaseline = args.CaptureBaseline;
            _baselineCaptureSeconds = args.BaselineCaptureSeconds;
            _clearBrowserCache = args.ClearBrowserCache;
            _executeWarmupRun = args.DoWarmupRun;
            _hostName = args.Host;
            _port = args.Port;
            _enableVerboseLogging = args.EnableVerboseWebDriverLogging;
            _enableScenarioTracing = args.EnableScenarioTracing;
            _executeScriptFileName = args.ExecuteScriptFileName;

            if (!string.IsNullOrEmpty(args.ExtensionsPath))
            {
                _extensionsPaths = GetExtensionPaths(args.ExtensionsPath);
            }
            else
            {
                _extensionsPaths = null;
            }

            if (args.MeasureResponsiveness)
            {
                _useTimer = true;
            }
            else
            {
                _useTimer = false;
            }
        }

        // Copies valid extensions in the given path to the appdata folder and returns the appdata paths. Reads the name and version
        // of each extension which can be used if we capture traces.
        private List<string> GetExtensionPaths(string path)
        {
            var sideloadExtensionPaths = new List<string>();
            var directoryName = path.Split('\\').Last();
            var extensionPaths = Directory.GetDirectories(path);
            var validExtensionPaths = GetValidExtensions(extensionPaths);

            // Check if the extensions path provided contains at least one extension
            if (validExtensionPaths.Count > 0)
            {
                var stagingPath = Path.Combine(ExtensionsStagingRootPath, directoryName);
                _extensionsStagingPath = stagingPath;

                if (Directory.Exists(stagingPath))
                {
                    CleanupExtensions();
                }

                Directory.CreateDirectory(stagingPath);

                foreach (var extensionPath in validExtensionPaths)
                {
                    var extensionFolderName = extensionPath.Split('\\').Last();
                    var extensionStagingPath = Path.Combine(ExtensionsStagingRootPath, directoryName, extensionFolderName);
                    Directory.CreateDirectory(extensionStagingPath);

                    Logger.LogWriteLine("Staging extensions files: Source='" + extensionPath + "' Destination='" + extensionStagingPath + "'", false);

                    try
                    {
                        DirectoryCopy(extensionPath, extensionStagingPath);
                        sideloadExtensionPaths.Add(Path.Combine(extensionStagingPath, "Extension"));

                        // Read the name and version of extension.
                        var manifestPath = Path.Combine(extensionPath, "Extension", "manifest.json");
                        string jsonText = File.ReadAllText(Path.GetFullPath(manifestPath));
                        var data = (JObject)JsonConvert.DeserializeObject(jsonText);
                        string name = data["name"].Value<string>();
                        string version = data["version"].Value<string>();
                        _extensionsNameAndVersion.Add(name, version);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWriteLine("Copying of extension(s) failed with: \n" + ex.Message);
                        throw new Exception("Copying of extension(s) failed with: \n" + ex.Message);
                    }
                }
            }
            else
            {
                Logger.LogWriteLine("No valid extensions found in given path " + path +
                    ". The folder structure should be as follows: \n" +
                    "unpackedExtensions \n| ----extension1\n| ----| ----Assets\n| ----| ----AppXManifest.xml\n| ----| ----Extension\n| ----| ----| ----manifest.json\n| ----| ----| ---- < otherExtFiles >\n| ----extension2\n| ----| ----Assets\n| ----| ----AppXManifest.xml\n| ----| ----Extension\n| ----| ----| ----manifest.json\n| ----| ----| ---- < otherExtFiles >");
                throw new Exception("No valid extensions found in given path " + path +
                    ". The folder structure should be as follows: \n" +
                    "unpackedExtensions \n| ----extension1\n| ----| ----Assets\n| ----| ----AppXManifest.xml\n| ----| ----Extension\n| ----| ----| ----manifest.json\n| ----| ----| ---- < otherExtFiles >\n| ----extension2\n| ----| ----Assets\n| ----| ----AppXManifest.xml\n| ----| ----Extension\n| ----| ----| ----manifest.json\n| ----| ----| ---- < otherExtFiles >");
            }

            return sideloadExtensionPaths;
        }

        private List<string> GetValidExtensions(string[] paths)
        {
            var validExtensions = new List<string>();

            // Check for folders with manifest.json
            foreach (var path in paths)
            {
                var extensionFilesPath = Path.Combine(path, "Extension");
                if ((Directory.Exists(extensionFilesPath)) && (File.Exists(Path.Combine(extensionFilesPath, "manifest.json"))))
                {
                    validExtensions.Add(path);
                }
            }

            return validExtensions;
        }

        private void CleanupExtensions()
        {
            if (!string.IsNullOrEmpty(_extensionsStagingPath))
            {
                if (Directory.Exists(_extensionsStagingPath))
                {
                    // Clean up existing directories
                    Directory.Delete(_extensionsStagingPath, /*recursive*/ true);
                }
            }
        }

        // Recursively copies all the files and subfolders of the source directory into the destination directory
        // Copied from https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void DirectoryCopy(string sourceDirectoryName, string destinationDirectoryName)
        {
            // Get the subdirectories for the specified source directory
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceDirectoryName);

            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirectoryName);
            }

            DirectoryInfo[] sourceSubDirectories = sourceDirectory.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destinationDirectoryName))
            {
                Directory.CreateDirectory(destinationDirectoryName);
            }

            // Get the files in the source directory and copy them to the new location
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destinationDirectoryName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // Copy the each of the subdirectories
            foreach (DirectoryInfo subDirectory in sourceSubDirectories)
            {
                string tempPath = Path.Combine(destinationDirectoryName, subDirectory.Name);
                DirectoryCopy(subDirectory.FullName, tempPath);
            }
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

        public List<string> GetResponsivenessResults()
        {
            return _timer.GetResults();
        }

        /// <summary>
        /// The main loop of the class. This method will run through the specified number of iterations on all the
        /// specified browsers across all the specified scenarios.
        /// </summary>
        public void Run()
        {
            bool passSucceeded = true;
            LogOsVersion();

            if (_executeWarmupRun)
            {
                Logger.LogWriteLine(" Starting warmup run");
                ScenarioEventSourceProvider.EventLog.WarmupExecutionStart();
                foreach (string browser in _browsers)
                {
                    // Even though we are not collecting any tracing during warmup runs, we need to pass a dummy ElevatorClient to the ExecuteWorkload function.
                    using (var elevatorClient = ElevatorClient.Create(false))
                    {
                        Logger.LogWriteLine(string.Format(" Executing warmup of {0} browser", browser));
                        // use -1 as the iteration value to denote warmup run
                        ExecuteWorkload(-1, browser, "None", "", "", true, false, elevatorClient);
                        Logger.LogWriteLine(string.Format(" Completed warmup of {0} browser", browser));
                    }
                }
                ScenarioEventSourceProvider.EventLog.WarmupExecutionStop();
                Logger.LogWriteLine(" Completed warmup run");
            }

            if (_useTimer)
            {
                _timer.Enable();
            }

            if (_usingTraceController)
            {
                Logger.LogWriteLine("Pausing before starting first tracing session to reduce interference.");

                // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                // that the browser energy data reported by E3 going forward is from this test run and not from before running the test pass.
                Thread.Sleep(_e3RefreshDelaySeconds * 1000);
            }

            using (var elevatorClient = ElevatorClient.Create(_usingTraceController))
            {
                elevatorClient.ConnectAsync().Wait();
                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_PASS} {_etlPath}").Wait();
                Logger.LogWriteLine("Starting Test Pass");

                // Core Execution Loop
                // TODO: Consider breaking up this large loop into smaller methods to ease readability.
                for (int iteration = 0; iteration < _iterations; iteration++)
                {
                    Logger.LogWriteLine(string.Format("Iteration: {0} ------------------", iteration));
                    _timer.SetIteration(iteration);
                    foreach (var currentMeasureSet in _measureSets)
                    {
                        if (_captureBaseline && _usingTraceController)
                        {
                            // capture a baseline of the system for this measureset
                            // A baseline is where we capture measureset data of the system but without running the browser and test pass.
                            // The idea is to get a baseline performance capture of the system without the browser and test pass so it
                            // can be used as a comparison.

                            Logger.LogWriteLine(string.Format(" Starting capture of system baseline for {0} seconds - measureset {1}  iteration {2}", _baselineCaptureSeconds, currentMeasureSet.Value.Item1, iteration));

                            // Start the trace capture for baseline scenario
                            elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_BROWSER} BASE ITERATION {iteration} SCENARIO_NAME BaseLineCapture WPRPROFILE {currentMeasureSet.Value.Item1} MODE {currentMeasureSet.Value.Item2}").Wait();

                            ScenarioEventSourceProvider.EventLog.MeasurementRegionStart("BaselineCapture");

                            Thread.Sleep(_baselineCaptureSeconds * 1000);

                            ScenarioEventSourceProvider.EventLog.MeasurementRegionStop("BaselineCapture");

                            Logger.LogWriteLine(string.Format(" Finished capture of system baseline of measureset {0}  iteration {1}", currentMeasureSet.Value.Item1, iteration));

                            // End the trace capture for baseline scenario
                            elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.END_BROWSER} BASE").Wait();

                            // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                            // that the browser energy data reported by E3 for this run is only for this run and does not bleed into any other runs.
                            Logger.LogWriteLine("  Pausing between tracing sessions to reduce interference.");
                            Thread.Sleep(_e3RefreshDelaySeconds * 1000);
                        }

                        _timer.SetMeasureSet(currentMeasureSet.Key);

                        // Randomize the order the browsers each iteration to reduce systematic bias in the test
                        Random rand = new Random();
                        _browsers = _browsers.OrderBy(a => rand.Next()).ToList<String>();

                        foreach (string browser in _browsers)
                        {
                            _timer.SetBrowser(browser, _extensionsNameAndVersion);

                            passSucceeded = ExecuteWorkload(iteration, browser, currentMeasureSet.Key, currentMeasureSet.Value.Item1, currentMeasureSet.Value.Item2, _overrideTimeout, _usingTraceController, elevatorClient);
                        }
                    }
                }

                CleanupExtensions();
                Logger.LogWriteLine("Completed Test Pass");
                elevatorClient.SendControllerMessageAsync(Elevator.Commands.END_PASS).Wait();
            }
        }

        // This method is the one actually responsible for executing each scenario in the workload
        // It includes error checking and retry attempts as well as controls for starting and stopping ETL tracing via ElevatorServer.exe
        private bool ExecuteWorkload(int iteration, string browser, string measureSetName, string wprProfileName, string tracingMode, bool overrideTimeout, bool usingTraceController, IElevatorClient elevatorClient)
        {
            bool passSucceeded = false;

            for (int attemptNumber = 0; attemptNumber < _maxAttempts && !passSucceeded; attemptNumber++)
            {
                int scenarioIndex = 0;

                if (attemptNumber > 0)
                {
                    Logger.LogWriteLine("  Attempting again...");
                }

                string workloadName = _workloadName;

                if (_enableScenarioTracing)
                {
                    // Capturing a trace per each scenario of the workload. Append the scenario name and index to the workloadname for documentation purposes.
                    workloadName = _workloadName + "-0-" + _scenarios[0].ScenarioName;
                }

                // Start tracing
                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_BROWSER} {browser} ITERATION {iteration} SCENARIO_NAME {workloadName} WPRPROFILE {wprProfileName} MODE {tracingMode}").Wait();
                if(!string.IsNullOrEmpty(_executeScriptFileName))
                {
                    // execute the selected script and pass "STARTSCENARIO"+<scenario>+<iteration>
                    ExecuteScript(_executeScriptFileName, "STARTSCENARIO " + _scenarios[0].ScenarioName + " " + iteration.ToString());
                }

                if (usingTraceController)
                {
                    Logger.LogWriteLine(string.Format("  Pausing {0} seconds after starting the trace session to reduce interference.", _e3RefreshDelaySeconds));

                    // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                    // that the browser energy data reported by E3 for this run is only for this run and does not bleed into any other runs.
                    Thread.Sleep(_e3RefreshDelaySeconds * 1000);
                }

                Logger.LogWriteLine(string.Format(" Launching Browser Driver: '{0}'", browser));
                ScenarioEventSourceProvider.EventLog.WorkloadStart(_workloadName, browser, wprProfileName, iteration, attemptNumber);

                using (var driver = RemoteWebDriverExtension.CreateDriverAndMaximize(browser, _clearBrowserCache, _enableVerboseLogging, _browserProfilePath, _extensionsPaths, _port, _hostName))
                {
                    string currentScenarioName = "";
                    try
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        bool isFirstScenario = true;

                        _timer.SetDriver(driver);

                        foreach (var currentScenario in _scenarios)                        
                        {
                            if (_enableScenarioTracing && scenarioIndex > 0)
                            {
                                // Capturing a trace per each scenario of the workload. 
                                // Stop and save the current trace session which is for the last scenario.
                                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.END_BROWSER} {browser}").Wait();
                                
                                // let's just wait a few seconds before starting the next trace
                                Thread.Sleep(3000); 

                                // Append the scenario name and index to the workloadname for documentation purposes.
                                workloadName = _workloadName + "-" + scenarioIndex + "-" + currentScenario.ScenarioName;

                                // Start tracing for the current scenario
                                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.START_BROWSER} {browser} ITERATION {iteration} SCENARIO_NAME {workloadName} WPRPROFILE {wprProfileName} MODE {tracingMode}").Wait();
                            }

                            if (!string.IsNullOrEmpty(_executeScriptFileName) && scenarioIndex > 0)
                            {
                                // execute the selected script and pass "ENDSCENARIO"
                                ExecuteScript(_executeScriptFileName, "ENDSCENARIO");

                                // execute the selected script and pass "STARTSCENARIO"+<scenario>+<iteration>
                                ExecuteScript(_executeScriptFileName, "STARTSCENARIO " + currentScenario.ScenarioName + " " + iteration.ToString());
                            }

                            // Save the name of the current scenarion in case an exception is thrown in which case the local variable 'currentScenario' will be lost
                            currentScenarioName = currentScenario.ScenarioName;

                            _timer.SetScenario(currentScenario.ScenarioName);

                            // We want every scenario to take the same amount of time total, even if there are changes in
                            // how long pages take to load. The biggest reason for this is so that you can measure energy
                            // or power and their ratios will be the same either way.
                            // So start by getting the current time.
                            var startTime = watch.Elapsed;

                            // The first scenario naviagates in the browser's new tab / welcome page.
                            // After that, scenarios open in their own tabs
                            if (!isFirstScenario && currentScenario.Tab == "new")
                            {
                                driver.CreateNewTab();
                            }

                            isFirstScenario = false;

                            Logger.LogWriteLine(string.Format("  Executing - Scenario: {0}  Iteration: {1}  Attempt: {2}  Browser: {3}  MeasureSet: {4}", currentScenario.ScenarioName, iteration, attemptNumber, browser, measureSetName));
                            ScenarioEventSourceProvider.EventLog.ScenarioExecutionStart(browser, currentScenario.ScenarioName);

                            // Here, control is handed to the scenario to navigate, and do whatever it wants
                            currentScenario.Scenario.Run(driver, browser, _logins, _timer);

                            // When we get control back, we sleep for the remaining time for the scenario. This ensures
                            // the total time for a scenario is always the same
                            var runTime = watch.Elapsed.Subtract(startTime);
                            var timeLeft = TimeSpan.FromSeconds(currentScenario.Duration).Subtract(runTime);

                            if (timeLeft < TimeSpan.FromSeconds(0) && !overrideTimeout)
                            {
                                // Of course it's possible we don't get control back until after we were supposed to
                                // continue to the next scenario. In that case, invalidate the run by throwing.
                                Logger.LogWriteLine(string.Format("   !!! Scenario {0} ran longer than expected! The browser ran for {1}s. The timeout for this scenario is {2}s.", currentScenario.ScenarioName, runTime.TotalSeconds, currentScenario.Duration));
                                throw new Exception(string.Format("Scenario {0} ran longer than expected! The browser ran for {1}s. The timeout for this scenario is {2}s.", currentScenario.ScenarioName, runTime.TotalSeconds, currentScenario.Duration));
                            }
                            else if (!overrideTimeout)
                            {
                                ScenarioEventSourceProvider.EventLog.ScenarioIdleStart(currentScenario.ScenarioName, timeLeft.TotalSeconds);
                                Logger.LogWriteLine(string.Format("    Scenario {0} returned in {1} seconds. Sleep for remaining {2} seconds.", currentScenario.ScenarioName, runTime.TotalSeconds, timeLeft.TotalSeconds));
                                Thread.Sleep((int)timeLeft.TotalMilliseconds);
                                ScenarioEventSourceProvider.EventLog.ScenarioIdleStop(currentScenario.ScenarioName, timeLeft.TotalSeconds);
                            }

                            ScenarioEventSourceProvider.EventLog.ScenarioExecutionStop(browser, currentScenario.ScenarioName);
                            Logger.LogWriteLine(string.Format("  Completed - Scenario: {0}  Iteration: {1}  Attempt: {2}  Browser: {3}  MeasureSet: {4}", currentScenario.ScenarioName, iteration, attemptNumber, browser, measureSetName, runTime.TotalSeconds));
                            scenarioIndex++;
                        }

                        driver.CloseBrowser(browser);
                        passSucceeded = true;

                        Logger.LogWriteLine(string.Format(" SUCCESS!  Completed Browser: {0}  Iteration: {1}  Attempt: {2}  MeasureSet: {3}", browser, iteration, attemptNumber, measureSetName));
                        ScenarioEventSourceProvider.EventLog.WorkloadStop(_workloadName, browser, wprProfileName, iteration, attemptNumber);
                    }
                    catch (Exception ex)
                    {
                        // If something goes wrong and we get an exception halfway through the scenario, we clean up
                        // and put everything back into a state where we can start the next iteration.
                        elevatorClient.SendControllerMessageAsync(Elevator.Commands.CANCEL_PASS);

                        if (!string.IsNullOrEmpty(_executeScriptFileName))
                        {
                            // execute the selected script and pass "FAIL"
                            ExecuteScript(_executeScriptFileName, "FAIL");
                        }

                        try
                        {
                            // Attempt to save the page source
                            string pageSourceFileName = string.Format("pageSource_{0}_{1}_{2}_{3}_{4}.html", browser, currentScenarioName, iteration, measureSetName, attemptNumber);
                            pageSourceFileName = Path.Combine(_etlPath, pageSourceFileName);
                            using (StreamWriter sw = new StreamWriter(pageSourceFileName, false))
                            {
                                sw.WriteLine(driver.PageSource);
                            }

                            // Attempt to save a screenshot
                            OpenQA.Selenium.Screenshot screenshot = driver.GetScreenshot();
                            string imageFileName = string.Format("screenshot_{0}_{1}_{2}_{3}_{4}.png", browser, currentScenarioName, iteration, measureSetName, attemptNumber);
                            imageFileName = Path.Combine(_etlPath, imageFileName);
                            screenshot.SaveAsFile(imageFileName, OpenQA.Selenium.ScreenshotImageFormat.Png);
                        }
                        catch (Exception)
                        {
                            // ignore this exception as we were just trying to see if we could get a screenshot and pagesource for the original exception.
                        }

                        driver.CloseBrowser(browser);
                        Logger.LogWriteLine("------ EXCEPTION caught while trying to run scenario! ------------------------------------");
                        Logger.LogWriteLine(string.Format("    Iteration:   {0}", iteration));
                        Logger.LogWriteLine(string.Format("    Measure Set: {0}", measureSetName));
                        Logger.LogWriteLine(string.Format("    Browser:     {0}", browser));
                        Logger.LogWriteLine(string.Format("    Attempt:     {0}", attemptNumber));
                        Logger.LogWriteLine(string.Format("    Scenario:    {0}", currentScenarioName));
                        Logger.LogWriteLine("    Exception:   " + ex.ToString());

                        if (usingTraceController)
                        {
                            Logger.LogWriteLine("   Trace has been discarded");
                        }

                        Logger.LogWriteLine("-------------------------------------------------------");
                    }
                    finally
                    {
                        if (usingTraceController)
                        {
                            Logger.LogWriteLine(string.Format("  Pausing {0} seconds before stopping the trace session to reduce interference.", _e3RefreshDelaySeconds));

                            // E3 system aggregates energy data at regular intervals. For our test passes we use 10 second intervals. Waiting here for 12 seconds before continuing ensures
                            // that the browser energy data reported by E3 for this run is only for this run and does not bleed into any other runs.
                            Thread.Sleep(_e3RefreshDelaySeconds * 1000);
                        }
                    }
                }
            }

            if (passSucceeded)
            {
                // Stop tracing
                elevatorClient.SendControllerMessageAsync($"{Elevator.Commands.END_BROWSER} {browser}").Wait();
                if (!string.IsNullOrEmpty(_executeScriptFileName))
                {
                    // execute the selected script and pass "ENDSCENARIO"
                    ExecuteScript(_executeScriptFileName, "ENDSCENARIO");
                }
            }
            else
            {
                CleanupExtensions();
                Logger.LogWriteLine(string.Format("!!! Failed to successfully complete iteration {0} with browser '{1}' after {2} attempts!", iteration, browser, _maxAttempts));
                throw new Exception(string.Format("!!! Failed to successfully complete iteration {0} with browser '{1}' after {2} attempts!", iteration, browser, _maxAttempts));
            }

            return passSucceeded;
        }

        // Log the OS version from the registry
        private void LogOsVersion()
        {
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"software\microsoft\windows NT\currentVersion"))
            {
                Logger.LogWriteLine("--- OS Version ------");
                Logger.LogWriteLine(string.Format("  BuildLabEx: {0}", (string)regKey.GetValue("BuildLabEx")));
                Logger.LogWriteLine(string.Format("  EditionID: {0}", (string)regKey.GetValue("EditionID")));
                Logger.LogWriteLine(string.Format("  ProductName: {0}", (string)regKey.GetValue("ProductName")));
                Logger.LogWriteLine("---------------------");
            }
        }

        // Open another process and run the desired script with the desired parameters
        private bool ExecuteScript(string scriptFileName, string parameterString)
        {
            bool isSuccess = false;

            Logger.LogWriteLine(string.Format("   Executing script: {0} {1}", scriptFileName, parameterString));

            if (string.IsNullOrEmpty(scriptFileName))
            {
                return false;
            }

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(scriptFileName);
                processInfo.Arguments = parameterString;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;

                Process commandProcess = new Process();
                commandProcess.StartInfo = processInfo;
                commandProcess.Start();
                commandProcess.WaitForExit();

                isSuccess = true;
            }
            catch (Exception ex)
            {
                Logger.LogWriteLine("------ EXCEPTION caught while trying to execute a script in a separate process! ------------------------------------");
                Logger.LogWriteLine(string.Format("    Script:     {0}", scriptFileName));
                Logger.LogWriteLine(string.Format("    Parameters: {0}", parameterString));
                Logger.LogWriteLine("    Exception:   " + ex.ToString());
                throw;
            }

            return isSuccess;
        }
    }
}