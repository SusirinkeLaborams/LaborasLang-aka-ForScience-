using NPEG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    class LexerSerializer
    {
        public static void WriteObject(string filename, AstNode tree)
        {
            // Create a new instance of the Person class.
            FileStream writer = new FileStream(filename, FileMode.OpenOrCreate);
            DataContractSerializer ser = new DataContractSerializer(typeof(AstNode));
            ser.WriteObject(writer, tree);
            writer.Close();
        }

        public static AstNode ReadObject(string filename)
        {
            // Deserialize an instance of the Person class  
            // from an XML file.
            FileStream fs = new FileStream(filename,
            FileMode.OpenOrCreate);
            DataContractSerializer ser = new DataContractSerializer(typeof(AstNode));
            // Deserialize the data and read it from the instance.
            AstNode tree = (AstNode)ser.ReadObject(fs);
            fs.Close();
            return tree;
        }

    }
}
