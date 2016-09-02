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
    /// Memory Reference Set Usage measure. Calculates the number of megabytes for dynamic and file 
    /// reference set categories by process.
    /// </summary>
    internal class RefSet : MeasureSet
    {
        public RefSet()
        {
            _wpaProfile = @".\MeasureSetDefinitionAssets\RefSet.wpaProfile";
            WprpFile = @".\MeasureSetDefinitionAssets\RefSet.wprp";
            TracingMode = TraceCaptureMode.File;
            Name = "refSet";
            _wpaExportedDataFileNames = new List<string>() { "Reference_Set_RefSet_without_PagePool.csv" };
        }

        /// <summary>
        /// Calculates the number of megabytes for dynamic and file reference set categories by process.
        /// </summary>
        /// <param name="csvData">The raw csv data to use for calculating the number of megabytes for dynamic and file reference sets.</param>
        /// <returns>A dictionary of processes and the number of megabytes used by reference set category.</returns>
        protected override Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData)
        {
            Dictionary<string, string> metrics = null;

            // Process the raw string data into a usable format.
            var rawRefSetData = from row in csvData.First().Value
                                let fields = SplitCsvString(row)
                                select new { ProcessName = fields[1], CategoryClass = fields[2], PageCategory = fields[3], ImpactingSize = Convert.ToDecimal(fields[6]) };

            if (rawRefSetData.Count() == 0)
            {
                return null;
            }

            // Get the dynamic category of data. We only need the top level dynamic category class of refset data since 
            // all the sub levels of the data are already summed up to the top level.
            var dynamicCategory = (from row in rawRefSetData
                                   where row.CategoryClass == "Dynamic" && row.PageCategory == ""
                                   group row by row.ProcessName into g
                                   orderby g.Key
                                   select new KeyValuePair<string, string>("ImpactingSize(MB) | Dynamic | " + g.Key.ToString(), g.Sum(s => s.ImpactingSize).ToString())).ToDictionary(s => s.Key, s => s.Value);

            // Get the file category of data. We only need the top level file category class of refset data since 
            // all the sub levels of the data are already summed up to the top level.
            var fileCategory = (from row in rawRefSetData
                                where row.CategoryClass == "File" && row.PageCategory == ""
                                group row by row.ProcessName into g
                                orderby g.Key
                                select new KeyValuePair<string, string>("ImpactingSize(MB) | File | " + g.Key.ToString(), g.Sum(s => s.ImpactingSize).ToString())).ToDictionary(s => s.Key, s => s.Value);

            metrics = dynamicCategory;

            foreach (var item in fileCategory)
            {
                metrics.Add(item.Key, item.Value);
            }

            return metrics;
        }
    }
}
