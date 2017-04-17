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
    internal class YelpSeattleDinner : Scenario
    {
        public YelpSeattleDinner()
        {
            Name = "YelpSeattleDinner";
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Nagivate to Yelp (Seattle)
            driver.NavigateToUrl("http://yelp.com/seattle");
            driver.Wait(5);

            // Search for dinner
            driver.TypeIntoField(driver.FindElementById("find_desc"), "Dueminuti Healthy Pasta" + Keys.Enter);
            driver.Wait(3);

            // Click into result
            driver.ClickElement(driver.FindElementByClassName("regular-search-result").FindElement(By.ClassName("biz-name")));
            driver.Wait(3);

            driver.ScrollPage(4);
        }
    }
}
