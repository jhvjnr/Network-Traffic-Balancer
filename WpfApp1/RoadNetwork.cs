using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace WpfApp1
{
    [Serializable]
    public class RoadNetwork
    {
        public List<Intersection> intersections { get; set; }
        public List<Link> links { get; set; }

        public RoadNetwork()
        {
            intersections = new List<Intersection>();
            links = new List<Link>();
        }

        /*
        public void saveNetworkAsBinary()
        {
            using (Stream myStream = File.Open("outputBinary.NTB", FileMode.Create))
            {
                var binFormattter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binFormattter.Serialize(myStream, this);
            }
        }

        public void saveNetworkAsXML()
        {
            using (var writer = new StreamWriter("RoadNetwork.xml"))
            {
                 Type[] types = new Type[3];
                 types[0] = typeof(Ellipse);
                 types[1] = typeof(Line);
                 //types[2] = typeof(ExitNode);
                var serializer = new XmlSerializer(GetType(), types);
                serializer.Serialize(writer, this);
                writer.Flush();
            }
        }

        public static RoadNetwork loadGraphFromXML(string FileName)
        {
            RoadNetwork output;
            using (var reader = new StreamReader(FileName))
            {
              /*  Type[] types = new Type[3];
                types[0] = typeof(BayNode);
                types[1] = typeof(Node);
                types[2] = typeof(ExitNode);
                var deserializer = new XmlSerializer(typeof(RoadNetwork));//, types);
                output = (RoadNetwork)deserializer.Deserialize(reader);
                reader.Close();
                reader.Dispose();
            }
            return output;
        }*/

    }
}
