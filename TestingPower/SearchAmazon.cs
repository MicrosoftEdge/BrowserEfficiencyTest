using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class SearchAmazon : Scenario
    {
        public SearchAmazon()
        {
            Name = "amazon";
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.amazon.com");

            Thread.Sleep(5000);

            var searchbox = driver.FindElementById("twotabsearchtextbox");
            foreach (char c in "Game of Thrones")
            {
                searchbox.SendKeys(c.ToString());
            }
            searchbox.SendKeys(Keys.Enter);

            Thread.Sleep(6 * 1000);

            var bookLink = driver.FindElementByXPath("//*[@title='Game of Thrones Season 1']");
            bookLink.SendKeys(String.Empty);
            driver.Keyboard.SendKeys(Keys.Enter);
            Thread.Sleep(2 * 1000);

            Program.scrollPage(5);
        }
    }
}
