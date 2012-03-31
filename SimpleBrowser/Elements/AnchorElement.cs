using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Elements
{
	internal class AnchorElement : HtmlElement
	{
		public AnchorElement(XElement element)
			: base(element)
		{
		}
		public string Href
		{
			get
			{
				return this.Element.GetAttributeCI("href");
			}
		}

		static Regex _postbackRecognizer = new Regex(@"javascript\:__doPostBack\('([^\']*)\'", RegexOptions.Compiled);
		public override ClickResult Click()
		{
			base.Click();

			if (Disabled) return ClickResult.SucceededNoOp;

			var match = _postbackRecognizer.Match(this.Href);
			if(match.Success)
			{
				var name = match.Groups[1].Value;
				var eventTarget = this.OwningBrowser.Select("input[name=__EVENTTARGET]");
				eventTarget.Value = name;
				if (this.SubmitForm())
				{
					return ClickResult.SucceededNavigationComplete;
				}
				else
				{
					return ClickResult.Failed;
				}
			}

			string url = this.Href;
			if (RequestNavigation(new NavigationArgs()
			{
				Uri = url
			}))
			{
				return ClickResult.SucceededNavigationComplete;
			}
			else
			{
				return ClickResult.SucceededNavigationError;
			}
		}

	}
}
