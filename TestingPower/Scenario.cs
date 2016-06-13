using OpenQA.Selenium.Remote;
using System.Collections.Generic;

namespace TestingPower
{

     // All scenarios will inherit from this class
    internal abstract class Scenario
    {
        public string Name { get; set; }

        // By default, a scenario will last 40s, but this can be overridden.
        public int Duration { get; set; } = 40;

        // Override this function with the "stuff" to do in the scenario
        public abstract void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins);
    }
}