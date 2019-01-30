using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommanderParameters;

namespace WIFI_Config_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool connect = false;
        public bool Connect { get => connect; set => connect = value; }
        public List<string> LocalIps = new List<string>();

        public static CommanderParameterFile CommanderParameterFile = new CommanderParameterFile();

        WIFIcofig WIFIcofig = new WIFIcofig();
        private DispatcherTimer dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            WIFIcofig.ServerStatus = "Disconnected";
            WIFIcofig.IpWatcherStart();
//            CommanderParameterFile = CommanderParameterManager.ReadCommanderParameterFile();

 //           CommanderParametersGrid.ItemsSource = CommanderParameterFile.CommanderParameterList;
 //           WiFimessages.ParameterListsize = CommanderParameterFile.CommanderParameterList.Count*4;

//            lstLocal.ItemsSource = WIFIcofig.NetworkDevicesp;
            CreateServer_Click(null, null);

            //  DispatcherTimer setup
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0,0,0, 300);
            dispatcherTimer.Start();
        }
        int prevCount = 0;
        int prevClients = 0;
        int textprevCount = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {            
            LblServerStatus.Content = WIFIcofig.ServerStatus;
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
                    BootloaderView.Visibility = Visibility.Collapsed;
                }                    
                ConnectedDevices.ItemsSource = WIFIcofig.TCPclients;                
            }             
            else
            {
                ConnectedDevicesCount.Content = "0";
                WIFIcofig.SelectedIP = "";
                SendMessage.IsEnabled = false;
                Bootload.IsEnabled = false;
                BootloaderView.Visibility = Visibility.Collapsed;

            }
                

            //if (WIFIcofig.ServerMessage!=null && textprevCount != WIFIcofig.ServerMessage.Count)
            //{
            //    MessagesGet.ItemsSource = WIFIcofig.ServerMessage;
            //    textprevCount = WIFIcofig.ServerMessage.Count;
            //}
                
            LocalIpsCount.Content = WIFIcofig.NetworkDevicesp.Count.ToString();
            if(WIFIcofig.NetworkDevicesp.Count != prevCount)
            {
                lstLocal.ItemsSource = WIFIcofig.NetworkDevicesp;
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
            string ssid = "WIFIConfigApp", key = "Mp123456";
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
                WIFIcofig.ServerMessageSend = SendmessageTextB.Text;
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
            BootloaderView.Visibility = Visibility.Visible;
        }
    }
}
