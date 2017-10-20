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
    /// CPU Usage measure. Calculates the CPU Usage time by process.
    /// </summary>
    internal class CpuUsageVerbose : MeasureSet
    {
        public CpuUsageVerbose()
        {
            _wpaProfiles = new List<string>() { @".\MeasureSetDefinitionAssets\CpuUsage.wpaProfile" };
            WprProfile = "cpuUsage";
            TracingMode = TraceCaptureMode.File;
            Name = "cpuUsageVerbose";
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
            decimal idleCpuUsageTime = 0;
            decimal cpuUsageTimeNotIdle = 0;

            // Process the raw string data into a usable format.
            var rawCpuUsageTimeData = from row in csvData.First().Value
                                      let fields = SplitCsvString(row)
                                      select new { ProcessName = fields[0], CpuUsagePercentage = Convert.ToDecimal(fields[2]) };

            idleCpuUsageTime = rawCpuUsageTimeData.First(s => s.ProcessName.Equals("Idle", StringComparison.InvariantCultureIgnoreCase)).CpuUsagePercentage;

            cpuUsageTimeNotIdle = (Decimal)100.0 - idleCpuUsageTime;

            metrics = new Dictionary<string, string>() { { "CPU Total Utilization %", cpuUsageTimeNotIdle.ToString() } };

            foreach (var row in rawCpuUsageTimeData)
            {
                metrics.Add("CPU Usage % | " + row.ProcessName, row.CpuUsagePercentage.ToString());
            }

            return metrics;
        }
    }
}
