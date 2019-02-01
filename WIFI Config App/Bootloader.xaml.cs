﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    public partial class Bootloader : UserControl
    {
        //string bootfile = "";
        byte[] bootfilebytes;
        int bootfileSize = 0;
        int bootchunks = 0;
        int bytesleft = 0;

        List<byte[]> BootFileList = new List<byte[]>();
        private DispatcherTimer dispatcherTimer;

        public Bootloader()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcherTimer.Start();
            FileSelect_Click(null, null);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(bootchunks>0 && WIFIcofig.BootSentIndex> 1)
                ProgBoot.Value = WIFIcofig.BootSentIndex / (double) bootchunks * 1000;
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //if (openFileDialog.ShowDialog() == true)
            //    bootfilebytes = File.ReadAllBytes(openFileDialog.FileName);

            bootfilebytes = File.ReadAllBytes("D:\\Users\\NeilPretorius\\Desktop\\V14 2\\TITAN VISION - V14\\ME-VISION-L4-PFW\\Debug\\ME-VISION-L4-PFW.binary");

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
                bootchunk[2] = (byte)'B';
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
            
            WIFIcofig.BootSentIndex = 0;
            WIFIcofig.BootAckIndex = -1;

            Thread BootloaderThread = new Thread(BootloaderDo);
            BootloaderThread.IsBackground = true;
            BootloaderThread.Start();

            WIFIcofig.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");
        }

        private void BootloaderDo()
        {
            while(true)
            {
                if (WIFIcofig.BootReady)
                {
                    
                    if (WIFIcofig.BootSentIndex == 0 && WIFIcofig.BootAckIndex == -1)
                    {
                        WIFIcofig.ServerMessageSend = BootFileList.ElementAt(WIFIcofig.BootSentIndex);
                        WIFIcofig.BootSentIndex++;
                    }

                    if(WIFIcofig.BootSentIndex<BootFileList.Count && WIFIcofig.BootAckIndex == WIFIcofig.BootSentIndex-1)
                    {
                        Thread.Sleep(10);
                        WIFIcofig.ServerMessageSend = BootFileList.ElementAt(WIFIcofig.BootSentIndex);
                        WIFIcofig.BootSentIndex++;
                    }

                    if(WIFIcofig.BootSentIndex == BootFileList.Count)
                    {
                        Console.WriteLine("====================Bootloading done======================");
                        //WIFIcofig.ServerMessageSend = 
                        WIFIcofig.BootReady = false;
                        break;
                    }
                }

            }

        }

        public void BootloaderParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&')) //
            {
                if (message[2] == 'B')
                {
                    if (message[3] == 'a' && message[6] == ']')
                    {
                        WIFIcofig.BroadCast = false;
                        WIFIcofig.BootReady = true;
                        WIFIcofig.SelectedIP = endPoint.ToString();
                    }

                    if (message[3] == 'e' && message[6] == ']')
                    {
                        WIFIcofig.BootSentIndex--;
                    }


                    if (message[3] == 'a' && (message[8] == ']' || message[9] == ']'))
                    {
                        WIFIcofig.BootAckIndex = BitConverter.ToUInt16(message, 4);

                    }
                        
                }
                else if(message[2] == 'h' /*&& message.Length == 522*/)
                {
                    Console.WriteLine("====================heartbeat recieved======================");
                }
            }
        }
    }
}
