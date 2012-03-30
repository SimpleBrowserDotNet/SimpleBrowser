using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class FormElementElement : HtmlElement
	{
		public FormElementElement(XElement element)
			: base(element)
		{
		}
		public string Name
		{
			get
			{
				var attr = GetAttribute("name");
				if (attr == null)
					return null; // no value attribute means empty string
				return attr.Value;
			}
		}
		public virtual IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
		{
			return new UserVariableEntry[0];
		}
	}
}
