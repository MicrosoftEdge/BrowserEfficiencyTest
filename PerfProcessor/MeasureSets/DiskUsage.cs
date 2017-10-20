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
    /// Disk Usage measure. Calculates the number of bytes read from disk, the number of bytes written to disk,
    /// and the time spent by the disk serving read/write requests by process.
    /// </summary>
    internal class DiskUsage : MeasureSet
    {
        public DiskUsage()
        {
            _wpaProfiles = new List<string>() { @".\MeasureSetDefinitionAssets\DiskUsage.wpaProfile" };
            WprProfile = "diskUsage";
            TracingMode = TraceCaptureMode.Memory;
            Name = "diskUsage";
            _wpaExportedDataFileNames = new List<string>() { "Disk_Usage_SizeAndServiceTimeByIOType.csv" };
            WpaRegionName = "TraceActiveRegion";
        }

        /// <summary>
        /// Calculates the number of bytes read from disk, the number of bytes written to disk,
        /// and the time spent by the disk serving read/write requests by process.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating disk usage.</param>
        /// <returns>A dictionary of processes and their total bytes written to disk, total bytes read from disk,
        /// and the total disk service time.
        /// </returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = null;

            // Process the raw string data into a usable format.
            var rawDiskUsageData = from row in csvData.First().Value
                                   let fields = SplitCsvString(row)
                                   select new { IoType = fields[0], IoCount = Convert.ToUInt64(fields[1]), DiskServiceTime = Convert.ToDecimal(fields[2]), SizeInBytes = Convert.ToUInt64(fields[3]) };

            if (rawDiskUsageData.Count() == 0)
            {
                return null;
            }

            // Format the usage time results to metric form.
            var usageTime = (from row in rawDiskUsageData
                             select new KeyValuePair<string, string>("Total Disk Service Time(us) | " + row.IoType, row.DiskServiceTime.ToString())).ToDictionary(k => k.Key, v => v.Value);

            // Format the usage size in bytes to metric form.
            var usageSize = (from row in rawDiskUsageData
                             select new KeyValuePair<string, string>("Total Bytes | " + row.IoType, row.SizeInBytes.ToString())).ToDictionary(k => k.Key, v => v.Value);

            // Format the count of IOs to metric form.
            var ioCount = (from row in rawDiskUsageData
                           select new KeyValuePair<string, string>("IO Count | " + row.IoType, row.IoCount.ToString())).ToDictionary(k => k.Key, v => v.Value);

            metrics = usageTime;

            foreach (var item in usageSize)
            {
                metrics.Add(item.Key, item.Value);
            }

            foreach (var item in ioCount)
            {
                metrics.Add(item.Key, item.Value);
            }

            return metrics;
        }
    }
}
