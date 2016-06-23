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

using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    internal class AmazonSearch : Scenario
    {
        public AmazonSearch()
        {
            Name = "amazon";
            // Keep default time
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.amazon.com");

            // Give it more than enough time to load
            Thread.Sleep(5000);

            // Type "Game of Thrones" in the search box and hit enter
            var searchbox = driver.FindElementById("twotabsearchtextbox");
            foreach (char c in "Game of Thrones")
            {
                searchbox.SendKeys(c.ToString());
            }
            searchbox.SendKeys(Keys.Enter);

            // Give the results lots of time to load
            Thread.Sleep(6 * 1000);

            // Click into "Game of Thrones Season 1"
            var bookLink = driver.FindElementByXPath("//*[@title='Game of Thrones Season 1']");
            bookLink.SendKeys(string.Empty);
            driver.Keyboard.SendKeys(Keys.Enter);

            // And let that load
            Thread.Sleep(2 * 1000);

            // Scroll down to reviews
            Program.scrollPage(5);
        }
    }
}
