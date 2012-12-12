using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class ButtonInputElement : InputElement
	{
		public ButtonInputElement(XElement element)
			: base(element)
		{
		}
		public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
		{
			if (isClickedElement && !String.IsNullOrEmpty(this.Name))
			{
				return base.ValuesToSubmit(isClickedElement);
			}
			return new UserVariableEntry[0];
		}
		public override ClickResult Click()
		{
			if (this.SubmitForm(clickedElement: this))
			{
				return ClickResult.SucceededNavigationComplete;
			}
			return ClickResult.SucceededNavigationError;
		}
	}
}
