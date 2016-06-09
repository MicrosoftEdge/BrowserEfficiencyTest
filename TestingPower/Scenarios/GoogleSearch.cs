using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class GoogleSearch : Scenario
    {
        public GoogleSearch()
        {
            Name = "google";
            // Default time
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Go to Google
            driver.Navigate().GoToUrl("http://www.google.com");

            Thread.Sleep(5 * 1000);

            // Search for "Seattle" and hit enter
            var searchBox = driver.FindElementByXPath("//*[@title='Search']");
            foreach (char c in "Seattle")
            {
                searchBox.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            driver.Keyboard.SendKeys(Keys.Enter);

            // Simply yield control back to the main thread and look at results
        }
    }
}
