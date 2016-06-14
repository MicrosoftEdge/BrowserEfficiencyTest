using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private string etlFileName;

        /// <summary>
        /// Initializes a new instance of the AutomateWPR class with default settings.
        /// </summary>
        public AutomateWPR()
        {
            this.wprExePath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe";
            this.etlFileName = "BrowserPower.etl";
            this.wprpFile = ""; // need a default wprp file
        }
        
        /// <summary>
        /// Initializes a new instance of the WprWrapper class with the specified wprp file.
        /// </summary>
        /// <param name="wprpFile">The wprp file to use for tracing.</param>
        public AutomateWPR(string wprpFile)
        {
            this.wprExePath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe";
            this.etlFileName = "BrowserPower.etl";
            this.wprpFile = wprpFile;
        }

        /// <summary>
        /// Initializes a new instance of the WprWrapper class with the specified wprp file and WPR.exe path.
        /// </summary>
        /// <param name="wprpFile">The wprp file to use for tracing.</param>
        /// <param name="etlFileName">The ETL file to use for saving the trace session to.</param>
        public AutomateWPR(string wprpFile, string etlFileName)
        {
            this.wprExePath = @"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe";
            this.etlFileName = "BrowserPower.etl";
            this.wprpFile = wprpFile;
        }

        /// <summary>
        /// Initializes a new instance of the WprWrapper class with the specified wprp file, WPR.exe path, and ETL file name.
        /// </summary>        
        /// <param name="wprpFile">The wprp file to use for tracing.</param>
        /// <param name="etlFileName">The ETL file to use for saving the trace session to.</param>
        /// <param name="wprPath">The WPR exe path and file name to use for controlling the WPR tracing.</param>
        public AutomateWPR(string wprpFile, string etlFileName, string wprPath)
        {
            this.wprExePath = wprPath;
            this.etlFileName = etlFileName;
            this.wprpFile = wprpFile;
        }

        /// <summary>
        /// Starts a WPR recording session.
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
        /// Starts a WPR recording session using the specified wprp file.
        /// </summary>
        /// <param name="wprpFile">The name of the wprp file to start the WPR tracing session with.</param>
        /// <returns>0 if the trace session was started successfully.</returns>
        public int StartWPR(string wprpFile)
        {
            int _retVal = -1;
            string _commandLine = "-start " + this.wprpFile;

            _retVal = this.RunWpr(_commandLine);
            return _retVal;
        }

        /// <summary>
        /// Stops the currently running WPR recording and saves it to an ETL file.
        /// </summary>
        /// <returns>0 if the trace session was stopped successfully.</returns>
        public int WprStop()
        {
            int _retVal = -1;
            string _commandLine = "-stop " + this.etlFileName;

            _retVal = this.RunWpr(_commandLine);

            return _retVal;
        }

        /// <summary>
        /// Stops the currently running WPR recording and saves it to the specified etl file name.
        /// </summary>
        /// <param name="etlFileName">The ETL file name to save the recording to.</param>
        /// <returns></returns>
        public int WprStop(string etlFileName)
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

            string _wprExe = System.IO.Path.Combine(this.wprExePath, "wpr.exe");

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(_wprExe);
                processInfo.Arguments = cmdLine;                                
                processInfo.UseShellExecute = false;

                Process commandProcess = new Process();
                commandProcess.StartInfo = processInfo;
                commandProcess.Start();
                commandProcess.WaitForExit();

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
