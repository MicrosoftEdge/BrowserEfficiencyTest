using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;
using System.Threading;

namespace TestingPower
{
    class ScenarioWikipedia : Scenario
    {
        public ScenarioWikipedia()
        {
            // Specifify name and that it's 30s
            Name = "wikipedia";
            Duration = 30;
        }
        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Nagivate to wikipedia
            driver.Navigate().GoToUrl("https://en.wikipedia.org/wiki/United_States");

            Thread.Sleep(2 * 1000);
            if (browser == "firefox")
            {
                // With Firefox, we had to get focus onto the page, or else PgDn scrolled through the address bar
                driver.FindElementById("firstHeading").SendKeys(String.Empty);
            }

            // Scroll a bit
            Program.scrollPage(15);
        }
    }
}
