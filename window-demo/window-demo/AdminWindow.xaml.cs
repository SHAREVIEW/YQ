using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.TeamFoundation.MVVM;
using System.Collections.ObjectModel;
using System.ComponentModel;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NativeWifi;

namespace window_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        BackgroundWorker bgBluetooth;
        BackgroundWorker bgWireless;
        private Logger log;
        private FileLogger filelog;
        private SessionSwitchEventHandler ssh;
        WlanClient client = new WlanClient();
        /*
         * We are going to use a system of 0,1,2 as codes to identify if a thread was started or not . 
         * 0 -> The thread was never started at all
         * 1 -> The thread was started 
         * 2 -> The thread was started but called LockWorkStation()
         * 
         * We are particularly interested in state 2 as we use it to re-start the thread when the LockWorkStation() is called . 
         */
        int startBluetooth = 0;
        int startWireless = 0;

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();
        void SysEventsCheck(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                //case SessionSwitchReason.SessionLock :
                // log.dispatchLogMessage("System has been LOCKED ! Check for Bluetooth Protag");
                //break;

                case SessionSwitchReason.SessionUnlock:
                    log.dispatchLogMessage("System has been UNLOCKED !  Re-initiating connection to a PROTAG device ! ");
                    InitializeComponent();
                    if (startBluetooth == 2)
                    {
                        bgBluetooth = new BackgroundWorker();
                        bgBluetooth.DoWork += new DoWorkEventHandler(bg_DoBluetoothWork);
                        bgBluetooth.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunBluetoothThreadCompleted);
                        if (!bgBluetooth.IsBusy)
                        {

                            bgBluetooth.RunWorkerAsync();
                        }
                    }
                    if (startWireless == 2)
                    {
                        
                        System.Threading.Thread.Sleep(20000);
                        checkCurrentWirelessCon();
                        registerWlanListener();
                    }
                    break;
            }
        }
        public AdminWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            ssh = new SessionSwitchEventHandler(SysEventsCheck);
            SystemEvents.SessionSwitch += ssh;
            initialiseLoggingFramework();
        }
        void initialiseLoggingFramework()
        {

            // Initialize the logging framework 
            log = Logger.Instance;
            filelog = new FileLogger(@"c:\Sumeet\bluetoothhlog.txt");
            log.dispatchLogMessage("Begin Logging for current session");
            log.dispatchLogMessage("***");
            log.registerObserver(filelog);
        }

        private void bluetooth_Click(object sender, RoutedEventArgs e)
        {

            bluetooth_devices window = new bluetooth_devices(this);
            window.Show();
        }

        private void wireless_click(object sender, RoutedEventArgs e)
        {
            wireless_devices window = new wireless_devices(this);
            window.Show();
        }

        public void updateBluetoothListView(List<String> l)
        {
            list1.ItemsSource = l;

        }
        public void updateWirelessListView(List<String> l)
        {
            list2.ItemsSource = l;

        }

        private void bluetooth_connect(object sender, RoutedEventArgs e)
        {
            tgbtn1.Content = (String)list1.Items[0];

            /*
             * Create a new background thread . 
             */

            // Bluetooth thread started.
            startBluetooth = 1;
            bgBluetooth = new BackgroundWorker();
            bgBluetooth.DoWork += new DoWorkEventHandler(bg_DoBluetoothWork);
            bgBluetooth.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunBluetoothThreadCompleted);
            if (!bgBluetooth.IsBusy)
            {
                bgBluetooth.RunWorkerAsync();
            }
        }

        private void bg_RunBluetoothThreadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void bg_DoBluetoothWork(object sender, DoWorkEventArgs e)
        {
            /*
             * Connect to a bluetooth device . We try to connect to the bluetooth device that appears in the 
             * datagrid in order of preference . So first attempt to connect to device that apprears top in the datagrid 
             * and so on ...
             */
            log = Logger.Instance;
            var mesg ="Background logging";
            /*
             * Extract the preference order of the Bluetooth devices
             */

            String device1 = (String)list1.Items[0];
            //String device2 = (String)list1.Items[1];
            //String device3 = (String)list1.Items[2];
            
            if(device1!=null)
            {
                doBluetoothWork(device1);
                mesg = "Mainservices : Bluetooth pairing is broken with device " + device1 + " .Now attempting to connect with next prefered device";
                log.dispatchLogMessage(mesg);
            }
            /*
            if (device2 != null)
            {
                doBluetoothWork(device2);
                mesg = "Mainservices : Bluetooth pairing is broken with device " + device2 + " .Now attempting to connect with next prefered device";
                log.dispatchLogMessage(mesg);
            }
            if (device3 != null)
            {
                doBluetoothWork(device3);
                mesg = "Mainservices : Bluetooth pairing is broken with device " + device3 + " .";
                log.dispatchLogMessage(mesg);
            }*/
            /*
             * If we hit here it means that we were unable to connect to & maintain a Bluetooth Connection with neither
             * of the above 3 devices . The Bluetooth pairing is surely brocken so we LOCK the computer !
             */
            mesg = "Mainservices : Bluetooth pairing is broken . Please check ! ";
            log.dispatchLogMessage(mesg);
            // Reset the code to indicate that the thread is now stopped . Upon unlocking it needs to be restarted.
            startBluetooth = 2;
            LockWorkStation();
        }
        private void bluetooth_disconnect(object sender, RoutedEventArgs e)
        {
            tgbtn1.Content = "Unsecured";

        }
        private void wireless_connect(object sender, RoutedEventArgs e)
        {
            String device1 = String.Empty;
            String device2 = String.Empty;

            try
            {
                device1 = (String)list2.Items[0];
                device2 = (String)list2.Items[1];
            }
            catch (Exception ex)
            {
                // If for some reason the device name is not populated then initialize it to NULL . 
                device1 = null;
                device2 = null;
            }
            // Wireless thread started

            if (device1 == null && device2 == null)
            {
                tgbtn2.Content = "Unsecured";
                MessageBox.Show("No Safe Wireless connection specified. ");
            }
            else
                tgbtn2.Content = "Secured";
            startWireless = 1;
            registerWlanListener();
            checkCurrentWirelessCon();

            /*
            bgWireless = new BackgroundWorker();
            bgWireless.DoWork += new DoWorkEventHandler(bg_DoWirelessWork);
            bgWireless.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWirelessThreadCompleted);
            if (!bgWireless.IsBusy)
            {
                bgWireless.RunWorkerAsync();
            }
             */
        }
        private void wireless_disconnect(object sender, RoutedEventArgs e)
        {
            tgbtn2.Content = "Unsecured";
            unregisterWlanListener();
        }
        private void bg_RunWirelessThreadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void bg_DoWirelessWork(object sender, DoWorkEventArgs e)
        {
            /*
             * In case of wireless connections we need to be sure of the Network profile the system is connected to . It should be 
             * connected to the item in the top of the datagrid . The datagrid listed items is the order of preference that we follow.
             */
            log = Logger.Instance;
            var mesg = "Background logging";
            /*
             * Extract the preference order of the Bluetooth devices
             */
            String device1 = (String)list2.Items[0];
            //String device2 = (String)list1.Items[1];
            //String device3 = (String)list1.Items[2];

            registerWlanListener();
            
        }
        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
        void doBluetoothWork(String deviceName)
        {

            // Check if a Bluetooth radio is available on the system that is compatible with the 32Feet library.
            // If not then exit.
            log.dispatchLogMessage("Mainservices : Attempting to find a Bluetooth radio ");
            bool d = BluetoothRadio.IsSupported;
            if (!d)
            {
                var msg = "No compatible Bluetooth radio found on system . Aboring application";
                log.dispatchLogMessage(msg);
                System.Windows.Forms.MessageBox.Show(msg);
                Environment.Exit(1);
            }
            log.dispatchLogMessage("Mainservices : Success! Compatibe Bluetooth radio found on system ");

            BluetoothRadio br = BluetoothRadio.PrimaryRadio;
            var messg = "Mainservices : Details of the Bluetooth radio : ";
            log.dispatchLogMessage(messg);
            // Manfucaturer
            messg = "Mainservices : Manufacturer : " + br.Manufacturer.ToString();
            log.dispatchLogMessage(messg);

            // System Name 
            messg = "Mainservices : Name : " + br.Name.ToString();
            log.dispatchLogMessage(messg);

            //Software Manufacturer
            messg = "Mainservices : Software Manufacturer :" + br.SoftwareManufacturer.ToString();
            log.dispatchLogMessage(messg);
            log.dispatchLogMessage("Mainservices : Bluetooth Radio initiated");
            log.dispatchLogMessage("***");

            // This must be put in a try block 
            InTheHand.Net.Sockets.BluetoothClient bc = new InTheHand.Net.Sockets.BluetoothClient();
            InTheHand.Net.Sockets.BluetoothDeviceInfo[] array = bc.DiscoverDevices();
            log.dispatchLogMessage("Mainservices : Bluetooth Devices found  in vicinity");

            int count = array.Length;
            BluetoothDeviceInfo dev;
            for (int i = 0; i < count; i++)
            {
    

                    Device device = new Device(array[i]);
                    if (device.DeviceName == deviceName)
                    {
                        /*
                         * Here the maxTries refers to the number of times we will make an attempt to "connect" to the device . Later
                         * the maxTries refer to the number of times we will make an attemp to "checkconnection" that was established earlier.
                         */
                        int attempts = 0;
                        int maxTries = 3;


                        while (attempts < maxTries)
                        {
                            
                        log.dispatchLogMessage("Mainservices : Found a Bluetooth device to connect to ");
                        log.dispatchLogMessage("Mainservices : Attempting connection to : " + deviceName);
                        dev = array[i];

                        var addr = device.DeviceAddress;
                        if (addr == null)
                        {
                            return;
                        }

                        Guid serviceClass = BluetoothService.SerialPort;

                        // Make a connection to the bluetooth device and continously check for pairing 
                        //  

                        // Make a connection to the specified Bluetooth device 
                        try
                        {
                            connectBluetoothDevice(bc, addr, serviceClass);


                            /*
                             * Here we fetch the RSSI values of the strength of the signal established betweem the system 
                             * and bluetooth device . Currently we just log it . Maybe later we make more better use of it.
                             */
                            try
                            {
                                int sigStrgt = dev.Rssi;
                                var mssg = "Mainservices : Signal strength of the connection is" + sigStrgt;
                                log.dispatchLogMessage(mssg);
                            }
                            catch (Exception e)
                            {
                                /*
                                 Rssi query may fail on certain platforms . Check http://inthehand.com/library/html/P_InTheHand_Net_Sockets_BluetoothDeviceInfo_Rssi.htm
                                 for platfrom details . Handle failure gracefully.
                                 */
                                var mssg = e.Message;
                                mssg += "Mainservices : Signal strength cannot be determined";
                                log.dispatchLogMessage(mssg);
                            }

                            checkconnection(dev, serviceClass, bc);
                            /*
                             * If you fall out from checkconnection you probably mean that the connection no longer exits . 
                             * So increment attempts.
                             */
                            attempts++;
                        }
                        catch (Exception ex)
                        {
                            // handle exception
                            attempts++;
                            var msg = "Mainservices : Bluetooth connection failed: " + ex.Message;
                            log.dispatchLogMessage(msg);
                            log.dispatchLogMessage("Mainservices : Re-initiating connection");
                            msg = "Mainservices : Attempt " + attempts;
                            log.dispatchLogMessage(msg);

                        }

                    }

                }

            }

        }
        public void connectBluetoothDevice(BluetoothClient bc, BluetoothAddress addr, Guid serviceClass)
        {
            try
            {
                var ep = new BluetoothEndPoint(addr, serviceClass);
                bc.Connect(ep);
            }
            catch (SocketException ex)
            {
                // Try to give a explanation reason by checking what error-code.
                // http://32feet.codeplex.com/wikipage?title=Errors
                // Note the error codes used on MSFT+WM are not the same as on
                // MSFT+Win32 so don't expect much there, we try to use the
                // same error codes on the other platforms where possible.
                // e.g. Widcomm doesn't match well, Bluetopia does.
                // http://32feet.codeplex.com/wikipage?title=Feature%20support%20table
                string reason;
                switch (ex.ErrorCode)
                {
                    case 10048: // SocketError.AddressAlreadyInUse
                        // RFCOMM only allow _one_ connection to a remote service from each device.
                        reason = "There is an existing connection to the remote Service from the device";
                        break;
                    case 10049: // SocketError.AddressNotAvailable
                        reason = " Service not running on remote device";
                        break;
                    case 10064: // SocketError.HostDown
                        reason = " Service not using RFCOMM (huh!!!)";
                        break;
                    case 10013: // SocketError.AccessDenied:
                        reason = "Authentication required";
                        break;
                    case 10060: // SocketError.TimedOut:
                        reason = "Timed-out";
                        break;
                    default:
                        reason = null;
                        break;
                }
                reason += " (" + ex.ErrorCode.ToString() + ") -- ";
                //
                var msg = "Mainservices : Bluetooth connection failed: " + ex.Message;
                msg = reason + msg;
                msg += "Mainservices : Attempting to reconnect ....";
                log.dispatchLogMessage(msg);
                //MessageBox.Show(msg);
            }
            var mssg = "Mainservices : Bluetooh connection established with a PROTAG device ";
            log.dispatchLogMessage(mssg);
        }
        public void checkconnection(BluetoothDeviceInfo dev, Guid serviceClass, BluetoothClient bc)
        {
            int attempts = 0;
            int maxTries = 5;
            while (attempts < maxTries)
            {
                try
                {
                    ServiceRecord[] records = dev.GetServiceRecords(serviceClass);
                    Thread.Sleep(10);
                }
                catch (SocketException exception)
                {
                    var msgg = " (" + exception.ErrorCode.ToString() + ") -- ";
                    //
                    msgg += "Mainservices : Bluetooth connection No longer exists : " + exception.Message;
                    log.dispatchLogMessage(msgg);
                    attempts++;
                    msgg = "Mainservices : Making attempt " + attempts + " to connect back";
                    log.dispatchLogMessage(msgg);
                }
            }
            bc.Close();
        }
        public void wlanConnectionChangeHandler(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            string msg = String.Empty;
            String device1 = String.Empty;
            String device2 = String.Empty;

            switch (notifyData.notificationSource)
            {
                case Wlan.WlanNotificationSource.ACM:

                    switch ((Wlan.WlanNotificationCodeAcm)notifyData.notificationCode)
                    {
                        case Wlan.WlanNotificationCodeAcm.ConnectionStart:
                            msg = "ConnectionStart";
                            break;

                        case Wlan.WlanNotificationCodeAcm.ConnectionComplete:
                            msg = "ConnectionComplete";
                            //WlanClient client = new WlanClient();
                            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                            {
                                
                                Wlan.WlanAssociationAttributes conAttributes = wlanIface.CurrentConnection.wlanAssociationAttributes;
                                Wlan.Dot11Ssid ssid = conAttributes.dot11Ssid;
                                try
                                {
                                    device1 = (String)list2.Items[0];
                                    device2 = (String)list2.Items[1];
                                }
                                catch (Exception e)
                                {
                                    // If for some reason the device name is not populated then initialize it to NULL . 
                                    device1 = null;
                                    device2 = null;
                                }
                                if (device1 != null && device2 != null)
                                {
                                    if (GetStringForSSID(ssid) != device1 )
                                    {
                                        if (GetStringForSSID(ssid) != device2)
                                        {
                                            msg = "MainServices (connectionChange Handler): Wireless connection is BROKEN ! Locking the system . ";
                                            log.dispatchLogMessage(msg);
                                            startWireless = 2;
                                            LockWorkStation();
                                            unregisterWlanListener();
                                        }
                                    }
                                }
                                break;
                            }

                            break;

                        case Wlan.WlanNotificationCodeAcm.Disconnecting:
                            msg = "Disconnecting";
                            break;

                        case Wlan.WlanNotificationCodeAcm.Disconnected:
                            msg = "Disconnected";
                            break;

                        default:
                            msg = "unknown notificationCode =" + notifyData.notificationCode;
                            break;

                    }
                    //MessageBox.Show(msg + " for profile:" + connNotifyData.profileName);
                    break;

                default:
                    //MessageBox.Show("irrelevant notification. Ignore");
                    break;
            }
        }
        public void checkCurrentWirelessCon()
        {
            String device1;
            String device2;

            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {

                Wlan.WlanAssociationAttributes conAttributes = wlanIface.CurrentConnection.wlanAssociationAttributes;
                Wlan.Dot11Ssid ssid = conAttributes.dot11Ssid;
                try
                {
                     device1 = (String)list2.Items[0];
                     device2 = (String)list2.Items[1];
                }
                catch (Exception e)
                { 
                    // If for some reason the device name is not populated then initialize it to NULL . 
                    device1 = null;
                    device2 = null;
                }
                if (device1 != null && device2!=null)
                {
                    if (GetStringForSSID(ssid) != device1 )
                    {
                        if (GetStringForSSID(ssid) != device2)
                        {
                            var msg = "MainServices (checkWirelessConnection): Wireless connection is BROKEN ! Locking the system . ";
                            log.dispatchLogMessage(msg);
                            startWireless = 2;
                            LockWorkStation();
                            unregisterWlanListener();
                        }
                    }
                }
            }
        }

        private void registerWlanListener()
        {
             

            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                string str = "Name=" + wlanIface.InterfaceName + ". State: ";

                switch (wlanIface.InterfaceState)
                {
                    case Wlan.WlanInterfaceState.NotReady:
                        str += "NotReady";
                        break;

                    case Wlan.WlanInterfaceState.Disconnected:
                        str += "Disconnected";
                        break;

                    case Wlan.WlanInterfaceState.Disconnecting:
                        str += "Disconnecting";
                        break;

                    case Wlan.WlanInterfaceState.Connected:
                        str += "Connected";
                        break;
                }

                wlanIface.WlanConnectionNotification += wlanConnectionChangeHandler;
                //MessageBox.Show(str + ". Listener registered");
            }
        }

        private void unregisterWlanListener()
        {
             

            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                wlanIface.WlanConnectionNotification -= wlanConnectionChangeHandler;
                //MessageBox.Show(wlanIface.InterfaceName + ". Listener unregistered");
            }
        }
        public Window setCreatingForm
        {
            get { return creatingForm; }
            set { creatingForm = value; }
        }

        public Window creatingForm { get; set; }

        public void Dispose()
        {
            SystemEvents.SessionSwitch -= ssh;
        }

        private void createUserClick(object sender, RoutedEventArgs e)
        {
            CreateUser createUser = new CreateUser();
            createUser.Show();
        }

        private void createAdminClick(object sender, RoutedEventArgs e)
        {
            CreateAdmin createAdmin = new CreateAdmin();
            createAdmin.Show();
        }
        private void logOutClick(object sender, RoutedEventArgs e)
        {
            if (creatingForm != null)
                this.creatingForm.Close();
            MainWindow window = new MainWindow();
            window.Show();
            this.Hide();
        }

    }
}
