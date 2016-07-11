//--------------------------------------------------------------
//
// Microsoft Edge Elevator
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

namespace Elevator
{
    public class ElevatorClient : IElevatorClient
    {
        private StreamReader _pipeReader;
        private NamedPipeClientStream _pipeStream;
        private StreamWriter _pipeWriter;

        private ElevatorClient()
        {
            _pipeStream = new NamedPipeClientStream("TracingControllerPipe");
        }

        public void Connect()
        {
            Console.WriteLine("Attempting to connect with the trace controller...");
            _pipeStream.Connect(10 * 1000);

            Console.WriteLine("Successfully connected to trace controller.");
            _pipeReader = new StreamReader(_pipeStream);
            _pipeWriter = new StreamWriter(_pipeStream);
        }

        public static IElevatorClient Create(bool enableElevatedServer)
        {
            if (enableElevatedServer)
            {
                return new ElevatorClient();
            }

            return new ElevatorClientMock();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (_pipeStream != null)
            {
                _pipeStream.Dispose();
                _pipeStream = null;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends a message to to the tracing controller and waits for response from the tracing controller. If the tracing controller option is not used then this method returns immediately.
        /// </summary>
        /// <param name="message">A command message to send to the tracing controller</param>
        public void SendControllerMessage(string message)
        {
            string controllerResponse = "";

            _pipeWriter.WriteLine(message);
            _pipeWriter.Flush();

            // wait for a response from the controller
            controllerResponse = _pipeReader.ReadLine();
        }
    }
}
