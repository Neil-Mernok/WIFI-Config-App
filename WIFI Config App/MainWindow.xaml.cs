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
        WIFIcofig WIFIcofig = new WIFIcofig();
        private DispatcherTimer dispatcherTimer;
        private DispatcherTimer dispatcherTimer2;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new
            {
                bindingTester,
                WIFIcofig,
            };

            WIFIcofig.ServerStatus = "Disconnected";
            WIFIcofig.IpWatcherStart();

            CreateServer_Click(null, null);

            //  DispatcherTimer setup
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0,0,0, 300);
            dispatcherTimer.Start();


            dispatcherTimer2 = new DispatcherTimer();
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer_Tick2);
            dispatcherTimer2.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer2.Start();
        }

        private void dispatcherTimer_Tick2(object sender, EventArgs e)
        {
            if (!WIFIcofig.BootReady)
            {
                heartBeatMess[0] = (byte)'[';
                heartBeatMess[1] = (byte)'&';
                heartBeatMess[2] = (byte)'h';
                heartBeatMess[3] = (byte)'h';
                byte[] counts = BitConverter.GetBytes(bindingTester.BindingT);
                heartBeatMess[4] = counts[0];
                heartBeatMess[5] = counts[1];
                heartBeatMess[6] = counts[2];
                heartBeatMess[7] = counts[3];
                heartBeatMess[519] = 0;
                heartBeatMess[520] = 0;
                heartBeatMess[521] = (byte)']';

                WIFIcofig.ServerMessageSend = heartBeatMess;
            }
        }

        int prevCount = 0;
        //int prevClients = 0;
        //int textprevCount = 0;

        public byte[] heartBeatMess = Enumerable.Repeat((byte)0, 522).ToArray();

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {


            if(ProgramFlow.ProgramWindow == (int)ProgramFlowE.Startup)
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

            bindingTester.BindingT++;
            if (WIFIcofig.clients != null)
            {
                ConnectedDevicesCount.Content = WIFIcofig.clients.Count;
                if(WIFIcofig.clients.Count>0)
                {
                    SendMessage.IsEnabled = true;
                    Bootload.IsEnabled = true;
                }
                else
                {
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


            //if (WIFIcofig.ServerMessage!=null && textprevCount != WIFIcofig.ServerMessage.Count)
            //{
            //    MessagesGet.ItemsSource = WIFIcofig.ServerMessage;
 //           RecievedMessagesCount.Content = WIFIcofig.ServerMessage.Count;
            //}

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

            
            //lstLocal.ItemsSource = WIFIcofig.NetworkDevices;

        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            WIFIcofig.serverRun();
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
                selectedIP.Content = WIFIcofig.SelectedIP;
            }
            else
            {
                WIFIcofig.BroadCast = true;
                WIFIcofig.SelectedIP = "";
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
