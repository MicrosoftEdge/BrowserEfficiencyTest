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

using System.Linq;

namespace BrowserEfficiencyTest
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            int returnValue = 0;

            Arguments arguments = new Arguments(args);
            if (arguments.ArgumentsAreValid)
            {
                ScenarioRunner scenarioRunner = new ScenarioRunner(arguments);

                // Run the automation. This will write traces to the current or provided directory if the user requested it
                if (arguments.Browsers.Count > 0 && arguments.Scenarios.Count > 0)
                {
                    scenarioRunner.Run();
                }

                // If traces have been written, process them into a csv of results
                // Only necessary if we're tracing and/or measuring responsiveness
                if ((arguments.UsingTraceController && arguments.DoPostProcessing) || arguments.MeasureResponsiveness)
                {
                    PerfProcessor perfProcessor = new PerfProcessor((arguments.SelectedMeasureSets).ToList());
                    perfProcessor.Execute(arguments.EtlPath, arguments.EtlPath, scenarioRunner.GetResponsivenessResults(), ScenarioRunner._extensionsNameAndVersion);
                }
            }
            else
            {
                returnValue = 1;
            }
            return returnValue;
        }
    }
}
