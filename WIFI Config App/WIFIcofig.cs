using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.DirectoryServices;

namespace WIFI_Config_App
{
    class WIFIcofig : INotifyPropertyChanged
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public TcpListener server = null;
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        private List<NetworkDevice> _NetworkDevicesp;
        public List<NetworkDevice> NetworkDevicesp
        {
            get
            {
                return _NetworkDevicesp;
            }

            set
            {
                _NetworkDevicesp = value;
                OnPropertyChanged("NetworkDevicesp");
            }
        }
        private List<TCPclient> _TCPclients;
        public List<TCPclient> TCPclients
        {
            get
            {
                return _TCPclients;
            }
            set
            {
                _TCPclients = value;
                OnPropertyChanged("TCPclients");
            }
        }

        private string _selectedIP;

        public string SelectedIP
        {
            get { return _selectedIP; }
            set { _selectedIP = value; OnPropertyChanged("SelectedIP"); }
        }

        private bool _broadCast;

        public bool BroadCast
        {
            get { return _broadCast; }
            set { _broadCast = value; OnPropertyChanged("BroadCast"); }
        }


        public static string ServerStatus { get; set; }

        public static List<string> ServerMessage { get; set; }

        public static string ServerMessageSend { get; set; }

        public  List<string> GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            List<string> Ips = new List<string>();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Ips.Add(ip.ToString());
                }
            }
            if (Ips.Count() == 0)
                throw new Exception("No network adapters with an IPv4 address in the system!");
            else
                return Ips;
        }

        public static List<NetworkDevice> NetworkDevices = new List<NetworkDevice>();     

        private int i;

        public static List<string> GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            NetworkDevices = new List<NetworkDevice>();
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {              

                if (item.OperationalStatus == OperationalStatus.Up) //item.NetworkInterfaceType == _type && 
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {                       
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            
                            ipAddrList.Add(ip.Address.ToString());
                            NetworkDevices.Add(new NetworkDevice() { DeviceName = item.Name, DeviceTipe = item.NetworkInterfaceType.ToString(), DeviceIP = ip.Address.ToString() });
                        }
                    }
                }
            }
            return ipAddrList;
        }

        public void Hotspot(string ssid, string key, bool status)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            Process process = Process.Start(processStartInfo);

            if (process != null)
            {
                if (status)
                {
                    process.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=" + ssid + " key=" + key);
                    process.StandardInput.WriteLine("netsh wlan start hostednetwork");
                    process.StandardInput.Close();
                }
                else
                {
                    process.StandardInput.WriteLine("netsh wlan stop hostednetwork");
                    process.StandardInput.Close();
                }
            }
        }

        public void IpWatcherStart()
        {
            Thread newThread = new Thread(new ThreadStart(IPWatch));
            newThread.Start();
        }

        private void IPWatch()
        {
            while(true)
            {
                GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
                NetworkDevicesp = NetworkDevices;              
            }
            
        }


        public void serverRun()
        {
            Thread newThread = new Thread(new ThreadStart(StartServer));
            newThread.Start();            
        }

        IPEndPoint ip;
        Socket socket;
        Socket client;
        public static List<Socket> clients;        

        private void StartServer()
        {
            ServerMessage = new List<string>();
            SelectedIP = "";
            clients = new List<Socket>();
            try
            {
                ip = new IPEndPoint(IPAddress.Any, 13000); //Any IPAddress that connects to the server on any port
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket

                socket.Bind(ip); //Bind to the client's IP
                socket.Listen(10); //Listen for maximum 10 connections
               
                Thread ClientsThread = new Thread(new ThreadStart(GetClients));
                ClientsThread.Start();

            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
            }
        }

        private void ClientLsitChanged()
        {
            List<TCPclient> TCPclientsdumm = new List<TCPclient>();
            if (clients != null)
            {
                foreach (Socket item in clients)
                {
                    TCPclientsdumm.Add(new TCPclient() { IP = item.RemoteEndPoint.ToString() });
                }
            }

            TCPclients = TCPclientsdumm;
        }

        private int clientnum = 0;
        private void GetClients()
        {
            int clientslot = 0;
            
            while (clientnum < 11)
            {
                
                try
                {
                    Console.WriteLine("Waiting for a client...");
                    client = socket.Accept();
                    clients.Add(client);
                    IPEndPoint clientep = (IPEndPoint)clients[clientnum].RemoteEndPoint;

                    Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);
                    ClientLsitChanged();

                    Thread readThread = new Thread(() => RecieveBytes(client.RemoteEndPoint));
                    readThread.Start();
                    clientslot = clientnum;
                    Thread sendThread = new Thread(() => SendBytes(client.RemoteEndPoint, clientslot));
                    sendThread.Start();

                    clientnum++;
                }
                catch
                {
                    Console.WriteLine("Client connection failed...");
                }
                
            }
            Console.WriteLine("Maximum amount of clients reached!");
        }

        private void RecieveBytes(EndPoint clientnumr)
        {
            try
            {
                List<Socket> clientR = clients.Where(t => t.RemoteEndPoint == clientnumr).ToList();
                byte[] data2 = new byte[1024];
                while (!clientR[0].Poll(10, SelectMode.SelectRead))
                {
                
                        if ((i = clientR[0].Receive(data2, data2.Length, SocketFlags.None)) != 0)
                        {
                            string recmeg = Encoding.ASCII.GetString(data2, 0, i);
                            Console.WriteLine("Received:" + recmeg + " from: " + clientR[0].RemoteEndPoint);
                            ServerMessage.Add(recmeg + "from:" + clientR[0].RemoteEndPoint);
                            data2 = new byte[1024];
                        }                              
                }
                Console.WriteLine("-------------- {0} closed recieve", clientnumr);
            }
            catch
            {
                Console.WriteLine("-------------- {0} closed recieve", clientnumr);
            }

        }

        private void SendBytes(EndPoint clientnumr, int remover)
        {
            byte[] data = new byte[1024];

            string welcome = "Welcome"; //This is the data we we'll respond with                       
            data = Encoding.ASCII.GetBytes(welcome); //Encode the data
            List<Socket> clientR = clients.Where(t => t.RemoteEndPoint == clientnumr).ToList();
            clientR[0].Send(data, data.Length, SocketFlags.None); //Send the data to the client
            
            while (!clientR[0].Poll(10, SelectMode.SelectRead))
            {
                try
                {
                    clientR = clients.Where(t => t.RemoteEndPoint == clientnumr).ToList();
                    if ((SelectedIP == clientnumr.ToString() || BroadCast == true) && ServerMessageSend != null && ServerMessageSend != "")
                    {
                        data = Encoding.ASCII.GetBytes(ServerMessageSend);
                        // Send back a response.
                        clientR[0].Send(data, data.Length, SocketFlags.None); //Send the data to the client
                        ServerStatus = "Sent: " + ServerMessageSend;
                        Console.WriteLine("Sent: {0}", ServerMessageSend);
                        ServerMessageSend = "";
                        //data = new byte[1024];
                    }
                }
                catch
                {
                    break;
                }
            }

            Console.WriteLine("-------------- {0} closed send", clientnumr);
            clientR[0].Close();
            clients.Remove(clientR[0]);
            ClientLsitChanged();
            clientnum--;
        }
    }

    public class TCPclient : INotifyPropertyChanged
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

        private string _IP;

        public string IP
        {
            get { return _IP; }
            set { _IP = value; OnPropertyChanged("IP"); }
        }

    }

    public class NetworkDevice : INotifyPropertyChanged
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

        private string _deviceName;

        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; OnPropertyChanged("DeviceName"); }
        }

        private string _deviceType;

        public string DeviceTipe
        {
            get { return _deviceType; }
            set { _deviceType = value; OnPropertyChanged("DeviceTipe"); }
        }

        private string _deviceIP;

        public string DeviceIP
        {
            get { return _deviceIP; }
            set { _deviceIP = value; OnPropertyChanged("DeviceIP"); }
        }

    }
}
