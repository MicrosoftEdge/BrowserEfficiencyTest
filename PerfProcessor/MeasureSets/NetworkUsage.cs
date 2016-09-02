//--------------------------------------------------------------
//
// Microsoft Edge Power Test
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

namespace TestingPower
{
    /// <summary>
    /// Network Usage measure. Calculates the number of bytes sent and received over the network by process.
    /// </summary>
    internal class NetworkUsage : MeasureSet
    {
        public NetworkUsage()
        {
            _wpaProfile = @".\MeasureSetDefinitionAssets\NetworkUsage.wpaProfile";
            WprpFile = @".\MeasureSetDefinitionAssets\NetworkUsage.wprp";
            TracingMode = TraceCaptureMode.File;
            Name = "networkUsage";
            _wpaExportedDataFileNames = new List<string>() { "TcpIp_Events_Throughput_over_Time.csv" };
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
                                      select new { ProcessName = fields[1], EventType = fields[2], Protocol = fields[3], NumBytes = Convert.ToUInt64(fields[4]) };

            // Compute the disk usage aggregated by process name and IO type.
            var networkUsageData = from row in rawNetworkUsageData
                                   group row by new { ProcessName = row.ProcessName, EventType = row.EventType } into g
                                   orderby g.Key.ProcessName, g.Key.EventType
                                   select new { ProcessName = g.Key.ProcessName, EventType = g.Key.EventType, NumBytes = g.Sum(b => (decimal)b.NumBytes) };

            if (networkUsageData.Count() == 0)
            {
                return null;
            }

            // Format the received usage bytes results to metric form.
            var receivedBytes = (from row in networkUsageData
                                 where row.EventType == "Receive"
                                 select new KeyValuePair<string, string>("Received Bytes | " + row.ProcessName, row.NumBytes.ToString())).ToDictionary(k => k.Key, v => v.Value);

            // Format the received usage bytes results to metric form.
            var sentBytes = (from row in networkUsageData
                             where row.EventType == "Send"
                             select new KeyValuePair<string, string>("Sent Bytes | " + row.ProcessName, row.NumBytes.ToString())).ToDictionary(k => k.Key, v => v.Value);

            metrics = receivedBytes;

            foreach (var item in sentBytes)
            {
                metrics.Add(item.Key, item.Value);
            }

            return metrics;
        }
    }
}
