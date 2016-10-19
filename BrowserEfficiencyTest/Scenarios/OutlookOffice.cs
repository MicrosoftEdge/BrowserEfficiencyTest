using System;
using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using System.Threading;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class OutlookOffice : Scenario
    {
        public OutlookOffice()
        {
            Name = "office";
            Duration = 80;
        }

        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            string username = "";
            string password = "";
            foreach (UserInfo item in logins)
            {
                if (item.Domain == "outlook.com")
                {
                    username = item.UserName;
                    password = item.PassWord;
                }
            }

            // Navigate
            driver.Navigate().GoToUrl("http://www.outlook.com");

            Thread.Sleep(5 * 1000);

            // Log in
            var emailIn = driver.FindElementById("i0116");
            foreach (char c in username)
            {
                emailIn.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            emailIn.SendKeys(Keys.Enter);

            Thread.Sleep(1000);

            var passIn = driver.FindElementById("i0118");
            foreach (char c in password)
            {
                passIn.SendKeys(c.ToString());
                Thread.Sleep(75);
            }
            passIn.SendKeys(Keys.Enter);

            Thread.Sleep(10 * 1000);

            // Go to office
            var topNav = driver.FindElementByClassName("o365cs-nav-button");
            topNav.SendKeys(String.Empty);
            topNav.SendKeys(Keys.Enter);

            var wordButton = driver.FindElementById("O365_AppTile_ShellWordOnline");
            wordButton.SendKeys(String.Empty);
            wordButton.SendKeys(Keys.Enter);

            Thread.Sleep(10 * 1000);

            // That opens up a new tab, so we have to give Webdriver focus in the new tab
            driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);

            Thread.Sleep(1000);

            // Open up a Word doc
            var exDocLink = driver.FindElementById("mruitem_0");
            exDocLink.SendKeys(String.Empty);
            exDocLink.SendKeys(Keys.Enter);

            Thread.Sleep(5 * 1000);

            // This next section is in an iframe, so we have to switch to the iframe to access content in it
            driver.SwitchTo().Frame(driver.FindElement(By.Id("sdx_ow_iframe")));

            // Edit the document
            var editDoc = driver.FindElementById("flyoutWordViewerEdit-Medium20");
            editDoc.SendKeys(String.Empty);
            editDoc.SendKeys(Keys.Enter);

            Thread.Sleep(1000);

            var editDocOnline = driver.FindElementById("btnFlyoutEditOnWeb-Menu32");
            editDocOnline.SendKeys(String.Empty);
            editDocOnline.SendKeys(Keys.Enter);

            Thread.Sleep(6 * 1000);

            driver.ScrollPage(2);
        }
    }
}
