using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class ImageInputElement : ButtonInputElement
	{
		private uint x = 0;
		private uint y = 0;

		public ImageInputElement(XElement element)
			: base(element)
		{
		}

		public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
		{
			if (isClickedElement)
			{
				if (String.IsNullOrEmpty(this.Name))
				{
					yield return new UserVariableEntry() { Name = "x", Value = this.x.ToString() };
					yield return new UserVariableEntry() { Name = "y", Value = this.y.ToString() };
				}
				else
				{
					yield return new UserVariableEntry() { Name = string.Format("{0}.x", this.Name), Value = this.x.ToString() };
					yield return new UserVariableEntry() { Name = string.Format("{0}.y", this.Name), Value = this.y.ToString() };
					if (!string.IsNullOrEmpty(this.Value))
					{
						yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
					}
				}
			}
			yield break;
		}

		public override ClickResult Click(uint x, uint y)
		{
			this.x = x;
			this.y = y;

			if (this.SubmitForm(clickedElement: this))
			{
				return ClickResult.SucceededNavigationComplete;
			}
			return ClickResult.SucceededNavigationError;
		}
	}
}
