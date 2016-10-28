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
    class PowerBIBrowse : Scenario
    {
        public PowerBIBrowse()
        {
            Name = "powerBi";
            Duration = 70;
        }

        public override void Run(RemoteWebDriver d, string browser, List<UserInfo> logins)
        {
            // Get the relevant username and password
            string username = "";
            string password = "";
            foreach (UserInfo item in logins)
            {
                if (item.Domain == "powerbi.com")
                {
                    username = item.UserName;
                    password = item.PassWord;
                }
            }

            // Navigate and log in
            d.Navigate().GoToUrl("http://app.powerbi.com");
            d.Wait(5);

            d.ClickElement(d.FindElement(By.XPath("//*[@data-event-property='signin']")));
            d.Wait(5);

            d.TypeIntoField(d.FindElementById("cred_userid_inputtext"), username);
            d.Wait(1);

            d.TypeIntoField(d.FindElementById("cred_password_inputtext"), password);
            d.Wait(1);
            d.Keyboard.SendKeys(Keys.Enter);
            d.Wait(5);

            // Click into Gross Margin %
            d.ClickElement(d.FindElement(By.XPath("//*[@data-id='2423882']")).FindElement(By.ClassName("inFocusTileBtn")));
            d.Wait(10);

            // Back to dashboard
            d.ClickElement(d.FindElement(By.XPath("//*[contains(text(), 'Exit Focus mode')]")).FindElement(By.XPath("..")));
            d.Wait(3);

            // Click into Total Revenue
            d.ClickElement(d.FindElement(By.XPath("//*[@data-id='2423887']")).FindElement(By.ClassName("inFocusTileBtn")));
            d.Wait(10);

            // Back to dashboard
            d.ClickElement(d.FindElement(By.XPath("//*[contains(text(), 'Exit Focus mode')]")).FindElement(By.XPath("..")));
        }
    }
}
