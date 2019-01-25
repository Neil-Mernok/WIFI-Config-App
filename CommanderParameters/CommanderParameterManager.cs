using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommanderParameters
{
    public class CommanderParameterManager
    {
        public static string CreateCommanderParameterFile(CommanderParameterFile f)
        {
            string result = "File created succesfully";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CommanderParameterFile));
                using (TextWriter writer = new StreamWriter(@"C:\Commander\Infrastructure\MernokAssetMasterList.xml"))
                {
                    serializer.Serialize(writer, f);
                }
            }
            catch (Exception e)
            {
                result = e.ToString();
            }

            return result;
        }

        public static string CommanderParameterContent { get; set; }
        //todo: Change this to accept a path for the file
        public static CommanderParameterFile ReadCommanderParameterFile(string filename)
        {
            //todo: add exception handling
            //Try Read the XML file
            XmlSerializer deserializer = new XmlSerializer(typeof(CommanderParameterFile));
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            TextReader reader = new StreamReader(filename);//(Environment.CurrentDirectory + @"\C2xxParameters.xml");
            CommanderParameterContent = reader.ReadToEnd();
            reader = new StringReader((string)CommanderParameterContent.Clone());
            object obj = deserializer.Deserialize(reader);
            CommanderParameterFile f = (CommanderParameterFile)obj;
            reader.Close();
            return f;
        }
    }
}
