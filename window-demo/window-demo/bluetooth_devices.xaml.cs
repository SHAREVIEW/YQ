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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.MVVM;
using System.ComponentModel;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using System.Net.Sockets;
using InTheHand.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO;


namespace window_demo
{
    /// <summary>
    /// Interaction logic for bluetooth_devices.xaml
    /// </summary>
    /// 
    public partial class bluetooth_devices : Window
    {
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        BackgroundWorker bg;
        private Logger log;
        private FileLogger filelog;

        AdminWindow mainform = null;
        List<String> selectedDevice = new List<String>();
        private ObservableCollection<Device> _unsecuredDevices = new ObservableCollection<Device>();
        private ObservableCollection<Device> _securedDevices = new ObservableCollection<Device>();

        public bluetooth_devices(AdminWindow w)
        {
            mainform = w;
            AddDevice = new RelayCommand(o => SecuredDevices.Add(o as Device), o => o != null);
            RemoveDevice = new RelayCommand(o => SecuredDevices.Remove(o as Device), o => o != null);
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //initialiseLoggingFramework();
            log = Logger.Instance;
            bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            if (!bg.IsBusy)
            {
                pb.Visibility = Visibility.Visible;
                bg.RunWorkerAsync();
            }
        }

        public ICommand AddDevice { get; set; }
        public ICommand RemoveDevice { get; set; }

        public ObservableCollection<Device> UnsecuredDevices
        {
            get { return _unsecuredDevices; }
            set { _unsecuredDevices = value; }
        }

        public ObservableCollection<Device> SecuredDevices
        {
            get { return _securedDevices; }
            set { _securedDevices = value; }
        }
        void initialiseLoggingFramework()
        {
            //create a new folder directory to store the log files
            string subPath = "C:\\ProtagLockit\\TempFolder";
            bool IsExists = Directory.Exists(subPath);
            if (!IsExists)
                Directory.CreateDirectory(subPath);

            // Initialize the logging framework 
            log = Logger.Instance;
            filelog = new FileLogger(subPath + "\\bluetoothhlog.txt");
            log.dispatchLogMessage("Begin Logging for current session");
            log.dispatchLogMessage("***");
            log.registerObserver(filelog);
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //device_list.ItemsSource = (List<Device>)e.Result;
            unSecure.ItemsSource = (ObservableCollection<Device>)e.Result;
            pb.Visibility = Visibility.Hidden;
        }
        


        void bg_DoWork(object sender, DoWorkEventArgs e)
        {


            /*
             * Loop over the list of all detected bluetooth devices and display them for selecion
             * by the user . 
             */
                ObservableCollection<Device> devices = new ObservableCollection<Device>();
                // Check if a Bluetooth radio is available on the system that is compatible with the 32Feet library.
                // If not then exit.
                
                log.dispatchLogMessage("Bluetooth_Devices: Attempting to find a Bluetooth radio ");
                bool d = BluetoothRadio.IsSupported;
                if (!d)
                {
                    var msg = "No compatible Bluetooth radio found on system . Aboring application";
                    log.dispatchLogMessage(msg);
                    System.Windows.Forms.MessageBox.Show(msg);
                    Environment.Exit(1);
                }
                log.dispatchLogMessage("Success! Compatibe Bluetooth radio found on system ");
                BluetoothRadio br = BluetoothRadio.PrimaryRadio;
                var messg = "Details of the Bluetooth radio : ";
                log.dispatchLogMessage(messg);
                // Manfucaturer
                messg = "Manufacturer : " + br.Manufacturer.ToString();
                log.dispatchLogMessage(messg);

                // System Name 
                messg = "Name : " + br.Name.ToString();
                log.dispatchLogMessage(messg);

                //Software Manufacturer
                messg = "Software Manufacturer :" + br.SoftwareManufacturer.ToString();
                log.dispatchLogMessage(messg);
                log.dispatchLogMessage("Bluetooth Radio initiated");
                log.dispatchLogMessage("***");

                // This must be put in a try block 
                InTheHand.Net.Sockets.BluetoothClient bc = new InTheHand.Net.Sockets.BluetoothClient();
                InTheHand.Net.Sockets.BluetoothDeviceInfo[] array = bc.DiscoverDevices();
                log.dispatchLogMessage("Bluetooth_Devices: Bluetooth Devices found  in vicinity");

                int count = array.Length;
                Device device;
                for (int i = 0; i < count; i++)
                {

                    device = new Device(array[i]);
                    devices.Add(device);
                    //UnsecuredDevices.Add(device);

                }
                _unsecuredDevices = devices;
                e.Result = _unsecuredDevices;
        }
        void doBluetoothWork()
        {
            int attempts = 0;
            int maxTries = 1;


            while (attempts < maxTries)
            {
                attempts++;
                List<Device> devices = new List<Device>();
                // Check if a Bluetooth radio is available on the system that is compatible with the 32Feet library.
                // If not then exit.
                log.dispatchLogMessage("Bluetooth Devices: Attempting to find a Bluetooth radio ");
                bool d = BluetoothRadio.IsSupported;
                if (!d)
                {
                    var msg = "No compatible Bluetooth radio found on system . Aboring application";
                    log.dispatchLogMessage(msg);
                    System.Windows.Forms.MessageBox.Show(msg);
                    Environment.Exit(1);
                }
                log.dispatchLogMessage("Success! Compatibe Bluetooth radio found on system ");
                BluetoothRadio br = BluetoothRadio.PrimaryRadio;
                var messg = "Details of the Bluetooth radio : ";
                log.dispatchLogMessage(messg);
                // Manfucaturer
                messg = "Manufacturer : " + br.Manufacturer.ToString();
                log.dispatchLogMessage(messg);

                // System Name 
                messg = "Name : " + br.Name.ToString();
                log.dispatchLogMessage(messg);

                //Software Manufacturer
                messg = "Software Manufacturer :" + br.SoftwareManufacturer.ToString();
                log.dispatchLogMessage(messg);
                log.dispatchLogMessage("Bluetooth Radio initiated");
                log.dispatchLogMessage("***");

                // This must be put in a try block 
                InTheHand.Net.Sockets.BluetoothClient bc = new InTheHand.Net.Sockets.BluetoothClient();
                InTheHand.Net.Sockets.BluetoothDeviceInfo[] array = bc.DiscoverDevices();
                log.dispatchLogMessage("Bluetooth Devices found  in vicinity");

                int count = array.Length;
                BluetoothDeviceInfo dev;
                for (int i = 0; i < count; i++)
                {

                    Device device = new Device(array[i]);
                    if (device.DeviceName == "SAMSUNG GT-I8350")
                    {
                        log.dispatchLogMessage("Found a PROTAG device to connect to ");
                        dev = array[i];

                        var addr = device.DeviceAddress;
                        if (addr == null)
                        {
                            return;
                        }

                        Guid serviceClass = BluetoothService.SerialPort;

                        // Make a connection to the bluetooth device and continously check for pairing 
                        // Lets make 3 consecutive checks to ensure that the connection still exists . 

                        // Make a connection to the specified Bluetooth device 
                        try
                        {
                            connectBluetoothDevice(bc, addr, serviceClass);


                            try
                            {
                                int sigStrgt = dev.Rssi;
                                var mssg = "Signal strength of the connection is" + sigStrgt;
                                log.dispatchLogMessage(mssg);
                            }
                            catch (Exception e)
                            {
                                /*
                                 Rssi query may fail on certain platforms . Check http://inthehand.com/library/html/P_InTheHand_Net_Sockets_BluetoothDeviceInfo_Rssi.htm
                                 for platfrom details . Handle failure gracefully.
                                 */
                                var mssg = e.Message;
                                mssg += "Signal strength cannot be determined";
                                log.dispatchLogMessage(mssg);
                            }

                            checkconnection(dev, serviceClass, bc);
                        }
                        catch (Exception ex)
                        {
                            // handle exception

                            var msg = "Bluetooth connection failed: " + ex.Message;
                            log.dispatchLogMessage(msg);
                            log.dispatchLogMessage("Re-initiating connection");
                            msg = "Attempt " + attempts;
                            log.dispatchLogMessage(msg);
                            Thread.Sleep(10);
                        }

                    }
                    devices.Add(device);
                }
                if (attempts >= maxTries) { }
                //e.Result = devices;
            }
            var mesg = "Bluetooth pairing is broken . Please check ! ";
            log.dispatchLogMessage(mesg);
            LockWorkStation();
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
                        reason = "There is an existing connection to the remote Chat2 Service";
                        break;
                    case 10049: // SocketError.AddressNotAvailable
                        reason = "Chat2 Service not running on remote device";
                        break;
                    case 10064: // SocketError.HostDown
                        reason = "Chat2 Service not using RFCOMM (huh!!!)";
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
                var msg = "Bluetooth connection failed: " + ex.Message;
                msg = reason + msg;
                msg += "Attempting to reconnect ....";
                log.dispatchLogMessage(msg);
                //MessageBox.Show(msg);
            }
            var mssg = "Bluetooh connection established with a PROTAG device ";
            log.dispatchLogMessage(mssg);
        }
        public void checkconnection(BluetoothDeviceInfo dev, Guid serviceClass, BluetoothClient bc)
        {
            int attempts = 0;
            int maxTries = 2;
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
                    msgg += "Bluetooth connection No longer exists : " + exception.Message;
                    log.dispatchLogMessage(msgg);
                    attempts++;
                    msgg = "Making attempt " + attempts + " to connect back";
                    log.dispatchLogMessage(msgg);
                }
            }
            bc.Close();
        }

        private void Save_button(object sender, RoutedEventArgs e)
        {
            List<String> l = new List<String>();
            int i = 0;
            foreach (var data in _securedDevices)
            {
                i++;
                l.Add(data.DeviceName);
                // We shall display only 5 names in the ListView for clarity . 
                if (i > 4)
                    break;
            }
            mainform.updateBluetoothListView(l);
            this.Close();
        }
    }
}