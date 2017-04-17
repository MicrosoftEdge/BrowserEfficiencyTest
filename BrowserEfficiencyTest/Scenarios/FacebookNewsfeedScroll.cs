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

using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System;

namespace BrowserEfficiencyTest
{
    internal class FacebookNewsfeedScroll : Scenario
    {
        public FacebookNewsfeedScroll()
        {
            Name = "FacebookNewsfeedScroll";
            DefaultDuration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            driver.NavigateToUrl("http://www.facebook.com");
            driver.Wait(5);

            UserInfo credentials = credentialManager.GetCredentials("facebook.com");

            // if not logged on, log on
            var elems = driver.FindElements(By.CssSelector("H2"));
            driver.Wait(2);

            var username = driver.FindElement(By.Id("email"));
            var password = driver.FindElement(By.Id("pass"));

            username.Clear();
            driver.TypeIntoField(username, credentials.Username);
            driver.Wait(1);

            password.Clear();
            driver.Wait(3);
            driver.TypeIntoField(password, credentials.Password + Keys.Enter);
            driver.Wait(1);

            // Check to makes sure the login was successful
            if(driver.Title == "Log into Facebook | Facebook")
            {
                throw new Exception("Login to Facebook failed!");
            }

            // Once we're logged in, all we're going to do is scroll through the page
            // We're simply measuring a user looking through their news feed for a minute
            driver.Wait(5);
            driver.ScrollPage(20);
        }
    }
}
