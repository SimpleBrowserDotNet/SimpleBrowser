using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser
{
	internal class NavigationState
	{
		public Uri Url;
		public string ContentType;
		public string Html;
		internal XDocument XDocument;
	}
}
