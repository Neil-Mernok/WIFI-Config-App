using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommanderParameters;

namespace WIFI_Config_App
{
    public static class ProgramFlow
    {
        public static int ProgramWindow { get; set; }
    }

    public enum ProgramFlowE
    {
        Startup,
        Bootloader,
        Configure,
        DataView,
        Debug
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool connect = false;
        public bool Connect { get => connect; set => connect = value; }
        public List<string> LocalIps = new List<string>();            

        BindingTester bindingTester = new BindingTester() { BindingT = 1 };
        Bootloader Bootloader = new Bootloader();
        WIFIcofig WIFIcofig = new WIFIcofig();
        private DispatcherTimer dispatcherTimer;
        private DispatcherTimer dispatcherTimer2;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new
            {
                bindingTester,
                WIFIcofig,
            };

            WIFIcofig.ServerStatus = "Disconnected";            
            WIFIcofig.IpWatcherStart();
            WIFIcofig.ServerMessage = new List<ClientMessage>();

            CreateServer_Click(null, null);

            Button_Click(null, null);

            //  DispatcherTimer setup
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(WindowUpdateTimer);
            dispatcherTimer.Interval = new TimeSpan(0, 0,0,0, 300);
            dispatcherTimer.Start();


            dispatcherTimer2 = new DispatcherTimer();
            dispatcherTimer2.Tick += new EventHandler(HeatBeatTimer);
            dispatcherTimer2.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer2.Start();
        }

        private void HeatBeatTimer(object sender, EventArgs e)
        {
            if (!Bootloader.BootReady)
            {
                heartBeatMess[0] = (byte)'[';
                heartBeatMess[1] = (byte)'&';
                heartBeatMess[2] = (byte)'h';
                heartBeatMess[3] = (byte)'h';
                heartBeatMess[4] = (byte)'e';
                heartBeatMess[5] = (byte)'a';
                heartBeatMess[6] = (byte)'r';
                heartBeatMess[7] = (byte)'t';
                heartBeatMess[8] = (byte)'b';
                heartBeatMess[9] = (byte)'e';
                heartBeatMess[10] = (byte)'a';
                heartBeatMess[11] = (byte)'t';
                byte[] counts = BitConverter.GetBytes(bindingTester.BindingT);
                heartBeatMess[12] = counts[0];
                heartBeatMess[13] = counts[1];
                heartBeatMess[14] = counts[2];
                heartBeatMess[521] = (byte)']';

                WIFIcofig.ServerMessageSend = heartBeatMess;
            }
            else if(!Bootloader.bootContinue)
            {
                Bootloader.BootSentIndex = 0;
            }
        }

        int prevCount = 0;
        //int prevClients = 0;
        int textprevCount = 0;

        public byte[] heartBeatMess = Enumerable.Repeat((byte)0, 522).ToArray();

        private void WindowUpdateTimer(object sender, EventArgs e)
        {
            
            #region screens
            if (Bootloader.BootMessage!=null)
                heartBeatlbl.Content = Bootloader.BootMessage;

            if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Startup)
            {
                DataView.Visibility = ConfigureView.Visibility = BootloaderView.Visibility = Visibility.Collapsed;                
            }
            else if(ProgramFlow.ProgramWindow == (int)ProgramFlowE.Bootloader)
            {
                DataView.Visibility = ConfigureView.Visibility =  Visibility.Collapsed;
                BootloaderView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Configure)
            {
                DataView.Visibility =  BootloaderView.Visibility = Visibility.Collapsed;
                ConfigureView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.DataView)
            {
                ConfigureView.Visibility = BootloaderView.Visibility = Visibility.Collapsed;
                DataView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Debug)
            {
                StartUpView.Visibility = DataView.Visibility = ConfigureView.Visibility = BootloaderView.Visibility = Visibility.Collapsed;
            }
            #endregion

            bindingTester.BindingT++;
            selectedIP.Content = WIFIcofig.SelectedIP;
            WiFiApStatus.Content = WIFIcofig.WiFiApStatus;
            ConnectedDevicesCount.Content = WIFIcofig.clients.Count;

            if(WIFIcofig.NetworkDevicesp.Where(t => t.DeviceName.Contains("Wireless")).Count() > 0)
            {
                WIFIcofig.WiFiApStatus = "Wifi Accesspoint Up";
            }
            else
            {
                WIFIcofig.WiFiApStatus = "Wifi Accesspoint Error";
            }

            if (WIFIcofig.clients != null)
            {
               
                if(WIFIcofig.clients.Count>0)
                {
                    SendMessage.IsEnabled = true;
                    Bootload.IsEnabled = true;
                    if(WIFIcofig.clients.Count == 1)
                        WIFIcofig.SelectedIP = WIFIcofig.TCPclients[0].IP.ToString();

                }
                else
                {
                    WIFIcofig.SelectedIP = null;
                    Bootload.IsEnabled = SendMessage.IsEnabled = false;
//                    BootloaderView.Visibility = Visibility.Collapsed;
                }                    
                              
            }             
            else
            {
                ConnectedDevicesCount.Content = "0";
                WIFIcofig.SelectedIP = "";
                SendMessage.IsEnabled = false;
                Bootload.IsEnabled = false;
//                BootloaderView.Visibility = Visibility.Collapsed;

            }


            if (WIFIcofig.ServerMessagges != null && textprevCount != WIFIcofig.ServerMessagges.Count)
            {
 //               WIFIcofig.ServerMessage = WIFIcofig.ServerMessagges;
                textprevCount = WIFIcofig.ServerMessage.Count;
            }

            LocalIpsCount.Content = WIFIcofig.NetworkDevicesp.Count.ToString();
            if(WIFIcofig.NetworkDevicesp.Count != prevCount)
            {
                //lstLocal.ItemsSource = WIFIcofig.NetworkDevicesp;
                prevCount = WIFIcofig.NetworkDevicesp.Count;
            }
            

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WIFIcofig.Hotspot(null, null, false);
            Environment.Exit(Environment.ExitCode);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
               //Automate pending
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string ssid = "GodZilla5", key = "Mp123456";
            if (!connect)
            {
                WIFIcofig.Hotspot(ssid, key, true);                
                //ConnectBtn.Content = "Stop";
                connect = true;
            }
            else
            {
                WIFIcofig.Hotspot(null, null, false);
                //ConnectBtn.Content = "Start";
                connect = false;
            }

        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            WIFIcofig.ServerRun();
        }

        private int DatagridIndx = 0;

        private void LstLocal_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DatagridIndx = lstLocal.SelectedIndex;
            //if (DatagridIndx != -1)
            //    ConnectionStatusLbl.Content = WIFIcofig.NetworkDevicesp[DatagridIndx].DeviceIP.ToString();
            //else
            //    ConnectionStatusLbl.Content = "0.0.0.0";
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if(SendmessageTextB.Text!="" && WIFIcofig.clients.Count>0)
            {
                WIFIcofig.ServerMessageSend = Encoding.ASCII.GetBytes(SendmessageTextB.Text);
            }
        }

        private void ConnectedDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(ConnectedDevices.SelectedIndex!=-1)
            {
                WIFIcofig.SelectedIP = WIFIcofig.TCPclients[ConnectedDevices.SelectedIndex].IP;                
            }
            else
            {
                WIFIcofig.BroadCast = true;
            }
        }

        private void Broadcast_Checked(object sender, RoutedEventArgs e)
        {
            WIFIcofig.BroadCast = true;
        }

        private void Broadcast_Unchecked(object sender, RoutedEventArgs e)
        {
            WIFIcofig.BroadCast = false;
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow.MainWindow CommanderUART_About = new AboutWindow.MainWindow();
            CommanderUART_About.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WIFIcofig.Hotspot(null, null, false);
            Environment.Exit(Environment.ExitCode);
        }

        private void Bootload_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Bootloader;
        }

        private void RestartServer_Click(object sender, RoutedEventArgs e)
        {
            WIFIcofig.CloseConnectAll = true;
            Thread.Sleep(50);
            WIFIcofig.CloseConnectAll = false;
            CreateServer_Click(null, null);
        }
    }

    public class BindingTester : INotifyPropertyChanged
    {

        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        private int _BindingT;

        public int BindingT
        {
            get { return _BindingT; }
            set { _BindingT = value; OnPropertyChanged("BindingT"); }
        }

    }
}
