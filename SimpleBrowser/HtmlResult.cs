using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SimpleBrowser;

namespace SimpleBrowser
{
	public interface IElement
	{
		string TagName { get; }
		bool Disabled { get; }
		bool Checked { get; set; }
		/// <summary>
		/// The "value" attribute for input and textarea elements, or the InnerText value for other elements
		/// </summary>
		string Value { get; set; }
		string InputType { get; }
		void Click();
		void SubmitForm();
		string GetAttribute(string name);
		XElement XElement { get; }
	}

	public class HtmlResult
	{
		private IElement _current;
		private List<IElement> _list;
		int _index = 0;

		public HtmlResult(List<IElement> results)
		{
			_current = results.Count > 0 ? results[0] : null;
			_list = results;
		}

		public HtmlResult(IElement result)
		{
			_current = result;
			_list = new List<IElement>(new[] { result });
		}

		internal IElement Element
		{
			get { return _current; }
		}

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

		public int TotalElementsFound
		{
			get { return _list.Count; }
		}

		public bool Exists
		{
			get { return _current != null; }
		}

		public bool Disabled
		{
			get
			{
				AssertElementExists();
				return _current.Disabled;
			}
		}

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

		public void Click()
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_current.Click();
		}

		public void SubmitForm()
		{
			AssertElementExists();
			AssertElementIsNotDisabled();
			_current.SubmitForm();
		}

		public string GetAttribute(string name)
		{
			AssertElementExists();
			return _current.GetAttribute(name);
		}
	}
}


