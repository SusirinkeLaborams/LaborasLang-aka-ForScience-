using NPEG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LaborasLangCompilerUnitTests.Utilities
{
    class TreeSerializer
    {
        static private DataContractSerializer _serializer;
        static private DataContractSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = new DataContractSerializer(typeof(AstNode));
                }

                return _serializer;
            }
        }

        static void Serialize(string fileName, AstNode tree)
        {
            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                Serializer.WriteObject(writer, tree);
            }
        }

        static AstNode Deserialize(string fileName)
        {
            using (var streamReader = new StreamReader(fileName))
            {
                using (var reader = new XmlTextReader(streamReader))
                {
                    return (AstNode)Serializer.ReadObject(reader);
                }
            }
        }
    }
}
