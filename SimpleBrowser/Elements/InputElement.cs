using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class InputElement : FormElementElement
	{
		public InputElement(XElement element) : base(element)
		{
		}

		public override string Value
		{
			get
			{
				var attr = GetAttribute("value");
				if (attr == null)
				{
					return string.Empty; // no value attribute means empty string
				}

				return attr.Value;
			}
			set
			{
				// Verifies that the input element type allowed to have a maxlength attribute
				string inputType = Element.GetAttributeCI("type");
				bool maxLengthAble = false;
				if (inputType == null || // According to the HTML5 specification, if the type attribute does not exist, by default, the input element is a text input.
					inputType.ToLower() == "text" ||
					inputType.ToLower() == "password" ||
					inputType.ToLower() == "search" ||
					inputType.ToLower() == "tel" ||
					inputType.ToLower() == "url" ||
					inputType.ToLower() == "email")
				{
					maxLengthAble = true;
				}

				int maxLength = int.MaxValue;
				// If the input element type allowed to have a maxlength attribute, if the element
				// has a maxlength attribute, verify that the attribute value is valid.
				if (maxLengthAble == true && Element.HasAttributeCI("maxlength"))
				{
					string maxLengthStr = Element.GetAttributeCI("maxlength");
					try
					{
						int length = Convert.ToInt32(maxLengthStr);
						if (length >= 0)
						{
							maxLength = length;
						}
						// Do nothing (implicitly) if the value of maxlength is negative, per the HTML5 spec.
					}
					catch
					{
						// Do nothing if the value of the maxlength is not a valid integer value, per the HTML5 spec.
					}
				}

				// If the length of the value being assigned is too long, truncate it.
				if (value.Length > maxLength)
				{
					Element.SetAttributeValue("value", value.Substring(0, maxLength));
				}
				else
				{
					Element.SetAttributeValue("value", value);
				}
			}
		}

		public override string InputType
		{
			get { return GetAttributeValue("type"); }
		}

		public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
		{
			if (!String.IsNullOrEmpty(this.Name))
			{
				yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
			}
			yield break;
		}
	}

	internal class SelectableInputElement : InputElement
	{
		public SelectableInputElement(XElement element)
			: base(element)
		{
		}
		public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
		{
			if (this.Selected && !String.IsNullOrEmpty(this.Name))
			{
				yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
			}
			yield break;
		}
	}
	internal class RadioInputElement : SelectableInputElement
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
					.Select(e => this.OwningBrowser.CreateHtmlElement<RadioInputElement>(e));
				return others;
			}
		}
	}
	internal class CheckboxInputElement : SelectableInputElement
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
		public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
		{
			if (this.XElement.HasAttributeCI("checked") && !String.IsNullOrEmpty(this.Name))
			{
				yield return new UserVariableEntry() { Name = this.Name, Value = string.IsNullOrEmpty(this.Value) ? "on" : this.Value };
			}
			yield break;
		}
	}
}
