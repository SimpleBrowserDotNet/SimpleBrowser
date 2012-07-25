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
		public Browser FrameBrowser { get; private set; }
		internal override string GetAttributeValue(string name)
		{
			if (name == "SimpleBrowser.WebDriver:frameWindowHandle")
			{
				return FrameBrowser.WindowHandle;
			}
			return base.GetAttributeValue(name);
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
				this.FrameBrowser = this.OwningBrowser.CreateChildBrowser(this.Name);
				this.FrameBrowser.Navigate(new Uri(OwningBrowser.Url, this.Src));
			}
		}

	}
}
