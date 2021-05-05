// -----------------------------------------------------------------------
// <copyright file="FormElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Implements a form HTML element.
    /// </summary>
    /// <remarks>
    /// HTML5 Specification for the form element: https://www.w3.org/TR/html52/sec-forms.html#the-form-element
    /// </remarks>
    internal class FormElement : HtmlElement
    {
        /// <summary>
        /// The element tag names that are recognized as form input elements.
        /// </summary>
        private static string[] formInputElementNames = new string[] { "select", "input", "button", "textarea", "button" };

        /// <summary>
        /// Initializes a new instance of the <see cref="FormElement"/> class.
        /// </summary>
        /// <param name="element">The XElement from the document corresponding to this element.</param>
        public FormElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets an enumeration of child form elements
        /// </summary>
        /// <remarks>
        /// This method finds all form input elements then selects the elements that belong to this form element. The form
        /// input element knows the form it belongs to, either due to being a descendant of a form element, using the form
        /// attribute, or, in the case of orphaned form input elements, it's position in the document relative to any
        /// form elements that may exist.
        /// </remarks>
        public IEnumerable<FormElementElement> Elements
        {
            get
            {
                IEnumerable<FormElementElement> formElements = this.OwningBrowser.XDocument.Descendants()
                    .Where(e => formInputElementNames.Contains(e.Name.LocalName.ToLower()))
                    .Select(e => this.OwningBrowser.CreateHtmlElement<FormElementElement>(e));

                return formElements.Where(e => e.OwningForm != null && e.OwningForm.XElement == this.XElement);
            }
        }

        /// <summary>
        /// Gets the form action
        /// </summary>
        public string Action
        {
            get
            {
                XAttribute actionAttr = this.GetAttribute(this.Element, "action");
                return actionAttr == null ? this.OwningBrowser.Url.ToString() : actionAttr.Value;
            }
        }

        /// <summary>
        /// Gets the form method
        /// </summary>
        public string Method
        {
            get
            {
                XAttribute attr = this.GetAttribute(this.Element, "method");
                return attr == null ? "GET" : attr.Value.ToUpper();
            }
        }

        /// <summary>
        /// Gets the form encoding type
        /// </summary>
        public string EncType
        {
            get
            {
                string val = this.GetAttributeValue("enctype");
                if (string.IsNullOrWhiteSpace(val))
                {
                    val = FormEncoding.FormUrlencode;
                }

                return val;
            }
        }

        /// <summary>
        /// Gets the form target
        /// </summary>
        public string Target
        {
            get
            {
                return this.GetAttributeValue("target");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the form should validate.
        /// </summary>
        private bool Validate { get; set; }

        [Obsolete("Use async version instead")]
        public override bool SubmitForm(string url = null, HtmlElement clickedElement = null)
        {
            return this.Submit(url, clickedElement).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Submits the form
        /// </summary>
        /// <param name="url">Optional. If specified, the url to submit the form. Overrides the form action.</param>
        /// <param name="clickedElement">Optional. The element clicked, resulting in form submission.</param>
        /// <returns>True, if the form submitted successfully, false otherwise.</returns>
        public override async Task<bool> SubmitFormAsync(string url = null, HtmlElement clickedElement = null)
        {
            return await this.Submit(url, clickedElement);
        }

        /// <summary>
        /// Submits the form
        /// </summary>
        /// <param name="url">Optional. If specified, the url to submit the form. Overrides the form action.</param>
        /// <param name="clickedElement">Optional. The element clicked, resulting in form submission.</param>
        /// <returns>True, if the form submitted successfully, false otherwise.</returns>
        private async Task<bool> Submit(string url = null, HtmlElement clickedElement = null)
        {
            string action = this.Action;
            string method = this.Method;
            string enctype = this.EncType;
            string target = this.Target;
            this.Validate = true;

            if (clickedElement != null)
            {
                string clickedElementAction = clickedElement.GetAttributeValue("formaction");
                if (!string.IsNullOrWhiteSpace(clickedElementAction))
                {
                    action = clickedElementAction;
                }

                string clickedElementEncType = clickedElement.GetAttributeValue("formenctype");
                if (!string.IsNullOrWhiteSpace(clickedElementEncType))
                {
                    enctype = clickedElementEncType;
                }

                string clickedElementMethod = clickedElement.GetAttributeValue("formmethod");
                if (!string.IsNullOrWhiteSpace(clickedElementMethod))
                {
                    method = clickedElementMethod;
                }

                string clickedElementTarget = clickedElement.GetAttributeValue("formtarget");
                if (!string.IsNullOrWhiteSpace(clickedElementTarget))
                {
                    target = clickedElementTarget;
                }

                if (clickedElement.XElement.HasAttributeCI("formnovalidate"))
                {
                    this.Validate = false;
                }
            }

            NavigationArgs navigation = new NavigationArgs
            {
                Uri = url ?? action,
                Method = method,
                ContentType = FormEncoding.FormUrlencode,
                EncodingType = enctype,
                Target = target
            };

            try
            {
                foreach (UserVariableEntry entry in this.Elements.SelectMany(e =>
                        {
                            bool isClicked = false;
                            if (clickedElement != null && clickedElement.Element == e.Element)
                            {
                                isClicked = true;
                            }

                            return e.ValuesToSubmit(isClicked, this.Validate);
                        }))
                {
                    navigation.UserVariables.Add(entry.Name, entry.Value);
                }
            }
            catch (FormElementValidationException formElementValidationException)
            {
                base.OwningBrowser.Log(formElementValidationException.Message, LogMessageType.Error);
                return false;
            }

            if (navigation.EncodingType == FormEncoding.MultipartForm && navigation.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                // create postdata according to multipart specs
                Guid token = Guid.NewGuid();
                navigation.UserVariables = null;
                StringBuilder post = new StringBuilder();
                foreach (FormElementElement element in this.Elements)
                {
                    bool isClickedElement = false;
                    if (clickedElement != null)
                    {
                        isClickedElement = element.Element == clickedElement.Element;
                    }

                    IEnumerable<UserVariableEntry> values = element.ValuesToSubmit(isClickedElement, this.Validate);
                    foreach (UserVariableEntry value in values)
                    {
                        post.AppendFormat("--{0}\r\n", token);
                        if (element is FileUploadElement)
                        {
                            post.AppendFormat(
                                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n{2}\r\n",
                                element.Name,
                                value.Name,
                                value.Value);
                        }
                        else
                        {
                            post.AppendFormat(
                                "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n",
                                value.Name,
                                value.Value);
                        }
                    }
                }

                post.AppendFormat("--{0}--\r\n", token);
                navigation.PostData = post.ToString();
                navigation.ContentType = FormEncoding.MultipartForm + "; boundary=" + token;
            }

            return await this.RequestNavigation(navigation);
        }

        /// <summary>
        /// Implements form encoding types
        /// </summary>
        public static class FormEncoding
        {
            /// <summary>
            /// URL encoded form encoding
            /// </summary>
            public const string FormUrlencode = "application/x-www-form-urlencoded";

            /// <summary>
            /// Multipart form encoding
            /// </summary>
            public const string MultipartForm = "multipart/form-data";
        }
    }
}