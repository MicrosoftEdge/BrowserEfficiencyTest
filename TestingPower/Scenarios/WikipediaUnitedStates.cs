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
using OpenQA.Selenium.Remote;
using System.Threading;

namespace TestingPower
{
    internal class WikipediaUnitedStates : Scenario
    {
        public WikipediaUnitedStates()
        {
            // Specifify name and that it's 30s
            Name = "wikipedia";
            Duration = 30;
        }
        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Nagivate to wikipedia
            driver.Navigate().GoToUrl("https://en.wikipedia.org/wiki/United_States");

            Thread.Sleep(2 * 1000);
            if (browser == "firefox")
            {
                // With Firefox, we had to get focus onto the page, or else PgDn scrolled through the address bar
                driver.FindElementById("firstHeading").SendKeys(string.Empty);
            }

            // Scroll a bit
            Program.scrollPage(driver, 15);
        }
    }
}
