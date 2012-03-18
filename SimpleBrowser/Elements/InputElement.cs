using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class InputElement : FormElementElement
	{
		public InputElement(XElement element)
			: base(element)
		{
		}
		public override string Value
		{
			get
			{
				var attr = GetAttribute("value");
				if (attr == null)
					return ""; // no value attribute means empty string
				return attr.Value;

			}
			set
			{
				Element.SetAttributeValue("value", value);
			}
		}
		public override string InputType
		{
			get { return GetAttributeValue("type"); }
		}
	}
	internal class RadioInputElement : InputElement
	{
		public RadioInputElement(XElement element)
			: base(element)
		{
		}
		public override ClickResult Click()
		{
			base.Click();
			if (!this.Selected)
			{
				this.Selected = true;
			}
			return ClickResult.SucceededNoNavigation;
		}
		public override bool Selected
		{
			get { return GetAttribute("checked") != null; }
			set
			{
				if (value)
				{
					this.Element.SetAttributeValue("checked", "true");
					foreach (var other in this.Siblings)
					{
						if (other.Element != this.Element) other.Selected = false;
					}
				}
				else
				{
					this.Element.RemoveAttributeCI("checked");
				}
			}
		}
		public IEnumerable<RadioInputElement> Siblings
		{
			get
			{
				var others = this.Element.Ancestors(XName.Get("form")).Descendants(XName.Get("input"))
					.Where(e => e.GetAttributeCI("type") == "radio" && e.GetAttributeCI("name") == this.Name)
					.Select(e => HtmlElement.CreateFor<RadioInputElement>(e));
				return others;
			}
		}
	}
	internal class CheckboxInputElement : InputElement
	{
		public CheckboxInputElement(XElement element)
			: base(element)
		{
		}
		public override ClickResult Click()
		{
			base.Click();
			this.Selected = !this.Selected;
			return ClickResult.SucceededNoNavigation;
		}
		public override bool Selected
		{
			get { return GetAttribute("checked") != null; }
			set 
			{
				if (value)
				{
					this.Element.SetAttributeValue("checked", "true");
				}
				else 
				{ 
					this.Element.RemoveAttributeCI("checked"); 
				}
			}
		}
	}
}
