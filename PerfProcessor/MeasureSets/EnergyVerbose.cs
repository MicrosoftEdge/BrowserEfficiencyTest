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
    /// Energy Usage Verbose measure. Reports the complete E3 energy data for a system.
    /// </summary>
    internal class EnergyVerbose : MeasureSet
    {
        public EnergyVerbose()
        {
            _wpaProfiles = new List<string>() { @".\MeasureSetDefinitionAssets\Energy.wpaProfile" };
            WprProfile = "energy";
            TracingMode = TraceCaptureMode.File;
            Name = "energyVerbose";
            _wpaExportedDataFileNames = new List<string>() { "Energy_Estimation_Engine_Summary_Table_(by_Process)_E3EnergyByProcess.csv" };
        }

        /// <summary>
        /// Reports all the E3 energy usage by all apps and services running.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating energy usage.</param>
        /// <returns>A dictionary of App and component energy usage in millijoules.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = new Dictionary<string, string>();

            // Since this is the verbose version of the energy measure, report all the energy data we have in the csv file.
            foreach (var row in csvData["Energy_Estimation_Engine_Summary_Table_(by_Process)_E3EnergyByProcess.csv"])
            {
                string [] columns = SplitCsvString(row);
                metrics.Add(columns[0] + " | CpuEnergy(mJ)", columns[1]);
                metrics.Add(columns[0] + " | SocEnergy(mJ)", columns[2]);
                metrics.Add(columns[0] + " | DiskEnergy(mJ)", columns[3]);
                metrics.Add(columns[0] + " | DisplayEnergy(mJ)", columns[4]);
                metrics.Add(columns[0] + " | MbbEnergy(mJ)", columns[5]);
                metrics.Add(columns[0] + " | NetworkEnergy(mJ)", columns[6]);
                metrics.Add(columns[0] + " | OtherEnergy(mJ)", columns[7]);
                metrics.Add(columns[0] + " | TotalEnergy(mJ)", columns[8]);
                metrics.Add(columns[0] + " | StartTime(ns)", columns[9]);
                metrics.Add(columns[0] + " | EndTime(ns)", columns[10]);
            }

            return metrics;
        }
    }
}
