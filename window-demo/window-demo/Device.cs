﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace window_demo
{
    public class Device
    {
        public string DeviceName { get; set; }
        public BluetoothAddress DeviceAddress { get; set; }
        /* These properties are not needed . They may be enabled later if needed . 
        public bool Authenticated { get; set; }
        public bool Connected { get; set; }
        public ushort Nap { get; set; }
        public uint Sap { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime LastUsed { get; set; }
        public bool Remembered { get; set; }
         */

        public Device(BluetoothDeviceInfo device_info)
        {

            this.DeviceName = device_info.DeviceName;
            this.DeviceAddress = device_info.DeviceAddress;

            /* These properties are not needed . They may be enabled later if needed . 
            this.Authenticated = device_info.Authenticated;
            this.Connected = device_info.Connected;
            this.LastSeen = device_info.LastSeen;
            this.LastUsed = device_info.LastUsed;
            this.Nap = device_info.DeviceAddress.Nap;
            this.Sap = device_info.DeviceAddress.Sap;
            this.Remembered = device_info.Remembered;
             */
        }

        public override string ToString()
        {
            return this.DeviceName;
        }
    }
}
