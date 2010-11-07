using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Parser
{
	public static class DocumentCleaner
	{
		public static void Rebuild(XDocument doc)
		{
			if(string.Compare(doc.Root.Name.LocalName, "html", true) != 0)
			{
				var root = new XElement("html");
				doc.Root.ReplaceWith(root);
			}
			return;

			foreach(var item in (from e in doc.Descendants()
									let n = new { Element = e, Ancestors = e.Ancestors() }
									orderby n.Ancestors.Count() descending
									select n))
			{
				if(item.Element.Parent == null)
					continue;
				var rule = ElementPositioningRule.Get(item.Element.Name.LocalName.ToLowerInvariant());
				if(rule == null) continue;

				// restrict children according to whether or not they are text nodes
				if(rule.TextChildren.HasValue)
				{
					var textNodes = (IEnumerable<XText>)item.Element.Nodes().Where(n => n is XText);
					if(rule.TextChildren.Value)
					{
						var sb = new StringBuilder();
						foreach(var node in textNodes)
							sb.Append(node.Value);
						item.Element.RemoveNodes();
						item.Element.Add(new XText(sb.ToString()));
					}
					else
					{
						foreach(var node in textNodes)
							node.Remove();
					}
				}

				//DocumentArea area
				//rule.Area
			}
		}
	}
}
