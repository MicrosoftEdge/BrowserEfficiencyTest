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
using OpenQA.Selenium;
using System.Threading;

namespace TestingPower
{
    internal class CnnTopStory : Scenario
    {
        public CnnTopStory()
        {
            Name = "cnn";

            // Using 90s as sometimes Chrome takes just over 80 seconds to run
            Duration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            IWebElement headlineElement = null;

            driver.Navigate().GoToUrl("http://www.cnn.com");

            // Give it more than enough time to load
            Thread.Sleep(5000);

            // Get the element that contains the headline story
            try
            {
                // CNN defaults to using js-screaming-banner as its top headline
                headlineElement = driver.FindElementByClassName("js-screaming-banner");
            }
            catch(NoSuchElementException e)
            {
                // On big news events CNN changes to using zh-banner class for their headline
                IWebElement znBannerElement = driver.FindElementByClassName("zn-banner");
                headlineElement = znBannerElement.FindElement(By.TagName("a"));
            }

            // Get focus on the headline story link
            headlineElement.SendKeys(string.Empty);

            // Open the link to the headline story
            headlineElement.SendKeys(Keys.Enter);

            // And let that load
            Thread.Sleep(2 * 1000);

            // Scroll down multiple times
            Program.scrollPage(10);
        }
    }
}
