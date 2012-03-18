using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SimpleBrowser.Elements;

namespace SimpleBrowser
{
	internal class HtmlElement
	{
		private readonly XElement _element;

		public HtmlElement(XElement element)
		{
			_element = element;
		}

		protected XAttribute GetAttribute(string name)
		{
			return GetAttribute(Element, name);
		}

		public XElement XElement
		{
			get { return _element; }
		}

		protected XAttribute GetAttribute(XElement x, string name)
		{
			return x.Attributes().Where(h => h.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
		}

		internal string GetAttributeValue(string name)
		{
			return GetAttributeValue(Element, name);
		}

		protected string GetAttributeValue(XElement x, string name)
		{
			var attr = GetAttribute(x, name);
			return attr == null ? null : attr.Value;
		}

		public string TagName
		{
			get { return Element.Name.LocalName; }
		}
		public virtual string InputType
		{
			get { throw new InvalidOperationException("Not an input element"); }
		}

		public bool Disabled
		{
			get { return GetAttribute("disabled") != null; }
		}

		/// <summary>
		/// Selected returns a boolean value reflecting to state of certain input element. In the Checkbox and Radiobutton
		/// it corresponds to the 'checked' attribute, in an option under a selectbox, it more or less reflects the 
		/// 'selected' attribute
		/// </summary>
		public virtual bool Selected
		{
			get { throw new InvalidOperationException("This element cannot be checked"); }
			set { throw new InvalidOperationException("This element cannot be checked"); }
		}


		public event Func<HtmlElement, ClickResult> Clicked;
		public event Func<HtmlElement, string, bool> FormSubmitted;
		public event Action<HtmlElement, string> AspNetPostBackLinkClicked;

		public virtual ClickResult Click()
		{
			if(Clicked != null)
				return Clicked(this);

			return ClickResult.SucceededNoOp;
		}

		public bool SubmitForm(string url = null)
		{
			if(FormSubmitted != null)
				return FormSubmitted(this, url);

			return false;
		}

		public void DoAspNetLinkPostBack()
		{
			if(TagName == "a")
			{
				var match = Regex.Match(GetAttributeValue("href"), @"javascript\:__doPostBack\('([^\']*)\'");
				if(match.Success)
				{
					var name = match.Groups[1].Value;
					if(AspNetPostBackLinkClicked != null)
						AspNetPostBackLinkClicked(this, name);
					return;
				}
			}
			throw new InvalidOperationException("This method must only be called on <a> elements having a __doPostBack javascript call in the href attribute");
		}

		public virtual string Value
		{
			get
			{
				return Element.Value;
			}
			set
			{
				throw new InvalidOperationException("Can only set the Value attribute for select, textarea and input elements");
			}
		}

		internal XElement Element
		{
			get { return _element; }
		}

		internal static T CreateFor<T>(XElement element) where T : HtmlElement
		{
			var result = CreateFor(element);
			if (result is T)
			{
				return (T)result;
			}
			throw new InvalidOperationException("The element was not of the corresponding type");
		}
		internal static HtmlElement CreateFor(XElement element)
		{
			switch (element.Name.LocalName.ToLower())
			{
				case "form":
					return new FormElement(element);
				case "input":
					string type = element.GetAttribute("type");
					switch (type.ToLower())
					{
						case "radio":
							return new RadioInputElement(element);
						case "checkbox":
							return new CheckboxInputElement(element);
						case "submit":
						case "image":
						case "button":
							return new InputElement(element);
						default:
							return new InputElement(element);
					}
				case "textarea":
					return new TextAreaElement(element);
				case "select":
					return new SelectElement(element);
				case "option":
					return new OptionElement(element);
				case "a":
					return new AnchorElement(element);
				case "label":
					return new LabelElement(element);
				default:
					return new HtmlElement(element);
			}
		}
	}
}


