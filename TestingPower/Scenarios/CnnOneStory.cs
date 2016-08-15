using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;

namespace TestingPower
{
    class CnnOneStory : Scenario
    {
        public CnnOneStory()
        {
            Name = "cnnOneStory";
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://bleacherreport.com/articles/2657513-tim-duncan-declined-olympic-invitation-from-president-obama?utm_source=cnn.com&utm_medium=referral&utm_campaign=editorial");
        }
    }
}
