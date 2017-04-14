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
    /// </summary>
    [EventSource(Name = "BrowserEfficiencyTest.ScenarioRunnerEventLog")]
    public sealed class ScenarioEventSourceProvider : EventSource
    {
        // the Singleton instance of our event provider
        static public ScenarioEventSourceProvider EventLog = new ScenarioEventSourceProvider();

        // By using Tasks along with the start and stop opcodes WPA is able to identify the region between these start and stop
        // opcodes as being significant or useful and will allow easier highlighting in WPA timelines.
        public class Tasks
        {
            public const EventTask WorkloadExecution = (EventTask)1;
            public const EventTask ScenarioExecution = (EventTask)2;
            public const EventTask Wait = (EventTask)3;
            public const EventTask SendKeys = (EventTask)4;
            public const EventTask TypeIntoField = (EventTask)5;
        }

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
        public void NavigateToUrl(string Url) { WriteEvent(7, Url); }

        [Event(8, Opcode = EventOpcode.Info)]
        public void PageReadyState() { WriteEvent(8); }

        [Event(9, Opcode = EventOpcode.Info)]
        public void ClickElement(string ElementText) { WriteEvent(9, ElementText); }

        [Event(10, Opcode = EventOpcode.Info)]
        public void OpenNewTab(int StartingTabCount, int EndingTabCount) { WriteEvent(10, StartingTabCount, EndingTabCount); }

        [Event(11, Opcode = EventOpcode.Info)]
        public void SwitchTab(string TabHandle) { WriteEvent(11, TabHandle); }

        [Event(12, Opcode = EventOpcode.Info)]
        public void CloseBrowser(string Browser) { WriteEvent(12, Browser); }

        [Event(13, Opcode = EventOpcode.Info)]
        public void ScrollEvent() { WriteEvent(13); }

        [Event(14, Opcode = EventOpcode.Start, Task = Tasks.Wait)]
        public void WaitStart(double SecondsToWait) { WriteEvent(14, SecondsToWait); }

        [Event(15, Opcode = EventOpcode.Stop, Task = Tasks.Wait)]
        public void WaitStop(double SecondsToWait) { WriteEvent(15, SecondsToWait); }

        [Event(16, Opcode = EventOpcode.Start, Task = Tasks.TypeIntoField)]
        public void TypeIntoFieldStart(int NumCharacters) { WriteEvent(16, NumCharacters); }

        [Event(17, Opcode = EventOpcode.Stop, Task = Tasks.TypeIntoField)]
        public void TypeIntoFieldStop(int NumCharacters) { WriteEvent(17, NumCharacters); }

        [Event(18, Opcode = EventOpcode.Start, Task = Tasks.SendKeys)]
        public void SendKeysStart(int NumCharacters) { WriteEvent(18, NumCharacters); }

        [Event(19, Opcode = EventOpcode.Stop, Task = Tasks.SendKeys)]
        public void SendKeysStop(int NumCharacters) { WriteEvent(19, NumCharacters); }

        [Event(20, Opcode = EventOpcode.Info)]
        public void NavigateBack() { WriteEvent(20); }

    }
}
