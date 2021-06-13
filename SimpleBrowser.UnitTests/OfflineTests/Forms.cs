// -----------------------------------------------------------------------
// <copyright file="Forms.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using NUnit.Framework;

    /// <summary>
    /// A unit tests for form and input elements
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
        /// Tests that text input elements that handle the $maxlength$ attribute correctly set the value
        /// </summary>
        /// <remarks>
        /// An input element with a $maxlength$ attribute should only allow the value to be set to a number of characters equal to or less than the value of the $maxlength$ attribute.
        /// </remarks>
        /// <param name="type">The type of the input element</param>
        /// <param name="maxlength">The maximum length allowed to be entered into the input element or null if none.</param>
        /// <param name="value">The value to assign to the input element</param>
        /// <param name="expectedValue">The expected value of the input element after the assignment.</param>
        [Test]
        [TestCase("text", null, "12345", "12345")]
        [TestCase("text", null, "123456789012345", "1234567890")]
        [TestCase("search", null, "12345", "12345")]
        [TestCase("search", null, "123456789012345", "1234567890")]
        [TestCase("password", null, "12345", "12345")]
        [TestCase("password", null, "123456789012345", "1234567890")]
        [TestCase("tel", null, "12345", "12345")]
        [TestCase("tel", null, "123456789012345", "1234567890")]
        [TestCase("url", null, "12345", "12345")]
        [TestCase("url", null, "123456789012345", "1234567890")]
        [TestCase("email", null, "12345", "12345")]
        [TestCase("email", null, "123456789012345", "1234567890")]
        [TestCase("text", "0", "12345", "")]
        [TestCase("text", "-1", "12345", "12345")]
        [TestCase("text", "invalid", "12345", "12345")]
        public void FormsTextInputElement_SetMaxLength_TextValue(string type, string maxlength, string value, string expectedValue)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));
            HtmlResult textInput = b.Find("maxlength1");

            textInput.XElement.SetAttributeCI("type", type);

            if (!string.IsNullOrWhiteSpace(maxlength))
            {
                textInput.XElement.SetAttributeCI("maxlength", maxlength);
            }

            textInput.Value = value;
            Assert.AreEqual(textInput.Value, expectedValue);
        }

        /// <summary>
        /// Tests that text area elements that handle the $maxlength$ attribute correctly set the value of the attribute.
        /// </summary>
        /// <remarks>
        /// An text area element with a $maxlength$ attribute should only allow the value to be set to a number of characters equal to or less than the value of the $maxlength$ attribute.
        /// </remarks>
        /// <param name="maxlength">The maximum length allowed to be entered into the text area element or null if none.</param>
        /// <param name="value">The value to assign to the text area element</param>
        /// <param name="expectedValue">The expected value of the text area element after the assignment.</param>
        [Test]
        [TestCase(null, "12345", "12345")]
        [TestCase(null, "123456789012345", "1234567890")]
        [TestCase("0", "12345", "")]
        [TestCase("-1", "12345", "12345")]
        [TestCase("invalid", "12345", "12345")]
        public void FormsTextAreaInputElement_SetMaxLength_TextValue(string maxlength, string value, string expectedValue)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));
            HtmlResult textArea = b.Find("maxlength2");

            if (!string.IsNullOrWhiteSpace(maxlength))
            {
                textArea.XElement.SetAttributeCI("maxlength", maxlength);
            }

            // Test textarea
            textArea.Value = value;
            Assert.AreEqual(textArea.Value, expectedValue);
        }

        /// <summary>
        /// Test successful submission of an e-mail address that may or may not be populated, required, mutilple or have a maximum or minimum length requirement
        /// </summary>
        /// <param name="emailValue">The e-mail address value</param>
        /// <param name="required">A value indicating if the input element is requried</param>
        /// <param name="multiple">A value indicating if the input element supports multiple value entry</param>
        /// <param name="minimumLength">The minimum length of the input element</param>
        /// <param name="maximumLength">The maximum length of the input element</param>
        [Test]
        [TestCase("", false, false, null, null)]
        [TestCase("", false, true, null, null)]
        [TestCase("a@b.com", false, false, null, null)]
        [TestCase("a@b.com", true, false, null, null)]
        [TestCase("a@b.com", false, false, 10, null)]
        [TestCase("a@b.com", false, false, 10, 1)]
        [TestCase("a@b.com", true, false, 10, null)]
        [TestCase("a@b.com", true, false, 10, 1)]
        [TestCase("a@b.com,", false, true, null, null)]
        [TestCase("a@b.com,b@c.com", false, true, null, null)]
        [TestCase("a@b.com,b@c.com", true, true, null, null)]
        [TestCase("a@b.com,b@c.com", false, true, 20, null)]
        [TestCase("a@b.com,b@c.com", true, true, null, 1)]
        [TestCase("a@b.com,b@c.com", false, true, 20, 1)]
        public async Task FormsEmailInputElement_SubmitForm_SubmitSucceeds(string emailValue, bool required, bool multiple, int? maximumLength, int? minimumLength)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            HtmlResult email = b.Find("email");

            if (required)
            {
                email.XElement.SetAttributeCI("required", "");
            }

            if (multiple)
            {
                email.XElement.SetAttributeCI("multiple", "");
            }

            if (maximumLength.HasValue)
            {
                email.XElement.SetAttributeCI("maxlength", maximumLength.ToString());
            }

            if (minimumLength.HasValue)
            {
                email.XElement.SetAttributeCI("minlength", minimumLength.ToString());
            }

            email.Value = emailValue;

            Assert.IsTrue(await email.SubmitFormAsync());
        }

        /// <summary>
        /// Test failed submission of an e-mail address that may or may not be populated, required, mutilple or have a maximum or minimum length requirement
        /// </summary>
        /// <param name="emailValue">The e-mail address value</param>
        /// <param name="required">A value indicating if the input element is requried</param>
        /// <param name="multiple">A value indicating if the input element supports multiple value entry</param>
        /// <param name="minimumLength">The minimum length of the input element</param>
        /// <param name="maximumLength">The maximum length of the input element</param>
        [Test]
        [TestCase("a@b.com,invalid", false, true, null, null)]
        [TestCase("a@b.com,invalid", true, true, null, null)]
        [TestCase("a@b.com,", true, true, null, null)]
        [TestCase("a@b.com", false, false, 1, null)]
        [TestCase("", true, false, null, null)]
        [TestCase("invalid", false, false, null, null)]
        [TestCase("invalid", true, false, null, null)]
        [TestCase("a@b.com,b@c.com", true, true, 1, null)]
        [TestCase("a@b.com,b@c.com", true, true, null, 30)]
        [TestCase("a@b.com,b@c.com", true, true, 1, 30)]
        [TestCase("a@b.com,b@c.com", false, true, 1, null)]
        [TestCase("a@b.com,b@c.com", false, true, null, 30)]
        [TestCase("a@b.com,b@c.com", false, true, 1, 30)]
        [TestCase("a@b.com,b@c.com", true, false, 1, null)]
        [TestCase("a@b.com,b@c.com", true, false, null, 30)]
        [TestCase("a@b.com,b@c.com", true, false, 1, 30)]
        [TestCase("a@b.com,b@c.com", false, false, 1, null)]
        [TestCase("a@b.com,b@c.com", false, false, null, 30)]
        [TestCase("a@b.com,b@c.com", false, false, 1, 30)]
        public async Task FormsEmailInputElement_Invoke_SubmitFails(string emailValue, bool required, bool multiple, int? maximumLength, int? minimumLength)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            HtmlResult email = b.Find("email");

            if (required)
            {
                email.XElement.SetAttributeCI("required", "");
            }

            if (multiple)
            {
                email.XElement.SetAttributeCI("multiple", "");
            }

            if (maximumLength.HasValue)
            {
                email.XElement.SetAttributeCI("maxlength", maximumLength.ToString());
            }

            if (minimumLength.HasValue)
            {
                email.XElement.SetAttributeCI("minlength", minimumLength.ToString());
            }

            email.Value = emailValue;

            Assert.IsFalse(await email.SubmitFormAsync());
        }

        /// <summary>
        /// Test successful submission of an e-mail address input element with the $formnovalidate$ attribute that may or may not be populated, required, mutilple or have a maximum or minimum length requirement
        /// </summary>
        /// <param name="emailValue">The e-mail address value</param>
        /// <param name="required">A value indicating if the input element is requried</param>
        /// <param name="multiple">A value indicating if the input element supports multiple value entry</param>
        /// <param name="minimumLength">The minimum length of the input element</param>
        /// <param name="maximumLength">The maximum length of the input element</param>
        [Test]
        [TestCase("a@b.com,invalid", false, true, null, null)]
        [TestCase("a@b.com,invalid", true, true, null, null)]
        [TestCase("a@b.com,", true, true, null, null)]
        [TestCase("a@b.com", false, false, 1, null)]
        [TestCase("", true, false, null, null)]
        [TestCase("invalid", false, false, null, null)]
        [TestCase("invalid", true, false, null, null)]
        [TestCase("a@b.com,b@c.com", true, true, 1, null)]
        [TestCase("a@b.com,b@c.com", true, true, null, 30)]
        [TestCase("a@b.com,b@c.com", true, true, 1, 30)]
        [TestCase("a@b.com,b@c.com", false, true, 1, null)]
        [TestCase("a@b.com,b@c.com", false, true, null, 30)]
        [TestCase("a@b.com,b@c.com", false, true, 1, 30)]
        [TestCase("a@b.com,b@c.com", true, false, 1, null)]
        [TestCase("a@b.com,b@c.com", true, false, null, 30)]
        [TestCase("a@b.com,b@c.com", true, false, 1, 30)]
        [TestCase("a@b.com,b@c.com", false, false, 1, null)]
        [TestCase("a@b.com,b@c.com", false, false, null, 30)]
        [TestCase("a@b.com,b@c.com", false, false, 1, 30)]
        public async Task FormsEmailInputElementWithFormNoValidate_Invoke_SubmitSucceeds(string emailValue, bool required, bool multiple, int? maximumLength, int? minimumLength)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            HtmlResult email = b.Find("email");

            if (required)
            {
                email.XElement.SetAttributeCI("required", "");
            }

            if (multiple)
            {
                email.XElement.SetAttributeCI("multiple", "");
            }

            if (maximumLength.HasValue)
            {
                email.XElement.SetAttributeCI("maxlength", maximumLength.ToString());
            }

            if (minimumLength.HasValue)
            {
                email.XElement.SetAttributeCI("minlength", minimumLength.ToString());
            }

            email.Value = emailValue;

            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            Assert.IsTrue(await submit.ClickAsync() == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that text input elements that handle the $minlength$ attribute correctly set the value of the attribute.
        /// </summary>
        [Test]
        public async Task Forms_Input_MinLength()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            HtmlResult textInput = b.Find("textinput");

            // Test input of type text
            textInput.Value = "12345";
            textInput.XElement.SetAttributeCI("minlength", "30");
            Assert.False(await textInput.SubmitFormAsync());

            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            textInput = b.Find("textinput");
            textInput.Value = "12345";
            textInput.XElement.SetAttributeCI("minlength", "3");
            Assert.True(await textInput.SubmitFormAsync());

            // Test textarea
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            HtmlResult textAreaInput = b.Find("textareainput");
            textAreaInput.Value = "12345";
            textAreaInput.XElement.SetAttributeCI("minlength", "30");
            Assert.False(await textAreaInput.SubmitFormAsync());

            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            textAreaInput = b.Find("textareainput");
            textAreaInput.Value = "12345";
            textAreaInput.XElement.SetAttributeCI("minlength", "3");
            Assert.True(await textAreaInput.SubmitFormAsync());
        }

        /// <summary>
        /// Tests that text area form elements properly handle the disabled and read only attributes.
        /// </summary>
        [Test]
        public async Task Forms_Validate_Input_Elements()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test text input properties
            HtmlResult testinput = b.Find("textinput");
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
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            HtmlResult names = b.Select("tt");
            HtmlResult values = b.Select("tt");
            Assert.IsTrue(names.Count() == values.Count());

            // Check to make sure the proper values submitted
            Assert.IsTrue(values.Where(e => e.Value == "text input updated").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "text area input updated").FirstOrDefault() != null);
        }

        /// <summary>
        /// Tests that a text input containing a dirname attribute in a left-to-right culture with no value submits like Chrome
        /// </summary>
        [Test]
        public async Task FormsValidateTextInputContainingDirnameWithoutValueInLtrCulture_SubmitForm_SubmissionContainsCorrectValues()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            b.Culture = CultureInfo.CreateSpecificCulture("en-US");

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textinput");
            testinput.Value = "text input updated";
            testinput.XElement.SetAttributeCI("dirname", string.Empty);

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "ltr").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == string.Empty).FirstOrDefault());
        }

        /// <summary>
        /// Tests that form elements properly handle the dirname attribute in a left-to-right culture submits properly.
        /// </summary>
        [Test]
        public async Task FormsValidateTextAreaContainingDirnameWithoutValueInLtrCulture_SubmitForm_SubmissionContainsCorrectValues()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            b.Culture = CultureInfo.CreateSpecificCulture("en-US");

            // Test that the textarea input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textareainput");
            testinput.Value = "text area input updated";
            testinput.XElement.SetAttributeCI("dirname", string.Empty);

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "ltr").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == string.Empty).FirstOrDefault());
        }

        /// <summary>
        /// Tests that a text input containing a dirname attribute in a right-to-left culture with no value submits like Chrome
        /// </summary>
        [Test]
        public async Task FormsValidateTextInputContainingDirnameWithoutValueInRtlCulture_SubmitForm_SubmissionContainsCorrectValues()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            b.Culture = CultureInfo.CreateSpecificCulture("ar-EG");

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textinput");
            testinput.Value = "text input updated";
            testinput.XElement.SetAttributeCI("dirname", string.Empty);

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "rtl").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == string.Empty).FirstOrDefault());
        }

        /// <summary>
        /// Tests that a text input containing a pattern attribute successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateTextInputWithPatternAttribute_SubmitForm_SubmissionSucceeds()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textinput");
            testinput.Value = "USA";
            testinput.XElement.SetAttributeCI("pattern", "[A-Za-z]{3}");

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "USA").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == "textinput").FirstOrDefault());
        }

        /// <summary>
        /// Tests that a text input containing a pattern attribute successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateTextInputWithPatternAttribute_SubmitForm_SubmissionFails()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textinput");
            testinput.Value = "US";
            testinput.XElement.SetAttributeCI("pattern", "[A-Za-z]{3}");

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a text input containing a pattern attribute and $formnovalidate$ attribute successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateTextInputWithPatternAttributewithFormNoValidate_SubmitForm_SubmissionSucceeds()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textinput");
            testinput.Value = "US";
            testinput.XElement.SetAttributeCI("pattern", "[A-Za-z]{3}");

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that form elements properly handle the dirname attribute in a right-to-left culture.
        /// </summary>
        [Test]
        public async Task FormsValidateTextAreaContainingDirnameWithoutValueInRtlCulture_SubmitForm_SubmissionContainsCorrectValues()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            b.Culture = CultureInfo.CreateSpecificCulture("ar-EG");

            // Test that the textarea input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textareainput");
            testinput.Value = "text area input updated";
            testinput.XElement.SetAttributeCI("dirname", string.Empty);

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "rtl").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == string.Empty).FirstOrDefault());
        }

        /// <summary>
        /// Tests that a text input containing a pattern attribute successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateUrlInput_SubmitForm_SubmissionSuceeds()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("textinput");
            testinput.XElement.SetAttributeCI("type", "url");
            testinput.Value = "https://github.com/SimpleBrowserDotNet/SimpleBrowser";

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a text input containing a pattern attribute does not successfully submit
        /// </summary>
        [Test]
        public async Task FormsValidateUrlInput_SubmitForm_SubmissionFails()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("url");
            testinput.Value = "US";

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a text input containing a pattern and the $formnovalidate$ attribute successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateUrlInputWithFormNoValidate_SubmitForm_SubmissionSucceds()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("url");
            testinput.Value = "US";

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date time local input successfully submits
        /// </summary>
        /// <param name="submittedValue">The value entered into the date time input</param>
        /// <param name="returnedValue">The value returned returned from the date time input</param>
        /// <param name="required">A value indicating whether the input element is required</param>
        /// <param name="minimumValue">The minimum alowed value</param>
        /// <param name="maximumValue">The maximum alowed value</param>
        [Test]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, null, null)]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, null, null)]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, "1912-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, "1912-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, "1910-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, "1910-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, null, "1912-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, null, "1912-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, null, "1914-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, null, "1914-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, "1910-12-13 3:18 PM", "1912-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, "1910-12-13 3:18 PM", "1912-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", false, "1910-12-13 3:18 PM", "1914-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", "1912-12-13T15:18", true, "1910-12-13 3:18 PM", "1914-12-13 3:18 PM")]
        public async Task FormsDateTimeInputElement_SubmitForm_SubmitSucceeds(string submittedValue, string returnedValue, bool required, string minimumValue, string maximumValue)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");

            if (required)
            {
                dateTimeInput.XElement.SetAttributeCI("required", "");
            }

            if (!string.IsNullOrWhiteSpace(minimumValue))
            {
                dateTimeInput.XElement.SetAttributeCI("min", minimumValue);
            }

            if (!string.IsNullOrWhiteSpace(maximumValue))
            {
                dateTimeInput.XElement.SetAttributeCI("max", maximumValue);
            }

            dateTimeInput.Value = submittedValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the improper values were not submitted
            Assert.IsNotNull(values.Where(e => e.Value == returnedValue).FirstOrDefault());
        }

        /// <summary>
        /// Tests that a date time local input with a step attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [Test]
        [TestCase("2019-05-04", "")]
        [TestCase("2019-05-04", "0")]
        [TestCase("2019-05-04", "60")]
        [TestCase("2019-05-04", "600")]
        [TestCase("2019-05-04", "AnY")]
        [TestCase("2019-05-04", "Invalid")]
        [TestCase("2019-05-04 4:37 PM", "")]
        [TestCase("2019-05-04 4:37 PM", "0")]
        [TestCase("2019-05-04 4:37 PM", "60")]
        [TestCase("2019-05-04 4:37 PM", "aNy")]
        [TestCase("2019-05-04 4:37:30 PM", "15")]
        [TestCase("2019-05-04 4:37:31.00 PM", ".5")]
        public async Task FormsDateTimeInputElementWithStepAttribute_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date time local input with a step attribute defined does not successfully submit
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [Test]
        [TestCase("2019-05-04 4:37:30.500 PM", ".34")]
        public async Task FormsDateTimeInputElementWithStepAttribute_SubmitForm_SubmitFails(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date time local input with a step attribute and the $formnovalidate$ attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [Test]
        [TestCase("2019-05-04 4:37:30.500 PM", ".34")]
        public async Task FormsDateTimeInputElementWithStepAttributeWithFormNoValidate_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date input with a step attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [Test]
        [TestCase("2019-05-04", "")]
        [TestCase("2019-05-04", "0")]
        [TestCase("2019-06-13", "60")]
        public async Task FormsDateInputElementWithStepAttribute_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "date");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date input with a step attribute defined successfully fails
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("2019-05-04", "60")]
        public async Task FormsDateInputElementWithStepAttribute_SubmitForm_SubmitFails(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "date");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date input with a step attribute and the form no validate attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("2019-05-04", "60")]
        public async Task FormsDateInputElementWithStepAttributeWithFormNoValidate_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "date");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a month input with a step attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("", "")]
        [TestCase("", "3")]
        [TestCase("2015-01-04", "3")]
        [TestCase("1954-01-04", "3")]
        public async Task FormsMonthInputElementWithStepAttribute_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "month");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a month input with a step attribute defined does not submit successfully
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("2015-03-04", "3")]
        [TestCase("1954-02-04", "3")]
        public async Task FormsMonthInputElementWithStepAttribute_SubmitForm_SubmitFails(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "month");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a month input with a step attribute and the $formnovalidate$ defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("2015-03-04", "3")]
        [TestCase("1954-02-04", "3")]
        public async Task FormsMonthInputElementWithStepAttributeWithFormNoValidate_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "month");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a week input with a step attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("", "")]
        [TestCase("", "3")]
        [TestCase("2019-01-01", "1")]
        public async Task FormsWeekInputElementWithStepAttribute_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "week");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a week input with a step attribute defined does not successfully submit
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("2019-01-01", "2")]
        public async Task FormsWeekInputElementWithStepAttribute_SubmitForm_SubmitFails(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "week");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a week input with a step attribute and $formnovalidate$ attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("2019-01-01", "2")]
        public async Task FormsWeekInputElementWithStepAttributeWithFormNoValidate_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "week");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a time input with a step attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("", "")]
        [TestCase("", "3")]
        [TestCase("12:45", "-1")]
        [TestCase("12:45", "5")]
        public async Task FormsTimeInputElementWithStepAttribute_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "time");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a time input with a step attribute defined fails to submit
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("12:45", "17")]
        public async Task FormsTimeInputElementWithStepAttribute_SubmitForm_SubmitFails(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "time");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a time input with a step attribute and the $formnovalidate$ attribute defined successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="step">The step value defined</param>
        [TestCase("12:45", "17")]
        public async Task FormsTimeInputElementWithStepAttributeWithFormNoValidate_SubmitForm_SubmitSucceeds(string dateTimeValue, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");
            dateTimeInput.XElement.SetAttributeCI("type", "time");

            if (!string.IsNullOrWhiteSpace(step))
            {
                dateTimeInput.XElement.SetAttributeCI("step", step);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date time local input successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The date time value to enter</param>
        /// <param name="required">A value indicating that the input is required</param>
        /// <param name="minimumValue">The minimum value allowed in the input</param>
        /// <param name="maximumValue">The maximum value allowed in the input</param>
        [Test]
        [TestCase("invalid", false, null, null)]
        public async Task FormsDateTimeInputElement_SubmitForm_SubmitSucceedsWithoutValues(string dateTimeValue, bool required, DateTime? minimumValue, DateTime? maximumValue)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult dateTimeInput = b.Find("datetime");

            if (required)
            {
                dateTimeInput.XElement.SetAttributeCI("required", "");
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNull(values.Where(e => e.Value == dateTimeValue).FirstOrDefault());
        }

        /// <summary>
        /// Tests that a number input successfully submits
        /// </summary>
        /// <param name="value">The value to enter into the input</param>
        /// <param name="step">The step value to apply to the input</param>
        /// <param name="min">The minimum value allowed to be entered into the input</param>
        /// <param name="max">The maximum value allowed to be entered into the input</param>
        [Test]
        [TestCase("-10", null, null, null, "en-US")]
        [TestCase("0", null, null, null, "en-US")]
        [TestCase("10", null, null, null, "en-US")]
        [TestCase("-10", "-20", null, null, "en-US")]
        [TestCase("0", "0", null, null, "en-US")]
        [TestCase("10", "5", null, null, "en-US")]
        [TestCase("-10", "-20", "0", null, "en-US")]
        [TestCase("0", "0", "0", null, "en-US")]
        [TestCase("10", "5", "100", null, "en-US")]
        [TestCase("-10", "-20", "0", "5", "en-US")]
        [TestCase("0", "0", "0", "5", "en-US")]
        [TestCase("10", "5", "100", "5", "en-US")]
        [TestCase("-7", "-20", "0", "Any", "en-US")]
        [TestCase("0", "0", "0", "aNy", "en-US")]
        [TestCase("7", "5", "100", "anY", "en-US")]
        [TestCase("-7", "-20", "0", "-1", "en-US")]
        [TestCase("0", "0", "0", "-1", "en-US")]
        [TestCase("7", "5", "100", "-1", "en-US")]
        [TestCase("-7", "-20", "0", "Invalid", "en-US")]
        [TestCase("0", "0", "0", "step", "en-US")]
        [TestCase("7", "5", "100", "strinG", "en-US")]
        //[TestCase("-.5", null, null, null, "en-US")]
        //[TestCase(".5", null, null, null, "en-US")]
        //[TestCase("-.5", "-1.1", null, null, "en-US")]
        //[TestCase(".5", "-1.2", null, null, "en-US")]
        //[TestCase("-.5", "-1.1", "2.4", null, "en-US")]
        //[TestCase(".5", "-1.2", "5.9", null, "en-US")]
        //[TestCase("-1.1", "-1.6", "2.4", ".5", "en-US")]
        //[TestCase(".3", "-1.2", "5.9", ".5", "en-US")]
		[TestCase("-.5", null, null, null, "nl-NL")]
		[TestCase(".5", null, null, null, "nl-NL")]
		[TestCase("-.5", "-1.1", null, null, "nl-NL")]
		[TestCase(".5", "-1.2", null, null, "nl-NL")]
		[TestCase("-.5", "-1.1", "2.4", null, "nl-NL")]
		[TestCase(".5", "-1.2", "5.9", null, "nl-NL")]
		//[TestCase("-1.1", "-1.6", "2.4", ".5", "nl-NL")]
		[TestCase(".3", "-1.2", "5.9", ".5", "nl-NL")]
		[TestCase("3e5", null, null, null, "en-US")]
        [TestCase("3e5", "1000", null, null, "en-US")]
        [TestCase("3e5", null, "500000", null, "en-US")]
        [TestCase("3e5", "1000", "500000", null, "en-US")]
        [TestCase("3e5", "1000", "500000", "100", "en-US")]
        [TestCase("notnumber", null, null, null, "en-US")]
        [TestCase("notnumber", "5", "15", "4", "en-US")]
        public async Task FormsNumberElement_SubmitForm_SubmitSucceeds(string value, string min, string max, string step, string cultureString)
        {
            Browser b = new Browser();
			b.Culture = CultureInfo.CreateSpecificCulture(cultureString);
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            HtmlResult numberInput = b.Find("number");

            if (!string.IsNullOrWhiteSpace(min))
            {
                numberInput.XElement.SetAttributeCI("min", min);
            }

            if (!string.IsNullOrWhiteSpace(max))
            {
                numberInput.XElement.SetAttributeCI("max", max);
            }

            if (!string.IsNullOrWhiteSpace(step))
            {
                numberInput.XElement.SetAttributeCI("step", step);
            }

            numberInput.Value = value;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a number input submit fails
        /// </summary>
        /// <param name="value">The value to enter into the input</param>
        /// <param name="min">The minimum value allowed to be entered into the input</param>
        /// <param name="max">The maximum value allowed to be entered into the input</param>
        /// <param name="step">The step value to apply to the input</param>
        [Test]
        [TestCase("3e5", "500000", null, null, "en-US")]
        [TestCase("3e5", null, "500", null, "en-US")]
        [TestCase("3e5", "500000", "50000000", null, "en-US")]
        [TestCase("3e5", "500000", "50000000", "117", "en-US")]
        [TestCase("-.5", "-1.1", "2.4", ".5", "en-US")]
        [TestCase(".5", "-1.2", "5.9", ".5", "en-US")]
		[TestCase("-,5", "-1,1", "2,4", ",5", "nl-NL")]
		[TestCase(",5", "-1,2", "5,9", ",5", "nl-NL")]
		public async Task FormsNumberElement_SubmitForm_SubmitFails(string value, string min, string max, string step, string cultureString)
        {
            Browser b = new Browser();
			b.Culture = CultureInfo.CreateSpecificCulture(cultureString);
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            HtmlResult numberInput = b.Find("number");

            if (!string.IsNullOrWhiteSpace(min))
            {
                numberInput.XElement.SetAttributeCI("min", min);
            }

            if (!string.IsNullOrWhiteSpace(max))
            {
                numberInput.XElement.SetAttributeCI("max", max);
            }

            if (!string.IsNullOrWhiteSpace(step))
            {
                numberInput.XElement.SetAttributeCI("step", step);
            }

            numberInput.Value = value;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a number input with a $formnovalidate$ attribute successfully submits
        /// </summary>
        /// <param name="value">The value to enter into the input</param>
        /// <param name="min">The minimum value allowed to be entered into the input</param>
        /// <param name="max">The maximum value allowed to be entered into the input</param>
        /// <param name="step">The step value to apply to the input</param>
        [Test]
        [TestCase("3e5", "500000", null, null)]
        [TestCase("3e5", null, "500", null)]
        [TestCase("3e5", "500000", "50000000", null)]
        [TestCase("3e5", "500000", "50000000", "117")]
        [TestCase("-.5", "-1.1", "2.4", ".5")]
        [TestCase(".5", "-1.2", "5.9", ".5")]
        public async Task FormsNumberElement_SubmitFormWithFormNoValidate_SubmitSucceeds(string value, string min, string max, string step)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            HtmlResult numberInput = b.Find("number");

            if (!string.IsNullOrWhiteSpace(min))
            {
                numberInput.XElement.SetAttributeCI("min", min);
            }

            if (!string.IsNullOrWhiteSpace(max))
            {
                numberInput.XElement.SetAttributeCI("max", max);
            }

            if (!string.IsNullOrWhiteSpace(step))
            {
                numberInput.XElement.SetAttributeCI("step", step);
            }

            numberInput.Value = value;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date time local input does not successfully submit
        /// </summary>
        /// <param name="dateTimeValue">The value to enter into the input</param>
        /// <param name="required">A value indicating whether the input is requried</param>
        /// <param name="minimumValue">The minimum value allowed to be entered into the input</param>
        /// <param name="maximumValue">The maximum value allowed to be entered into the input</param>
        [Test]
        [TestCase("invalid", true, null, null)]
        [TestCase("1912-12-13 3:18 PM", false, "1920-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", true, "1920-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", false, null, "1910-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", true, null, "1910-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", false, "1920-12-13 3:18 PM", "1910-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", true, "1920-12-13 3:18 PM", "1910-12-13 3:18 PM")]
        public async Task FormsValidateDateTimeInput_SubmitForm_SubmissionFails(string dateTimeValue, bool required, string minimumValue, string maximumValue)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            HtmlResult dateTimeInput = b.Find("datetime");

            if (required)
            {
                dateTimeInput.XElement.SetAttributeCI("required", "");
            }

            if (!string.IsNullOrWhiteSpace(minimumValue))
            {
                dateTimeInput.XElement.SetAttributeCI("min", minimumValue);
            }

            if (!string.IsNullOrWhiteSpace(maximumValue))
            {
                dateTimeInput.XElement.SetAttributeCI("max", maximumValue);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsFalse(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date time local input with submitted with the $formnovalidate$ successfully submits
        /// </summary>
        /// <param name="dateTimeValue">The value to enter into the input</param>
        /// <param name="required">A value indicating whether the input is requried</param>
        /// <param name="minimumValue">The minimum value allowed to be entered into the input</param>
        /// <param name="maximumValue">The maximum value allowed to be entered into the input</param>
        [Test]
        [TestCase("invalid", true, null, null)]
        [TestCase("1912-12-13 3:18 PM", false, "1920-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", true, "1920-12-13 3:18 PM", null)]
        [TestCase("1912-12-13 3:18 PM", false, null, "1910-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", true, null, "1910-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", false, "1920-12-13 3:18 PM", "1910-12-13 3:18 PM")]
        [TestCase("1912-12-13 3:18 PM", true, "1920-12-13 3:18 PM", "1910-12-13 3:18 PM")]
        public async Task FormsValidateDateTimeInputWithFormNoValidate_SubmitForm_SubmissionSucceeds(string dateTimeValue, bool required, string minimumValue, string maximumValue)
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            HtmlResult dateTimeInput = b.Find("datetime");

            if (required)
            {
                dateTimeInput.XElement.SetAttributeCI("required", "");
            }

            if (!string.IsNullOrWhiteSpace(minimumValue))
            {
                dateTimeInput.XElement.SetAttributeCI("min", minimumValue);
            }

            if (!string.IsNullOrWhiteSpace(maximumValue))
            {
                dateTimeInput.XElement.SetAttributeCI("max", maximumValue);
            }

            dateTimeInput.Value = dateTimeValue;

            // Submit the form
            HtmlResult submit = b.Find("es");
            submit.XElement.SetAttributeCI("formnovalidate", "");

            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);
        }

        /// <summary>
        /// Tests that a date input successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateDateInput_SubmitForm_SubmissionSucceeds()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult testinput = b.Find("datetime");
            testinput.XElement.SetAttributeCI("type", "date");
            testinput.Value = "1912-12-13";

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "1912-12-13").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == "datetime").FirstOrDefault());
        }

        /// <summary>
        /// Tests that a color input successfully submits
        /// </summary>
        [Test]
        public async Task FormsValidateColorInput_SubmitForm_SubmissionSucceeds()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // Test that the text input with a dirname with an empty value properly submits
            HtmlResult colorInput = b.Find("color");
            colorInput.Value = "#34abe6";

            // Submit the form
            HtmlResult submit = b.Find("es");
            ClickResult clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            IEnumerable<HtmlResult> values = b.Select("tt").ToList().Where((c, i) => i % 2 != 0);
            IEnumerable<HtmlResult> names = b.Select("tt").ToList().Where((c, i) => i % 2 == 0);

            // Check to make sure the proper values submitted
            Assert.IsNotNull(values.Where(e => e.Value == "#34abe6").FirstOrDefault());
            Assert.IsNotNull(names.Where(e => e.Value == "color").FirstOrDefault());
        }

        /// <summary>
        /// Tests that form elements properly handle the disabled and read only attributes.
        /// </summary>
        [Test]
        public async Task Forms_Disabled_and_ReadOnly()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));
            HtmlResult textarea = b.Find("readonlytextarea");
            textarea.Value = "some value";
            Assert.IsTrue(textarea.Value == "readme textarea");
            Assert.IsTrue(textarea.ReadOnly);
            Assert.IsFalse(textarea.Disabled);

            textarea = b.Find("disabledtextarea");
            textarea.Value = "some value";
            Assert.IsTrue(textarea.Value == "disableme textarea");
            Assert.IsFalse(textarea.ReadOnly);
            Assert.IsTrue(textarea.Disabled);

            HtmlResult textinput = b.Find("readonlytext");
            textinput.Value = "some value";
            Assert.IsTrue(textinput.Value == "readme");
            Assert.IsTrue(textinput.ReadOnly);
            Assert.IsFalse(textinput.Disabled);

            textinput = b.Find("disabledtext");
            textinput.Value = "some value";
            Assert.IsTrue(textinput.Value == "disableme");
            Assert.IsFalse(textinput.ReadOnly);
            Assert.IsTrue(textinput.Disabled);

            HtmlResult checkbox = b.Find("disabledcheck");
            Assert.IsTrue(checkbox.Disabled);

            HtmlResult radio = b.Find("disabledradio");
            Assert.IsTrue(radio.Disabled);

            HtmlResult disabledSubmit = b.Find("ds");
            Assert.IsTrue(disabledSubmit.Disabled);
            ClickResult clickResult = await disabledSubmit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNoOp);

            HtmlResult submit = b.Find("es");
            Assert.IsFalse(submit.Disabled);
            clickResult = await submit.ClickAsync();
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            // Check to make sure the form submitted.
            HtmlResult names = b.Select("tt");
            HtmlResult values = b.Select("tt");
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
        /// Tests that input elements can be found by their id and name attributes
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
        public async Task Forms_Form_Attribute()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.HTML5Elements.htm"));

            // This test covers the following cases:
            // 1. An input outside of the form with the form attribute is submitted.
            // 2. An input in another form with the form attribute is submitted.
            HtmlResult field1 = b.Find("field1");
            field1.Value = "Name1";

            HtmlResult field1a = b.Find("field1a");
            field1a.Value = "Name1a";

            HtmlResult field2 = b.Find("field2");
            field2.Value = "Name2";

            HtmlResult submit1 = b.Find("submit1");
            ClickResult clickResult = await submit1.ClickAsync();

            // Check to make sure the form submitted.
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            HtmlResult names = b.Select("tt");
            HtmlResult values = b.Select("tt");
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

            HtmlResult field2a = b.Find("field2a");
            field2a.Value = "Name2a";

            HtmlResult submit2 = b.Find("submit2");
            clickResult = await submit2.ClickAsync();

            // Check to make sure the form submitted.
            Assert.IsTrue(clickResult == ClickResult.SucceededNavigationComplete);

            names = b.Select("tt");
            values = b.Select("tt");
            Assert.IsTrue(values.Where(e => e.Value == "Submit2").FirstOrDefault() != null);
            Assert.IsTrue(values.Where(e => e.Value == "Name2a").FirstOrDefault() != null);
            Assert.IsFalse(values.Where(e => e.Value == "Name2").FirstOrDefault() != null);
        }
    }
}