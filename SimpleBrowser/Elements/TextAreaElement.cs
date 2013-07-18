using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class TextAreaElement : FormElementElement
	{
		public TextAreaElement(XElement element) : base(element)
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
				int maxLength = int.MaxValue;
				if (Element.HasAttributeCI("maxlength"))
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

				Element.RemoveNodes();
				// If the length of the value being assigned is too long, truncate it.
				if (value.Length > maxLength)
				{
					Element.AddFirst(value.Substring(0, maxLength));
				}
				else
				{
					Element.SetAttributeValue("value", value);
					Element.AddFirst(value);
				}
			}
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
}
