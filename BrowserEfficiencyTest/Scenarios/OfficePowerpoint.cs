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
    class OfficePowerpoint : Scenario
    {
        public OfficePowerpoint()
        {
            Name = "powerpoint";
            Duration = 60;
        }

        public override void Run(RemoteWebDriver d, string browser, CredentialManager credentialManager)
        {
            UserInfo credentials = credentialManager.GetCredentials("office.com");

            // Navigate
            d.Navigate().GoToUrl("http://www.outlook.com");
            d.Wait(5);

            // Log in
            d.TypeIntoField(d.FindElementById("i0116"), credentials.Username + Keys.Enter);
            d.Wait(1);

            d.TypeIntoField(d.FindElementById("i0118"), credentials.Password + Keys.Enter);
            d.Wait(10);

            // Go to office
            d.ClickElement(d.FindElementByClassName("o365cs-nav-button"));
            d.ClickElement(d.FindElementById("O365_AppTile_ShellPowerPointOnline"));
            d.Wait(8);

            // That opens up a new tab, so we have to give Webdriver focus in the new tab
            d.SwitchTo().Window(d.WindowHandles[d.WindowHandles.Count - 1]);
            d.Wait(1);

            // Open up a  doc
            d.ClickElement(d.FindElementById("mruitem_0"));
            d.Wait(6);

            // This next section is in an iframe, so we have to switch to the iframe to access content in it
            d.SwitchTo().Frame(d.FindElement(By.Id("sdx_ow_iframe")));

            // Edit the document
            d.ClickElement(d.FindElementById("PptUpperToolbar.LeftButtonDock.FlyoutPptEdit-Medium20"));
            d.Wait(1);
            d.ClickElement(d.FindElementById("PptUpperToolbar.LeftButtonDock.FlyoutPptEdit.EditInWebApp-Menu32"));
            d.Wait(6);
            d.ScrollPage(2);
        }
    }
}
