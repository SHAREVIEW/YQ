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
using System.IO;
using MySql.Data.MySqlClient;

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
            AddDevice = new RelayCommand(o => SecuredDevices.Add(o as WirelessDevice), o => (o != null && (SecuredDevices.Count <= 1)));
            RemoveDevice = new RelayCommand(o => SecuredDevices.Remove(o as WirelessDevice), o => o != null);
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //initialiseLoggingFramework();
            log = Logger.Instance;
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

                    WirelessDevice d = new WirelessDevice(ssid, network.wlanSignalQuality);
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
            //log = Logger.Instance;
            //log.dispatchLogMessage("Inside wireless_devices()");
         
            //create a new folder directory to store the log files
            string subPath = "C:\\ProtagLockit\\TempFolder"; 
            bool IsExists = Directory.Exists(subPath);
            if (!IsExists)
                Directory.CreateDirectory(subPath);

            // Initialize the logging framework 
            log = Logger.Instance;
            filelog = new FileLogger(subPath + "\\wirelesslog.txt");
            log.dispatchLogMessage("Begin Logging for current session");
            log.dispatchLogMessage("***");
            log.registerObserver(filelog);
             
        }

        private void Save_button(object sender, RoutedEventArgs e)
        {
            List<String> l = new List<String>();
            String preference_1 = String.Empty, preference_2 = String.Empty;
            int i = 0;
            foreach (var data in _securedDevices)
            {

                if (i == 0)
                    preference_1 = data.SSID;
                if (i == 1)
                    preference_2 = data.SSID;
                i++;
                l.Add(data.SSID);
            }
            mainform.updateWirelessListView(l);
            this.Close();

            /*
             * Lets save the preference into the database 
             */
            String a = Global.empId;
            String str = @"server=localhost;database=users;userid=root;password=;";
            MySqlConnection con = null;
            MySqlDataReader reader = null;
            int count;

            try
            {
                con = new MySqlConnection(str);
                con.Open(); //open the connection



                MySqlCommand cmdOne = new MySqlCommand("SELECT  EmployeeId FROM wireless_preference WHERE EmployeeId=" + Global.empId, con);

                cmdOne.ExecuteNonQuery();
                reader = cmdOne.ExecuteReader();
                count = reader.FieldCount;
                if (reader != null)
                    reader.Close();
                if (count == 1)
                {
                    //update
                    // MySqlCommand cmd = new MySqlCommand("UPDATE wireless_preference SET preference_1='" + preference_1 + "',preference_2='" + preference_2 +"' WHERE EmployeeId='"+Global.empId +"'"), con);
                    MySqlCommand cmd = new MySqlCommand("UPDATE wireless_preference SET preference_1='" + preference_1 + "' , preference_2='" + preference_2 + "' WHERE EmployeeId='" + Global.empId + "'", con);
                    cmd.ExecuteNonQuery();
                    log.dispatchLogMessage("Wirless services: Updated preference of user " + Global.empId + " to : " + preference_1 + " & " + preference_2);
                }
                else
                {
                    //insert 
                    MySqlCommand cmd = new MySqlCommand("Insert into wireless_preference(EmployeeId,  preference_1,preference_2) values('" + Global.empId + "','" + preference_1 + "','" + preference_2 + "')", con);
                    cmd.ExecuteNonQuery();
                    log.dispatchLogMessage("Wirless services: Inserted new preference of user " + Global.empId + " to : " + preference_1 + " & " + preference_2);
                }

            }
            catch (MySqlException err) //capture and display any MySql errors that will occur
            {

                log.dispatchLogMessage("Wireless_devices : Mysql error inserting preference into the database " + err.ToString() + " ");
            }
            finally
            {
                if (con != null)
                {
                    con.Close(); //safely close the connection
                }
            }
        }
    }
}
