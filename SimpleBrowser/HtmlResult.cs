using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using SimpleBrowser.Query;

namespace SimpleBrowser
{
	/// <summary>
	/// This class is the standard return type for "find" operations performed on a <see cref="Browser" /> object.
	/// Rather than returning a collection of results, this class provides a more fluent mechanism for assessing the
	/// results, as it is more common that we're only interested in a single result, rather than any subsequent matches.
	/// A find result should never throw an exception and will never return null. Instead. check the Exists property
	/// of the result to find out if the requested element exists. If you prefer to use a traditional IEnumerable
	/// approach, simply iterate through the collection in the normal way. When doing this, the Exists property does not
	/// need to be checked as the enumerator will only return existing matched elements.
	/// </summary>
	public class HtmlResult : IEnumerable<HtmlResult>
	{
		private HtmlElement _current;
		private List<HtmlElement> _list;
		private readonly Browser _browser;
		int _index;

		internal HtmlResult(List<HtmlElement> results, Browser browser)
		{
			_current = results.Count > 0 ? results[0] : null;
			_list = results;
			_browser = browser;
			_browser.Log("New HTML result set obtained, containing " + results.Count + " element(s)", LogMessageType.Internal);
		}

		internal HtmlResult(HtmlElement result, Browser browser)
		{
			_current = result;
			_browser = browser;
			_list = new List<HtmlElement>(new[] { result });
		}

		internal HtmlElement CurrentElement
		{
			get { return _current; }
		}

		/// <summary>
		/// Returns a new result set derived from the current element, using jQuery selector syntax
		/// </summary>
		public HtmlResult Select(string query)
		{
			AssertElementExists();
			return new HtmlResult(XQuery.Execute("* " + query, _browser.XDocument, _current.XElement).Select(_browser.CreateHtmlElement).ToList(), _browser);
		}

		/// <summary>
		/// Returns a new result set derived from the current set of elements, using jQuery selector syntax
		/// </summary>
		public HtmlResult Refine(string query)
		{
			return new HtmlResult(XQuery.Execute(query, _browser.XDocument, _list.Select(h => h.XElement).ToArray()).Select(_browser.CreateHtmlElement).ToList(), _browser);
		}

		/// <summary>
		/// Returns a reference to the underlying XElement object to allow for further analysis that is beyond the
		/// scope of the existing SimpleBrowser codebase
		/// </summary>
		public XElement XElement { get { return _current.XElement; } }

		private void AssertElementExists()
		{
			if(_current == null)
				throw new InvalidOperationException("The requested operation is not available when Exists is false");
			if (!_current.Valid)
				throw new InvalidOperationException("The requested operation is not available. Navigating makes the existing HtmlResult objects invalid.");
		}
		private void AssertElementIsNotDisabled()
		{
			if(_current.Disabled)
				throw new InvalidOperationException("The requested operation is not available on disabled elements");
		}

		/// <summary>
		/// Attempts to move to the next element that matched the query that generated this HtmlResult object. No error
		/// will be thrown if there are no more matching elements, but <see cref="Exists" /> should be checked before
		/// attempting to perform any further operations.
		/// </summary>
		/// <returns>True if there was another element, or False if we're no longer pointing at an element</returns>
		public bool Next()
		{
			if(_index < _list.Count - 1)
			{
				_current = _list[++_index];
				return true;
			}
			_current = null;
			return false;
		}

		/// <summary>
		/// The total number of elements that matched the query that generated this HtmlResult object
		/// </summary>
		public int TotalElementsFound
		{
			get { return _list.Count; }
		}

		/// <summary>
		/// Specifies whether the HtmlResult object is pointing at an element to perform operations on.
		/// </summary>
		public bool Exists
		{
			get { return _current != null; }
		}

		/// <summary>
		/// Indicates whether the element contains a "disabled" attribute. Throws an exception if Exists is false.
		/// </summary>
		public bool Disabled
		{
			get
			{
				AssertElementExists();
				return _current.Disabled;
			}
		}

		/// <summary>
		/// Gets or sets the value of the element. If the element is of type INPUT, the VALUE attribute is used. If
		/// the element is of type SELECT, the value of selected option (or the first option if none are explicitly
		/// selected) will be used. If no VALUE attribute exists on the selected option, the text in the option element
		/// will be used. If the element is of type TEXTAREA, then inner text will be used, as opposed to the VALUE
		/// attribute, as this is how the value is set and retrieved structurally in the HTML. The inner text is used
		/// for all other element types, making this property useful for returning the text contained in any arbitrary
		/// HTML element. Setting the value of an element that is not used in form submission serves no useful purpose
		/// other than basic modification of the current underlying XDocument structure.
		/// </summary>
		public string Value
		{
			get
			{
				AssertElementExists();
				return _current.Value;
			}
			set
			{
				AssertElementExists();
				_browser.Log("Setting the value of " + HttpUtility.HtmlEncode(XElement.ToString()) + " to " + HttpUtility.HtmlEncode(value.ShortenTo(30, true)), LogMessageType.Internal);
				_current.Value = value;
			}
		}

        /// <summary>
        /// Gets the decoded Value of the element. For example if the Value is &copy; 2011 the decoded Value will 
        /// be © 2011
        /// </summary>
        public string DecodedValue
        {
            get { return HttpUtility.HtmlDecode(Value); }
        }

		/// <summary>
		/// Gets or sets whether or not the CHECKED attribute is set for the current element.  Throws an exception if
		/// Exists is false or if the current element is anything other than a RADIO or CHECKBOX (INPUT) element.
		/// </summary>
		public bool Checked
		{
			get
			{
				AssertElementExists();
				return _current.Selected;
			}
			set
			{
				AssertElementExists();
				_browser.Log("Setting the checked state of " + HttpUtility.HtmlEncode(XElement.ToString()) + " to " + (value ? "CHECKED" : "UNCHECKED"), LogMessageType.Internal);
				_current.Selected = value;
			}
		}

		/// <summary>
		/// Simulates a click on an element, which has differing effects depending on the element type. If the element
		/// is a BUTTON or INPUT TYPE=SUBMIT or INPUT TYPE=IMAGE element, the current form (if any) will be submitted,
		/// with the name/value of the clicked element being used in the submission values where relevant. If the
		/// element is a checkbox, the CHECKED value will be toggled on or off. If the element is a radio button, other
		/// radio buttons in the group will have their CHECKED attribute removed and the current element will have its
		/// CHECKED element set.
		/// NOTE: If the element IS an INPUT TYPE=IMAGE element, this method will "click" the image input as though the
		/// element had focus and the space bar or enter key was pressed to activate the element, performing the click.
		/// </summary>
		public ClickResult Click()
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_browser.Log("Clicking element: " + HttpUtility.HtmlEncode(XElement.ToString()), LogMessageType.Internal);
			return _current.Click();
		}

		/// <summary>
		/// Simulates a click on an element at the specified coorinates, which has differing effects depending on the
		/// element type. If the element IS an INPUT TYPE=IMAGE element, this method will "click" the image input as
		/// though the element had been clicked with a pointing device (i.e., a mouse) at the specified coordinates.
		/// If the element does not support being clicked at specified coordinates (i.e., the element IS NOT an INPUT
		/// TYPE=IMAGE element), the element will be clicked as though the Click() method (without parameters) been called.
		/// </summary>
		public ClickResult Click(uint x, uint y)
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_browser.Log("Clicking element: " + HttpUtility.HtmlEncode(XElement.ToString()), LogMessageType.Internal);
			return _current.Click(x, y);
		}

		/// <summary>
		/// This method can be used on any element contained within a form, or the form element itself. The form will be 
		/// serialized and submitted as close as possible to the way it would be in a normal browser request. In
		/// addition, any values currently in the ExtraFormValues property of the Browser object will be submitted as
		/// well.
		/// </summary>
		/// <param name="url">Optional. If specified, the form will be submitted to this URL instead.</param>
		public bool SubmitForm(string url = null)
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_browser.Log("Submitting parent/ancestor form of: " + HttpUtility.HtmlEncode(XElement.ToString()), LogMessageType.Internal);
			return _current.SubmitForm(url);
		}

		/// <summary>
		/// This method is designed for use on Asp.Net WebForms sites where the anchor link being clicked only has a postback
		/// javascript function as its method of navigating to the next page.
		/// </summary>
		public ClickResult DoAspNetLinkPostBack()
		{
			AssertElementExists();
			_browser.Log("Performing ASP.Net postback click for : " + HttpUtility.HtmlEncode(XElement.ToString()), LogMessageType.Internal);
			return _current.DoAspNetLinkPostBack();
		}

		/// <summary>
		/// return the value of the specified attribute. Throws an exception if Exists is false.
		/// </summary>
		public string GetAttribute(string name)
		{
			AssertElementExists();
			return _current.GetAttributeValue(name);
		}

		public IEnumerator<HtmlResult> GetEnumerator()
		{
			foreach(var el in _list)
				yield return new HtmlResult(el, _browser);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}


