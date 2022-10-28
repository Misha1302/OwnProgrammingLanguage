using System.Runtime.CompilerServices;
using System.Xml;

namespace RussianLanguage.Xml;

public static class XmlReaderDictionary
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static Dictionary<string, string> GetXmlElements(string path)
    {
        var xDoc = new XmlDocument();
        xDoc.Load(path);

        var xRoot = xDoc.DocumentElement;
        if (xRoot == null) throw new Exception("Xml document element is null");

        var xmlElements = new Dictionary<string, string>(2);

        foreach (XmlElement xNode in xRoot)
        {
            var text = xNode.InnerText;
            if (text[0] == '\\') text = Path.GetFullPath(text);
            xmlElements.Add(xNode.Name, text);
        }

        return xmlElements;
    }
}