using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserEfficiencyTest
{
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

        public ResponsivenessTimer()
        {
            _results = new List<List<String>>();
            _enabled = false;
        }

        public void setIteration(int iteration)
        {
            _iteration = iteration;
        }

        public void setBrowser(string browser)
        {
            _browser = browser;
        }

        public void setMeasureSet(string measureSet)
        {
            _measureSet = measureSet;
        }

        public void setScenario(string scenario)
        {
            _currentSceanrio = scenario;
        }

        public void setDriver(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        public void enable()
        {
            _enabled = true;
        }


        public List<string> getResults()
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

        public void extractPageLoadTime()
        {
            if (_enabled)
            {
                IJavaScriptExecutor javascriptEngine = _driver as IJavaScriptExecutor;
                var timeToLoad = javascriptEngine.ExecuteScript("return performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart");
                makeRecord("Page Load Time (ms)", timeToLoad.ToString());
            }
        }

        // Internal functions:

        private void makeRecord(string measure, string result)
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
