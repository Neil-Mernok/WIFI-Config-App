using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderParameters
{
    public class CommanderParameter
    {       
        public string Name { get; set; }
        public byte Type { get; set; }
        public int Value { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int DefValue { get; set; }
    }

    public class CommanderParameterFile
    {
        public List<CommanderParameter> CommanderParameterList;
        public UInt16 version;
        public DateTime dateCreated;
        public string createdBy;            //Name of file creator
    }
}
