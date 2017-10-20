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
    /// Network Usage measure. Calculates the number of bytes sent and received over the network by process.
    /// </summary>
    internal class NetworkUsage : MeasureSet
    {
        public NetworkUsage()
        {
            _wpaProfiles = new List<string>() { @".\MeasureSetDefinitionAssets\NetworkUsage.wpaProfile" };
            WprProfile = "networkUsage";
            TracingMode = TraceCaptureMode.File;
            Name = "networkUsage";
            _wpaExportedDataFileNames = new List<string>() { "TcpIp_Events_SendReceiveBytes.csv" };
        }

        /// <summary>
        /// Calculates the number of bytes sent and received over the network by process.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating the number of bytes sent and received over the network.</param>
        /// <returns>A dictionary of processes and the number of bytes send and recieved over the network.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = null;

            // Process the raw string data into a usable format.
            var rawNetworkUsageData = from row in csvData.First().Value
                                      let fields = SplitCsvString(row)
                                      select new { EventType = fields[0], NumBytes = Convert.ToUInt64(fields[1]) };

            if (rawNetworkUsageData.Count() == 0)
            {
                return null;
            }

            // Format the network usage bytes results to metric form.
            var totalBytes = (from row in rawNetworkUsageData
                                 select new KeyValuePair<string, string>("Total Bytes | " + row.EventType, row.NumBytes.ToString())).ToDictionary(k => k.Key, v => v.Value);

            metrics = totalBytes;

            return metrics;
        }
    }
}
