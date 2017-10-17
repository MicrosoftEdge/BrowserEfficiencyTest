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
using System.Linq;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Exctracts and calculates various performance measure data from ETL files and saves the resulting
    /// performance data to disk.
    /// </summary>
    public class PerfProcessor
    {
        private AutomateWPAExporter _wpaExporter;

        private List<MeasureSet> _selectedMeasureSets;

        /// <summary>
        /// Contains a list of available measure sets.
        /// </summary>
        public static IReadOnlyDictionary<string, MeasureSet> AvailableMeasureSets = new Dictionary<string, MeasureSet>((new List<MeasureSet>()
        {
            new CpuUsage(),
            new DiskUsage(),
            new Energy(),
            new EnergyVerbose(),
            new NetworkUsage(),
            new RefSet(),
            new CpuUsageVerbose(),
            new Debug()
            // Add new MeasureSets here.
        }).ToDictionary(k => k.Name, v => v));

        /// <summary>
        /// Creates a PerfProcessor instance and selects the measure sets to use.
        /// </summary>
        /// <param name="measureSetNames">A list of measureset names to use by PerfProcessor.</param>
        public PerfProcessor(List<MeasureSet> measureSets)
        {
            _wpaExporter = new AutomateWPAExporter();
            _selectedMeasureSets = measureSets;
        }

        /// <summary>
        /// Executes the extraction and calculation of the performance data from various ETL files and saves the
        /// results to a csv file.
        /// </summary>
        /// <param name="etlFolderPath">Location of the ETL files to process the performance data from.</param>
        /// <param name="saveFolderPath">Location of where to save the results csv file.</param>
        /// <param name="resultsToAdd">Any already-measured results to include in the csv file</param>
        public void Execute(string etlFolderPath = ".", string saveFolderPath = "", List<string> resultsToAdd = null, Dictionary<string, string> extensionsNameAndVersion = null)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> results = null;
            List<string> formattedResults = null;
            IEnumerable<string> etlFiles = Directory.EnumerateFiles(etlFolderPath, "*.etl");

            Console.WriteLine("[{0}] - Starting performance processing. -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            results = ProcessEtls(etlFiles);

            formattedResults = FormatResults(results, extensionsNameAndVersion);

            if (resultsToAdd != null)
            {
                foreach (string result in resultsToAdd)
                {
                    formattedResults.Add(result);
                }
            }

            SaveResults(formattedResults, saveFolderPath);
        }

        // Executes the extraction and calculation of data from a collection of ETL files.
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> ProcessEtls(IEnumerable<string> etlFiles)
        {
            // format: Dictionary< "ETL File name", Dictionary< "measure set name", Dictionary< "metric name", "metric value" >>>
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> results = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            // format: Dictionary< "metric name", "metric value" >
            Dictionary<string, string> measureMetrics = null;

            if (etlFiles.Count() == 0)
            {
                Console.WriteLine("No ETL files were found. No trace-based results have been measured.");
                return null;
            }

            // Go through each ETL file one at a time.
            foreach (var etl in etlFiles)
            {
                // Find a matching measureset to use with the etl (ETL will have the measureset name in the title)
                var etlNameTokens = Path.GetFileName(etl).Split('_');

                var measureSet = _selectedMeasureSets.Find(s => s.WprProfile.Equals(etlNameTokens[3], StringComparison.OrdinalIgnoreCase));

                if (measureSet != null)
                {
                    measureMetrics = measureSet.GetMetrics(etl);

                    if (measureMetrics == null)
                    {
                        Console.WriteLine("No Metrics found for measure set {0} on ETL {1}.", measureSet.Name, etl);
                    }
                    else
                    {
                        results.Add(Path.GetFileNameWithoutExtension(etl), new Dictionary<string, Dictionary<string, string>>() { { measureSet.Name, measureMetrics } });
                    }
                }
            }

            return results;
        }

        // Format the results into a list of CSV strings.
        private List<string> FormatResults(Dictionary<string, Dictionary<string, Dictionary<string, string>>> results, Dictionary<string, string> extensionsNameAndVersion = null)
        {
            string headerRow = "EtlFileName,Scenario,Iteration,Browser,DateStamp,TimeStamp,MeasureSet,Measure,Result";
            List<string> formattedData = new List<string>();
            string dataRow = "";
            string[] etlNameTokens = null;
            string etlName = "";
            string scenario = "";
            string iteration = "";
            string browser = "";
            string dateStamp = "";
            string timeStamp = "";
            string measureSetName = "";
            string metricName = "";
            string metricValue = "";

            formattedData.Add(headerRow);

            if (results == null)
            {
                return formattedData;
            }

            // The results data format is Dictionary< "ETL File name" , Dictionary< "measure set name" , Dictionary< "metric name" , "metric value" >>>
            foreach (var etl in results)
            {
                etlName = etl.Key;

                // The etl filename contains the browser, scenario, iteration, measureSet, datestamp and timestamp information split by underscores
                etlNameTokens = etl.Key.Split('_');

                browser = etlNameTokens[0];
                scenario = etlNameTokens[1];
                iteration = etlNameTokens[2];
                dateStamp = etlNameTokens[4];
                timeStamp = etlNameTokens[5];

                foreach (var measureSet in etl.Value)
                {
                    measureSetName = measureSet.Key;

                    foreach (var metric in measureSet.Value)
                    {
                        metricName = metric.Key;
                        metricValue = metric.Value;
                        if (extensionsNameAndVersion != null && extensionsNameAndVersion.Count != 0)
                        {
                            foreach (var extension in extensionsNameAndVersion)
                            {
                                browser = browser + "|" + extension.Key + " " + extension.Value;
                            }
                        }

                        dataRow = etlName + "," + scenario + "," + iteration + "," + browser + "," + dateStamp + "," + timeStamp + "," + measureSetName + "," + metric.Key + "," + metric.Value;
                        formattedData.Add(dataRow);
                    }
                }
            }

            return formattedData;
        }

        // Saves the results data to a file in csv format.
        private void SaveResults(List<string> results, string saveFolderPath)
        {
            string resultsFileName = "";

            if (results == null || results.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(saveFolderPath))
            {
                saveFolderPath = Directory.GetCurrentDirectory();
            }

            resultsFileName = System.IO.Path.Combine(saveFolderPath, "PerformanceResults_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

            SaveToFile(results, resultsFileName);
        }

        // Saves a list of strings to a text file.
        private static bool SaveToFile(List<string> dataRows, string fileName)
        {
            bool success = false;

            // If there is no file name, header row or data rows then exit without saving to file.
            if (string.IsNullOrEmpty(fileName) || dataRows == null || dataRows.Count == 0)
            {
                return false;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    foreach (var row in dataRows)
                    {
                        sw.WriteLine(row);
                    }
                }
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception writing to file. Exception Message: {0}", e.Message);
                return false;
            }
            return success;
        }
    }
}
