# Usage

```
Usage: BrowserEfficiencyTest.exe -browser|-b [chrome|edge|firefox|opera|operabeta] -scenario|-s all|<scenario1> <scenario2> [-iterations|-i <iterationcount>] [-tracecontrolled|-tc <etlpath>] [-measureset|-ms <measureset1> <measureset2>] [-warmup|-w] [-profile|-p <chrome profile path>] [-attempts|-a <attempts to make per iteration>]

 -browser|-b           Selects the browser or browsers to run the scenarios with. For multiple browsers, separate each browser with a space.
 -scenario|-s 	       Selects the scenario or scenarios to run. Multiple scenarios can be selected by separating each scenario with a space.
 -iterations|-i        Runs the specified scenarios n times with each iteration being a unique run.
 -tracecontrolled|-tc  Runs the specified scenarios while running a WPR trace session. Requires using the -measureset option and running with ElevatorServer.exe.
                       The path specified is where the ETL files and processed data files will be saved to. If the path does not exist, it will be created.
 -measureset|-ms       Selects the measure sets to use for the trace controlled run. Requires using the -tracecontrolled option. Each measure set selected will be run as a separate pass per browser per iteration.	 
 -warmup|-w            For use with tracecontrolled runs. Runs the specified scenarios once before running the main set of iterations with the trace tracecontroller (ElevatorServer.exe).
 -profile|-p           Enables the Chrome driver to use the passed in profile. Requires passing in the path to the Chrome profile.
 -attempts|-a          Defines the number of attempts to make per iteration. If an exception is caught, the trace is discarded and a new attempt is made. Default 3
```

> **WARNING**
> When run on Microsoft Edge, this will delete all browser data, including bookmarks, saved passwords, and form fill.

If you use `-tracecontrolled` or `-tc`, you'll also need to have an instance of [Elevator](https://github.com/MicrosoftEdge/Elevator) running to do the tracing for you.

#Examples
Run the CNN scenario on Microsoft Edge

```BrowserEfficiencyTest.exe -browser edge -scenario cnn```

 or

 ```BrowserEfficiencyTest.exe -b edge -s cnn```

Run the Wikipedia and YouTube scenarios on Microsoft Edge, Chrome, and Firefox

```BrowserEfficiencyTest.exe -b edge chrome firefox -scenario wikipedia youtube```

Run the Wikipedia and YouTube scenarios on Microsoft Edge and Firefox ten times.

```BrowserEfficiencyTest.exe -b edge firefox -scenario wikipedia youtube -i 10```

Run the MSN, Gmail and Google scenarios on Chrome, Firefox and Opera five times with the tracing controller to gather energy data and store the ETL and processed data files in C:\powerRun\Test1.

```BrowserEfficiencyTest.exe -b chrome firefox opera -scenario msn gmail google -i 5 -tc C:\powerRun\Test1 -ms energy```

Run the TechRadar and YouTube scenarios on Microsoft Edge and Firefox ten times with the tracing controller to gather energy data and run a warmup pass. Store the ETL and processed data files in C:\powerRun\Test1.

```BrowserEfficiencyTest.exe -b edge firefox -scenario techRadar youtube -i 10 -tc C:\powerRun\Test1 -ms energy -w```

Run the Gmail and Amazon scenarios on Chrome using the Chrome profile located at C:\ChromeUserData.

```BrowserEfficiencyTest.exe -b chrome -scenario gmail amazon -p C:\ChromeUserData```
