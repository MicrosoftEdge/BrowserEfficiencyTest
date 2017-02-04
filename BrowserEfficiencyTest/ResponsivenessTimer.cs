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
        private List<MeasureRecord>  _results;
        private string _currentSceanrio;
        private int _iteration;
        private string _browser;
        private string _measureSet;
        private RemoteWebDriver _driver;
        private bool _enabled;

        internal class MeasureRecord
        {
            public string EtlName;
            public string Scenario;
            public string Iteration;
            public string Browser;
            public string Date;
            public string Time;
            public string MeasureSet;
            public string Measure;
            public string Result;

            public MeasureRecord(string etlName, string scenario, string iteration, string browser, string date, string time, string measureSet, string measure, string result)
            {
                EtlName = etlName;
                Scenario = scenario;
                Iteration = iteration;
                Browser = browser;
                Date = date;
                Time = time;
                MeasureSet = measureSet;
                Measure = measure;
                Result = result;
            }
        }

        public ResponsivenessTimer()
        {
            _results = new List<MeasureRecord>();
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

        private void makeRecord(string measure, int result)
        {
            DateTime now = new DateTime();
            string date = "" + now.Year + now.Month + now.Day;
            string time = "" + now.Hour + now.Minute + now.Second;
            MeasureRecord record = new MeasureRecord("no_etl", _currentSceanrio, _iteration.ToString(), _browser, date, time, "responsiveness (" + _measureSet + ")", measure, result.ToString());
            _results.Add(record);
        }

        // Functions for scenarios to record responsiveness results:

        public void extractPLT()
        {
            if (_enabled)
            {
                makeRecord("Page Load Time (ms)", 99);
                Console.WriteLine("new record created: " + _results[_results.Count - 1].ToString());
            }
        }
    }
}
