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

using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace BrowserEfficiencyTest
{
    internal class YoutubeWatchVideo : Scenario
    {
        public YoutubeWatchVideo()
        {
            this.Name = "youtube";
            // Leave the default time
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Browse to Youtube if we're not there already
            if (!driver.Url.Contains("youtube.com"))
            {
                driver.Navigate().GoToUrl("https://www.youtube.com/watch?v=l42U5Cwn1Y0");
                Thread.Sleep(2000);
            }

            string movieId = "movie_player";
            var player = driver.FindElementById(movieId);

            // Play if it's paused
            if (player.GetAttribute("class").Contains("paused-mode"))
            {
                player.Click();
            }

            Thread.Sleep(2000);
        }
    }
}
