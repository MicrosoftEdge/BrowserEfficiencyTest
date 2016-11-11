# Usage

BrowserEfficiencyTest is best run from the command line, and requires an instance of [Elevator](https://github.com/MicrosoftEdge/Elevator) running if you use `-tracecontrolled|-tc` and `-measureset|-ms`.

> **WARNING**
> When run on Microsoft Edge, this will delete all browser data, including bookmarks, saved passwords, and form fill.

## Parameters

Usage:

```
BrowserEfficiencyTest.exe -browser|-b [chrome|edge|firefox|opera|operabeta] -scenario|-s all|<scenario1> <scenario2> [-iterations|-i <iterationcount>] [-tracecontrolled|-tc <etlpath> -measureset|-ms <measureset1> <measureset2>] [-warmup|-w] [-profile|-p <chrome profile path>] [-attempts|-a <attempts to make per iteration>]
```

*   **-browser|-b (REQUIRED)** Selects the browser or browsers to run the scenarios with. Multiple browsers can be selected by separating each browser with a space. E.g. `-b edge chrome`. The possible options are:
    * `edge` will include Microsoft Edge in the test pass
    * `chrome` will include Chrome in the test pass
    * `firefox` will include Firefox in the test pass
    * `opera` will include Opera in the test pass

*   **-scenario|-s (REQUIRED)** Selects the scenario or scenarios to run. Multiple scenarios can be selected by separating each scenario with a space. E.g. `-s wikipedia gmail facebook`. When multiple scenarios are selected, they will all be run on every browser, in the order they were provided, all in different tabs. When a scenario completes and there's additional scenarios after it, it will be left running in a background tab. The possible options are:
    * `amazon` will load Amazon, do a search for "game of thrones", click on the first result, and then scroll down to the reviews
    * `bbcNews` will load BBC, click on the top story, and scroll down
    * `cnnOneStory` will directly load a news story from CNN, but not interact with CNN besides the page load
    * `cnnTopStory` will load CNN, load the top story, and scroll down
    * `facebook` will log into facebook with the provided credentials, and scroll through the news feed. Requires facebook credentials to be provided in the `config.json` file
    * `fastScenario` will load google and quickly exit. This is designed to be fast, and is for testing BrowserEfficiencyTest
    * `gmail` will load Gmail, and scroll through 5 emails in the inbox. Requires gmail credentials to be provided in the `config.json` file
    * `google` will navigate to google.com and then search for "Seattle". It sees the results of the search, but doesn't navigate to any of them
    * `msn` will navigate to MSN
    * `msnbc` will navigate to MSNBC
    * `outlook` will navigate to outlook.com, log in, and scroll through 10 emails. Requires outlook credentials to be provided in the `config.json` file
    * `techRadar` will navigate to techradar.com's review of the Surface Pro 4 and scroll through the page
    * `wikipedia` will navigate to the Wikipedia article on the United States and scroll
    * `yahooNews` will navigate to Yahoo.com, go to news, and navigate to the top story. It will then scroll through it.
    * `youtube` will play the video "Microsoft Design: Connecting Makers" on Youtube

*   **-iterations|-i** Selects the number of times to run each set of scenarios on each browser. Each time will result in its own trace file, and its own measurement(s) which can be aggregated and analyzed however desired. Running multiple iterations is highly recommended to account for variablity in the test. If not provided, 1 iteration will be run

*   **-tracecontrolled|-tc** Provides the path to save the trace files in. This must be provided if and only if `-measureset|-ms` is also provided. After the test completes, look for the trace files in this path. If not provided, no tracing will be done and no measures will be provided after the test completes. You may wish to do this if you are testing or extracting measures in some other way not covered within BrowserEfficiencyTest.

*   **-measureset|-ms** Determines which measures will be traced for and extracted out of the traces. This must be provided if and only if `tracecontrolled|-tc` is provided. If not provided, no tracing will be done and no measures will be provided after the test completes. You may wish to do this if you are testing or extracting measures in some other way not covered within BrowserEfficiencyTest. The possible measure sets are:
    * `cpuUsage` will measure how much the CPU was used during the test. Specifically, it measures how much time the CPU was not idle during the test pass.

    * `diskUsage` will measure the disk activity during the test pass.

    * `energy` will measure how much energy was consumed by the system during the test pass.

      **Note:** The `energy` measure set only works on a Surface Book with the Maxim power chip driver installed, and in order to measure total system power, the top must be detached from the keyboard and unplugged from power.

    * `energyVerbose` detailed version of the 'energy' measure set. Reports the energy for the system broken down by component and applications that ran during the test pass.

    * `networkUsage` will measure the network activity during the test pass.

    * `refSet` is a measure of the memory used during the test pass.

*   **-warmup|-w** Runs each of the scenarios on each browser once before starting measured iterations, to populate the cache

*   **-profile|-p** Enables the Chrome driver to use the passed in profile. Requires passing in the path to the Chrome profile.

*   **-attempts|-a** Defines the number of attempts to make per iteration. If an exception is caught, the trace is discarded and a new attempt is made. This paramater allows you to override the default number of attempts before giving up on the iteration, which is 3.

*   **-notimeout** Allows the test run to continue even if the scenario took longer than expected to complete. Without this flag, the test harness will throw out the run and attempt again if any scenario takes longer to complete than its specified duration.

## Examples

### Simple one tab, one browser, no measures

This example will run the wikipedia scenario on Microsoft Edge. Since it's only one scenario, it will only use one tab. This example does not take any traces or result in any measures; it simply uses Webdriver to automate the given scenario on the given browser.

```BrowserEfficiencyTest.exe -browser edge -scenario wikipedia```

 or

```BrowserEfficiencyTest.exe -b edge -s wikipedia```

### Running multiple scenarios on multiple browsers

This example will run wikipedia, youtube, and facebook on Microsoft Edge, Chrome, and Firefox. Each scenario (site) will be run in its own tab on each browser. No measureset is defined, so this run will only automate the actions on each browser; it won't result in any measurements being taken.

Remember that using the Facebook scenario requires defining a Facebook login inside the `config.json` file.

```BrowserEfficiencyTest.exe -browser edge chrome firefox -scenario wikipedia youtube facebook```

or

```BrowserEfficiencyTest.exe -b edge chrome firefox -s wikipedia youtube facebook```

### Getting measurements through tracing

This example builds off the above example with multiple scenarios on multiple browsers. It will run the same automation on the same browsers, but will also measure how much CPU the system consumed to run each of them. The final results will be outputted in a CSV file in the active directory, and the traces from each iteration or each browser will be available in the path you specify.

Remember that an instance of Elevator must be running and listening for a client connection in order for this command to complete.

It's highly recommended that you aggregate multiple iterations of a test run in order to get a final number, because these results have variability. Therefore, this example will also run 10 iterations. This results in each browser running through the full set of scenarios 10 times, and each run having its own associated trace. Each iteration will show up as its own row (or set of rows, if the measure set has multiple measures) in the final CSV file.

```BrowserEfficiencyTest.exe -browser edge chrome firefox -scenario wikipedia youtube facebook -tracecontrolled C:\Some\Path\To\Store\Traces -measureset cpuUsage -iterations 10```

or

```BrowserEfficiencyTest.exe -b edge chrome firefox -s wikipedia youtube facebook -tc C:\Some\Path\To\Store\Traces -ms cpuUsage -i 10```
