using System;
using System.Linq;
using System.Xml.Linq;
using SimpleBrowser;
using SimpleBrowser.Query;
using NUnit.Framework;
using SimpleBrowser.UnitTests;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture, Ignore]
	public class QueryTests
	{
		XDocument GetTestDocument()
		{
			var browser = new Browser();
			// this must be the document that was apparently been available on http://xbrowser.axefrog.com/htmltests/basic2.htm
			browser.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.Axefrog_Basic2.htm"));
			return browser.Select("*").XElement.Document;
		}

		/*
		All Selector (“*”)										Selects all elements.
		:animated Selector										Select all elements that are in the progress of an animation at the time the selector is run.
		Attribute Contains Prefix Selector [name|=value]		Selects elements that have the specified attribute with a value either equal to a given string or starting with that string followed by a hyphen (-).
		Attribute Contains Selector [name*=value]				Selects elements that have the specified attribute with a value containing the a given substring.
		Attribute Contains Word Selector [name~=value]			Selects elements that have the specified attribute with a value containing a given word, delimited by spaces.
		Attribute Ends With Selector [name$=value]				Selects elements that have the specified attribute with a value ending exactly with a given string.
		Attribute Equals Selector [name=value]					Selects elements that have the specified attribute with a value exactly equal to a certain value.
		Attribute Not Equal Selector [name!=value]				Select elements that either don't have the specified attribute, or do have the specified attribute but not with a certain value.
		Attribute Starts With Selector [name^=value]			Selects elements that have the specified attribute with a value beginning exactly with a given string.
		:button Selector										Selects all button elements and elements of type button.
		:checkbox Selector										Selects all elements of type checkbox.
		:checked Selector										Matches all elements that are checked.
		Child Selector (“parent > child”)						Selects all direct child elements specified by "child" of elements specified by "parent".
		Class Selector (“.class”)								Selects all elements with the given class.
		:contains() Selector									Select all elements that contain the specified text.
		Descendant Selector (“ancestor descendant”)				Selects all elements that are descendants of a given ancestor.
		:disabled Selector										Selects all elements that are disabled.
		Element Selector (“element”)							Selects all elements with the given tag name.
		:empty Selector											Select all elements that have no children (including text nodes).
		:enabled Selector										Selects all elements that are enabled.
		:eq() Selector											Select the element at index n within the matched set.
		:even Selector											Selects even elements, zero-indexed. See also odd.
		:file Selector											Selects all elements of type file.
		:first-child Selector									Selects all elements that are the first child of their parent.
		:first Selector											Selects the first matched element.
		:gt() Selector											Select all elements at an index greater than index within the matched set.
		Has Attribute Selector [name]							Selects elements that have the specified attribute, with any value.
		:has() Selector											Selects elements which contain at least one element that matches the specified selector.
		:header Selector										Selects all elements that are headers, like h1, h2, h3 and so on.
		:hidden Selector										Selects all elements that are hidden.
		ID Selector (“#id”)										Selects a single element with the given id attribute.
		:image Selector											Selects all elements of type image.
		:input Selector											Selects all input, textarea, select and button elements.
		:last-child Selector									Selects all elements that are the last child of their parent.
		:last Selector											Selects the last matched element.
		:lt() Selector											Select all elements at an index less than index within the matched set.
		Multiple Attribute Selector [name=value][name2=value2]	Matches elements that match all of the specified attribute filters.
		Multiple Selector (“selector1, selector2, selectorN”)	Selects the combined results of all the specified selectors.
		Next Adjacent Selector (“prev + next”)					Selects all next elements matching "next" that are immediately preceded by a sibling "prev".
		Next Siblings Selector (“prev ~ siblings”)				Selects all sibling elements that follow after the "prev" element, have the same parent, and match the filtering "siblings" selector.
		:not() Selector											Selects all elements that do not match the given selector.
		:nth-child Selector										Selects all elements that are the nth-child of their parent.
		:odd Selector											Selects odd elements, zero-indexed. See also even.
		:only-child Selector									Selects all elements that are the only child of their parent.
		:parent Selector										Select all elements that are the parent of another element, including text nodes.
		:password Selector										Selects all elements of type password.
		:radio Selector											Selects all elements of type radio.
		:reset Selector											Selects all elements of type reset.
		:selected Selector										Selects all elements that are selected.
		:submit Selector										Selects all elements of type submit.
		:text Selector											Selects all elements of type text.
		:visible Selector										Selects all elements that are visible.
*/
		/// <summary>
		/// All Selector (“*”) - Selects all elements.
		/// </summary>
		[Test]
		public void Test_AllSelector()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute("*", doc);
			Assert.AreEqual(doc.Descendants().Count(), elements.Length, "All elements in the document should have been returned");
		}

		/// <summary>
		/// Attribute Contains Prefix Selector [name|=value] - Selects elements that have the specified attribute with a value either equal to a given string or starting with that string followed by a hyphen (-).
		/// Attribute Contains Selector [name*=value] - Selects elements that have the specified attribute with a value containing the a given substring.
		/// Attribute Contains Word Selector [name~=value] - Selects elements that have the specified attribute with a value containing a given word, delimited by spaces.
		/// Attribute Ends With Selector [name$=value] - Selects elements that have the specified attribute with a value ending exactly with a given string.
		/// Attribute Equals Selector [name=value] - Selects elements that have the specified attribute with a value exactly equal to a certain value.
		/// Attribute Not Equal Selector [name!=value] - Select elements that either don't have the specified attribute, or do have the specified attribute but not with a certain value.
		/// Attribute Starts With Selector [name^=value] - Selects elements that have the specified attribute with a value beginning exactly with a given string.
		/// </summary>
		[Test]
		public void Test_AttributeSelector_ContainsPrefix()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute("[href|=http]", doc);
			Assert.AreEqual(1, elements.Count(), "Should have returned exactly one element");
			Assert.AreEqual("http://validator.w3.org/", elements.First().GetAttribute("href"), "Element should have contained the string: http://validator.w3.org/");
			elements = XQuery.Execute("head > meta[content|=text]", doc);
			Assert.AreEqual(1, elements.Count(), "Should have returned exactly one element");
			Assert.AreEqual("text/html; charset=iso-8859-1", elements.First().GetAttribute("content"), "Element should have contained the string: text/html; charset=iso-8859-1");
		}

		/// <summary>
		/// Element Selector (“element”) - Selects all elements with the given tag name.
		/// </summary>
		[Test]
		public void Test_ElementSelector()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute("li", doc);
	
			Assert.AreEqual(7, elements.Length, "Exactly 7 elements should have been returned");
			Assert.IsTrue(elements.All(e => e.Name.LocalName.ToLower() == "li"), "All elements should have been LI");
		}

		/// <summary>
		/// ID Selector (“#id”) - Selects a single element with the given id attribute.
		/// </summary>
		[Test]
		public void Test_IdSelector()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute("#link", doc);
	
			Assert.AreEqual(1, elements.Length, "Exactly 1 element should have been returned");
			Assert.IsTrue(String.Compare(elements[0].Name.LocalName, "a", true)==0, "Element returned should have been an anchor tag");
			Assert.AreEqual("link", elements[0].GetAttributeCI("id"), "Element returned should have had id=link");
		}

		/// <summary>
		/// Class Selector (“.class”) - Selects all elements with the given class.
		/// </summary>
		[Test]
		public void Test_ClassSelector()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute(".highlight", doc);
	
			Assert.AreEqual(2, elements.Length, "Exactly 2 elements should have been returned");
			Assert.IsTrue(elements.All(e => String.Compare(e.Name.LocalName, "p", true) == 0), "Elements returned should have been P tags");
			Assert.IsTrue(elements.All(e => e.GetAttributeCI("class") == "highlight"), "Element returned should have had class=highlight");
		}

		/// <summary>
		/// Child Selector (“parent > child”) - Selects all direct child elements specified by "child" of elements specified by "parent".
		/// </summary>
		[Test]
		public void Test_ChildSelector()
		{
			var doc = GetTestDocument();

			var elements = XQuery.Execute("body > ul > li", doc);
			Assert.AreEqual(3, elements.Length, "Exactly 3 elements should have been returned");
			Assert.IsTrue(elements.All(e => e.Name.LocalName.ToLower() == "li"), "All elements should have been LI");
			Assert.AreEqual(elements[0].ElementsBeforeSelf().Count(), 0, "Element 0 should have been the first child element");
			Assert.AreEqual(elements[0].ElementsAfterSelf().FirstOrDefault(), elements[1], "Element 0 should have been directly before element 1");
			Assert.AreEqual(elements[1].ElementsAfterSelf().FirstOrDefault(), elements[2], "Element 1 should have been directly before element 2");
			Assert.AreEqual(elements[2].ElementsAfterSelf().Count(), 0, "Element 2 should have been the last child element");
		}

		/// <summary>
		/// Descendant Selector (“ancestor descendant”) - Selects all elements that are descendants of a given ancestor.
		/// </summary>
		[Test]
		public void Test_DescendentSelector()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute("ul li", doc);
	
			Assert.AreEqual(4, elements.Length, "Exactly 4 elements should have been returned");
			Assert.IsTrue(elements.All(e => e.Name.LocalName.ToLower() == "li"), "All elements should have been LI");
		}

		[Test]
		public void Query_starting_or_ending_with_transposal_selector_should_throw_exception()
		{
			var doc = GetTestDocument();
			XQueryException exception = null;
			try { XQuery.Execute("> body", doc); }
			catch(XQueryException ex) { exception = ex; }
			Assert.IsNotNull(exception, "XQueryException should have been thrown");
			exception = null;
			try { XQuery.Execute("body >", doc); }
			catch(XQueryException ex) { exception = ex; }
			Assert.IsNotNull(exception, "XQueryException should have been thrown");
			exception = null;
			try { XQuery.Execute("> body >", doc); }
			catch(XQueryException ex) { exception = ex; }
			Assert.IsNotNull(exception, "XQueryException should have been thrown");
		}

		[Test]
		public void Test_CommaSelector()
		{
			var doc = GetTestDocument();
			var elements = XQuery.Execute("ul, ol", doc);
			Assert.AreEqual(3, elements.Length, "Exactly 3 elements should have been returned");
			Assert.AreEqual("ul", elements[0].Name.LocalName.ToLower());
			Assert.AreEqual("ul", elements[1].Name.LocalName.ToLower());
			Assert.AreEqual("ol", elements[2].Name.LocalName.ToLower());
		}
	}
}
