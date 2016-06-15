using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TestingPower
{
    /// <summary>
    /// Provides Windows tracing controls using Windows Performance Recorder (WPR).
    /// By default AutomateWPR assumes WPR has been installed on the test machine as part of the Windows Performance Toolkit.
    /// Other WPR.exe can optionally be specified by passing the path to the WPR.exe location during instantiation.
    ///
    /// NOTE: TestingPower.exe must be run elevated in order to allow control of WPR.exe.
    /// 
    /// </summary>
    class AutomateWPR
    {
        private string wprExePath;
        private string wprpFile;

        public string WprRecordingProfile
        {
            get
            {
                return wprpFile;
            }
            set
            {
                this.wprpFile = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AutomateWPR class with default settings.
        /// </summary>
        public AutomateWPR()
        {
            this.wprExePath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe";            
            this.wprpFile = ""; // need a default wprp file
        }

        /// <summary>
        /// Initializes a new instance of the WprWrapper class with the specified wprp file, WPR.exe path, and ETL file name.
        /// </summary>        
        /// <param name="wprpFile">The wprp file to use for tracing.</param>
        /// <param name="wprPath">The WPR exe path and file name to use for controlling the WPR tracing.</param>
        public AutomateWPR(string wprpFile, string wprPath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe")
        {
            this.wprExePath = wprPath;
            this.wprpFile = wprpFile;
        }

        /// <summary>
        /// Starts a WPR recording session using the specified wprp file.
        /// </summary>
        /// <returns>0 if the trace session was started successfully.</returns>
        public int StartWPR()
        {
            int _retVal = -1;
            
            string _commandLine = "-start " + this.wprpFile;

            _retVal = this.RunWpr(_commandLine);
            return _retVal;
        }

        /// <summary>
        /// Stops the currently running WPR recording and saves it to the specified etl file name.
        /// </summary>
        /// <param name="etlFileName">The ETL file name to save the recording to.</param>
        /// <returns></returns>
        public int StopWPR(string etlFileName = "BrowserPower.etl")
        {
            int _retVal = -1;
            
            string _commandLine = "-stop " + etlFileName;

            _retVal = this.RunWpr(_commandLine);

            return _retVal;
        }

        // executes the wpr.exe commandline with the passed in command line parameters
        private int RunWpr(string cmdLine)
        {
            int _retVal = 1;
            string errorOutput = "";            

            string _wprExe = System.IO.Path.Combine(this.wprExePath, "wpr.exe");
            
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(_wprExe);
                processInfo.Arguments = cmdLine;                                
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
                                
                if (!String.IsNullOrEmpty(errorOutput))
                {
                    throw new Exception(errorOutput.ToString());
                }

                _retVal = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception trying to run wpr.exe!");
                Console.WriteLine(e.Message);
                
                _retVal = -1;
            }

            return _retVal;
        }
    }
}
