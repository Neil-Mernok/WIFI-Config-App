using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderParameters
{
    public class CommanderParameter
    {       
        public string Name;
        public byte Type;
        public int Value;
        public int MaxValue;
        public int MinValue;
        public int DefValue;
    }

    public class CommanderParameterFile
    {
        public List<CommanderParameter> CommanderParameterList;
        public UInt16 version;
        public DateTime dateCreated;
        public string createdBy;            //Name of file creator
    }
}
