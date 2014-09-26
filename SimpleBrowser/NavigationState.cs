namespace SimpleBrowser
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;

	internal class NavigationState
	{
		public Uri Url;
		public string ContentType;
		public string Html;
		internal XDocument XDocument;
		public Uri Referer { get; set; }
	}
}
