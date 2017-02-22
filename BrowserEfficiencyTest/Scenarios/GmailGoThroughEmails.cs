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
            DefaultDuration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            NavigateToGmail(driver);
            driver.Wait(2);
            LogIn(driver, credentialManager, timer);
            BrowseEmails(driver, 5);
        }

        private void NavigateToGmail(RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://accounts.google.com/ServiceLogin?service=mail&continue=https://mail.google.com/mail/#identifier");
            driver.WaitForPageLoad();
        }

        private void LogIn(RemoteWebDriver driver, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Get the relevant username and password
            UserInfo credentials = credentialManager.GetCredentials("gmail.com");

            try
            {
                // Enter username
                driver.TypeIntoField(driver.FindElementById("Email"), credentials.Username);
                driver.Wait(1);

                // Tab down and hit next button
                driver.Keyboard.SendKeys(Keys.Tab);
                driver.Keyboard.SendKeys(Keys.Enter);

                driver.Wait(1);
            }
            catch (ElementNotVisibleException)
            {
                // If using profiles, the Email element will not be found and user will be able to enter the password
            }

            // Enter password
            driver.TypeIntoField(driver.FindElementById("Passwd"), credentials.Password);

            // Tab down and hit submit button
            driver.Keyboard.SendKeys(Keys.Tab);
            driver.Wait(1);

            // Start measuring how long it takes the when we hit enter after entering the password
            timer.StartMeasureOnEnterKeyPressed("Inbox visible");
            driver.Wait(1);
            driver.Keyboard.SendKeys(Keys.Enter);
            driver.Wait(1);
            timer.ExtractMeasures();

            driver.Wait(7);
            timer.EndMeasureOnDomContentLoaded("Inbox visible");
            driver.Wait(1);
            timer.ExtractMeasures();

            // Check the url to make sure login was successful
            if(driver.Url != @"https://mail.google.com/mail/u/0/#inbox" && driver.Url != @"https://mail.google.com/mail/#inbox")
            {
                throw new Exception("Login to Gmail failed!");
            }

        }

        private void BrowseEmails(RemoteWebDriver driver, int numOfEmailsToBrowse)
        {
            // Go through some emails
            for (int i = 0; i < numOfEmailsToBrowse; i++)
            {
                // Simply using the shortcut keys worked pretty well.
                // Note that they have to be enabled in the account you're using (gmail settings)
                driver.Keyboard.SendKeys("o");
                driver.Wait(4);

                // Go back to inbox
                driver.Keyboard.SendKeys("u");
                driver.Wait(2);
                
                // Select next email with the "cursor". Do this with the j key in gmail
                driver.Keyboard.SendKeys("j");
                driver.Wait(2);
            }
        }
    }
}
