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
		public string Target
		{
			get
			{
				string value = this.Element.GetAttributeCI("target");
				return value;
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
			string target = this.Target;
			string queryStringValues = null;

			if (url != null)
			{
				string[] querystring = url.Split(new[] { '?' });
				if (querystring.Length > 1)
					queryStringValues = querystring[1];
			}

			if (RequestNavigation(new NavigationArgs()
			{
				Uri = url,
				Target = target,
				UserVariables = StringUtil.MakeCollectionFromQueryString(queryStringValues)
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
