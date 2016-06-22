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

﻿using System;
using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System.Threading;

namespace TestingPower
{
    class CnnTopStory : Scenario
    {
        public CnnTopStory()
        {
            Name = "cnn";

            // Using 80 as sometimes Chrome takes just over 70 seconds to run            
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.cnn.com");

            // Give it more than enough time to load
            Thread.Sleep(5000); 

            // Get the element that contains the headline story
            var headlineElement = driver.FindElementByClassName("js-screaming-banner");

            // Get focus on the headline story link
            headlineElement.SendKeys(String.Empty);

            // Open the link to the headline story
            headlineElement.SendKeys(Keys.Enter);

            // And let that load
            Thread.Sleep(2 * 1000);

            // Scroll down multiple times
            Program.scrollPage(10);
        }
    }
}
