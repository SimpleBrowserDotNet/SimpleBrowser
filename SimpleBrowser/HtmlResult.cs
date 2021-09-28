// -----------------------------------------------------------------------
// <copyright file="HtmlResult.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml.Linq;
    using SimpleBrowser.Elements;
    using SimpleBrowser.Query;

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
        /// <summary>
        /// The browser instance in which this result is found.
        /// </summary>
        private readonly Browser browser;

        /// <summary>
        /// The current element in the list of found HTML element results.
        /// </summary>
        private HtmlElement currentElement;

        /// <summary>
        /// The list of found HTML element results.
        /// </summary>
        private List<HtmlElement> resultList;

        /// <summary>
        /// The index of the current item in the list of found HTML results.
        /// </summary>
        private int resultListIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlResult"/> class.
        /// </summary>
        /// <param name="results">A collection of <see cref="HtmlElement"/></param>
        /// <param name="browser">The browser instance where the HTML results were found</param>
        internal HtmlResult(List<HtmlElement> results, Browser browser)
        {
            this.currentElement = results.Count > 0 ? results[0] : null;
            this.resultList = results;
            this.browser = browser;
            browser.Log("New HTML result set obtained, containing " + results.Count + " element(s)", LogMessageType.Internal);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlResult"/> class.
        /// </summary>
        /// <param name="result">A <see cref="HtmlElement"/></param>
        /// <param name="browser">The browser instance where the HTML results was found</param>
        internal HtmlResult(HtmlElement result, Browser browser)
        {
            this.currentElement = result;
            this.browser = browser;
            this.resultList = new List<HtmlElement>(new[] { result });
        }

        /// <summary>
        /// Gets a reference to the underlying <see cref="XElement"/>.
        /// </summary>
        public XElement XElement
        {
            get
            {
                return this.currentElement.XElement;
            }
        }

        /// <summary>
        /// Gets the total number of elements that matched the query that generated this HtmlResult object
        /// </summary>
        public int TotalElementsFound
        {
            get
            {
                return this.resultList.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the HtmlResult object is pointing at an element to perform operations on.
        /// </summary>
        public bool Exists
        {
            get
            {
                return this.currentElement != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the HtmlResult object is pointing at an element that is read only.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                this.AssertElementExists();
                if (this.currentElement is InputElement)
                {
                    return ((InputElement)this.currentElement).ReadOnly;
                }

                if (this.currentElement is TextAreaElement)
                {
                    return ((TextAreaElement)this.currentElement).ReadOnly;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the element contains a "disabled" attribute. Throws an exception if Exists is false.
        /// </summary>
        public bool Disabled
        {
            get
            {
                this.AssertElementExists();
                if (this.currentElement is FormElementElement)
                {
                    return ((FormElementElement)this.currentElement).Disabled;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets or sets the value of the element.
        /// </summary>
        public string Value
        {
            get
            {
                this.AssertElementExists();
                if (this.currentElement is FormElementElement)
                {
                    return ((FormElementElement)this.currentElement).Value;
                }

                return this.currentElement.XElement.Value;
            }

            set
            {
                this.AssertElementExists();
                this.browser.Log("Setting the value of " + HttpUtility.HtmlEncode(XElement.ToString()) + " to " + HttpUtility.HtmlEncode(value.ShortenTo(30, true)), LogMessageType.Internal);

                if (this.currentElement is FormElementElement)
                {
                    ((FormElementElement)this.currentElement).Value = value;
                }
            }
        }

        /// <summary>
        /// Gets the decoded Value of the element. For example if the Value is <![CDATA[&copy;]]> 2011 the decoded Value will be © 2011.
        /// </summary>
        public string DecodedValue
        {
            get
            {
                return HttpUtility.HtmlDecode(this.Value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checked attribute is set for the current element. Throws an exception if
        /// exists is false or if the current element is anything other than a RADIO or CHECKBOX (INPUT) element.
        /// </summary>
        public bool Checked
        {
            get
            {
                this.AssertElementExists();
                if (this.currentElement is SelectableInputElement)
                {
                    return ((SelectableInputElement)this.currentElement).Selected;
                }

                return false;
            }

            set
            {
                this.AssertElementExists();
                this.browser.Log("Setting the checked state of " + HttpUtility.HtmlEncode(this.XElement.ToString()) + " to " + (value ? "CHECKED" : "UNCHECKED"), LogMessageType.Internal);

                if (this.currentElement is SelectableInputElement)
                {
                    ((SelectableInputElement)this.currentElement).Selected = value;
                }
            }
        }

        /// <summary>
        /// Gets the currently selected HTML element from the list of HTML results.
        /// </summary>
        internal HtmlElement CurrentElement
        {
            get
            {
                return this.currentElement;
            }
        }

        /// <summary>
        /// Returns a new result set derived from the current element, using jQuery selector syntax
        /// </summary>
        /// <param name="query">The jQuery selector query</param>
        /// <returns>The <see cref="HtmlResult"/> of the query</returns>
        public HtmlResult Select(string query)
        {
            this.AssertElementExists();
            return new HtmlResult(XQuery.Execute("* " + query, this.browser.XDocument, this.currentElement.XElement).Select(this.browser.CreateHtmlElement).ToList(), this.browser);
        }

        /// <summary>
        /// Returns a new result set derived from the current set of elements, using jQuery selector syntax
        /// </summary>
        /// <param name="query">The jQuery selector query</param>
        /// <returns>The <see cref="HtmlResult"/> of the query</returns>
        public HtmlResult Refine(string query)
        {
            return new HtmlResult(XQuery.Execute(query, this.browser.XDocument, this.resultList.Select(h => h.XElement).ToArray()).Select(this.browser.CreateHtmlElement).ToList(), this.browser);
        }

        /// <summary>
        /// Attempts to move to the next element that matched the query that generated this HtmlResult object. No error
        /// will be thrown if there are no more matching elements, but <see cref="Exists" /> should be checked before
        /// attempting to perform any further operations.
        /// </summary>
        /// <returns>True if there was another element, or false if we're no longer pointing at an element</returns>
        public bool Next()
        {
            if (this.resultListIndex < this.resultList.Count - 1)
            {
                this.currentElement = this.resultList[++this.resultListIndex];
                return true;
            }

            this.currentElement = null;
            return false;
        }


        [Obsolete("Use ClickAsync instead")]
        public ClickResult Click()
        {
            return ClickAsync().GetAwaiter().GetResult();
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
        /// <returns>A <see cref="ClickResult"/> indicating the results of the click.</returns>
        public async Task<ClickResult> ClickAsync()
        {
            this.AssertElementExists();
            this.browser.Log("Clicking element: " + HttpUtility.HtmlEncode(this.XElement.ToString()), LogMessageType.Internal);
            return await this.currentElement.ClickAsync();
        }


        [Obsolete("Use async version instead")]
        public ClickResult Click(uint x, uint y)
        {
            return ClickAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Simulates a click on an element at the specified coordinates, which has differing effects depending on the
        /// element type. If the element IS an INPUT TYPE=IMAGE element, this method will "click" the image input as
        /// though the element had been clicked with a pointing device (i.e., a mouse) at the specified coordinates.
        /// If the element does not support being clicked at specified coordinates (i.e., the element IS NOT an INPUT
        /// TYPE=IMAGE element), the element will be clicked as though the Click() method (without parameters) been called.
        /// </summary>
        /// <param name="x">The x-coordinate of the click location</param>
        /// <param name="y">The y-coordinate of the click location</param>
        /// <returns>A <see cref="ClickResult"/> indicating the results of the click.</returns>
        public async Task<ClickResult> ClickAsync(uint x, uint y)
        {
            this.AssertElementExists();
            this.browser.Log("Clicking element: " + HttpUtility.HtmlEncode(this.XElement.ToString()), LogMessageType.Internal);
            return await this.currentElement.ClickAsync(x, y);
        }


        [Obsolete("Use SubmitFormAsync instead")]
        public bool SubmitForm(string url = null)
        {
            return SubmitFormAsync(url).GetAwaiter().GetResult();
        }

            /// <summary>
            /// This method can be used on any element contained within a form, or the form element itself. The form will be 
            /// serialized and submitted as close as possible to the way it would be in a normal browser request. In
            /// addition, any values currently in the ExtraFormValues property of the Browser object will be submitted as
            /// well.
            /// </summary>
            /// <param name="url">Optional. If specified, the form will be submitted to this URL instead.</param>
            /// <returns>True if the form was successfully submitted. Otherwise, false.</returns>
            public async Task<bool> SubmitFormAsync(string url = null)
        {
            this.AssertElementExists();
            this.browser.Log("Submitting parent/ancestor form of: " + HttpUtility.HtmlEncode(this.XElement.ToString()), LogMessageType.Internal);
            return await this.currentElement.SubmitFormAsync(url);
        }

        [Obsolete("Use Async version instead")]
        public ClickResult DoAspNetLinkPostBack()
        {
            return DoAspNetLinkPostBackAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// This method is designed for use on Asp.Net WebForms sites where the anchor link being clicked only has a post back
        /// JavaScript function as its method of navigating to the next page.
        /// </summary>
        /// <returns>A <see cref="ClickResult"/> indicating the results of the click.</returns>
        public async Task<ClickResult> DoAspNetLinkPostBackAsync()
        {
            this.AssertElementExists();
            this.browser.Log("Performing ASP.Net postback click for : " + HttpUtility.HtmlEncode(this.XElement.ToString()), LogMessageType.Internal);
            return await this.currentElement.DoAspNetLinkPostBackAsync();
        }

        /// <summary>
        /// Return the value of the specified attribute. Throws an exception if Exists is false.
        /// </summary>
        /// <param name="name">The name of the attribute</param>
        /// <returns>Returns the string value of the requested attribute</returns>
        public string GetAttribute(string name)
        {
            this.AssertElementExists();
            return this.currentElement.GetAttributeValue(name);
        }

        /// <summary>
        /// Gets the collection of HTML results.
        /// </summary>
        /// <returns>A collection of <see cref="HtmlResult"/></returns>
        public IEnumerator<HtmlResult> GetEnumerator()
        {
            foreach (var el in this.resultList)
            {
                yield return new HtmlResult(el, this.browser);
            }
        }

        /// <summary>
        /// Implements IEnumerator
        /// </summary>
        /// <returns>A collection of <see cref="HtmlResult"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Validates that the element exists and is valid
        /// </summary>
        private void AssertElementExists()
        {
            if (this.currentElement == null)
            {
                throw new InvalidOperationException("The requested operation is not available when Exists is false");
            }

            if (!this.currentElement.Valid)
            {
                throw new InvalidOperationException("The requested operation is not available. Navigating makes the existing HtmlResult objects invalid.");
            }
        }
    }
}
