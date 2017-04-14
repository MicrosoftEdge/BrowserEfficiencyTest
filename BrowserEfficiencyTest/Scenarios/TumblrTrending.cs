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
    internal class TumblrTrending : Scenario
    {
        public TumblrTrending()
        {
            Name = "TumblrTrending";
            DefaultDuration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Nagivate to the homepage for Tumblr
            driver.NavigateToUrl("https://www.tumblr.com/explore/trending");
            driver.Wait(10);

            // Try changing content with content controls
            //     At the time of writing, this is interesting because it doesn't require an actual page load

            driver.ClickElement(driver.FindElementByClassName("l-header-container").FindElement(By.XPath("//*[@data-text='Staff picks']")));
            driver.Wait(10);
            driver.ClickElement(driver.FindElementByClassName("l-header-container").FindElement(By.XPath("//*[@data-text='Trending']")));
            driver.Wait(5);

            // Scroll through the infinite list
            driver.ScrollPage(10);
        }
    }
}
