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
    class OfficeLauncher : Scenario
    {
        public OfficeLauncher()
        {
            Name = "officeLauncher";
        }

        public override void Run(RemoteWebDriver d, string browser, List<UserInfo> logins)
        {
            string username = "";
            string password = "";

            foreach (var item in logins)
            {
                if (item.Domain == "office.com")
                {
                    username = item.UserName;
                    password = item.PassWord;
                    break;
                }
            }

            // Navigate
            d.Navigate().GoToUrl("http://www.office.com");
            d.Wait(5);

            // Click on "Sign In" button
            d.ClickElement(d.FindElementByLinkText("Sign in"));
            d.Wait(2);

            // Log in
            d.TypeIntoField(d.FindElementById("cred_userid_inputtext"), username + Keys.Tab);
            d.Wait(8);
            d.TypeIntoField(d.FindElementByName("passwd"), password + Keys.Enter);
            d.Wait(5);
        }
    }
}
