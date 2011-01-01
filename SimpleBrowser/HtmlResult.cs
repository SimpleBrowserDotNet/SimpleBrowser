using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
		int _index;

		internal HtmlResult(List<HtmlElement> results)
		{
			_current = results.Count > 0 ? results[0] : null;
			_list = results;
		}

		internal HtmlResult(HtmlElement result)
		{
			_current = result;
			_list = new List<HtmlElement>(new[] { result });
		}

		internal HtmlElement CurrentElement
		{
			get { return _current; }
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
		}
		private void AssertElementIsInputType(params string[] type)
		{
			if(_current.TagName.ToLower() == "input" && type.Contains(_current.InputType))
				return;
			throw new InvalidOperationException("The requested operation is only valid on input elements of type(s) " + type.Concat(", "));
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
				_current.Value = value;
			}
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
				AssertElementIsInputType("radio", "checkbox");
				return _current.Checked;
			}
			set
			{
				AssertElementExists();
				AssertElementIsInputType("radio", "checkbox");
				_current.Checked = value;
			}
		}

		/// <summary>
		/// Simulates a click on an element, which has differing effects depending on the element type. If the element
		/// is a BUTTON or INPUT TYPE=SUBMIT element, the current form (if any) will be submitted, with the name/value
		/// of the clicked element being used in the submission values where relevant. If the element is a checkbox,
		/// the CHECKED value will be toggled on or off. If the element is a radio button, other radio buttons in the
		/// group will have their CHECKED attribute removed and the current element will have its CHECKED element set.
		/// If the element is a link, the 
		/// </summary>
		public void Click()
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_current.Click();
		}

		/// <summary>
		/// This method therefore can be used on any element contained within a form, or the form element itself. The
		/// form will be serialized and submitted as close as possible to the way it would be in a normal browser
		/// request. In addition, any values currently in the ExtraFormValues property of the Browser object will be
		/// submitted as well.
		/// </summary>
		public void SubmitForm()
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_current.SubmitForm();
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
				yield return new HtmlResult(el);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}


