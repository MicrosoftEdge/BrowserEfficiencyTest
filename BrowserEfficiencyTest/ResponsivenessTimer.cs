using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private List<List<String>> _results;
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
        public void SetBrowser(string browser, Dictionary<string, string> extensionsNameAndVersion = null)
        {
             _browser = browser;
            if (extensionsNameAndVersion != null && extensionsNameAndVersion.Count != 0)
            {
                foreach (var extension in extensionsNameAndVersion)
                {
                    _browser = _browser + "|" + extension.Key + " " + extension.Value;
                }
            }
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

        // Internal functions:

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
