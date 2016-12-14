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
    internal class YahooNews : Scenario
    {
        public YahooNews()
        {
            Name = "yahooNews";
            DefaultDuration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager)
        {
            driver.Navigate().GoToUrl("http://www.yahoo.com");
            driver.Wait(10);

            // No reliable class or id for the news link, so get the news icon, then find its parent
            IWebElement newsLink = driver.FindElementByClassName("IconNews").FindElement(By.XPath(".."));
            newsLink.SendKeys(String.Empty);
            newsLink.SendKeys(Keys.Enter);

            Thread.Sleep(10000);

            // Get the "mega" story and navigate to it
            // We appear to be taking advantage of a test hook in the page for their own tests
            IWebElement mega = driver.FindElement(By.XPath("//*[@data-test-locator='mega']"));
            IWebElement articleLink = mega.FindElement(By.TagName("h3")).FindElement(By.TagName("a"));
            driver.ClickElement(articleLink);
        }
    }
}