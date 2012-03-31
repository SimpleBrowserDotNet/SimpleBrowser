using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class FormElement : HtmlElement
	{
		public FormElement(XElement element)
			: base(element)
		{
		}
		private IEnumerable<FormElementElement> _elements = null;
		public IEnumerable<FormElementElement> Elements
		{
			get
			{
				if (_elements == null)
				{
					var formElements = Element.Descendants()
						.Where(e => new string[]{"select", "input", "button", "textarea"}.Contains(e.Name.LocalName.ToLower()))
						.Select(e => this.OwningBrowser.CreateHtmlElement<FormElementElement>(e));
					_elements = formElements;
				}
				return _elements;
			}
		}
		public string Action
		{
			get
			{
				var actionAttr = GetAttribute(Element, "action");
				return actionAttr == null ? "." : actionAttr.Value;
			}
		}
		public string Method
		{
			get
			{
				var attr = GetAttribute(Element, "method");
				return attr == null ? "GET" : attr.Value.ToUpper();
			}
		}

		public override bool SubmitForm(string url = null, HtmlElement clickedElement = null)
		{
			//return base.SubmitForm(url, clickedElement);
			return Submit(url, clickedElement);
		}

		private bool Submit(string url = null, HtmlElement clickedElement = null)
		{
			NavigationArgs navigation = new NavigationArgs();
			navigation.Uri = url ?? this.Action;
			navigation.Method = this.Method;
			List<string> valuePairs = new List<string>();
			foreach (var entry in Elements.SelectMany(e => 
					{
						bool isClicked = false;
						if (clickedElement != null && clickedElement.Element == e.Element) isClicked = true;
						return e.ValuesToSubmit(isClicked);
					}
					))
			{
				navigation.UserVariables.Add(entry.Name, entry.Value);
			}
			return RequestNavigation(navigation);
		}
		
	}
}
