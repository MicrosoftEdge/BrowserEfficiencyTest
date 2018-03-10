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
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class GmailGoThroughEmails : Scenario
    {
        public GmailGoThroughEmails()
        {
            Name = "GmailGoThroughEmails";
            DefaultDuration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            driver.NavigateToUrl("https://accounts.google.com/ServiceLogin?service=mail&continue=https://mail.google.com/mail/#identifier");
            driver.Wait(5);
            LogIn(driver, credentialManager);
            BrowseEmails(driver, 5);
        }

        private void LogIn(RemoteWebDriver driver, CredentialManager credentialManager)
        {
            string startingPageTitle = driver.Title;

            // If an account is not already logged in, the page title will be simply 'Gmail'
            if (startingPageTitle.Equals("Gmail", StringComparison.InvariantCultureIgnoreCase))
            {
                // Get the relevant username and password
                UserInfo credentials = credentialManager.GetCredentials("gmail.com");

                Logger.LogWriteLine("    Starting logging into Gmail...");

                ScenarioEventSourceProvider.EventLog.AccountLogInStart("Gmail");
                try
                {
                    // Enter username
                    driver.TypeIntoField(driver.FindElement(By.XPath("//input[@type='email']")), credentials.Username + Keys.Enter);
                    driver.Wait(1);
                }
                catch (ElementNotVisibleException) { }
                catch (InvalidOperationException)
                {
                    // If using profiles, the Email element will not be found and user will be able to enter the password
                }

                // Enter password
                driver.TypeIntoField(driver.FindElement(By.XPath("//input[@type='password']")), credentials.Password + Keys.Enter);

                // give the page some time to load
                driver.Wait(14);

                // Check the url to make sure login was successful
                if (driver.Url != @"https://mail.google.com/mail/u/0/#inbox" && driver.Url != @"https://mail.google.com/mail/#inbox")
                {
                    throw new Exception("Login to Gmail failed!");
                }

                ScenarioEventSourceProvider.EventLog.AccountLogInStop("Gmail");
                Logger.LogWriteLine("    Completed logging into Gmail...");
            }
            else
            {
                Logger.LogWriteLine("    Already logged into Gmail...");
            }
        }

        private void BrowseEmails(RemoteWebDriver driver, int numOfEmailsToBrowse)
        {
            ScenarioEventSourceProvider.EventLog.ScenarioActionStart("Browse Emails");
            // Go through some emails
            for (int i = 0; i < numOfEmailsToBrowse; i++)
            {
                // Simply using the shortcut keys worked pretty well.
                // Note that they have to be enabled in the account you're using (gmail settings)
                driver.SendKeys("o");
                driver.Wait(4);

                // Go back to inbox
                driver.SendKeys("u");
                driver.Wait(2);
                
                // Select next email with the "cursor". Do this with the j key in gmail
                driver.SendKeys("j");
                driver.Wait(2);
            }
            ScenarioEventSourceProvider.EventLog.ScenarioActionStop("Browse Emails");
        }
    }
}
