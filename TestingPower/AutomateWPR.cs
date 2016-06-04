using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingPower
{
    // Future code for automating WPR
    class AutomateWPR
    {
        private static void StartWPR()
        {
            Process.Start(@"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe", "-start <profile>");
        }

        private static void StopWPR()
        {
            Process.Start(@"C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpr.exe", "-stop <recording filename> <Problem description>");
        }
    }
}
