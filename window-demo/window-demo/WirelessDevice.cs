using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace window_demo
{
    public class WirelessDevice
    {
        public string SSID { get; set; }
       // public string ProfileName { get; set; }
        public uint SignalQuality { get; set; }

        public WirelessDevice(String ssid , uint signalQuality)
        {
            this.SSID = ssid;
            this.SignalQuality = signalQuality;
            //this.ProfileName = profileName;
        }
    }
}