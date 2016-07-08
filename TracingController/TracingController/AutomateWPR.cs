﻿//--------------------------------------------------------------
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

namespace TracingController
{
    /// <summary>
    /// Provides Windows tracing controls using Windows Performance Recorder (WPR).
    /// By default AutomateWPR assumes WPR has been installed on the test machine as part of the Windows Performance Toolkit.
    /// Other WPR.exe can optionally be specified by passing the path to the WPR.exe location during instantiation.
    ///
    /// NOTE: TestingPower.exe must be run elevated in order to allow control of WPR.exe. 
    /// </summary>
    internal class AutomateWPR
    {
        private string _wprExePath;
        private string _wprpFile;

        public string WprRecordingProfile
        {
            get
            {
                return _wprpFile;
            }
            set
            {
                _wprpFile = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AutomateWPR class with default settings.
        /// </summary>
        public AutomateWPR()
        {
            _wprExePath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\";
            _wprpFile = ""; // need a default wprp file
        }

        /// <summary>
        /// Initializes a new instance of the WprWrapper class with the specified wprp file, WPR.exe path, and ETL file name.
        /// </summary>        
        /// <param name="wprpFile">The wprp file to use for tracing.</param>
        /// <param name="wprPath">The WPR exe path and file name to use for controlling the WPR tracing.</param>
        public AutomateWPR(string wprpFile, string wprPath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\")
        {
            _wprExePath = wprPath;
            _wprpFile = wprpFile;
        }

        /// <summary>
        /// Starts a WPR recording session using the specified wprp file.
        /// </summary>
        /// <returns>True if the trace session was started successfully.</returns>
        public bool StartWPR()
        {
            bool isSuccess = false;

            string commandLine = "-start " + _wprpFile + " -filemode";

            isSuccess = this.RunWpr(commandLine);

            return isSuccess;
        }

        /// <summary>
        /// Stops the currently running WPR recording and saves it to the specified etl file name.
        /// </summary>
        /// <param name="etlFileName">The ETL file name to save the recording to.</param>
        /// <returns>True if WPR trace recording was stopped.</returns>
        public bool StopWPR(string etlFileName = "BrowserPower.etl")
        {
            bool isSuccess = false;

            string commandLine = "-stop " + etlFileName;

            isSuccess = this.RunWpr(commandLine);

            return isSuccess;
        }

        /// <summary>
        /// Cancels the currently running WPR recording session.
        /// </summary>
        /// <returns>True if WPR trace recording was cancelled.</returns>
        public bool CancelWPR()
        {
            bool isSuccess = false;

            string commandLine = "-cancel";

            isSuccess = this.RunWpr(commandLine, true);

            return isSuccess;
        }

        // executes the wpr.exe commandline with the passed in command line parameters
        private bool RunWpr(string cmdLine, bool ignoreError = false)
        {
            bool isSuccess = false;
            string errorOutput = "";

            string wprExe = System.IO.Path.Combine(_wprExePath, "wpr.exe");

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
