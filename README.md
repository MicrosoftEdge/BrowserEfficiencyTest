# Welcome!
PowerTest is a [Selenium WebDriver](http://docs.seleniumhq.org/) based web browser test automation project written in C#.

## Links
* [Selenium WebDriver](http://docs.seleniumhq.org/)
* [Selenium WebDriver Downloads](http://docs.seleniumhq.org/download/)
* [Microsoft Edge WebDriver Developer](https://developer.microsoft.com/en-us/microsoft-edge/platform/documentation/dev-guide/tools/webdriver/)
* [Microsoft WebDriver Download](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/)

## Coding Style
This project follows the [dotnet/corefx C# Coding Style](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md).

## Code of Conduct
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Notes
Using the XPerf and WPR modules requires the [Windows Performance Toolkit.](https://msdn.microsoft.com/en-us/library/windows/hardware/dn927310(v=vs.85).aspx)

## Usage

```
Usage: TestingPower.exe -browser|-b [chrome|edge|firefox|opera|operabeta] -scenario|-s all|<scenario1> <scenario2> [-loops <loopcount>] [-iterations|-i <iterationcount>] [-tracecontrolled|-tc <etlpath>] [-warmup|-w] [-profile|-p <chrome profile path>]

 -browser|-b           Selects the browser or browsers to run the scenarios with. For multiple browsers, separate each browser with a space.
 -scenario|-s 	       Selects the scenario or scenarios to run. Multiple scenarios can be selected by separating each scenario with a space.
 -loops                Causes the test to run the specified scenario n times as specified in the loop but with each loop run in a new tab building on the previous loop.
 -iterations|-i        Runs the specified scenarios n times with each iteration being a unique run.
 -tracecontrolled|-tc  Runs the specified scenarios while running a WPR trace session. Requires running with ElevatorServer.exe.
                       The path specified is where the ETL files and processed data files will be saved to. If the path does not exist, it will be created.
 -warmup|-w            For use with tracecontrolled runs. Runs the specified scenarios once before running the main set of iterations with the trace tracecontroller (ElevatorServer.exe).
 -profile|-p           Enables the Chrome driver to use the passed in profile. Requires passing in the path to the Chrome profile.
```

> **WARNING**
> When run on Microsoft Edge, this will delete all browser data, including bookmarks, saved passwords, and form fill.

#Examples
Run the CNN scenario on Edge

```testingpower.exe -browser edge -scenario cnn```

 or

 ```testingpower.exe -b edge -s cnn```

Run the Wikipedia and YouTube scenarios on Edge, Chrome, and Firefox

```testingpower.exe -b edge chrome firefox -scenario wikipedia youtube```

Run the Wikipedia and YouTube scenarios on Edge and Firefox ten times.

```testingpower.exe -b edge firefox -scenario wikipedia youtube -i 10```

Run the MSN, Gmail and Google scenarios on Chrome, Firefox and Opera five times with the tracing controller and store the ETL and processed data files in C:\powerRun\Test1.

```testingpower.exe -b chrome firefox opera -scenario msn gmail google -i 5 -tc C:\powerRun\Test1```

Run the TechRadar and YouTube scenarios on Edge and Firefox ten times with the tracing controller and run a warmup pass. Store the ETL and processed data files in C:\powerRun\Test1.

```testingpower.exe -b edge firefox -scenario techRadar youtube -i 10 -tc C:\powerRun\Test1 -w```

Run the Gmail and Amazon scenarios on Chrome using the Chrome profile located at C:\ChromeUserData.

```testingpower.exe -b chrome -scenario gmail amazon -p C:\ChromeUserData```
