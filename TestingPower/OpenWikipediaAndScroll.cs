using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;
using System.Threading;

namespace TestingPower
{
    class OpenWikipediaAndScroll : Scenario
    {
        public OpenWikipediaAndScroll()
        {
            Name = "wikipedia";
            Duration = 30;
        }
        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("https://en.wikipedia.org/wiki/United_States");
            Thread.Sleep(2 * 1000);
            if (browser == "firefox")
            {
                driver.FindElementById("firstHeading").SendKeys(String.Empty);
                // this is to give the page focus, or else Firefox breaks on scrolling with PgDn
            }
            Program.scrollPage(15);
        }
    }
}
