using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;
using System.Threading;
using OpenQA.Selenium;

namespace TestingPower
{
    class ScenarioOutlook : Scenario
    {
        public ScenarioOutlook()
        {
            Name = "outlook";
            Duration = 60;
        }
        public override void Run(RemoteWebDriver driver, string browser, List<UserInfo> logins)
        {
            driver.Navigate().GoToUrl("http://www.outlook.com");
            try
            {
                // need to log on
                string userName = string.Empty, passWord = string.Empty;

                foreach (var item in logins)
                {
                    if (item.Domain == "outlook.com")
                    {
                        userName = item.UserName;
                        passWord = item.PassWord;
                        break;
                    }
                }

                var userElement = driver.FindElementByName("loginfmt");
                userElement.SendKeys(userName);
                Thread.Sleep(2000);

                var passwordElement = driver.FindElementByName("passwd");
                passwordElement.SendKeys(passWord);
                Thread.Sleep(2000);

                Console.WriteLine("tab to keep signed in");
                // Tab past "keep signed in"
                driver.Keyboard.SendKeys(Keys.Tab);
                Thread.Sleep(2000);

                // Tab to button
                Console.WriteLine("tab to button");
                driver.Keyboard.SendKeys(Keys.Tab);
                Thread.Sleep(2000);
                Console.WriteLine("click button");
                driver.Keyboard.SendKeys(Keys.Enter);
                Thread.Sleep(3000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // already logged on do nothing
            }

            Thread.Sleep(2000);

            // Down arrow thorugh 10 emails
            for (int i = 0; i < 10; i++)
            {
                driver.Keyboard.SendKeys(Keys.ArrowDown);
                Thread.Sleep(2000);
            }
        }
    }
}
