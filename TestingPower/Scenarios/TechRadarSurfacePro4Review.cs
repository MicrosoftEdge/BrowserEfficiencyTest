using System;
using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System.Threading;

namespace TestingPower
{
    class TechRadarSurfacePro4Review : Scenario
    {
        public TechRadarSurfacePro4Review()
        {
            Name = "techRadar";
            Duration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Navigate to the Surface Pro 4 review on TechRadar.
            driver.Navigate().GoToUrl("http://www.techradar.com/us/reviews/pc-mac/tablets/microsoft-surface-pro-4-1290285/review");

            // Give it more than enough time to load
            Thread.Sleep(5000);

            // Scroll down multiple times
            Program.scrollPage(10);
        }
    }
}