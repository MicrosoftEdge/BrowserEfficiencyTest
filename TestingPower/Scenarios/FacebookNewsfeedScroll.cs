using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System.Threading;

namespace TestingPower
{
    class FacebookNewsfeedScroll : Scenario
    {
        public FacebookNewsfeedScroll()
        {
            // Specify its name and the total time the scenario will take (in seconds)
            Name = "facebook";
            Duration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.facebook.com");

            // if not logged on, log on
            var elems = driver.FindElements(By.CssSelector("H2"));
            foreach (IWebElement elem in elems)
            {
                if (elem.Text.Contains("Sign Up")) // not signed in
                {
                    string userName = string.Empty, passWord = string.Empty;

                    foreach (var item in logins)
                    {
                        if (item.Domain == "facebook.com")
                        {
                            userName = item.UserName;
                            passWord = item.PassWord;
                            break;
                        }
                    }

                    Thread.Sleep(2000);
                    var username = driver.FindElement(By.Id("email"));
                    var password = driver.FindElement(By.Id("pass"));

                    username.Clear();
                    username.SendKeys(userName);
                    Thread.Sleep(1000);

                    password.Clear();
                    Thread.Sleep(3000);
                    password.SendKeys(passWord);
                    Thread.Sleep(1000);

                    // Avoding applying click to button because of ObscureElement bug in Edge on high DPI monitors
                    // Instead use tab and enter. Seemed to be pretty reliable across browsers
                    driver.Keyboard.SendKeys(Keys.Tab);
                    driver.Keyboard.SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    break;
                }
            }

            // Once we're logged in, all we're going to do is scroll through the page
            // We're simply measuring a user looking through their news feed for a minute

            Program.scrollPage(20);
        }
    }
}
