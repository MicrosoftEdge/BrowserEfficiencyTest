# Usage

BrowserEfficiencyTest is best run from the command line, and requires an instance of [Elevator](https://github.com/MicrosoftEdge/Elevator) running if you use  `-measureset` or `-ms`.

> **WARNING**
> When run on Microsoft Edge, this will delete browser data, including history, saved passwords, and form fill.

## Parameters

Usage:

```
BrowserEfficiencyTest.exe [-browser|-b [chrome|edge|firefox|opera|operabeta] [-workload|-w <workload name>]|[-scenario|-s <scenario1> <scenario2>]] [-iterations|-i <iterationcount>] [-resultspath|-rp <etlpath>] [-measureset|-ms <measureset1> <measureset2>] [-profile|-p <chrome profile path>] [-attempts|-a <attempts to make per iteration>] [-notimeout] [-noprocessing|-np] [-credentialpath|-cp <path to credentials json file>] [-responsiveness|-r] [-filelogging|-fl [<path for logfile>]] [-capturebaseline|-cb <integer representing number of seconds>] [-extensions|-e <path to directory containing unpacked extension AppX(s)>] [-clearbrowsercache|-cbc] [-warmuprun|-wu] [-host|-h <host name>] [-port <port number>]
```

*   **-browser|-b** Selects the browser or browsers to run the scenarios with. This option must be provided. Multiple browsers can be selected by separating each browser with a space. E.g. `-b edge chrome`. The possible options are:
    * `edge` will include Microsoft Edge in the test pass
    * `chrome` will include Chrome in the test pass
    * `firefox` will include Firefox in the test pass
    * `opera` will include Opera in the test pass

*   **-workload|-w** Selects the workload to run. This option must be provided unless a scenario is provided instead. The possible options are provided here, but they can be easily modified in `workloads.json`:

    *   `representative` This workload runs through 12 common sites across 4 tabs, spending a minute and a half on each one, with some meaningful interaction with each of them. This workload takes 18 minutes to complete and requires credentials to be specified in `credentials.json` for Facebook and Gmail.
    *   `heavymultitab` This workload runs through 8 sites all in different tabs, with interaction on each of them. It represents heavy usage of the browser especially across many tabs. It takes about 8 minutes to complete and requires credentials to be specified in `credentials.json` for Facebook and Gmail.
    *   `simple` This is a simple workload that finishes quickly for testing purposes. It does not require any credentials.

*   **-scenario|-s** Selects the scenario or scenarios to run. This option must be provided unless a workload is provided instead. Multiple scenarios can be selected by separating each scenario with a space. Scenario names are not case sensitive. E.g. `-s wikipedia gmail facebook`. When multiple scenarios are selected, they will all be run on every browser, in the order they were provided, all in different tabs. When a scenario completes and there's additional scenarios after it, it will be left running in a background tab. The possible options are:

    *   `AboutBlank` will go to about:blank, loading no content for one minute
    *   `AmazonSearch` will load Amazon, do a search for "game of thrones", click on the first result, and then scroll down to the reviews
    *   `BbcNews` will load BBC, click on the top story, and scroll down
        *   Responsiveness measures: Homepage load time, article load time
    *   `BrainPopAvalanches` will navigate through Brainpop to the page on avalanches.
    *   `CnnOneStory` will directly load a news story from CNN, but not interact with CNN besides the page load
    *   `ColoradoStatesOfMatter` will navigate to and load a simulation on the states of matter.
    *   `EspnHomepage` will load ESPN and scroll through the infinite list on the homepage
        *   Responsiveness measures: ESPN homepage load time
    *   `FacebookNewsfeedScroll` will log into facebook with the provided credentials, and scroll through the news feed. Requires facebook credentials to be provided in the `credentials.json` file
    *   `FastScenario` will load google and quickly exit. This is designed to be fast, and is for testing BrowserEfficiencyTest
    *   `GmailGoThroughEmails` will load Gmail, and scroll through 5 emails in the inbox. Requires gmail credentials to be provided in the `credentials.json` file
    *   `GooglePrimeFactorization` will load Google, search for "prime factorization", and then read an article on how to do prime factorization.
    *   `GoogleSearch` will navigate to google.com and then search for "Seattle". It sees the results of the search, but doesn't navigate to any of them
    *   `HistoryWWII` will go to topics on History.com, then browse to "American Women in World War II" and watch the video / read the article.
    *   `InstagramNYPL` will load up the public feed for the NY public library and scroll down several times
        *   Responsiveness measures: Page load time
    *   `IxlEighthGradeScience` will go to Ixl.com, navigate to 8th grade science, and attempt to answer questions in a quiz.
    *   `KhanAcademyGrade8Math` will go to Khan academy, and then watch a video about repeating decimals
    *   `LinkedInSatya` will load the LinkedIn profile for Satya Nadella
    *   `Msn` will navigate to MSN
    *   `Msnbc` will navigate to MSNBC
    *   `NewselaChineseNewYear` will go to Newsela, then search for the article on Chinese New Year.
    *   `OfficeLauncher` will navigate to Office.com and then log in. It requires credentials to be provided in `credentials.json`
    *   `OfficePowerpoint` will navigate to the office log in page, log in, open PowerPoint in a new tab, open the most recent book, then go to edit mode. It requires credentials to be provided in `credentials.json`
    *   `OutlookEmail` will navigate to outlook.com, log in, and scroll through 5 emails. It will then send a new email to another account. Requires outlook credentials to be provided in the `credentials.json` file
    *   `OutlookOffice` will navigate to the office log in page, log in, open Word in a new tab, open the most recent document, then go to edit mode. It requires credentials to be provided in `credentials.json`
    *   `PinterestExplore` will navigate to pinterest, log in, and scroll through pins. It requires credentials to be provided in `credentials.json`
    *   `PowerBiBrowse` will navigate to PowerBI, log in, click through two graphs and go back to the dashboard. It requires credentials to be provided in `credentials.json`
    *   `RedditSurfaceSearch` will navigate to Reddit.com, perform a search form  "Microsoft Surface" and scroll down through the results.
    *   `ScholasticHarryPotter` will browse through the Harry Potter section of the Scholastic.com site.
    *   `TechRadarSurfacePro4Review` will navigate to techradar.com's review of the Surface Pro 4 and scroll through the page
    *   `TumblrTrending` will navigate to tumblr.com, click on "staff picks", then click on "trending", before scrolling through the trending posts
    *   `TwitterPublic` will navigate to the Microsoft Twitter page and scroll through the featured posts on the homepage
    *   `WikipediaUnitedStates` will navigate to the Wikipedia article on the United States and scroll
        *   Responsiveness measures: Article page load time
    *   `YahooNews` will navigate to Yahoo.com, go to news, and navigate to a story (the story is injected by Javascript so it will always navigate to the same one). It will then scroll through it before going back to the listing of all news.
        *   Responsiveness measures: Homepage load time, article page load time
    *   `YelpSeattleDinner` will navigate to Yelp, search for a restaurant, and click into it
    *   `YoutubeTrigonometry` will go to Youtube, search for "trigonometry", and then watch a video about it.
    *   `YoutubeWatchVideo` will play the video "Microsoft Design: Connecting Makers" on Youtube
    *   `ZillowSearch` will load a map of places for sale, expand the map view, then load the top listing

*   **-iterations|-i** Selects the number of times to run each set of scenarios on each browser. Each time will result in its own trace file, and its own measurement(s) which can be aggregated and analyzed however desired. Running multiple iterations is highly recommended to account for variablity in the test. If not provided, 1 iteration will be run

*   **-resultspath|-rp** Provides the path to save the trace files in. If not provided, trace files will be saved to the current working directory.

*   **-measureset|-ms** Determines which measures will be traced for and extracted out of the traces. If not provided, no tracing will be done and no measures will be provided after the test completes. You may wish to do this if you are testing or extracting measures in some other way not covered within BrowserEfficiencyTest.  The trace files will be saved in the current working directory unless otherwise specified with the '-resultspath|-rp' parameters. The possible measure sets are:
    * `cpuUsage` will measure how much the CPU was used during the test. Specifically, it measures how much time the CPU was not idle during the test pass.

    * `diskUsage` will measure the disk activity during the test pass.

    * `energy` will measure how much energy was consumed by the system during the test pass.

      **Note:** The `energy` measure set only works on a Surface Book with the Maxim power chip driver installed, and in order to measure total system power, the top must be detached from the keyboard and unplugged from power.

    * `energyVerbose` detailed version of the 'energy' measure set. Reports the energy for the system broken down by component and applications that ran during the test pass.

    * `networkUsage` will measure the network activity during the test pass.

    * `refSet` is a measure of the memory used during the test pass.

*   **-profile|-p** Enables the Chrome driver to use the passed in profile. Requires passing in the path to the Chrome profile.

*   **-attempts|-a** Defines the number of attempts to make per iteration. If an exception is caught, the trace is discarded and a new attempt is made. This paramater allows you to override the default number of attempts before giving up on the iteration, which is 3.

*   **-notimeout** Allows the test run to continue even if the scenario took longer than expected to complete. Without this flag, the test harness will throw out the run and attempt again if any scenario takes longer to complete than its specified duration.

*   **-noprocessing|-np** Allows the test to run without post processing the results at the end of the test. Use this flag with the `-measureset|-ms` options to collect trace files for the specified measureset but skip post processing of the results after the test completes. This is useful where you want to run the test and collect etl traces but want to process the results separately at a later time.

*   **-credentialpath|-cp** Allows a path to be specified that points to a different credentials.json file. An absolute or a relative path can be provided, though the path cannot contain any spaces.

*   **-responsiveness|-r** Records all responsiveness measures specified by the scenarios (e.g. page load time). These will be included in the resulting CSV when the run is complete.

*   **-filelogging|-fl** Enables recording of BrowserEfficiencyTest log messages to a file. The path of where to save the log file can be optionally passed as the first parameter after `-filelogging` or `-fl`

*   **-capturebaseline|-cb** Captures a trace for the specificed number of seconds without running any browser or test. Use this option to capture a baseline trace of the system where the system is not running any test or browser. Specify the length of time in seconds to capture the baseline trace for. Only available when using the `-measureset|-ms` option.

*   **-extensions|-e** Allows unpacked extensions located in the specified folder to be side loaded in the browser when tests are run. Currently, this capability is only supported in Microsoft Edge.

*   **-clearbrowsercache|-cbc** Clear Edge browser cache before each test run. By default, Edge browser does not clear its cache before each run.

*   **-warmuprun|-wu** Executes an initial test run on all selected browsers without tracing before the main test run loop. Can be used to have browsers such as Edge cache webpage content before the main test run loop.

*   **-host|-h** Host name of a remote machine running an instance of MicrosoftWebDriver.exe. Currently, this capability is only supported in Microsoft Edge.

*   **-port** Port number of the host to use when sending webdriver commands to a remote host. Currently, this capability is only supported in Microsoft Edge.

*   **-region** Specifies the region name to use in the ActiveRegion.xml (located under MeasureSetDefinitionAssets) when post processing the results.

*   **-verbose** Enables verbose logging output of MicrosoftWebDriver.exe.

*   **-executescript|-es** Enables execution of an external script whenever a scenario starts, stops or an exception is found. The intent is to allow an external script to start and stop tracing or other related functions. The script file name must be specified after the parameter.

## Examples

### Simple one tab, one browser, no measures

This example will run the WikipediaUnitedStates scenario on Microsoft Edge. Since it's only one scenario, it will only use one tab. This example does not take any traces or result in any measures; it simply uses Webdriver to automate the given scenario on the given browser.

```BrowserEfficiencyTest.exe -browser edge -scenario WikipediaUnitedStates```

 or

```BrowserEfficiencyTest.exe -b edge -s WikipediaUnitedStates```

### Running multiple scenarios on multiple browsers

This example will run WikipediaUnitedStates, YoutubeWatchVideo, and FacebookNewsfeedScroll on Microsoft Edge, Chrome, and Firefox. Each scenario (site) will be run in its own tab on each browser. No measureset is defined, so this run will only automate the actions on each browser; it won't result in any measurements being taken.

Remember that using the FacebookNewsfeedScroll scenario requires defining a Facebook login inside the `credentials.json` file.

```BrowserEfficiencyTest.exe -browser edge chrome firefox -scenario WikipediaUnitedStates YoutubeWatchVideo FacebookNewsfeedScroll```

or

```BrowserEfficiencyTest.exe -b edge chrome firefox -s WikipediaUnitedStates YoutubeWatchVideo FacebookNewsfeedScroll```

### Running a workload

Instead of specifying individual scenarios, a workload can be provided, which will execute the scenarios in `workloads.json`. The defined workloads generally require credentials to be provided. More details on each workload are provided above.

```BrowserEfficiencyTest.exe -browser edge chrome -workload representative```

or

```BrowserEfficiencyTest.exe -b edge chrome -w representative```

### Getting measurements through tracing

This example builds off the above example with multiple scenarios on multiple browsers. It will run the same automation on the same browsers, but will also measure how much CPU the system consumed to run each of them. The final results will be outputted in a CSV file located along with the traces from each iteration and each browser in the path you specify with the 'resultspath' option.

Remember that an instance of Elevator must be running and listening for a client connection in order for this command to complete.

It's highly recommended that you aggregate multiple iterations of a test run in order to get a final number, because these results have variability. Therefore, this example will also run 10 iterations. This results in each browser running through the full set of scenarios 10 times, and each run having its own associated trace. Each iteration will show up as its own row (or set of rows, if the measure set has multiple measures) in the final CSV file.

```BrowserEfficiencyTest.exe -browser edge chrome firefox -scenario WikipediaUnitedStates YoutubeWatchVideo FacebookNewsfeedScroll -resultspath C:\Some\Path\To\Store\Traces -measureset cpuUsage -iterations 10```

or

```BrowserEfficiencyTest.exe -b edge chrome firefox -s WikipediaUnitedStates YoutubeWatchVideo FacebookNewsfeedScroll -rp C:\Some\Path\To\Store\Traces -ms cpuUsage -i 10```

### Collecting measurement traces but processing the measurement results at a later time

This example is the same as the above example except the post processing of the measurement results is skipped. Since the length of time needed for post processing of the measurement increases the more browsers, scenarios, measuresets and iterations there are it may be desirable to skip the immediate post processing of the trace files when the test completes so it can be done at a later time. This is where the -noprocessing|-np option is useful.

This example will create a trace file (.ETL file) for each combination of browser, scenario, and iteration for the cpuUsage measureset but the traces will not be automatically processed after the test.

Remember that an instance of Elevator must be running and listening for a client connection in order for this command to complete.

```BrowserEfficiencyTest.exe -browser edge chrome firefox -scenario WikipediaUnitedStates YoutubeWatchVideo FacebookNewsfeedScroll -resultspath C:\Some\Path\To\Store\Traces -measureset cpuUsage -iterations 10 -noprocessing```

or

```BrowserEfficiencyTest.exe -b edge chrome firefox -s WikipediaUnitedStates YoutubeWatchVideo FacebookNewsfeedScroll -rp C:\Some\Path\To\Store\Traces -ms cpuUsage -i 10 -np```

Then, with the resulting trace files, we can process the results separately without running the test again by executing BrowserEfficiencyTest.exe and omitting the `-browser|-b` and `-scenario|-s` options. We will need to include the `-resultspath|-rp` and `-measureset|-ms` options so that BrowserEfficiencyTest knows where the ETL files are and what measureset to process the ETL files with.

```BrowserEfficiencyTest.exe -resultspath C:\Some\Path\To\Store\Traces -measureset cpuUsage```

or

```BrowserEfficiencyTest.exe -rp C:\Some\Path\To\Store\Traces -ms cpuUsage```

### Side Loading Extensions (only supported in Microsoft Edge)
To side load extensions in Microsoft Edge while tests run, the first step is to prepare a directory (`C:\unpackedExtensions` was used for the purpose of this demo) that contains one or more unpacked extension AppXs:

```
C:\unpackedExtensions
|---- extension1
|----|---- Assets
|----|---- AppXManifest.xml
|----|---- Extension
|----|----|---- manifest.json
|----|----|---- <otherExtFiles>
|---- extension2 (optional)
|----|---- Assets
|----|---- AppXManifest.xml
|----|---- Extension
|----|----|---- manifest.json
|----|----|---- <otherExtFiles>
```
#### Testing your own extension
If you wish prepare your own extension for testing, you can follow the first half of the Microsoft Edge Extension [ManifoldJS Packaging Guide](https://docs.microsoft.com/en-us/microsoft-edge/extensions/guides/packaging/using-manifoldjs-to-package-extensions) to generate an unpacked extension AppX. From there, you will need to copy the `manifest` folder that ManifoldJS generates into a folder (`C:\unpackedExtensions` for the purposes of this demo) to match the structure above. 

#### Testing an extension from the Windows Store
If you wish to run BrowserEfficiencyTest with an extension that is available from the Windows Store, you will need to perform the following steps:
1. Install an extension from the Windows Store in Microsoft Edge
2. Open an elevated command prompt
3. Run `cd C:\Program Files\WindowsApps`
4. Run `dir | findstr /i <searchQuery>` (where `searchQuery` is part of the name of the extension you installed)
5. Run `mkdir C:\unpackedExtensions`
5. Run `mkdir C:\unpackedExtensions\extension1`
6. Run `xcopy /r /s C:\ProgramFiles\WindowsApps\FolderReturnedInStep2 C:\unpackedExtensions\extension1`

This will copy the extension folder from the `WindowsApps` folder into `C:\unpackedExtensions\extension1`, mirroring the format shown above.

#### Using the -e flag
To load extensions in Microsoft Edge, run BrowserEfficiencyTest with the `-extensions|-e` flag as follows:

```BrowserEfficiencyTest.exe -browser edge -e C:\unpackedExtensions -workload representative``` 

While the tests execute, you will see the extension(s) located in the `unpackedExtensions` directory loaded in Microsoft Edge. The name of the side loaded extensions will be included in the `browser` column of the results CSV.