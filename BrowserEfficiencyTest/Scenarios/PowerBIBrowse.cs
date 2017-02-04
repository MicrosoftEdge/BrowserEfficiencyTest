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
    internal class PowerBiBrowse : Scenario
    {
        public PowerBiBrowse()
        {
            Name = "PowerBiBrowse";
            DefaultDuration = 70;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Get the relevant username and password
            UserInfo credentials = credentialManager.GetCredentials("powerbi.com");

            // Navigate and log in
            driver.Navigate().GoToUrl("http://app.powerbi.com");
            driver.Wait(5);

            driver.ClickElement(driver.FindElement(By.XPath("//*[@data-event-property='signin']")));
            driver.Wait(5);

            driver.TypeIntoField(driver.FindElementById("cred_userid_inputtext"), credentials.Username);
            driver.Wait(1);

            driver.TypeIntoField(driver.FindElementById("cred_password_inputtext"), credentials.Password);
            driver.Wait(1);
            driver.Keyboard.SendKeys(Keys.Enter);
            driver.Wait(5);

            // Click into Gross Margin %
            driver.ClickElement(driver.FindElement(By.XPath("//*[@data-id='2423882']")).FindElement(By.ClassName("inFocusTileBtn")));
            driver.Wait(10);

            // Back to dashboard
            driver.ClickElement(driver.FindElement(By.XPath("//*[contains(text(), 'Exit Focus mode')]")).FindElement(By.XPath("..")));
            driver.Wait(3);

            // Click into Total Revenue
            driver.ClickElement(driver.FindElement(By.XPath("//*[@data-id='2423887']")).FindElement(By.ClassName("inFocusTileBtn")));
            driver.Wait(10);

            // Back to dashboard
            driver.ClickElement(driver.FindElement(By.XPath("//*[contains(text(), 'Exit Focus mode')]")).FindElement(By.XPath("..")));
        }
    }
}
