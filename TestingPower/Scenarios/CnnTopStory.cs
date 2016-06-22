using System;
using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System.Threading;

namespace TestingPower
{
    class CnnTopStory : Scenario
    {
        public CnnTopStory()
        {
            Name = "cnn";

            // Using 80 as sometimes Chrome takes just over 70 seconds to run            
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.cnn.com");

            // Give it more than enough time to load
            Thread.Sleep(5000); 

            // Get the element that contains the headline story
            var headlineElement = driver.FindElementByClassName("js-screaming-banner");

            // Get focus on the headline story link
            headlineElement.SendKeys(String.Empty);

            // Open the link to the headline story
            headlineElement.SendKeys(Keys.Enter);

            // And let that load
            Thread.Sleep(2 * 1000);

            // Scroll down multiple times
            Program.scrollPage(10);
        }
    }
}