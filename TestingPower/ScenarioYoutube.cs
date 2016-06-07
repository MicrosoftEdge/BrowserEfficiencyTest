using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace TestingPower
{
    class ScenarioYoutube : Scenario
    {
        public ScenarioYoutube()
        {
            this.Name = "youtube";
            // Leave the default time
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            // Browse to Youtube if we're not there already
            if (!driver.Url.Contains("youtube.com"))
            {
                driver.Navigate().GoToUrl("https://www.youtube.com/watch?v=l42U5Cwn1Y0");
                Thread.Sleep(2000);
            }

            string movieId = "movie_player";
            var player = driver.FindElementById(movieId);
            
            // Earlier iteration of this code could play OR pause. We may need that code again in future.
            // if ((hitPlay && player.GetAttribute("class").Contains("paused-mode")) ||
            //     (!hitPlay && player.GetAttribute("class").Contains("playing-mode")))
            // {
            //     player.Click();
            // }

            // Play if it's paused
            if (player.GetAttribute("class").Contains("paused-mode"))
            {
                player.Click();
            }

            Thread.Sleep(2000);
        }
    }
}
