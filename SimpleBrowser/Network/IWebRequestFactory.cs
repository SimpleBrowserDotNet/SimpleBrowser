using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleBrowser.Network
{
	public interface IWebRequestFactory
	{
		IHttpWebRequest GetWebRequest(Uri url);
	}
	public class DefaultRequestFactory : IWebRequestFactory
	{
		#region IWebRequestFactory Members

		public IHttpWebRequest GetWebRequest(Uri url)
		{
			return new WebRequestWrapper(url);
		}

		#endregion
	}
}
