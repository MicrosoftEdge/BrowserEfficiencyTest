using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class AmazonSearch : Scenario
    {
        public AmazonSearch()
        {
            Name = "amazon";
            // Keep default time
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Go to Amazon
            driver.Navigate().GoToUrl("http://www.amazon.com");

            // Give it more than enough time to load
            Thread.Sleep(5000);

            // Type "Game of Thrones" in the search box and hit enter
            var searchbox = driver.FindElementById("twotabsearchtextbox");
            foreach (char c in "Game of Thrones")
            {
                searchbox.SendKeys(c.ToString());
            }
            searchbox.SendKeys(Keys.Enter);

            // Give the results lots of time to load
            Thread.Sleep(6 * 1000);

            // Click into "Game of Thrones Season 1"
            var bookLink = driver.FindElementByXPath("//*[@title='Game of Thrones Season 1']");
            bookLink.SendKeys(String.Empty);
            driver.Keyboard.SendKeys(Keys.Enter);

            // And let that load
            Thread.Sleep(2 * 1000);

            // Scroll down to reviews
            Program.scrollPage(5);
        }
    }
}
