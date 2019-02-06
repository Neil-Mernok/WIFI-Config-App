using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIFI_Config_App
{
    public class Devices : INotifyPropertyChanged
    {
        #region Onproperty changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            else
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        /////////////////////////////////////////////////////////////
        #endregion

        public DateTime _LastSeen;
        public UInt32 _UID;
        public int times_seen;

        public static UInt32 Parse_message_UID(byte[] data)
        {
            if (data.Length > 5)
                return BitConverter.ToUInt32(data, 1);
            else
                return 0;
        }

        public UInt32 VID = 0xFFFF;
        public string Vehicle_ID
        {
            get
            {
                return VID.ToString();
            }

        }

        private Byte FirmRev = 0;
        public string Rev
        {
            get
            {
                return "V" + FirmRev.ToString();
            }
        }

        private Byte rssi = 0;
        public Byte RSSI
        {
            get { return rssi; }
            set { SetField(ref rssi, value, "RSSI"); }
        }

        private string _Device_Name = "";
        public string Device_Name { get { return _Device_Name; } set { SetField(ref _Device_Name, value, "Device_Name"); } }


        public void UpdateAll()
        {
            OnPropertyChanged(null);
        }

        public Devices()
        {
            _UID = 0;
            _LastSeen = DateTime.Now;
            times_seen = 0;
        }

        public List<Devices> Device_collection;

        public void Device_parse(byte[] message)
        {
            if (message.Length > 2)
            {
                if (message[0] == (byte)'i')
                {
                    UInt32 UID = Parse_message_UID(message);
                    if (UID != 0)
                    {

                        bool found = false;

                        Devices T = Device_collection.SingleOrDefault(x => x._UID == UID);
                        if (T == null)
                            T = new Devices();
                        else
                            found = true;

                        T.Parse_message_into_Device(message);
                        if ((found == false) && (UID != 0))
                        {
                            Device_collection.Add(T);
                        }
                        OnPropertyChanged("tag_list");
                    }
                }

            }
        }

        public void Parse_message_into_Device(byte[] data)
        {
            int PacketSize = 39;
            SetField(ref _UID, Parse_message_UID(data), "UID");


            if ((data.Length >= PacketSize) && ((data[0] == 'P') || (data[0] == 'A')))
            {

                _LastSeen = DateTime.Now.AddSeconds(0 - (double)data[7]);

                //VID = BitConverter.ToUInt32(data, 10);
                SetField(ref VID, BitConverter.ToUInt32(data, 10), "Vehicle_ID");
                RSSI = data[6];


                if (data.Length > PacketSize)
                        Device_Name = Encoding.ASCII.GetString(data, PacketSize, Math.Min(PacketSize, data.Length - PacketSize));
                    else
                        Device_Name = "";
                

            }
            OnPropertyChanged("Last_Seen");
        }
    }
}
