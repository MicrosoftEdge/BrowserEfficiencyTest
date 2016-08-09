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
    internal class Program
    {
        private static void Main(string[] args)
        {
            Arguments arguments = new Arguments(args);

            ScenarioRunner scenarioRunner = new ScenarioRunner(arguments);

            scenarioRunner.Run();

            ProcessEnergyData(arguments);
        }

        /// <summary>
        /// Extracts the E3 Energy data from ETL files created during the test, aggregates the data and saves it to csv files.
        /// </summary>
        private static void ProcessEnergyData(Arguments args)
        {
            if (args.UsingTraceController)
            {
                IEnumerable<string> etlFiles = null;
                AutomateXPerf xPerf = new AutomateXPerf();
                EnergyDataProcessor energyProcessor = new EnergyDataProcessor();

                Console.WriteLine("[{0}] - Starting processing of energy data. -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                etlFiles = Directory.EnumerateFiles(args.EtlPath, "*.etl");

                if (etlFiles.Count() == 0)
                {
                    Console.WriteLine("No ETL files were found! Unable to process E3 Energy data.");
                    return;
                }

                foreach (var etl in etlFiles)
                {
                    string csvFileName = Path.ChangeExtension(etl, ".csv");

                    xPerf.DumpEtlEventsToFile(etl, csvFileName);

                    energyProcessor.ProcessEnergyData(csvFileName);
                }

                // TODO: Refactor EnergyDatProcessor so that this method call is not needed.
                energyProcessor.ProcessDataByEtl();

                energyProcessor.SaveProcessedDataToFiles(args.EtlPath);

                Console.WriteLine("[{0}] - Completed processing of energy data. -", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }
}
