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
            Duration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.cnn.com");

            // Give it more than enough time to load
            Thread.Sleep(5000); 

            // Get the element that contains the headline story
            var headlineElement = driver.FindElementByClassName("zn-banner");
            var aRefElement = headlineElement.FindElement(By.TagName("a"));

            // Get focus on the headline story link 
            aRefElement.SendKeys(String.Empty);

            // Open the link to the headline story
            aRefElement.SendKeys(Keys.Enter);

            // And let that load
            Thread.Sleep(2 * 1000);

            // Scroll down multiple times
            Program.scrollPage(10);
        }
    }
}