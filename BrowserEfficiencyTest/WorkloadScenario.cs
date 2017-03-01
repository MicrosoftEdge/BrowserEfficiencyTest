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

using Newtonsoft.Json;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// This is a wrapper class for Workloads to specify Scenarios.
    /// This class allows a scenario to be specified in a workload and can
    /// be read in through a JSON deserializer.
    /// </summary>
    internal class WorkloadScenario
    {
        [JsonProperty("ScenarioName")]
        public string ScenarioName { get; set; }

        [JsonProperty("Tab")]
        public string Tab { get; set; }

        [JsonProperty("Duration")]
        public int Duration { get; set; }

        // Holds a reference to the actual scenario.
        public Scenario Scenario { get; set; }

        public WorkloadScenario(string scenarioName, string tab, int duration, Scenario scenario)
        {
            ScenarioName = scenarioName.ToLowerInvariant();
            Tab = tab;
            Duration = duration;
            Scenario = scenario;
        }
    }
}
