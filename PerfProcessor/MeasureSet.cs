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
using System.IO;
using System.Text.RegularExpressions;

namespace BrowserEfficiencyTest
{
    public enum TraceCaptureMode { Memory = 0x01, File = 0x02 }

    /// <summary>
    /// Base class for all measures. This class provides implementation details for using WPAExporter to 
    /// extract data and load it from a csv file.
    /// All measures must inherit from this class and implement the ProcessData.
    /// </summary>
    public abstract class MeasureSet
    {
        private AutomateWPAExporter _wpaExporter;

        /// <summary>
        /// A list of the Windows Performance Analyzer(WPA) Profiles (.wpaprofile) to use when extracting data from an ETL.
        /// </summary>
        protected List<string> _wpaProfiles;

        /// <summary>
        /// The Windows Performance Analyzer(WPA) Region name to use when extracting data from an ETL.
        /// </summary>
        public string WpaRegionName;

        /// <summary>
        /// The Windows Performance Recorder(WPR) Profile to use when recording a trace session for this measure.
        /// </summary>
        public string WprProfile
        {
            get;
            protected set;
        }

        /// <summary>
        /// The trace capture mode to use when recording a trace session for this measure.
        /// </summary>
        public TraceCaptureMode TracingMode
        {
            get;
            protected set;
        }

        /// <summary>
        /// Name of the measure.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// The csv file names that are exported by the specified WPA Profile. The csv file name
        /// is determine by the WPAProfile and WPA.
        /// </summary>
        protected List<string> _wpaExportedDataFileNames;

        /// <summary>
        /// The base default constructor.
        /// </summary>
        protected MeasureSet()
        {
            _wpaExporter = new AutomateWPAExporter();

            // _wpaRegionName is optional so instantiate it to empty.
            WpaRegionName = "";
        }

        /// <summary>
        /// Executes the measure processing for this measure using the specified ETL.
        /// This method extracts the raw data from the ETL using the WPAProfile, loads
        /// the data from a csv file and calculates the metrics from the data for the measure.
        /// </summary>
        /// <param name="etlFileName">The ETL file name to process for this measure.</param>
        /// <returns>A dictionary of metrics.</returns>
        public Dictionary<string, string> GetMetrics(string etlFileName)
        {
            Dictionary<string, string> metrics = null;
            Dictionary<string, List<string>> etlRawCsvDataSet = new Dictionary<string, List<string>>();
            List<string> etlRawCsvData = null;

            if (_wpaProfiles == null || _wpaProfiles.Count == 0 || string.IsNullOrEmpty(Name) || _wpaExportedDataFileNames.Count == 0)
            {
                throw new ArgumentException("Not all required members of MeasureSetDefinition have been defined by the child class.");
            }

            Console.WriteLine("[{0}] - Processing ETL {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), etlFileName);

            // Dump the data from the ETL File using the specified wpaProfiles for the measureset.
            foreach (var wpaProfile in _wpaProfiles)
            {
                Console.WriteLine("[{0}] -   Exporting profile {1} data from ETL {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), wpaProfile, etlFileName);
                _wpaExporter.WPAExport(etlFileName, wpaProfile, WpaRegionName);
            }
            // Load the data from all the csv files exported above.
            foreach (string wpaExportedDataFile in _wpaExportedDataFileNames)
            {
                // Get the data from the csv file.
                etlRawCsvData = GetDataFromFile(wpaExportedDataFile);

                // Only add the raw data to our working dataset if there is actual data.
                if (etlRawCsvData != null)
                {
                    etlRawCsvDataSet.Add(wpaExportedDataFile, etlRawCsvData);
                }

                // We don't need the file anymore so delete it.
                File.Delete(wpaExportedDataFile);
            }

            // Only calculate metrics if we have data to calculate the metrics from.
            if (etlRawCsvDataSet.Count > 0)
            {
                metrics = CalculateMetrics(etlRawCsvDataSet);
            }

            return metrics;
        }

        /// <summary>
        /// Processes the raw csv data for the measure and returns a dictionary of metrics.
        /// </summary>
        /// <param name="csvData">The raw csv data to process.</param>
        /// <returns>A dictionary of metrics.</returns>
        protected abstract Dictionary<string, string> CalculateMetrics(Dictionary<string, List<string>> csvData);

        /// <summary>
        /// Splits the csv string and removes quotes and any commas that are in between a pair of quotes.
        /// </summary>
        /// <param name="csvString"></param>
        /// <returns>An array of strings representing the split csv data.</returns>
        protected static string[] SplitCsvString(string csvString)
        {
            string[] tokens = null;
            string cleanedCsvString = "";

            // 1) The regular expression (\"[^\",]+,[^\"]+\") finds all the substrings of quoted text containing commas
            // 2) The matched quoted substrings has all commas removed by replacing them with an empty string (string.Empty)
            // 3) The matched quoted substrings with their commas removed are placed back in the original string over their original substrings
            // 4) The reformed string is then cleaned of any quotes by having them replaced with an empty string.
            cleanedCsvString = (Regex.Replace(csvString, "(\"[^\",]+,[^\"]+\")", m => m.Value.Replace(",", string.Empty))).Replace("\"", string.Empty);

            tokens = cleanedCsvString.Split(',');

            return tokens;
        }

        /// <summary>
        /// Reads in a file as text and returns the text as a list of strings.
        /// </summary>
        /// <param name="csvFileName">The file name of the csv file to load data from.</param>
        /// <returns>A list of strings containing the raw data from the csv file.</returns>
        protected static List<string> GetDataFromFile(string csvFileName)
        {
            List<string> csvData = new List<string>();
            string line;

            if (!File.Exists(csvFileName))
            {
                return null;
            }

            try
            {
                using (StreamReader sr = new StreamReader(csvFileName))
                {
                    // read in the header row
                    sr.ReadLine();

                    // read the data in from the exported data file
                    while ((line = sr.ReadLine()) != null)
                    {
                        csvData.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in reading csv file!  " + ex.Message);
                return null;
            }

            return csvData;
        }
    }
}
