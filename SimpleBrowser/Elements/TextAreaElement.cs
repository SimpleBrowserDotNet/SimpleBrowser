using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class TextAreaElement : FormElementElement
	{
		public TextAreaElement(XElement element)
			: base(element)
		{
		}
		public override string Value
		{
			get
			{
				return base.Value;
			}
			set
			{
				Element.RemoveNodes();
				Element.AddFirst(value);
			}
		}
	}
}
