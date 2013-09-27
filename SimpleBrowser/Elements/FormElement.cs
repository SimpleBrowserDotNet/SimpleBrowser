using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace SimpleBrowser.Elements
{
	internal class FormElement : HtmlElement
	{
		public FormElement(XElement element)
			: base(element)
		{
		}
		private IEnumerable<FormElementElement> _elements = null;
		public IEnumerable<FormElementElement> Elements
		{
			get
			{
				if (_elements == null)
				{
					var formElements = Element.Descendants()
						.Where(e => new string[]{"select", "input", "button", "textarea"}.Contains(e.Name.LocalName.ToLower()))
						.Select(e => this.OwningBrowser.CreateHtmlElement<FormElementElement>(e));
					_elements = formElements;
				}
				return _elements;
			}
		}
		public string Action
		{
			get
			{
				var actionAttr = GetAttribute(Element, "action");
				return actionAttr == null ? "." : actionAttr.Value;
			}
		}
		public string Method
		{
			get
			{
				var attr = GetAttribute(Element, "method");
				return attr == null ? "GET" : attr.Value.ToUpper();
			}
		}

		public override bool SubmitForm(string url = null, HtmlElement clickedElement = null)
		{
			//return base.SubmitForm(url, clickedElement);
			return Submit(url, clickedElement);
		}

		private bool Submit(string url = null, HtmlElement clickedElement = null)
		{
			NavigationArgs navigation = new NavigationArgs();
			navigation.Uri = url ?? this.Action;
			navigation.Method = this.Method;
			navigation.ContentType = FormEncoding.FormUrlencode;
			foreach (var entry in Elements.SelectMany(e => 
					{
						bool isClicked = false;
						if (clickedElement != null && clickedElement.Element == e.Element) isClicked = true;
						return e.ValuesToSubmit(isClicked);
					}
					))
			{
				// This call to Remove() guarantees that for each element with a duplicate name
				// only the last element on the form is submitted.
				navigation.UserVariables.Remove(entry.Name);
				navigation.UserVariables.Add(entry.Name, entry.Value);
			}
			if (this.EncType == FormEncoding.MultipartForm && this.Method.ToUpper() == "POST" )
			{
				// create postdata according to multipart specs
				Guid token = Guid.NewGuid();
				navigation.UserVariables = null;
				StringBuilder post = new StringBuilder();
				foreach (var element in this.Elements)
				{
					if (element is IHasRawPostData)
					{
						post.AppendFormat("--{0}\r\n", token);
						string filename = new FileInfo(element.Value).Name;
						post.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n{2}\r\n", element.Name, filename, ((IHasRawPostData)element).GetPostData());
					}
					else
					{
						bool isClickedElement = false;
						if (clickedElement != null)
						{
							isClickedElement = element.Element == clickedElement.Element;
						}
						var values = element.ValuesToSubmit(isClickedElement);
						foreach (var value in values)
						{
							post.AppendFormat("--{0}\r\n", token);
							post.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n", value.Name, value.Value);
						}
					}
				}
				post.AppendFormat("--{0}\r\n", token);
				navigation.PostData = post.ToString();
				navigation.ContentType = FormEncoding.MultipartForm + "; boundary=" + token;

			}
			return RequestNavigation(navigation);
		}
		public string EncType
		{
			get
			{
				string val = this.GetAttributeValue("enctype");
				if (val == null) val = FormEncoding.FormUrlencode;
				return val;
			}
		}
		public static class FormEncoding
		{
			public const string FormUrlencode = "application/x-www-form-urlencoded";
			public const string MultipartForm = "multipart/form-data";
		}
	}
}
