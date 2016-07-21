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

namespace TestingPower
{
    /// <summary>
    /// Data container representing the event Microsoft-Windows-Energy-Estimation-Engine/QueryStats/EnergyEstimate.
    /// </summary>
    public struct E3EnergyEstimateEvent
    {
        public uint TimeStamp;
        public string ProcessName;
        public int UserId;
        public uint CpuEnergy;
        public uint SocEnergy;
        public uint DisplayEnergy;
        public uint DiskEnergy;
        public uint NetworkEnergy;
        public uint MbbEnergy;
        public uint LossEnergy;
        public uint OtherEnergy;
        public uint EmiEnergy;
        public uint ForInternalUse;
        public uint TimeInMSec;
        public uint RecordFlags;
        public uint RecordMeasured;
        public uint InteractivityState;
        public uint Committed;
        public uint TotalEnergy;

        /// <summary>
        /// Initializes a new instance of the E3EnergyEstimateEvent struct using data from a csv formatted string.
        /// </summary>
        public E3EnergyEstimateEvent(string csvEvent)
        {
            string[] tokens = csvEvent.Split(',');
            TimeStamp = Convert.ToUInt32(tokens[1]);

            tokens[9] = tokens[9].Trim();

            // strip the quotes from the beginning and end of the Appname.
            ProcessName = tokens[9].Substring(1, tokens[9].Length - 2);

            UserId = Convert.ToInt32(tokens[10]);
            CpuEnergy = Convert.ToUInt32(tokens[11]);
            SocEnergy = Convert.ToUInt32(tokens[12]);
            DisplayEnergy = Convert.ToUInt32(tokens[13]);
            DiskEnergy = Convert.ToUInt32(tokens[14]);
            NetworkEnergy = Convert.ToUInt32(tokens[15]);
            MbbEnergy = Convert.ToUInt32(tokens[16]);
            LossEnergy = Convert.ToUInt32(tokens[17]);
            OtherEnergy = Convert.ToUInt32(tokens[18]);
            EmiEnergy = Convert.ToUInt32(tokens[19]);
            ForInternalUse = Convert.ToUInt32(tokens[20]);
            TimeInMSec = Convert.ToUInt32(tokens[21]);
            RecordFlags = Convert.ToUInt32(tokens[22]);
            RecordMeasured = Convert.ToUInt32(tokens[23]);
            InteractivityState = Convert.ToUInt32(tokens[24]);
            Committed = Convert.ToUInt32(tokens[25]);

            TotalEnergy = CpuEnergy + SocEnergy + DisplayEnergy + DiskEnergy + NetworkEnergy + MbbEnergy + LossEnergy + OtherEnergy + EmiEnergy;
        }
    }

    /// <summary>
    /// Data container representing the event Microsoft-Windows-Energy-Estimation-Engine/UnknownEnergy/UnknownEnergy.
    /// </summary>
    public struct E3UnknownEnergyEvent
    {
        public uint TimeStamp;
        public uint TotalDuration;
        public uint StandByDuration;
        public uint NonDripsDuration;
        public uint PdcDuration;
        public uint BIDuration;
        public uint TargettedBIEnergy;
        public uint ActualBIEnergy;
        public uint UnknownEnergy;
        public uint ScaleFactor;
        public uint Policy;
        public uint DripsPowerFloorMilliWatts;
        public uint NonDripsPenaltyMilliWatts;

        /// <summary>
        /// Initializes a new instance of the E3UnknownEnergyEvent struct using data from a csv formatted string.
        /// </summary>
        public E3UnknownEnergyEvent(string csvEvent)
        {
            string[] tokens = csvEvent.Split(',');
            TimeStamp = Convert.ToUInt32(tokens[1]);
            TotalDuration = Convert.ToUInt32(tokens[9]);
            StandByDuration = Convert.ToUInt32(tokens[10]);
            NonDripsDuration = Convert.ToUInt32(tokens[11]);
            PdcDuration = Convert.ToUInt32(tokens[12]);
            BIDuration = Convert.ToUInt32(tokens[13]);
            TargettedBIEnergy = Convert.ToUInt32(tokens[14]);
            ActualBIEnergy = Convert.ToUInt32(tokens[15]);
            UnknownEnergy = Convert.ToUInt32(tokens[16]);
            ScaleFactor = Convert.ToUInt32(tokens[17]);
            Policy = Convert.ToUInt32(tokens[18]);
            DripsPowerFloorMilliWatts = Convert.ToUInt32(tokens[19]);
            NonDripsPenaltyMilliWatts = Convert.ToUInt32(tokens[20]);
        }
    }
}
