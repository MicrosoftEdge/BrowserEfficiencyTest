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
    /// Disk Usage measure. Calculates the number of bytes read from disk, the number of bytes written to disk,
    /// and the time spent by the disk serving read/write requests by process.
    /// </summary>
    internal class DiskUsage : MeasureSet
    {
        public DiskUsage()
        {
            _wpaProfile = @".\MeasureSetDefinitionAssets\DiskUsage.wpaProfile";
            WprpFile = @".\MeasureSetDefinitionAssets\DiskUsage.wprp";
            TracingMode = TraceCaptureMode.Memory;
            Name = "DiskUsage";
            _wpaExportedDataFileNames = new List<string>() { "Disk_Usage_DiskFileIO_ByProcess.csv" };
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
                                   select new { ProcessWithPID = fields[0], ProcessName = fields[1], IoType = fields[2], DiskServiceTime = Convert.ToDouble(fields[6]), NumBytes = Convert.ToUInt64(fields[7]) };

            // Compute the disk usage aggregated by process name and IO type.
            var diskUsageData = from row in rawDiskUsageData
                                group row by new { ProcessName = row.ProcessName, IoType = row.IoType } into g
                                orderby g.Key.ProcessName, g.Key.IoType
                                select new { ProcessName = g.Key.ProcessName, IOType = g.Key.IoType, DiskServiceTimeMicroSec = g.Sum(t => (decimal)t.DiskServiceTime), SizeInBytes = (ulong)(g.Sum(s => (double)s.NumBytes)) };

            if (diskUsageData.Count() == 0)
            {
                return null;
            }

            // Format the usage time results to metric form.
            var usageTime = (from row in diskUsageData
                             select new KeyValuePair<string, string>("Disk Service Time(us) | " + row.ProcessName + " | " + row.IOType, row.DiskServiceTimeMicroSec.ToString())).ToDictionary(k => k.Key, v => v.Value);

            // Format the read bytes results to metric form.
            var readBytes = (from row in diskUsageData
                             where row.IOType == "Read"
                             select new KeyValuePair<string, string>("Read Bytes | " + row.ProcessName, row.SizeInBytes.ToString())).ToDictionary(k => k.Key, v => v.Value);

            // Format the written bytes results to metric form.
            var writeBytes = (from row in diskUsageData
                              where row.IOType == "Write"
                              select new KeyValuePair<string, string>("Written Bytes | " + row.ProcessName, row.SizeInBytes.ToString())).ToDictionary(k => k.Key, v => v.Value);

            metrics = usageTime;

            foreach (var item in readBytes)
            {
                metrics.Add(item.Key, item.Value);
            }

            foreach (var item in writeBytes)
            {
                metrics.Add(item.Key, item.Value);
            }

            return metrics;
        }
    }
}
