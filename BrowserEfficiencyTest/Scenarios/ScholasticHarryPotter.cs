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
    internal class ScholasticHarryPotter : Scenario
    {
        public ScholasticHarryPotter()
        {
            Name = "ScholasticHarryPotter";
            DefaultDuration = 60;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Go to Scholastic
            driver.NavigateToUrl("http://www.scholastic.com");
            driver.Wait(5);

            // The dropdown nav does not play well with webdriver, so navigate to Harry Potter directly
            driver.NavigateToUrl("http://harrypotter.scholastic.com/?esp=CORPHP/ib/////NAV/Kids/QLinks/STACKSHarryPotter////");
            driver.Wait(10);

            // Go to the first book
            driver.ClickElement(driver.FindElementById("home_nonflash").FindElement(By.XPath("//*[@href='/sorcerers_stone/']")));
            driver.WaitForPageLoad();
            driver.Wait(5);

            // Then scroll down
            driver.ScrollPage(2);
        }
    }
}
