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
    internal class KhanAcademyGrade8Math : Scenario
    {
        public KhanAcademyGrade8Math()
        {
            Name = "KhanAcademyGrade8Math";
            DefaultDuration = 90;
        }

        public override void Run(RemoteWebDriver driver, string browser, CredentialManager credentialManager, ResponsivenessTimer timer)
        {
            // Go to Khan Academy
            driver.NavigateToUrl("http://www.khanacademy.org");
            driver.Wait(5);

            driver.ScrollPage(1);

            // Go to 8th grade math
            driver.NavigateToUrl("https://www.khanacademy.org/math/cc-eighth-grade-math");
            driver.Wait(5);

            // Click on the section on repeating decimals
            driver.ClickElement(driver.FindElement(By.XPath("//*[contains(text(), 'Repeating decimals')]")));
            driver.Wait(5);

            // Get the element with the text "Converting a fraction...", but click on its grandparent, because the grandparent
            // is the anchor
            driver.ClickElement(driver.FindElement(By.XPath("//*[contains(text(), 'Converting a fraction to a repeating decimal')]"))
                .FindElement(By.XPath(".."))    // ...These two lines each go up to the parent element
                .FindElement(By.XPath("..")));

            // Watch the movie for 30s, then go back.
            driver.Wait(30);
            driver.NavigateBack();
        }
    }
}
