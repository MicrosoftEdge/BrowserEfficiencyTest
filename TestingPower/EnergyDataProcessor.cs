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

        /// <summary>
        /// Gets the E3 energy data aggregated by process name.
        /// </summary>
        /// <Returns>
        /// A dictionary with the process name as the key and aggregated energy data as the value.
        /// </Returns>
        public Dictionary<string, E3EnergyEstimateEvent> EnergyByProcessName;

        /// <summary>
        /// Gets the summed E3 energy data by system component.
        /// </summary>
        /// <Returns>
        /// A dictionary with the system component as the key and summed energy data as the value.
        /// </Returns>
        public Dictionary<string, uint> EnergyByComponent;

        /// <summary>
        /// Initializes a new instance of the EnergyDataProcessor class.
        /// </summary>
        public EnergyDataProcessor()
        {
            _E3EnergyEstimateEvents = new List<E3EnergyEstimateEvent>();
            _E3UnknownEnergyEvents = new List<E3UnknownEnergyEvent>();
            EnergyByProcessName = new Dictionary<string, E3EnergyEstimateEvent>();
            EnergyByComponent = new Dictionary<string, uint>();
        }

        /// <summary>
        /// Processes the E3 energy data from the passed in csv file containing E3 event data. 
        /// Processing includes:
        ///   - Aggregating the energy data by process name
        ///   - Aggregating the energy data by system component
        /// The csv file can be created from an ETL file using XPerf with the 'dumper' action.
        /// </summary>        
        /// <param name="energyCsvFile">The filename containing the E3 event data in CSV format.</param>
        /// <returns>true if the energy data was successfully read from the csv file and processed.</returns>
        public bool ProcessEnergyData(string energyCsvFile)
        {
            bool success = false;

            success = LoadEnergyEventsFromFile(energyCsvFile);

            AggregateEnergyDataByProcess();
            AggregateEnergyDataByComponent();

            return success;
        }


        // Aggregates the energy data by process name. 
        // The resulting data is stored in the EnergyByProcessName property.
        private bool AggregateEnergyDataByProcess()
        {
            bool success = false;

            // Make sure we have data to work with.
            if (_E3EnergyEstimateEvents.Count == 0)
            {
                return false;
            }

            var processEnergyData = (from e in _E3EnergyEstimateEvents
                                     group e by e.ProcessName into g
                                     select new
                                     {
                                         process = g.Key,
                                         data = new E3EnergyEstimateEvent()
                                         {
                                             ProcessName = g.Key,
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
                                             TotalEnergy = (uint)g.Sum(s => s.TotalEnergy),
                                             RecordFlags = g.First().RecordFlags,
                                             RecordMeasured = g.First().RecordMeasured,
                                             Committed = g.First().Committed
                                         }
                                     }).ToDictionary(k => k.process, d => d.data);

            EnergyByProcessName = processEnergyData;

            success = true;

            return success;
        }

        // Aggregates the energy data by system component. 
        // The resulting data is stored in the EnergyByComponent property.
        private bool AggregateEnergyDataByComponent()
        {
            bool success = false;
            Dictionary<string, uint> componentEnergy = new Dictionary<string, uint>();

            // Make sure we have data to work with.
            if (_E3EnergyEstimateEvents.Count == 0)
            {
                return false;
            }

            componentEnergy.Add("CpuEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.CpuEnergy));
            componentEnergy.Add("SocEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.SocEnergy));
            componentEnergy.Add("DisplayEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.DisplayEnergy));
            componentEnergy.Add("DiskEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.DiskEnergy));
            componentEnergy.Add("NetworkEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.NetworkEnergy));
            componentEnergy.Add("MbbEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.MbbEnergy));
            componentEnergy.Add("LossEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.LossEnergy));
            componentEnergy.Add("OtherEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.OtherEnergy));
            componentEnergy.Add("EmiEnergy", (uint)_E3EnergyEstimateEvents.Sum(s => s.EmiEnergy));

            EnergyByComponent = componentEnergy;

            success = true;

            return success;
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
        /// Saves all the processed energy data to various csv files using the passed in csvFileName as the prefix for the filename of the csvs.
        /// </summary>
        /// <param name="csvFileName">The name to prefix the saved csv files.</param>
        public void SaveToCsv(string csvFileName)
        {
            string e3ProcessDataCsvFileName = "";
            string e3ComponentDataCsvFileName = "";

            e3ProcessDataCsvFileName = System.IO.Path.Combine(csvFileName, "_E3ProcessData.csv");
            e3ComponentDataCsvFileName = System.IO.Path.Combine(csvFileName, "_E3ComponentData.csv");

            if (!SaveProcessEnergyDataToCsv(e3ProcessDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the aggregated process energy data!");
            }

            if (SaveCompononentEnergyDataToCsv(e3ComponentDataCsvFileName))
            {
                Console.WriteLine("There was a problem saving the aggregated component energy data!");
            }
        }

        /// <summary>
        /// Saves the energy data aggregated by process to a file in csv format.
        /// </summary>
        /// <param name="fileName">The name to save the file as.</param>
        /// <returns>Returns true if the data was successfully saved.</returns>
        public bool SaveProcessEnergyDataToCsv(string fileName)
        {
            bool success = false;
            string dataRow = "";
            string headerRow = "";

            // first check that there is processed energy data to save
            if (EnergyByProcessName.Count == 0)
            {
                return false;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    headerRow = "ProcessName,CpuEnergy,SocEnergy,DisplayEnergy,DiskEnergy,NetworkEnergy,MbbEnergy,LossEnergy,OtherEnergy,EmiEnergy,TotalEnergy,TimeInMilliSec,RecordFlags,RecordMeasured";
                    sw.WriteLine(headerRow);

                    foreach (var processData in EnergyByProcessName)
                    {
                        dataRow = processData.Key + "," + processData.Value.CpuEnergy + "," + processData.Value.SocEnergy + "," + processData.Value.DisplayEnergy + "," + processData.Value.DiskEnergy + "," + processData.Value.NetworkEnergy + "," + processData.Value.MbbEnergy + "," + processData.Value.LossEnergy + "," + processData.Value.OtherEnergy + "," + processData.Value.EmiEnergy + "," + processData.Value.TotalEnergy + "," + processData.Value.TimeInMSec + "," + processData.Value.RecordFlags + "," + processData.Value.RecordMeasured;
                        sw.WriteLine(dataRow);
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
        public bool SaveCompononentEnergyDataToCsv(string fileName)
        {
            bool success = false;
            string dataRow = "";
            string headerRow = "";

            // first check that there is component energy data to save
            if (EnergyByComponent.Count == 0)
            {
                return false;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    foreach (var component in EnergyByComponent)
                    {
                        headerRow += component.Key + ",";
                        dataRow += component.Value + ",";
                    }

                    sw.WriteLine(headerRow);
                    sw.WriteLine(dataRow);
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
}
