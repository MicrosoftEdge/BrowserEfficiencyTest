using OpenQA.Selenium.Remote;
using System.Collections.Generic;

namespace TestingPower
{
    internal abstract class Scenario
    {
        public string Name { get; set; }
        public int Duration { get; set; } = 40;

        public abstract void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins);
    }
}