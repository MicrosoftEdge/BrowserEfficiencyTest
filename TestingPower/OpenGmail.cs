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
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class OpenGmail : Scenario
    {
        public OpenGmail()
        {
            Name = "gmail";
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.gmail.com");

            /////////////
            // Log in //
            ///////////

            // Get the relevant username and password
            String username = "";
            String password = "";
            foreach (UserInfo item in logins)
            {
                if (item.Domain == "gmail.com")
                {
                    username = item.UserName;
                    password = item.PassWord;
                }
            }

            // Log in with them
            IWebElement userElement = null;
            try
            {
                userElement = driver.FindElementById("Email");
            }
            catch (Exception e)
            {
                var signInButton = driver.FindElementByXPath("//*[@data-g-label='Sign in']");
                signInButton.SendKeys(String.Empty);
                driver.Keyboard.SendKeys(Keys.Enter);
                Thread.Sleep(2 * 1000);
                userElement = driver.FindElementById("Email");
            }
            foreach (char c in username)
            {
                userElement.SendKeys(c.ToString());
                Thread.Sleep(75);
            }

            Thread.Sleep(1000);

            // Tab down and hit next button
            driver.Keyboard.SendKeys(Keys.Tab);
            driver.Keyboard.SendKeys(Keys.Enter);

            Thread.Sleep(1000);

            // Enter password
            var passwordElement = driver.FindElementById("Passwd");
            foreach (char c in password)
            {
                passwordElement.SendKeys(c.ToString());
                Thread.Sleep(75);
            }

            // Tab down and hit submit button
            driver.Keyboard.SendKeys(Keys.Tab);
            Thread.Sleep(2000);
            driver.Keyboard.SendKeys(Keys.Enter);

            Thread.Sleep(7 * 1000);

            // Go through some emails
            for (int i = 0; i < 5; i++)
            {
                // Go into email
                //driver.FindElementsByClassName("zA").ElementAt(i).SendKeys(String.Empty);
                driver.Keyboard.SendKeys("o");
                
                Thread.Sleep(4000);
                // Go back to inbox
                driver.Keyboard.SendKeys(Keys.Backspace);
                Thread.Sleep(2000);
                // Select next email with the "cursor". Do this with the j key in gmail
                driver.Keyboard.SendKeys("j");
                Thread.Sleep(2000);
            }
        }

    }
}
