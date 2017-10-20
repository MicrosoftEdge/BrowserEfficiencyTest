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
    /// Memory Reference Set Usage measure. Calculates the number of megabytes for dynamic and file 
    /// reference set categories by process.
    /// </summary>
    internal class RefSet : MeasureSet
    {
        public RefSet()
        {
            _wpaProfiles = new List<string>() { @".\MeasureSetDefinitionAssets\RefSet.wpaProfile" };
            WprProfile = "refSet";
            TracingMode = TraceCaptureMode.File;
            Name = "refSet";
            _wpaExportedDataFileNames = new List<string>() { "Reference_Set_RawData_without_PagePool.csv" };
        }

        /// <summary>
        /// Calculates the peak and average reference set size in bytes for the entire system during the trace session.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating the number of bytes for dynamic and file reference sets.</param>
        /// <returns>A dictionary of the Reference Set size and the number of bytes used by reference set category.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = null;
            long dynamicPeakRefSet = 0;
            long filePeakRefSet = 0;
            long systemPeakRefSet = 0;
            decimal dynamicPeakRefSetTimeMark = 0;
            decimal filePeakRefSetTimeMark = 0;
            decimal systemPeakRefSetTimeMark = 0;
            long dynamicRunningRefSetTotal = 0;
            long fileRunningRefSetTotal = 0;
            long systemRunningRefSetTotal = 0;
            decimal dynamicRefSetArea = 0;
            decimal fileRefSetArea = 0;
            decimal systemRefSetArea = 0;
            decimal systemPreviousTimeMark = 0;
            decimal dynamicPreviousTimeMark = 0;
            decimal filePreviousTimeMark = 0;
            decimal systemStartTimeMark = 0;
            decimal dynamicStartTimeMark = 0;
            decimal fileStartTimeMark = 0;
            decimal averageSystemRefSet = 0;
            decimal averageSystemDynamicRefSet = 0;
            decimal averageSystemFileRefSet = 0;

            var rawRefSetData = from row in csvData.First().Value
                                let fields = SplitCsvString(row)
                                select new { CategoryClass = fields[0], AccessTime = Convert.ToDecimal(fields[1]), ReleaseTime = Convert.ToDecimal(fields[2]), Size = Convert.ToInt64(fields[3]) };

            if (rawRefSetData.Count() == 0)
            {
                return null;
            }

            // Create a new list with three columns: TimeMark, CategoryClass, SizeDelta.
            // TimeMark will be created by combining the AccessTime and ReleaseTime columns from rawRefSetData.
            // CategoryClass will be the same as in rawRefSetData.
            // SizeDelta will be the Size column from rawRefSetData with the following caveat:
            //     - SizeDelta will be a positive value for TimeMark values that are from the AccessTime column of rawRefSetData
            //     - SizeDelta will be a negative value for TimeMark values that are from the ReleaseTime column of rawRefSetData

            // create the positive SizeDelta values for the full list.
            var accesses = from row in rawRefSetData
                           select new { TimeMark = row.AccessTime, CategoryClass = row.CategoryClass, SizeDelta = row.Size };

            // create the negative SizeDelta values for the full list.
            var releases = from row in rawRefSetData
                           select new { TimeMark = row.ReleaseTime, CategoryClass = row.CategoryClass, SizeDelta = -row.Size };

            // create the full list by concatenating accesses and releases together and order by TimeMark ascending.
            var fullList = accesses.Concat(releases).OrderBy(s => s.TimeMark);

            // Step through the full list adding the SizeDeltas to get the running reference set total.
            // Also calculate the area under each running refset total (system, dynamic, file) to use later when calculating the average.
            foreach (var row in fullList)
            {
                // Calculate the area of the rectangle under the systemRunningRefSetTotal curve. This will be used to calculate the average refset for all category classes.
                systemRefSetArea += (row.TimeMark - systemPreviousTimeMark) * systemRunningRefSetTotal;

                systemRunningRefSetTotal += row.SizeDelta;

                // check if this is the peak and store the value and time mark if it is the largest value so far.
                if (systemRunningRefSetTotal > systemPeakRefSet)
                {
                    systemPeakRefSet = systemRunningRefSetTotal;
                    systemPeakRefSetTimeMark = row.TimeMark;
                }

                if (row.CategoryClass.Equals("Dynamic"))
                {
                    // Calculate the area of the rectangle under the dynamicRunningRefSetTotal curve. This will be used to calculate the average dynamic refset.
                    dynamicRefSetArea += (row.TimeMark - dynamicPreviousTimeMark) * dynamicRunningRefSetTotal;

                    dynamicRunningRefSetTotal += row.SizeDelta;

                    // check if this is the peak and store the value and time mark if it is the largest value so far.
                    if (dynamicRunningRefSetTotal > dynamicPeakRefSet)
                    {
                        dynamicPeakRefSet = dynamicRunningRefSetTotal;
                        dynamicPeakRefSetTimeMark = row.TimeMark;
                    }
                    dynamicPreviousTimeMark = row.TimeMark;
                }

                if (row.CategoryClass.Equals("File"))
                {
                    // Calculate the area of the rectangle under the fileRunningRefSetTotal curve. This will be used to calculate the average file refset.
                    fileRefSetArea += (row.TimeMark - filePreviousTimeMark) * fileRunningRefSetTotal;

                    fileRunningRefSetTotal += row.SizeDelta;

                    // check if this is the peak and store the value and time mark if it is the largest value so far.
                    if (fileRunningRefSetTotal > filePeakRefSet)
                    {
                        filePeakRefSet = fileRunningRefSetTotal;
                        filePeakRefSetTimeMark = row.TimeMark;
                    }
                    filePreviousTimeMark = row.TimeMark;
                }

                systemPreviousTimeMark = row.TimeMark;
            }

            metrics = new Dictionary<string, string>();

            // calculate the values and record the metrics
            metrics.Add("Peak Reference Set Size (Bytes) | System | Total", systemPeakRefSet.ToString());
            metrics.Add("Peak Reference Set Time Occurrence (seconds) | System | Total", systemPeakRefSetTimeMark.ToString());

            metrics.Add("Peak Reference Set Size (Bytes) | System | Dynamic", dynamicPeakRefSet.ToString());
            metrics.Add("Peak Reference Set Time Occurrence (seconds) | System | Dynamic", dynamicPeakRefSetTimeMark.ToString());

            metrics.Add("Peak Reference Set Size (Bytes) | System | File", filePeakRefSet.ToString());
            metrics.Add("Peak Reference Set Time Occurrence (seconds) | System | File", filePeakRefSetTimeMark.ToString());

            // get the last time mark values for each category we are reporting (system, Dynamic, File)
            systemStartTimeMark = fullList.First().TimeMark;
            dynamicStartTimeMark = fullList.Where(s => s.CategoryClass == "Dynamic").First().TimeMark;
            fileStartTimeMark = fullList.Where(s => s.CategoryClass == "File").First().TimeMark;

            if ((systemPreviousTimeMark - systemStartTimeMark) > 0)
            {
                averageSystemRefSet = systemRefSetArea / (systemPreviousTimeMark - systemStartTimeMark);
            }

            if ((dynamicPreviousTimeMark - dynamicStartTimeMark) > 0)
            {
                averageSystemDynamicRefSet = dynamicRefSetArea / (dynamicPreviousTimeMark - dynamicStartTimeMark);
            }

            if ((filePreviousTimeMark - fileStartTimeMark) > 0)
            {
                averageSystemFileRefSet = fileRefSetArea / (filePreviousTimeMark - fileStartTimeMark);
            }

            metrics.Add("Average Reference Set Size (Bytes) | System | Total", averageSystemRefSet.ToString());
            metrics.Add("Average Reference Set Size (Bytes) | System | Dynamic", averageSystemDynamicRefSet.ToString());
            metrics.Add("Average Reference Set Size (Bytes) | System | File", averageSystemFileRefSet.ToString());

            return metrics;
        }
    }
}
