using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using SimpleBrowser.Network;
using Moq;
using System.Net;

namespace SimpleBrowser.UnitTests
{
	public class Helper
	{
		internal static string GetFromResources(string resourceName)
		{
			Assembly assem = Assembly.GetExecutingAssembly();
			using (Stream stream = assem.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
		internal static IWebRequestFactory GetAllways200RequestMocker()
		{
			return new Always200RequestMocker();
		}
		class Always200RequestMocker : IWebRequestFactory
		{
			public Always200RequestMocker()
			{
				ResponseContent = "";
			}
			public string ResponseContent { get; set; }
			#region IWebRequestFactory Members

			public IHttpWebRequest GetWebRequest(Uri url)
			{
				var mock = new Mock<IHttpWebRequest>();
				mock.SetupAllProperties();
				mock.Setup(m => m.GetResponse())
					.Returns(() =>
					{
						var mockResponse = new Mock<IHttpWebResponse>();
						mockResponse.SetupAllProperties();
						mockResponse.SetupProperty(m => m.Headers, new WebHeaderCollection());

						byte[] responseContent = Encoding.UTF8.GetBytes(this.ResponseContent);
						mockResponse.Setup(r => r.GetResponseStream()).Returns(new MemoryStream(responseContent));
						return mockResponse.Object;
					});
				mock.SetupProperty(m => m.Headers, new WebHeaderCollection());
				mock.Setup(m => m.GetRequestStream()).Returns(new MemoryStream(new byte[2000000]));
				return mock.Object;
			}

			#endregion
		}
		internal static IWebRequestFactory GetMoviesRequestMocker()
		{
			return new MoviesRequestMocker();
		}
		class MoviesRequestMocker : IWebRequestFactory
		{

			#region IWebRequestFactory Members

			public IHttpWebRequest GetWebRequest(Uri url)
			{
				var mock = new Mock<IHttpWebRequest>();
				mock.SetupAllProperties();
				mock.Setup(m => m.GetResponse())
					.Returns(() =>
					{
						var mockResponse = new Mock<IHttpWebResponse>();
						mockResponse.SetupAllProperties();
						mockResponse.SetupProperty(m => m.Headers, new WebHeaderCollection());

						byte[] responseContent = new byte[0];
						if (mock.Object.Method == "GET")
						{
							if (url.AbsolutePath == "/movies/")
								responseContent = Encoding.UTF8.GetBytes(GetFromResources("SimpleBrowser.UnitTests.SampleDocs.movies1.htm"));
							if (url.AbsolutePath == "/movies/Movies/Create")
								responseContent = Encoding.UTF8.GetBytes(GetFromResources("SimpleBrowser.UnitTests.SampleDocs.movies2.htm"));
						}
						else if (mock.Object.Method == "POST")
						{
							if (url.AbsolutePath == "/movies/Movies/Create")
							{
								mockResponse.Object.StatusCode = HttpStatusCode.Moved;
								mockResponse.Object.Headers.Add(HttpResponseHeader.Location, "http://localhost/movies/");
							}
						}
						mockResponse.Setup(r => r.GetResponseStream()).Returns(new MemoryStream(responseContent));
						return mockResponse.Object;
					});
				mock.SetupProperty(m => m.Headers, new WebHeaderCollection());
				mock.Setup(m => m.GetRequestStream()).Returns(new MemoryStream(new byte[20000]));
				return mock.Object;
			}

			#endregion
		}


		internal static IWebRequestFactory GetFramesMock()
		{
			return new FramesRequestMocker();
		}
		class FramesRequestMocker : IWebRequestFactory
		{

			public IHttpWebRequest GetWebRequest(Uri url)
			{
				var mock = new Mock<IHttpWebRequest>();
				mock.SetupAllProperties();
				mock.Setup(m => m.GetResponse())
					.Returns(() =>
					{
						var mockResponse = new Mock<IHttpWebResponse>();
						mockResponse.SetupAllProperties();
						mockResponse.SetupProperty(m => m.Headers, new WebHeaderCollection());

						byte[] responseContent = new byte[0];
						if (mock.Object.Method == "GET")
						{
							if (url.AbsolutePath == "/")
								responseContent = Encoding.UTF8.GetBytes(GetFromResources("SimpleBrowser.UnitTests.SampleDocs.framecontainer.htm"));
							else
							{
								responseContent = Encoding.UTF8.GetBytes(GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));
							}
						}
						mockResponse.Setup(r => r.GetResponseStream()).Returns(new MemoryStream(responseContent));
						return mockResponse.Object;
					});
				mock.SetupProperty(m => m.Headers, new WebHeaderCollection());
				mock.Setup(m => m.GetRequestStream()).Returns(new MemoryStream(new byte[20000]));
				return mock.Object;
			}
		}
	}
}
