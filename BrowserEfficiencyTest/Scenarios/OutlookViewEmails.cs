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
using OpenQA.Selenium.Remote;
using System.Threading;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class OutlookViewEmails : Scenario
    {
        public OutlookViewEmails()
        {
            Name = "outlook";
            Duration = 60;
        }
        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.outlook.com");
            try
            {
                // need to log on
                string userName = string.Empty, passWord = string.Empty;

                foreach (var item in logins)
                {
                    if (item.Domain == "outlook.com")
                    {
                        userName = item.UserName;
                        passWord = item.PassWord;
                        break;
                    }
                }

                var userElement = driver.FindElementByName("loginfmt");
                userElement.SendKeys(userName);
                Thread.Sleep(2000);

                var passwordElement = driver.FindElementByName("passwd");
                passwordElement.SendKeys(passWord);
                Thread.Sleep(2000);

                Console.WriteLine("tab to keep signed in");
                // Tab past "keep signed in"
                driver.Keyboard.SendKeys(Keys.Tab);
                Thread.Sleep(2000);

                // Tab to button
                Console.WriteLine("tab to button");
                driver.Keyboard.SendKeys(Keys.Tab);
                Thread.Sleep(2000);
                Console.WriteLine("click button");
                driver.Keyboard.SendKeys(Keys.Enter);
                Thread.Sleep(3000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // already logged on, do nothing
            }

            Thread.Sleep(2000);

            // Down arrow thorugh 10 emails
            for (int i = 0; i < 10; i++)
            {
                driver.Keyboard.SendKeys(Keys.ArrowDown);
                Thread.Sleep(2000);
            }
        }
    }
}
