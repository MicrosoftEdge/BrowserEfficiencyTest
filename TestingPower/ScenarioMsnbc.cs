using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace TestingPower
{
    class ScenarioMsnbc : Scenario
    {
        public ScenarioMsnbc ()
        {
            Name = "msnbc";
            Duration = 50;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.msnbc.com");
            // and scroll up / down
            Program.scrollPage(10);

            // click on one of the links on the page
            // first get back to the top
            driver.ExecuteScript("return window.scrollTo(0,0);");
            Thread.Sleep(2000);

            // TODO: Commented out clicking a subarticle link because it doesn't work on Edge and crashes on Chrome
            // ClickMsnbcLink();
        }

        private static void ClickMsnbcLink(RemoteWebDriver driver)
        {
            var links = driver.FindElements(By.CssSelector("a"));
            foreach (var link in links)
            {
                if (!link.GetAttribute("href").Contains("http://www.msnbc.com"))
                {
                    // Clicking link in Edge hits obscure bug
                    //link.Click();
                    break;
                }
            }
        }
    }
}
