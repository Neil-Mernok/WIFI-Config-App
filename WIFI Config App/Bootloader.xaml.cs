using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace WIFI_Config_App
{
    /// <summary>
    /// Interaction logic for Bootloader.xaml
    /// </summary>
    public partial class Bootloader : UserControl , INotifyPropertyChanged
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

 //       WIFIcofig WIFIcofig = new WIFIcofig();

        public static string BootStatus { get; set; }

        public static int BootFlashPersentage { get; set; }

        public static bool BootReady { get; set; }

        public static bool BootDone { get; set; }

        public static int BootSentIndex { get; set; }

        public static bool bootContinue;

        public static int BootAckIndex { get; set; }
        //string bootfile = "";
        byte[] bootfilebytes;
        int bootfileSize = 0;
        static int bootchunks = 0;
        int bytesleft = 0;

        List<byte[]> BootFileList = new List<byte[]>();
        private DispatcherTimer dispatcherTimer;

        private static string _bootMessage;

        public static string BootMessage
        {
            get { return _bootMessage; }
            set { _bootMessage = value; }
        }

        public Bootloader()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(ProgressBarrTimer);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcherTimer.Start();
            //DGDevicesSocket.ItemsSource = WIFIcofig.clients;
            DataContext = this;
            //FileSelect_Click(null, null);
            BootStatus = "Booting...";
        }

        private void ProgressBarrTimer(object sender, EventArgs e)
        {
            if(bootchunks>0 && BootSentIndex> 1)
                ProgBoot.Value = BootSentIndex / (double) bootchunks * 1000;

            BootStatusLbl.Content = BootStatus;
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                bootfilebytes = File.ReadAllBytes(openFileDialog.FileName);

            //bootfilebytes = File.ReadAllBytes("D:\\Users\\NeilPretorius\\Desktop\\V14 2\\TITAN VISION - V14\\ME-VISION-L4-PFW\\Debug\\ME-VISION-L4-PFW.binary");

            //           txtEditor.Text = openFileDialog.FileName;

            //bootfile = openFileDialog.FileName;

            int fileChunck = 512;

            bytesleft = bootfileSize = bootfilebytes.Length;
            bootchunks = (int)Math.Round(bootfileSize / (double)fileChunck);
            int shifter = 0;
            for (int i = 0; i <= bootchunks; i++)
            {
                byte[] bootchunk = Enumerable.Repeat((byte)0xFF, 522).ToArray();
                byte[] bytes = BitConverter.GetBytes(i);
                byte[] bytes2 = BitConverter.GetBytes(bootchunks);
                bootchunk[0] = (byte)'[';
                bootchunk[1] = (byte)'&';
                bootchunk[2] = (byte)'D';
                bootchunk[3] = bytes[0];
                bootchunk[4] = bytes[1];
                bootchunk[5] = bytes2[0];
                bootchunk[6] = bytes2[1];

                if (bytesleft> fileChunck)
                    Array.Copy(bootfilebytes, shifter, bootchunk, 7, fileChunck);
                else if(bytesleft>0)
                    Array.Copy(bootfilebytes, shifter, bootchunk, 7, bytesleft);

                bootchunk[519] = 0;
                bootchunk[520] = 0;
                bootchunk[521] = (byte)']';
                BootFileList.Add(bootchunk);
                shifter += fileChunck;
                bytesleft -= fileChunck;
            }

            
        }

        private void BootExit_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
        }

        private void Bootload_Click(object sender, RoutedEventArgs e)
        {

            BootReady = false;
            BootSentIndex = 0;
            BootAckIndex = -1;

            Thread BootloaderThread = new Thread(BootloaderDo)
            {
                IsBackground = true
            };
            BootloaderThread.Start();
            BootStatus = "Asking device to boot...";
            WIFIcofig.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");
        }

        private void BootloaderDo()
        {
            while(true)
            {
                if (BootReady)
                {
                    
                    if (BootSentIndex == 0 && BootAckIndex == -1)
                    {
                        WIFIcofig.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if(BootSentIndex<BootFileList.Count && BootAckIndex == BootSentIndex-1)
                    {
                        Thread.Sleep(10);
                        WIFIcofig.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if(BootSentIndex == BootFileList.Count)
                    {
                        Console.WriteLine("====================Bootloading done======================");
                        //WIFIcofig.ServerMessageSend = 
                        BootReady = false;
                        
                        break;
                    }
                }

            }

        }
        int heart = 0;
        public void BootloaderParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&')) //
            {
                if (message[2] == 'B')
                {
                    if (message[3] == 'a' && message[6] == ']')
                    {
                        BootStatus = "Device ready to boot...";
                        WIFIcofig.BroadCast = false;
                        BootReady = true;
                        WIFIcofig.SelectedIP = endPoint.ToString();
                    }

                    if(message[3] == 's' && message[6] == ']')
                    {
                        //done bootloading
                        BootStatus = "Device bootloading done...";
                        MessageBox.Show("Bootloading Complete!!");
                        BootDone = true;

                    }

                    if (message[3] == 'e' && message[8] == ']')
                    {
                        BootStatus = "Device bootloading packet error...";
                        BootSentIndex--;
                    }

                    if(message[3]== 'f' && message[7] == ']')
                    {
                        BootFlashPersentage = message[4];
                        BootReady = true;
                        BootStatus = "Device bootloading flash erase... " + BootFlashPersentage.ToString() + "%";
                    }
                        
                }
                else if(message[2] == 'D')
                {

                    if (message[3] == 'a' && message[8] == ']')
                    {
                        bootContinue = true;
                        BootAckIndex = BitConverter.ToUInt16(message, 4);
                        BootStatus = "Device bootloading packet " + BootAckIndex.ToString() + " of " + bootchunks.ToString() + "...";

                    }
                }
            }
        }
    }
}
