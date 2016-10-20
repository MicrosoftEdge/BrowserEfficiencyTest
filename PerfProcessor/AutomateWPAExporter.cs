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
using System.Diagnostics;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// Provides Windows Performance Analyzer (WPA) ETL trace data exporting controls using Windows Performance Analyzer Exporter (WPAExporter.exe).
    /// By default AutomateWPAExporter assumes WPAExporter has been installed on the test machine as part of the Windows Performance Toolkit.
    /// Other WPAExporter.exe can optionally be specified by passing the path to the WPR.exe location during instantiation.
    /// </summary>
    internal class AutomateWPAExporter
    {
        private string _wpaExporterExePath;

        /// <summary>
        /// Initializes a new instance of the AutomateWPAExporter class with the specified WPAExporter.exe path.
        /// </summary>        
        /// <param name="wprPath">The WPR exe path and file name to use for controlling the WPR tracing.</param>
        public AutomateWPAExporter(string wpaExporterPath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\")
        {
            _wpaExporterExePath = wpaExporterPath;
        }

        /// <summary>
        /// Calls WPAExporter.exe to export data tables from an ETL file.
        /// </summary>
        /// <returns>True if the data tables were successfully exported.</returns>
        /// <param name="etlFile">The ETL file to export from.</param>
        /// <param name="wpaExporterProfile">The .wpaProfile file to use for exporting data from the ETL file.</param>
        /// <param name="region">The name of the region to export if using regions.</param>
        /// <param name="outputFolder">The folder of where to place the file containig the xported data. Default is the current folder.</param>
        public bool WPAExport(string etlFile, string wpaExporterProfile, string region = "", string outputFolder = ".")
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(etlFile) || string.IsNullOrEmpty(wpaExporterProfile))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(region))
            {
                region = " -region " + region;
            }
            else
            {
                region = "";
            }

            string commandLine = "-i " + etlFile + region + " -profile " + wpaExporterProfile + " -outputFolder " + outputFolder;

            isSuccess = this.RunWpaExporter(commandLine, true);

            return isSuccess;
        }

        // executes the wpr.exe commandline with the passed in command line parameters
        private bool RunWpaExporter(string cmdLine, bool ignoreError = false)
        {
            bool isSuccess = false;
            string errorOutput = "";

            string wprExe = System.IO.Path.Combine(_wpaExporterExePath, "wpaexporter.exe");

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(wprExe);
                processInfo.Arguments = cmdLine;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;

                Process commandProcess = new Process();
                commandProcess.StartInfo = processInfo;
                commandProcess.Start();

                // capture any error output. We'll use this to throw an exception.
                errorOutput = commandProcess.StandardError.ReadToEnd();

                commandProcess.WaitForExit();

                if (!ignoreError)
                {
                    // output the standard error to the console window. The standard output is routed to the console window by default.
                    Console.WriteLine(errorOutput);

                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        throw new Exception(errorOutput.ToString());
                    }
                }

                isSuccess = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception trying to run wpr.exe!");
                Console.WriteLine(e.Message);

                return false;
            }

            return isSuccess;
        }
    }
}
