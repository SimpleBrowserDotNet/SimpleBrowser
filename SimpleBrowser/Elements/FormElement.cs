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
	}
}
