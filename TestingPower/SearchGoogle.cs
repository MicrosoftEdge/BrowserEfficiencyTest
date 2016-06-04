using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class SearchGoogle : Scenario
    {
        public SearchGoogle()
        {
            Name = "google";
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.google.com");

            Thread.Sleep(5 * 1000);

            var searchBox = driver.FindElementByXPath("//*[@title='Search']");
            foreach (char c in "Seattle")
            {
                searchBox.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            driver.Keyboard.SendKeys(Keys.Enter);
        }
    }
}
