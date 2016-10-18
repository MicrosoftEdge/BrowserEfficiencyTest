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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;
using System.Threading;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class AzureDashboard : Scenario
    {
        public AzureDashboard()
        {
            Name = "azure";
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Get the relevant username and password
            string username = "";
            string password = "";
            foreach (UserInfo item in logins)
            {
                if (item.Domain == "azure.com")
                {
                    username = item.UserName;
                    password = item.PassWord;
                }
            }

            // Go to the login
            driver.Navigate().GoToUrl("http://portal.azure.com");

            Thread.Sleep(3 * 1000);

            // Log in
            var usernameIn = driver.FindElementById("cred_userid_inputtext");
            var passwordIn = driver.FindElementById("cred_password_inputtext");
            foreach (char c in username)
            {
                usernameIn.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            foreach (char c in password)
            {
                passwordIn.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            passwordIn.SendKeys(Keys.Enter);

            Thread.Sleep(9 * 1000);

            // Open a blade
            var sidebar = driver.FindElementByClassName("fxs-sidebar-bar");
            var allResourcesLink = sidebar.FindElement(By.XPath("//*[@title='All resources']"));
            allResourcesLink.SendKeys(String.Empty);
            allResourcesLink.SendKeys(Keys.Enter);

            Thread.Sleep(5 * 1000);

            // Open the marketplace
            var browseButton = sidebar.FindElement(By.ClassName("fxs-sidebar-browse"));
            browseButton.SendKeys(String.Empty);
            browseButton.SendKeys(Keys.Enter);

            Thread.Sleep(2 * 1000);

            var filterInput = driver.FindElement(By.ClassName("fxs-sidebar-filter-input"));
            foreach (char c in "marketplace")
            {
                filterInput.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            filterInput.SendKeys(Keys.Enter);

            Thread.Sleep(12 * 1000);

            // Search for Visual studio
            var marketplaceSearchBox = driver.FindElementByClassName("ext-gallery-search-container").FindElement(By.ClassName("azc-input"));
            foreach (char c in "Visual studio")
            {
                marketplaceSearchBox.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            marketplaceSearchBox.SendKeys(Keys.Enter);

            Thread.Sleep(8 * 1000);

            // Open another blade
            var billingLink = sidebar.FindElement(By.XPath("//*[@title='Billing']"));
            billingLink.SendKeys(String.Empty);
            billingLink.SendKeys(Keys.Enter);
        }
    }
}
