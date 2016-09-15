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
    internal class CnnTopStory : Scenario
    {
        public CnnTopStory()
        {
            Name = "cnnTopStory";

            // Using 90s as sometimes Chrome takes just over 80 seconds to run
            Duration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            IWebElement headlineElement = null;

            driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));

            // Occasionally CNN never completes loading or doesn't seem to report it has loaded. Here, we work around
            // this issue by catching a webdriver timeout exception and respond with sending the ESC key which
            // will stop the page from continuing to load
            try
            {
                driver.Navigate().GoToUrl("http://www.cnn.com");
            }
            catch (WebDriverTimeoutException)
            {
                Thread.Sleep(3 * 1000);
                driver.Keyboard.SendKeys(Keys.Escape);
            }

            // Get the element that contains the headline story
            try
            {
                // CNN defaults to using js-screaming-banner as its top headline
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
                headlineElement = driver.FindElementByClassName("js-screaming-banner");
            }
            catch (NoSuchElementException)
            {
                // On big news events CNN changes to using zh-banner class for their headline
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
                IWebElement znBannerElement = driver.FindElementByClassName("zn-banner");
                headlineElement = znBannerElement.FindElement(By.TagName("a"));
            }

            // Get focus on the headline story link
            headlineElement.SendKeys(string.Empty);

            // Open the link to the headline story
            // Same as above case where CNN occasionally never returns after sending a key. Here we catch
            // this exception and send the ESC key if it occurs.
            try
            {
                headlineElement.SendKeys(Keys.Enter);
            }
            catch (WebDriverTimeoutException)
            {
                Thread.Sleep(3 * 1000);
                driver.Keyboard.SendKeys(Keys.Escape);
            }

            // And let that load
            Thread.Sleep(5 * 1000);

            // Scroll down multiple times
            driver.ScrollPage(10);
        }
    }
}
