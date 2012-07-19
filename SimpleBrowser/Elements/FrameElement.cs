using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser.Elements
{
	internal class FrameElement : HtmlElement
	{
		public FrameElement(XElement element)
			: base(element)
		{
		}
		public string Src
		{
			get
			{
				return this.Element.GetAttributeCI("src");
			}
		}
		public string Name
		{
			get
			{
				return this.Element.GetAttributeCI("name");
			}
		}
		internal override Browser OwningBrowser
		{
			get
			{
				return base.OwningBrowser;
			}
			set
			{
				base.OwningBrowser = value;
				Browser frameBrowser = this.OwningBrowser.CreateChildBrowser(this.Name);
				frameBrowser.Navigate(new Uri(OwningBrowser.Url, this.Src));

			}
		}

	}
}
