using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;
using System.Threading;
using OpenQA.Selenium;

namespace TestingPower
{
    class ScenarioReddit : Scenario
    {
        public ScenarioReddit()
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
            // Edge ObscureElement bug
            // subreddit.Click();

            Program.scrollPage(3);

            var subredditThread = driver.FindElementByClassName("thumbnail");
            // Edge ObscureElement bug
            // subredditThread.Click();
            Thread.Sleep(5000);
            driver.Navigate().GoToUrl("https://www.reddit.com/r/ContestOfChampions/comments/4luknw/rank_upteam_buildingawakening_post/");
            Thread.Sleep(10000);

            Program.scrollPage(15);
        }
    }
}
