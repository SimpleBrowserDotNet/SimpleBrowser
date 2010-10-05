using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Net;

namespace SimpleBrowser
{
	public abstract class HttpBrowserBase : IDisposable
	{
		public abstract void SetProxy(string host, int port);
		public abstract void SetProxy(string host, int port, string username, string password);

		public bool Navigate(string url) { return Navigate(new Uri(url)); }
		public bool Navigate(string url, int timeoutMilliseconds) { return Navigate(new Uri(url), timeoutMilliseconds); }

		public abstract bool Navigate(Uri url);
		public abstract bool Navigate(Uri url, int timeoutMilliseconds);

		public abstract HtmlResult Find(ElementType elementType, FindBy findBy, string value);
		public abstract HtmlResult Find(string tagName, FindBy findBy, string value);
		public abstract HtmlResult Find(string id);
		public abstract HtmlResult Find(ElementType elementType, string attributeName, string attributeValue);
		public abstract HtmlResult Find(ElementType elementType, object elementAttributes);
		public abstract HtmlResult Find(string tagName, object elementAttributes);
		public abstract HtmlResult FindAll(string tagName);
		public abstract HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName);
		public abstract HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName, object elementAttributes);

		public bool AutoLogScreenshots { get; set; }
		public bool AutoLogRequestData { get; set; }
		public bool AutoLogStatusMessages { get; set; }
		public Uri Url { get; protected set; }
		public string ResponseText { get; protected set; }

		protected HttpBrowserBase()
		{
			AutoLogRequestData = true;
			AutoLogScreenshots = true;
			AutoLogStatusMessages = true;
		}

		public void Log(string message)
		{
			if(MessageLogged != null)
				MessageLogged(this, message);
		}

		public void LogRequestData()
		{
			HttpRequestLog log = AcquireRequestData();
			if(log != null && RequestLogged != null)
				RequestLogged(this, log);
		}

		public void LogScreenshot()
		{
			Image img = AcquireScreenshot();
			if(img != null && ScreenshotLogged != null)
				ScreenshotLogged(this, img);
		}

		protected virtual HttpRequestLog AcquireRequestData()
		{
			return null;
		}

		protected virtual Image AcquireScreenshot()
		{
			return null;
		}

		public event Action<HttpBrowserBase, string> MessageLogged;
		public event Action<HttpBrowserBase, HttpRequestLog> RequestLogged;
		public event Action<HttpBrowserBase, Image> ScreenshotLogged;

		public abstract void Dispose();
		public abstract bool ContainsText(string text);
	}

	public class HttpRequestLog
	{
		public string Text { get; set; }
		public string Method { get; set; }
		public NameValueCollection PostData { get; set; }
		public WebHeaderCollection RequestHeaders { get; set; }
		public WebHeaderCollection ResponseHeaders { get; set; }
		public int StatusCode { get; set; }
		public Uri Url { get; set; }
	}

	public enum FindBy
	{
		Name,
		Id,
		Class,
		Value,
		Text,
		PartialText,
		PartialName,
		PartialClass,
		PartialValue,
		PartialId
	}

	public enum ElementType
	{
		Anchor,
		TextField,
		Button,
		RadioButton,
		Checkbox,
		SelectBox,
		Script
	}

	//public abstract class HttpBrowser
	//{
	//    public abstract void SetProxy(string host, int port);
	//    public abstract void SetProxy(string host, int port, string username, string password);

	//    public abstract void Navigate(Uri url);
	//    public abstract void Navigate(Uri url, int timeout);

	//    private List<FindElement> ConvertElementType(ElementType elementType, string valueOrText)
	//    {
	//        return ConvertElementType(elementType, new List<Attribute>(), valueOrText);
	//    }

	//    private List<FindElement> ConvertElementType(ElementType elementType, List<Attribute> requiredAttributes, string valueOrText)
	//    {
	//        List<FindElement> list = new List<FindElement>();
	//        switch(elementType)
	//        {
	//            case ElementType.Anchor:
	//                list.Add(new FindElement("a", requiredAttributes, valueOrText));
	//                break;

	//            case ElementType.Button:
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "submit")));
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "image")));
	//                list.Add(new FindElement("button", requiredAttributes, valueOrText));
	//                break;

	//            case ElementType.Checkbox:
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "checkbox")));
	//                break;

	//            case ElementType.RadioButton:
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "radio")));
	//                break;

	//            case ElementType.Script:
	//                list.Add(new FindElement("script", requiredAttributes, valueOrText));
	//                break;

	//            case ElementType.SelectBox:
	//                list.Add(new FindElement("select", requiredAttributes, valueOrText));
	//                break;

	//            case ElementType.TextField:
	//                list.Add(new FindElement("textarea", requiredAttributes, valueOrText));
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "text")));
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "password")));
	//                list.Add(new FindElement("input", requiredAttributes, valueOrText, new Attribute("type", "hidden")));
	//                break;

	//            default:
	//                throw new NotImplementedException("Not supported: " + elementType);
	//        }
	//        return list;
	//    }

	//    private List<Attribute> ConvertFindBy(FindBy findBy, string attributeValue, out string valueOrText)
	//    {
	//        List<Attribute> list = new List<Attribute>();
	//        valueOrText = null;
	//        switch(findBy)
	//        {
	//            case FindBy.Class: list.Add(new Attribute("class", attributeValue)); break;
	//            case FindBy.Id: list.Add(new Attribute("id", attributeValue)); break;
	//            case FindBy.Name: list.Add(new Attribute("name", attributeValue)); break;
	//            case FindBy.TextValue: valueOrText = valueOrText; break;
	//            default: throw new NotImplementedException("Not supported: " + findBy);
	//        }
	//        return list;
	//    }

	//    public HtmlResult Find(ElementType elementType, FindBy findBy, string value)
	//    {
	//        string valueOrText;
	//        return Filter(ConvertElementType(elementType, ConvertFindBy(findBy, value, out valueOrText), valueOrText), null);
	//    }

	//    public HtmlResult Find(string tagName, FindBy findBy, string value)
	//    {
	//        string valueOrText;
	//        var list = new List<FindElement>() { new FindElement(tagName, ConvertFindBy(findBy, value, out valueOrText), valueOrText) };
	//        return Filter(list, null);
	//    }

	//    public HtmlResult Find(string id)
	//    {
	//        string valueOrText;
	//        var list = new List<FindElement>() { new FindElement(null, new Attribute("id", id)) };
	//        return Filter(list, null);
	//    }

	//    public HtmlResult Find(ElementType elementType, string attributeName, string attributeValue)
	//    {
	//        return Filter(ConvertElementType(elementType, new List<Attribute> { new Attribute(attributeName, attributeValue) }, null), null);
	//    }

	//    public HtmlResult Find(ElementType elementType, object elementAttributes)
	//    {
	//        //Skybound.Gecko.Xpcom.Initialize();
	//        return null;
	//    }

	//    public abstract HtmlResult Find(string tagName, object elementAttributes);
	//    public abstract HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName);
	//    public abstract HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName, object elementAttributes);

	//    protected abstract HtmlResult Filter(List<FindElement> match, FilterContext context);

	//    protected class FindElement
	//    {
	//        public string TagName { get; set; }
	//        public string InnerText { get; set; }
	//        public bool PartialMatchInnerText { get; set; }
	//        public List<Attribute> RequiredAttributes { get; set; }

	//        public FindElement(string tagName, params Attribute[] requiredAttributes)
	//        {
	//            TagName = tagName;
	//            RequiredAttributes = new List<Attribute>(requiredAttributes);
	//        }

	//        public FindElement(string tagName, List<Attribute> requiredAttributes, string valueOrText, params Attribute[] moreRequiredAttributes)
	//        {
	//            TagName = tagName;
	//            RequiredAttributes = new List<Attribute>(moreRequiredAttributes);
	//            foreach(var attr in requiredAttributes)
	//                RequiredAttributes.Add(new Attribute(attr.Name, attr.Value));
	//            if(!string.IsNullOrEmpty(valueOrText))
	//                if(tagName == "input")
	//                    RequiredAttributes.Add(new Attribute("value", valueOrText));
	//                else
	//                    InnerText = valueOrText;
	//        }
	//    }

	//    protected class FilterContext
	//    {
	//        public HtmlResult Context { get; set; }
	//        public SearchIn SearchIn { get; set; }
	//    }

	//    protected class Attribute
	//    {
	//        public string Name { get; set; }
	//        public string Value { get; set; }
	//        public bool PartialMatchValue { get; set; }

	//        public Attribute(string name)
	//        {
	//            Name = name;
	//        }

	//        public Attribute(string name, string value)
	//        {
	//            Name = name;
	//            Value = value;
	//        }
	//    }

	//    protected enum SearchIn
	//    {
	//        All,
	//        Ancestors,
	//        Descendants
	//    }
	//}
}


