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
using System.Threading;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class RedditSearchSubreddit : Scenario
    {
        public RedditSearchSubreddit()
        {
            Name = "reddit";
        }
        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("https://www.reddit.com/");
            Thread.Sleep(2000);

            const string SearchTerm = "marvel contest";
            var search = driver.FindElementByName("q");

            search.SendKeys(SearchTerm);
            search.SendKeys(Keys.Tab);
            search.SendKeys(Keys.Enter);

            while (!driver.Title.Contains(SearchTerm))
            {
                Thread.Sleep(1000);
            }

            Thread.Sleep(1000);
            var subreddit = driver.FindElementByClassName("search-title");
            driver.Navigate().GoToUrl("https://www.reddit.com/r/ContestOfChampions/?ref=search_subreddits");
            Thread.Sleep(5000);

            driver.ScrollPage(3);

            var subredditThread = driver.FindElementByClassName("thumbnail");

            Thread.Sleep(5000);
            driver.Navigate().GoToUrl("https://www.reddit.com/r/ContestOfChampions/comments/4luknw/rank_upteam_buildingawakening_post/");
            Thread.Sleep(10000);

            driver.ScrollPage(15);
        }
    }
}
