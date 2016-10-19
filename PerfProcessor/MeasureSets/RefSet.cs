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
            _wpaProfile = @".\MeasureSetDefinitionAssets\RefSet.wpaProfile";
            WprProfile = "refSet";
            TracingMode = TraceCaptureMode.File;
            Name = "refSet";
            _wpaExportedDataFileNames = new List<string>() { "Reference_Set_RefSet_without_PagePool.csv" };
        }

        /// <summary>
        /// Calculates the peak memory refset in bytes for the entire system during the trace session.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating the number of bytes for dynamic and file reference sets.</param>
        /// <returns>A dictionary of the number of bytes used per refset category class.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = null;

            // Process the raw string data into a usable format.
            var rawRefSetData = (from row in csvData.First().Value
                                let fields = SplitCsvString(row)
                                select new { CategoryClass = fields[0], PeakSizeMark = fields[2] }).ToDictionary( k => k.CategoryClass, v => v.PeakSizeMark );

            if (rawRefSetData.Count() == 0)
            {
                return null;
            }

            metrics = rawRefSetData.ToDictionary( k => "Peak RefSet Size (Bytes) | " + k.Key.ToString(), v => v.Value);

            return metrics;
        }
    }
}
