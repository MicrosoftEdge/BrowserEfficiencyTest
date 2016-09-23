# Welcome!

BrowserEfficiencyTest is a [Selenium WebDriver](http://docs.seleniumhq.org/) based web browser test automation project written in C#. It allows you to run through common tasks done in browsers (look through a Facebook feed, go through some emails, browse the news) done in multiple tabs, and uses Windows Performance Recorder to measure how much power, CPU, or other resources were used in order to accomplish those tasks. It currently supports:
* Microsoft Edge
* Google Chrome
* Firefox
* Opera

## Documentation

* [Get started with BrowserEfficiencyTest](Documentation/GetStarted.md).
* [Using BrowserEfficiencyTest](Documentation/Usage.md)
* [Extending BrowserEfficiencyTest](Documentation/Extending.md)

## Links

* [Selenium WebDriver](http://docs.seleniumhq.org/)
* [Selenium WebDriver Downloads](http://docs.seleniumhq.org/download/)
* [Microsoft Edge WebDriver Developer](https://developer.microsoft.com/en-us/microsoft-edge/platform/documentation/dev-guide/tools/webdriver/)
* [Microsoft WebDriver Download](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/)

## Coding Style

This project follows the [dotnet/corefx C# Coding Style](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md).

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Dependencies

Using the XPerf and WPR modules requires the [Windows Performance Toolkit.](https://msdn.microsoft.com/en-us/library/windows/hardware/dn927310(v=vs.85).aspx)

This also requires [Elevator](https://github.com/MicrosoftEdge/Elevator) in order to trace (which must be done with admin rights).