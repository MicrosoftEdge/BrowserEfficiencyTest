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
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TracingController
{
    internal class Program
    {
        private const string DefaultPowerTraceProfile = "DefaultPowerTraceProfile.wprp";
        private static void Main(string[] args)
        {
            string traceProfile = "";
            CancellationTokenSource tokenSource = null;

            if (args.Length > 0)
            {
                traceProfile = args[0];
            }
            else
            {
                Console.WriteLine("No tracing profile file was specified so setting {0} as the tracing profile...", DefaultPowerTraceProfile);
                traceProfile = DefaultPowerTraceProfile;
            }

            // If the trace profile doesn't exist exit the program.
            if (!File.Exists(traceProfile))
            {
                Console.WriteLine("Unable to find the trace profile \"{0}\". Make sure the path and name are correct.", traceProfile);
                Console.WriteLine("Exiting....");
                Environment.Exit(1);
            }

            tokenSource = new CancellationTokenSource();

            // run the control server in an asynchronous task            
            Task task = new Task(() => RunTracingControlServer(traceProfile, tokenSource.Token), tokenSource.Token);
            task.Start();

            Console.WriteLine("Press ESC to stop and exit the Tracing Controller.");

            // Wait until the user presses the ESC key.
            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
            }

            // cancel the server task and clean up before exiting
            try
            {
                tokenSource.Cancel();
                task.Wait();
            }
            catch (OperationCanceledException)
            {
                // expected exception since we canceled the cancel token.
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        // This method runs the main loop of the tracing controller. 
        // It is run as an asynchronous task and takes a CancellationToken as a parameter.
        private static async Task RunTracingControlServer(string traceProfile, CancellationToken cancelToken)
        {
            AutomateWPR wpr = new AutomateWPR(traceProfile);

            string etlFileName = "";
            string line = "";

            // Need to specifically set the security to allow "Everyone" since this app runs as an admin
            // while the client runs as the default user
            PipeSecurity pSecure = new PipeSecurity();
            pSecure.SetAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

            while (!cancelToken.IsCancellationRequested)
            {
                Console.WriteLine("{0}: Tracing Controller Server starting....", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                using (var pipeServer = new NamedPipeServerStream("TracingControllerPipe", PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 255, 255, pSecure))
                {
                    Console.WriteLine("{0}: Waiting for client connection.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    try
                    {
                        await pipeServer.WaitForConnectionAsync(cancelToken);
                    }
                    catch (OperationCanceledException)
                    {
                        continue;
                    }

                    var inputPipeStream = new StreamReader(pipeServer);
                    var outputPipeStream = new StreamWriter(pipeServer);

                    // Begin interacting with the client
                    while (pipeServer.IsConnected && !cancelToken.IsCancellationRequested)
                    {
                        // get a command from the client
                        line = inputPipeStream.ReadLine();

                        // sometimes we receive a null or empty line from the client and need to skip
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        // A command line from the client is delimited by spaces
                        var messageTokens = line.Split(' ');

                        // the first token of the command line is the actual command
                        string command = messageTokens[0];

                        switch (command)
                        {
                            case "PASS_START":
                                Console.WriteLine("{0}: Client is starting the test pass.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                // acknowledge
                                outputPipeStream.WriteLine("OK");
                                outputPipeStream.Flush();
                                break;
                            case "START_BROWSER":
                                Console.WriteLine("{0}: -Starting- Iteration: {1}  Browser: {2}  Scenario: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), messageTokens[3], messageTokens[1], messageTokens[5]);
                                Console.WriteLine("{0}: Starting tracing session.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                // first cancel any currently running trace sessions
                                wpr.CancelWPR();

                                // start tracing
                                wpr.StartWPR();

                                // create the ETL file name which we will use later
                                etlFileName = messageTokens[1] + "_" + messageTokens[5] + "_" + messageTokens[3] + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".etl";

                                // acknowledge
                                outputPipeStream.WriteLine("OK");
                                outputPipeStream.Flush();
                                break;
                            case "END_BROWSER":
                                Console.WriteLine("{0}: -Finished- Browser: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), messageTokens[1]);
                                Console.WriteLine("{0}: Stopping tracing session and saving as ETL: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), etlFileName);

                                // end tracing
                                wpr.StopWPR(etlFileName);

                                Console.WriteLine("{0}: Done saving ETL file: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), etlFileName);

                                // acknowledge
                                outputPipeStream.WriteLine("OK");
                                outputPipeStream.Flush();
                                break;
                            case "PASS_END":
                                Console.WriteLine("{0}: Client test pass has ended.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                // acknowledge
                                outputPipeStream.WriteLine("OK");
                                outputPipeStream.Flush();
                                break;
                            default:
                                throw new Exception($"Unknown command encountered: {command}");
                        } // switch (Command)
                    } // while (pipeServer.IsConnected && !cancelToken.IsCancellationRequested)
                } // using (var pipeServer
            } // while (!cancelToken.IsCancellationRequested)
        }
    }
}
