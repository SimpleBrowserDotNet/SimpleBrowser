// -----------------------------------------------------------------------
// <copyright file="CommentElements.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;
	using System.Text;
	using NUnit.Framework;

	/// <summary>
	/// A unit test for the following comment (or comment-like) elements:
	/// <!doctype>, <!-- comment -->, <![CDATA[data]]> and <![conditional]>
	/// </summary>
	[TestFixture]
	public class CommentElements
	{
		/*
		/// <summary>
		/// Tests the HTML doctype element. Note: Unable to test. While the HTML parser is capable of parsing the
		/// doctype, it does not expose it to the browser. This is an issue that should be resolved. When it is,
		/// this test can be un-commented.
		/// </summary>
		[Test]
		public void HtmlElement_Doctype()
		{
			Browser b = new Browser();
			b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.CommentElements.htm"));
			b.Find("link1");
			Assert.That(b.XDocument.DocumentType.ToString(), Is.EqualTo("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"));
		}
		*/

		/// <summary>
		/// Tests HTML comments (well-formed and malformed) as well as those elements that become comments (Microsoft's downlevel revealed condition comments).
		/// </summary>
		[Test]
		public void HtmlElement_Comment()
		{
			Browser b = new Browser();
			b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.CommentElements.htm"));
			b.Find("link1");

			var comments = from node in b.XDocument.Elements().DescendantNodesAndSelf()
						   where node.NodeType == XmlNodeType.Comment
						   select node as XComment;

			var comment = comments.First();
			Assert.That(comment.ToString(), Is.EqualTo("<!-- Valid comment -->"));

			comment = comments.Skip(1).First();
			Assert.That(comment.ToString(), Is.EqualTo("<!-- Malformed comment -->"));

			comment = comments.Skip(2).First();
			Assert.That(comment.ToString(), Is.EqualTo("<!--[if gt IE 9]-->"));

			comment = comments.Skip(3).First();
			Assert.That(comment.ToString(), Is.EqualTo("<!--[endif]-->"));

			comment = comments.Skip(4).First();
			Assert.That(comment.ToString(), Is.EqualTo("<!--[if gt IE 10]>\r\n<a id=\"link2\" href=\"http://www.microsoft.com\">Downlown-level hidden conditional comment test</a>\r\n<![endif]-->"));
		}

		/// <summary>
		/// Tests the CDATA element
		/// </summary>
		public void HtmlElement_Cdata()
		{
			Browser b = new Browser();
			b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.CommentElements.htm"));
			b.Find("link1");

			var comments = from node in b.XDocument.Elements().DescendantNodesAndSelf()
						   where node.NodeType == XmlNodeType.CDATA
						   select node as XCData;
			
			var comment = comments.First();
			Assert.That(comment.ToString(), Is.EqualTo("<![CDATA[Some content]]>"));
		}
	}
}
