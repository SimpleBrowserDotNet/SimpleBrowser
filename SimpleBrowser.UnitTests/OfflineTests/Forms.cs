// -----------------------------------------------------------------------
// <copyright file="Forms.cs" company="SimpleBrowser">
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using NUnit.Framework;

    /// <summary>
    /// A unit test for form and input elements
    /// </summary>
    [TestFixture]
    public class Forms
    {
        /// <summary>
        /// Tests handling of malformed HTML in select and option elements.
        /// </summary>
        [Test]
        public void Forms_Malformed_Select()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));

            // Test the malformed option.
            HtmlResult options = b.Find("malformedOption1");
            Assert.IsNotNull(options);
            Assert.IsTrue(options.Exists);

            options.Value = "3";
            Assert.That(options.Value == "3");

            // Test the malformed option group
            IEnumerable<XElement> groups = options.XElement.Elements("optgroup");
            Assert.That(groups.Count() == 2);

            XElement group = groups.First();
            Assert.That(group.Elements("option").Count() == 4);

            group = groups.ElementAt(1);
            Assert.That(group.Elements("option").Count() == 3);

            options = b.Find("malformedOption2");
            Assert.IsNotNull(options);
            Assert.IsTrue(options.Exists);

            options.Value = "3";
            Assert.That(options.Value != "3");
            Assert.That(options.Value == "4");

            options.Value = "V";
            Assert.That(options.Value == "V");

            // Test the malformed option group
            groups = options.XElement.Elements("optgroup");
            Assert.That(groups.Count() == 2);

            group = groups.First();
            Assert.That(group.Elements("option").Count() == 2);

            group = groups.ElementAt(1);
            Assert.That(group.Elements("option").Count() == 5);
        }

        /// <summary>
        /// Tests that input elements that handle the $maxlength$ attribute correctly set the value of the attribute.
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

        /// <summary>
        /// Tests that form elements properly handle the disabled and read only attributes.
        /// </summary>
        [Test]
        public void Forms_Validate_Input_Elements()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test text input properties
            var testinput = b.Find("textinput");
            testinput.Value = "text input updated";
            Assert.IsTrue(testinput.Value == "text input updated");
            Assert.IsTrue(testinput.TotalElementsFound == 1);
            Assert.IsFalse(testinput.Checked);

            // Test text area properties
            testinput = b.Find("textareainput");
            testinput.Value = "text area input updated";
            Assert.IsTrue(testinput.Value == "text area input updated");
            Assert.IsTrue(testinput.TotalElementsFound == 1);
            Assert.IsFalse(testinput.Checked);

            // Test checkbox input properties
            testinput = b.Find("checkboxinput");
            string ads = testinput.Value;
            testinput.Value = "text area input updated";
            Assert.IsTrue(testinput.Value == "text area input updated");
            Assert.IsTrue(testinput.TotalElementsFound == 1);
            Assert.IsFalse(testinput.Checked);

            // Submit the form
            HtmlResult submit = b.Find("es");
            var clickResult = submit.Click();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            var names = b.Select("td.desc");
            var values = b.Select("td.val");
            Assert.IsTrue(names.Count() == values.Count());

            // Check to make sure the proper values submitted
            Assert.IsTrue(values.Where(e => e.Value == "text input updated").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "text area input updated").FirstOrDefault() != null);
        }

        /// <summary>
        /// Tests that form elements properly handle the disabled and read only attributes.
        /// </summary>
        [Test]
        public void Forms_Disabled_and_ReadOnly()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            var textarea = b.Find("readonlytextarea");
            textarea.Value = "some value";
            Assert.IsTrue(textarea.Value == "readme textarea");
            Assert.IsTrue(textarea.ReadOnly);
            Assert.IsFalse(textarea.Disabled);

            textarea = b.Find("disabledtextarea");
            textarea.Value = "some value";
            Assert.IsTrue(textarea.Value == "disableme textarea");
            Assert.IsFalse(textarea.ReadOnly);
            Assert.IsTrue(textarea.Disabled);

            var textinput = b.Find("readonlytext");
            textinput.Value = "some value";
            Assert.IsTrue(textinput.Value == "readme");
            Assert.IsTrue(textinput.ReadOnly);
            Assert.IsFalse(textinput.Disabled);

            textinput = b.Find("disabledtext");
            textinput.Value = "some value";
            Assert.IsTrue(textinput.Value == "disableme");
            Assert.IsFalse(textinput.ReadOnly);
            Assert.IsTrue(textinput.Disabled);

            var checkbox = b.Find("disabledcheck");
            Assert.IsTrue(checkbox.Disabled);

            var radio = b.Find("disabledradio");
            Assert.IsTrue(radio.Disabled);

            HtmlResult disabledSubmit = b.Find("ds");
            Assert.IsTrue(disabledSubmit.Disabled);
            ClickResult clickResult = disabledSubmit.Click();
            Assert.IsTrue(clickResult == ClickResult.SucceededNoOp);

            HtmlResult submit = b.Find("es");
            Assert.IsFalse(submit.Disabled);
            clickResult = submit.Click();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            var names = b.Select("td.desc");
            var values = b.Select("td.val");
            Assert.IsTrue(names.Count() == values.Count());
            Assert.IsTrue(values.Where(e => e.Value == "readme textarea").FirstOrDefault() != null);

            // Now, validate that the disabled fields were not.
            Assert.IsTrue(values.Where(e => e.Value == "disableme textarea").FirstOrDefault() == null);
            Assert.IsTrue(values.Where(e => e.Value == "disableme").FirstOrDefault() == null);
            Assert.IsTrue(values.Where(e => e.Value == "disabledcheck").FirstOrDefault() == null);
            Assert.IsTrue(values.Where(e => e.Value == "disabledradio").FirstOrDefault() == null);
            Assert.IsTrue(values.Where(e => e.Value == "disabledselect").FirstOrDefault() == null);
        }

        /// <summary>
        /// Tests that input elements that handle the $maxlength$ attribute correctly set the value of the attribute.
        /// </summary>
        [Test]
        public void Forms_Input_Types()
        {
            // Initialize browser and load content.
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));

            // Test ability to find HTML5 elements.
            // Find by ID.
            HtmlResult colorBox = b.Find("colorBox");
            Assert.IsNotNull(colorBox);
            Assert.IsTrue(colorBox.Exists);

            // Find by name.
            // Any undefined or unknown type is considered a text field.
            colorBox = b.Find(ElementType.TextField, FindBy.Name, "colorBox");
            Assert.IsNotNull(colorBox);
            Assert.IsTrue(colorBox.Exists);
        }

        /// <summary>
        /// Tests that input elements with the form attribute submit with the correct form.
        /// </summary>
        [Test]
        public void Forms_Form_Attribute()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // This test covers the following cases:
            // 1. An input outside of the form with the form attribute is submitted.
            // 2. An input in another form with the form attribute is submitted.
            var field1 = b.Find("field1");
            field1.Value = "Name1";

            var field1a = b.Find("field1a");
            field1a.Value = "Name1a";

            var field2 = b.Find("field2");
            field2.Value = "Name2";

            var submit1 = b.Find("submit1");
            ClickResult clickResult = submit1.Click();

            // Check to make sure the form submitted.
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            var names = b.Select("td.desc");
            var values = b.Select("td.val");
            Assert.IsTrue(names.Count() == values.Count());
            Assert.IsTrue(values.Where(e => e.Value == "Name1").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "Name2").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "Name1a").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "Submit1").FirstOrDefault() != null);

            // This test covers the following cases:
            // 1. An input in the form with a form element corresponding to another form is not submitted.
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            field2 = b.Find("field2");
            field2.Value = "Name2";

            var field2a = b.Find("field2a");
            field2a.Value = "Name2a";

            var submit2 = b.Find("submit2");
            clickResult = submit2.Click();

            // Check to make sure the form submitted.
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            names = b.Select("td.desc");
            values = b.Select("td.val");
            Assert.IsTrue(values.Where(e => e.Value == "Submit2").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "Name2a").FirstOrDefault() != null);
            Assert.IsFalse(values.Where(e => e.Value == "Name2").FirstOrDefault() != null);
        }
    }
}
