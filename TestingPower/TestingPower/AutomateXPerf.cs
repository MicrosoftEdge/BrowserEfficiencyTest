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
using System.Diagnostics;

namespace TestingPower
{
    /// <summary>
    /// Provides Windows tracing controls using XPerf.
    /// By default AutomateXPerf assumes XPerf has been installed on the test machine as part of the Windows Performance Toolkit.
    /// NOTE: TestingPower.exe must be run elevated in order to allow control of XPerf.exe.
    /// </summary>
    internal class AutomateXPerf
    {
        private string _XperfPath;

        /// <summary>
        /// Initializes a new instance of the AutomateXPerf class using the passed in path as the location of XPerf.
        /// </summary>
        /// <param name="xPerfPath">The folder path of where xperf.exe resides. Defaults to C:\Program Files (x86)\Window Kits\10\Windows Performance Toolkit\</param>
        public AutomateXPerf(string xPerfPath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\")
        {
            _XperfPath = System.IO.Path.Combine(xPerfPath, "xperf.exe");
        }

        /// <summary>
        /// Dumps the events in the passed in ETL file to a csv file.
        /// </summary>        
        /// <param name="etlFile">The ETL file to dump the events from.</param>
        /// <param name="csvFileName">The file name to save the dumped csv ETL events to.</param>
        public bool DumpEtlEventsToFile(string etlFile, string csvFileName)
        {
            bool success = false;

            string arguments = "-i " + etlFile + " -o " + csvFileName + " -a dumper";

            success = RunXperf(arguments);

            return success;
        }

        // executes the XPerf.exe commandline with the passed in command line parameters
        private bool RunXperf(string arguments)
        {
            bool success = false;
            string errorOutput = "";

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(_XperfPath);
                processInfo.Arguments = arguments;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;

                Process commandProcess = new Process();
                commandProcess.StartInfo = processInfo;
                commandProcess.Start();

                // capture any error output. We'll use this to throw an exception.
                errorOutput = commandProcess.StandardError.ReadToEnd();

                commandProcess.WaitForExit();

                // output the standard error to the console window. The standard output is routed to the console window by default.
                Console.WriteLine(errorOutput);

                if (!string.IsNullOrEmpty(errorOutput))
                {
                    throw new Exception(errorOutput.ToString());
                }

                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception trying to run Xperf!");
                Console.WriteLine(e.Message);
                return false;
            }

            return success;
        }
    }
}
