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

using System.Diagnostics.Tracing;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// This is the event provider class for BrowserEfficiencyTest.
    /// These events can be captured using Windows Performance Recorder (WPR).
    /// The Provider name is "BrowserEfficiencyTest.ScenarioRunnerEventLog".
    /// For XPerf use the provider GUID 7592876e-8266-5a0c-3f40-1a859137e93a.
    /// </summary>
    [EventSource(Name = "BrowserEfficiencyTest.ScenarioRunnerEventLog")]
    public sealed class ScenarioEventSourceProvider : EventSource
    {
        // The Singleton instance of our event provider.
        static public ScenarioEventSourceProvider EventLog = new ScenarioEventSourceProvider();

        // By using Tasks along with the start and stop opcodes WPA is able to identify the region between these start and stop
        // opcodes as being significant or useful and will allow easier highlighting in WPA timelines.
        public class Tasks
        {
            public const EventTask WorkloadExecution = (EventTask)1;
            public const EventTask ScenarioExecution = (EventTask)2;
            public const EventTask ScenarioIdle = (EventTask)3;
            public const EventTask Wait = (EventTask)4;
            public const EventTask SendKeys = (EventTask)5;
            public const EventTask TypeIntoField = (EventTask)6;
            public const EventTask AccoungLogIn = (EventTask)7;
            public const EventTask WarmupExecution = (EventTask)8;
            public const EventTask MeasurementRegion = (EventTask)9;
            public const EventTask ClearEdgeBrowserCache = (EventTask)10;
            public const EventTask ScenarioAction = (EventTask)11;
        }

        // Basic events - Events for basic test operations are covered by these such as starting and ending of a scenario
        [Event(1, Opcode = EventOpcode.Start, Task = Tasks.WorkloadExecution)]
        public void WorkloadStart(string Workload, string Browser, string WprProfile, int Iteration, int Attempt) { WriteEvent(1, Workload, Browser, WprProfile, Iteration, Attempt); }

        [Event(2, Opcode = EventOpcode.Stop, Task = Tasks.WorkloadExecution)]
        public void WorkloadStop(string Workload, string Browser, string WprProfile, int Iteration, int Attempt) { WriteEvent(2, Workload, Browser, WprProfile, Iteration, Attempt); }

        [Event(3, Opcode = EventOpcode.Start, Task = Tasks.ScenarioExecution)]
        public void ScenarioExecutionStart(string Browser, string ScenarioName) { WriteEvent(3, Browser, ScenarioName); }

        [Event(4, Opcode = EventOpcode.Stop, Task = Tasks.ScenarioExecution)]
        public void ScenarioExecutionStop(string Browser, string ScenarioName) { WriteEvent(4, Browser, ScenarioName); }

        [Event(5, Opcode = EventOpcode.Info)]
        public void LaunchWebDriver(string Browser) { WriteEvent(5, Browser); }

        [Event(6, Opcode = EventOpcode.Info)]
        public void MaximizeBrowser(string Browser) { WriteEvent(6, Browser); }

        [Event(7, Opcode = EventOpcode.Info)]
        public void CloseBrowser(string Browser) { WriteEvent(7, Browser); }

        [Event(8, Opcode = EventOpcode.Info)]
        public void OpenNewTab(int StartingTabCount) { WriteEvent(8, StartingTabCount); }

        [Event(9, Opcode = EventOpcode.Info)]
        public void SwitchTab(string TargetTabHandle) { WriteEvent(9, TargetTabHandle); }

        [Event(10, Opcode = EventOpcode.Info)]
        public void NavigateToUrl(string Url) { WriteEvent(10, Url); }

        [Event(11, Opcode = EventOpcode.Info)]
        public void PageReadyState() { WriteEvent(11); }

        [Event(12, Opcode = EventOpcode.Start, Task = Tasks.ScenarioIdle)]
        public void ScenarioIdleStart(string ScenarioName, double SecondsToWait) { WriteEvent(12, ScenarioName, SecondsToWait); }

        [Event(13, Opcode = EventOpcode.Stop, Task = Tasks.ScenarioIdle)]
        public void ScenarioIdleStop(string ScenarioName, double SecondsToWait) { WriteEvent(13, ScenarioName, SecondsToWait); }

        [Event(14, Opcode = EventOpcode.Start, Task = Tasks.Wait)]
        public void WaitStart(double SecondsToWait, string WaitTag) { WriteEvent(14, SecondsToWait, WaitTag); }

        [Event(15, Opcode = EventOpcode.Stop, Task = Tasks.Wait)]
        public void WaitStop(double SecondsToWait, string WaitTag) { WriteEvent(15, SecondsToWait, WaitTag); }

        // Functional Events - Events for WebDriver functions such as ClickElement are covered here
        [Event(30, Opcode = EventOpcode.Info)]
        public void ClickElement(string ElementText) { WriteEvent(30, ElementText); }

        [Event(31, Opcode = EventOpcode.Info)]
        public void ScrollEvent() { WriteEvent(31); }

        [Event(32, Opcode = EventOpcode.Start, Task = Tasks.TypeIntoField)]
        public void TypeIntoFieldStart(int NumCharacters) { WriteEvent(32, NumCharacters); }

        [Event(33, Opcode = EventOpcode.Stop, Task = Tasks.TypeIntoField)]
        public void TypeIntoFieldStop(int NumCharacters) { WriteEvent(33, NumCharacters); }

        [Event(34, Opcode = EventOpcode.Start, Task = Tasks.SendKeys)]
        public void SendKeysStart(int NumCharacters) { WriteEvent(34, NumCharacters); }

        [Event(35, Opcode = EventOpcode.Stop, Task = Tasks.SendKeys)]
        public void SendKeysStop(int NumCharacters) { WriteEvent(35, NumCharacters); }

        [Event(36, Opcode = EventOpcode.Info)]
        public void NavigateBack() { WriteEvent(36); }

        // These are events for optional base functionality such as clearing the browser cache
        [Event(60, Opcode = EventOpcode.Start, Task = Tasks.WarmupExecution)]
        public void WarmupExecutionStart() { WriteEvent(60); }

        [Event(61, Opcode = EventOpcode.Stop, Task = Tasks.WarmupExecution)]
        public void WarmupExecutionStop() { WriteEvent(61); }

        [Event(62, Opcode = EventOpcode.Start, Task = Tasks.ClearEdgeBrowserCache)]
        public void ClearEdgeBrowserCacheStart() { WriteEvent(62); }

        [Event(63, Opcode = EventOpcode.Stop, Task = Tasks.ClearEdgeBrowserCache)]
        public void ClearEdgeBrowserCacheStop() { WriteEvent(63); }

        // Scenario actions - events for noting scenario level actions such as logging in to an account
        [Event(80, Opcode = EventOpcode.Start, Task = Tasks.AccoungLogIn)]
        public void AccountLogInStart(string WebsiteName) { WriteEvent(80, WebsiteName); }

        [Event(81, Opcode = EventOpcode.Stop, Task = Tasks.AccoungLogIn)]
        public void AccountLogInStop(string WebsiteName) { WriteEvent(81, WebsiteName); }

        [Event(82, Opcode = EventOpcode.Start, Task = Tasks.ScenarioAction)]
        public void ScenarioActionStart(string ActionTag) { WriteEvent(82, ActionTag); }

        [Event(83, Opcode = EventOpcode.Stop, Task = Tasks.ScenarioAction)]
        public void ScenarioActionStop(string ActionTag) { WriteEvent(83, ActionTag); }

        // These are the MeasurementRegion start and stop events.
        // Place these in a scenario and use with the -regions option to force the PerfProcessor
        // to extract the desired metrics from between the start and stop events.
        [Event(100, Opcode = EventOpcode.Start, Task = Tasks.MeasurementRegion)]
        public void MeasurementRegionStart(string MeasurementTag) { WriteEvent(100, MeasurementTag); }

        [Event(101, Opcode = EventOpcode.Stop, Task = Tasks.MeasurementRegion)]
        public void MeasurementRegionStop(string MeasurementTag) { WriteEvent(101, MeasurementTag); }
    }
}
