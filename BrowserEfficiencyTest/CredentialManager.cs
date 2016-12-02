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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BrowserEfficiencyTest
{
    internal class CredentialManager
    {
        private List<UserInfo> _logins;
        private string _credentialsPath;

        /// <summary>
        /// Creates a new CredentialManager with info from credentials.json
        /// </summary>
        /// <param name="path">The file path to the json file with the stored credentials</param>
        public CredentialManager(string path)
        {
            _credentialsPath = path;
            string jsonText = File.ReadAllText(path);
            _logins = JsonConvert.DeserializeObject<List<UserInfo>>(jsonText);
        }

        /// <summary>
        /// Given the domain requested, it returns the username and password as a UserInfo object
        /// </summary>
        /// <param name="domain">The desired domain, matching the domain in the credentials.json file</param>
        /// <returns>A UserInfo object with the desired credentials</returns>
        public UserInfo GetCredentials(string domain)
        {
            foreach (UserInfo item in _logins)
            {
                if (item.Domain == domain)
                {
                    return item;
                }
            }
            throw new Exception("No credentials matching domain '" + domain + "' were found in " + _credentialsPath);
        }
    }
}
