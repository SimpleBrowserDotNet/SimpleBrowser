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
	/// A unit test for form and input elements
	/// </summary>
	[TestFixture]
	public class Forms
	{
		/// <summary>
		/// Tests that input elements that handle the maxlength attribute correctly set the value of the attribute.
		/// </summary>
		[Test]
		public void Forms_Input_MaxLength()
		{
			Browser b = new Browser();
			b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));
			HtmlResult maxLength1 = b.Find("maxlength1");
			HtmlResult maxLength2 = b.Find("maxlength2");

			// Test input of type text
			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			maxLength1.Value = "123456789012345";
			Assert.That(maxLength1.Value, Is.EqualTo("1234567890"));

			// Test input of type search
			maxLength1.XElement.SetAttributeCI("type", "search");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			maxLength1.Value = "123456789012345";
			Assert.That(maxLength1.Value, Is.EqualTo("1234567890"));

			// Test input of type password
			maxLength1.XElement.SetAttributeCI("type", "password");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			maxLength1.Value = "123456789012345";
			Assert.That(maxLength1.Value, Is.EqualTo("1234567890"));

			// Test input of type tel
			maxLength1.XElement.SetAttributeCI("type", "tel");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			maxLength1.Value = "123456789012345";
			Assert.That(maxLength1.Value, Is.EqualTo("1234567890"));

			// Test input of type url
			maxLength1.XElement.SetAttributeCI("type", "url");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			maxLength1.Value = "123456789012345";
			Assert.That(maxLength1.Value, Is.EqualTo("1234567890"));

			// Test input of type email
			maxLength1.XElement.SetAttributeCI("type", "email");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			maxLength1.Value = "123456789012345";
			Assert.That(maxLength1.Value, Is.EqualTo("1234567890"));

			// Test input with a maxlength value of 0
			maxLength1.XElement.SetAttributeCI("maxlength", "0");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo(string.Empty));

			// Test input with a negative maxlength value
			maxLength1.XElement.SetAttributeCI("maxlength", "-1");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			// Test input with an invalid maxlength value
			maxLength1.XElement.SetAttributeCI("maxlength", "invalid");

			maxLength1.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));

			// Test textarea
			maxLength2.Value = "12345";
			Assert.That(maxLength2.Value, Is.EqualTo("12345"));

			maxLength2.Value = "123456789012345";
			Assert.That(maxLength2.Value, Is.EqualTo("1234567890"));

			// Test textarea with a maxlength value of 0
			maxLength2.XElement.SetAttributeCI("maxlength", "0");

			maxLength2.Value = "12345";
			Assert.That(maxLength2.Value, Is.EqualTo(string.Empty));

			// Test textarea with a negative maxlength value
			maxLength2.XElement.SetAttributeCI("maxlength", "-1");

			maxLength2.Value = "12345";
			Assert.That(maxLength2.Value, Is.EqualTo("12345"));

			// Test textarea with an invalid maxlength value
			maxLength2.XElement.SetAttributeCI("maxlength", "invalid");

			maxLength2.Value = "12345";
			Assert.That(maxLength1.Value, Is.EqualTo("12345"));
		}
	}
}
