//--------------------------------------------------------------
//
// Microsoft Edge Power Test
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

﻿using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace TestingPower
{
    class Msnbc : Scenario
    {
        public Msnbc ()
        {
            Name = "msnbc";
            Duration = 50;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.msnbc.com");
            // and scroll up / down
            Program.scrollPage(10);

            // click on one of the links on the page
            // first get back to the top
            driver.ExecuteScript("return window.scrollTo(0,0);");
            Thread.Sleep(2000);

        }

        private static void ClickMsnbcLink(RemoteWebDriver driver)
        {
            var links = driver.FindElements(By.CssSelector("a"));
            foreach (var link in links)
            {
                if (!link.GetAttribute("href").Contains("http://www.msnbc.com"))
                {
                    // Clicking link in Edge hits obscure bug
                    //link.Click();
                    break;
                }
            }
        }
    }
}
