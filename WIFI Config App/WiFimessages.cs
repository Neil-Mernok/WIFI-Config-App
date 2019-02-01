using CommanderParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WIFI_Config_App
{
    class WiFimessages
    {

        public static CommanderParameterFile CommanderParameterFile = new CommanderParameterFile();
        public static int MessageLength = 11;
        int package_success = 0;
        int package_recieved = 0;
        public int ackno_count = 0;
        public int heartbeatCounter = 0;

        public bool AdminRights = false;
        public int Firmware_Number = 0;
        public byte[] CommanderName = new byte[20];

        public string DateString;//, LicDateString;
        public string TimeString;
        public string SecondString;
        public string Local_date, Local_time;

        public static int ParameterListsize;

        public bool CommsEstablished = false;
        public bool CommsLost = false;
        public bool CommsReadyToRecieve = false;
        public bool minPackageAKN = false;
        public bool maxPackageAKN = false;
        public bool defPackageAKN = false;
        public bool curPackageAKN = false;
        public bool All_recieved = false;
        public bool All_send = false;

        public static string CommMessageBuilder(int MessageLength)
        {
            string ComMSGstring = "";
            ComMSGstring += "[&_";
            for (int i = 3; i < MessageLength - 1; i++)
            {
                ComMSGstring += " ";
            }
            ComMSGstring += "]";

            return ComMSGstring;
        }
        public static string CommMessage1 = CommMessageBuilder(MessageLength);
        public static string ConfigMessage = CommMessage1.Replace("_", "C");// "[&C  ]";
        public static string HeartBeatMessage = CommMessage1.Replace("_", "H");// "[&H  ]";
        public static string SendAllMinMessage = CommMessage1.Replace("_", "m");// "[&m  ]";
        public static string SendAllMaxMessage = CommMessage1.Replace("_", "M");// "[&M  ]";
        public static string SendAllDefMessage = CommMessage1.Replace("_", "D");// "[&D  ]";
        public static string SendAllCurMessage = CommMessage1.Replace("_", "N");// "[&N  ]";
        public static string LicenceDetailMessage = CommMessage1.Replace("_", "L");// "[&L  ]";
        public static string BootloaderMessage = CommMessage1.Replace("_", "B");// "[&B  ]";
        public static string RecieveAllMessage = CommMessage1.Replace("_", "S");// "[&S  ]";
        public static string AcknowledgeSMessage = CommMessage1.Replace("_", "A");// "[&A  ]";
        public static string AcknowledgeRMessage = CommMessage1.Replace("_", "a");// "[&a  ]";
        public static string TimeMessage = CommMessage1.Replace("_", "T");// "[&t  ]";
        public static string CommisionMessage = CommMessage1.Replace("_", "R");// "[&t  ]";

        public void Parse(byte[] recData, EndPoint clientnumr)
        {
            List<CommanderParameter> _CommanderParms = new List<CommanderParameter>();
            package_recieved++;
            if ((recData.Length >= MessageLength) && (recData[0] == '[') && (recData[1] == '&') && (recData[MessageLength - 1] == ']')) //
            {
                package_success++;
                heartbeatCounter = 0;
                if (recData[2] == 'h')
                {
                    WIFIcofig.BroadCast = false;
                    WIFIcofig.SelectedIP = clientnumr.ToString();
                    WIFIcofig.ServerMessageSend = Encoding.ASCII.GetBytes(AcknowledgeSMessage);
                    CommsEstablished = true;
                    //Console.WriteLine("Heartbeat recieved");
                    if (recData[3] == 1)
                    {
                        AdminRights = true;
                    }
                    else if (recData[3] != 1)
                    {
                        AdminRights = false;
                    }

                    //Firmware_Number = recData[3];
                    #region TimeandDate
                    DateString = "Date: 20" + String.Format("{0:x2}", (uint)System.Convert.ToUInt32(recData[4].ToString())) + "/" + String.Format("{0:x2}", (uint)System.Convert.ToUInt32(recData[5].ToString())) + "/" + String.Format("{0:x2}", (uint)System.Convert.ToUInt32(recData[6].ToString()));
                    //MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.DateLbl.Content = DateString);

                    SecondString = String.Format("{0:x2}", (uint)System.Convert.ToUInt32(recData[9].ToString()));
                    int SecondsNum = Int32.Parse(SecondString);
                    TimeString = "Time: " + String.Format("{0:x2}", (uint)System.Convert.ToUInt32(recData[7].ToString())) + ":" + String.Format("{0:x2}", (uint)System.Convert.ToUInt32(recData[8].ToString()));
                    //MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.TimeLbl.Content = TimeString + ":" + SecondString);

                    DateTime localDate = DateTime.Now;
                    Local_date = "Date: " + localDate.Year.ToString() + "/" + localDate.Month.ToString("00") + "/" + localDate.Day.ToString("00");
                    Local_time = "Time: " + localDate.Hour.ToString("00") + ":" + localDate.Minute.ToString("00");
                    if ((DateString == Local_date) && (Local_time == TimeString))
                    {
                        //MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.DateBorder.Background = Brushes.LightSeaGreen);

                    }
                    else
                    {
                       // MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.DateBorder.Background = Brushes.MediumVioletRed);
                    }

                    //string LicenceType = "";


                    // char[] LicenceNameChar = new char[20];
                    // LicenceNameChar = Encoding.UTF8.GetChars(LicName);

                    //// var str = Encoding.Default.GetString(LicName);
                    // MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.LicenceName_lbl.Content = "Name: " + new string(LicenceNameChar));

                    #endregion
                }
                else if (recData[2] == 'f')
                {
                    All_send = false;
                    ackno_count = 3;
                   // MessageBox.Show("Failed to update Commander1XX after 5 retries!");
                }

                if (recData[2] == 'a')
                {
                    Console.WriteLine("Aknowledgement recieved");
                    //Console.WriteLine(ackno_count);
                    ackno_count++;
                    //if (ackno_count == 1)
                    //{
                    //    All_send = true;
                    //    Console.WriteLine("Aknowledgement recieved to send all");
                    //    //MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.Create_SendingByte());
                    //    //SendSerialByte(SendByteCurrent);
                    //    //Console.WriteLine("Sending All Parms:" + SendByteCurrent.Length);
                    //}
                    //else if (ackno_count == 2)
                    //{
                    //    All_send = false;
                    //   // Console.WriteLine("Aknowledgement of reciept");
                    //    CommsEstablished = false;
                    //    MessageBox.Show("The Parameters have been sent to the Commander1XX");

                    //}

                }
            }
            else if ((recData.Length == ParameterListsize) && (recData[0] == '[') && (recData[1] == '&') && (recData[recData.Length - 1] == ']')) // 
            {
                heartbeatCounter = 0;
                //MainWindow.AppWindow.Dispatcher.Invoke(() => MainWindow.AppWindow.heartBeater());
                if (recData[2] == 'm')
                {
                    minPackageAKN = true;
                    WIFIcofig.ServerMessageSend = Encoding.ASCII.GetBytes(AcknowledgeSMessage);
                    //SendSerialString(AcknowledgeSMessage);
                    //Console.WriteLine("========================");
                    //Console.WriteLine("goodpackage MIn recieved");
                    //Console.WriteLine("========================");
                    //SerialIntegrity();
                    for (int i = 0; i < _CommanderParms.Count; i++)
                    {
                        byte[] MinArray = new byte[4];
                        Array.Copy(recData, i * 4 + 3, MinArray, 0, 4);
                        if (_CommanderParms.ElementAt(i) != null)
                        {
                            _CommanderParms[i].MinValue = BitConverter.ToInt32(MinArray, 0);
                            //Console.WriteLine(i);
                            //Console.WriteLine(BitConverter.ToUInt32(DefArray, 0));
                        }

                    }

                }
                if (recData[2] == 'M')
                {
                    maxPackageAKN = true;
                    WIFIcofig.ServerMessageSend = Encoding.ASCII.GetBytes(AcknowledgeSMessage);
                    //SendSerialString(AcknowledgeSMessage);
                    //Console.WriteLine("========================");
                    //Console.WriteLine("goodpackage Max recieved");
                    //Console.WriteLine("========================");
                    //SerialIntegrity();
                    for (int i = 0; i < _CommanderParms.Count; i++)
                    {
                        byte[] MaxArray = new byte[4];
                        Array.Copy(recData, i * 4 + 3, MaxArray, 0, 4);
                        if (_CommanderParms.ElementAt(i) != null)
                        {
                            _CommanderParms[i].MaxValue = BitConverter.ToInt32(MaxArray, 0);
                            //Console.WriteLine(i);
                            //Console.WriteLine(BitConverter.ToUInt32(DefArray, 0));
                        }

                    }

                }
                if (recData[2] == 'd')
                {
                    defPackageAKN = true;
                    //SendSerialString(AcknowledgeSMessage);
                    //Console.WriteLine("========================");
                    //Console.WriteLine("goodpackage Def recieved");
                    //Console.WriteLine("========================");
                    //SerialIntegrity();
                    for (int i = 0; i < _CommanderParms.Count; i++)
                    {
                        byte[] DefArray = new byte[4];
                        Array.Copy(recData, i * 4 + 3, DefArray, 0, 4);
                        if (_CommanderParms.ElementAt(i) != null)
                        {
                            _CommanderParms[i].DefValue = BitConverter.ToInt32(DefArray, 0);
                            //Console.WriteLine(i);
                            //Console.WriteLine(BitConverter.ToUInt32(DefArray, 0));
                        }

                    }

                }
                if (recData[2] == 'n')
                {
                    curPackageAKN = true;
                    //SendSerialString(AcknowledgeSMessage);
                    //Console.WriteLine("========================");
                    //Console.WriteLine("goodpackage Cur recieved");
                    //Console.WriteLine("========================");
                    //SerialIntegrity();
                    for (int i = 0; i < _CommanderParms.Count; i++)
                    {
                        byte[] CurArray = new byte[4];
                        Array.Copy(recData, i * 4 + 3, CurArray, 0, 4);
                        if (_CommanderParms.ElementAt(i) != null)
                        {
                            _CommanderParms[i].Value = BitConverter.ToInt32(CurArray, 0);
                        }

                    }
                    for (int i = 0; i < 20; i++)
                    {
                        Array.Copy(recData, i * 4 + (6 * 4 + 3), CommanderName, i, 1);
                    }


                }

            }

        }

    }
}
