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
using System.Threading;

namespace BrowserEfficiencyTest
{
    internal class FacebookNewsfeedScroll : Scenario
    {
        public FacebookNewsfeedScroll()
        {
            Name = "facebook";
            Duration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.facebook.com");
            driver.Wait(5);

            // if not logged on, log on
            var elems = driver.FindElements(By.CssSelector("H2"));
            foreach (IWebElement elem in elems)
            {
                string userName = string.Empty, passWord = string.Empty;

                foreach (var item in logins)
                {
                    if (item.Domain == "facebook.com")
                    {
                        userName = item.UserName;
                        passWord = item.PassWord;
                        break;
                    }
                }

                Thread.Sleep(2000);
                var username = driver.FindElement(By.Id("email"));
                var password = driver.FindElement(By.Id("pass"));

                username.Clear();
                driver.TypeIntoField(username, userName);
                driver.Wait(1);

                password.Clear();
                driver.Wait(3);
                driver.TypeIntoField(password, passWord);
                driver.Wait(1);

                // Avoding applying click to button because of ObscureElement bug in Microsfot Edge with high DPI
                // Instead use tab and enter. Seemed to be pretty reliable across browsers
                driver.Keyboard.SendKeys(Keys.Tab);
                driver.Keyboard.SendKeys(Keys.Enter);
                driver.Wait(2);
                break;
            }

            // Once we're logged in, all we're going to do is scroll through the page
            // We're simply measuring a user looking through their news feed for a minute

            driver.ScrollPage(20);
        }
    }
}
