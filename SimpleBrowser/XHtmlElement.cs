using System;
using System.Linq;
using System.Xml.Linq;

namespace SimpleBrowser
{
	[Obsolete]
	public class XHtmlElement : IElement
	{
		private readonly XElement _element;

		public XHtmlElement(XElement element)
		{
			_element = element;
		}

		private XAttribute GetAttribute(string name)
		{
			return GetAttribute(Element, name);
		}
		private XAttribute GetAttribute(XElement x, string name)
		{
			return x.Attributes().Where(h => h.Name.LocalName.ToLower() == name).FirstOrDefault();
		}
		private string GetAttributeValue(string name)
		{
			return GetAttributeValue(Element, name);
		}
		private string GetAttributeValue(XElement x, string name)
		{
			var attr = GetAttribute(x, name);
			return attr == null ? null : attr.Value;
		}

		public string TagName
		{
			get { return Element.Name.LocalName; }
		}

		public bool Disabled
		{
			get { return GetAttribute("disabled") != null; }
		}

		public bool Checked
		{
			get { return GetAttribute("checked") != null; }
			set { if(Checked != value) Click(); }
		}

		public string InputType
		{
			get { return GetAttributeValue("type"); }
		}

		public event Action<XHtmlElement> Clicked;
		public event Action<XHtmlElement> FormSubmitted;

		public void Click()
		{
			if(Clicked != null)
				Clicked(this);
		}

		public void SubmitForm()
		{
			if(FormSubmitted != null)
				FormSubmitted(this);
		}

		string IElement.GetAttribute(string name)
		{
			var attr = _element.Attributes().Where(a => a.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
			return attr == null ? null : attr.Value;
		}

		public string Value
		{
			get
			{
				if(Element.Name.LocalName.ToLower() != "input")
					return Element.Value;
				var attr = GetAttribute("value");
				if(attr == null)
					return null;
				return attr.Value;
			}
			set
			{
				switch(Element.Name.LocalName.ToLower())
				{
					case "textarea":
						Element.RemoveNodes();
						Element.AddFirst(value);
						break;

					case "input":
						Element.SetAttributeValue("value", value);
						break;

					case "select":
						foreach(XElement x in Element.Descendants("option"))
						{
							var attr = GetAttribute(x, "value");
							string val = attr == null ? x.Value : attr.Value;
							x.SetAttributeValue("selected", val == value ? "selected" : null);
						}
						break;

					default:
						throw new InvalidOperationException("Can only set the Value attribute for select, textarea and input elements");
				}
			}
		}

		internal XElement Element
		{
			get { return _element; }
		}
	}
}


