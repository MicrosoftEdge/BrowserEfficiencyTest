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
using System.IO;

namespace BrowserEfficiencyTest
{
    /// <summary>
    /// A simple logger.
    /// Default is to output log statements to the console window but can be configured to output to a file as well.
    /// All outputs to the console window should use this static class by calling Logger.LogWriteLine(string logLine).
    /// </summary>
    internal static class Logger
    {
        private static bool _isFileLoggingEnabled = false;
        private static string _logFileName = "";

        /// <summary>
        /// Configures the Logger to output to a file. 
        /// The name of the log file will be 'BrowserEfficiencyTestLog_ followed by the date-timestamp in the
        /// form of yyyyMMdd_HHmmss.
        /// </summary>
        /// <param name="path">Path of where to place the log file. Default is the current working directory.</param>
        public static void SetupFileLogging(string path = null)
        {
            string _logPath = null;
            string fileName = string.Format("BrowserEfficiencyTestLog" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

            if (string.IsNullOrEmpty(path) )
            {
                _logPath = Directory.GetCurrentDirectory();
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                _logPath = path;
            }

            _logFileName = Path.Combine(_logPath, fileName);
            _isFileLoggingEnabled = true;
        }

        /// <summary>
        /// Writes a string message to the Logger.
        /// The default is to output log messages to the console window. 
        /// If file logging is enabled, the log messages are also saved to the log file.
        /// </summary>
        /// <param name="logString">The log message to write to the log.</param>
        /// <param name="includeDateTimeStamp">Set to true to include the current date-time stamp.q</param>
        public static void LogWriteLine(string logString, bool includeDateTimeStamp = true)
        {
            string dateTimeStamp = "";

            if (string.IsNullOrEmpty(logString))
            {
                return;
            }

            if (includeDateTimeStamp)
            {
                // prefix log message with the current date-time stamp
                dateTimeStamp = string.Format("[{0}] ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            string logEntry = dateTimeStamp + logString;

            // always output log messages to the console window.
            Console.WriteLine(logEntry);

            if (_isFileLoggingEnabled)
            {
                using (StreamWriter sw = new StreamWriter(_logFileName, true))
                {
                    sw.WriteLine(logEntry);
                }
            }
        }
    }
}
