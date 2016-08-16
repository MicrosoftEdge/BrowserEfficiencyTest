using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;

namespace TestingPower
{
    /// <summary>
    /// This scenario is designed to end quickly, and is for testing
    /// </summary>
    class FastScenario : Scenario
    {
        public FastScenario()
        {
            Name = "fastScenario";
            Duration = 10;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.google.com");
        }
    }
}
