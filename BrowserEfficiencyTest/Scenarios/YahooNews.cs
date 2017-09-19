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

using System.Linq;

namespace BrowserEfficiencyTest
{
    internal class YahooNews : Scenario
    {
        public YahooNews()
        {
            Name = "YahooNews";
            DefaultDuration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            driver.NavigateToUrl("http://www.yahoo.com");
            driver.Wait(5);

            timer.ExtractPageLoadTime("Yahoo homepage");

            IWebElement newsLink;
            // Go to the News section
            // There are a number of different layouts currently.  Trying each one in order of observed probability.
            try
            {
                // This should cover most of the cases
                newsLink = driver.FindElement(By.LinkText("News"));
            }
            catch
            {
                try
                {
                    newsLink = driver.FindElement(By.XPath("//a[@href='https://www.yahoo.com/news/']"));
                }
                catch
                {
                    try
                    {
                        newsLink = driver.FindElement(By.XPath("//a[@href='https://news.yahoo.com/']"));
                    }
                    catch
                    {
                        // ok fine, let's try finding the news link element with a little more brute force
                        // First get all the anchor elements
                        var anchorElements = driver.FindElementsByXPath("//a");

                        // Filter down the elements to the specific one(s) we are looking for.
                        var newsLinkElements = from elem in anchorElements
                                                let hrefx = elem.GetAttribute("href")
                                                where hrefx.EndsWith("yahoo.com/news/") || hrefx.EndsWith("news.yahoo.com/")
                                                select elem;

                        // There should only be one or none.
                        newsLink = newsLinkElements.FirstOrDefault();
                    }
                }
            }

            driver.ClickElement(newsLink);

            driver.Wait(5);

            // Add a link to a known article
            // This hack is a little unfortunate, as no real user is injecting javascript to create a link, but
            // the overhead of doing this is minimal compared to normal code running on the page, and it allows
            // us to control exactly which article we navigate to, and is consistent for every run
            driver.ExecuteScript(@"
                var newA = document.createElement(""a"");
                newA.href = ""https://www.yahoo.com/tech/microsoft-surface-studio-hands-on-221123426.html"";
                newA.innerHTML = ""Go to a typical news story (This link added by automation)"";
                newA.setAttribute(""id"", ""bet_automationLink"");
                var stream = document.querySelector(""#YDC-Stream"");
                stream.insertBefore(newA, stream.firstChild);
                ");

            // Navigate to the article we added
            driver.Wait(5);
            driver.ClickElement(driver.FindElement(By.Id("bet_automationLink")));

            // Simulate reading it
            driver.Wait(6);
            // Give focus to some element on the page so that PG DN scrolls
            driver.FindElement(By.Id("app")).FindElement(By.TagName("a")).SendKeys("");

            driver.ScrollPage(2);
            driver.Wait(6);
            driver.ScrollPage(1);
            driver.Wait(6);

            timer.ExtractPageLoadTime("Yahoo article");

            // Then go back to the news homepage
            driver.NavigateBack();
        }
    }
}