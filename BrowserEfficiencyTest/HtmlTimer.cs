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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserEfficiencyTest
{
    internal class HtmlTimer
    {
        private RemoteWebDriver _driver;

        /// <summary>
        /// Creates a new HtmlTimer. Must be created after page has been loaded. Data will be cleared when navigating to a new domain
        /// </summary>
        /// <param name="driver">The driver to send commands to</param>
        public HtmlTimer(RemoteWebDriver driver)
        {
            _driver = driver;

            // Create an object in the javascript heap that will collect perf measures
            _driver.ExecuteScript("htmlTimerResults = { };");
        }

        /// <summary>
        /// Must be called before a navigation occurs.
        /// </summary>
        public void CollectMetrics()
        {
            Dictionary<string, object> results = (Dictionary <string, object>) _driver.ExecuteScript("return htmlTimerResults;");
            _driver.ExecuteScript("htmlTimerResults = { };");
            for (int i = 0; i < results.Count; i++)
            {

            }
        }

        public void StartMeasure(string key)
        {
            // TODO validate key
            _driver.ExecuteScript("htmlTimerResults." + key + " = { }; htmlTimerResults." + key + ".Start = Date();");
        }

        public void EndMeasure(string key)
        {
            // TODO validate key
            _driver.ExecuteScript("htmlTimerResults." + key + ".End = Date();");
        }

        public void MeasureToElementExists(string key, string domIdentifier)
        {

        }
    }
}
