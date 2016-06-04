using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Threading;

namespace TestingPower
{
    class PlayYoutubeVideo : Scenario
    {
        public PlayYoutubeVideo()
        {
            this.Name = "youtube";
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            if (!driver.Url.Contains("youtube.com"))// have not browsed to youtube yet
            {
                driver.Navigate().GoToUrl("https://www.youtube.com/watch?v=l42U5Cwn1Y0");
                Thread.Sleep(2000);
            }

            string movieId = "movie_player";
            var player = driver.FindElementById(movieId);
            
            // Earlier iteration of this code could play OR pause. We may need that code again in future.
            //if ((hitPlay && player.GetAttribute("class").Contains("paused-mode")) ||
            //    (!hitPlay && player.GetAttribute("class").Contains("playing-mode")))
            //{
            //    player.Click();
            //}

            if (player.GetAttribute("class").Contains("paused-mode"))
            {
                player.Click();
            }

            Thread.Sleep(2000);
        }
    }
}
