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
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class RedditSurfaceSearch : Scenario
    {
        public RedditSurfaceSearch()
        {
            Name = "RedditSurfaceSearch";
            DefaultDuration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            driver.NavigateToUrl("https://www.reddit.com/");
            driver.Wait(2);

            ScenarioEventSourceProvider.EventLog.ScenarioActionStart("Execute search for 'Microsoft Surface'");
            // Find search box and type "Microsoft Surface" in it
            var redditSearchBoxElement = driver.FindElementById("search").FindElement(By.XPath("//input[@type='text']"));
            driver.TypeIntoField(redditSearchBoxElement, "Microsoft Surface");

            //Find the execute search button and click it to start the search
            var executeSearchButtonElement = driver.FindElementById("search").FindElement(By.XPath("//input[@type='submit']"));
            driver.ClickElement(executeSearchButtonElement);

            driver.WaitForPageLoad();
            ScenarioEventSourceProvider.EventLog.ScenarioActionStop("Execute search for 'Microsoft Surface'");

            driver.Wait(3);

            ScenarioEventSourceProvider.EventLog.ScenarioActionStart("Scroll Down");
            driver.ScrollPage(3);
            ScenarioEventSourceProvider.EventLog.ScenarioActionStop("Scroll Down");

            // Then go back to the news homepage
            driver.NavigateBack();

        }
    }
}
