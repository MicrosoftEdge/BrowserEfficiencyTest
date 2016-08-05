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
using System.IO;
using System.Linq;

namespace TestingPower
{
    /// <summary>
    /// Processes the energy data from E3 event data.
    /// Processing includes:
    ///   - Aggregating the energy data by process name
    ///   - Aggregating the energy data by system component
    /// </summary>
    internal class EnergyDataProcessor
    {
        // TODO: Move EnergyDataProcessor to its own project and make it a DLL.
        // TODO: Refactor to improve the interface. As it is now it is clunky and requires code using this class to know about some of its inner mechanics.
        // TODO: Refactor the structs used by EnergyDataProcessor (E3BrowserTestRunEnergyByProcess, E3BrowserTestRunEnergyByComponent, E3EnergyData) to be classes.
        private List<E3EnergyEstimateEvent> _E3EnergyEstimateEvents;
        private List<E3UnknownEnergyEvent> _E3UnknownEnergyEvents;
        public List<E3BrowserTestRunEnergyByProcess> TestPassProcessEnergy;
        public List<E3BrowserTestRunEnergyByComponent> TestPassComponentEnergy;
        public List<E3BrowserTestRunEnergyByProcess> TestPassHardwareMeasuredEnergy;

        /// <summary>
        /// Initializes a new instance of the EnergyDataProcessor class.
        /// </summary>
        public EnergyDataProcessor()
        {
            _E3EnergyEstimateEvents = new List<E3EnergyEstimateEvent>();
            _E3UnknownEnergyEvents = new List<E3UnknownEnergyEvent>();
            TestPassProcessEnergy = new List<E3BrowserTestRunEnergyByProcess>();
            TestPassComponentEnergy = new List<E3BrowserTestRunEnergyByComponent>();
            TestPassHardwareMeasuredEnergy = new List<E3BrowserTestRunEnergyByProcess>();
        }

        /// <summary>
        /// Processes the E3 energy data from the passed in csv file containing E3 event data. 
        /// Processing includes:
        ///   - Aggregating the energy data by process name
        ///   - Aggregating the energy data by system component
        /// The csv file can be created from an ETL file using XPerf with the 'dumper' action.
        /// </summary>        
        /// <param name="energyCsvFile">The filename containing the E3 event data in CSV format.</param>
        public void ProcessEnergyData(string energyCsvFile)
        {
            List<E3EnergyEstimateEvent> hardwareMeasuredE3Events = null;
            List<E3EnergyEstimateEvent> filteredE3EstimateEvents = null;
            string[] nameTokens = null;
            Dictionary<string, E3EnergyData> e3EnergyByProcess = null;
            Dictionary<string, E3EnergyData> hardwareE3EnergyByMeter = null;
            E3EnergyData e3EnergyByComponent;
            string etlFileName = "";

            LoadEnergyEventsFromFile(energyCsvFile);

            // We need to separate any hardware energy measurements from the rest of the E3 energy data to prevent
            // them from being aggregated and summed with the rest of the E3 energy data resulting in erroneous
            // energy values.
            // Hardware measurements are reported by E3 as a process with the names beginning with "EMI_".
            hardwareMeasuredE3Events = (from e in _E3EnergyEstimateEvents
                                       where e.ProcessName.StartsWith("EMI_")
                                       select e).ToList();

            filteredE3EstimateEvents = (from e in _E3EnergyEstimateEvents
                                        where !e.ProcessName.StartsWith("EMI_")
                                        select e).ToList();

            // perform the various data aggregation tasks
            e3EnergyByProcess = AggregateEnergyDataByProcess(filteredE3EstimateEvents);
            e3EnergyByComponent = AggregateEnergyDataByComponent(filteredE3EstimateEvents);
            hardwareE3EnergyByMeter = AggregateEnergyDataByProcess(hardwareMeasuredE3Events);

            // the filename contains the browser, scenario, iteration, datestamp and timestamp information split by underscores
            nameTokens = Path.GetFileNameWithoutExtension(energyCsvFile).Split('_');

            // the original ETL filename is the same as the energy CSV file but with a different extension
            etlFileName = Path.ChangeExtension(energyCsvFile, ".etl");

            // store the aggregated energy data combined with the test run information
            E3BrowserTestRunEnergyByComponent testRunEnergyByComponent = new E3BrowserTestRunEnergyByComponent(etlFileName, nameTokens[1], Convert.ToInt32(nameTokens[2]), nameTokens[0], nameTokens[3], nameTokens[4], e3EnergyByComponent);
            E3BrowserTestRunEnergyByProcess testRunEnergyByProcess = new E3BrowserTestRunEnergyByProcess(etlFileName, nameTokens[1], Convert.ToInt32(nameTokens[2]), nameTokens[0], nameTokens[3], nameTokens[4], e3EnergyByProcess);
            E3BrowserTestRunEnergyByProcess testRunHardwareEnergyByMeter = new E3BrowserTestRunEnergyByProcess(etlFileName, nameTokens[1], Convert.ToInt32(nameTokens[2]), nameTokens[0], nameTokens[3], nameTokens[4], hardwareE3EnergyByMeter);

            // add the test run data in with the rest of the complete test pass data set
            TestPassProcessEnergy.Add(testRunEnergyByProcess);
            TestPassComponentEnergy.Add(testRunEnergyByComponent);
            TestPassHardwareMeasuredEnergy.Add(testRunHardwareEnergyByMeter);
        }

        // Aggregates the energy data by process name. 
        // The resulting data is stored in the EnergyByProcessName property.
        private static Dictionary<string, E3EnergyData> AggregateEnergyDataByProcess(List<E3EnergyEstimateEvent> e3EnergyEstimateEvents)
        {
            Dictionary<string, E3EnergyData> energyByProcess = new Dictionary<string, E3EnergyData>();

            // Make sure we have data to work with.
            if (e3EnergyEstimateEvents.Count == 0)
            {
                return energyByProcess;
            }

            energyByProcess = (from e in e3EnergyEstimateEvents
                                     group e by e.ProcessName into g
                                     select new {
                                         ProcessName = g.Key,
                                         EnergyData = new E3EnergyData() {
                                             CpuEnergy = (uint)g.Sum(s => s.CpuEnergy),
                                             SocEnergy = (uint)g.Sum(s => s.SocEnergy),
                                             DisplayEnergy = (uint)g.Sum(s => s.DisplayEnergy),
                                             DiskEnergy = (uint)g.Sum(s => s.DiskEnergy),
                                             NetworkEnergy = (uint)g.Sum(s => s.NetworkEnergy),
                                             MbbEnergy = (uint)g.Sum(s => s.MbbEnergy),
                                             LossEnergy = (uint)g.Sum(s => s.LossEnergy),
                                             OtherEnergy = (uint)g.Sum(s => s.OtherEnergy),
                                             EmiEnergy = (uint)g.Sum(s => s.EmiEnergy),
                                             TimeInMSec = (uint)g.Sum(s => s.TimeInMSec),
                                             TotalEnergy = (uint)g.Sum(s => s.TotalEnergy)
                                         }
                                     }).ToDictionary(k => k.ProcessName, d => d.EnergyData);

            return energyByProcess;
        }

        // Sums the energy data by system component. 
        private static E3EnergyData AggregateEnergyDataByComponent(List<E3EnergyEstimateEvent> e3EnergyEstimateEvents)
        {
            E3EnergyData componentEnergy = new E3EnergyData();

            // Make sure we have data to work with.
            if (e3EnergyEstimateEvents.Count == 0)
            {
                return componentEnergy;
            }

            componentEnergy = new E3EnergyData() {
                CpuEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.CpuEnergy),
                SocEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.SocEnergy),
                DisplayEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.DisplayEnergy),
                DiskEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.DiskEnergy),
                NetworkEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.NetworkEnergy),
                MbbEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.MbbEnergy),
                LossEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.LossEnergy),
                OtherEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.OtherEnergy),
                EmiEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.EmiEnergy),
                TotalEnergy = (uint)e3EnergyEstimateEvents.Sum(s => s.TotalEnergy),
                TimeInMSec = (uint)e3EnergyEstimateEvents.Sum(s => s.TimeInMSec)};

            return componentEnergy;
        }

        // Opens a csv file containing E3 event data and extracts the E3 events
        private bool LoadEnergyEventsFromFile(string csvEventsFile)
        {
            bool success = false;
            string line = "";
            string eventTag = "";

            int lineNumber = 1;
            int eventNameIndex = 0;

            // Re-instantiate the energy data lists since we are loading a new file.
            _E3EnergyEstimateEvents = new List<E3EnergyEstimateEvent>();
            _E3UnknownEnergyEvents = new List<E3UnknownEnergyEvent>();

            try
            {
                // read the events from the exported events data file
                using (StreamReader sr = new StreamReader(csvEventsFile))
                {
                    //first read past the headers
                    while (((line = sr.ReadLine()) != null) && !(line.Equals("EndHeader")))
                    {
                        lineNumber++;
                    }

                    // done reading past the headers now comes the event data 
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineNumber++;
                        try
                        {
                            eventNameIndex = line.IndexOf(',');
                            if (eventNameIndex > 0)
                            {
                                eventTag = line.Substring(0, line.IndexOf(','));
                                if (eventTag == @"Microsoft-Windows-Energy-Estimation-Engine/QueryStats/EnergyEstimate")
                                {
                                    _E3EnergyEstimateEvents.Add(new E3EnergyEstimateEvent(line));
                                }
                                else if (eventTag == @"Microsoft-Windows-Energy-Estimation-Engine/UnknownEnergy/UnknownEnergy")
                                {
                                    _E3UnknownEnergyEvents.Add(new E3UnknownEnergyEvent(line));
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine("inner excpetion: " + innerEx.Message);
                            return false;
                        }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in AddCsvEventsFileToDB!:" + ex.Message);
                return false;
            }

            return success;
        }

        /// <summary>
        /// Saves the processed energy data to three files in csv format.
        ///  - Saves the aggregated component energy data to E3ComponentEnergyData_[DateStamp]_[TimeStamp].csv
        ///  - Saves the aggregated process energy data to E3ProcessEnergyData_[DateStamp]_[TimeStamp].csv
        ///  - Saves the hardware measured energy data to HardwareMeterEnergyData_[DateStamp]_[TimeStamp].csv
        /// </summary>
        /// <param name="filePath">The directory path of where to save the csv files.</param>
        public void SaveProcessedDataToFiles(string filePath)
        {
            string e3ProcessDataCsvFileName = System.IO.Path.Combine(filePath, "E3ProcessEnergyData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            string e3ComponentDataCsvFileName = System.IO.Path.Combine(filePath, "E3ComponentEnergyData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            string e3HardwareMeterDataCsvFileName = System.IO.Path.Combine(filePath, "HardwareMeterEnergyData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

            if (!SaveProcessEnergyToFile(e3ProcessDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the aggregated process energy data!");
            }

            if (!SaveComponentEnergyToFile(e3ComponentDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the aggregated component energy data!");
            }

            if (!SaveHardwareMeterEnergyToFile(e3HardwareMeterDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the hardware meter energy data!");
            }
        }

        // Formats the hardware measured energy as CSV strings and saves the data to a file.
        private bool SaveHardwareMeterEnergyToFile(string fileName)
        {
            bool success = false;
            List<string> dataRows = null;
            string headerRow = "EtlFileName,Scenario,Iteration,Browser,DateStamp,TimeStamp,HardwareMeter,CpuEnergy,SocEnergy,DisplayEnergy,DiskEnergy,NetworkEnergy,MbbEnergy,LossEnergy,OtherEnergy,EmiEnergy,TotalEnergy,TimeInMilliSec";

            // Check that there is hardware measured energy data to save.
            if (TestPassHardwareMeasuredEnergy.Count == 0)
            {
                return false;
            }

            // Format the data as strings of comma separated values.
            dataRows = (from testRun in TestPassHardwareMeasuredEnergy
                        from process in testRun.E3EnergyByProcess
                        select testRun.EtlFileName + "," + testRun.Scenario + "," + testRun.Iteration + "," + testRun.Browser + "," + testRun.DateStamp + "," + testRun.TimeStamp + "," + process.Key + "," + process.Value.CpuEnergy + "," + process.Value.SocEnergy + "," + process.Value.DisplayEnergy + "," + process.Value.DiskEnergy + "," + process.Value.NetworkEnergy + "," + process.Value.MbbEnergy + "," + process.Value.LossEnergy + "," + process.Value.OtherEnergy + "," + process.Value.EmiEnergy + "," + process.Value.TotalEnergy + "," + process.Value.TimeInMSec).ToList();

            success = SaveToFile(fileName, headerRow, dataRows);

            return success;
        }

        // Formats the energy data aggregated by process as CSV strings and saves the data to a file.
        private bool SaveProcessEnergyToFile(string fileName)
        {
            bool success = false;
            List<string> dataRows = null;
            string headerRow = "EtlFileName,Scenario,Iteration,Browser,DateStamp,TimeStamp,ProcessName,FriendlyName,CpuEnergy,SocEnergy,DisplayEnergy,DiskEnergy,NetworkEnergy,MbbEnergy,LossEnergy,OtherEnergy,EmiEnergy,TotalEnergy,TimeInMilliSec";

            // Check that there is process energy data to save.
            if (TestPassProcessEnergy.Count == 0)
            {
                return false;
            }

            // Format the data as strings of comma separated values.
            dataRows = (from testRun in TestPassProcessEnergy
                        from process in testRun.E3EnergyByProcess
                        select testRun.EtlFileName + "," + testRun.Scenario + "," + testRun.Iteration + "," + testRun.Browser + "," + testRun.DateStamp + "," + testRun.TimeStamp + "," + process.Key + "," + process.Key.Split('\\').Last() + "," + process.Value.CpuEnergy + "," + process.Value.SocEnergy + "," + process.Value.DisplayEnergy + "," + process.Value.DiskEnergy + "," + process.Value.NetworkEnergy + "," + process.Value.MbbEnergy + "," + process.Value.LossEnergy + "," + process.Value.OtherEnergy + "," + process.Value.EmiEnergy + "," + process.Value.TotalEnergy + "," + process.Value.TimeInMSec).ToList();

            success = SaveToFile(fileName, headerRow, dataRows);

            return success;
        }

        // Formats the energy data aggregated by component as CSV strings and saves the data a file.
        private bool SaveComponentEnergyToFile(string fileName)
        {
            bool success = false;
            List<string> dataRows = null;
            string headerRow = "EtlFileName,Scenario,Iteration,Browser,DateStamp,TimeStamp,CpuEnergy,SocEnergy,DisplayEnergy,DiskEnergy,NetworkEnergy,MbbEnergy,LossEnergy,OtherEnergy,EmiEnergy,TotalEnergy,TimeInMilliSec";

            // first check that there is component energy data to save
            if (TestPassComponentEnergy.Count == 0)
            {
                return false;
            }

            dataRows = (from testRun in TestPassComponentEnergy
                       select testRun.EtlFileName + "," + testRun.Scenario + "," + testRun.Iteration + "," + testRun.Browser + "," + testRun.DateStamp + "," + testRun.TimeStamp + "," + testRun.E3ComponentEnergy.CpuEnergy + "," + testRun.E3ComponentEnergy.SocEnergy + "," + testRun.E3ComponentEnergy.DisplayEnergy + "," + testRun.E3ComponentEnergy.DiskEnergy + "," + testRun.E3ComponentEnergy.NetworkEnergy + "," + testRun.E3ComponentEnergy.MbbEnergy + "," + testRun.E3ComponentEnergy.LossEnergy + "," + testRun.E3ComponentEnergy.OtherEnergy + "," + testRun.E3ComponentEnergy.EmiEnergy + "," + testRun.E3ComponentEnergy.TotalEnergy + "," + testRun.E3ComponentEnergy.TimeInMSec).ToList();

            success = SaveToFile(fileName, headerRow, dataRows);

            return success;
        }

        // Saves the passed in header and data rows as text to a file.
        private bool SaveToFile(string fileName, string headerRow, List<string> dataRows)
        {
            bool success = false;

            // If there is no file name, header row or data rows then exit without saving to file.
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(headerRow) || (dataRows == null))
            {
                return false;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.WriteLine(headerRow);

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

    /// <summary>
    /// Data container holding the E3 Energy data for a single browser and iteration test run. The data broken down by component.
    /// This data represents the energy from one ETL trace from a testingPower Test pass
    /// </summary>
    public struct E3BrowserTestRunEnergyByComponent
    {
        public string EtlFileName;
        public string Scenario;
        public int Iteration;
        public string Browser;
        public string DateStamp;
        public string TimeStamp;
        public E3EnergyData E3ComponentEnergy;

        public E3BrowserTestRunEnergyByComponent(string etlFileName, string scenario, int iteration, string browser, string dateStamp, string timeStamp, E3EnergyData e3ComponentEnergy)
        {
            EtlFileName = etlFileName;
            Scenario = scenario;
            Iteration = iteration;
            Browser = browser;
            DateStamp = dateStamp;
            TimeStamp = timeStamp;
            E3ComponentEnergy = e3ComponentEnergy;
        }
    }

    /// <summary>
    /// Data container holding the E3 Energy data for a single browser and iteration test run. The data is broken down by process.
    /// This data represents the energy from one ETL trace from a testingPower Test pass
    /// </summary>
    public struct E3BrowserTestRunEnergyByProcess
    {
        public string EtlFileName;
        public string Scenario;
        public int Iteration;
        public string Browser;
        public string DateStamp;
        public string TimeStamp;
        public Dictionary<string, E3EnergyData> E3EnergyByProcess;

        public E3BrowserTestRunEnergyByProcess(string etlFileName, string scenario, int iteration, string browser, string dateStamp, string timeStamp, Dictionary<string, E3EnergyData> e3EnergyByProcess)
        {
            EtlFileName = etlFileName;
            Scenario = scenario;
            Iteration = iteration;
            Browser = browser;
            DateStamp = dateStamp;
            TimeStamp = timeStamp;
            E3EnergyByProcess = e3EnergyByProcess;
        }
    }

    /// <summary>
    /// E3 data is broken across several components.
    /// This struct is the smallest collection unit of E3 Energy data.
    /// </summary>
    public struct E3EnergyData
    {
        public uint CpuEnergy;
        public uint SocEnergy;
        public uint DisplayEnergy;
        public uint DiskEnergy;
        public uint NetworkEnergy;
        public uint MbbEnergy;
        public uint LossEnergy;
        public uint OtherEnergy;
        public uint EmiEnergy;
        public uint TotalEnergy;
        public uint TimeInMSec;
    }
}
