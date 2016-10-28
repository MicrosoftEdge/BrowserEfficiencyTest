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
    internal class AzureDashboard : Scenario
    {
        public AzureDashboard()
        {
            Name = "azure";
            Duration = 120;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager)
        {
            // Get the relevant username and password
            UserInfo credentials = credentialManager.GetCredentials("azure.com");

            // Go to the login
            driver.Navigate().GoToUrl("http://portal.azure.com");
            driver.Wait(3);

            // Log in
            driver.TypeIntoField(driver.FindElementById("cred_userid_inputtext"), credentials.Username);
            driver.TypeIntoField(driver.FindElementById("cred_password_inputtext"), credentials.Password + Keys.Enter);
            driver.Wait(8);

            // Open a blade
            var sidebar = driver.FindElementByClassName("fxs-sidebar-bar");
            driver.ClickElement(sidebar.FindElement(By.XPath("//*[@title='All resources']")));
            driver.Wait(10);

            // Open the marketplace
            driver.ClickElement(sidebar.FindElement(By.ClassName("fxs-sidebar-browse")));
            driver.Wait(2);

            driver.TypeIntoField(driver.FindElement(By.ClassName("fxs-sidebar-filter-input")), "marketplace" + Keys.Enter);
            driver.Wait(12);

            // Search for Visual studio
            driver.TypeIntoField(driver.FindElementByClassName("ext-gallery-search-container").FindElement(By.ClassName("azc-input")), "Visual studio" + Keys.Enter);
            driver.Wait(10);

            // Open more blades
            driver.ClickElement(sidebar.FindElement(By.XPath("//*[@title='Billing']")));
            driver.Wait(10);

            driver.ClickElement(sidebar.FindElement(By.XPath("//*[@title='Resource groups']")));
            driver.Wait(10);

            driver.ClickElement(sidebar.FindElement(By.XPath("//*[@title='App Services']")));
            driver.Wait(10);

            driver.ClickElement(sidebar.FindElement(By.XPath("//*[@title='SQL databases']")));
            driver.Wait(10);

            driver.ClickElement(sidebar.FindElement(By.XPath("//*[@title='Virtual machines']")));
            driver.Wait(10);
        }
    }
}
