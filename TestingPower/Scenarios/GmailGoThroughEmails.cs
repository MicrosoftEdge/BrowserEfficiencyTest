using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace TestingPower
{
    class GmailGoThroughEmails : Scenario
    {

        public GmailGoThroughEmails()
        {
            Name = "gmail";
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            NavigateToGmail(driver);
            Thread.Sleep(2 * 1000);
            LogIn(driver, logins);
            Thread.Sleep(7 * 1000);
            BrowseEmails(driver, 5);
        }

        private void NavigateToGmail(RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl("http://www.gmail.com");
        }

        private void LogIn(RemoteWebDriver driver, List<UserInfo> logins)
        {
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

            // We were getting two different log in pages from gmail and it didn't seem very deterministic which one
            // to expect, so, currently handling this with a try/catch.
            try
            {
                // One of the pages had this element...
                userElement = driver.FindElementById("Email");
            }
            catch (Exception e)
            {
                // ...but if we couldn't find it, we apparently got sereved this other log in page.
                // So catch the exception and get to the login page we're looking for
                var signInButton = driver.FindElementByXPath("//*[@data-g-label='Sign in']");
                signInButton.SendKeys(String.Empty);
                driver.Keyboard.SendKeys(Keys.Enter);
                Thread.Sleep(2 * 1000);
                userElement = driver.FindElementById("Email");
            }
            // So now, no matter which page we we're served originally, we're in the same place
            // Type in the user name
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
        }

        private void BrowseEmails(RemoteWebDriver driver, int numOfEmailsToBrowse)
        {
            // Go through some emails
            for (int i = 0; i < numOfEmailsToBrowse; i++)
            {
                // Go into email

                // This was to get focus, but it didn't work super reliably across browsers
                // driver.FindElementsByClassName("zA").ElementAt(i).SendKeys(String.Empty);
                // Simply using the shortcut keys worked pretty well though
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
