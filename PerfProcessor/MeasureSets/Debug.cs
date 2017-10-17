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

using System;
using System.Collections.Generic;
using System.Linq;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Debug MeasureSet. This is a dummy placeholder for using the Debug wprp profile.
    /// </summary>
    internal class Debug : MeasureSet
    {
        public Debug()
        {
            _wpaProfile = @".\MeasureSetDefinitionAssets\CpuUsage.wpaProfile";
            WprProfile = "debug";
            TracingMode = TraceCaptureMode.File;
            Name = "debug";
            _wpaExportedDataFileNames = new List<string>() { "CPU_Usage_(Attributed)_CPU_UsageTime_ByProcess.csv" };
        }

        /// <summary>
        /// Calculates the CPU usage time % that the system is not idle.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating CPU usage time.</param>
        /// <returns>A dictionary of processes and their CPU usage time in milliseconds.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = null;
            // This is just an empty MeasureSet to be used with Debug wprp profile.
            return metrics;
        }
    }
}
