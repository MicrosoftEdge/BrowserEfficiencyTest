//--------------------------------------------------------------
//
// Browser Efficiency Test
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files(the ""Software""),
// to deal in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//--------------------------------------------------------------

using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace BrowserEfficiencyTest
{
    internal class NewselaChineseNewYear : Scenario
    {
        public NewselaChineseNewYear()
        {
            Name = "NewselaChineseNewYear";
            DefaultDuration = 120;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Go to Newsela
            driver.NavigateToUrl("http://www.newsela.com");
            driver.Wait(5);

            // Navigate to the library
            driver.ClickElement(driver.FindElement(By.XPath("//*[@href='/articles/#/rule/latest-library']")));
            driver.WaitForPageLoad();
            driver.Wait(5);

            // Search for "Chinese New Year"
            driver.TypeIntoField(driver.FindElementById("inset-search").FindElement(By.TagName("input")), "chinese new year" + Keys.Enter);
            driver.WaitForPageLoad();
            driver.Wait(2);

            // Go to the article
            driver.ClickElement(driver.FindElementByXPath("//*[@href='/articles/lib-history-chinese-new-year/id/25129/']"));
            driver.WaitForPageLoad();
            driver.Wait(2);

            // Sadly, we can't scroll far since we're not logged in
            driver.ScrollPage(1);
        }
    }
}
