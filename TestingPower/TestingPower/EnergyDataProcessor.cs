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
        private List<E3EnergyEstimateEvent> _E3EnergyEstimateEvents;
        private List<E3UnknownEnergyEvent> _E3UnknownEnergyEvents;
        public List<E3BrowserTestRunEnergyByProcess> TestPassProcessEnergy;
        public List<E3BrowserTestRunEnergyByComponent> TestPassComponentEnergy;

        /// <summary>
        /// Initializes a new instance of the EnergyDataProcessor class.
        /// </summary>
        public EnergyDataProcessor()
        {
            _E3EnergyEstimateEvents = new List<E3EnergyEstimateEvent>();
            _E3UnknownEnergyEvents = new List<E3UnknownEnergyEvent>();
            TestPassProcessEnergy = new List<E3BrowserTestRunEnergyByProcess>();
            TestPassComponentEnergy = new List<E3BrowserTestRunEnergyByComponent>();
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
            string[] nameTokens = null;
            Dictionary<string, E3EnergyData> e3EnergyByProcess = null;
            E3EnergyData e3EnergyByComponent;
            string etlFileName = "";

            LoadEnergyEventsFromFile(energyCsvFile);

            e3EnergyByProcess = AggregateEnergyDataByProcess(_E3EnergyEstimateEvents);
            e3EnergyByComponent = AggregateEnergyDataByComponent(_E3EnergyEstimateEvents);

            // the filename contains the browser, scenario, iteration, datestamp and timestamp information split by underscores
            nameTokens = Path.GetFileNameWithoutExtension(energyCsvFile).Split('_');

            // the original ETL filename is the same as the energy CSV file but with a different extension
            etlFileName = Path.ChangeExtension(energyCsvFile, ".etl");

            E3BrowserTestRunEnergyByComponent testRunEnergyByComponent = new E3BrowserTestRunEnergyByComponent();
            testRunEnergyByComponent.EtlFileName = etlFileName;
            testRunEnergyByComponent.Scenario = nameTokens[1];
            testRunEnergyByComponent.Iteration = Convert.ToInt32(nameTokens[2]);
            testRunEnergyByComponent.Browser = nameTokens[0];
            testRunEnergyByComponent.DateStamp = nameTokens[3];
            testRunEnergyByComponent.TimeStamp = nameTokens[4];
            testRunEnergyByComponent.E3ComponentEnergy = e3EnergyByComponent;

            E3BrowserTestRunEnergyByProcess testRunEnergyByProcess = new E3BrowserTestRunEnergyByProcess();
            testRunEnergyByProcess.EtlFileName = etlFileName;
            testRunEnergyByProcess.Scenario = nameTokens[1];
            testRunEnergyByProcess.Iteration = Convert.ToInt32(nameTokens[2]);
            testRunEnergyByProcess.Browser = nameTokens[0];
            testRunEnergyByProcess.DateStamp = nameTokens[3];
            testRunEnergyByProcess.TimeStamp = nameTokens[4];
            testRunEnergyByProcess.E3EnergyByProcess = e3EnergyByProcess;

            TestPassProcessEnergy.Add(testRunEnergyByProcess);
            TestPassComponentEnergy.Add(testRunEnergyByComponent);
        }

        // Aggregates the energy data by process name. 
        // The resulting data is stored in the EnergyByProcessName property.
        private static Dictionary<string, E3EnergyData> AggregateEnergyDataByProcess(List<E3EnergyEstimateEvent> e3EnergyEstimateEvents)
        {
            // Make sure we have data to work with.
            if (e3EnergyEstimateEvents.Count == 0)
            {
                throw new Exception("No E3 Energy data to aggregate!");
            }

            var processEnergyData = (from e in e3EnergyEstimateEvents
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

            return processEnergyData;
        }

        // Aggregates the energy data by system component. 
        // The resulting data is stored in the EnergyByComponent property.
        private static E3EnergyData AggregateEnergyDataByComponent(List<E3EnergyEstimateEvent> e3EnergyEstimateEvents)
        {
            // Make sure we have data to work with.
            if (e3EnergyEstimateEvents.Count == 0)
            {
                throw new Exception("No E3 Energy data to aggregate!");
            }

            E3EnergyData E3ComponentEnergy = new E3EnergyData() {
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
                TimeInMSec = (uint)e3EnergyEstimateEvents.Sum(s => s.TimeInMSec)
            };

            return E3ComponentEnergy;
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
        /// Saves the processed component and process energy data to two csv files.
        /// </summary>
        /// <param name="csvFileName">The name to prefix the saved csv files.</param>
        public void SaveToCsv(string csvFileName)
        {
            string e3ProcessDataCsvFileName = "";
            string e3ComponentDataCsvFileName = "";

            e3ProcessDataCsvFileName = System.IO.Path.Combine(csvFileName, "_E3ProcessData.csv");
            e3ComponentDataCsvFileName = System.IO.Path.Combine(csvFileName, "_E3ComponentData.csv");

            if (!SaveProcessEnergyToFile(e3ProcessDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the aggregated process energy data!");
            }

            if (SaveCompononentEnergyToFile(e3ComponentDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the aggregated component energy data!");
            }
        }

        /// <summary>
        /// Saves the energy data aggregated by process to a file in csv format.
        /// </summary>
        /// <param name="fileName">The name to save the file as.</param>
        /// <returns>Returns true if the data was successfully saved.</returns>
        public bool SaveProcessEnergyToFile(string fileName)
        {
            bool success = false;
            string dataRow = "";
            string headerRow = "";
            string testPassData = "";

            // first check that there is processed energy data to save
            if (TestPassProcessEnergy.Count == 0)
            {
                return false;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    headerRow = "EtlFileName,Scenario,Iteration,Browser,DateStamp,TimeStamp,ProcessName,CpuEnergy,SocEnergy,DisplayEnergy,DiskEnergy,NetworkEnergy,MbbEnergy,LossEnergy,OtherEnergy,EmiEnergy,TotalEnergy,TimeInMilliSec";
                    sw.WriteLine(headerRow);

                    foreach (var pass in TestPassProcessEnergy)
                    {
                        testPassData = pass.EtlFileName + "," + pass.Scenario + "," + pass.Iteration + "," + pass.Browser + "," + pass.DateStamp + "," + pass.TimeStamp;
                        foreach (var processEnergy in pass.E3EnergyByProcess)
                        {
                            dataRow = testPassData + "," + processEnergy.Key + "," + processEnergy.Value.CpuEnergy + "," + processEnergy.Value.SocEnergy + "," + processEnergy.Value.DisplayEnergy + "," + processEnergy.Value.DiskEnergy + "," + processEnergy.Value.NetworkEnergy + "," + processEnergy.Value.MbbEnergy + "," + processEnergy.Value.LossEnergy + "," + processEnergy.Value.OtherEnergy + "," + processEnergy.Value.EmiEnergy + "," + processEnergy.Value.TotalEnergy + "," + processEnergy.Value.TimeInMSec;
                            sw.WriteLine(dataRow);
                        }
                    }
                }
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception writing process energy to file. Exception Message: {0}", e.Message);
                return false;
            }

            return success;
        }

        /// <summary>
        /// Saves the energy data aggregated by component to a file in csv format.
        /// </summary>
        /// <param name="fileName">The name to save the file as.</param>
        /// <returns>Returns true if the data was successfully saved.</returns>
        public bool SaveCompononentEnergyToFile(string fileName)
        {
            bool success = false;
            string dataRow = "";
            string headerRow = "";

            // first check that there is component energy data to save
            if (TestPassComponentEnergy.Count == 0)
            {
                return false;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    headerRow = "EtlFileName,Scenario,Iteration,Browser,DateStamp,TimeStamp,CpuEnergy,SocEnergy,DisplayEnergy,DiskEnergy,NetworkEnergy,MbbEnergy,LossEnergy,OtherEnergy,EmiEnergy,TotalEnergy,TimeInMilliSec";
                    sw.WriteLine(headerRow);

                    foreach (var component in TestPassComponentEnergy)
                    {
                        dataRow = component.EtlFileName + "," + component.Scenario + "," + component.Iteration + "," + component.Browser + "," + component.DateStamp + "," + component.TimeStamp + "," + component.E3ComponentEnergy.CpuEnergy + "," + component.E3ComponentEnergy.SocEnergy + "," + component.E3ComponentEnergy.DisplayEnergy + "," + component.E3ComponentEnergy.DiskEnergy + "," + component.E3ComponentEnergy.NetworkEnergy + "," + component.E3ComponentEnergy.MbbEnergy + "," + component.E3ComponentEnergy.LossEnergy + "," + component.E3ComponentEnergy.OtherEnergy + "," + component.E3ComponentEnergy.EmiEnergy + "," + component.E3ComponentEnergy.TotalEnergy + "," + component.E3ComponentEnergy.TimeInMSec;
                        sw.WriteLine(dataRow);
                    }
                }
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception writing component energy to file. Exception Message: {0}", e.Message);
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
