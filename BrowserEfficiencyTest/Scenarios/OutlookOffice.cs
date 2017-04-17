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
    internal class OutlookOffice : Scenario
    {
        public OutlookOffice()
        {
            Name = "OutlookOffice";
            DefaultDuration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            UserInfo credentials = credentialManager.GetCredentials("outlook.com");

            // Navigate
            driver.NavigateToUrl("https://outlook.live.com/owa/?nlp=1");
            driver.Wait(5);

            // Log in
            driver.TypeIntoField(driver.FindElementById("i0116"), credentials.Username + Keys.Enter);
            driver.Wait(1);

            driver.TypeIntoField(driver.FindElementById("i0118"), credentials.Password + Keys.Enter);
            driver.Wait(10);

            // Go to office
            driver.ClickElement(driver.FindElementByClassName("o365cs-nav-button"));
            driver.ClickElement(driver.FindElementById("O365_AppTile_ShellWordOnline"));
            driver.Wait(8);

            // That opens up a new tab, so we have to give Webdriver focus in the new tab
            driver.SwitchTab(driver.WindowHandles[driver.WindowHandles.Count - 1]);
            driver.Wait(1);

            // Open up a Word doc
            driver.ClickElement(driver.FindElementById("mruitem_0"));
            driver.Wait(6);

            // This next section is in an iframe, so we have to switch to the iframe to access content in it
            driver.SwitchTo().Frame(driver.FindElement(By.Id("sdx_ow_iframe")));

            // Edit the document
            driver.ClickElement(driver.FindElementById("flyoutWordViewerEdit-Medium20"));
            driver.Wait(1);
            driver.ClickElement(driver.FindElementById("btnFlyoutEditOnWeb-Menu32"));
            driver.Wait(6);
            driver.ScrollPage(2);
        }
    }
}
