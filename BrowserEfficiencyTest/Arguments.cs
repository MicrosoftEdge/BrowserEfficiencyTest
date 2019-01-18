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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Parses the command line arguments and provides access to the various 
    /// arguments and options.
    /// </summary>
    internal class Arguments
    {
        private static readonly List<string> s_SupportedBrowsers = new List<string> { "chrome", "edge", "firefox", "opera" };

        private Dictionary<string, WorkloadScenario> _possibleScenarios;
        private List<WorkloadScenario> _scenarios;
        private List<string> _browsers;
        private Dictionary<string, MeasureSet> _availableMeasureSets;
        private List<MeasureSet> _selectedMeasureSets;
        private List<Workload> _workloads;

        public string ScenarioName { get; private set; }
        public string BrowserProfilePath { get; private set; }
        public int Iterations { get; private set; }
        public bool UsingTraceController { get; private set; }
        public string EtlPath { get; private set; }
        public string ExtensionsPath { get; private set; }
        public int MaxAttempts { get; private set; }
        public bool OverrideTimeout { get; private set;  }
        public bool DoPostProcessing { get; private set; }
        public string CredentialPath { get; private set; }
        public bool MeasureResponsiveness { get; private set; }
        public bool ArgumentsAreValid { get; private set; }
        public string BrowserEfficiencyTestVersion { get; private set; }
        public bool CaptureBaseline { get; private set; }
        public int BaselineCaptureSeconds { get; private set; }
        public bool ClearBrowserCache { get; private set; }
        public bool DoWarmupRun { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string RegionOfInterest { get; private set; }
        public bool EnableVerboseWebDriverLogging { get; private set; }
        public bool EnableScenarioTracing { get; private set; }
        public string ExecuteScriptFileName { get; private set; }

        /// <summary>
        /// List of all scenarios to be run.
        /// </summary>
        public IReadOnlyCollection<WorkloadScenario> Scenarios
        {
            get { return _scenarios.AsReadOnly(); }
        }

        /// <summary>
        /// List of all browsers to be run.
        /// </summary>
        public IReadOnlyCollection<string> Browsers
        {
            get { return _browsers.AsReadOnly(); }
        }

        /// <summary>
        /// List of all measure sets selected to be run.
        /// </summary>
        public IReadOnlyCollection<MeasureSet> SelectedMeasureSets
        {
            get { return _selectedMeasureSets.AsReadOnly(); }
        }

        /// <summary>
        /// Initializes a new instance of the Arguments class and processes the passed in array of command line arguments.
        /// Based on user input in the args, here we break them apart and determine:
        ///  - Which browser to run on
        ///  - Which scenario(s) to run
        ///  - How many loops to execute
        /// </summary>
        /// <param name="args">Array of strings containing the command line arguments.</param>
        public Arguments(string[] args)
        {
            _possibleScenarios = new Dictionary<string, WorkloadScenario>();
            _scenarios = new List<WorkloadScenario>();
            _browsers = new List<string>();
            _availableMeasureSets = PerfProcessor.AvailableMeasureSets.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value);
            _selectedMeasureSets = new List<MeasureSet>();
            _workloads = new List<Workload>();

            ScenarioName = "";
            BrowserProfilePath = "";
            Iterations = 1;
            UsingTraceController = false;
            EtlPath = Directory.GetCurrentDirectory();
            ExtensionsPath = "";
            MaxAttempts = 3;
            OverrideTimeout = false;
            DoPostProcessing = true;
            CredentialPath = "credentials.json";
            MeasureResponsiveness = false;
            BrowserEfficiencyTestVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CaptureBaseline = false;
            BaselineCaptureSeconds = 600; // 10 minutes as the default
            ClearBrowserCache = false;
            DoWarmupRun = false;
            Host = "localhost";
            Port = 17556; // 17556 is the default port value MicrosoftWebDriver.exe uses
            RegionOfInterest = "";
            EnableVerboseWebDriverLogging = false;
            EnableScenarioTracing = false;
            ExecuteScriptFileName = "";

            CreatePossibleScenarios();
            LoadWorkloads();
            ArgumentsAreValid = ProcessArgs(args);
        }

        /// <summary>
        /// Go through and process the list of passed in commandline arguments.
        /// Here we'll decide which browser, scenarios, and number of loops to run.
        /// If any of the arguments are invalid, this method returns false.
        /// </summary>
        private bool ProcessArgs(string[] args)
        {
            bool argumentsAreValid = true;

            for (int argNum = 0; (argNum < args.Length) && argumentsAreValid; argNum++)
            {
                var arg = args[argNum].ToLowerInvariant();
                switch (arg)
                {
                    case "-browser":
                    case "-b":
                        // One or more browsers can be specified after the -b|-browser option.
                        while (argumentsAreValid && ((argNum + 1) < args.Length) && !(args[argNum + 1].StartsWith("-")))
                        {
                            argNum++;

                            if (args[argNum].ToLowerInvariant() == "all")
                            {
                                _browsers = s_SupportedBrowsers;
                                break;
                            }
                            else if (s_SupportedBrowsers.Contains(args[argNum].ToLowerInvariant()))
                            {
                                _browsers.Add(args[argNum].ToLowerInvariant());
                            }
                            else
                            {
                                argumentsAreValid = false;
                                Logger.LogWriteLine("Invalid or unsupported browser specified!", false);
                            }
                        }

                        if (_browsers.Count == 0)
                        {
                            // no browsers were specified.
                            argumentsAreValid = false;
                            Logger.LogWriteLine("No valid browsers were specified!", false);
                        }

                        break;
                    case "-workload":
                    case "-w":
                        // One workload must be specified after the -w|-workload option.
                        // If no workload is specified after the -w|-workload option then display a list of available workloads
                        // and the scenarios in them.
                        argNum++;

                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            Workload selectedWorkload = _workloads.FirstOrDefault(wl => wl.Name.ToLowerInvariant() == args[argNum].ToLowerInvariant());

                            if (selectedWorkload == null)
                            {
                                argumentsAreValid = false;
                                Logger.LogWriteLine(string.Format("The specified workload '{0}' was not found!", args[argNum]), false);
                            }
                            else
                            {
                                bool successfullyAddedScenarios = AddScenariosInWorkload(selectedWorkload);
                                if (!successfullyAddedScenarios)
                                {
                                    argumentsAreValid = false;
                                    Logger.LogWriteLine(string.Format("Invalid scenario specified in workload '{0}'!", selectedWorkload.Name), false);
                                }
                                else
                                {
                                    // add the workload name to the ScenarioName string which is used elsewhere such as part of naming ETL files.
                                    if (string.IsNullOrEmpty(ScenarioName))
                                    {
                                        ScenarioName = selectedWorkload.Name;
                                    }
                                    else
                                    {
                                        ScenarioName = ScenarioName + "-" + selectedWorkload.Name;
                                    }
                                }
                            }
                        }
                        else
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("No valid workload was specified!", false);
                            DisplayAvailableWorkloads();
                        }

                        break;
                    case "-scenario":
                    case "-s":
                        // One or more scenarios must be specified after the -s|-scenario option.
                        // If no scenario name is passed after the -s|-scenario option or an invalid scenario is
                        // specified, display a list of all available scenarios to the console window.
                        while (argumentsAreValid && ((argNum + 1) < args.Length) && !(args[argNum + 1].StartsWith("-")))
                        {
                            argNum++;
                            string selectedScenario = args[argNum].ToLowerInvariant();

                            if (_possibleScenarios.ContainsKey(selectedScenario))
                            {
                                _scenarios.Add(_possibleScenarios[selectedScenario]);

                                // add each of the specified scenario names to the ScenarioName string which is used elsewhere such as part of naming ETL files.
                                if (string.IsNullOrEmpty(ScenarioName))
                                {
                                    ScenarioName = selectedScenario;
                                }
                                else
                                {
                                    ScenarioName = ScenarioName + "-" + selectedScenario;
                                }
                            }
                            else
                            {
                                argumentsAreValid = false;
                                Logger.LogWriteLine(string.Format("The specified scenario '{0}' does not exist!", selectedScenario), false);
                            }
                        }

                        if (_scenarios.Count == 0)
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("No valid scenario specified!", false);
                            DisplayAvailableScenarios();
                        }

                        break;
                    case "-resultspath":
                    case "-rp":
                        // A valid path must be specified after the -rp|-resultspath option.
                        // If the path does not exist, create it.
                        argNum++;

                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            string etlPath = args[argNum];

                            if (!Directory.Exists(etlPath))
                            {
                                Directory.CreateDirectory(etlPath);
                            }

                            EtlPath = Path.GetFullPath(etlPath);
                        }
                        else
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("Invalid results path!", false);
                        }

                        break;
                    case "-extensions":
                    case "-e":
                        foreach (var browser in _browsers)
                        {
                            if (browser.ToLower() != "edge")
                            {
                                argumentsAreValid = false;
                                Logger.LogWriteLine("Side loading of extensions is supported only in Microsoft Edge", false);
                            }
                        }

                        if (argumentsAreValid)
                        {
                            argNum++;
                            // A valid path must be specified after the -e|-extensions option.
                            if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                            {
                                string extensionsPath = args[argNum];
                                if (!Directory.Exists(extensionsPath))
                                {
                                    argumentsAreValid = false;
                                    Logger.LogWriteLine("Invalid extensions path: " + extensionsPath, false);
                                }
                                else
                                {
                                    ExtensionsPath = Path.GetFullPath(extensionsPath);
                                }
                            }
                            else
                            {
                                argumentsAreValid = false;
                                Logger.LogWriteLine("Invalid extensions path!", false);
                            }
                        }

                        break;
                    case "-measureset":
                    case "-ms":
                        // One or more measuresets must be specified after the -ms|-measureset option.
                        // If no measureset is specified after the -ms|-measureset option or an invalid measureset is
                        // specified, display a list of available measuresets.
                        while (argumentsAreValid && ((argNum + 1) < args.Length) && !(args[argNum + 1].StartsWith("-")))
                        {
                            argNum++;
                            string measureSet = args[argNum].ToLowerInvariant();

                            if (_availableMeasureSets.ContainsKey(measureSet))
                            {
                                UsingTraceController = true;
                                _selectedMeasureSets.Add(_availableMeasureSets[measureSet]);
                            }
                            else
                            {
                                // The specified measureset is invalid.
                                argumentsAreValid = false;
                                Logger.LogWriteLine(string.Format("The specified measureset '{0}' does not exist!", measureSet), false);
                            }
                        }

                        if (_selectedMeasureSets.Count == 0)
                        {
                            // No measuresets or no valid measuresets were specified.
                            argumentsAreValid = false;
                            Logger.LogWriteLine("A valid measureset must be specified after the -ms|measureset option.", false);
                            DisplayAvailableMeasureSets();
                        }

                        break;
                    case "-iterations":
                    case "-i":
                        // An integer value greater than 0 must be specified after the -i|-iterations option.
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            int iterations = 0;
                            argumentsAreValid = int.TryParse(args[argNum], out iterations);
                            Iterations = iterations;
                        }
                        else
                        {
                            // No iteration value was specified after the -i|-iterations option.
                            argumentsAreValid = false;
                        }

                        if (!argumentsAreValid || Iterations < 1)
                        {
                            Logger.LogWriteLine("Invalid value for iterations. Must be an integer greater than 0.", false);
                        }

                        break;
                    case "-attempts":
                    case "-a":
                        // An integer value greater than 0 must be specified after the -a|-attempts option.
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            int attempts = 0;
                            argumentsAreValid = int.TryParse(args[argNum], out attempts);
                            MaxAttempts = attempts;
                        }
                        else
                        {
                            // No attempts value was specified after the -a|-attemps option.
                            argumentsAreValid = false;
                        }

                        if (!argumentsAreValid || MaxAttempts < 1)
                        {
                            Logger.LogWriteLine("Invalid value for attempts. Must be an integer greater than 0.", false);
                        }

                        break;
                    case "-profile":
                    case "-p":
                        // An existing folder path must be specified after the -p|-profile option.
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            BrowserProfilePath = args[argNum];
                        }
                        else
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("A valid profile path must be specified after the -p|-profile option!", false);
                        }

                        break;
                    case "-notimeout":
                        OverrideTimeout = true;
                        break;
                    case "-noprocessing":
                    case "-np":
                        DoPostProcessing = false;
                        break;
                    case "-credentialpath":
                    case "-cp":
                        // An existing credential file must be passed after the -cp|-credentialpath option.
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            CredentialPath = args[argNum];
                            if (!File.Exists(CredentialPath))
                            {
                                argumentsAreValid = false;
                                Logger.LogWriteLine(string.Format("The credential file: {0} does not exist!", CredentialPath), false);
                            }
                        }
                        else
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("A valid credential file must be specified after the -cp|-credentialpath option!", false);
                        }

                        break;
                    case "-responsiveness":
                    case "-r":
                        MeasureResponsiveness = true;
                        break;
                    case "-filelogging":
                    case "-fl":
                        // Enable file logging. If a path is specified after the -fl|-filelogging option, then place the file log
                        // in the specified path.
                        string logPath = Directory.GetCurrentDirectory();

                        if (((argNum + 1) < args.Length) && !(args[argNum + 1].StartsWith("-")))
                        {
                            argNum++;
                            logPath = args[argNum];

                            if (!Directory.Exists(logPath))
                            {
                                Directory.CreateDirectory(logPath);
                            }

                            logPath = Path.GetFullPath(logPath);
                        }

                        Logger.SetupFileLogging(logPath);
                        Logger.LogWriteLine("Arguments: " + string.Join(" ", args), false);
                        break;
                    case "-capturebaseline":
                    case "-cb":
                        // Enable capturing an ETL of the system doing nothing as the system baseline.
                        // The number specified is the number of seconds to capture file logging.
                        // If no number is specified then just use the default
                        // This parameter has no effect if the measureset option is not selected
                        CaptureBaseline = true;

                        if (((argNum + 1) < args.Length) && !(args[argNum + 1].StartsWith("-")))
                        {
                            argNum++;
                            int seconds = 0;
                            argumentsAreValid = int.TryParse(args[argNum], out seconds);

                            if ((BaselineCaptureSeconds > 0) && (BaselineCaptureSeconds < 36000))
                            {
                                BaselineCaptureSeconds = seconds;
                            }
                            else
                            {
                                argumentsAreValid = false;
                            }
                        }

                        if (!argumentsAreValid)
                        {
                            Logger.LogWriteLine("Invalid value for the baseline capture time in seconds. Must be an integer greater than 0 and less than 36000.", false);
                        }
                        break;
                    case "-clearbrowsercache":
                    case "-cbc":
                        ClearBrowserCache = true;
                        break;
                    case "-warmuprun":
                    case "-wu":
                        DoWarmupRun = true;
                        break;
                    case "-host":
                    case "-h":
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            Host = args[argNum];
                        }
                        else
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("A valid host must be specified after the -host|-h option!", false);
                        }

                        break;
                    case "-port":
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            int portNumber = 0;
                            argumentsAreValid = int.TryParse(args[argNum], out portNumber);
                            Port = portNumber;
                        }
                        else
                        {
                            argumentsAreValid = false;
                        }

                        if (!argumentsAreValid)
                        {
                            Logger.LogWriteLine("A valid port number must be specified after the -port option!", false);
                        }

                        break;
                    case "-region":
                        // The name of the region must be specified after the -region option.
                        // The region must be defined in the ActiveRegion.xml
                        // See https://docs.microsoft.com/en-us/windows-hardware/test/wpt/regions-of-interest for more information
                        // on Regions of Interest.
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            RegionOfInterest = args[argNum];
                        }
                        else
                        {
                            argumentsAreValid = false;
                            Logger.LogWriteLine("A valid region of interest name must be specified after the -region option!", false);
                        }
                        break;
                    case "-verbose":
                        EnableVerboseWebDriverLogging = true;
                        break;
                    case "-enablescenariotracing":
                        EnableScenarioTracing = true;
                        break;
                    case "-executescript":
                    case "-es":
                        // The script name must follow the -es|-executescript option.
                        argNum++;
                        if ((argNum < args.Length) && !(args[argNum].StartsWith("-")))
                        {
                            ExecuteScriptFileName = args[argNum];
                        }
                        else
                        {
                            // No filename was specified after the -es|-executescript option.
                            argumentsAreValid = false;
                            Logger.LogWriteLine("A valid script filename must be specified after the -es|-executescript option!", false);
                        }
                        break;
                    default:
                        argumentsAreValid = false;
                        Logger.LogWriteLine(string.Format("Invalid argument encountered '{0}'", args[argNum]), false);
                        DisplayUsage();

                        break;
                }
            }

            // For running the test, both a valid browser and scenario must be specified. If only one of either is set then that's an error condition.
            if (argumentsAreValid && (_scenarios.Count == 0 ^ _browsers.Count == 0))
            {
                argumentsAreValid = false;
                if (_scenarios.Count > 0)
                {
                    Logger.LogWriteLine(" No valid browser was specified for the specified scenario(s) or workload!", false);
                }
                else
                {
                    Logger.LogWriteLine(" No valid scenario or workload was specified for the specified browser(s)!", false);
                }
                Logger.LogWriteLine("Both a browser and a scenario or workload must be specified to run the test!", false);
                Logger.LogWriteLine("If you wish to only run the performance processor then omit the browser and scenario/workload arguments and specify a measureset.", false);
            }

            // If a user specifies a region of interest and a measureset then assign that region of interest to each selected measureset
            if (argumentsAreValid && RegionOfInterest != "" && UsingTraceController)
            {
                foreach (var measureSet in _selectedMeasureSets)
                {
                    measureSet.WpaRegionName = RegionOfInterest;
                }
            }

            Logger.LogWriteLine(string.Format("BrowserEfficiencyTest Version: {0}", BrowserEfficiencyTestVersion), false);

            if (args.Length == 0)
            {
                // No options were specified so display the usage, available scenarios, available workloads and available measuresets.
                DisplayUsage();
                DisplayAvailableScenarios();
                DisplayAvailableWorkloads();
                DisplayAvailableMeasureSets();
            }

            return argumentsAreValid;
        }

        // Output the command line options.
        private void DisplayUsage()
        {
            Logger.LogWriteLine("Usage:", false);
            Logger.LogWriteLine("BrowserEfficiencyTest.exe "
                                + "[-browser|-b [chrome|edge|firefox|opera|operabeta] "
                                + "[-scenario|-s <scenario1> <scenario2>] "
                                + "[-iterations|-i <iterationcount>] "
                                + "[-resultspath|-rp <etlpath>] "
                                + "[-measureset|-ms <measureset1> <measureset2>] "
                                + "[-profile|-p <chrome profile path>] "
                                + "[-attempts|-a <attempts to make per iteration>] "
                                + "[-notimeout] "
                                + "[-noprocessing|-np] "
                                + "[-workload|-w <workload name>] "
                                + "[-credentialpath|-cp <path to credentials json file>] "
                                + "[-responsiveness|-r] "
                                + "[-filelogging|-fl [<path for logfile>]] "
                                + "[-capturebaseline|-cb <integer representing number of seconds>] "
                                + "[-extensions|-e <path to directory containing unpacked extension AppX(s)>] "
                                + "[-clearbrowsercache|-cbc] "
                                + "[-warmuprun|-wu] "
                                + "[-host|-h <host name>] "
                                + "[-port <port number>] "
                                + "[-region <region of interest name from ActiveRegion.xml> "
                                + "[-verbose] "
                                + "[-enablescenariotracing] "
                                + "[-executescript|-es <script file name>] "
                                , false);
        }

        // Output all the available scenarios.
        private void DisplayAvailableScenarios()
        {
            Logger.LogWriteLine("Available scenarios:", false);
            foreach (var scenario in _possibleScenarios)
            {
                Logger.LogWriteLine(scenario.Key, false);
            }
        }

        // Output all the available workloads and the scenarios specified in those workloads.
        private void DisplayAvailableWorkloads()
        {
            Logger.LogWriteLine("Available workloads and scenarios:", false);
            foreach (var workload in _workloads)
            {
                Logger.LogWriteLine(string.Format("Workload: {0}", workload.Name), false);
                Logger.LogWriteLine("  Scenarios:", false);

                foreach (var scenario in workload.Scenarios)
                {
                    Logger.LogWriteLine(string.Format("    {0}", scenario.ScenarioName), false);
                }
            }
        }

        // Output all the available measuresets.
        private void DisplayAvailableMeasureSets()
        {
            Logger.LogWriteLine("Available measuresets:", false);
            foreach (var measureSet in _availableMeasureSets)
            {
                Logger.LogWriteLine(measureSet.Key.ToString(), false);
            }
        }

        /// <summary>
        /// All scenarios must be instantiated and added to the list of possible scenarios in this method.
        /// The order doensn't matter.
        /// </summary>
        private void CreatePossibleScenarios()
        {
            // All scenarios are added to the list even if they're not final / not great. Order doesn't matter here.
            AddScenario(new FacebookNewsfeedScroll());
            AddScenario(new GmailGoThroughEmails());
            AddScenario(new Msn());
            AddScenario(new Msnbc());
            AddScenario(new WikipediaUnitedStates());
            AddScenario(new YoutubeWatchVideo());
            AddScenario(new AmazonSearch());
            AddScenario(new GoogleSearch());
            AddScenario(new TechRadarSurfacePro4Review());
            AddScenario(new YahooNews());
            AddScenario(new BbcNews());
            AddScenario(new CnnOneStory());
            AddScenario(new FastScenario());
            AddScenario(new OutlookOffice());
            AddScenario(new OutlookEmail());
            AddScenario(new PowerBiBrowse());
            AddScenario(new OfficePowerpoint());
            AddScenario(new AboutBlank());
            AddScenario(new OfficeLauncher());
            AddScenario(new YelpSeattleDinner());
            AddScenario(new ZillowSearch());
            AddScenario(new EspnHomepage());
            AddScenario(new LinkedInSatya());
            AddScenario(new TwitterPublic());
            AddScenario(new TumblrTrending());
            AddScenario(new InstagramNYPL());
            AddScenario(new PinterestExplore());
            AddScenario(new GooglePrimeFactorization());
            AddScenario(new YoutubeTrigonometry());
            AddScenario(new IxlEighthGradeScience());
            AddScenario(new ScholasticHarryPotter());
            AddScenario(new KhanAcademyGrade8Math());
            AddScenario(new HistoryWWII());
            AddScenario(new NewselaChineseNewYear());
            AddScenario(new ColoradoStatesOfMatter());
            AddScenario(new BrainPopAvalanches());
            AddScenario(new RedditSurfaceSearch());
            AddScenario(new Idle());
        }

        private void AddScenario(Scenario scenario)
        {
            _possibleScenarios.Add(scenario.Name.ToLowerInvariant(), new WorkloadScenario(scenario.Name.ToLowerInvariant(), "new", scenario.DefaultDuration, scenario));
        }

        // Load in the Workloads and their list of ScenarioWorkloads from workloads.json
        private void LoadWorkloads()
        {
            string jsonText = File.ReadAllText("workloads.json");
            _workloads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Workload>>(jsonText);
        }

        // Add the ScenarioWorkloads from the passed in workload to the list of ScenarioWorkloads to be executed
        // returns false if an invalid scenario was found in the workload        
        private bool AddScenariosInWorkload(Workload workload)
        {
            bool allScenariosAdded = true;

            foreach (var workloadScenario in workload.Scenarios)
            {
                string currentScenarioName = workloadScenario.ScenarioName.ToLowerInvariant();
                if (_possibleScenarios.ContainsKey(currentScenarioName))
                {
                    if (!(workloadScenario.Duration > 0))
                    {
                        // The workloadScenario does not have a valid duration so use the scenario's default
                        workloadScenario.Duration = _possibleScenarios[currentScenarioName].Duration;
                    }

                    workloadScenario.Scenario = _possibleScenarios[currentScenarioName].Scenario;
                    _scenarios.Add(workloadScenario);
                }
                else
                {
                    return false;
                }
            }

            return allScenariosAdded;
        }
    }
}
