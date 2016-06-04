using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace TestingPower
{
    class OpenMsn : Scenario
    {
        public OpenMsn()
        {
            this.Name = "msn";
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.msn.com");
            Thread.Sleep(1000);
        }
    }
}
