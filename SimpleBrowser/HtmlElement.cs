﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SimpleBrowser
{
	internal class HtmlElement
	{
		private readonly XElement _element;

		public HtmlElement(XElement element)
		{
			_element = element;
		}

		private XAttribute GetAttribute(string name)
		{
			return GetAttribute(Element, name);
		}

		public XElement XElement
		{
			get { return _element; }
		}

		private XAttribute GetAttribute(XElement x, string name)
		{
			return x.Attributes().Where(h => h.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
		}

		internal string GetAttributeValue(string name)
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

		public event Func<HtmlElement, ClickResult> Clicked;
		public event Func<HtmlElement, string, bool> FormSubmitted;
		public event Action<HtmlElement, string> AspNetPostBackLinkClicked;

		public ClickResult Click()
		{
			if(Clicked != null)
				return Clicked(this);

			return ClickResult.SucceededNoOp;
		}

		public bool SubmitForm(string url = null)
		{
			if(FormSubmitted != null)
				return FormSubmitted(this, url);

			return false;
		}

		public void DoAspNetLinkPostBack()
		{
			if(TagName == "a")
			{
				var match = Regex.Match(GetAttributeValue("href"), @"javascript\:__doPostBack\('([^\']*)\'");
				if(match.Success)
				{
					var name = match.Groups[1].Value;
					if(AspNetPostBackLinkClicked != null)
						AspNetPostBackLinkClicked(this, name);
					return;
				}
			}
			throw new InvalidOperationException("This method must only be called on <a> elements having a __doPostBack javascript call in the href attribute");
		}

		public string Value
		{
			get
			{
				var name = Element.Name.LocalName.ToLower();
				switch(name)
				{
					case "input":
						var attr = GetAttribute("value");
						if(attr == null)
							return ""; // no value attribute means empty string
						return attr.Value;

					case "select":
						var options = Element.Descendants("option");
						var optionEl = options.Where(d => d.Attribute("selected") != null).FirstOrDefault() ?? options.FirstOrDefault();
						if(optionEl == null) return null;
						var valueAttr = optionEl.Attribute("value");
						return valueAttr == null ? optionEl.Value : valueAttr.Value;

					default:
						return Element.Value;
				}
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


