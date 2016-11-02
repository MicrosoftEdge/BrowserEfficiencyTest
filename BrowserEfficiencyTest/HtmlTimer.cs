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
        private const string measureSetName = "duration";

        private RemoteWebDriver _driver;
        private List<List<string>> _results;

        // Information that we'll need eventually to pivot results on
        private string _browser;
        private string _scenario;
        private int _iteration;

        /// <summary>
        /// Creates a new HtmlTimer. Create one of these for each scenario on each browser
        /// </summary>
        /// <param name="driver">The driver to send commands to</param>
        public HtmlTimer(RemoteWebDriver driver, string browser, string scenario, int iteration)
        {
            _driver = driver;
            _results = new List<List<string>>();
            _browser = browser;
            _scenario = scenario;
            _iteration = iteration;
        }

        /// <summary>
        /// Must be called before a navigation occurs away from a page that had page load or a duration measured.
        /// If this isn't called, those measures will be discarded.
        /// </summary>
        public void CollectMetricsFromPage()
        {
            Dictionary<string, object> results = (Dictionary <string, object>) _driver.ExecuteScript("return htmlTimerResults;");
            _driver.ExecuteScript("htmlTimerResults = { };");
            foreach (KeyValuePair<string, object> entry in results)
            {
                Dictionary<string, object> measureResult = (Dictionary<string, object>) entry.Value;
                List<string> outputRow = new List<string>();
                long duration = (long)measureResult["End"] - (long)measureResult["Start"];
                DateTime tempTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
                DateTime finishTime = tempTime.AddMilliseconds((long)measureResult["End"]);

                outputRow.Add("No ETL");
                outputRow.Add(_scenario);
                outputRow.Add(_iteration.ToString());
                outputRow.Add(_browser);
                outputRow.Add(finishTime.Year.ToString() + finishTime.Month.ToString() + finishTime.Day.ToString());
                outputRow.Add(finishTime.Hour.ToString() + finishTime.Minute.ToString() + finishTime.Second.ToString());
                outputRow.Add(measureSetName);
                outputRow.Add(entry.Key);
                outputRow.Add(duration.ToString());

                _results.Add(outputRow);
            }
        }

        /// <summary>
        /// Returns all page loads and durations from all pages this HtmlTimer was used to measure.
        /// </summary>
        /// <returns>
        /// A two dimensional list. Each outer item is a measurement, which contains string representations of:
        /// EtlFileName, Scenario, Iteration, Browser, DateStamp, TimeStamp, MeasureSet, Measure, Result
        /// </returns>
        public List<List<string>> GetAllResults()
        {
            return _results;
        }

        /// <summary>
        /// Ensures that the variable exists to write perf results in the javascript. Can be called multiple times.
        /// </summary>
        private void Initialize()
        {
            _driver.ExecuteScript("htmlTimerResults = { };");
        }

        /// <summary>
        /// Starts timing a new measure from now. Call EndMeasure(key) later to complete this measurement.
        /// </summary>
        /// <param name="key">The key that will identify this measure in the final results</param>
        public void StartMeasure(string key)
        {
            // TODO validate key
            Initialize();
            _driver.ExecuteScript("htmlTimerResults." + key + " = { }; htmlTimerResults." + key + ".Start = new Date().getTime();");
        }

        /// <summary>
        /// Finishes timing a measure, ending now. Assumes you've already called StartMeasure(key).
        /// </summary>
        /// <param name="key">The key that will identify this measure in the final results</param>
        public void EndMeasure(string key)
        {
            // TODO validate key
            _driver.ExecuteScript("htmlTimerResults." + key + ".End = new Date().getTime();");
        }

        /// <summary>
        /// Measures the duration from now to when the providing DOM element exists / is added.
        /// </summary>
        /// <param name="key">The key that will identify this measure in the final results</param>
        /// <param name="domIdType">Either 'id' or 'class' depending on if the domIdentifier is an id or class name</param>
        /// <param name="domIdentifier">The DOM element that will end the measure when it is added.</param>
        public void MeasureToElementExists(string key, string domIdType, string domIdentifier)
        {
            // TODO validate key
            Initialize();
            throw new NotImplementedException();
        }
    }
}
