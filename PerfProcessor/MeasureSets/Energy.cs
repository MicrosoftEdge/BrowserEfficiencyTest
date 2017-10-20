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
    /// Energy Usage measure. Calculates the hardware measured energy usage by hardware meter.
    /// </summary>
    internal class Energy : MeasureSet
    {
        public Energy()
        {
            _wpaProfiles = new List<string>() { @".\MeasureSetDefinitionAssets\Energy.wpaProfile" };
            WprProfile = "energy";
            TracingMode = TraceCaptureMode.File;
            Name = "energy";
            _wpaExportedDataFileNames = new List<string>() { "Energy_Estimation_Engine_Summary_Table_(by_Process)_E3EnergyByProcess.csv" };
        }

        /// <summary>
        /// Calculates the hardware measured energy usage by hardware meter.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating energy usage.</param>
        /// <returns>A dictionary of hardware meters and their energy usage in millijoules.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            ulong elapsedTime = 0;
            Dictionary<string, string> metrics = null;

            // Process the raw string data into a usable format.
            var hardwareMeasurements = from row in csvData.First().Value
                                       let fields = SplitCsvString(row)
                                       where fields[0].StartsWith("EMI_")
                                       select new { Name = fields[0], Energy = fields[8], ElapsedTime = (Convert.ToUInt64(fields[10]) - Convert.ToUInt64(fields[9])) };

            if (hardwareMeasurements.Count() > 0)
            {
                elapsedTime = hardwareMeasurements.Max(s => s.ElapsedTime);

                metrics = hardwareMeasurements.ToDictionary(k => "Energy(mJ) | " + k.Name, v => v.Energy);
                metrics.Add("ElapsedTime(ns)", elapsedTime.ToString());
            }

            return metrics;
        }
    }
}
