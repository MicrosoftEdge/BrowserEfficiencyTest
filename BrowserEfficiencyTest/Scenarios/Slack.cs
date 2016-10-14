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
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace BrowserEfficiencyTest
{
    // Sample command line params:
    // -browser chrome -s slack -tc c:\etls -ms energy -i 5
    internal class Slack : Scenario
    {
        public Slack()
        {
            Name = "slack";
            Duration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            if (browser != "hwa")
            {
                string userName = string.Empty, passWord = string.Empty;

                driver.Navigate().GoToUrl("http://mswebeco.slack.com/");
                Thread.Sleep(5000);

                try
                {
                    driver.FindElementById("direct_messages_header").Click();
                }
                catch (Exception)
                {
                    // login to slack
                    foreach (var item in logins)
                    {
                        if (item.Domain == "slack.com")
                        {
                            userName = item.UserName;
                            passWord = item.PassWord;
                            break;
                        }
                    }

                    var userElem = driver.FindElement(By.Id("email"));
                    var passElem = driver.FindElement(By.Id("password"));

                    userElem.Clear();
                    userElem.SendKeys(userName);
                    Thread.Sleep(500);

                    passElem.Clear();
                    passElem.SendKeys(passWord);
                    Thread.Sleep(500);

                    //// hack around bug in WebDriver:
                    //driver.FindElement(By.Id("signin_btn")).Click();

                    passElem.SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                }
            }
            Thread.Sleep(10000);
            for (var i = 0; i < 10; i++)
            {
                // simulate some scrolling. every other time we will
                // go down
                // but we will also go up (so, sometimes up 2x, sometimes down then up)
                var scrollElem = driver.FindElementById("message-input");
                scrollElem.SendKeys(i%2 == 0 ? Keys.PageUp : Keys.PageDown);

                Thread.Sleep(1000);
                scrollElem.SendKeys(Keys.PageUp);

                Thread.Sleep(1000);
            }
        }   
    }
}
