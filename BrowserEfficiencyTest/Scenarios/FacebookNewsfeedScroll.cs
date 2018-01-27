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

using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System;

namespace BrowserEfficiencyTest
{
    internal class FacebookNewsfeedScroll : Scenario
    {
        public FacebookNewsfeedScroll()
        {
            Name = "FacebookNewsfeedScroll";
            DefaultDuration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            IWebElement notificationWindow = null;
            IWebElement cancelButton = null;

            driver.NavigateToUrl("http://www.facebook.com");
            driver.Wait(5);

            UserInfo credentials = credentialManager.GetCredentials("facebook.com");

            if (driver.Title == "Log into Facebook | Facebook" || driver.Title == "Facebook - Log In or Sign Up")
            {
                Logger.LogWriteLine("    Starting logging into Facebook...");
                ScenarioEventSourceProvider.EventLog.AccountLogInStart("Facebook");

                // if not logged on, log on
                var elems = driver.FindElements(By.CssSelector("H2"));
                driver.Wait(2);

                var username = driver.FindElement(By.Id("email"));
                var password = driver.FindElement(By.Id("pass"));

                username.Clear();
                username.Clear();

                driver.TypeIntoField(username, credentials.Username);
                driver.Wait(1);

                driver.TypeIntoField(password, credentials.Password + Keys.Enter);
                driver.Wait(1);

                ScenarioEventSourceProvider.EventLog.AccountLogInStop("Facebook");
                Logger.LogWriteLine("    Completed logging into Facebook...");
            }
            else
            {
                Logger.LogWriteLine("    Already logged into Facebook...");
            }

            // Check to makes sure the login was successful
            if (driver.Title == "Log into Facebook | Facebook" || driver.Title == "Facebook - Log In or Sign Up")
            {
                throw new Exception("Login to Facebook failed!");
            }

            // It sometimes takes Facebook several seconds before it pops up a notification popup asking the user to enable or disable facebook notifications.
            // Using 20 seconds to be safe and account for having longer delays on slower devices.
            driver.Wait(20);

            // find the "Turn on Facebook Notifications" window if it exists.
            // This should only show up on the first login to Facebook on a new machine or if cookies have been cleared.
            try
            {
                Logger.LogWriteLine("    Check for Facebook notification request window.");
                notificationWindow = driver.FindElementById("notification-permission-title");

                Logger.LogWriteLine("    Facebook notification request window found! Attempting to click 'Not Now' button.");
                cancelButton = driver.FindElementByLinkText("Not Now");
                driver.ClickElement(cancelButton);
            }
            catch (NoSuchElementException)
            {
                // No facebook notification window found so continue on our merry way.
                if (notificationWindow == null)
                {
                    Logger.LogWriteLine("    No notification window found. Continuing on with scenario execution.");
                }
                else
                {
                    Logger.LogWriteLine("    Unable to click 'Not Now' button in the Facebook notification request window!");
                    throw;
                }
            }

            // Once we're logged in, all we're going to do is scroll through the page
            // We're simply measuring a user looking through their news feed for a minute
            driver.Wait(1);
            ScenarioEventSourceProvider.EventLog.ScenarioActionStart("Scroll through timeline");
            driver.ScrollPage(10);
            ScenarioEventSourceProvider.EventLog.ScenarioActionStop("Scroll through timeline");
        }
    }
}
