using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleBrowser.Network
{
    // TODO Review 
    //   1) consider adding XML comments (documentation) to all public members

    public interface IWebRequestFactory
	{
		IHttpWebRequest GetWebRequest(Uri url);
	}

	public class DefaultRequestFactory : IWebRequestFactory
	{
		public IHttpWebRequest GetWebRequest(Uri url)
		{
			return new WebRequestWrapper(url);
		}
	}
}