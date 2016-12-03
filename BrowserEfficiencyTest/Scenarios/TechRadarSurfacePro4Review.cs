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
using OpenQA.Selenium;
using System.Threading;

namespace BrowserEfficiencyTest
{
    internal class TechRadarSurfacePro4Review : Scenario
    {
        public TechRadarSurfacePro4Review()
        {
            Name = "techRadar";
            DefaultDuration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager)
        {
            driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));

            // Occasionally TechRadar never completes loading or doesn't seem to report it has loaded. Here, we work around
            // this issue by catching a webdriver timeout exception and respond with sending the ESC key which
            // will stop the page from continuing to load
            try
            {
                // Navigate to the Surface Pro 4 review on TechRadar.
                driver.Navigate().GoToUrl("http://www.techradar.com/us/reviews/pc-mac/tablets/microsoft-surface-pro-4-1290285/review");
            }
            catch (WebDriverTimeoutException)
            {
                Thread.Sleep(3 * 1000);
                driver.Keyboard.SendKeys(Keys.Escape);
            }

            // Give it more than enough time to load
            Thread.Sleep(5 * 1000);

            // Scroll down multiple times
            driver.ScrollPage(10);
        }
    }
}
