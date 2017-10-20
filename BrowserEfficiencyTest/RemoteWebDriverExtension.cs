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

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Extension class for the RemoteWebDriver class.
    /// Additional WebDriver functionality can be added to this class and will extend the
    /// RemoteWebDriver class.
    /// </summary>
    public static class RemoteWebDriverExtension
    {
        private static int _port = -1;
        private static int _edgeWebDriverBuildNumber = 0;
        private static int _edgeBrowserBuildNumber = 0;
        private static string _hostName = "localhost";
        private static string _browser = "";

        /// <summary>
        /// Navigates to the url passed as a string
        /// A wrapper for RemoteWebDriver.Navigate().GoToUrl(...) method but includes tracing events and pageloading waits
        /// </summary>
        /// <param name="url">Url to navigate to in string form.</param>
        /// <param name="timeoutSec">Number of seconds to wait for the page to load before timing out.</param>
        public static void NavigateToUrl(this RemoteWebDriver remoteWebDriver, string url, int timeoutSec = 30)
        {
            ScenarioEventSourceProvider.EventLog.NavigateToUrl(url);
            remoteWebDriver.Navigate().GoToUrl(url);
            remoteWebDriver.WaitForPageLoad();
        }

        /// <summary>
        /// Navigates back one page.
        /// A wrapper for RemoteWebDriver.Navigate().Back() method but includes tracing events and pageloading waits
        /// </summary>
        /// <param name="remoteWebDriver"></param>
        /// <param name="timeoutSec">Number of seconds to wait for the page to load before timing out.</param>
        public static void NavigateBack(this RemoteWebDriver remoteWebDriver, int timeoutSec = 30)
        {
            ScenarioEventSourceProvider.EventLog.NavigateBack();
            remoteWebDriver.Navigate().Back();
            remoteWebDriver.WaitForPageLoad();
        }

        /// <summary>
        /// Creates a new tab in the browser.
        /// </summary>
        public static void CreateNewTab(this RemoteWebDriver remoteWebDriver)
        {
            int originalTabCount = remoteWebDriver.WindowHandles.Count;
            int endingTabCount = 0;

            ScenarioEventSourceProvider.EventLog.OpenNewTab(originalTabCount);

            if (IsNewTabCommandSupported(remoteWebDriver))
            {
                Logger.LogWriteLine(" New Tab: Attempting to create a new tab using the Edge newTab webdriver command.");
                CallEdgeNewTabCommand(remoteWebDriver).Wait();
            }
            else
            {
                // Use some JS. Note that this means you have to disable popup blocking in Microsoft Edge
                // You actually have to in Opera too, but that's provided in a flag below
                Logger.LogWriteLine(" New Tab: Attempting to create a new tab using the javascript method window.open()");
                remoteWebDriver.ExecuteScript("window.open();");
            }

            endingTabCount = remoteWebDriver.WindowHandles.Count;

            // sanity check to make sure we in fact did get a new tab opened.
            if (endingTabCount != (originalTabCount + 1))
            {
                throw new Exception(string.Format("New tab was not created as expected! Expected {0} tabs but found {1} tabs.", (originalTabCount + 1), endingTabCount));
            }

            // Go to that tab
            remoteWebDriver.SwitchTab(remoteWebDriver.WindowHandles[remoteWebDriver.WindowHandles.Count - 1]);

            // Give the browser more than enough time to open the tab and get to it so the next commands from the
            // scenario don't get lost
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Switches the browser tab to the tab referred to by tabHandle.
        /// </summary>
        /// <param name="tabHandle">The webdriver tabHandle of the desired tab to switch to.</param>
        public static void SwitchTab(this RemoteWebDriver remoteWebDriver, string tabHandle)
        {
            ScenarioEventSourceProvider.EventLog.SwitchTab(tabHandle);
            remoteWebDriver.SwitchTo().Window(tabHandle);
        }

        /// <summary>
        /// Closes the browser.
        /// </summary>
        public static void CloseBrowser(this RemoteWebDriver remoteWebDriver, string browser)
        {
            if (browser == "opera")
            {
                // Opera wouldn't close the window using .Quit() Instead, thespeed dial would remain open, which
                // would interfere with other tests. This key combination is used as a workaround.
                remoteWebDriver.FindElement(By.TagName("body")).SendKeys(Keys.Control + Keys.Shift + 'x');
            }
            else
            {
                remoteWebDriver.Quit();
            }
            ScenarioEventSourceProvider.EventLog.CloseBrowser(browser);
        }

        /// <summary>
        /// Scrolls down a web page using the page down key.
        /// </summary>
        /// <param name="timesToScroll">An abstract quantification of how much to scroll</param>
        public static void ScrollPage(this RemoteWebDriver remoteWebDriver, int timesToScroll)
        {
            // Webdriver examples had scrolling by executing Javascript. That approach seemed troublesome because the
            // browser is scrolling in a way very different from how it would with a real user, so we don't do it.
            // Page down seemed to be the best compromise in terms of it behaving like a real user scrolling, and it
            // working reliably across browsers.
            // Use the page down key.
            for (int i = 0; i < timesToScroll; i++)
            {
                ScenarioEventSourceProvider.EventLog.ScrollEvent();
                if (remoteWebDriver.ToString().ToLower().Contains("firefoxdriver"))
                {
                    // Send the commands to the body element for Firefox.
                    IWebElement body = remoteWebDriver.FindElementByTagName("body");
                    body.SendKeys(Keys.PageDown);
                }
                else
                {
                    remoteWebDriver.Keyboard.SendKeys(Keys.PageDown);
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Sends keystrokes to the browser. Not to a specific element.
        /// Wrapper for driver.Keyboard.SendKeys(...)
        /// </summary>
        /// <param name="keys">Keystrokes to send to the browser.</param>
        public static void SendKeys(this RemoteWebDriver remoteWebDriver, string keys)
        {
            ScenarioEventSourceProvider.EventLog.SendKeysStart(keys.Length);
            // Firefox driver does not currently support sending keystrokes to the browser.
            // So instead, get the body element and send the keystrokes to that element.
            if (remoteWebDriver.ToString().ToLower().Contains("firefoxdriver"))
            {
                IWebElement body = remoteWebDriver.FindElementByTagName("body");
                body.SendKeys(keys);
            }
            else
            {
                remoteWebDriver.Keyboard.SendKeys(keys);
            }
            ScenarioEventSourceProvider.EventLog.SendKeysStop(keys.Length);
        }

        /// <summary>
        /// Waits for the specified amount of time before executing the next command.
        /// </summary>
        /// <param name="secondsToWait">The number of seconds to wait</param>
        public static void Wait(this RemoteWebDriver remoteWebDriver, double secondsToWait)
        {
            ScenarioEventSourceProvider.EventLog.WaitStart(secondsToWait, "");
            Thread.Sleep((int)(secondsToWait * 1000));
            ScenarioEventSourceProvider.EventLog.WaitStop(secondsToWait, "");
        }

        /// <summary>
        /// Waits for the specified amount of time before executing the next command.
        /// </summary>
        /// <param name="secondsToWait">The number of seconds to wait</param>
        /// <param name="waitEventTag">String to be inserted with the wait start and stop trace event</param>
        public static void Wait(this RemoteWebDriver remoteWebDriver, double secondsToWait, string waitEventTag)
        {
            ScenarioEventSourceProvider.EventLog.WaitStart(secondsToWait, waitEventTag);
            Thread.Sleep((int)(secondsToWait * 1000));
            ScenarioEventSourceProvider.EventLog.WaitStop(secondsToWait, waitEventTag);
        }

        /// <summary>
        /// Types into the given WebElement the specified text
        /// </summary>
        /// <param name="element">The WebElement to type into</param>
        /// <param name="text">The text to type</param>
        public static void TypeIntoField(this RemoteWebDriver remoteWebdriver, IWebElement element, string text)
        {
            ScenarioEventSourceProvider.EventLog.TypeIntoFieldStart(text.Length);
            foreach (char c in text)
            {
                element.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            ScenarioEventSourceProvider.EventLog.TypeIntoFieldStop(text.Length);
        }

        /// <summary>
        /// Types the given text into whichever field has focus
        /// </summary>
        /// <param name="text">The text to type</param>
        public static void TypeIntoField(this RemoteWebDriver remoteWebDriver, string text)
        {
            ScenarioEventSourceProvider.EventLog.TypeIntoFieldStart(text.Length);
            foreach (char c in text)
            {
                remoteWebDriver.Keyboard.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            ScenarioEventSourceProvider.EventLog.TypeIntoFieldStop(text.Length);
        }

        /// <summary>
        /// Clicks on the given web element. Makes multiple attempts if necessary.
        /// </summary>
        /// <param name="element">The WebElement to click on</param>
        public static void ClickElement(this RemoteWebDriver remoteWebDriver, IWebElement element, int maxAttemptsToMake = 3)
        {
            int attempt = 0;
            bool isClickSuccessful = false;

            while (isClickSuccessful == false)
            {
                try
                {
                    ScenarioEventSourceProvider.EventLog.ClickElement(element.Text);
                    // Send the empty string to give focus, then enter. We do this instead of click() because
                    // click() has a bug on high DPI screen we're working around
                    element.SendKeys(string.Empty);
                    element.SendKeys(Keys.Enter);
                    isClickSuccessful = true;
                }
                catch (Exception)
                {
                    attempt++;

                    Logger.LogWriteLine("Failed attempt " + attempt + " to click element " + element.ToString());

                    Thread.Sleep(1000);

                    if (attempt >= maxAttemptsToMake)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates a RemoteWebDriver instance based on the browser passed to this method. Opens the browser and maximizes its window.
        /// </summary>
        /// <param name="browser">The browser to get instantiate the Web Driver for.</param>
        /// <param name="browserProfilePath">The folder path to the browser user profile to use with the browser.</param>
        /// <returns>The RemoteWebDriver of the browser passed in to the method.</returns>
        public static RemoteWebDriver CreateDriverAndMaximize(string browser, bool clearBrowserCache, bool enableVerboseLogging = false, string browserProfilePath = "", List<string> extensionPaths = null, int port = 17556, string hostName = "localhost")
        {
            // Create a webdriver for the respective browser, depending on what we're testing.
            RemoteWebDriver driver = null;
            _browser = browser.ToLowerInvariant();
            
            switch (browser)
            {
                case "opera":
                case "operabeta":
                    OperaOptions oOption = new OperaOptions();
                    oOption.AddArgument("--disable-popup-blocking");
                    oOption.AddArgument("--power-save-mode=on");
                    // TODO: This shouldn't be a hardcoded path, but Opera appeared to need this speficied directly to run well
                    oOption.BinaryLocation = @"C:\Program Files (x86)\Opera\launcher.exe";
                    if (browser == "operabeta")
                    {
                        // TODO: Ideally, this code would look inside the Opera beta folder for opera.exe
                        // rather than depending on flaky hard-coded version in directory
                        oOption.BinaryLocation = @"C:\Program Files (x86)\Opera beta\38.0.2220.25\opera.exe";
                    }
                    ScenarioEventSourceProvider.EventLog.LaunchWebDriver(browser);
                    driver = new OperaDriver(oOption);
                    break;
                case "firefox":
                    ScenarioEventSourceProvider.EventLog.LaunchWebDriver(browser);
                    driver = new FirefoxDriver();
                    break;
                case "chrome":
                    ChromeOptions option = new ChromeOptions();
                    option.AddUserProfilePreference("profile.default_content_setting_values.notifications", 1);
                    ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
                    if (enableVerboseLogging)
                    {
                        chromeDriverService.EnableVerboseLogging = true;
                    }

                    if (!string.IsNullOrEmpty(browserProfilePath))
                    {
                        option.AddArgument("--user-data-dir=" + browserProfilePath);
                    }

                    ScenarioEventSourceProvider.EventLog.LaunchWebDriver(browser);
                    driver = new ChromeDriver(chromeDriverService, option);
                    break;
                default:
                    EdgeOptions edgeOptions = new EdgeOptions();
                    edgeOptions.AddAdditionalCapability("browserName", "Microsoft Edge");

                    EdgeDriverService edgeDriverService = null;

                    if (extensionPaths != null && extensionPaths.Count != 0)
                    {
                        // Create the extensionPaths capability object
                        edgeOptions.AddAdditionalCapability("extensionPaths", extensionPaths);
                        foreach (var path in extensionPaths)
                        {
                            Logger.LogWriteLine("Sideloading extension(s) from " + path);
                        }
                    }

                    if (hostName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Using localhost so create a local EdgeDriverService and instantiate an EdgeDriver object with it.
                        // We have to use EdgeDriver here instead of RemoteWebDriver because we need a
                        // DriverServiceCommandExecutor object which EdgeDriver creates when instantiated

                        edgeDriverService = EdgeDriverService.CreateDefaultService();
                        if(enableVerboseLogging)
                        {
                            edgeDriverService.UseVerboseLogging = true;
                        }

                        _port = edgeDriverService.Port;
                        _hostName = hostName;

                        Logger.LogWriteLine(string.Format("  Instantiating EdgeDriver object for local execution - Host: {0}  Port: {1}", _hostName, _port));
                        ScenarioEventSourceProvider.EventLog.LaunchWebDriver(browser);
                        driver = new EdgeDriver(edgeDriverService, edgeOptions);
                    }
                    else
                    {
                        // Using a different host name.
                        // We will make the assumption that this host name is the host of a remote webdriver instance.
                        // We have to use RemoteWebDriver here since it is capable of being instantiated without automatically
                        // opening a local MicrosoftWebDriver.exe instance (EdgeDriver automatically launches a local process with
                        // MicrosoftWebDriver.exe).

                        _port = port;
                        _hostName = hostName;
                        var remoteUri = new Uri("http://" + _hostName + ":" + _port + "/");

                        Logger.LogWriteLine(string.Format("  Instantiating RemoteWebDriver object for remote execution - Host: {0}  Port: {1}", _hostName, _port));
                        ScenarioEventSourceProvider.EventLog.LaunchWebDriver(browser);
                        driver = new RemoteWebDriver(remoteUri, edgeOptions.ToCapabilities());
                    }

                    driver.Wait(2);

                    if (clearBrowserCache)
                    {
                        Logger.LogWriteLine("   Clearing Edge browser cache...");
                        ScenarioEventSourceProvider.EventLog.ClearEdgeBrowserCacheStart();
                        // Warning: this blows away all Microsoft Edge data, including bookmarks, cookies, passwords, etc
                        HttpClient client = new HttpClient();
                        client.DeleteAsync($"http://{_hostName}:{_port}/session/{driver.SessionId}/ms/history").Wait();
                        ScenarioEventSourceProvider.EventLog.ClearEdgeBrowserCacheStop();
                    }

                    _edgeBrowserBuildNumber = GetEdgeBuildNumber(driver);
                    Logger.LogWriteLine(string.Format("   Browser Version - MicrosoftEdge Build Version: {0}", _edgeBrowserBuildNumber));

                    string webDriverServerVersion = GetEdgeWebDriverVersion(driver);
                    Logger.LogWriteLine(string.Format("   WebDriver Server Version - MicrosoftWebDriver.exe File Version: {0}", webDriverServerVersion));
                    _edgeWebDriverBuildNumber = Convert.ToInt32(webDriverServerVersion.Split('.')[2]);

                    break;
            }

            ScenarioEventSourceProvider.EventLog.MaximizeBrowser(browser);
            driver.Manage().Window.Maximize();
            driver.Wait(1);
            return driver;
        }

        // Gets the Edge build version number.
        private static int GetEdgeBuildNumber(RemoteWebDriver remoteWebDriver)
        {
            int edgeBuildNumber = 0;

            string response = (string)remoteWebDriver.ExecuteScript("return navigator.userAgent;");
            string edgeVersionToken = response.Split(' ').ToList().FirstOrDefault(s => s.StartsWith("Edge"));

            if (string.IsNullOrEmpty(edgeVersionToken))
            {
                Logger.LogWriteLine("   Unable to extract Edge build version!");
            }
            else
            {
                var edgeBuildTokens = edgeVersionToken.Split('.');
                if (edgeBuildTokens.Length > 1)
                {
                    if (int.TryParse(edgeBuildTokens[1], out int returnedInt))
                    {
                        edgeBuildNumber = returnedInt;
                    }
                    else
                    {
                        Logger.LogWriteLine(string.Format("   Unable to extract Edge build version from {0}", edgeVersionToken));
                    }
                }
            }
            return edgeBuildNumber;
        }
        /// <summary>
        /// Waits up to timeoutSec for the page load to complete
        /// </summary>
        /// <param name="timeoutSec">Number of seconds to wait</param>
        public static void WaitForPageLoad(this RemoteWebDriver driver, int timeoutSec = 30)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeoutSec));
            wait.Until(wd => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            ScenarioEventSourceProvider.EventLog.PageReadyState();
        }

        /// <summary>
        /// For Edge only - Checks to see if the Edge browser and MicrosoftWebDriver.exe support the Edge specific 'NewTab' command.
        /// </summary>
        /// <param name="remoteWebDriver"></param>
        /// <returns>True if the current Edge browser and MicrosoftWebDriver support the new</returns>
        private static bool IsNewTabCommandSupported(this RemoteWebDriver remoteWebDriver)
        {
            bool isNewTabCommandSupported = false;

            if (_browser.Equals("edge"))
            {
                if (_edgeBrowserBuildNumber > 16203 && _edgeWebDriverBuildNumber > 16203)
                {
                    isNewTabCommandSupported = true;
                }
            }

            return isNewTabCommandSupported;
        }

        /// <summary>
        /// For Edge only - executes the webdriver tab command which opens a new tab without using the javascript window.Open() command
        /// </summary>
        /// <param name="remoteWebDriver"></param>
        /// <returns></returns>
        private static async Task CallEdgeNewTabCommand(this RemoteWebDriver remoteWebDriver)
        {
            string response = "";
            HttpContent content = new StringContent("");
            HttpResponseMessage newTabResponse = null;

            HttpClient client = new HttpClient();
            newTabResponse = await client.PostAsync($"http://{_hostName}:{_port}/session/{remoteWebDriver.SessionId}/ms/tab", content);
            response = await newTabResponse.Content.ReadAsStringAsync();

            if (response == "Unknown command received")
            {
                throw new Exception("New Tab command functionality is not implemented!!!");
            }
        }

        // Retrieves the WebDriver server version
        private static string GetEdgeWebDriverVersion(this RemoteWebDriver remoteWebDriver)
        {
            var statusResponse = Newtonsoft.Json.Linq.JObject.Parse(CallStatusCommand(remoteWebDriver).Result);
            string webDriverVersion = (string)statusResponse["value"]["build"]["version"];

            return webDriverVersion;
        }

        // Calls the WebDriver status command
        private static async Task<string> CallStatusCommand(this RemoteWebDriver remoteWebDriver)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage statusResponse = await client.GetAsync($"http://{_hostName}:{_port}/status");
            string response = await statusResponse.Content.ReadAsStringAsync();

            return response;
        }
    }
}
