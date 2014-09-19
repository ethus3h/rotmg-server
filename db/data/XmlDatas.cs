using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml.XPath;
using System.Reflection;
using System.Text.RegularExpressions;

public class XmlDatas
{
    static XmlDatas()
    {
        TypeToId = new Dictionary<short, string>();
        IdToType = new Dictionary<string, short>();
        TypeToElement = new Dictionary<short, XElement>();
        TileDescs = new Dictionary<short, TileDesc>();
        ItemDescs = new Dictionary<short, Item>();
        ObjectDescs = new Dictionary<short, ObjectDesc>();

        Stream stream;

        var assembly = typeof(XmlDatas).Assembly;
        var names = assembly.GetManifestResourceNames();

        var pattern = "^db.data.dat([0-9])+.xml";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        foreach (var name in names)
        {
            if (regex.IsMatch(name))
            {
                Console.WriteLine("Parsing " + name);
                stream = typeof(XmlDatas).Assembly.GetManifestResourceStream(name);
                try
                {
                    ProcessXml(stream);
                }
                catch (Exception ex)
                {
                    var prev = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.ForegroundColor = prev;
                }
            }
        }

        stream = typeof(XmlDatas).Assembly.GetManifestResourceStream("db.data.addition.xml");
        ProcessXml(stream);
        stream.Position = 0;

        using (StreamReader rdr = new StreamReader(stream))
            AdditionXml = rdr.ReadToEnd();

    }

    static void ProcessXml(Stream stream)
    {
        XElement root = XElement.Load(stream);
        foreach (var elem in root.Elements("Ground"))
        {
            short type = (short)Utils.FromString(elem.Attribute("type").Value);
            string id = elem.Attribute("id").Value;

            TypeToId[type] = id;
            IdToType[id] = type;
            TypeToElement[type] = elem;

            TileDescs[type] = new TileDesc(elem);
        }
        foreach (var elem in root.Elements("Object"))
        {
            if (elem.Element("Class") == null) continue;
            string cls = elem.Element("Class").Value;
            short type = (short)Utils.FromString(elem.Attribute("type").Value);
            string id = elem.Attribute("id").Value;

            TypeToId[type] = id;
            IdToType[id] = type;
            TypeToElement[type] = elem;

            if (cls == "Equipment" || cls == "Dye" || cls == "Pet")
                ItemDescs[type] = new Item(elem);
            else if (cls == "Character" || cls == "GameObject" || cls == "Wall" ||
                cls == "ConnectedWall" || cls == "CaveWall")
                ObjectDescs[type] = new ObjectDesc(elem);
        }
    }


    public static readonly Dictionary<short, string> TypeToId;
    public static readonly Dictionary<string, short> IdToType;
    public static readonly Dictionary<short, XElement> TypeToElement;
    public static readonly Dictionary<short, TileDesc> TileDescs;
    public static readonly Dictionary<short, Item> ItemDescs;
    public static readonly Dictionary<short, ObjectDesc> ObjectDescs;

    public static readonly string AdditionXml;
}