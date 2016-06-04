using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class OpenGmail : Scenario
    {
        public OpenGmail()
        {
            Name = "gmail";
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.gmail.com");

            /////////////
            // Log in //
            ///////////

            // Get the relevant username and password
            String username = "";
            String password = "";
            foreach (UserInfo item in logins)
            {
                if (item.Domain == "gmail.com")
                {
                    username = item.UserName;
                    password = item.PassWord;
                }
            }

            // Log in with them
            IWebElement userElement = null;
            try
            {
                userElement = driver.FindElementById("Email");
            }
            catch (Exception e)
            {
                var signInButton = driver.FindElementByXPath("//*[@data-g-label='Sign in']");
                signInButton.SendKeys(String.Empty);
                driver.Keyboard.SendKeys(Keys.Enter);
                Thread.Sleep(2 * 1000);
                userElement = driver.FindElementById("Email");
            }
            foreach (char c in username)
            {
                userElement.SendKeys(c.ToString());
                Thread.Sleep(75);
            }

            Thread.Sleep(1000);

            // Tab down and hit next button
            driver.Keyboard.SendKeys(Keys.Tab);
            driver.Keyboard.SendKeys(Keys.Enter);

            Thread.Sleep(1000);

            // Enter password
            var passwordElement = driver.FindElementById("Passwd");
            foreach (char c in password)
            {
                passwordElement.SendKeys(c.ToString());
                Thread.Sleep(75);
            }

            // Tab down and hit submit button
            driver.Keyboard.SendKeys(Keys.Tab);
            Thread.Sleep(2000);
            driver.Keyboard.SendKeys(Keys.Enter);

            Thread.Sleep(7 * 1000);

            // Go through some emails
            for (int i = 0; i < 5; i++)
            {
                // Go into email
                //driver.FindElementsByClassName("zA").ElementAt(i).SendKeys(String.Empty);
                driver.Keyboard.SendKeys("o");
                
                Thread.Sleep(4000);
                // Go back to inbox
                driver.Keyboard.SendKeys(Keys.Backspace);
                Thread.Sleep(2000);
                // Select next email with the "cursor". Do this with the j key in gmail
                driver.Keyboard.SendKeys("j");
                Thread.Sleep(2000);
            }
        }

    }
}
