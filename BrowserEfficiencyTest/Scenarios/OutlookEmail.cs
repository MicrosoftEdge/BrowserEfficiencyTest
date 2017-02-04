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

namespace BrowserEfficiencyTest
{
    internal class OutlookEmail : Scenario
    {
        public OutlookEmail()
        {
            Name = "OutlookEmail";
            DefaultDuration = 100;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            UserInfo credentials = credentialManager.GetCredentials("outlook.com");

            // Navigate
            driver.Navigate().GoToUrl("https://outlook.live.com/owa/?nlp=1");
            driver.Wait(5);

            // Log in
            driver.TypeIntoField(driver.FindElementById("i0116"), credentials.Username + Keys.Enter);
            driver.Wait(1);

            driver.TypeIntoField(driver.FindElementById("i0118"), credentials.Password + Keys.Enter);
            driver.Wait(5);
            
            // Cycle through some emails, simply with the down arrow key
            for (int i = 0; i < 5; i++)
            {
                driver.Keyboard.SendKeys(Keys.Down);
                driver.Wait(5);
            }

            // Compose a new email and send to a test account
            driver.ClickElement(driver.FindElement(By.XPath("//*[@title='Write a new message (N)']")));
            driver.Wait(3);

            driver.TypeIntoField("echopoweracct@gmail.com" + Keys.Tab);
            driver.Wait(3);

            driver.TypeIntoField(driver.FindElement(By.XPath("//*[@aria-label='Subject,']")), "Subject" + Keys.Tab);
            driver.Wait(3);

            driver.TypeIntoField(driver.FindElement(By.XPath("//*[@aria-label='Message body']")), "This is a message.");
            driver.Wait(1);

            // Send the message with ctrl + Enter shortcut
            driver.Keyboard.PressKey(Keys.Control);
            driver.Keyboard.SendKeys(Keys.Enter);
            driver.Keyboard.ReleaseKey(Keys.Control);
            driver.Wait(5);

            // Arrow back up to the top
            for (int i = 0; i < 5; i++)
            {
                driver.Keyboard.SendKeys(Keys.Up);
                driver.Wait(2);
            }
        }
    }
}
