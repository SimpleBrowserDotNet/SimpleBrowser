using System.Linq;
using System.Xml.Linq;

namespace SimpleBrowser
{
	public static class XmlExtensions
	{
		public static bool HasAttributeCI(this XElement x, string attributeName)
		{
			return x.Attributes().Where(a => a.Name.LocalName.CaseInsensitiveCompare(attributeName)).FirstOrDefault() != null;
		}

		public static string GetAttributeCI(this XElement x, string attributeName)
		{
			var attr = x.Attributes().Where(a => a.Name.LocalName.CaseInsensitiveCompare(attributeName)).FirstOrDefault();
			return attr == null ? null : attr.Value;
		}

		public static string GetAttributeCI(this XElement x, string attributeName, string namespaceUri)
		{
			var attr = x.Attributes().Where(a => a.Name.LocalName.CaseInsensitiveCompare(attributeName) && a.Name.NamespaceName == namespaceUri).FirstOrDefault();
			return attr == null ? null : attr.Value;
		}

		public static string GetAttribute(this XElement x, string attributeName)
		{
			var attr = x.Attribute(attributeName);
			return attr == null ? null : attr.Value;
		}

		public static string GetAttribute(this XElement x, string attributeName, string namespaceUri)
		{
			var attr = x.Attributes().Where(a => a.Name.LocalName == attributeName && a.Name.NamespaceName == namespaceUri).FirstOrDefault();
			return attr == null ? null : attr.Value;
		}

		public static XAttribute RemoveAttributeCI(this XElement x, string attributeName)
		{
			var attr = x.Attributes().Where(a => a.Name.LocalName.CaseInsensitiveCompare(attributeName)).FirstOrDefault();
			if(attr != null)
				attr.Remove();
			return attr;
		}

		public static void SetAttributeCI(this XElement x, string attributeName, object value)
		{
			var attr = x.Attributes().Where(a => a.Name.LocalName.CaseInsensitiveCompare(attributeName)).FirstOrDefault();
			if(attr != null && value != null)
				attr.SetValue(value);
			else if(attr == null)
				x.SetAttributeValue(attributeName, value);
		}
		public static XElement GetAncestorCI(this XElement x, string elementName)
		{
			XElement result = x.Ancestors().Where(a => a.Name.LocalName.ToLower() == elementName).FirstOrDefault();
			return result;
		}
		public static XElement GetAncestorOfSelfCI(this XElement x, string elementName)
		{
			XElement result = x.AncestorsAndSelf().Where(a => a.Name.LocalName.ToLower() == elementName).FirstOrDefault();
			return result;
		}
		
	}
}
