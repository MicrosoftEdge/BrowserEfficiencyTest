# Extending BrowserEfficiencyTest

## Adding scenarios

Scenarios define the automation that will be executed by BrowserEfficiencyTest. If you wish to test new websites or actions, you can define a new scenario. To do so, you create a class that extends `Scenario.cs`. You must specify a few things in this class.

First, in the constructor, specify a name and optionally a constructor. This is the name you'll use when calling the scenario from the command line. You can also provide a duration in seconds. If you don't, it will default to 40s.

```
public ExampleScenario()
{
    this.Name = "example";
    this.Duration = 60; // optional. Defaults to 40s
}
```

Next, add in the actions you want your scenario to take, inside the function `Run`.  In this function, several things will be provided for you:

* `driver` The webdriver, which you can use to control the browser
* `browser` A string that you can use to query which browser is being automated. Normally, this shouldn't be used, as the same automation is meant to be executed on each browser, but some browser-specific bugs or behaviors may make this necessary.
* `logins` A list of usernames and passwords provided by the user in the config file. If you need to use a login in your automation, you can find it in here. This should be used instead of hardcoding login credentials in scenarios.

The first thing you're likely to do is navigate to a URL. Let's look at that as a simple example that shows this:

```
public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
{
    driver.Navigate().GoToUrl("http://www.google.com");
}
```

Of course, you can do much more with `driver`. Use the documentation for Selenium Webdriver (with C# bindings), and the examples in this project for more details.

Finally, to make the scenario accessible from the command line, make sure you add the scenario to `Aruguments.cs` under `CreatePossibleScenarios()`.

## Adding measures

In its current form, you can use BrowserEfficiencyTest to measure power consumption, CPU, memory reference set, disk, and networking throughput. New measures can be added by extending `MeasureSet.cs`. In your new measure set, you must specify a trace profile (.wprp) to use, a WPA profile (.wpaprofile) to use, and define the logic to convert the CSVs outputted from the WPA profile to your final measures, which is speficied in `CalculateMetrics(Dictionary<string, List<string>> csvData)`. The trace profile will be defined in Elevator, and so adding a new trace profile will also require changes to Elevator.

This is an expert workflow, and requires a working knowledge of the Windows Performance Toolkit.