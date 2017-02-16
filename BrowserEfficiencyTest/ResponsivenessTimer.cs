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

using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserEfficiencyTest
{

    /// <summary>
    /// Handles the logic for measuring user-perceivable performance. This class makes it easy for scenarios to measure page load time etc
    /// </summary>
    internal class ResponsivenessTimer
    {
        private List<List<string>> _results;
        private Dictionary<string, Dictionary<string, long>> _partialResults;
        private string _currentSceanrio;
        private int _iteration;
        private string _browser;
        private string _measureSet;
        private RemoteWebDriver _driver;
        private bool _enabled;

        // Functions for test harness:

        /// <summary>
        /// Creates a new ResponsivenessTimer. It will be disabled unless enabled later on. When disabled, all calls that create measures will be no-ops.
        /// </summary>
        public ResponsivenessTimer()
        {
            _results = new List<List<String>>();
            _enabled = false;
            _partialResults = new Dictionary<string, Dictionary<string, long>>();
        }

        /// <summary>
        /// Sets the iteration, which will be included when a measurement is recorded later.
        /// </summary>
        /// <param name="iteration">The current iteration</param>
        public void SetIteration(int iteration)
        {
            _iteration = iteration;
        }

        /// <summary>
        /// Sets the browser, which will be included when a measurement is recorded later.
        /// </summary>
        /// <param name="browser">The current browser</param>
        public void SetBrowser(string browser)
        {
            _browser = browser;
        }

        /// <summary>
        /// Sets the measureSet, which will be included when a measurement is recorded later.
        /// </summary>
        /// <param name="measureSet"></param>
        public void SetMeasureSet(string measureSet)
        {
            _measureSet = measureSet;
        }

        /// <summary>
        /// Sets the sceanrio, which will be included when a measurement is recorded later.
        /// </summary>
        /// <param name="scenario">The current scenario</param>
        public void SetScenario(string scenario)
        {
            _currentSceanrio = scenario;
        }

        /// <summary>
        /// Sets the driver, needed to perform measurements later.
        /// </summary>
        /// <param name="driver">The driver to access the browser</param>
        public void SetDriver(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        /// <summary>
        /// Enables the ResponsivenessTimer. After this function is called, measurements will be taken when running scenarios.
        /// </summary>
        public void Enable()
        {
            _enabled = true;
        }

        /// <summary>
        /// Returns all recorded results
        /// </summary>
        /// <returns>A list of comma-delimited strings with measurements</returns>
        public List<string> GetResults()
        {
            List<string> results = new List<string>();
            foreach (List<string> result in _results)
            {
                string resultString = "";
                foreach(string component in result)
                {
                    if (resultString != "")
                    {
                        resultString += ",";
                    }
                    resultString += component;
                }
                results.Add(resultString);
            }
            return results;
        }

        // Functions for scenarios to record responsiveness results:

        /// <summary>
        /// Records how long the current page took to load (from navigation start to the end of the DOMContentLoaded event).
        /// </summary>
        /// <param name="pageLoaded">The name (or other identifier) of the current page. Needed to distinguish multiple page loads in a single scenario.</param>
        public void ExtractPageLoadTime(string pageLoaded = null)
        {
            if (_enabled)
            {
                IJavaScriptExecutor javascriptEngine = _driver as IJavaScriptExecutor;
                var timeToLoad = javascriptEngine.ExecuteScript("return performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart");
                string measureName = "Page Load Time (ms)";
                if (pageLoaded != null && pageLoaded != "")
                {
                    measureName += ": " + pageLoaded;
                }
                MakeRecord(measureName, timeToLoad.ToString());
            }
        }

        public void StartMeasure(string key)
        {
            if (_enabled)
            {
                _driver.ExecuteScript(@"
                if (!document.responsivenessResults) {
                    document.responsivenessResults = {};
                }
                if (!document.responsivenessResults[""" + key + @"""]) {
                    document.responsivenessResults[""" + key + @"""] = {};
                }
                document.responsivenessResults[""" + key + @"""][""start""] = (new Date()).getTime();
                ");
            }

        }

        public void StartMeasureOnEnterKeyPressed(string key)
        {
            if (_enabled)
            {
                _driver.ExecuteScript(@"
                function recordResultOnEnter(e) {
                    if (e.keyCode === 13) {
                        if (!document.responsivenessResults) {
                            document.responsivenessResults = {};
                        }
                        if (!document.responsivenessResults[""" + key + @"""]) {
                            document.responsivenessResults[""" + key + @"""] = {};
                        }
                        document.responsivenessResults[""" + key + @"""][""start""] = (new Date()).getTime();
                        document.removeEventListener(""keyup"", recordResultOnEnter);
                    }
                }

                document.addEventListener(""keyup"", recordResultOnEnter);
                ");
            }
        }

        public void EndMeasureWhenElementExists(string key, string domIdentifier)
        {
            if (_enabled)
            {
                _driver.ExecuteScript(@"
                function recordResult() {
                    if (!document.responsivenessResults) {
                        document.responsivenessResults = {};
                    }
                    if (!document.responsivenessResults[""" + key + @"""]) {
                        document.responsivenessResults[""" + key + @"""] = {};
                    }
                    document.responsivenessResults[""" + key + @"""][""end""] = (new Date()).getTime();
                }

                if (document.querySelectorAll(""" + domIdentifier + @""").length > 0) {
                    recordResult();
                } else {
                    if (document.observer instanceof MutationObserver) {
                        document.observer.disconnect();
                    } else {
                        document.observer = new MutationObserver(function (mutations) {
                            mutations.forEach(function (mutation) {
                                if (!mutation.addedNodes) {
                                    return;
                                }
                                for (var i = 0; i < mutation.addedNodes.length; i++) {
                                    console.log(mutation.addedNodes[i]);
                                    if (mutation.addedNodes[i].matches(""" + domIdentifier + @""") || mutation.addedNodes[i].querySelectorAll(""" + domIdentifier + @""").length > 0) {
                                        recordResult();
                                        document.observer.disconnect();
                                    }
                                }
                            });
                        });
                    }

                    document.observer.observe(document.body, {
                        childList: true,
                        subtree: true,
                        attributes: false,
                        characterData: false
                    });
                }
                ");
            }
        }

        public void ExtractMeasures()
        {
            IJavaScriptExecutor javascriptEngine = _driver as IJavaScriptExecutor;
            var resultsFromJS = javascriptEngine.ExecuteScript("return document.responsivenessResults;");
            Dictionary<string, object> castedResultsFromJS = (Dictionary<string, object>)resultsFromJS;
            if (castedResultsFromJS != null)
            {
                foreach (object partialResult in castedResultsFromJS)
                {
                    KeyValuePair<string, object> kvPair = (KeyValuePair<string, object>)partialResult;
                    Dictionary<string, object> kvPairContents = (Dictionary<string, object>)kvPair.Value;

                    // We get a big object of measurements from the JS heap. Here, we iterate through them
                    // and for each one, we add the pieces to _partialResults. This way, they'll be kept
                    // after a page navigate, which will drop the JS object. Because it's valid to get
                    // a start and an end from different pages, we allow any given page to extract one or
                    // the other (or both of course)

                    if (kvPairContents.ContainsKey("start"))
                    {
                        AddPartialEntryTimestamp(kvPair.Key, "start", (long)kvPairContents["start"]);
                    }
                    if (kvPairContents.ContainsKey("end"))
                    {
                        AddPartialEntryTimestamp(kvPair.Key, "end", (long)kvPairContents["end"]);
                    }
                }
            }
            RecordCompletedPartialResults();
        }

        // Internal functions:

        private void AddPartialEntryTimestamp(string key, string timepointName, long timestamp)
        {
            if (!_partialResults.ContainsKey(key))
            {
                _partialResults.Add(key, new Dictionary<string, long>());
            }
            if (!_partialResults[key].ContainsKey(timepointName))
            {
                _partialResults[key][timepointName] = timestamp;
            }
        }

        private void RecordCompletedPartialResults()
        {
            List<string> completedResults = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string,long>> measurement in _partialResults)
            {
                // For every set of measurements in _partialResults, if it has both a start and an end,
                // then we have all the info we need to record a complete measurement in _results
                if (measurement.Value.ContainsKey("start") && measurement.Value.ContainsKey("end"))
                {
                    long result = measurement.Value["end"] - measurement.Value["start"];
                    makeRecord(measurement.Key, result.ToString());
                    completedResults.Add(measurement.Key);
                }
            }

            // Can't modify _partialResults while iterating through it above, so remove completed results
            // here instead
            foreach (string keyToRemove in completedResults)
            {
                _partialResults.Remove(keyToRemove);
            }
        }

        /// <summary>
        /// Records a measurement. Fills in most fields automatically, and so it only needs the measure name and result
        /// </summary>
        /// <param name="measure">The measure name</param>
        /// <param name="result">The result of the measurement</param>
        private void MakeRecord(string measure, string result)
        {
            DateTime now = DateTime.Now;
            List<String> record = new List<String>();
            record.Add("no_etl");
            record.Add(_currentSceanrio);
            record.Add(_iteration.ToString());
            record.Add(_browser);
            record.Add(now.ToString("yyyyMMdd"));
            record.Add(now.ToString("HHmmss"));
            record.Add("responsiveness (" + _measureSet + ")");
            record.Add(measure);
            record.Add(result);
            _results.Add(record);
        }
    }
}
