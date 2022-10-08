using System.Xml;

namespace RussianLanguage;

public static class XmlReaderDictionary
{
    public static Dictionary<string, string> GetXmlElements(string path)
    {
        var xDoc = new XmlDocument();
        xDoc.Load(path);

        var xRoot = xDoc.DocumentElement;
        if (xRoot == null) throw new Exception("Xml document element is null");

        var xmlElements = new Dictionary<string, string>(16);

        foreach (XmlElement xNode in xRoot)
        {
            var text = xNode.InnerText;
            if (text[0] == '\\') text = Directory.GetCurrentDirectory() + text;
            xmlElements.Add(xNode.Name, text);
        }

        return xmlElements;
    }
}