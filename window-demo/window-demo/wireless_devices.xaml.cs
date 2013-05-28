using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NativeWifi;
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.MVVM;

namespace window_demo
{
    /// <summary>
    /// Interaction logic for wireless_devices.xaml
    /// </summary>
    public partial class wireless_devices : Window
    {
        BackgroundWorker bg;
        private Logger log;
        private FileLogger filelog;
        private ObservableCollection<WirelessDevice> _unsecuredDevices = new ObservableCollection<WirelessDevice>();
        private ObservableCollection<WirelessDevice> _securedDevices = new ObservableCollection<WirelessDevice>();
        AdminWindow mainform = null;

        public wireless_devices(AdminWindow w)
        {
            mainform = w;
            AddDevice = new RelayCommand(o => SecuredDevices.Add(o as WirelessDevice), o => o != null);
            RemoveDevice = new RelayCommand(o => SecuredDevices.Remove(o as WirelessDevice), o => o != null);
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            initialiseLoggingFramework();
            bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            if (!bg.IsBusy)
            {
                bg.RunWorkerAsync();
            }
        }

        public ICommand AddDevice { get; set; }
        public ICommand RemoveDevice { get; set; }

        public ObservableCollection<WirelessDevice> UnsecuredDevices
        {
            get { return _unsecuredDevices; }
            set { _unsecuredDevices = value; }
        }

        public ObservableCollection<WirelessDevice> SecuredDevices
        {
            get { return _securedDevices; }
            set { _securedDevices = value; }
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            unSecure.ItemsSource = (ObservableCollection<WirelessDevice>)e.Result;
            log.dispatchLogMessage("Background worker thread completed");
        }
        void bg_DoWork(object sender, DoWorkEventArgs e)
        {

            ObservableCollection<WirelessDevice> devices = new ObservableCollection<WirelessDevice>();
            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                // Lists all networks in the vicinity

                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {

                    string ssid = GetStringForSSID(network.dot11Ssid);
                    string msg = "Found network with SSID " + ssid;
                    log.dispatchLogMessage(msg);
                    msg = "Signal: " + network.wlanSignalQuality;
                    log.dispatchLogMessage(msg);
                    msg = "BSS Type : " + network.dot11BssType;
                    log.dispatchLogMessage(msg);
                    msg = "Profile Name : " + network.profileName;
                    log.dispatchLogMessage(msg);
                    log.dispatchLogMessage("");

                    WirelessDevice d = new WirelessDevice(ssid , network.wlanSignalQuality);
                    devices.Add(d);
                }
            }
            _unsecuredDevices = devices;
            e.Result = _unsecuredDevices;
        }

        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
        void initialiseLoggingFramework()
        {

            // Initialize the logging framework 
            log = Logger.Instance;
            filelog = new FileLogger(@"c:\Sumeet\wirelesslog.txt");
            log.dispatchLogMessage("Begin Logging for current session");
            log.dispatchLogMessage("***");
            log.registerObserver(filelog);
        }

        private void Save_button(object sender, RoutedEventArgs e)
        {
            List<String> l = new List<String>();
            int i = 0;
            foreach (var data in _securedDevices)
            {
                i++;
                l.Add(data.SSID);
                // We shall display only 5 names in the ListView for clarity .
                if (i > 4)
                    break;
            }
            mainform.updateWirelessListView(l);
            this.Close();
        }
    }
}
